using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerInstallControl : MonoBehaviour
{
    [SerializeField] private int towerCount;
    [SerializeField] private GameObject towerBasePrefab;
    [SerializeField] private GameObject towerInfoObj;
    [SerializeField] private RectTransform PlanetTransform;
    [SerializeField] private float rotateSpeed = 300f;
    [SerializeField] private GameObject emptySlotPrefab;
    [SerializeField] private GameObject planetObj;
    private Planet planet;
    private List<GameObject> towers;
    private float towerRadius = 100f;

    [SerializeField] private PlanetTowerUI planetTowerUI;
    private float currentAngle;

    //test
    private bool[] emptyTowerTest;
    [SerializeField] private List<TowerDataSO> availableTowerDatas;
    private TowerDataSO[] assignedTowerDatas;

    private void Awake()
    {
        planetTowerUI.TowerCount = towerCount;

        emptyTowerTest = new bool[towerCount];
        for (int i = 0; i < towerCount; i++)
        {
            emptyTowerTest[i] = Random.value < 0.5f;
        }
        assignedTowerDatas = new TowerDataSO[towerCount];
    }

    void Start()
    {
        planet = planetObj.GetComponent<Planet>();

        ResetTowerSlot(towerCount);
        towerInfoObj.SetActive(false);

        DebugTowerSetup(); //debug
    }

    private void Update()
    {
        if (planetTowerUI != null && currentAngle != planetTowerUI.Angle)
        {
            currentAngle += rotateSpeed * Time.unscaledDeltaTime * (planetTowerUI.TowerRotateClock ? -1f : 1f);

            SettingTowerTransform(currentAngle);
            
            if(Mathf.Abs(currentAngle - planetTowerUI.Angle) < 10f)
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

            if (emptyTowerTest[index])
            {
                tower = Instantiate(emptySlotPrefab, PlanetTransform);
                var buttonEmpty = tower.GetComponent<Button>();
                buttonEmpty.onClick.AddListener(() => OpenInstallUI(index));
                towers.Add(tower);

                assignedTowerDatas[index] = null;
                continue;
            }
            else
            {
                tower = Instantiate(towerBasePrefab, PlanetTransform);
                var chosenData = PickRandomTowerData();
                assignedTowerDatas[index] = chosenData;

                //Init TargetingSystem 
                var targeting = tower.GetComponentInChildren<TowerTargetingSystem>();
                if (targeting != null) targeting.SetTowerData(chosenData);
                
                //Init TowerAttack
                var attack = tower.GetComponentInChildren<TowerAttack>();
                if (attack != null) attack.SetTowerData(chosenData);
            }
            
            var button = tower.GetComponent<Button>();
            button.onClick.AddListener(() => OpenInfoUI(index));

            // test
            var image = tower.GetComponentInChildren<Image>();
            image.color = Color.Lerp(Color.red, Color.blue, (float)i / (slotCount - 1));

            towers.Add(tower);
        }

        SettingTowerTransform(0f);
        currentAngle = 0f;   
    }

    private void TryAssignDataToTower(GameObject towerObj, TowerDataSO data)
    {
        if (data == null || towerObj == null) return;

        var targeting = towerObj.GetComponent<TowerTargetingSystem>();
        if (targeting != null)
        {
            targeting.SetTowerData(data);
        }

        var nameText = towerObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = data.towerId;
    }

    private TowerDataSO PickRandomTowerData()
    {
        int idx = Random.Range(0, availableTowerDatas.Count);
        return availableTowerDatas[idx];
    }

    private void OpenInstallUI(int index)
    {
        // test Install
        var newTower = Instantiate(towerBasePrefab, PlanetTransform);

        var chosenData = PickRandomTowerData();

        Destroy(towers[index]);
        towers[index] = newTower;

        var button = newTower.GetComponent<Button>();
        button.onClick.AddListener(() => OpenInfoUI(index));

        var image = newTower.GetComponentInChildren<Image>();
        image.color = Color.Lerp(Color.red, Color.blue, (float)index / (towerCount - 1));

        //Init TargetingSystem 
        var targeting = newTower.GetComponentInChildren<TowerTargetingSystem>();
        if (targeting != null) targeting.SetTowerData(chosenData);

        //Init TowerAttack
        var attack = newTower.GetComponentInChildren<TowerAttack>();
        if (attack != null) attack.SetTowerData(chosenData);

        planet?.SetTower(assignedTowerDatas[index], index);
        SettingTowerTransform(currentAngle);
    }

    private void OpenInfoUI(int index)
    {
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

    private void DebugTowerSetup() //debug 
    {
        for (int i = 0; i < towers.Count; i++)
        {
            var tower = towers[i];
            if (tower == null) continue;

            var data = assignedTowerDatas[i];
            string dataName = data != null ? data.towerId : "null";

            var targeting = tower.GetComponentInChildren<TowerTargetingSystem>();
            string targetingStatus = targeting != null
                ? $"Range:{targeting.AssignedTowerData?.rangeData?.GetRange() ?? 0} Target:{targeting.AssignedTowerData?.targetPriority}"
                : "No TargetingSystem";

            var attack = tower.GetComponentInChildren<TowerAttack>();
            string attackStatus = attack != null ? "TowerAttack OK" : "No TowerAttack";

            Debug.Log($"Tower[{i}] -> Data:{dataName} | {targetingStatus} | {attackStatus}");
        }
    }
}