using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusUI : MonoBehaviour
{
    private Enemy enemy;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider hpSlider;

    private Transform cameraTransform;
    private BattleUI battleUI;

    private float radius;
    private Vector3 canvasOffset;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        enemy.HpDecreseEvent += HpValueChanged;
        enemy.DamageEvent += MakeDamagePopup;

        cameraTransform = Camera.main.transform;

        hpSlider?.gameObject.SetActive(false);

        if(enemy.EnemyType == 4)
        {
            battleUI = GameObject.FindGameObjectWithTag(TagName.BattleUI).GetComponent<BattleUI>();        
        }

        ConstellationEnemy constellationEnemy = enemy as ConstellationEnemy;
        if(constellationEnemy != null)
        {
            radius = 1f;
        }
        else
        {
            var collider = enemy.GetComponent<SphereCollider>();
            if(collider != null)
            {
                radius = collider.radius * enemy.transform.localScale.y;
            }
            else
            {
                radius = 0.5f;
            }
        }

        canvasOffset = new Vector3(0f, -(radius + 0.1f), 0f);
    }

    private void LateUpdate()
    {
        if(enemy.EnemyType == 4 || !hpSlider.gameObject.activeSelf)
        {
            return;
        }

        canvas.transform.position = enemy.transform.position + canvasOffset;
        canvas.transform.rotation = Quaternion.identity;
        
    }

    private void OnEnable()
    {
        hpSlider.value = 1f;

        if(enemy != null && enemy.EnemyType == 4 && battleUI != null)
        {
            battleUI?.SetBossHp(enemy.Data.EnemyTextName, enemy.Health, enemy.MaxHealth);
        }

        enemy = GetComponentInParent<Enemy>();
        enemy.HpDecreseEvent += HpValueChanged;
        enemy.DamageEvent += MakeDamagePopup;
    }

    private void OnDisable()
    {
        hpSlider.gameObject.SetActive(false);

        if(enemy != null)
        {
            enemy.HpDecreseEvent -= HpValueChanged;
            enemy.DamageEvent -= MakeDamagePopup;
        }
    }

    private void OnDestroy()
    {
        
    }

    private void HpValueChanged(float hp)
    {
        if(enemy.EnemyType == 4)
        {
            battleUI?.SetBossHp(enemy.Data.EnemyTextName, enemy.Health, enemy.MaxHealth);
        }
        else
        {
            hpSlider.gameObject.SetActive(true);
            hpSlider.value = hp / enemy.MaxHealth;
        }
    }

    private void MakeDamagePopup(float damage)
    {
        var popup = SpawnManager.Instance.GetDamagePopup();

        popup.transform.rotation = Quaternion.identity;
        popup.transform.position = enemy.transform.position - canvasOffset;

        var popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
        popupText.text = Mathf.RoundToInt(damage).ToString();
    }
}
