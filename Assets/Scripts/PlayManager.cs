using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private Planet planet;


    [SerializeField] private GameObject gameOverUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planet.OnDeathEvent += GameOver;
        gameOverUI.SetActive(false);
    }

    void OnDestroy()
    {
        planet.OnDeathEvent -= GameOver;
    }
    
    private void GameOver()
    {
        gameOverUI.SetActive(true);
    }
}
