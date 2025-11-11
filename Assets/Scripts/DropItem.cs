using System.Linq;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    protected Planet planet;
    protected float movingSpeed = 10f;
    private float movingTimeTotal;
    private float movingTime = 0f;
    private Vector3 initPos;
    protected bool isDestroy;

    [SerializeField] private ParticleSystem particle;
    

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void Start()
    {
        planet = GameObject.FindWithTag(TagName.Planet)?.GetComponent<Planet>();
        initPos = transform.position;
        movingTimeTotal = Vector3.Distance(initPos, planet.transform.position) / movingSpeed;

        // particle?.Play();
    }
    
    public virtual void Initialize()
    {
        
    }

    protected virtual void Update()
    {
        if(planet != null)
            AutoCollect();
    }
    
    protected virtual void AutoCollect()
    {
        movingTime += Time.deltaTime;
        var nextPos = Vector3.Lerp(initPos, planet.transform.position, movingTime / movingTimeTotal);
        transform.position = nextPos;
    }

    protected void OnCollisionEnter(Collision other)
    {
        var obj = other.gameObject;
        if (other.collider.CompareTag(TagName.Planet))
        {
            isDestroy = true;
        }
    }
}
