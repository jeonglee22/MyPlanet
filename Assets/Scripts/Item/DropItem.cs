using System.Linq;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    protected Planet planet;
    protected Transform centerStone;
    protected float movingSpeed = 10f;
    private float movingTimeTotal;
    private float movingTime = 0f;
    private Vector3 initPos;
    protected bool isDestroy;

    [SerializeField] private ParticleSystem particle;
    

    protected virtual void OnEnable()
    {
        
    }
    //
    protected virtual void Start()
    {
        planet = GameObject.FindWithTag(TagName.Planet)?.GetComponent<Planet>();
        centerStone = GameObject.FindWithTag(TagName.CenterStone)?.GetComponent<Transform>();
        initPos = transform.position;
        if (planet == null)
            return;
            
        movingTimeTotal = Vector3.Distance(initPos, centerStone.position) / movingSpeed;

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
        var nextPos = Vector3.Lerp(initPos, centerStone.position, movingTime / movingTimeTotal);
        transform.position = nextPos;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag(TagName.CenterStone))
        {
            isDestroy = true;
        }
    }
}
