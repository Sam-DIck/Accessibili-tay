using UnityEngine;

public class Radar : MonoBehaviour
{
    Driver driver;
    SubController subController;

    [SerializeField] int rayCount = 100;
    [SerializeField] LayerMask radarMask;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        driver ??= GetComponent<Driver>();
        subController ??= FindAnyObjectByType<SubController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 position = subController.position;
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
        float mormailsedAngle = (closestAngle + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
        // 0 is dead ahead/behind, -90 is left, 90 is right
        float bal = Mathf.Sin(mormailsedAngle);
        if (driver) driver.SetVibrations(-bal, bal);
    }
}
