using UnityEngine;

public class JoyStickDisable : MonoBehaviour
{
    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0)
        {
            gameObject.SetActive(false);
        }
    }
}
