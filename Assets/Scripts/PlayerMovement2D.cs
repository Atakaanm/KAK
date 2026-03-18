using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Data (opsiyonel)")]
    public PlayerData playerData; // Atanirsa moveSpeed buradan alinir

    private Rigidbody2D rb;
    private Vector2 movementInput;

    public Vector2 MovementInput => movementInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Data varsa hizi oradan al
        if (playerData != null)
        {
            moveSpeed = playerData.moveSpeed;
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(x, y).normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }
}