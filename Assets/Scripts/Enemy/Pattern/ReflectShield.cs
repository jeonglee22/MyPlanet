using UnityEngine;

public class ReflectShield : MonoBehaviour
{
    private Enemy owner;

    public void Initialize(Enemy enemy)
    {
        owner = enemy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(TagName.Projectile))
        {
            return;
        }

        Projectile projectile = other.GetComponent<Projectile>();
        if(projectile == null)
        {
            return;
        }

        Vector3 incomDirection = projectile.transform.forward;
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        Vector3 normal = (contactPoint - transform.position).normalized;

        Vector3 reflectDirection = Vector3.Reflect(incomDirection, normal);

        projectile.direction = reflectDirection;
        projectile.transform.forward = reflectDirection;
    }
}
