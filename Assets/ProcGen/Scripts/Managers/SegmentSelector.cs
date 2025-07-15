using System.Collections.Generic;
using UnityEngine;

public static class SegmentSelector
{
    public static List<HighwaySegmentSO> GetValidSegments(HighwaySegmentSO previous)
    {
        var valid = new List<HighwaySegmentSO>();
        Debug.Log($"[Selector] Previous segment: {previous.name} (exitType={previous.exitType})");
        Debug.Log($"[Selector] Database contains {SegmentDatabase.All.Count} segments:");

        foreach (var candidate in SegmentDatabase.All)
        {
            Debug.Log($"  - {candidate.name} (entryType={candidate.entryType})");
            if (candidate.entryType == previous.exitType)
            {
                valid.Add(candidate);
                Debug.Log($"    -> Added {candidate.name}");
            }
        }

        if (valid.Count == 0)
            Debug.LogWarning($"[Selector] No valid segments found for exitType={previous.exitType}");

        return valid;
    }
}