using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AsyncRaidUI : MonoBehaviour
{
    [SerializeField] private AsyncRaidManager asyncRaidManager;
    
    [SerializeField] private GameObject userStatPanel;

    [SerializeField] private TextMeshProUGUI[] nicknameTexts;
    [SerializeField] private Image[] connectionIcons;
    [SerializeField] private Image[] disConnectionIcons;
    [SerializeField] private Image[] hpFillImages1;
    [SerializeField] private Image[] hpFillImages2;
    [SerializeField] private Image[] hpFillImages3;
    [SerializeField] private Image[] backGroundImages;
    [SerializeField] private Image[] infoPanels;

    private List<AsyncUserPlanet> asyncUserPlanets;
    private Color initColor;
    private Color initTransparentColor;

    private void OnEnable()
    {
        asyncUserPlanets = asyncRaidManager.AsyncUserPlanets;

        Debug.Log("AsyncRaidUI OnEnable :" + asyncUserPlanets?.Count);

        if (asyncUserPlanets == null)
            return;

        for (int i = 0; i < asyncUserPlanets.Count; i++)
        {
            var planet = asyncUserPlanets[i];
            var index = i;

            planet.HpDecreseEvent += (health) => ControllImageCount(health, index);
            planet.OnDeathEvent += () => ChangeToDeathState(index);
            planet.OnDeathEvent += () => planet.gameObject.SetActive(false);

            SetUserNickname(i, planet.BlurNickname);
        }

        for (int i = asyncUserPlanets.Count; i < 3; i++)
        {
            SetTransparentUserPlanetInfo(i);
        }
    }

    private void OnDisable()
    {
        if (asyncUserPlanets == null)
            return;

        for (int i = 0; i < asyncUserPlanets.Count; i++)
        {
            var planet = asyncUserPlanets[i];
            var index = i;

            planet.HpDecreseEvent -= (health) => ControllImageCount(health, index);
            planet.OnDeathEvent -= () => ChangeToDeathState(index);
            planet.OnDeathEvent -= () => planet.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void OnClickResetRaid()
    {
        asyncRaidManager.IsSettingAsyncUserPlanet = false;
        asyncRaidManager.CanStartSpawn = true;
        asyncRaidManager.IsStartRaid = false;

        gameObject.SetActive(false);

        if (asyncUserPlanets == null)
            return;

        foreach (var planet in asyncUserPlanets)
        {
            Destroy(planet.gameObject);
            ResetImages();
        }
    }

    public void OnClickStartRaid()
    {
        asyncRaidManager.IsStartRaid = true;
    }

    public void SetTransparentUserPlanetInfo(int index)
    {
        connectionIcons[index].gameObject.SetActive(false);
        disConnectionIcons[index].gameObject.SetActive(false);
        initColor = backGroundImages[index].color;
        initTransparentColor = infoPanels[index*2].color;
        backGroundImages[index].color = new Color(0f,0f,0f,0f);
        infoPanels[index*2].color = new Color(0f,0f,0f,0f);
        infoPanels[index*2+1].color = new Color(0f,0f,0f,0f);
        nicknameTexts[index].color = new Color(0f,0f,0f,0f);
        
        var images = index switch
        {
            0 => hpFillImages1,
            1 => hpFillImages2,
            2 => hpFillImages3,
            _ => null
        };

        foreach (var img in images)
        {
            img.fillAmount = 0f;
        }
    }

    public void ControllImageCount(float hp, int index)
    {
        var images = index switch
        {
            0 => hpFillImages1,
            1 => hpFillImages2,
            2 => hpFillImages3,
            _ => null
        };

        var hpRatio = hp / 100f * images.Length - Mathf.FloorToInt(hp / 100f * images.Length);
        var i = Mathf.FloorToInt(hp / 100f * images.Length);

        if (i < 0)
        {
            i = 0;
            hpRatio = 0;
            images[0].fillAmount = 0f;
            return;
        }
        if (i < images.Length-1)
        {
            images[i+1].fillAmount = 0f;
        }
        // images[i].fillAmount = hpRatio;
    }

    public void SetUserNickname(int index, string nickname)
    {
        nicknameTexts[index].text = nickname;
    }

    private void ChangeToDeathState(int index)
    {
        connectionIcons[index].gameObject.SetActive(false);
        disConnectionIcons[index].gameObject.SetActive(true);
        initColor = backGroundImages[index].color;
        initTransparentColor = infoPanels[index*2].color;
        backGroundImages[index].color = new Color(0.5f, 0.5f, 0.5f, initColor.a);
        infoPanels[index*2].color = new Color(0.5f, 0.5f, 0.5f, initTransparentColor.a);
        infoPanels[index*2+1].color = new Color(0.5f, 0.5f, 0.5f, initColor.a);
        // disConnectionIcons[index].color = new Color(0.5f, 0.5f, 0.5f, initColor.a);
    }

    private void ResetImages()
    {
        foreach (var img in hpFillImages1)
        {
            img.fillAmount = 1f;
        }
        foreach (var img in hpFillImages2)
        {
            img.fillAmount = 1f;
        }
        foreach (var img in hpFillImages3)
        {
            img.fillAmount = 1f;
        }

        for (int i = 0; i < connectionIcons.Length; i++)
        {
            connectionIcons[i].gameObject.SetActive(true);
            disConnectionIcons[i].gameObject.SetActive(false);
            backGroundImages[i].color = initColor;
            infoPanels[i*2].color = initTransparentColor;
            infoPanels[i*2 + 1].color = initColor;
            nicknameTexts[i].color = Color.white;
        }
    }
}
