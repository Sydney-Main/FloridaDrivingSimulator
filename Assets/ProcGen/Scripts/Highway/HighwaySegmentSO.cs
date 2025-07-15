using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HighwaySegmentSO : ScriptableObject
{
    public string segmentType;
    public string entryType;
    public string exitType;
    public List<float> allowedRotations; // e.g. { 0f, 90f, 180f, 270f }
    public GameObject prefab;

    public bool CanConnectTo(HighwaySegmentSO previous, float rotationDiff)
    {
        return previous.exitType == entryType && allowedRotations.Contains(rotationDiff);
    }

}
