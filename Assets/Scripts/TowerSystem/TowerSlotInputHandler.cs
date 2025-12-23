using System.Collections.Generic;
// using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerSlotInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private TowerInstallControl installControl;
    private int slotIndex;

    [Header("Input Settings")]
    [SerializeField] private float longPressThreshold = 0.3f;
    [SerializeField] private List<Image> upgradeStars;
    [SerializeField] private GameObject upgradeStarPart;

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

        // ① 짧게 탭: 클릭으로 처리
        if (!isLongPressTriggered && heldTime < longPressThreshold)
        {
            installControl?.OnSlotClick(slotIndex);
        }
        // ② 롱프레스 끝: 드롭 처리
        else if (isLongPressTriggered)
        {
            installControl?.OnSlotLongPressEnd(slotIndex, eventData.position);
            installControl.LeftRotateRect.gameObject.SetActive(false);
            installControl.RightRotateRect.gameObject.SetActive(false);
            upgradeStarPart.SetActive(true);
        }

        isPointerDown = false;
        isLongPressTriggered = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPointerDown) return;

        float heldTime = Time.unscaledTime - pointerDownTime;

        // 롱프레스 시작 지점
        if (!isLongPressTriggered && heldTime >= longPressThreshold)
        {
            isLongPressTriggered = true;
            installControl?.OnSlotLongPressStart(slotIndex, pointerDownPos);
            installControl.LeftRotateRect.gameObject.SetActive(true);
            installControl.RightRotateRect.gameObject.SetActive(true);
            upgradeStarPart.SetActive(false);
        }

        // 롱프레스 중 드래그
        if (isLongPressTriggered)
        {
            installControl?.OnSlotLongPressDrag(slotIndex, eventData.position);
        }
    }
}
