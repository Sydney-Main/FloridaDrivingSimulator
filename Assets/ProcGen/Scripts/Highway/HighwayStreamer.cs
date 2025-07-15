using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighwayStreamer : MonoBehaviour
{
    [Header("Streaming Volume")]
    public Collider genCircle;
    public Transform spawnOrigin;

    [Header("Streaming Settings")]
    public HighwaySegmentSO startingSegmentSO;
    public int initialSegments = 5;
    public int cellSize = 10;        // size used for grid-chunk-coords

    private SpatialTracker spatialTracker;
    private List<HighwaySegment> segments = new List<HighwaySegment>();

    void Start()
    {
        spatialTracker = new SpatialTracker(cellSize);

        // seed the first chain
        HighwaySegment prev = CreateSegment(
            startingSegmentSO,
            spawnOrigin.position,
            Quaternion.identity);
        spatialTracker.Register(prev);
        segments.Add(prev);

        for (int i = 1; i < initialSegments; i++)
        {
            if (!TryExtend(prev, out prev))
                break;
            segments.Add(prev);
        }
    }

    void Update()
    {
        if (segments.Count == 0)
            return;

        // compute chunk-coord bounds from the genCircle
        Bounds b = genCircle.bounds;
        Vector2Int minChunk = WorldToChunkCoord(b.min);
        Vector2Int maxChunk = WorldToChunkCoord(b.max);

        // 1) extend forward if last exit is within [min...max]
        var last = segments.Last();
        Vector2Int lastCoord = WorldToChunkCoord(last.exitTrigger.position);
        bool insideX = lastCoord.x >= minChunk.x && lastCoord.x <= maxChunk.x;
        bool insideZ = lastCoord.y >= minChunk.y && lastCoord.y <= maxChunk.y;
        if (insideX && insideZ)
        {
            if (TryExtend(last, out HighwaySegment next))
                segments.Add(next);
        }

        // 2) unload first segment if its entry is outside [min...max]
        var first = segments.First();
        Vector2Int firstCoord = WorldToChunkCoord(first.entryTrigger.position);
        bool inX = firstCoord.x >= minChunk.x && firstCoord.x <= maxChunk.x;
        bool inZ = firstCoord.y >= minChunk.y && firstCoord.y <= maxChunk.y;
        if (!inX || !inZ)
        {
            spatialTracker.Unregister(first);
            segments.RemoveAt(0);
            Destroy(first.gameObject);
        }
    }

    bool TryExtend(HighwaySegment previous, out HighwaySegment next)
    {
        next = null;
        foreach (var so in SegmentSelector
                             .GetValidSegments(previous.data)
                             .OrderBy(_ => Random.value))
        {
            var seg = CreateSegment(so, Vector3.zero, Quaternion.identity);

            // snap and gap-nudge
            if (ConnectorManager.TryAlignSegment(previous, seg))
                seg.transform.position += previous.exitTrigger.forward * 0.1f;

            // overlap test
            if (!spatialTracker.IsOverlapping(seg))
            {
                spatialTracker.Register(seg);
                next = seg;
                return true;
            }

            Destroy(seg.gameObject);
        }

        return false;
    }

    HighwaySegment CreateSegment(
        HighwaySegmentSO so,
        Vector3 pos,
        Quaternion rot)
    {
        GameObject go = Instantiate(so.prefab, pos, rot);
        HighwaySegment seg = go.GetComponent<HighwaySegment>();
        seg.data = so;
        return seg;
    }

    // converts world->chunk grid coords
    Vector2Int WorldToChunkCoord(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int z = Mathf.FloorToInt(worldPos.z / cellSize);
        return new Vector2Int(x, z);
    }
}