using UnityEngine;
using UnityEngine.Rendering;

public struct HitInfo
{
    public RaycastHit hit;
    public Vector3 hitDirection;
    public float hitSpeed;
    public Bullet bullet;
    //public bool destoryBullet;
}

public class Bullet : MonoBehaviour
{
    Vector3 startPosition;
    Vector3 startDirection; // e_v
    Vector2 wind;
    float mass; // m ( kg )
    float startVelocity; // v_0
    float velocity; // v
    float gravity = 9.82f;

    float startTime;
    bool isInitialized = false;

    public void Initialize(Vector3 startPosition, Vector3 startDirection, Vector2 wind, float mass, float velocity)
    {
        this.startPosition = startPosition;
        this.startDirection = startDirection;
        this.wind = wind;
        this.mass = mass;
        this.startVelocity = velocity;
        isInitialized = true;
        startTime = -1;
    }

    //  THIS IS BASICLY WHAT MATTIAS CARES ABOUT :)
    //  
    //  Denna funktionen ger ut en vector från origo för positionen som 
    //  skåttet befinner sig vid på en viss tidspunkt (time) efter utskjutning

    private Vector3 FindPointOnParabola(float time) 
    {
        Vector3 movementVector = startDirection * time * startVelocity; // c * v^2 * e_v
        Vector3 gravityAdjustment = mass * gravity * Vector3.down * time * time; // m * g * e_v
        Vector3 windAdjustment = new Vector3(wind.x, 0, wind.y) * time * time;
        return startPosition + movementVector + gravityAdjustment + windAdjustment;
    }

    private float GetDragCoefficient()
    {
        return 0.0f;
    }

    private bool CastRayBetweenPoints(Vector3 startPoint, Vector3 endPoint, out RaycastHit hit)
    {
        return Physics.Raycast(startPoint, endPoint - startPoint, out hit, (endPoint - startPoint).magnitude);
    }

    private void OnHit(RaycastHit hit, Vector3 hitVector)
    {
        ShootableObject[] shootableObjects = hit.transform.GetComponents<ShootableObject>();
        HitInfo hitInfo = new HitInfo()
        {
            hit = hit,
            hitDirection = hitVector.normalized,
            hitSpeed = hitVector.magnitude / Time.fixedDeltaTime,
            bullet = this
        };
        foreach (ShootableObject shootableObject in shootableObjects)
        {
            shootableObject.OnHit(ref hitInfo);
        }

        Destroy(gameObject); // Deleting bullet
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;
        if (startTime < 0) startTime = Time.time;

        RaycastHit hit;
        float currentTime = Time.time - startTime;
        float prevTime = currentTime - Time.fixedDeltaTime;
        float nextTime = currentTime + Time.fixedDeltaTime;

        Vector3 currentPoint = FindPointOnParabola(currentTime);
        Vector3 nextPoint = FindPointOnParabola(nextTime);

        if (prevTime > 0)
        {
            Vector3 prevPoint = FindPointOnParabola(prevTime);

            if (CastRayBetweenPoints(prevPoint, currentPoint, out hit))
            {
                OnHit(hit, currentPoint - prevPoint);
            }
        }

        if (CastRayBetweenPoints(currentPoint, nextPoint, out hit))
        {
            OnHit(hit, nextPoint - currentPoint);
        }
    }


    private void Update()
    {
        if (!isInitialized ||startTime < 0) return;

        float currentTime = Time.time - startTime;
        Vector3 currentPoint = FindPointOnParabola(currentTime);
        transform.position = currentPoint;
    }
}
