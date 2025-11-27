using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TowerInstallType
{
    Attack,
    Amplifier,
}

public class TowerInstallChoice
{
    public TowerInstallType InstallType;
    public TowerDataSO AttackTowerData;
    public AmplifierTowerDataSO AmplifierTowerData;
    public int ability;
    public int[] BuffSlotIndex; //basic buff
    public int[] RandomAbilitySlotIndex; //ability buff
}

public class TowerInstallControl : MonoBehaviour
{
    [SerializeField] private int towerCount;
    public int TowerCount { get => towerCount; }

    private int currentTowerCount;
    public int CurrentTowerCount
    {
        get { return currentTowerCount; }
        set 
        { 
            currentTowerCount = value;
            OnTowerInstalled?.Invoke();
        }
    }

    private int maxTowerCount = 6;
    public int MaxTowerCount { get => maxTowerCount; }

    public event Action OnTowerInstalled;

    [SerializeField] private RectTransform PlanetTransform;
    [SerializeField] private GameObject planetObj;

    [SerializeField] private float rotateSpeed = 300f;

    [SerializeField] private GameObject towerInfoObj;
    [SerializeField] private GameObject towerUIBasePrefab; //UI
    [SerializeField] private GameObject emptySlotPrefab; //UI

    private Planet planet;
    private List<GameObject> towers;
    public List<GameObject> Towers => towers;

    private float towerRadius = 100f;

    [SerializeField] private PlanetTowerUI planetTowerUI;
    private float currentAngle;

    //test
    private bool[] emptyTower;
    [SerializeField] private List<TowerDataSO> availableTowerDatas; //Attack Tower List
    private TowerDataSO[] assignedTowerDatas;

    public bool IsReadyInstall { get; set; }
    public TowerInstallChoice ChoosedData { get; set; }

    public bool isInstall = true;

    private void Awake()
    {
        planetTowerUI.TowerCount = towerCount;

        emptyTower = new bool[towerCount];
        
        var index = UnityEngine.Random.Range(0, towerCount);
        for (int i = 0; i < towerCount; i++)
        {
            emptyTower[i] = true;
        }
        assignedTowerDatas = new TowerDataSO[towerCount];
    }

    void Start()
    {
        planet = planetObj.GetComponent<Planet>();

        ResetTowerSlot(towerCount);

        if (towerInfoObj != null)
            towerInfoObj.SetActive(false);
        CurrentTowerCount = 0;
    }

    private void Update()
    {
        if (planetTowerUI != null && currentAngle != planetTowerUI.Angle)
        {
            var beforeDiff = currentAngle - planetTowerUI.Angle;
            currentAngle += rotateSpeed * Time.unscaledDeltaTime * (planetTowerUI.TowerRotateClock ? -1f : 1f);
            var newDiff = currentAngle - planetTowerUI.Angle;

            SettingTowerTransform(currentAngle);
            
            if(beforeDiff * newDiff <= 0f)
            {
                currentAngle = planetTowerUI.Angle;
                SettingTowerTransform(currentAngle);
            }
        }
    }

    private void ResetTowerSlot(int slotCount)
    {
        towers = new List<GameObject>();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject tower;
            int index = i;

            if (emptyTower[index])
            {
                tower = Instantiate(emptySlotPrefab, PlanetTransform);
                var buttonEmpty = tower.GetComponent<Button>();
                buttonEmpty.onClick.AddListener(() => IntallNewTower(index));
                towers.Add(tower);

                // Test (Slot Index Text)
                var numtext = tower.GetComponentInChildren<TextMeshProUGUI>();
                numtext.text = index.ToString();

                assignedTowerDatas[index] = null;
                continue;
            }

            //Install Attack Tower (Default: attack tower)
            tower = Instantiate(towerUIBasePrefab, PlanetTransform);
            var chosenData = PickRandomTowerData();
            assignedTowerDatas[index] = chosenData;

            TryAssignDataToTower(tower, chosenData);
            // attack.SetRandomAbility();

            //Install Attack System -> Planet.cs
            //Tower Targeting System Index Debug
            var targeting = tower.GetComponent<TowerTargetingSystem>();
            if (targeting != null)
            {
                targeting.SetSlotIndex(index);
                targeting.SetTowerData(chosenData);
            }

            var button = tower.GetComponent<Button>();
            button.onClick.AddListener(() => OpenInfoUI(index));

            // Test (Color)
            var image = tower.GetComponentInChildren<Image>();
            image.color = Color.Lerp(Color.red, Color.blue, (float)i / (slotCount - 1));

            var text = tower.GetComponentInChildren<TextMeshProUGUI>();
            text.text = index.ToString();

            towers.Add(tower);
            planet?.SetAttackTower(assignedTowerDatas[index], index);
        }
        SettingTowerTransform(0f);
        currentAngle = 0f;   
    }

    public bool IsUsedSlot(int index)
    {
        if (emptyTower == null) return false;

        return !emptyTower[index];
    }

    public void UpgradeTower(int index)
    {
        if (!IsReadyInstall) return;
        if (ChoosedData == null) return;

        planet?.UpgradeTower(index);   

        IsReadyInstall = false;
        isInstall = true;
    }


    private void TryAssignDataToTower(GameObject towerObj, TowerDataSO data)
    {
        if (data == null || towerObj == null) return;

        //UI (Test: Print Tower Name)
        var nameText = towerObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nameText != null) nameText.text = data.towerId;
    }

    private TowerDataSO PickRandomTowerData()
    {
        int idx = UnityEngine.Random.Range(0, availableTowerDatas.Count);
        return availableTowerDatas[idx];
    }

    public void IntallNewTower(int index)
    {
        if (!IsReadyInstall) return;

        if(currentTowerCount >= maxTowerCount)
        {
            Debug.Log("Tower limit");
            return;
        }

        // test Install
        // var newTower = Instantiate(towerBasePrefab, PlanetTransform);
        // var chosenData = PickRandomTowerData();
        if (ChoosedData == null) return;

        Destroy(towers[index]);

        GameObject newTower = null;
        
        // Attack||Amplifier Tower Branch ------------
        if(ChoosedData.InstallType==TowerInstallType.Attack)
        {
            //Install Attack Tower
            newTower = Instantiate(towerUIBasePrefab, PlanetTransform);
            towers[index] = newTower;

            TowerDataSO chosenData = ChoosedData.AttackTowerData;
            assignedTowerDatas[index] = chosenData;
            //UI
            TryAssignDataToTower(newTower, chosenData);

            //Attack Tower Targeting System
            var targeting = newTower.GetComponent<TowerTargetingSystem>();
            if (targeting != null)
            {
                targeting.SetSlotIndex(index); //Debug
                targeting.SetTowerData(chosenData);
            }

            //Set Attack Tower In Planet
            planet?.SetAttackTower(
                assignedTowerDatas[index], 
                index,
                ChoosedData.ability);
        }
        else if(ChoosedData.InstallType == TowerInstallType.Amplifier)
        {
            //Install Amplifier Tower
            newTower = Instantiate(towerUIBasePrefab, PlanetTransform);
            towers[index] = newTower;

            assignedTowerDatas[index] = null; //(assignedTowerDatas: only attack tower)

            //Set Amplifier Tower In Planet
            if (ChoosedData.AmplifierTowerData != null)
            {
                planet?.SetAmplifierTower(
                    ChoosedData.AmplifierTowerData, 
                    index,
                    ChoosedData.ability,
                    ChoosedData.BuffSlotIndex
                    );
            }
        }
        //---------------------------------------------------------

        //UI-------------------------------------------------------
        if (newTower!=null)
        {
            var button = newTower.GetComponent<Button>();
            button.onClick.AddListener(() => OpenInfoUI(index));

            var image = newTower.GetComponentInChildren<Image>();
            var text = newTower.GetComponentInChildren<TextMeshProUGUI>();

            if(ChoosedData.InstallType==TowerInstallType.Attack)
            {
                image.color = Color.Lerp(Color.red, Color.blue, (float)index / (towerCount - 1));
                text.text = index.ToString();
            }else
            {
                image.color = Color.yellow;
                text.text = $"{index}+";
            }
        }
        //----------------------------------------------------------

        SettingTowerTransform(currentAngle);

        emptyTower[index] = false;
        planetTowerUI.gameObject.SetActive(false);

        CurrentTowerCount += 1;
        IsReadyInstall = false;
        ChoosedData = null;
        isInstall = true;
    }

    public void OpenInfoUI(int index)
    {
        Debug.Log($"[OpenInfoUI] clicked index={index}, IsReadyInstall={IsReadyInstall}, towerInfoObj={towerInfoObj}");

        if (IsReadyInstall || towerInfoObj == null) return;

        towerInfoObj.SetActive(true);
        towerInfoObj.GetComponent<TowerInfoUI>().SetInfo(index);
    }
    public TowerDataSO GetTowerData(int index)
    {
        if (index < 0 || index >= assignedTowerDatas.Length) return null;
        return assignedTowerDatas[index];
    }

    private void SettingTowerTransform(float baseAngle)
    {
        foreach (var tower in towers)
        {
            var pos = new Vector2(
                Mathf.Cos((baseAngle + 90f) * Mathf.Deg2Rad), 
                Mathf.Sin((baseAngle + 90f) * Mathf.Deg2Rad)
                ) * towerRadius;
            var rot = new Vector3(0, 0, baseAngle);
            var towerRect = tower.GetComponent<RectTransform>();
            towerRect.localPosition = pos;
            towerRect.rotation = Quaternion.Euler(rot);

            baseAngle += 360f / towerCount;
        }
    }

    public TowerAttack GetAttackTower(int index)
    {
        if (planet == null) return null;
        return planet.GetAttackTowerToAmpTower(index);
    }

    public TowerAmplifier GetAmplifierTower(int index)
    {
        if (planet == null) return null;
        return planet.GetAmplifierTower(index);
    }

    public void UpgradeMaxTowerCount()
    {
        maxTowerCount += 1;
    }
    public TowerDataSO GetRandomAttackTowerDataForCard() //For Pick Card
    {
        return PickRandomTowerData();
    }
}
