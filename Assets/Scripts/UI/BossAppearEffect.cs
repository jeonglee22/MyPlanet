using UnityEngine;

public class BossAppearEffect : MonoBehaviour
{
    [SerializeField] private RectTransform bossAppearEffectRect;

    private float effectMoveSpeed = 200f;

    private float effectTime = 2f;
    private float currentTime = 0f;

    private float appearTime = 5f;

    private float aboveYPos = 50f;
    private float belowYPos = -50f;
    private bool isMovingDown = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= appearTime)
        {
            gameObject.SetActive(false);
            return;
        }

        if (effectMoveSpeed == 0f)
            return;

        if (isMovingDown)
        {
            bossAppearEffectRect.anchoredPosition += Vector2.down * effectMoveSpeed * Time.deltaTime;
            if (bossAppearEffectRect.anchoredPosition.y <= belowYPos)
            {
                bossAppearEffectRect.anchoredPosition = new Vector2(bossAppearEffectRect.anchoredPosition.x, belowYPos);
                isMovingDown = false;
            }
        }
        else
        {
            bossAppearEffectRect.anchoredPosition += Vector2.up * effectMoveSpeed * Time.deltaTime;
            if (bossAppearEffectRect.anchoredPosition.y >= aboveYPos)
            {
                bossAppearEffectRect.anchoredPosition = new Vector2(bossAppearEffectRect.anchoredPosition.x, aboveYPos);
                isMovingDown = true;
            }
        }

        if (currentTime >= effectTime)
        {
            effectMoveSpeed = 0f;
        }
    }
}
