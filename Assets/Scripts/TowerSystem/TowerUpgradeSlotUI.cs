using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private TextMeshProUGUI towerInstallText;
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
    private int[] abilities;
    public Color choosedColor { get; private set; }

    private bool isNotUpgradeOpen = false;
    public bool IsNotUpgradeOpen
    {
        get { return isNotUpgradeOpen; }
        set { isNotUpgradeOpen = value; }
    }

    private GameObject dragImage = null;
    private int choosedIndex = -1;
    private bool isFirstInstall = true;
    [SerializeField] private Button[] refreshButtons;

    private void Start()
    {
        // foreach (var ui in upgradeUIs)
        //     ui.SetActive(false);
        towerColor = Color.yellow;

        // SetActiveRefreshButtons(false);
        installControl.OnTowerInstalled += SetTowerInstallText;
    }

    void OnDestroy()
    {
        installControl.OnTowerInstalled -= SetTowerInstallText;
    }

    private async UniTaskVoid OnEnable()
    {
        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);

        if (isNotUpgradeOpen)
        {
            isNotUpgradeOpen = false;
            SetActiveRefreshButtons(false);
            return;
        }

        foreach (var ui in upgradeUIs)
            ui.SetActive(true);

        foreach (var refreshButton in refreshButtons)
        {
            refreshButton.gameObject.SetActive(true);
            refreshButton.interactable = true;
        }

        SettingUpgradeCards();
        
    }

    private void SetTowerInstallText()
    {
        Debug.Log("SetTowerInstallText");
        towerInstallText.text = $"({installControl.CurrentTowerCount}/{installControl.MaxTowerCount})";
    }

    private void OnDisable()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);
        SetActiveRefreshButtons(false);

        Time.timeScale = 1f;
        numlist = null;
        choosedIndex = -1;
        isStartTouch = false;
        towerImageIsDraging = false;
        isFirstInstall = false;
    }

    private void Update()
    {
        if (towerInfoUI.gameObject.activeSelf) return;

        OnTouchStateCheck();
        OnTouchMakeDrageImage();
    }

    private void SetActiveRefreshButtons(bool active)
    {
        foreach (var refreshButton in refreshButtons)
            refreshButton.gameObject.SetActive(active);
    }

    private void SettingUpgradeCards()
    {
        ResetChoose();
        installControl.IsReadyInstall = false;

        numlist = new List<int>();
        for (int i = 0; i < uiTexts.Length; i++)
        {
            int number;
            
            while (true)
            {
                number = Random.Range(0, installControl.TowerCount);

                if (installControl.MaxTowerCount == installControl.CurrentTowerCount && 
                    !installControl.IsUsedSlot(number))
                {
                    continue;
                }

                if(!numlist.Contains(number))
                    break;
            }

            numlist.Add(number);

            if (installControl == null) continue;
            //UsedSlot ? UpgradeCard : New Tower-------------------
            if (!installControl.IsUsedSlot(number))
            {
                SetUpCard(i, number);
            }
            else
            {
                choices[i].InstallType = TowerInstallType.Attack;
                choices[i].ability = abilities[i];
                choices[i].AmplifierTowerData = null;
                choices[i].BuffSlotIndex = null;
                uiTexts[i].text = $"Upgrade\n{number}";
            }
        }
    }

    private void SetUpCard(int i, int slotNumber)
    {
        //Random Tower Type (0:Attack, 1:DamageMatrix, 2:ProjectileCore)
        int towerType = Random.Range(0, 3);

        if(isFirstInstall) towerType = 0;

        var ability = AbilityManager.GetAbility(abilities[i]);

        if (towerType == 0) //Attack
        {
            choices[i].InstallType = TowerInstallType.Attack;
            choices[i].ability = abilities[i];
            choices[i].AmplifierTowerData = null;
            choices[i].BuffSlotIndex = null;
            uiTexts[i].text = $"new\nAttack\nTower\n\n{ability}";
        }
        else if (towerType == 1) //Damage Matrix
        {
            choices[i].InstallType = TowerInstallType.Amplifier;
            choices[i].ability = -1;
            choices[i].AmplifierTowerData = damageMatrixCoreSO;

            int buffCount = damageMatrixCoreSO != null
                ? Mathf.Max(1, damageMatrixCoreSO.FixedBuffedSlotCount) : 1;

            int[] buffSlots = GetRandomBuffSlot(buffCount);
            choices[i].BuffSlotIndex = buffSlots;

            string slotText = (buffSlots != null && buffSlots.Length > 0)
                ? string.Join(",", buffSlots) : "-";

            uiTexts[i].text = $"new\nDamage\nMatrix\n\nbuff slots: {slotText}";

        }
        else //Projectile Core
        {
            choices[i].InstallType = TowerInstallType.Amplifier;
            choices[i].ability = -1;
            choices[i].AmplifierTowerData = proejctileCoreSO;
            choices[i].BuffSlotIndex = null;
            uiTexts[i].text = "new\nProjectile\nCore\n\nright slot buff";
        }
    }
    private void ResetUpgradeCard(int index)
    {
        abilities[index] = AbilityManager.GetRandomAbility();
        installControl.IsReadyInstall = false;
        upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;

        int number;
        while (true)
        {
            number = Random.Range(0, installControl.TowerCount);

            if (installControl.MaxTowerCount == installControl.CurrentTowerCount && 
                !installControl.IsUsedSlot(number))
            {
                continue;
            }

            if(!numlist.Contains(number))
                break;
        }

        numlist[index] = number;

        if (installControl == null) return;

        if (!installControl.IsUsedSlot(number))
        {

            SetUpCard(index, number);
        }
        else
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].ability = abilities[index];
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            uiTexts[index].text = $"Upgrade\n{number}";
        }
    }

    public void OnClickRefreshButton(int index)
    {
        ResetUpgradeCard(index);
        if(refreshButtons == null) return;

        refreshButtons[index].interactable = false;
    }

    public void OnClickUpgradeUIClicked(int index)
    {
        var currentColor = upgradeUIs[index].GetComponentInChildren<Image>().color;
        if (currentColor != Color.white)
        {
            installControl.IsReadyInstall = false;
            upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;
            return;
        }

        choosedColor = towerColor;
        upgradeUIs[index].GetComponentInChildren<Image>().color = choosedColor;
        upgradeUIs[(index + 1) % 3].GetComponentInChildren<Image>().color = Color.white;
        upgradeUIs[(index + 2) % 3].GetComponentInChildren<Image>().color = Color.white;
        installControl.IsReadyInstall = true;
        installControl.ChoosedData = choices[index];

        if (installControl.IsUsedSlot(numlist[index]))
        {
            installControl.UpgradeTower(numlist[index]);
            towerInfoUI.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
    
    private void ResetChoose()
    {
        abilities = new int[upgradeUIs.Length];
        choices = new TowerInstallChoice[upgradeUIs.Length];

        for(int i = 0; i < upgradeUIs.Length; i++)
        {
            upgradeUIs[i].GetComponentInChildren<Image>().color = Color.white;
            abilities[i] = AbilityManager.GetRandomAbility();
            choices[i] = new TowerInstallChoice();
            choices[i].BuffSlotIndex = null;
        }
    }

    public void OnTouchMakeDrageImage()
    {
        var touchPos = TouchManager.Instance.TouchPos;

        if(!TouchManager.Instance.IsTouching || towerImageIsDraging)
            return;
        
        if(!isStartTouch)
        {
            isStartTouch = true;
            initTouchPos = touchPos;
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

        if (choosedIndex == -1 || 
            (numlist != null && installControl.IsUsedSlot(numlist[choosedIndex])))
            return;

        dragImage = Instantiate(dragImagePrefab, upgradeUIs[choosedIndex].transform);
        towerImageIsDraging = true;
        dragImage.SetActive(true);
        dragImage.transform.position = touchPos;
    }

    public void OnTouchStateCheck()
    {
        var currentPhase = TouchManager.Instance.TouchPhase;
        
        if (currentPhase == InputActionPhase.Canceled)
        {
            isStartTouch = false;
            towerImageIsDraging = false;
            isNewTouch = true;

            var index = GetEndTouchOnInstallArea();
            if (index != -1 && dragImage != null && choosedIndex != -1)
            {
                installControl.IsReadyInstall = true;
                installControl.ChoosedData = choices[choosedIndex];
                installControl.IntallNewTower(index);
                gameObject.SetActive(false);
            }

            Destroy(dragImage);
            dragImage = null;

            choosedIndex = -1;
        }
    }

    private int GetEndTouchOnInstallArea()
    {
        var touchPos = TouchManager.Instance.TouchPos; 
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

    private int[] GetRandomBuffSlot(int count) //Seperate Pick Random Slots Logic Method
    {
        int towerCount = installControl.TowerCount;
        if (towerCount <=0 || count <= 0) return new int[0];

        count = Mathf.Clamp(count, 1, towerCount);

        List<int> available = new List<int>(towerCount);
        for(int i=0; i<towerCount; i++)
        {
            available.Add(i);
        }

        int[] results = new int[count];

        for(int n=0; n<count; n++)
        {
            int randIndex = UnityEngine.Random.Range(0, available.Count);
            results[n] = available[randIndex];
            available.RemoveAt(randIndex);
        }
        return results;
    }
}