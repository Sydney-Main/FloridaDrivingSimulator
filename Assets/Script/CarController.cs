using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool wasDrifting = false; // Used to prevent repeating drift logic every frame

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
    }

    // Enable input controls when the object is active
    private void OnEnable() => controls.Enable();

    // Disable input controls when the object is inactive
    private void OnDisable() => controls.Disable();

    // Called on each physics update (FixedUpdate is better for physics)
    private void FixedUpdate()
    {
        ApplySteering();         // Turn the wheels based on player input
        ApplyMotor();            // Accelerate based on throttle input
        ApplyBrakes();           // Brake if braking is active
        ApplyDrift();            // Adjust friction if drifting
        UpdateWheelTransforms(); // Sync visual wheels with physics wheels
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
            // Drift just started
            Debug.Log("Drift started");
            SetFriction(0.5f); // Lower grip = more slide
            wasDrifting = true;
        }
        else if (!isDrifting && wasDrifting)
        {
            // Drift just ended
            Debug.Log("Drift ended");
            SetFriction(1.0f); // Restore grip
            wasDrifting = false;
        }
    }

    // Set sideways friction stiffness for all wheels
    void SetFriction(float stiffness)
    {
        SetWheelFriction(frontLeftWheel, stiffness);
        SetWheelFriction(frontRightWheel, stiffness);
        SetWheelFriction(rearLeftWheel, stiffness);
        SetWheelFriction(rearRightWheel, stiffness);
    }

    // Apply friction value to one wheel
    void SetWheelFriction(WheelCollider wheel, float stiffness)
    {
        WheelFrictionCurve friction = wheel.sidewaysFriction;
        friction.stiffness = stiffness;
        wheel.sidewaysFriction = friction;
    }

    // Update the visual wheel models to match the physics wheel positions
    void UpdateWheelTransforms()
    {
        UpdateWheel(frontLeftWheel, frontLeftTransform);
        UpdateWheel(frontRightWheel, frontRightTransform);
        UpdateWheel(rearLeftWheel, rearLeftTransform);
        UpdateWheel(rearRightWheel, rearRightTransform);
    }

    // Copy the position and rotation from the WheelCollider to the visual mesh
    void UpdateWheel(WheelCollider col, Transform trans)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}