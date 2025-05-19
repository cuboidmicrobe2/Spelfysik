using System;
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
    Vector3 direction; // e_v
    Vector2 wind;
    float mass; // m ( kg )
    float startVelocity; // v_0  ( m/s )
    float velocity; // v  ( m/s )
    Vector3 tempV;
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

    //  THIS IS BASICLY WHAT MATTIAS CARES ABOUT :)
    //  
    //  Denna funktionen ger ut en vector från origo för positionen som 
    //  skåttet befinner sig vid på en viss tidspunkt (time) efter utskjutning

    //private Vector3 FindPointOnParabola(float time) 
    //{
    //    float tempC = GetDragCoefficient(velocity / 340.0f);
    //    Vector3 movementVector = tempC * velocity * velocity * direction; // c * v^2 * e_v
    //    Vector3 gravityAdjustment = mass * gravity * Vector3.down * time * time; // m * g * e_v
    //    //Vector3 windAdjustment = new Vector3(wind.x, 0, wind.y) * time * time;
    //
    //    Vector3 result = movementVector + gravityAdjustment; // + windAdjustment;
    //    Debug.Log("velocity: " + velocity + " dir: " + direction + " movementVec: " + movementVector + " Coefficient: " + tempC + " result.mag: " + result.magnitude + " gravtiyAdj: " + gravityAdjustment + " Result: " + result);
    //
    //    tempV.Normalize();
    //    tempV = tempV + Time.fixedDeltaTime * result / mass;
    //    velocity = tempV.magnitude;
    //    direction = tempV.normalized;
    //
    //    return startPosition + result;
    //}

    private void SimulateStep(float deltaTime)
    {
        // Beräkna enhetsvektor i riktning av velocity
        Vector3 e_v = tempV.normalized;

        // Luftmotståndskoefficient
        float tempC = GetDragCoefficient(tempV.magnitude / 340.0f);
        if (tempC < 0f)
        {
            Debug.LogWarning("Negative drag coefficient detected! Clamping to zero.");
            tempC = 0f;
        }

        // Total kraft: gravitation + drag + vind
        Vector3 dragForce = -0.5f * airDensity * bulletArea * tempC * tempV.sqrMagnitude * e_v; // Drag force: F_d = -½ * p * C_d * A * v² * e_v
        Vector3 gravityForce = mass * gravity * Vector3.down;

        Vector3 windVelocity = new Vector3(wind.x, 0, wind.y);
        Vector3 relativeWind = windVelocity - tempV;
        float C_wind = 0.3f; // empirisk rimlig vind
        Vector3 windForce = 0.5f * airDensity * C_wind * bulletArea * relativeWind.sqrMagnitude * relativeWind.normalized;

        Vector3 totalForce = gravityForce + dragForce + windForce;

        // Acceleration
        Vector3 acceleration = totalForce / mass;

        // Uppdatera velocity och position
        tempV += deltaTime * acceleration;
        currentPosition += deltaTime * tempV;

        // Uppdatera riktning och hastighet
        direction = tempV.normalized;
        velocity = tempV.magnitude;

        Debug.Log($"[Step] V: {velocity:F2} | Dir: {direction} | Drag: {dragForce} | WindF: {windForce} | Pos: {currentPosition}");
    }


    private float GetDragCoefficient(float velocityInMach)
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

    //private void FixedUpdate()
    //{
    //    if (!isInitialized) return;
    //    if (startTime < 0) startTime = Time.time;
    //
    //    RaycastHit hit;
    //    float currentTime = Time.time - startTime;
    //    float prevTime = currentTime - Time.fixedDeltaTime;
    //    float nextTime = currentTime + Time.fixedDeltaTime;
    //
    //    Vector3 currentPoint = FindPointOnParabola(currentTime);
    //    Vector3 nextPoint = FindPointOnParabola(nextTime);
    //
    //    if (prevTime > 0)
    //    {
    //        Vector3 prevPoint = FindPointOnParabola(prevTime);
    //
    //        if (CastRayBetweenPoints(prevPoint, currentPoint, out hit))
    //        {
    //            OnHit(hit, currentPoint - prevPoint);
    //        }
    //    }
    //
    //    if (CastRayBetweenPoints(currentPoint, nextPoint, out hit))
    //    {
    //        OnHit(hit, nextPoint - currentPoint);
    //    }
    //}
    private void FixedUpdate()
    {
        if (!isInitialized) return;
        if (startTime < 0)
        {
            startTime = Time.time;
            currentPosition = startPosition;
            tempV = direction * startVelocity;
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
        //Vector3 currentPoint = FindPointOnParabola(currentTime);
        transform.position = currentPosition;
    }
}
