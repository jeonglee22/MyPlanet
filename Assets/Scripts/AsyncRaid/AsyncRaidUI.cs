using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AsyncRaidUI : MonoBehaviour
{
    [SerializeField] private AsyncRaidManager asyncRaidManager;
    
    [SerializeField] private GameObject userStatPanel;

    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image connectionIcon;
    [SerializeField] private Image disConnectionIcon;
    [SerializeField] private Image[] hpFillImages;
    [SerializeField] private Image backGroundImage;
    [SerializeField] private Image[] infoPanels;    

    private AsyncUserPlanet asyncUserPlanet;
    private Color initColor;
    private Color initTransparentColor;

    private void OnEnable()
    {
        asyncUserPlanet = asyncRaidManager.UserPlanet;

        Debug.Log("AsyncRaidUI OnEnable :" + asyncUserPlanet);

        if (asyncUserPlanet == null)
            return;

        var planet = asyncUserPlanet;

        planet.HpDecreseEvent += (health) => ControllImageCount(health);
        planet.OnDeathEvent += () => ChangeToDeathState();
        planet.OnDeathEvent += () => planet.gameObject.SetActive(false);

        SetUserNickname(planet.BlurNickname);

        // SetTransparentUserPlanetInfo();
    }

    private void OnDisable()
    {
        if (asyncUserPlanet == null)
            return;

        asyncUserPlanet.HpDecreseEvent -= (health) => ControllImageCount(health);
        asyncUserPlanet.OnDeathEvent -= () => ChangeToDeathState();
        asyncUserPlanet.OnDeathEvent -= () => asyncUserPlanet.gameObject.SetActive(false);
    }

    void Start()
    {
        gameObject.SetActive(false);

        infoPanels[2].gameObject.SetActive(false);
        infoPanels[3].gameObject.SetActive(false);
        infoPanels[4].gameObject.SetActive(false);
        infoPanels[5].gameObject.SetActive(false);
    }

    public void OnClickResetRaid()
    {
        asyncRaidManager.IsSettingAsyncUserPlanet = false;
        asyncRaidManager.CanStartSpawn = true;
        asyncRaidManager.IsStartRaid = false;

        gameObject.SetActive(false);

        if (asyncUserPlanet == null)
            return;

        Destroy(asyncUserPlanet.gameObject);
        ResetImages();
    }

    public void OnClickStartRaid()
    {
        asyncRaidManager.IsStartRaid = true;
    }

    public void SetTransparentUserPlanetInfo()
    {
        connectionIcon.gameObject.SetActive(false);
        disConnectionIcon.gameObject.SetActive(false);
        initColor = backGroundImage.color;
        initTransparentColor = infoPanels[0].color;
        backGroundImage.color = new Color(0f,0f,0f,0f);
        infoPanels[0].color = new Color(0f,0f,0f,0f);
        infoPanels[1].color = new Color(0f,0f,0f,0f);
        nicknameText.color = new Color(0f,0f,0f,0f);
        

        foreach (var img in hpFillImages)
        {
            img.fillAmount = 0f;
        }
    }

    public void ControllImageCount(float hp)
    {
        var hpRatio = hp / 100f * hpFillImages.Length - Mathf.FloorToInt(hp / 100f * hpFillImages.Length);
        var i = Mathf.FloorToInt(hp / 100f * hpFillImages.Length);

        if (i < 0)
        {
            i = 0;
            hpRatio = 0;
            hpFillImages[0].fillAmount = 0f;
            return;
        }
        if (i < hpFillImages.Length-1)
        {
            hpFillImages[i+1].fillAmount = 0f;
        }
        // images[i].fillAmount = hpRatio;
    }

    public void SetUserNickname(string nickname)
    {
        nicknameText.text = nickname;
    }

    private void ChangeToDeathState()
    {
        connectionIcon.gameObject.SetActive(false);
        disConnectionIcon.gameObject.SetActive(true);
        initColor = backGroundImage.color;
        initTransparentColor = infoPanels[0].color;
        backGroundImage.color = new Color(0.5f, 0.5f, 0.5f, initColor.a);
        infoPanels[0].color = new Color(0.5f, 0.5f, 0.5f, initTransparentColor.a);
        infoPanels[1].color = new Color(0.5f, 0.5f, 0.5f, initColor.a);
        // disConnectionIcons[index].color = new Color(0.5f, 0.5f, 0.5f, initColor.a);
    }

    private void ResetImages()
    {
        foreach (var img in hpFillImages)
        {
            img.fillAmount = 1f;
        }

        connectionIcon.gameObject.SetActive(true);
        disConnectionIcon.gameObject.SetActive(false);
        backGroundImage.color = initColor;
        infoPanels[0].color = initTransparentColor;
        infoPanels[1].color = initColor;
        nicknameText.color = Color.white;
    }
}
