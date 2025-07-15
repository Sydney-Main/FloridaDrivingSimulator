using System.Collections.Generic;
using UnityEngine;

public static class ConnectorManager
{
    public static bool TryAlignSegment(HighwaySegment previous, HighwaySegment next)
    {
        Transform prevExit = previous.exitTrigger;
        Transform nextEntry = next.entryTrigger;

        // 1) Determine the snapped Y-rotation
        Quaternion rawRot = Quaternion.LookRotation(prevExit.forward);
        float rawY = rawRot.eulerAngles.y;
        float snappedY = NearestAngle(rawY, next.data.allowedRotations);
        Quaternion targetRot = Quaternion.Euler(0f, snappedY, 0f);

        // 2) Capture local pivot of the entry trigger before rotating
        Vector3 localPivot = next.transform.InverseTransformPoint(nextEntry.position);

        // 3) Apply rotation
        next.transform.rotation = targetRot;

        // 4) Compute world-space position of that pivot *after* rotation
        Vector3 pivotWorldAfter = next.transform.TransformPoint(localPivot);

        // 5) Offset segment so that entryPivot lands on prevExit
        Vector3 offset = prevExit.position - pivotWorldAfter;
        next.transform.position += offset;

        // 6) Validation: type match + rotation difference
        float rotDiff = Mathf.Abs(Mathf.DeltaAngle(
            previous.transform.eulerAngles.y, snappedY));
        return next.data.CanConnectTo(previous.data, rotDiff);
    }

    static float NearestAngle(float current, List<float> options)
    {
        float best = options[0];
        float minDelta = Mathf.Abs(Mathf.DeltaAngle(current, best));
        foreach (float angle in options)
        {
            float delta = Mathf.Abs(Mathf.DeltaAngle(current, angle));
            if (delta < minDelta)
            {
                minDelta = delta;
                best = angle;
            }
        }
        return best;
    }
}