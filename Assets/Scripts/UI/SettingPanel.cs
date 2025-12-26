using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] private Button exitBtn;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI bgmText;
    [SerializeField] private TextMeshProUGUI sfxText;

    private float tempBgmVolume;
    private float tempSfxVolume;

    public void Initialize()
    {
        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 10f;
        bgmSlider.wholeNumbers = true;

        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 10f;
        sfxSlider.wholeNumbers = true;

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

        bgmSlider.SetValueWithoutNotify(tempBgmVolume * 10f);
        sfxSlider.SetValueWithoutNotify(tempSfxVolume * 10f);

        UpdateVolumeTexts();
    }

    private void OnBgmVolumeChanged(float value)
    {
        float actualVolume = value / 10f;
        SoundManager.Instance.SetBGMVolume(actualVolume);

        bgmText.text = $"{Mathf.RoundToInt(value * 10f)}";
    }

    private void OnSfxVolumeChanged(float value)
    {
        float actualVolume = value / 10f;
        SoundManager.Instance.SetSFXVolume(actualVolume);

        sfxText.text = $"{Mathf.RoundToInt(value * 10f)}";
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

    private void UpdateVolumeTexts()
    {
        float bgmVolume = SoundManager.Instance.GetBGMVolume();
        float sfxVolume = SoundManager.Instance.GetSFXVolume();

        bgmText.text = $"{Mathf.RoundToInt(bgmVolume * 100f)}";
        sfxText.text = $"{Mathf.RoundToInt(sfxVolume * 100f)}";
    }
}
