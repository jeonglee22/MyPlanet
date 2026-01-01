using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public enum SfxChoice {World, Button}

    public enum SoundPriority
    {
        Critical = 0, //boss, gameOver, victory
        VeryHigh = 50, //explosion, chain, levelup
        High = 100, //hit, specialAttack
        Normal = 150, //shoot
        Low = 200, //ui, background
    }

    [SerializeField] private AudioMixer mixer;
    private string bgmParam = "BGM";
    private string sfxParam = "SFX";
    private string masterParam = "Master";

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sampleSource;
    [SerializeField] private AudioSource sfxButtonSource;

    [Header("Tower Sounds")]
    [SerializeField] private AudioClip pistolShot;
    [SerializeField] private AudioClip gatlingShot;
    [SerializeField] private AudioClip sniperShot;
    [SerializeField] private AudioClip shotgunShot;
    [SerializeField] private AudioClip missileShot;
    [SerializeField] private AudioClip laserShot;

    [Header("Effect Sounds")]
    [SerializeField] private AudioClip planetHit;
    [SerializeField] private AudioClip chainEffect;
    [SerializeField] private AudioClip explosionEffect;
    [SerializeField] private AudioClip reflectShield;
    [SerializeField] private AudioClip enemyLaser;
    [SerializeField] private AudioClip sunFireball;
    [SerializeField] private AudioClip bossAppear;
    [SerializeField] private AudioClip enemyHit;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip refreshSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    [SerializeField] private AudioClip quasarSelect;
    [SerializeField] private AudioClip deployOpen;
    [SerializeField] private AudioClip deployClose;
    [SerializeField] private AudioClip clickSound;

    [Header("Background Music")]
    [SerializeField] private AudioClip loginBGM;
    [SerializeField] private AudioClip lobbyBGM;
    [SerializeField] private AudioClip battleBGM;
    public AudioClip BattleBGM => battleBGM;
    public AudioClip LobbyBGM => lobbyBGM;
    public AudioClip LoginBGM => loginBGM;

    private const int POOLSIZE = 35;

    private Queue<AudioSource> audioPool = new Queue<AudioSource>();
    private Dictionary<AudioSource, AudioClip> playing = new Dictionary<AudioSource, AudioClip>();

    private DatabaseReference settingsRef;
    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private bool isMute = false;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    private CancellationTokenSource destroyCts;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == SceneName.LoadingScene)
        {
            return;
        }

        if (isInitialized && AuthManager.Instance != null && AuthManager.Instance.IsSignedIn)
        {
            LoadSettingsAsync().Forget();
        }
    }

    private async UniTaskVoid Start()
    {
        for(int i = 0; i < POOLSIZE; i++)
        {
            var source = Instantiate(sampleSource, transform);
            source.gameObject.SetActive(false);
            audioPool.Enqueue(source);
        }

        bgmSource.priority = 0;
        sfxButtonSource.priority = 0;

        await FireBaseInitializer.Instance.WaitInitialization();
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        if (AuthManager.Instance.IsSignedIn)
        {
            InitializeReference();

            var dataSnapshot = await settingsRef.GetValueAsync().AsUniTask();
            if (dataSnapshot.Exists)
            {
                await LoadSettingsAsync();
            }
            else
            {
                masterVolume = 1f;
                bgmVolume = 1f;
                sfxVolume = 1f;
                isMute = false;

                await SaveSettingsAsync();
                ApplyVolumes();
            }
        }
        else
        {
            ApplyVolumes();
        }

        Cancel();

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }

        Cancel();
    }

    private void Cancel()
    {
        destroyCts?.Cancel();
        destroyCts?.Dispose();
        destroyCts = new CancellationTokenSource();
    } 

    private void InitializeReference()
    {
        string userId = AuthManager.Instance.UserId;
        settingsRef = FirebaseDatabase.DefaultInstance.GetReference("userdata").Child(userId).Child("settings");
    }

    public async UniTask LoadSettingsAsync()
    {
        Debug.Log($"[SoundManager] LoadSettings - IsSignedIn: {AuthManager.Instance.IsSignedIn}");
        Debug.Log($"[SoundManager] LoadSettings - UserId: {AuthManager.Instance.UserId}");

        if(!AuthManager.Instance.IsSignedIn)
        {
            ApplyVolumes();
            return;
        }

        InitializeReference();

        string userId = AuthManager.Instance.UserId;
        string path = $"userdata/{userId}/settings";
        Debug.Log($"[SoundManager] Attempting to access: {path}");

        try
        {
            var dataSnapshot = await settingsRef.GetValueAsync();
            if (dataSnapshot.Exists)
            {
                masterVolume = dataSnapshot.Child("masterVolume").Value != null ? Convert.ToSingle(dataSnapshot.Child("masterVolume").Value) : 1f;
                bgmVolume = dataSnapshot.Child("bgmVolume").Value != null ? Convert.ToSingle(dataSnapshot.Child("bgmVolume").Value) : 1f;
                sfxVolume = dataSnapshot.Child("sfxVolume").Value != null ? Convert.ToSingle(dataSnapshot.Child("sfxVolume").Value) : 1f;
                isMute = dataSnapshot.Child("isMute").Value != null ? Convert.ToBoolean(dataSnapshot.Child("isMute").Value) : false;
            }
            else
            {

                await SaveSettingsAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load sound settings: {e.Message}");
        }
        finally
        {
            ApplyVolumes();
        }
    }

    public async UniTask SaveSettingsAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return;
        }

        try
        {
            var settingsData = new Dictionary<string, object>
            {
                { "masterVolume", masterVolume },
                { "bgmVolume", bgmVolume },
                { "sfxVolume", sfxVolume },
                { "isMute", isMute }
            };

            await settingsRef.UpdateChildrenAsync(settingsData).AsUniTask();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save sound settings: {e.Message}");
        }
    }

    private void ApplyVolumes()
    {
        SetVolume(bgmParam, masterVolume * bgmVolume);
        SetVolume(sfxParam, masterVolume * sfxVolume);
        MuteAll(isMute);
    }

    private void SetVolume(string param, float vol)
    {
        float t = Mathf.Clamp01(vol);

        if(t < 0.02f)
        {
            mixer.SetFloat(param, -80f);
            return;
        }

        const float k = 2.2f;
        t = Mathf.Pow(t, k);

        const float vMin = 0.0001f;
        float v = Mathf.Lerp(vMin, 1f, t);

        float dB = 20f * Mathf.Log10(v);
        mixer.SetFloat(param, dB);
    }

    public void SetMasterVolume(float vol)
    {
        masterVolume = Mathf.Clamp01(vol);
        ApplyVolumes();
    }

    public void SetBGMVolume(float vol)
    {
        bgmVolume = Mathf.Clamp01(vol);
        ApplyVolumes();
    }

    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        ApplyVolumes();
    }

    public void MuteAll(bool mute)
    {
        isMute = mute;
        if (mute)
        {
            mixer.SetFloat(masterParam, -80f);
        }
        else
        {
            mixer.SetFloat(masterParam, 0f);
        }
    }

    public float GetMasterVolume() => masterVolume;
    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMuted() => isMute;

    private AudioSource GetSource(int priority)
    {
        if(audioPool.Count > 0)
        {
            var s = audioPool.Dequeue();
            s.gameObject.SetActive(true);
            s.priority = priority;
            return s;
        }

        return null;
    }

    public void ReturnSource(AudioSource s)
    {
        if(s == null)
        {
            return;
        }

        playing.Remove(s);
        s.gameObject.SetActive(false);
        audioPool.Enqueue(s);
    }

    private void PlaySfx(AudioClip clip, Vector3 pos, int priority = (int)SoundPriority.Normal)
    {
        if(clip == null)
        {
            return;
        }

        int activeCount = POOLSIZE - audioPool.Count;

        if(activeCount >= POOLSIZE - 1)
        {
            foreach(var kv in playing)
            {
                if(kv.Value == clip)
                {
                    return;
                }
            }
        }

        var s = GetSource(priority);
        if(s == null)
        {
            return;
        }

        s.transform.position = pos;
        playing[s] = clip;
        s.PlayOneShot(clip);

        ReturnAfterPlayAsync(s, clip, destroyCts.Token).Forget();
    }

    private async UniTask ReturnAfterPlayAsync(AudioSource s, AudioClip clip, CancellationToken token)
    {
        try
        {
            float est = clip.length / Mathf.Max(0.01f, Mathf.Abs(s.pitch));
            est += 0.02f;

            await UniTask.Delay(TimeSpan.FromSeconds(est), cancellationToken: token);

            while(s != null && s.isPlaying)
            {
                await UniTask.Yield(token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.02f), cancellationToken: token);

            ReturnSource(s);
        }
        catch (OperationCanceledException)
        {
         
        }
    }

    public void PlayPistolShot(Vector3 pos) => PlaySfx(pistolShot, pos, (int)SoundPriority.Normal);
    public void PlayGatlingShot(Vector3 pos) => PlaySfx(gatlingShot, pos, (int)SoundPriority.Normal);
    public void PlaySniperShot(Vector3 pos) => PlaySfx(sniperShot, pos, (int)SoundPriority.Normal);
    public void PlayShotgunShot(Vector3 pos) => PlaySfx(shotgunShot, pos, (int)SoundPriority.Normal);
    public void PlayMissileShot(Vector3 pos) => PlaySfx(missileShot, pos, (int)SoundPriority.Normal);
    public void PlayLaserShot(Vector3 pos) => PlaySfx(laserShot, pos, (int)SoundPriority.Normal);

    public void PlayPlanetHit(Vector3 pos) => PlaySfx(planetHit, pos, (int)SoundPriority.High);
    public void PlayChainEffect(Vector3 pos) => PlaySfx(chainEffect, pos, (int)SoundPriority.VeryHigh);
    public void PlayExplosionEffect(Vector3 pos) => PlaySfx(explosionEffect, pos, (int)SoundPriority.VeryHigh);
    public void PlayReflectShield(Vector3 pos) => PlaySfx(reflectShield, pos, (int)SoundPriority.High);

    public void PlayEnemyLaser(Vector3 pos) => PlaySfx(enemyLaser, pos, (int)SoundPriority.High);
    public void PlaySunFireball(Vector3 pos) => PlaySfx(sunFireball, pos, (int)SoundPriority.High);
    public void PlayBossAppear(Vector3 pos) => PlaySfx(bossAppear, pos, (int)SoundPriority.Critical);
    public void PlayEnemyHit(Vector3 pos) => PlaySfx(enemyHit, pos, (int)SoundPriority.High);

    public void PlayLevelUpSound() => sfxButtonSource.PlayOneShot(levelUpSound);
    public void PlayQuasarSelect() => sfxButtonSource.PlayOneShot(quasarSelect);
    public void PlayRefreshSound() => sfxButtonSource.PlayOneShot(refreshSound);
    public void PlayDeployOpen() => sfxButtonSource.PlayOneShot(deployOpen);
    public void PlayDeployClose() => sfxButtonSource.PlayOneShot(deployClose);
    public void PlayVictorySound() => sfxButtonSource.PlayOneShot(victorySound);
    public void PlayDefeatSound() => sfxButtonSource.PlayOneShot(defeatSound);
    public void PlayClickSound() => sfxButtonSource.PlayOneShot(clickSound);

    public void PlayBGM(AudioClip clip)
    {
        if(clip == null)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayBattleBGM() => PlayBGM(battleBGM);
    public void PlayLobbyBGM() => PlayBGM(lobbyBGM);
    public void PlayLoginBGM() => PlayBGM(loginBGM);

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBgm()
    {
        bgmSource.Pause();
    }

    public void ResumeBgm()
    {
        bgmSource.UnPause();
    }

    public void PauseGameSound()
    {
        foreach(var source in audioPool)
        {
            if(source != null && source.isPlaying)
            {
                source.Pause();
            }
        }

        foreach(var kv in playing)
        {
            if(kv.Key != null && kv.Key.isPlaying)
            {
                kv.Key.Pause();
            }
        }

        
    }

    public void ResumeGameSound()
    {
        foreach (var source in audioPool)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }

        foreach (var kv in playing)
        {
            if (kv.Key != null)
            {
                kv.Key.UnPause();
            }
        }
    }

    public AudioSource PlayLaserShotLoop(Vector3 pos)
    {
        if(laserShot == null)
        {
            return null;
        }

        var source = GetSource((int)SoundPriority.Normal);
        if(source == null)
        {
            return null;
        }

        source.transform.position = pos;
        source.clip = laserShot;
        source.loop = true;
        source.Play();

        playing[source] = laserShot;

        return source;
    }

    public void StopLaserShotLoop(AudioSource source)
    {
        if(source == null)
        {
            return;
        }

        source.Stop();
        source.clip = null;
        source.loop = false;
        playing.Remove(source);
        ReturnSource(source);
    }

    public AudioSource PlayEnemyLaserLoop(Vector3 pos)
    {
        if (enemyLaser == null)
        {
            return null;
        }

        var source = GetSource((int)SoundPriority.High);
        if (source == null)
        {
            return null;
        }

        source.transform.position = pos;
        source.clip = enemyLaser;
        source.loop = true;
        source.Play();

        playing[source] = enemyLaser;

        return source;
    }

    public void StopEnemyLaserLoop(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.Stop();
        source.clip = null;
        source.loop = false;
        playing.Remove(source);
        ReturnSource(source);
    }
    

}
