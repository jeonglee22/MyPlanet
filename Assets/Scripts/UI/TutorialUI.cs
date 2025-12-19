using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialUI : PopUpUI
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    private bool wasTouchingLastFrame = false;

    private Button panelBtn;

    public void SetText(string text)
    {
        tutorialText.text = text;

        if(panelBtn == null)
        {
            panelBtn = GetComponent<Button>();
        }
        panelBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            TutorialManager.Instance.OnTextUIDisabled();
        });
    }

    protected override void Update()
    {
        if (TouchManager.Instance.IsTouching && !wasTouchingLastFrame)
        {
            touchPos = TouchManager.Instance.TouchPos;

            if(!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPos))
            {
                gameObject.SetActive(false);
                TutorialManager.Instance.OnTextUIDisabled();
            }
        }

        wasTouchingLastFrame = TouchManager.Instance.IsTouching;
    }
}
