using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Highway/Segment Database")]
public class SegmentDatabase : ScriptableObject
{
    // Populate with SOs
    public List<HighwaySegmentSO> allSegments = new List<HighwaySegmentSO>();

    private static SegmentDatabase _instance;
    public static IReadOnlyList<HighwaySegmentSO> All
    {
        get
        {
            if (_instance == null)
            {
                // Attempts to load a Database asset from Resources/Highway
                _instance = Resources.Load<SegmentDatabase>("Highway/SegmentDatabase");

                // Fallback: create a temporary one
                if (_instance == null)
                    Debug.LogError("SegmentDatabase not found in Resources/Highway/SegmentDatabase");
            }
            return _instance.allSegments;
        }
    }
}