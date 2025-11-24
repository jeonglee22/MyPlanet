using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainTitleUI : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStartButton.onClick.AddListener(() => OnStartGameButtonClicked().Forget());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async UniTaskVoid OnStartGameButtonClicked()
    {
        Debug.Log("Game Start Button Clicked");
        await SceneControlManager.Instance.LoadScene("LoginScene");
    }
}
