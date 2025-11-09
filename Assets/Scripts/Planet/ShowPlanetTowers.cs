using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPlanetTowers : MonoBehaviour
{
    [SerializeField] private int towerCount;
    [SerializeField] private GameObject towerBasePrefab;
    [SerializeField] private RectTransform PlanetTransform;

    private List<GameObject> towers;
    private float towerRadius = 200f;

    void Start()
    {
        ResetTowerSlot(towerCount);
    }

    private void ResetTowerSlot(int slotCount)
    {
        towers = new List<GameObject>();

        float angle = 0;
        for (int i = 0; i < slotCount; i++)
        {
            var tower = Instantiate(towerBasePrefab, PlanetTransform);

            var pos = new Vector2(Mathf.Cos((angle + 90f) * Mathf.Deg2Rad), Mathf.Sin((angle + 90f) * Mathf.Deg2Rad)) * towerRadius;
            var rot = new Vector3(0, 0, angle);
            var towerRect = tower.GetComponent<RectTransform>();
            towerRect.localPosition = pos;
            towerRect.rotation = Quaternion.Euler(rot);

            var button = tower.GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => Debug.Log($"Tower{index}"));

            // test

            var image = tower.GetComponentInChildren<Image>();
            image.color = Color.Lerp(Color.red, Color.blue, (float)i / (slotCount - 1));

            //

            angle += 360f / slotCount;

            towers.Add(tower);
        }
    }

}
