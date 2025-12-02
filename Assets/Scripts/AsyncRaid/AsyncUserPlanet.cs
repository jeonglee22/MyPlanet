using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AsyncUserPlanet : LivingEntity
{
    private float livingTime;
    private float elapsedTime = 0f;
    private float attack = 0f;
    private float attackDps;

    private TowerAttack tower;
    private List<int> abilities;

    private UserPlanetData planetData;
    private string blurNickname;
    private AsyncPlanetData asyncPlanetData;

    private async void Awake()
    {
        livingTime = Random.Range(5f, 10f);
        // livingTime = Random.Range(20f, 40f);
        abilities = new List<int>();
    }

    private async void Start()
    {
        
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= livingTime)
        {
            Die();
        }

        TestMove();
    }

    private void TestMove()
    {
        transform.Translate(Vector3.down * Time.deltaTime * 2f);
    }

    public void InitializePlanet(UserPlanetData data, float damage)
    {
        if (data == null)
        {
            data = new UserPlanetData("Unknown", 0);
        }

        planetData = data;
        attack = damage;
        attackDps = attack / livingTime;
        tower = GetComponentInChildren<TowerAttack>();
        SetBlurNickname(planetData.nickName);
        asyncPlanetData = DataTableManager.AsyncPlanetTable.GetRandomData();
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);
    }

    public override void Die()
    {
        base.Die();
    }

    private void SetBlurNickname(string nickname)
    {
        if (nickname.Length <= 2)
        {
            blurNickname = $"{nickname[0]}*";
            return;
        }
        if (nickname.Length == 3)
        {
            blurNickname = $"{nickname[0]}*{nickname[2]}";
            return;
        }

        var sb = new StringBuilder();
        sb.Append(nickname.Substring(0, 2));
        for (int i = 2; i < nickname.Length - 1; i++)
        {
            sb.Append("*");
        }
        sb.Append(nickname[nickname.Length - 1]);

        Debug.Log(sb.ToString());
        blurNickname = sb.ToString();
    }
}
