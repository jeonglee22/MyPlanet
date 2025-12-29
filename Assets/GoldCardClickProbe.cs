using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class GoldCardClickProbe : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[GoldCardProbe] PointerDown on={gameObject.name} pos={eventData.position}");
        DumpRaycast(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[GoldCardProbe] PointerClick on={gameObject.name} pos={eventData.position}");
        DumpRaycast(eventData);
    }

    private void DumpRaycast(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        var sb = new StringBuilder();
        sb.AppendLine($"[GoldCardProbe] Raycast hits={results.Count}");
        for (int i = 0; i < Mathf.Min(results.Count, 10); i++)
        {
            sb.AppendLine($"  #{i} {results[i].gameObject.name}");
        }
        Debug.Log(sb.ToString());
    }
}
