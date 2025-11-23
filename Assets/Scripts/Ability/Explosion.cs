using UnityEngine;

public class Explosion : MonoBehaviour
{
    private float explosionRadius = 1f;
    private float initRadius = 0.01f;
    private SphereCollider explosionCollider;

    private float explosionTimeInterval = 0.3f;
    private float explosionTimer = 0f;

    void Awake()
    {
        explosionCollider = GetComponent<SphereCollider>();
    }

    public void SetInitRadius(float initRadius, float explosionRadius)
    {
        this.initRadius = initRadius;
        this.explosionRadius = explosionRadius;
    }

    // Update is called once per frame
    private void Update()
    {
        explosionTimer += Time.deltaTime;
        initRadius += Time.deltaTime * (explosionRadius / explosionTimeInterval);
        explosionCollider.transform.localScale = new Vector3(initRadius, initRadius, initRadius);
        if(explosionTimer >= explosionTimeInterval)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var damagable = other.gameObject.GetComponent<IDamagable>();
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (damagable != null && enemy != null)
        {
            damagable.OnDamage(100f);
        }
    }
}
