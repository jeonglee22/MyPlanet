using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class AsyncUserPlanet : LivingEntity
{
    [SerializeField] HighestHpPrioritySO highestHpPrioritySO;
    [SerializeField] TargetRangeSO targetRangeSO;
    [SerializeField] TextMeshProUGUI nicknameText;
    [SerializeField] TowerAttack towerAttackObject;
    [SerializeField] private List<TowerDataSO> towerDataSOs;

    private float livingTime;
    private float elapsedTime = 0f;
    private float attack = 0f;
    private float dieDps;
    private float attackDps;
    private float attackTimer = 0f;

    private UserPlanetData planetData;
    private string blurNickname;
    public string BlurNickname => blurNickname;
    private AsyncPlanetData asyncPlanetData;
    private TowerDataSO towerDataSO;
    private float currentDamage;

    private Vector3 reflectPosition;
    public Vector3 ReflectPosition => reflectPosition;
    private float moveSpeed = 2f;
    private Vector3 direction;

    private void Awake()
    {
        // livingTime = Random.Range(5f, 10f);
        livingTime = Random.Range(10f, 15f);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void Update()
    {
        // elapsedTime += Time.deltaTime;
        OnDamage(dieDps * Time.deltaTime);
        
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f)
        {
            // Variables.LastBossEnemy?.OnDamage(attackDps);

            currentDamage += attackDps;
            Debug.Log("AsyncUserPlanet Deal Damage : " + currentDamage);
            attackTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        transform.position += direction * moveSpeed * Time.fixedDeltaTime;
    }

    private void OnDisable()
    {
        Debug.Log("Damage to Boss : " + attack);
    }

    // public void InitializePlanet(UserPlanetData data, float damage, AsyncPlanetData asyncData, TowerDataSO towerData, Vector3 ReflectPosition)
    // {
    //     if (data == null)
    //     {
    //         data = new UserPlanetData("Unknown", 0);
    //     }

    //     Debug.Log(livingTime);

    //     planetData = data;
    //     attack = damage;
    //     attackDps = attack / livingTime;
    //     dieDps = Health / livingTime;
    //     tower = GetComponent<TowerAttack>();
    //     tower.IsOtherUserTower = true;

    //     SetBlurNickname(planetData.nickName);
    //     asyncPlanetData = asyncData;
    //     var towerId = asyncPlanetData.AttackTower_Id;
    //     towerDataSO = ScriptableObject.Instantiate(towerData);
    //     towerDataSO.targetPriority = highestHpPrioritySO;
    //     towerDataSO.rangeData = targetRangeSO;

    //     tower.SetTowerData(towerDataSO);
    //     tower.DamageBuffMul = 0f;

    //     var targetingSystem = tower.GetComponent<TowerTargetingSystem>();
    //     targetingSystem.SetTowerData(towerDataSO);

    //     SetupAbilities();
    //     foreach (var abilityId in abilities)
    //     {
    //         var ability = AbilityManager.GetAbility(abilityId);
    //         tower.AddAbility(abilityId);
    //         ability?.ApplyAbility(tower.gameObject);

    //     }

    //     Debug.Log(ReflectPosition + " / " + transform.position);
    //     direction = (ReflectPosition - transform.position).normalized;
    //     moveSpeed = Vector3.Distance(ReflectPosition, transform.position) / livingTime;

    //     nicknameText.text = blurNickname;
    // }

    public void InitializePlanet(UserPlanetData data, float damage, UserTowerData[] userTowerDatas, Vector3 ReflectPosition)
    {
        if (data == null)
        {
            data = new UserPlanetData("Unknown");
        }

        Debug.Log(livingTime);

        planetData = data;
        // attack = damage;
        attackDps = attack / livingTime;
        dieDps = Health / livingTime;

        var needApplyAbilities = new List<int>{ 200005, 200007, 200008, 200009, 200010, 200011, 200013};

        for (int i =0 ; i < userTowerDatas.Length; i++)
        {
            var userTowerData = userTowerDatas[i];
            var towerDataSo = towerDataSOs.Find(x => x.towerIdInt == userTowerData.towerId);

            var insertTowerDataSO = ScriptableObject.Instantiate(towerDataSo);
            insertTowerDataSO.targetPriority = highestHpPrioritySO;
            insertTowerDataSO.rangeData = targetRangeSO;

            if (userTowerData == null || userTowerData.towerLevelId == -1)
                continue;

            var tower = Instantiate(towerAttackObject, transform);
            tower.gameObject.SetActive(true);
            tower.IsOtherUserTower = true;
            tower.SetTowerData(insertTowerDataSO);
            tower.CurrentProjectileData = userTowerData.BuffedProjectileData;

            var targetingSystem = tower.GetComponent<TowerTargetingSystem>();
            targetingSystem.SetTowerData(insertTowerDataSO);

            var abilities = userTowerData.abilities;

            foreach (var abilityId in abilities)
            {
                if (!needApplyAbilities.Contains(abilityId))
                    continue;

                var ability = AbilityManager.GetAbility(abilityId);
                tower.AddAbility(abilityId);
                ability?.ApplyAbility(tower.gameObject);
                ability?.Setting(tower.gameObject);
            }
        }       
        // asyncPlanetData = asyncData;
        // var towerId = asyncPlanetData.AttackTower_Id;
        // towerDataSO = ScriptableObject.Instantiate(towerData);
        // towerDataSO.targetPriority = highestHpPrioritySO;
        // towerDataSO.rangeData = targetRangeSO;

        SetBlurNickname(planetData.nickName);

        direction = (ReflectPosition - transform.position).normalized;
        moveSpeed = Vector3.Distance(ReflectPosition, transform.position) / livingTime;

        nicknameText.text = blurNickname;
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
