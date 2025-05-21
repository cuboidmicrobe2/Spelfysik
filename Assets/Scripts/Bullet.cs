using System;
using UnityEngine;

public struct HitInfo
{
    public RaycastHit hit;
    public Vector3 hitDirection;
    public float hitSpeed;
    public Bullet bullet;
}

public class Bullet : MonoBehaviour
{
    Vector3 startPosition;
    Vector3 startDirection; 
    Vector2 wind;
    Vector3 direction; // e_v
    Vector3 velocityVector; // v * e_v

    Vector3 dragForce; // F_d ( N )
    Vector3 windForce; // F_wind ( N )
    Vector3 gravityForce; // mg ( kgm/s² )

    float mass; // m ( kg )
    float startVelocity; // v_0  ( m/s )
    float velocity; // v  ( m/s )
    float C_d; // C_d

    float gravity = 9.82f; // g  ( m/s² )
    float airDensity = 1.225f; // p ( kg/m³ )
    float bulletArea = 0.0001f; // A ( m² )

    float startTime;
    Vector3 currentPosition;
    Vector3 previousPosition;
    bool isInitialized = false;

    public void Initialize(Vector3 startPosition, Vector3 startDirection, Vector2 wind, float mass, float velocity)
    {
        this.startPosition = startPosition;
        this.startDirection = startDirection;
        this.direction = startDirection;
        this.wind = wind;
        this.mass = mass;
        this.startVelocity = velocity;
        this.velocity = this.startVelocity;
        isInitialized = true;
        startTime = -1;
    }

    public string GetUIInfo()
    {
        string infoString = $"Start Values:\n" + 
                            $"V_0 (m/s):  {startVelocity:F2}\n" +
                            $"Massa (kg): {mass:F4}\n" +
                            $"Start Dir:  {startDirection}\n" +
                            $"Start Pos:  {startPosition:F0}\n" +
                            $"Wind:       {wind}\n" +
                            $"\nDynamic Values:\n" +
                            $"V (m/s):    {velocity:F2}\n" +
                            $"V_dir:      {direction}\n" +
                            $"C_d:        {C_d:F5}\n" +
                            $"F_drag:     {dragForce}\n" +
                            $"F_wind:     {windForce}\n" +
                            $"Position:   {currentPosition:F0}";
        
        return infoString;
    }

    private void SimulateStep(float deltaTime)
    {
        // Beräkna enhetsvektor i riktning av velocity,
        // (behövs inte längre pga vind används istället)
        //Vector3 e_v = velocityVector.normalized;

        // Luftmotståndskoefficient
        C_d = GetDragCoefficient(velocityVector.magnitude / 340.0f);

        // Total kraft: gravitation + drag + vind
        Vector3 windVelocity = new Vector3(wind.x, 0, wind.y);
        Vector3 relativeWind = windVelocity - velocityVector; 
        float C_wind = 0.3f; // empirisk rimlig vind 
        windForce = 0.5f * airDensity * C_wind * bulletArea * relativeWind.sqrMagnitude * relativeWind.normalized; // Wind force: F_d = -½ * p * C_wind * A * v² * e_v

        gravityForce = mass * gravity * Vector3.down; // m * g * (0, 0, -1)
        dragForce = 0.5f * airDensity * C_d * bulletArea * velocityVector.sqrMagnitude * relativeWind.normalized; // Drag force: F_d = -½ * p * C_d * A * v² * e_v

        Vector3 totalForce = gravityForce + dragForce + windForce; 

        // Acceleration
        Vector3 acceleration = totalForce / mass;      // a = F/m

        // Uppdatera velocity och position
        velocityVector += deltaTime * acceleration;    // v_i+1 = v_i + dt * a
        currentPosition += deltaTime * velocityVector; // r_i+1 = r_i + dt * v

        // Uppdatera riktning och hastighet
        direction = velocityVector.normalized;
        velocity = velocityVector.magnitude;
    }


    private float GetDragCoefficient(float velocityInMach) 
    {
        // Funktionen är skapad efter en G1 standard projectile (Figure 1) från:
        // https://appliedballisticsllc.com/wp-content/uploads/2021/06/Aerodynamic-Drag-Modeling-for-Ballistics.pdf 

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

        Destroy(gameObject); // Deleting bullet on hit
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;
        if (startTime < 0)
        {
            startTime = Time.time;
            currentPosition = startPosition;
            previousPosition = currentPosition;
            velocityVector = direction * startVelocity;
        }

        RaycastHit hit;
        Vector3 oldPosition = previousPosition;
        previousPosition = currentPosition;

        // Simulera framåt ett steg i tiden
        SimulateStep(Time.fixedDeltaTime);

        // Collision detection (två raycasts per steg)
        if (CastRayBetweenPoints(oldPosition, previousPosition, out hit))
        {
            OnHit(hit, previousPosition - oldPosition);
        }
        if (CastRayBetweenPoints(previousPosition, currentPosition, out hit))
        {
            OnHit(hit, currentPosition - previousPosition);
        }
    }


    private void Update()
    {
        if (!isInitialized || startTime < 0) return;

        float currentTime = Time.time - startTime;
        transform.position = currentPosition;
    }
}
