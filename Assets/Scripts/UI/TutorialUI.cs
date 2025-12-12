using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialUI : PopUpUI
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    private bool wasTouchingLastFrame = false;

    public void SetText(string text)
    {
        tutorialText.text = text;
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
