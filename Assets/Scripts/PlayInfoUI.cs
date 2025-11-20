using TMPro;
using UnityEngine;

public class PlayInfoUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;

    private float fpsTimeInterval = 0.5f;
    private float fpsTimer = 0f;

    void Start()
    {
        Application.targetFrameRate = 60;
        fpsText.color = Color.yellow;
    }

    // Update is called once per frame
    void Update()
    {
        fpsTimer += Time.unscaledDeltaTime;
        if(fpsTimer >= fpsTimeInterval)
        {
            SetFpsText(Time.unscaledDeltaTime);
            fpsTimer = 0f;
        }
    }

    private void SetFpsText(float deltaTime)
    {
        fpsText.text = $"FPS: {Mathf.RoundToInt(1f / deltaTime)}";
    }
}
