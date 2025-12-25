using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetLevelUpgradeUI : MonoBehaviour
{
    [SerializeField] private Image planetIcon;
    [SerializeField] private TextMeshProUGUI planetNameText;
    [SerializeField] private TextMeshProUGUI attackPowerText;

    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI itemCountText;

    [SerializeField] private Button levelUpButton;
}
