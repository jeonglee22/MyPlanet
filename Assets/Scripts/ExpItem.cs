using UnityEngine;

public class ExpItem : DropItem
{
    private float exp;

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
        base.Update();

        if(isDestroy)
        {
            Destroy(gameObject);
            planet.CurrentExp += exp;
        }
    }
}
