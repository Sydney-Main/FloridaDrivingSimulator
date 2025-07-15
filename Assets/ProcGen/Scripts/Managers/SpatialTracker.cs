using System.Collections.Generic;
using UnityEngine;

public class SpatialTracker
{
    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    private int cellSize;

    public SpatialTracker(int cellSize)
    {
        this.cellSize = cellSize;
        // Debug.Log("SpatialTracker created with cellSize = " + cellSize);
    }

    public bool IsOverlapping(HighwaySegment seg)
    {
        // Debug.Log("Checking overlap for segment: " + seg.name);
        foreach (Vector2Int cell in GetFootprintCells(seg))
        {
            // Debug.Log("  checking cell " + cell);
            if (occupied.Contains(cell))
            {
                // Debug.Log("  overlap detected at cell " + cell);
                return true;
            }
        }
        // Debug.Log("  no overlap for segment: " + seg.name);
        return false;
    }

    public void Register(HighwaySegment seg)
    {
        // Debug.Log("Registering segment: " + seg.name);
        foreach (Vector2Int cell in GetFootprintCells(seg))
        {
            /*
            if (occupied.Add(cell))
                Debug.Log("  added occupied cell " + cell);
            else
                Debug.Log("  cell already occupied: " + cell);
            */
        }
    }

    public void Unregister(HighwaySegment seg)
    {
        // Debug.Log("Unregistering segment: " + seg.name);
        foreach (Vector2Int cell in GetFootprintCells(seg))
        {
            /*
            if (occupied.Remove(cell))
                Debug.Log("  removed cell " + cell);
            else
                Debug.Log("  cell not found: " + cell);
            */
        }
    }

    private IEnumerable<Vector2Int> GetFootprintCells(HighwaySegment seg)
    {
        Bounds combined = GetCombinedBounds(seg);
        /*
        Debug.Log("Combined bounds for " + seg.name +
                  ": min=" + combined.min +
                  " max=" + combined.max);
        */

        Vector3 min = combined.min;
        Vector3 max = combined.max;

        int xMin = Mathf.FloorToInt(min.x / cellSize);
        int zMin = Mathf.FloorToInt(min.z / cellSize);
        int xMax = Mathf.FloorToInt(max.x / cellSize);
        int zMax = Mathf.FloorToInt(max.z / cellSize);

        /*
          Debug.Log("  footprint grid range x[" + xMin + "," + xMax +
                  "] z[" + zMin + "," + zMax + "]");
        */

        for (int x = xMin; x <= xMax; x++)
        {
            for (int z = zMin; z <= zMax; z++)
            {
                yield return new Vector2Int(x, z);
            }
        }
    }

    private Bounds GetCombinedBounds(HighwaySegment seg)
    {
        // Debug.Log("Getting combined bounds for: " + seg.name);
        Collider[] cols = seg.GetComponentsInChildren<Collider>();
        if (cols == null || cols.Length == 0)
        {
            // Debug.LogWarning("  no colliders found on segment: " + seg.name);
            return new Bounds(seg.transform.position, Vector3.zero);
        }

        Bounds b = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++)
        {
            b.Encapsulate(cols[i].bounds);
        }
        // Debug.Log("  combined bounds size: " + b.size);
        return b;
    }
}