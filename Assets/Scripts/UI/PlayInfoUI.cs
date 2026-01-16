using TMPro;
using UnityEngine;

public class PlayInfoUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;

    private float fpsTimeInterval = 0.5f;
    private float fpsTimer = 0f;

    private float deltaTime = 0.0f;

    void Start()
    {
        Application.targetFrameRate = 60;
        fpsText.color = Color.yellow;
    }

    // Update is called once per frame
    void Update()
    {
        fpsTimer += Time.unscaledDeltaTime;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if(fpsTimer >= fpsTimeInterval)
        {
            SetFpsText(deltaTime);
            fpsTimer = 0f;
        }
    }

    private void SetFpsText(float deltaTime)
    {
        fpsText.SetText("FPS: {0:0}", 1.0f / deltaTime);
    }
}
