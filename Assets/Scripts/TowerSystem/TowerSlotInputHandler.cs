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
        if (!isPointerDown) return;

        float heldTime = Time.unscaledTime - pointerDownTime;
        float movedDist = Vector2.Distance(pointerDownPos, eventData.position);

        if (!isLongPressTriggered &&
            heldTime < longPressThreshold &&
            movedDist < dragStartDistance)
        {
            installControl?.OnSlotClick(slotIndex);
        }
        else if (isLongPressTriggered)
        {
            installControl?.OnSlotLongPressEnd(slotIndex, eventData.position);
        }
        isPointerDown = false;
        isLongPressTriggered = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPointerDown) return;

        float heldTime = Time.unscaledTime - pointerDownTime;
        float movedDist = Vector2.Distance(pointerDownPos, eventData.position);

        if (!isLongPressTriggered && heldTime >= longPressThreshold)
        {
            isLongPressTriggered = true;
            installControl?.OnSlotLongPressStart(slotIndex, pointerDownPos);
        }

        if (isLongPressTriggered)
        {
            installControl?.OnSlotLongPressDrag(slotIndex, eventData.position);
        }
    }
}