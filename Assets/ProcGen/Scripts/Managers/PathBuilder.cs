using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathBuilder : MonoBehaviour
{
    [Header("Initial Setup")]
    public HighwaySegmentSO startingSegmentSO;
    public Transform spawnOrigin;
    public int maxSegments = 30;

    private List<HighwaySegment> placedSegments = new List<HighwaySegment>();
    private SpatialTracker spatialTracker;
    private int cellSize = 10;  // adjust to your segment length

    void Start()
    {
        spatialTracker = new SpatialTracker(cellSize);
        BuildPath();
    }

    void BuildPath()
    {
        // Instantiate and register the very first segment
        HighwaySegment start = InstantiateSegment(
            startingSegmentSO,
            spawnOrigin.position,
            Quaternion.identity);

        spatialTracker.Register(start);
        placedSegments.Add(start);

        // Begin recursive placement
        TryBuildRecursive(start, 1);
    }

    bool TryBuildRecursive(HighwaySegment previous, int depth)
    {
        if (depth >= maxSegments)
            return true;

        var candidates = SegmentSelector.GetValidSegments(previous.data)
                .Where(s => s != previous.data)
                .OrderBy(_ => Random.value)
                .ToList();

        foreach (var candidate in candidates)
        {
            // Spawn the next piece
            GameObject obj = Instantiate(candidate.prefab);
            var next = obj.GetComponent<HighwaySegment>();
            next.data = candidate;

            // Snap into place
            if (ConnectorManager.TryAlignSegment(previous, next))
            {
                // Overlap check
                if (spatialTracker.IsOverlapping(next))
                {
                    Destroy(obj);
                    continue;
                }

                // Register and recurse
                spatialTracker.Register(next);
                placedSegments.Add(next);

                if (TryBuildRecursive(next, depth + 1))
                    return true;

                // Backtrack
                spatialTracker.Unregister(next);
                placedSegments.Remove(next);
                Destroy(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        return false;
    }

    HighwaySegment InstantiateSegment(HighwaySegmentSO so, Vector3 pos, Quaternion rot)
    {
        GameObject obj = Instantiate(so.prefab, pos, rot);
        var seg = obj.GetComponent<HighwaySegment>();
        seg.data = so;
        return seg;
    }

    // Debug: Draw connections
    void OnDrawGizmos()
    {
        if (placedSegments == null) return;
        Gizmos.color = Color.yellow;

        for (int i = 0; i < placedSegments.Count - 1; i++)
        {
            var a = placedSegments[i].exitTrigger.position;
            var b = placedSegments[i + 1].entryTrigger.position;
            Gizmos.DrawLine(a, b);
        }
    }
}