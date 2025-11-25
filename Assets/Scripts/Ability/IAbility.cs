using UnityEngine;

public interface IAbility
{
    public AbilityApplyType AbilityType { get; }
    public float UpgradeAmount { get; }
    public void Setting(GameObject gameObject);
    public void ApplyAbility(GameObject gameObject);
    public void RemoveAbility(GameObject gameObject);
    public void StackAbility(float amount);
    public IAbility Copy();
}
