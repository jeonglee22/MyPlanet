using UnityEngine;

public class QuasarUIManager : MonoBehaviour
{
    public static QuasarUIManager Instance { get; private set; }
    public RectTransform quasarUIRect;

    void Awake()
    {
        Instance = this;
    }
}
