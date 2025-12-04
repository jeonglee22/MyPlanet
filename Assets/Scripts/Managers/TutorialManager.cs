using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTextData
{
    public string text;
    public TutorialPoint pointType;

    public TutorialTextData(string text, TutorialPoint pointType)
    {
        this.text = text;
        this.pointType = pointType;
    }
}

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance;
    public static TutorialManager Instance => instance;

    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private RectTransform tutorialRect;
    [SerializeField] private List<TextPoint> textPoints = new List<TextPoint>();

    public bool IsTutorialMode { get; private set; }

    private int currentStep = -1;
    private int currentTextIndex = 0;
    private List<TutorialTextData> currentTexts = new List<TutorialTextData>();

    private Dictionary<int, List<TutorialTextData>> tutorialData = new Dictionary<int, List<TutorialTextData>>();
    private Dictionary<int, bool> completedSteps = new Dictionary<int, bool>();

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        InitializeTutorialData();
    }

    private void Start()
    {
        IsTutorialMode = (Variables.Stage <= 2);
        if(!IsTutorialMode)
        {
            tutorialPanel?.SetActive(false);
            return;
        }

        tutorialPanel?.SetActive(true);
    }

    private void InitializeTutorialData()
    {
        //Stage 1
        tutorialData[1] = new List<TutorialTextData>
        {
          new TutorialTextData("행성은 궤도를 따라 자동으로 공전합니다.", TutorialPoint.CenterMidium),
          new TutorialTextData("화면 터치 시 나타나는 조이스틱으로 행성을 자유롭게 움직일 수 있습니다.", TutorialPoint.CenterMidium),
          new TutorialTextData("요격 타워를 1개 이상 배치했다면 행성이 자동으로 공격합니다.", TutorialPoint.CenterMidium)
        };
        tutorialData[2] = new List<TutorialTextData>
        {
          new TutorialTextData("레벨업시, 증강을1개선택할수있습니다", TutorialPoint.TopMidium),
          new TutorialTextData("각선택지는 1회씩 새로고침할 수 있습니다.", TutorialPoint.CenterMidiumTwo),
          new TutorialTextData("신규 타워 획득: 랜덤한 능력을 가진 새 타워를 획득할 수 있습니다.", TutorialPoint.BottomBig),
          new TutorialTextData("기존 타워 강화: 보유한 타워의 능력치를 강화할 수 있습니다.", TutorialPoint.BottomBig)
        };
        tutorialData[3] = new List<TutorialTextData>
        {
          new TutorialTextData("신규 타워 획득 시, 원하는 슬롯에 드래그해 설치할 수 있습니다.", TutorialPoint.TopMidium)
        };
        tutorialData[4] = new List<TutorialTextData>
        {
          new TutorialTextData("증폭 타워의 경우 특정 슬롯을 강화합니다.", TutorialPoint.BottomBig)
        };
        tutorialData[5] = new List<TutorialTextData>
        {
          new TutorialTextData("보스 등장", TutorialPoint.TopMidium)
        };

        //Stage 2
        tutorialData[6] = new List<TutorialTextData>
        {
          new TutorialTextData("스테이지 시작 시 퀘이사를 1개 보유한 상태로 시작합니다.", TutorialPoint.TopRight),
          new TutorialTextData("스테이지 진행 중 원하는 시점에 아이콘을 터치해 사용할 수 있습니다.", TutorialPoint.TopRightTwo)
        };
        tutorialData[7] = new List<TutorialTextData>
        {
          new TutorialTextData("'퀘이사'는 배치 가능한 타워 수를 1개 늘려주거나,", TutorialPoint.TopBig),
          new TutorialTextData("보유한 타워에 랜덤 능력을 추가할 수 있는 아이템입니다.", TutorialPoint.TopBig)
        };
        tutorialData[8] = new List<TutorialTextData>
        {
          new TutorialTextData("랜덤 능력을 추가할 타워를 선택하세요", TutorialPoint.TopBig),
          new TutorialTextData("추가할 랜덤 능력을 1개 선택하세요", TutorialPoint.CenterMidiumTwo)
        };
        tutorialData[9] = new List<TutorialTextData>
        {
          new TutorialTextData("중간 보스 등장", TutorialPoint.TopLeftSmall)
        };
        tutorialData[10] = new List<TutorialTextData>
        {
          new TutorialTextData("중간 보스를 처치하면 퀘이사를 1개 획득할 수 있습니다.", TutorialPoint.TopRight)
        };
        tutorialData[11] = new List<TutorialTextData>
        {
          new TutorialTextData("최종 보스 등장", TutorialPoint.TopLeftSmall)
        };
        tutorialData[12] = new List<TutorialTextData>
        {
          new TutorialTextData("최종 보스를 처치하면 스테이지를 클리어하고 보상을 획득할 수 있습니다.", TutorialPoint.CenterMidium)
        };

        completedSteps.Clear();
    }

    public void ShowTutorialStep(int step)
    {
        if(Variables.Stage > 2)
        {
            return;
        }

        if((completedSteps.ContainsKey(step) && completedSteps[step]) || !tutorialData.ContainsKey(step))
        {
            return;
        }

        tutorialPanel?.SetActive(true);

        currentStep = step;
        currentTextIndex = 0;
        currentTexts = tutorialData[step];

        ShowCurrentText();

        GamePauseManager.Instance.Pause();
    }

    private void ShowCurrentText()
    {
        if(currentTextIndex < currentTexts.Count)
        {
            TutorialTextData data = currentTexts[currentTextIndex];

            ApplyTextPoint(data.pointType);

            tutorialUI.SetText(data.text);
            tutorialUI.gameObject.SetActive(true);

            Canvas.ForceUpdateCanvases();
        }
        else
        {
            OnStepCompleted();
        }
    }

    public void OnTextUIDisabled()
    {
        currentTextIndex++;
        ShowCurrentText();
    }

    private void OnStepCompleted()
    {
        GamePauseManager.Instance.Resume();

        completedSteps.Add(currentStep, true);
        currentStep = -1;
        currentTextIndex = 0;
        currentTexts.Clear();

        tutorialPanel?.SetActive(false);
    }

    private void ApplyTextPoint(TutorialPoint pointType)
    {
        TextPoint textPoint = textPoints.Find(tp => tp.PointType == pointType);
        if(textPoint == null)
        {
            return;
        }

        RectTransform textRect = textPoint.GetRectTransform();

        tutorialRect.SetParent(textRect.parent, false);

        tutorialRect.anchorMin = textRect.anchorMin;
        tutorialRect.anchorMax = textRect.anchorMax;
        tutorialRect.pivot = textRect.pivot;

        tutorialRect.anchoredPosition = textRect.anchoredPosition;
        tutorialRect.sizeDelta = textRect.sizeDelta;

        LayoutRebuilder.ForceRebuildLayoutImmediate(tutorialRect);
    }
}
