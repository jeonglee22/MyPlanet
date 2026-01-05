using UnityEngine;

public class QuasarItem : DropItem
{
    private bool isStartMove = false;

    private float waitTimeInterval = 0.5f;
    private float waitTime = 0f;
    [SerializeField] private float uiOffsetY = 10f;

    private Vector3 uiTopPosition;
    private Vector3 uiCenterPosition;
    private Vector3 initPos;

    private float totalDuration = 3f;
    private float moveTime = 0f;
    private bool isMovingToUI = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        Initialize();
    }
    public override void Initialize()
    {
        base.Initialize();
        moveTime = 0f;
        waitTime = 0f;
        isMovingToUI = false;
        initPos = transform.position;

        if (QuasarUIManager.Instance != null)
        {
            RectTransform quasarUIRectTransform = QuasarUIManager.Instance.quasarUIRect;
            Vector3 screenPos = quasarUIRectTransform.position;
            Camera mainCam = Camera.main;

            if (mainCam != null)
            {
                uiCenterPosition = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCam.nearClipPlane + 10f));
                uiTopPosition = uiCenterPosition + Vector3.up * (uiOffsetY * 0.01f); 
            }
        }
    }

    protected override void Start()
    {
    }

    protected override void Update()
    {
        if (!isStartMove)
        {
            Waiting();
            return;
        }

        if (isMovingToUI)
        {
            MoveToUI();
        }
    }

    private void MoveToUI()
    {
        if (QuasarUIManager.Instance == null || QuasarUIManager.Instance.quasarUIRect == null)
        {
            Destroy(gameObject);
            return;
        }

        moveTime += Time.deltaTime;
        float progress = moveTime / totalDuration;
        if (progress <= 0.5f)
        {
            float phase1Progress = progress * 2f;
            transform.position = Vector3.Lerp(initPos, uiTopPosition, phase1Progress);
        }
        else if (progress <= 1f)
        {
            float phase2Progress = (progress - 0.5f) * 2f;
            transform.position = Vector3.Lerp(uiTopPosition, uiCenterPosition, phase2Progress);
        }
        else
        {
            Destroy(gameObject);
            Variables.Quasar++;
        }
    }

    private void Waiting()
    {
        if (waitTime < waitTimeInterval)
        {
            waitTime += Time.deltaTime;
            return;
        }
        else
        {
            isStartMove = true;
            isMovingToUI = true;
        }
    }
}