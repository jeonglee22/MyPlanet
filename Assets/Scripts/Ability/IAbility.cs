using UnityEngine;

public interface IAbility
{
    public void Setting(GameObject gameObject);
    public void ApplyAbility(GameObject gameObject);
    public void RemoveAbility(GameObject gameObject);
}
