using UnityEngine;

[CreateAssetMenu(fileName = "AmplifierTowerDataSO", menuName = "Scriptable Objects/AmplifierTowerDataSO")]
public class AmplifierTowerDataSO : ScriptableObject
{
    public float damageBuff;
    public float fireRateBuff; //attack speed
    public float accelerationBuff;
    public float hitRadiusBuff; //hitbox Size
    public float percentPenetrationBuff;
    public float fixedPenetrationBuff;
    public int projectileCountBuff; //new variable
    public int targetNumberBuff;
    public float hitRateBuff;
}