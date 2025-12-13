using TMPro;
using UnityEngine;

public class DpsCalculator : MonoBehaviour
{
    private static float damage = 0f;
    private static float dpsTimer = 0f;
    private static float averageDpsTimer = 0f;
    [SerializeField] private TextMeshProUGUI dpsText;
    [SerializeField] private TextMeshProUGUI maxDpsText;
    [SerializeField] private TextMeshProUGUI averageDpsText;

    private static float maxDps = 0f;
    private static float averageDamage;

    // Update is called once per frame
    void FixedUpdate()
    {
        dpsTimer += Time.fixedDeltaTime;
        if (dpsTimer >= 1f)
        {
            dpsText.text = $"{damage / dpsTimer:F2}";
            if (damage / dpsTimer > maxDps)
            {
                maxDps = damage / dpsTimer;
                maxDpsText.text = $"{maxDps:F2}";
            }
            dpsTimer = 0f;
            damage = 0f;
        }
        averageDpsTimer += Time.fixedDeltaTime;
        if (averageDpsTimer >= 10f)
        {
            averageDpsText.text = $"{averageDamage / averageDpsTimer:F2}";
            averageDpsTimer = 0f;

            averageDamage = 0f;
        }
    }

    public static void AddDamage(float dmg)
    {
        damage += dmg;
        averageDamage += dmg;
    }

    public void ResetDps()
    {
        damage = 0f;
        dpsTimer = 0f;
        dpsText.text = $"0";
        maxDps = 0f;
        maxDpsText.text = $"0";
        averageDamage = 0f;
        averageDpsText.text = $"0";
        averageDpsTimer = 0f;
    }
}
