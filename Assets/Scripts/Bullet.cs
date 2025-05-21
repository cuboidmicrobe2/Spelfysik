using System;
using UnityEngine;

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
    Vector3 direction; // e_v
    Vector2 wind;
    Vector3 velocityVector;
    float mass; // m ( kg )
    float startVelocity; // v_0  ( m/s )
    float velocity; // v  ( m/s )
    float gravity = 9.82f; // g  ( m/s² )
    float airDensity = 1.225f; // p ( kg/m³ )
    float bulletArea = 0.0001f; // A ( m² )

    Vector3 currentPosition;

    float startTime;
    bool isInitialized = false;

    public void Initialize(Vector3 startPosition, Vector3 startDirection, Vector2 wind, float mass, float velocity)
    {
        this.startPosition = startPosition;
        this.direction = startDirection;
        this.wind = wind;
        this.mass = mass;
        this.startVelocity = velocity;
        this.velocity = this.startVelocity;
        isInitialized = true;
        startTime = -1;
    }

    private void SimulateStep(float deltaTime)
    {
        // Beräkna enhetsvektor i riktning av velocity
        Vector3 e_v = velocityVector.normalized; //e_v bortsett från vind, med vind används istället relativeWind

        // Luftmotståndskoefficient
        float Cd = GetDragCoefficient(velocityVector.magnitude / 340.0f);

        // Total kraft: gravitation + drag + vind
        Vector3 windVelocity = new Vector3(wind.x, 0, wind.y);
        Vector3 relativeWind = windVelocity - velocityVector;
        float C_wind = 0.3f; // empirisk rimlig vind
        Vector3 windForce = 0.5f * airDensity * C_wind * bulletArea * relativeWind.sqrMagnitude * relativeWind.normalized;

        Vector3 dragForce = 0.5f * airDensity * bulletArea * Cd * velocityVector.sqrMagnitude * relativeWind.normalized; // Drag force: F_d = -½ * p * C_d * A * v² * e_v
        Vector3 gravityForce = mass * gravity * Vector3.down;

        Vector3 totalForce = gravityForce + dragForce + windForce;

        // Acceleration
        Vector3 acceleration = totalForce / mass;

        // Uppdatera velocity och position
        velocityVector += deltaTime * acceleration;
        currentPosition += deltaTime * velocityVector;

        // Uppdatera riktning och hastighet
        direction = velocityVector.normalized;
        velocity = velocityVector.magnitude;

        Debug.Log($"[Step] V: {velocity:F2} | Dir: {direction} | Drag: {dragForce} | WindF: {windForce} | Pos: {currentPosition}");
    }


    private float GetDragCoefficient(float velocityInMach) //referens
    {
        return (0.50f + (-0.63f / (1 + Mathf.Pow((float)Math.E, 9.57f * (velocityInMach - 0.96f)))) + 0.40f * (Mathf.Pow((float)Math.E, -0.27f * Mathf.Pow(velocityInMach + 0.33f, 2))));
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
        if (startTime < 0)
        {
            startTime = Time.time;
            currentPosition = startPosition;
            velocityVector = direction * startVelocity;
        }

        RaycastHit hit;
        float currentTime = Time.time - startTime;

        Vector3 prevPoint = currentPosition;

        // Simulera framåt ett steg i tiden
        SimulateStep(Time.fixedDeltaTime);

        Vector3 nextPoint = currentPosition;

        // Collision detection (två raycasts per steg)
        if (CastRayBetweenPoints(prevPoint, currentPosition, out hit))
        {
            OnHit(hit, currentPosition - prevPoint);
        }
    }


    private void Update()
    {
        if (!isInitialized || startTime < 0) return;

        float currentTime = Time.time - startTime;
        transform.position = currentPosition;
    }
}
