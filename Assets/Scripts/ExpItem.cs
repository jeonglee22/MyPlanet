using UnityEngine;

public class ExpItem : DropItem
{
    private float exp;

    private bool isStartMove = false;

    private float waitTimeInterval = 0.5f;
    private float waitTime = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        exp = 20f;
    }

    protected override void Update()
    {
        if (!isStartMove)
        {
            Waiting();
            return;
        }

        base.Update();

        if (isDestroy)
        {
            Destroy(gameObject);
            planet.CurrentExp += exp;
        }
    }
    
    private void Waiting()
    {
        if(waitTime < waitTimeInterval)
        {
            waitTime += Time.deltaTime;
            return;
        }
        else
        {
            isStartMove = true;
        }
    }
}
