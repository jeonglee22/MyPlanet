using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusUI : MonoBehaviour
{
    private Enemy enemy;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider hpSlider;

    private Transform cameraTransform;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        enemy.HpDecreseEvent += HpValueChanged;

        hpSlider.gameObject.SetActive(false);

        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if(!hpSlider.gameObject.activeSelf)
        {
            return;
        }

        canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - cameraTransform.position);
    }

    private void OnEnable()
    {
        hpSlider.value = 1f;
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
        hpSlider.gameObject.SetActive(true);
        hpSlider.value = hp / enemy.MaxHealth;
    }
}
