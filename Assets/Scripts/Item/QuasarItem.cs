using UnityEngine;

public class QuasarItem : DropItem
{
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
    }

    protected override void Update()
    {
        if (!isStartMove)
        {
            Waiting();
            return;
        }

        base.Update();

        if (isDestroy && centerStone != null)
        {
            Destroy(gameObject);
            Variables.Quasar++;
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
