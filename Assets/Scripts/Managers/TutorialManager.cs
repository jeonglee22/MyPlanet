using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance;
    public static TutorialManager Instance => instance;

    public event Action<bool> OnTutorialModeChanged;

    public bool IsTutorialMode { get; private set; }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void Initialize()
    {
        IsTutorialMode = (Variables.Stage == 1 || Variables.Stage == 2);

        OnTutorialModeChanged?.Invoke(IsTutorialMode);
    }
}
