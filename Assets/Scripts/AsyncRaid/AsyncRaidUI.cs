using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    private List<AsyncUserPlanet> asyncUserPlanets;

    private void OnEnable()
    {
        asyncUserPlanets = asyncRaidManager.AsyncUserPlanets;

        if (asyncUserPlanets == null)
            return;

        for (int i = 0; i < asyncUserPlanets.Count; i++)
        {
            var planet = asyncUserPlanets[i];
            var index = i;

            planet.HpDecreseEvent += (health) => ControllImageCount(health, index);
            planet.OnDeathEvent += () => userStatPanel.SetActive(false);
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
            planet.OnDeathEvent -= () => userStatPanel.SetActive(false);
        }
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(WaveManager.Instance.IsBossBattle && !asyncRaidManager.IsSettingAsyncUserPlanet)
        {
            gameObject.SetActive(true);
        }
    }

    public void OnClickResetRaid()
    {
        asyncRaidManager.IsSettingAsyncUserPlanet = false;
        if (asyncUserPlanets == null)
            return;

        foreach (var planet in asyncUserPlanets)
        {
            planet.Die();
            Destroy(planet.gameObject);
        }
    }

    public void SetTransparentUserPlanetInfo(int index)
    {
        
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

        images[i].fillAmount = hpRatio;
    }
}
