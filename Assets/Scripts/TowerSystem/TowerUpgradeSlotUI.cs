using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerUpgradeSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject[] upgradeUIs;
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private TowerInfoUI towerInfoUI;
    [SerializeField] private GameObject dragImagePrefab;
    private bool towerImageIsDraging = false;
    private bool isNewTouch;
    private bool isStartTouch = false;
    private Vector2 initTouchPos;

    //Add Amplifier Choice
    [SerializeField] private AmplifierTowerDataSO damageMatrixCoreSO;
    [SerializeField] private AmplifierTowerDataSO proejctileCoreSO;
    private TowerInstallChoice[] choices; //Tuple(Ability,Index) -> Struct(Add Tower Type)

    //test
    private Color towerColor;
    private List<int> numlist;
    [SerializeField] private TextMeshProUGUI[] uiTexts;
    private IAbility[] abilities;
    public Color choosedColor { get; private set; }

    private bool isNotUpgradeOpen = true;
    public bool IsNotUpgradeOpen
    {
        get { return isNotUpgradeOpen; }
        set { isNotUpgradeOpen = value; }
    }

    private GameObject dragImage = null;
    private int choosedIndex = -1;
    [SerializeField] private Button refreshButton;
    private string canRefreshText = "Refresh";
    private string cannotRefreshText = "IsUsed";

    private void Start()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);
        towerColor = Color.yellow;

        refreshButton.onClick.AddListener(OnClickRefreshButton);
        refreshButton.gameObject.SetActive(false);

        Debug.Log("[UpgradeUI] Start - upgradeUIs:" + upgradeUIs.Length);
    }

    private void OnEnable()
    {
        Debug.Log("[UpgradeUI] OnEnable - isNotUpgradeOpen = " + isNotUpgradeOpen);

        if (isNotUpgradeOpen)
        {
            isNotUpgradeOpen = false;
            refreshButton.gameObject.SetActive(false);
            return;
        }

        foreach (var ui in upgradeUIs)
            ui.SetActive(true);

        refreshButton.gameObject.SetActive(true);
        refreshButton.interactable = true;
        refreshButton.GetComponentInChildren<TextMeshProUGUI>().text = canRefreshText;

        SettingUpgradeCards();
    }

    private void OnDisable()
    {
        Debug.Log("[UpgradeUI] OnDisable");

        foreach (var ui in upgradeUIs)
            ui.SetActive(false);

        Time.timeScale = 1f;
        isStartTouch = false;
        towerImageIsDraging = false;
    }

    private void SettingUpgradeCards()
    {
        Debug.Log("[UpgradeUI] SettingUpgradeCards 호출");

        ResetChoose();
        installControl.IsReadyInstall = false;

        numlist = new List<int>();
        for (int i = 0; i < uiTexts.Length; i++)
        {
            int number;
            int count = 0;
            do
            {
                number = Random.Range(0, installControl.TowerCount);
                count++;
            } while (numlist.Contains(number) && count < installControl.TowerCount);
            numlist.Add(number);


            if (installControl == null) continue;
            //UsedSlot ? UpgradeCard : New Tower-------------------
            if (!installControl.IsUsedSlot(number))
            {
                //Random Tower Type (0:Attack, 1:DamageMatrix, 2:ProjectileCore)
                int towerType = Random.Range(0, 3);

                if(towerType==0) //Attack
                {
                    choices[i].InstallType = TowerInstallType.Attack;
                    choices[i].ability = abilities[i];
                    choices[i].AmplifierTowerData = null;
                    uiTexts[i].text = $"new\nAttack\nTower\n\n{abilities[i]}";

                    Debug.Log($"[UpgradeUI] 카드 {i} - 빈 슬롯 {number}, 타입=Attack, ability={abilities[i]}");
                }
                else if(towerType==1) //Damage Matrix
                {
                    choices[i].InstallType = TowerInstallType.Amplifier;
                    choices[i].ability = null;
                    choices[i].AmplifierTowerData = damageMatrixCoreSO;
                    uiTexts[i].text = $"new\nDamage\nMatrix\n\n{abilities[i]}";

                    Debug.Log($"[UpgradeUI] 카드 {i} - 빈 슬롯 {number}, 타입=DamageMatrix");
                }
                else //Projectile Core
                {
                    choices[i].InstallType = TowerInstallType.Amplifier;
                    choices[i].ability = null;
                    choices[i].AmplifierTowerData = proejctileCoreSO;
                    uiTexts[i].text = $"new\nProjectile\nCore\n\n{abilities[i]}";

                    Debug.Log($"[UpgradeUI] 카드 {i} - 빈 슬롯 {number}, 타입=ProjectileCore");
                }
            }
            else
            {
                choices[i].InstallType = TowerInstallType.Attack;
                choices[i].ability = abilities[i];
                choices[i].AmplifierTowerData=null;
                uiTexts[i].text = $"Upgrade\n{number}";

                Debug.Log($"[UpgradeUI] 카드 {i} - 사용중 슬롯 {number}, 업그레이드 카드");
            }
        }
    }

    private void OnClickRefreshButton()
    {
        SettingUpgradeCards();
        refreshButton.GetComponentInChildren<TextMeshProUGUI>().text = cannotRefreshText;
        refreshButton.interactable = false;
    }

    public void OnClickUpgradeUIClicked(int index)
    {
        Debug.Log($"[UpgradeUI] 카드 클릭 index={index}, numlist={numlist[index]}");

        var currentColor = upgradeUIs[index].GetComponentInChildren<Image>().color;
        if (currentColor != Color.white)
        {
            Debug.Log("[UpgradeUI] 이미 선택된 카드 다시 클릭 → 선택 해제");

            installControl.IsReadyInstall = false;
            upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;
            return;
        }

        choosedColor = towerColor;
        upgradeUIs[index].GetComponentInChildren<Image>().color = choosedColor;
        upgradeUIs[(index + 1) % 3].GetComponentInChildren<Image>().color = Color.white;
        upgradeUIs[(index + 2) % 3].GetComponentInChildren<Image>().color = Color.white;
        //installControl.IsReadyInstall = true;
        //installControl.ChoosedData = (abilities[index], uiTexts[index].text);

        if (installControl.IsUsedSlot(numlist[index]))
        {
            Debug.Log($"[UpgradeUI] 업그레이드 슬롯 클릭 → UpgradeTower({numlist[index]}) 호출");

            installControl.UpgradeTower(numlist[index]);
            towerInfoUI.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
    
    private void ResetChoose()
    {
        Debug.Log("[UpgradeUI] ResetChoose 호출");

        abilities = new IAbility[upgradeUIs.Length];
        choices = new TowerInstallChoice[upgradeUIs.Length];

        for(int i = 0; i < upgradeUIs.Length; i++)
        {
            upgradeUIs[i].GetComponentInChildren<Image>().color = Color.white;
            abilities[i] = AbilityManager.Instance.GetRandomAbility();
            choices[i] = new TowerInstallChoice();

            Debug.Log($"[UpgradeUI] 카드 {i} 초기화 - ability={abilities[i]}");
        }
    }

    public void OnTouchMakeDrageImage(InputAction.CallbackContext context)
    {
        if(!context.performed || towerImageIsDraging)
            return;

        var touchPos = context.ReadValue<Vector2>();
        Debug.Log($"[UpgradeUI] OnTouchMakeDrageImage performed - pos={touchPos}, isStartTouch={isStartTouch}, isNewTouch={isNewTouch}");

        if (!isStartTouch)
        {
            isStartTouch = true;
            initTouchPos = touchPos;
            Debug.Log($"[UpgradeUI] 드래그 시작 지점 기록: {initTouchPos}");
        }

        if(Vector2.Distance(initTouchPos, touchPos) < 5f || !isNewTouch)
            return;

        choosedIndex = -1;
        foreach (var upgradeUi in upgradeUIs)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(upgradeUi.GetComponent<RectTransform>(), touchPos))
            {
                choosedIndex = System.Array.IndexOf(upgradeUIs, upgradeUi);
            }
        }

        Debug.Log($"[UpgradeUI] DragStart - choosedIndex={choosedIndex}, isUsedSlot={(choosedIndex >= 0 ? installControl.IsUsedSlot(numlist[choosedIndex]).ToString() : "N/A")}");

        if (choosedIndex == -1 || 
            installControl.IsUsedSlot(numlist[choosedIndex]))
            return;

        dragImage = Instantiate(dragImagePrefab, upgradeUIs[choosedIndex].transform);
        towerImageIsDraging = true;
        dragImage.SetActive(true);
    }

    public void OnTouchStateCheck(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isStartTouch = false;
            towerImageIsDraging = false;
            isNewTouch = true;
        }
        if (context.canceled)
        {
            isStartTouch = false;
            towerImageIsDraging = false;
            isNewTouch = false;

            var index = GetEndTouchOnInstallArea();

            Debug.Log($"[UpgradeUI] TouchEnd - slotIndex={index}, choosedIndex={choosedIndex}, dragImageNull={dragImage == null}");
            if (index != -1 && dragImage != null && choosedIndex != -1)
            {
                installControl.IsReadyInstall = true;
                installControl.ChoosedData = choices[choosedIndex];
                installControl.IntallNewTower(index);
                
                Destroy(dragImage);
                dragImage = null;
                gameObject.SetActive(false);
            }

            choosedIndex = -1;
        }
    }

    private int GetEndTouchOnInstallArea()
    {
        var touchScreen = Touchscreen.current;
        if (touchScreen == null) return -1;

        var primary = touchScreen.primaryTouch;

        if (primary.press.isPressed)
            return -1;

        var touchPos = primary.position.ReadValue();

        var towers = installControl.Towers;
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if(!installControl.IsUsedSlot(i) && RectTransformUtility.RectangleContainsScreenPoint(towers[i].GetComponent<RectTransform>(), touchPos))
            {
                return i;
            }
        }

        return -1;
    }
}