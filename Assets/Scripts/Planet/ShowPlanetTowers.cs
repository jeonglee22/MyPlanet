using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPlanetTowers : MonoBehaviour
{
    [SerializeField] private int towerCount;
    [SerializeField] private GameObject towerBasePrefab;
    [SerializeField] private GameObject towerInfoObj;
    [SerializeField] private RectTransform PlanetTransform;

    private List<GameObject> towers;
    private float towerRadius = 200f;

    private PlanetTowerUI planetTowerUI;
    private float currentAngle;

    private void Awake()
    {
        planetTowerUI = GetComponent<PlanetTowerUI>();
        planetTowerUI.TowerCount = towerCount;
    }

    void Start()
    {
        ResetTowerSlot(towerCount);
        towerInfoObj.SetActive(false);
    }

    private void Update()
    {
        if (planetTowerUI != null && currentAngle != planetTowerUI.Angle)
        {
            currentAngle = planetTowerUI.Angle;
            SettingTowerTransform(currentAngle);
        }     
    }

    private void ResetTowerSlot(int slotCount)
    {
        towers = new List<GameObject>();

        for (int i = 0; i < slotCount; i++)
        {
            var tower = Instantiate(towerBasePrefab, PlanetTransform);

            var button = tower.GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => OpenInfoUI(index));

            // test

            var image = tower.GetComponentInChildren<Image>();
            image.color = Color.Lerp(Color.red, Color.blue, (float)i / (slotCount - 1));

            //

            towers.Add(tower);
        }

        SettingTowerTransform(0f);
        currentAngle = 0f;
    }

    private void OpenInfoUI(int index)
    {
        towerInfoObj.SetActive(true);
        towerInfoObj.GetComponent<TowerInfoUI>().SetInfo(index);
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
