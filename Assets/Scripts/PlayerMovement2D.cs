using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Data (opsiyonel)")]
    public PlayerData playerData;

    [Header("Mobil Kontrol")]
    public VirtualJoystick joystick; // Inspector'dan baglanir

    private Rigidbody2D rb;
    private Vector2 movementInput;

    public Vector2 MovementInput => movementInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (playerData != null)
        {
            moveSpeed = playerData.moveSpeed;
        }
    }

    void Update()
    {
        // Oncelik joystick'te: eger joystick bagli ve input varsa onu kullan
        if (joystick != null && joystick.Direction.sqrMagnitude > 0.01f)
        {
            movementInput = joystick.Direction;
        }
        else
        {
            // Klavye inputu (editorde test icin)
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            movementInput = new Vector2(x, y).normalized;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }
}