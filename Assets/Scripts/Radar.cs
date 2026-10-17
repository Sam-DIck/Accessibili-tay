using Unity.VisualScripting;
using UnityEngine;

public class Radar : MonoBehaviour
{
    Driver driver;
    SubController subController;

    [SerializeField] AnimationCurve blipOnTime = new(new Keyframe(0f, 0.1f), new Keyframe(1f, 0.1f));
    [SerializeField] AnimationCurve blipOffTime = new(new Keyframe(0f, 10f), new Keyframe(1f, 0.5f));

    // 
    static float mapPing(float dist)
    {
        dist = Mathf.Max(0f, dist);
        return Mathf.Clamp01((dist) / (1f + dist));
    }

    [SerializeField] int rayCount = 100;
    [SerializeField] LayerMask radarMask;
    [SerializeField] float angleOffset = 90f;
    [SerializeField] bool flipAngle = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        driver ??= GetComponent<Driver>();
        subController ??= FindAnyObjectByType<SubController>();
    }
    
    bool blipOn = false;
    float blipTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        Vector2 position = subController.position;
        float rotation = subController.rotation * Mathf.Deg2Rad;
        Debug.Log(position);
        if (driver == null || !driver.enabled) return;

        float angleStep = 2 * Mathf.PI / rayCount;

        float closestDistance = float.PositiveInfinity;
        float closestAngle = 0; // initialise dead ahead
        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            Ray2D ray = new Ray2D(position, dir);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, radarMask);
            if (hit)
            {
                float distance = hit.distance;
                Debug.DrawLine(position, position + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * distance, Color.green);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestAngle = angle;
                }
            }
        }


        Debug.DrawLine(position, position + new Vector2(Mathf.Sin(closestAngle), Mathf.Cos(closestAngle)) * closestDistance, Color.red);

        float correctedAngle = (closestAngle - rotation) * (flipAngle ? -1f : 1f) + angleOffset * Mathf.Deg2Rad;
        float normailsedAngle = (correctedAngle + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
        Debug.Log($"closestAngle {closestAngle * Mathf.Rad2Deg} rotation {rotation * Mathf.Rad2Deg} normailsedAngle {normailsedAngle * Mathf.Rad2Deg}");

        // 0 is dead ahead/behind, -90 is left, 90 is right
        float bal = Mathf.Sin(normailsedAngle);
        if (driver)
        {
            float pingFactor = mapPing(closestDistance);
            Debug.Log($"blipOn: {blipOn}, closestDistance: {closestDistance} ({pingFactor})");
            blipTimer += Time.deltaTime;
            if (blipOn)
            {
                if (blipTimer > blipOnTime.Evaluate(pingFactor))
                {
                    blipOn = false;
                    blipTimer = 0f;
                }
                driver.SetVibrations(-bal, bal);
            }
            else
            {
                if (blipTimer > blipOffTime.Evaluate(pingFactor))
                {
                    blipOn = true;
                    blipTimer = 0f;
                }
                driver.SetVibrations(0, 0);
            }
        }
    }
}
