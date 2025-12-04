using System.Collections.Generic;
using UnityEngine;

public class RootLazerPattern : LazerPattern
{
    private struct LazerSegment
    {
        public Vector3 Start;
        public Vector3 End;

        public LazerSegment(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }
    }

    private int maxDepth = 10;
    private int minBrances = 1;
    private int maxBranches = 2;
    private float segmentLength = 1.5f;
    private float angleVariance = 60f; //각도분산
    private float downwardBias = 0.6f; //하향 편향

    private List<Lazer> activeLazers = new List<Lazer>();
    private int completedLazerCount = 0;

    protected override void Shoot()
    {
        Vector3 startPosition = owner.transform.position;

        LazerNode rootNode = GenerateRootTree(startPosition);

        List<LazerSegment> segments = CollectAllSegments(rootNode);

        completedLazerCount = 0;
        activeLazers.Clear();

        foreach(var segment in segments)
        {
            GameObject lazerObject = GetOrCreateLazer();
            Lazer lazer = lazerObject.GetComponent<Lazer>();

            Vector3 direction = (segment.End - segment.Start).normalized;
            float segmentActualLength = Vector3.Distance(segment.Start, segment.End);
            float damage = owner.atk;

            lazer.SetDuration(duration);
            lazer.SetLazerWidth(laserWidth);
            lazer.SetTickInterval(tickInterval);

            lazer.Initialize(segment.Start, direction, damage, OnSingleLazerComplete, segmentActualLength);

            lazerObject.SetActive(true);
            activeLazers.Add(lazer);
        }

        if(movement != null)
        {
            movement.CanMove = false;
        }
    }

    private LazerNode GenerateRootTree(Vector3 startPosition)
    {
        LazerNode root = new LazerNode(startPosition);
        CreateBranches(root, 0, Vector3.down);
        return root;
    }

    private void CreateBranches(LazerNode parentNode, int currentDepth, Vector3 parentDirection)
    {
        if(currentDepth >= maxDepth)
        {
            return;
        }

        Rect screenBounds = SpawnManager.Instance.ScreenBounds;
        if(parentNode.Position.y <= screenBounds.yMin)
        {
            return;
        }

        int branchCount = Random.Range(minBrances, maxBranches + 1);

        for(int i = 0; i < branchCount; i++)
        {
            Vector3 breanchDirection = CalculateBranchDirection(parentDirection);

            Vector3 childPosition = parentNode.Position + breanchDirection * segmentLength;

            childPosition.x = Mathf.Clamp(childPosition.x, screenBounds.xMin, screenBounds.xMax);
            childPosition.y = Mathf.Max(childPosition.y, screenBounds.yMin);

            LazerNode childNode = new LazerNode(childPosition, parentNode);
            parentNode.AddChild(childNode);

            CreateBranches(childNode, currentDepth + 1, breanchDirection);
        }
    }

    private Vector3 CalculateBranchDirection(Vector3 baseDirection)
    {
        Vector3 downward = Vector3.down; //base direction

        float randomAngle = Random.Range(-angleVariance, angleVariance);
        Vector3 randomDirection = Quaternion.Euler(0f, 0f, randomAngle) * baseDirection;

        Vector3 finalDirection = Vector3.Lerp(randomDirection, downward, downwardBias).normalized;

        return finalDirection;
    }

    private List<LazerSegment> CollectAllSegments(LazerNode root)
    {
        List<LazerSegment> segments = new List<LazerSegment>();

        CollectSegmentsRecursive(root, segments);

        return segments;
    }

    private void CollectSegmentsRecursive(LazerNode node, List<LazerSegment> segments)
    {
        foreach(var child in node.Children)
        {
            segments.Add(new LazerSegment(node.Position, child.Position));
            CollectSegmentsRecursive(child, segments);
        }
    }

    private void OnSingleLazerComplete()
    {
        completedLazerCount++;
        if(completedLazerCount >= activeLazers.Count)
        {
            if(movement != null)
            {
                movement.CanMove = true;
            }

            activeLazers.Clear();
            completedLazerCount = 0;

            planet.IsLazerHit = false;

            lazerCompletionSource?.TrySetResult();
        }
    }
}
