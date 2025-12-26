using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] private Button exitBtn;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private float tempBgmVolume;
    private float tempSfxVolume;

    public void Initialize()
    {
        bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        exitBtn.onClick.AddListener(OnExitBtnClicked);
        exitBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
    }

    public void LoadCurrentSettings()
    {
        if(SoundManager.Instance == null || !SoundManager.Instance.IsInitialized)
        {
            return;
        }

        tempBgmVolume = SoundManager.Instance.GetBGMVolume();
        tempSfxVolume = SoundManager.Instance.GetSFXVolume();

        bgmSlider.SetValueWithoutNotify(tempBgmVolume);
        sfxSlider.SetValueWithoutNotify(tempSfxVolume);
    }

    private void OnBgmVolumeChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }

    private void OnExitBtnClicked()
    {
        SaveSettingsAsync().Forget();

        gameObject.SetActive(false);
    }

    private async UniTask SaveSettingsAsync()
    {
        if(SoundManager.Instance == null)
        {
            return;
        }

        await SoundManager.Instance.SaveSettingsAsync();
    }

    private void OnDestroy()
    {
        bgmSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();
    }
}
