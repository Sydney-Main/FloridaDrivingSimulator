using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Controls a car using Unity's WheelCollider physics and the new Input System
public class CarController : MonoBehaviour
{
    // Tuning values for power, steering, and braking
    public float maxMotorTorque = 1500f;
    public float maxSteeringAngle = 30f;
    public float brakeForce = 3000f;

    // References to the WheelColliders used for physics simulation
    public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;

    // References to the visible wheel meshes for visual rotation
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    // Input values received from the player
    private float steerInput = 0f;
    private float throttleInput = 0f;
    private bool isBraking = false;
    private bool isDrifting = false;
    private bool wasDrifting = false;
    private bool isSpinning = false;

    // Style meter variables
    private int stylePoints = 0;         // Current style points
    private float lastHitTime = 0f;      // Time since last style gain
    private const float decayDelay = 3f; // Time in seconds before style decays
    private const int maxStyle = 100;    // Maximum style value

    // Reference to the input action map (auto-generated from Input Actions asset)
    private CarControls controls;

    // Setup input bindings
    private void Awake()
    {
        controls = new CarControls();

        // Steering input (A/D or Left/Right keys)
        controls.Driving.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
        controls.Driving.Steer.canceled += _ => steerInput = 0f;

        // Throttle input (W/S or Up/Down keys)
        controls.Driving.Throttle.performed += ctx => throttleInput = ctx.ReadValue<float>();
        controls.Driving.Throttle.canceled += _ => throttleInput = 0f;

        // Brake input (Spacebar)
        controls.Driving.Brake.performed += _ => isBraking = true;
        controls.Driving.Brake.canceled += _ => isBraking = false;

        // Drift input (Left Shift)
        controls.Driving.Drift.performed += _ => isDrifting = true;
        controls.Driving.Drift.canceled += _ => isDrifting = false;

        // Attack input (E key)
        controls.Driving.Attack.performed += _ => SpinAttack();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        ApplySteering();
        ApplyMotor();
        ApplyBrakes();
        ApplyDrift();
        UpdateWheelTransforms();
        UpdateStyleDecay();
    }

    // Apply throttle input to the rear wheels
    void ApplyMotor()
    {
        rearLeftWheel.motorTorque = throttleInput * maxMotorTorque;
        rearRightWheel.motorTorque = throttleInput * maxMotorTorque;
    }

    // Apply steering input to the front wheels
    void ApplySteering()
    {
        float steer = steerInput * maxSteeringAngle;
        frontLeftWheel.steerAngle = steer;
        frontRightWheel.steerAngle = steer;
    }

    // Apply brake force to all four wheels if braking
    void ApplyBrakes()
    {
        float brake = isBraking ? brakeForce : 0f;
        frontLeftWheel.brakeTorque = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque = brake;
        rearRightWheel.brakeTorque = brake;
    }

    // Adjust wheel friction to simulate drifting
    void ApplyDrift()
    {
        if (isDrifting && !wasDrifting)
        {
            SetFriction(0.5f);
            wasDrifting = true;
        }
        else if (!isDrifting && wasDrifting)
        {
            SetFriction(1.0f);
            wasDrifting = false;
        }
    }

    // Adjust friction on all wheels
    void SetFriction(float stiffness)
    {
        SetWheelFriction(frontLeftWheel, stiffness);
        SetWheelFriction(frontRightWheel, stiffness);
        SetWheelFriction(rearLeftWheel, stiffness);
        SetWheelFriction(rearRightWheel, stiffness);
    }

    void SetWheelFriction(WheelCollider wheel, float stiffness)
    {
        WheelFrictionCurve friction = wheel.sidewaysFriction;
        friction.stiffness = stiffness;
        wheel.sidewaysFriction = friction;
    }

    // Update wheel mesh transforms
    void UpdateWheelTransforms()
    {
        UpdateWheel(frontLeftWheel, frontLeftTransform);
        UpdateWheel(frontRightWheel, frontRightTransform);
        UpdateWheel(rearLeftWheel, rearLeftTransform);
        UpdateWheel(rearRightWheel, rearRightTransform);
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        trans.position = pos;
        trans.rotation = rot;
    }

    // Trigger spin attack
    void SpinAttack()
    {
        if (!isSpinning)
        {
            StartCoroutine(Spin360());
            AddStylePoints(10); // Gain style when spinning
        }
    }

    // Coroutine to rotate the car 360 degrees
    IEnumerator Spin360()
    {
        isSpinning = true;

        float spinDuration = 0.5f;
        float elapsed = 0f;
        float startY = transform.eulerAngles.y;
        float endY = startY + 360f;

        while (elapsed < spinDuration)
        {
            float t = elapsed / spinDuration;
            float currentY = Mathf.Lerp(startY, endY, t);
            Vector3 currentEuler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(currentEuler.x, currentY, currentEuler.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, endY % 360f, transform.eulerAngles.z);
        isSpinning = false;
    }

    // Add style points (clamped)
    void AddStylePoints(int amount)
    {
        stylePoints = Mathf.Clamp(stylePoints + amount, 0, maxStyle);
        lastHitTime = Time.time;
        Debug.Log($"Style: {stylePoints} ({GetStyleRank()})");
    }

    // Handle automatic decay after inactivity
    void UpdateStyleDecay()
    {
        if (Time.time - lastHitTime > decayDelay && stylePoints > 0)
        {
            stylePoints = Mathf.Max(0, stylePoints - 1); // Lose 1 per frame
            Debug.Log($"Style Decay: {stylePoints} ({GetStyleRank()})");
        }
    }

    // Get letter rank based on style meter value
    string GetStyleRank()
    {
        if (stylePoints >= 100) return "S";
        if (stylePoints >= 75) return "A";
        if (stylePoints >= 50) return "B";
        if (stylePoints >= 25) return "C";
        return "D";
    }
}