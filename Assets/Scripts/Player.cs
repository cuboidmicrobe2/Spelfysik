using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 turn;
    public float sensitivity;
    public float scopeSensitivity;
    public float baseSensitivity;
    public float moveSpeed;
    public GameObject cam;

    public bool showScope = false;

    public GameObject bulletPrefab;

    public WindManager windManager;

    public static Player Instance;

    public Bullet lastBulletFire;
    public Vector3 lastPosition;
    bool isFollowingBullet = false;
    float followDieCooldown = 2.0f;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //sensitivity = (showScope) ? scopeSensitivity : baseSensitivity; 

        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        transform.localRotation = Quaternion.Euler(0, turn.x, 0);
        cam.transform.localRotation = Quaternion.Euler(-turn.y, 0, 0);

        if (!isFollowingBullet)
        {
            Vector3 deltaMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * moveSpeed * Time.deltaTime;
            transform.Translate(deltaMove);

            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
            if (Input.GetKeyDown(KeyCode.F)) // FOLLOW BULLET
            {
                lastPosition = transform.position;
                isFollowingBullet = true;
            }
        }
        else if (isFollowingBullet)
        {
            if (lastBulletFire != null)
            {
                transform.position = new Vector3(lastBulletFire.transform.position.x, lastBulletFire.transform.position.y + 0.5f, lastBulletFire.transform.position.z - 1);
            }
            else
            {
                followDieCooldown -= Time.deltaTime;
                if (followDieCooldown < 0)
                {
                    transform.position = lastPosition;
                    followDieCooldown = 2.0f;
                    isFollowingBullet = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.F)) // UN-FOLLOW BULLET
            {
                transform.position = lastPosition;
                followDieCooldown = 2.0f;
                isFollowingBullet = false;
            }
        }
    }

    private void Shoot()
    {
        Vector3 startPosition = cam.transform.position;
        Vector3 startDirection = cam.transform.forward;
        Vector2 windDirection = windManager.GetWind();
        float bulletSpeed = 200.0f; 

        Bullet bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity).GetComponent<Bullet>(); // Skapa en bullet i Scenen + skapa temporär referens
        
        bullet.Initialize(startPosition, startDirection, windDirection, bulletSpeed);
        
        lastBulletFire = bullet;
        Destroy(bullet, 20.0f); // Delete bullet if not hit anything for 20 sec
    }

}
