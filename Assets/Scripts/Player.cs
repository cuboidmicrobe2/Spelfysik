using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 turn;
    public float sensitivity = 0.5f;
    public Vector3 deltaMove;
    public float speed = 1f;
    public GameObject mover;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        mover.transform.localRotation = Quaternion.Euler(0, turn.x, 0);
        transform.localRotation = Quaternion.Euler(-turn.y, 0, 0);

        deltaMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * speed * Time.deltaTime;
        mover.transform.Translate(deltaMove);

    }
}
