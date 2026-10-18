
using UnityEngine;

public class Radar : MonoBehaviour
{
    private Driver driver;
    private SubController subController;

    [Header("Treasure Detection")]
    [SerializeField] private string treasureTag = "Treasure";

    [Header("Vibration Timing")]
    [SerializeField]
    private AnimationCurve blipOnTime =
        new AnimationCurve(
            new Keyframe(0f, 0.1f),
            new Keyframe(1f, 0.1f)
        );

    [SerializeField]
    private AnimationCurve blipOffTime =
        new AnimationCurve(
            new Keyframe(0f, 10f),
            new Keyframe(1f, 0.5f)
        );

    [Header("Vibration")]
    [SerializeField, Range(0f, 1f)]
    private float vibrationStrength = 1f;

    private bool blipOn;
    private float blipTimer;

    private void Awake()
    {
        driver = GetComponent<Driver>();
        subController = FindAnyObjectByType<SubController>();
    }

    private void Update()
    {
        if (driver == null || !driver.enabled)
            return;

        if (subController == null)
            return;

        // Get the submarine's position.
        Vector2 playerPosition = subController.position;

        // Find all treasures in the scene.
        GameObject[] treasures = GameObject.FindGameObjectsWithTag(treasureTag);

        Transform nearestTreasure = null;
        float closestDistanceSquared = float.PositiveInfinity;

        foreach (GameObject treasure in treasures)
        {
            if (treasure == null)
                continue;

            Vector2 treasurePosition = treasure.transform.position;

            // Squared distance avoids unnecessary square roots.
            float distanceSquared =
                (treasurePosition - playerPosition).sqrMagnitude;

            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                nearestTreasure = treasure.transform;
            }
        }

        // No treasure exists.
        if (nearestTreasure == null)
        {
            blipOn = false;
            blipTimer = 0f;
            driver.SetVibrations(0f, 0f);
            return;
        }

        Vector2 treasureDirection =
            ((Vector2)nearestTreasure.position - playerPosition).normalized;

        // Assumes rotation 0 degrees means the submarine faces upward.
        float rotation = subController.rotation * Mathf.Deg2Rad;

        Vector2 forward = new Vector2(
            Mathf.Sin(rotation),
            Mathf.Cos(rotation)
        );

        // Dot product: positive = in front, negative = behind.
        float dot = Vector2.Dot(forward, treasureDirection);

        // 2D cross product: determines the side of the forward vector.
        float cross =
            forward.x * treasureDirection.y -
            forward.y * treasureDirection.x;

        bool treasureInFront = dot > 0f;

        // Positive cross = left; negative cross = right
        // under this forward-vector convention.
        bool treasureOnLeft = cross < -0.001f;
        bool treasureOnRight = cross > 0.001f;

        // Debug visualization.
        Debug.DrawLine(
            playerPosition,
            playerPosition + forward * 3f,
            Color.blue
        );

        Debug.DrawLine(
            playerPosition,
            nearestTreasure.position,
            Color.red
        );

        // Distance-based pulse timing.
        float distance = Mathf.Sqrt(closestDistanceSquared);

        float pingFactor = Mathf.Clamp01(
            distance / (1f + distance)
        );

        blipTimer += Time.deltaTime;

        if (blipOn)
        {
            if (blipTimer >= blipOnTime.Evaluate(pingFactor))
            {
                blipOn = false;
                blipTimer = 0f;
            }

            if (treasureOnLeft)
            {
                driver.SetVibrations(vibrationStrength, 0f);
            }
            else if (treasureOnRight)
            {
                driver.SetVibrations(0f, vibrationStrength);
            }
            else
            {
                // Treasure is directly in front of or behind the submarine.
                driver.SetVibrations(0f, 0f);
            }
        }
        else
        {
            if (blipTimer >= blipOffTime.Evaluate(pingFactor))
            {
                blipOn = true;
                blipTimer = 0f;
            }

            driver.SetVibrations(0f, 0f);
        }
    }

    private void OnDisable()
    {
        if (driver != null)
            driver.SetVibrations(0f, 0f);
    }
}
