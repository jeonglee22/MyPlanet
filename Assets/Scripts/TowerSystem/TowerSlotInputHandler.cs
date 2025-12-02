using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSlotInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private TowerInstallControl installControl;
    private int slotIndex;

    [Header("Input Settings")]
    [SerializeField] private float longPressThreshold = 0.3f;
    [SerializeField] private float dragStartDistance = 10f;

    private bool isPointerDown = false;
    private bool isLongPressTriggered = false;
    private float pointerDownTime;
    private Vector2 pointerDownPos;

    public void Initialize(TowerInstallControl control, int index)
    {
        installControl = control;
        slotIndex = index;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        isLongPressTriggered = false;
        pointerDownTime = Time.unscaledTime;
        pointerDownPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown)
            return;

        float heldTime = Time.unscaledTime - pointerDownTime;

        if (!isLongPressTriggered)
        {
            if (heldTime < longPressThreshold)
                installControl?.OnSlotClick(slotIndex);
            else
                installControl?.OnSlotLongPressStart(slotIndex, pointerDownPos);
        }
        isPointerDown = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPointerDown || isLongPressTriggered)
            return;

        float heldTime = Time.unscaledTime - pointerDownTime;
        float movedDist = Vector2.Distance(pointerDownPos, eventData.position);

        if (heldTime >= longPressThreshold)
        {
            isLongPressTriggered = true;
            installControl?.OnSlotLongPressStart(slotIndex, pointerDownPos);
        }
    }
}