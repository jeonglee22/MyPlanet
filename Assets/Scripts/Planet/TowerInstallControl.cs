using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private GameObject towerBasePrefab;
    [SerializeField] private GameObject towerInfoObj;
    [SerializeField] private RectTransform PlanetTransform;
    [SerializeField] private float rotateSpeed = 300f;
    [SerializeField] private GameObject emptySlotPrefab;
    [SerializeField] private GameObject planetObj;

    private Planet planet;
    private List<GameObject> towers;
    public List<GameObject> Towers => towers;

    private float towerRadius = 100f;

    [SerializeField] private PlanetTowerUI planetTowerUI;
    private float currentAngle;

    //test
    private bool[] emptyTower;
    [SerializeField] private List<TowerDataSO> availableTowerDatas;
    private TowerDataSO[] assignedTowerDatas;

    public bool IsReadyInstall { get; set; }
    public (IAbility ability, string towerData) ChoosedData { get; set; }
    // public (Color color, TowerDataSO towerData) ChoosedData { get; set; }

    private void Awake()
    {
        planetTowerUI.TowerCount = towerCount;

        emptyTower = new bool[towerCount];
        
        var index = UnityEngine.Random.Range(0, towerCount);
        for (int i = 0; i < towerCount; i++)
        {
            emptyTower[i] = true;

            if (i == index)
                emptyTower[i] = false;
        }
        assignedTowerDatas = new TowerDataSO[towerCount];
    }

    void Start()
    {
        planet = planetObj.GetComponent<Planet>();

        ResetTowerSlot(towerCount);
        towerInfoObj.SetActive(false);
        CurrentTowerCount = 1;
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

                // test
                var numtext = tower.GetComponentInChildren<TextMeshProUGUI>();
                numtext.text = index.ToString();
                //

                assignedTowerDatas[index] = null;
                continue;
            }

            //Install Tower
            tower = Instantiate(towerBasePrefab, PlanetTransform);
            var chosenData = PickRandomTowerData();
            assignedTowerDatas[index] = chosenData;

            TryAssignDataToTower(tower, chosenData);
            // attack.SetRandomAbility();

            //Init TowerAttack
            var attack = tower.GetComponent<TowerAttack>();
            if (attack != null) attack.SetTowerData(chosenData);

            //Tower Targeting System Index Debug
            var targeting = tower.GetComponent<TowerTargetingSystem>();
            if (targeting != null)
            {
                targeting.SetSlotIndex(index);
                targeting.SetTowerData(chosenData);
            }


            var button = tower.GetComponent<Button>();
            button.onClick.AddListener(() => OpenInfoUI(index));

            // test
            var image = tower.GetComponentInChildren<Image>();
            image.color = Color.Lerp(Color.red, Color.blue, (float)i / (slotCount - 1));

            var text = tower.GetComponentInChildren<TextMeshProUGUI>();
            text.text = index.ToString();

            towers.Add(tower);
            planet?.SetTower(assignedTowerDatas[index], index);
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

        planet?.UpgradeTower(index);
        IsReadyInstall = false;
    }

    private void TryAssignDataToTower(GameObject towerObj, TowerDataSO data)
    {
        if (data == null || towerObj == null) return;

        //UI
        var nameText = towerObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = data.towerId;
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
        var newTower = Instantiate(towerBasePrefab, PlanetTransform);
        var chosenData = PickRandomTowerData();

        Destroy(towers[index]);
        towers[index] = newTower;

        //Debug targeting system index
        var targeting = newTower.GetComponent<TowerTargetingSystem>();
        if(targeting!=null)
        {
            targeting.SetSlotIndex(index);
            targeting.SetTowerData(chosenData);
        }

        var button = newTower.GetComponent<Button>();
        button.onClick.AddListener(() => OpenInfoUI(index));

        var image = newTower.GetComponentInChildren<Image>();
        image.color = Color.Lerp(Color.red, Color.blue, (float)index / (towerCount - 1));

        assignedTowerDatas[index] = chosenData;
        TryAssignDataToTower(newTower, chosenData);

        // test index
        var text = newTower.GetComponentInChildren<TextMeshProUGUI>();
        text.text = index.ToString();

        planet?.SetTower(assignedTowerDatas[index], index, ChoosedData.ability);
        SettingTowerTransform(currentAngle);

        emptyTower[index] = false;
        planetTowerUI.gameObject.SetActive(false);

        CurrentTowerCount += 1;
        IsReadyInstall = false;
    }

    private void OpenInfoUI(int index)
    {
        if (IsReadyInstall) return;

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
            var pos = new Vector2(Mathf.Cos((baseAngle + 90f) * Mathf.Deg2Rad), Mathf.Sin((baseAngle + 90f) * Mathf.Deg2Rad)) * towerRadius;
            var rot = new Vector3(0, 0, baseAngle);
            var towerRect = tower.GetComponent<RectTransform>();
            towerRect.localPosition = pos;
            towerRect.rotation = Quaternion.Euler(rot);

            baseAngle += 360f / towerCount;
        }
    }
}
