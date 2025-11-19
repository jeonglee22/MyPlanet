using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
    [SerializeField] private Button[] refreshButtons;

    private void Start()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);
        towerColor = Color.yellow;

        SetActiveRefreshButtons(false);
        installControl.OnTowerInstalled += SetTowerInstallText;
    }

    void OnDestroy()
    {
        installControl.OnTowerInstalled -= SetTowerInstallText;
    }

    private void OnEnable()
    {
        if(isNotUpgradeOpen)
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
    }

    private void Update()
    {
        if (towerInfoUI.gameObject.activeSelf)
            return;

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
            int count = 0;
            do
            {
                number = Random.Range(0, installControl.TowerCount);
                count++;
            } while (numlist.Contains(number) && count < installControl.TowerCount);

            numlist.Add(number);

            if (installControl == null)
                continue;

            // test
            if (!installControl.IsUsedSlot(number))
            {
                uiTexts[i].text = $"new Tower\n\n{abilities[i]}";
            }
            else
            {
                uiTexts[i].text = $"Upgrade\n{number}\nTower";
            }
            //
        }
    }

    private void ResetUpgradeCard(int index)
    {
        abilities[index] = AbilityManager.Instance.GetRandomAbility();

        int number;
        int count = 0;
        do
        {
            number = Random.Range(0, installControl.TowerCount);
            count++;
        } while (numlist.Contains(number) && count < installControl.TowerCount);

        numlist[index] = number;

        if (installControl == null)
            return;

        // test
        if (!installControl.IsUsedSlot(number))
        {
            uiTexts[index].text = $"new Tower\n\n{abilities[index]}";
        }
        else
        {
            uiTexts[index].text = $"Upgrade\n{number}\nTower";
        }
        //
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
        installControl.ChoosedData = (abilities[index], uiTexts[index].text);

        if (installControl.IsUsedSlot(numlist[index]))
        {
            installControl.UpgradeTower(numlist[index]);
            towerInfoUI.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
    
    private void ResetChoose()
    {
        abilities = new IAbility[upgradeUIs.Length];
        for(int i = 0; i < upgradeUIs.Length; i++)
        {
            upgradeUIs[i].GetComponentInChildren<Image>().color = Color.white;
            abilities[i] = AbilityManager.Instance.GetRandomAbility();
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
            if(index != -1 && dragImage != null && choosedIndex != -1)
            {
                installControl.IsReadyInstall = true;
                installControl.ChoosedData = (abilities[choosedIndex], uiTexts[choosedIndex].text);
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
}