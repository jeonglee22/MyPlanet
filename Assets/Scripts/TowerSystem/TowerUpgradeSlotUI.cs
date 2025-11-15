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

    //test
    private Color towerColor;
    private List<int> numlist;
    [SerializeField] private TextMeshProUGUI[] uiTexts;
    private IAbility[] abilities;
    public Color choosedColor { get; private set; }

    private bool isFirstEnable = true;

    private void Start()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);
        towerColor = Color.yellow;
    }

    private void OnEnable()
    {
        if(isFirstEnable)
        {
            isFirstEnable = false;
            return;
        }

        foreach (var ui in upgradeUIs)
            ui.SetActive(true);

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
                uiTexts[i].text = $"Upgrade\n{number}";
            }
            //
        }
    }

    private void OnDisable()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);

        Time.timeScale = 1f;
        isStartTouch = false;
        towerImageIsDraging = false;
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
    private GameObject dragImage = null;
    private int choosedIndex = -1;

    public void OnTouchMakeDrageImage(InputAction.CallbackContext context)
    {
        if(!context.performed || towerImageIsDraging)
            return;

        var touchPos = context.ReadValue<Vector2>();
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
            if(index != -1 && dragImage != null && choosedIndex != -1)
            {
                installControl.IsReadyInstall = true;
                installControl.ChoosedData = (abilities[choosedIndex], uiTexts[choosedIndex].text);
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