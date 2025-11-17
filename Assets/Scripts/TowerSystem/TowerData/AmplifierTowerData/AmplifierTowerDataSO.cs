using UnityEngine;

public enum AmplifierType
{
    DamageMatrix,   
    ProjectileCore, 
}

public enum AmplifierTargetMode
{
    RandomSlots,   
    LeftNeighbor,   
}

[CreateAssetMenu(fileName = "AmplifierTowerDataSO", menuName = "Scriptable Objects/AmplifierTowerDataSO")]
public class AmplifierTowerDataSO : ScriptableObject
{
    [SerializeField] private string amplifierId;        
    [SerializeField] private AmplifierType amplifierType;

    [SerializeField] private AmplifierTargetMode targetMode;
    [SerializeField] private int buffedSlotCount = 1;      
    [SerializeField] private bool onlyAttackTower = true;

    [SerializeField] private float damageBuff = 1f;  
    [SerializeField] private float fireRateBuff = 1f; 
    [SerializeField] private float accelerationBuff = 0f; 
    [SerializeField] private float hitRadiusBuff = 0f; 
    [SerializeField] private float percentPenetrationBuff = 1f;
    [SerializeField] private float fixedPenetrationBuff = 0f;
    [SerializeField] private int projectileCountBuff = 0;
    [SerializeField] private int targetNumberBuff = 0;
    [SerializeField] private float hitRateBuff = 1f;

    public string AmplifierId => amplifierId;
    public AmplifierType AmplifierType => amplifierType;
    public AmplifierTargetMode TargetMode => targetMode;
    public int AffectedSlotCount => buffedSlotCount;
    public bool AffectInterceptorOnly => onlyAttackTower;
    public float DamageBuff => damageBuff;
    public float FireRateBuff => fireRateBuff;
    public float AccelerationBuff => accelerationBuff;
    public float HitRadiusBuff => hitRadiusBuff;
    public float PercentPenetrationBuff => percentPenetrationBuff;
    public float FixedPenetrationBuff => fixedPenetrationBuff;
    public int ProjectileCountBuff => projectileCountBuff;
    public int TargetNumberBuff => targetNumberBuff;
    public float HitRateBuff => hitRateBuff;
}