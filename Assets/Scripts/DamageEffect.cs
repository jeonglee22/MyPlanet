using TMPro;
using UnityEngine;

public class DamageEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;


    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += new Vector3(0, 0.1f, 0);
    }
}
