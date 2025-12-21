using System.Collections.Generic;
using UnityEngine;

public class ConstellationEnemy : Enemy
{
    [SerializeField] private Material lineMaterial;

    private float lineWidth = 0.05f;
    private float horizontalSpacing = 2.5f;
    private float verticalRandomRange = 3f;

    private List<GameObject> starObjects = new List<GameObject>();
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    private float baseHealth;
    private float accumulatedDamage = 0f;
    private int currentStarCount;

    public override void Initialize(EnemyTableData enemyData, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, int spawnPointIndex)
    {
        CollectStartObjects();

        ArrangeStars();

        CreateLineRenderers();

        baseHealth = enemyData.Hp * scaleData.HpScale;

        ScaleData modifiedScaleData = scaleData;
        modifiedScaleData.HpScale = scaleData.HpScale * starObjects.Count;

        base.Initialize(enemyData, enemyId, poolManager, modifiedScaleData, spawnPointIndex);

        currentStarCount = starObjects.Count;
        accumulatedDamage = 0f;
    }

    private void CollectStartObjects()
    {
        starObjects.Clear();

        foreach(Transform child in transform)
        {
            if(child.gameObject.CompareTag(TagName.Enemy))
            {
                starObjects.Add(child.gameObject);
            }
        }
    }

    private void ArrangeStars()
    {
        if(starObjects.Count == 0)
        {
            return;
        }

        float starX = -(horizontalSpacing * (starObjects.Count - 1)) / 2f;

        for(int i = 0; i < starObjects.Count; i++)
        {
            float x = starX + (i * horizontalSpacing);
            float y = Random.Range(-verticalRandomRange, verticalRandomRange);

            starObjects[i].transform.localPosition = new Vector3(0f, y, x);
        }
    }

    private void CreateLineRenderers()
    {
        if(lineRenderers.Count > 0)
        {
            UpdateLinePositions();
            return;
        }

        for(int i = 0; i < starObjects.Count - 1; i++)
        {
            GameObject lineObj = new GameObject($"Line_{i}_{i + 1}");
            lineObj.transform.SetParent(transform);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();

            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.useWorldSpace = true;

            if (lineMaterial != null)
            {
                line.material = lineMaterial;
            }
            else
            {
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = Color.white;
                line.endColor = Color.white;
            }

            lineRenderers.Add(line);
        }

        UpdateLinePositions();
    }

    private void LateUpdate()
    {
        UpdateLinePositions();

        
    }

    private void UpdateLinePositions()
    {
        for(int i = 0; i < lineRenderers.Count; i++)
        {
            if(lineRenderers[i] != null && lineRenderers[i].gameObject.activeSelf)
            {
                if(i < starObjects.Count - 1 && starObjects[i].activeSelf && starObjects[i + 1].activeSelf)
                {
                    Vector3 start = starObjects[i].transform.position;
                    Vector3 end = starObjects[i + 1].transform.position;

                    lineRenderers[i].SetPosition(0, start);
                    lineRenderers[i].SetPosition(1, end);
                }
            }
        }
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);

        accumulatedDamage += damage;

        int expectedStarCount = starObjects.Count - (int)(accumulatedDamage / baseHealth);

        while(currentStarCount > expectedStarCount && currentStarCount > 0)
        {
            RemoveLastStar();
        }
    }

    private void RemoveLastStar()
    {
        if(currentStarCount <= 0)
        {
            return;
        }

        int indexRemove = currentStarCount - 1;

        if(indexRemove < starObjects.Count)
        {
            starObjects[indexRemove].SetActive(false);
        }

        if(indexRemove > 0 && indexRemove - 1 < lineRenderers.Count)
        {
            lineRenderers[indexRemove - 1].gameObject.SetActive(false);
        }

        currentStarCount--;

        if(currentStarCount == 0)
        {
            Die();
        }
    }

    public override List<Vector3> GetShootPositions()
    {
        List<Vector3> shootPositions = new List<Vector3>();

        foreach(var star in starObjects)
        {
            if(star != null && star.activeSelf)
            {
                shootPositions.Add(star.transform.position);
            }
        }

        return shootPositions;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        var parentCollider = GetComponent<SphereCollider>();
        if(parentCollider != null)
        {
            parentCollider.enabled = true;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        accumulatedDamage = 0f;
        currentStarCount = starObjects.Count;

        foreach(var star in starObjects)
        {
            if(star != null)
            {
                star.SetActive(true);
            }
        }

        foreach(var line in lineRenderers)
        {
            if(line != null)
            {
                line.gameObject.SetActive(true);
            }
        }

        var parentCollider = GetComponent<SphereCollider>();
        if(parentCollider != null)
        {
            parentCollider.enabled = true;
        }
    }

    public override void OnLifeTimeOver()
    {
        accumulatedDamage = 0f;
        currentStarCount = starObjects.Count;

        foreach(var star in starObjects)
        {
            if(star != null)
            {
                star.SetActive(true);
            }
        }

        foreach(var line in lineRenderers)
        {
            if(line != null)
            {
                line.gameObject.SetActive(true);
            }
        }

        var parentCollider = GetComponent<SphereCollider>();
        if(parentCollider != null)
        {
            parentCollider.enabled = true;
        }

        base.OnLifeTimeOver();
    }

    public void DisableCollider()
    {
        var parentCollider = GetComponent<SphereCollider>();
        if(parentCollider != null)
        {
            parentCollider.enabled = false;
        }
    }
}
