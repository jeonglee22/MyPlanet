using TMPro;
using UnityEngine;

public class PlanetItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI planetLevelText;

    public void Initialize(PlanetData planetData)
    {
        planetLevelText.text = $"Lv. {planetData.PlanetName}";
    }
}
