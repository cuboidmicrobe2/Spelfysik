using TMPro;
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

    public static Player Instance;

    [SerializeField] private TextMeshProUGUI bulletUIInfotext;

    public Bullet lastBulletFire;
    public Vector3 lastPosition;
    bool isFollowingBullet = false;
    float followDieCooldown = 2.0f;

    float bulletMass = 0.008f;
    float bulletSpeed = 715.0f; 
    Vector2 windDirection = new Vector2(10.0f, 0.0f);

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
        GetSwapInput();

        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        transform.localRotation = Quaternion.Euler(0, turn.x, 0);
        cam.transform.localRotation = Quaternion.Euler(-turn.y, 0, 0);

        if (!isFollowingBullet)
        {
            // Movement for player/camera
            //Vector3 deltaMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * moveSpeed * Time.deltaTime;
            //transform.Translate(deltaMove);

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

            if (Input.GetKeyDown(KeyCode.F)) // UNFOLLOW BULLET
            {
                transform.position = lastPosition;
                followDieCooldown = 2.0f;
                isFollowingBullet = false;
            }
        }

        if (lastBulletFire != null)
        {
            bulletUIInfotext.text = lastBulletFire.GetUIInfo();
        }
    }

    private void GetSwapInput()
    {
        // Swap Weapon/Bullet info
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Glock-18
        {
            bulletMass = 0.0075f;
            bulletSpeed = 375.0f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // AK-47
        {
            bulletMass = 0.008f;
            bulletSpeed = 715.0f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) // H&K G36C
        {
            bulletMass = 0.004f;
            bulletSpeed = 600.0f;
        }
        // Swap Vind
        if (Input.GetKeyDown(KeyCode.Alpha7)) // 10 sekundmeter i x-riktning
        {
            windDirection = new Vector2(10.0f, 0.0f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8)) // -20 sekundmeter i x-riktning
        {
            windDirection = new Vector2(-20.0f, 0.0f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9)) // 30 sekundmeter i x-riktning
        {
            windDirection = new Vector2(30.0f, 0.0f);
        }
    }

    private void Shoot()
    {
        Vector3 startPosition = cam.transform.position;
        Vector3 startDirection = cam.transform.forward;

        Bullet bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity).GetComponent<Bullet>(); // Skapa en bullet i Scenen + skapa temporär referens
        bullet.Initialize(startPosition, startDirection, windDirection, bulletMass, bulletSpeed);

        lastBulletFire = bullet; // Spara referens till senast utskjutna bullet
        Destroy(bullet, 30.0f); // Delete bullet if not hit anything for 30 sec
    }

}
