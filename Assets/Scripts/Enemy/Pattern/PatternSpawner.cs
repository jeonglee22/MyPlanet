using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PatternSpawner : MonoBehaviour
{
    public static PatternSpawner Instance { get; private set; }

    private ObjectPoolManager<int, PatternProjectile> objectPoolManager = new ObjectPoolManager<int, PatternProjectile>();

    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 500;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public PatternProjectile SpawnPattern(int skillId, string visualAssetName, Vector3 position, Vector3 direction, float damage, float speed, float lifeTime, Enemy owner)
    {
        if(!objectPoolManager.HasPool(skillId))
        {
            CreatePoolFromLoadedPrefab(skillId, visualAssetName);
        }

        PatternProjectile pattern = objectPoolManager.Get(skillId);
        if(pattern == null)
        {
            return null;
        }

        pattern.transform.position = position;
        pattern.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        pattern.Initialize(skillId, damage, speed, lifeTime, direction, this, owner);
        pattern.gameObject.SetActive(true);

        return pattern;
    }

    public PatternProjectile SpawnAccelPattern(int skillId, string visualAssetName, Vector3 position, Vector3 initialVelocity, Vector3 acceleration, float damage, float lifeTime)
    {
        if(!objectPoolManager.HasPool(skillId))
        {
            CreatePoolFromLoadedPrefab(skillId, visualAssetName);
        }

        PatternProjectile pattern = objectPoolManager.Get(skillId);
        if(pattern == null)
        {
            return null;
        }

        pattern.transform.position = position;

        if(initialVelocity != Vector3.zero)
        {
            pattern.transform.rotation = Quaternion.LookRotation(initialVelocity, Vector3.up);
        }

        pattern.Initialize(skillId, damage, initialVelocity, acceleration, lifeTime, this);

        return pattern;
    }

    private void CreatePoolFromLoadedPrefab(int skillId, string visualAssetName)
    {
        GameObject loadedPrefab = LoadManager.GetLoadedGamePrefabOriginal(visualAssetName);
        if(loadedPrefab == null)
        {
            return;
        }

        objectPoolManager.CreatePool
        (
            skillId,
            loadedPrefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck,
            transform
        );
    }

    public void ReturnPatternToPool(PatternProjectile pattern)
    {
        if(pattern == null)
        {
            return;
        }

        objectPoolManager.Return(pattern.SkillId, pattern);
    }
}
