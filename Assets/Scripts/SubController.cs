using UnityEngine;
using UnityEngine.UIElements;

public class SubController : MonoBehaviour
{
    
    [SerializeField] Rigidbody2D boat;
    [SerializeField] Transform map;
    [SerializeField] Transform periscope;

    [Header("Movement")]
    [SerializeField] float thrustMultiplier = 8f;
    [SerializeField] float reverseMultiplier = 5f;
    [SerializeField] float maxSpeed = 8f;
    [SerializeField] float waterDrag = 0.5f;
    [SerializeField] float lateralDrag = 2f;

    [Header("Steering")]
    [SerializeField] float turnSpeed = 100f; // degrees/sec
    [SerializeField] float turnAcceleration = 180f;
    [SerializeField] float turnDrag = 2f;
    [SerializeField] float steeringAtLowSpeed = 0.4f;

    public Vector2 position => new Vector2(boat.position.x, boat.position.y);

    float throttleInput;
    float steeringInput;
    float turnVelocity;

    public void Move(float throttle, float turn)
    {
        throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        steeringInput = Mathf.Clamp(turn, -1f, 1f);
    }

    private void FixedUpdate()
    {
        Vector2 forward = boat.transform.up;
        Vector2 right = boat.transform.right;
        Vector2 velocity = boat.linearVelocity;

        // Forward and reverse propulsion.
        float thrust = throttleInput >= 0f
            ? thrustMultiplier
            : reverseMultiplier;

        boat.AddForce(
            forward * throttleInput * thrust,
            ForceMode2D.Force
        );

        // Water resistance, split into forward and sideways movement.
        float forwardSpeed = Vector2.Dot(velocity, forward);
        float sideSpeed = Vector2.Dot(velocity, right);

        Vector2 drag =
            -velocity * waterDrag
            - right * sideSpeed * lateralDrag;

        boat.AddForce(drag, ForceMode2D.Force);

        // Smooth steering response.
        float speedFactor = Mathf.Lerp(
            steeringAtLowSpeed,
            1f,
            Mathf.Clamp01(velocity.magnitude / maxSpeed)
        );

        float targetTurnVelocity =
            -steeringInput * turnSpeed * speedFactor;

        turnVelocity = Mathf.MoveTowards(
            turnVelocity,
            targetTurnVelocity,
            turnAcceleration * Time.fixedDeltaTime
        );

        // Gradually slow rotation when steering is released.
        if (Mathf.Abs(steeringInput) < 0.01f)
        {
            turnVelocity = Mathf.MoveTowards(
                turnVelocity,
                0f,
                turnDrag * turnSpeed * Time.fixedDeltaTime
            );
        }

        boat.MoveRotation(
            boat.rotation + turnVelocity * Time.fixedDeltaTime
        );

        // Arcade speed cap.
        if (boat.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            boat.linearVelocity =
                boat.linearVelocity.normalized * maxSpeed;
        }
    }
    public void RotatePeriscope(float input)
    {
        periscope.Rotate(Vector3.back, input * Time.deltaTime * turnSpeed);
    }
}
