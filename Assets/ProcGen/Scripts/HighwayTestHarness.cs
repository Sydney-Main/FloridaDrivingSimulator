using UnityEngine;

public class HighwayTestHarness : MonoBehaviour
{
    [Header("Test Seg. Setup")]
    public HighwaySegmentSO startSegmentSO;
    public HighwaySegmentSO connectedSegmentSO;

    [Header("Gen Settings")]
    public Transform spawnOrigin;

    public HighwaySegment startSegment;
    public HighwaySegment connectedSegment;

    void Start()
    {
        RunTest();
    }

    void RunTest()
    {
        if (startSegmentSO == null || connectedSegmentSO == null || spawnOrigin == null)
        {
            Debug.LogError("Missing fields in HighwayTestHarness.");
            return;
        }

        // Instantiate first segment (Starting/Spawn segment)
        GameObject startObj = Instantiate(startSegmentSO.prefab, spawnOrigin.position, Quaternion.identity);
        startSegment = startObj.GetComponent<HighwaySegment>();
        startSegment.data = startSegmentSO;

        // Instantiate connected segment at same position temp.
        GameObject connectedObj = Instantiate(connectedSegmentSO.prefab, spawnOrigin.position, Quaternion.identity);
        connectedSegment = connectedObj.GetComponent<HighwaySegment>();
        connectedSegment.data = connectedSegmentSO;

        // Attempt align
        bool success = ConnectorManager.TryAlignSegment(startSegment, connectedSegment);

        if (success)
        {
            Debug.Log("Connection successful");
        }
        else
        {   
            Debug.LogWarning("Connection failed. Destroying connected segment.");
            Destroy(connectedObj);
        }
        
    }
}
