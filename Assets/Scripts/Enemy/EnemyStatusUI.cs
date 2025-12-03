using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusUI : MonoBehaviour
{
    private Enemy enemy;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider hpSlider;

    private Transform cameraTransform;
    private BattleUI battleUI;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        enemy.HpDecreseEvent += HpValueChanged;

        cameraTransform = Camera.main.transform;

        hpSlider.gameObject.SetActive(false);

        if(enemy.EnemyType == 4)
        {
            battleUI = GameObject.FindGameObjectWithTag(TagName.BattleUI).GetComponent<BattleUI>();        
        }
    }

    private void LateUpdate()
    {
        if(enemy.EnemyType == 4 || !hpSlider.gameObject.activeSelf)
        {
            return;
        }

        canvas.transform.rotation = Quaternion.identity;
    }

    private void OnEnable()
    {
        hpSlider.value = 1f;

        if(enemy != null && enemy.EnemyType == 4 && battleUI != null)
        {
            battleUI.SetBossHp(enemy.Data.EnemyTextName, enemy.Health, enemy.MaxHealth);
        }
    }

    private void OnDisable()
    {
        hpSlider.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        enemy.HpDecreseEvent -= HpValueChanged;
    }

    private void HpValueChanged(float hp)
    {
        if(enemy.EnemyType == 4)
        {
            battleUI.SetBossHp(enemy.Data.EnemyTextName, enemy.Health, enemy.MaxHealth);
        }
        else
        {
            hpSlider.gameObject.SetActive(true);
            hpSlider.value = hp / enemy.MaxHealth;
        }
    }
}
