using System.Collections.Generic;
using UnityEngine;

public class AsyncRaidUI : MonoBehaviour
{
    [SerializeField] private AsyncRaidManager asyncRaidManager;
    
    [SerializeField] private GameObject userStatPanel;

    private List<AsyncUserPlanet> asyncUserPlanets;

    private void OnEnable()
    {
        if (asyncUserPlanets == null)
            return;

        foreach (var planet in asyncUserPlanets)
        {
            planet.HpDecreseEvent += (damage) => Debug.Log($"{damage}");;
            planet.OnDeathEvent += () => userStatPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (asyncUserPlanets == null)
            return;

        foreach (var planet in asyncUserPlanets)
        {
            planet.HpDecreseEvent -= (damage) => Debug.Log($"{damage}");;
            planet.OnDeathEvent -= () => userStatPanel.SetActive(false);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
