using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Button installUIButton;
    [SerializeField] private GameObject towerInstallUiObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        installUIButton.onClick.AddListener(OnOpenInstallUIClicked);
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void OnOpenInstallUIClicked()
    {
        towerInstallUiObj.SetActive(true);
        Time.timeScale = 0f;
    }
}
