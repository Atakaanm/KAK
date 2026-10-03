using UnityEngine;

public class PlayerDirectionSprite : MonoBehaviour
{
    [System.Serializable]
    public class DirectionAnimation
    {
        public Sprite idle;
        public Sprite[] runFrames;
    }

    public PlayerMovement2D movement;
    public SpriteRenderer spriteRenderer;

    [Header("Direction Animations")]
    public DirectionAnimation north;
    public DirectionAnimation south;
    public DirectionAnimation east;
    public DirectionAnimation west;
    public DirectionAnimation northEast;
    public DirectionAnimation northWest;
    public DirectionAnimation southEast;
    public DirectionAnimation southWest;

    [Header("Animation Settings")]
    public float runFrameRate = 0.12f;
    [Tooltip("runFrameRate bu hızda (birim/sn) geçerli; daha hızlı koşunca kareler hızlanır, yavaşta yavaşlar")]
    public float referenceSpeed = 4f;
    [Tooltip("Yön sınırında titreme payı (derece)")]
    public float directionHysteresis = 12f;

    private Vector2 lastDirection = Vector2.down;
    private int octant = 6; // Güney
    private float animationTimer = 0f;
    private int currentRunFrame = 0;

    void Update()
    {
        if (movement == null || spriteRenderer == null)
            return;
        if (movement.Frozen) return; // ölüm: son karede kal

        Vector2 input = movement.MovementInput;
        bool isMoving = input.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            lastDirection = input.normalized;
            octant = DirectionUtil.OctantSticky(lastDirection, octant, directionHysteresis);
        }

        DirectionAnimation currentAnim = DirectionUtil.PickOctant(octant, east, northEast, north, northWest, west, southWest, south, southEast);

        if (currentAnim == null)
            return;

        if (!isMoving)
        {
            currentRunFrame = 0;
            animationTimer = 0f;
            spriteRenderer.sprite = currentAnim.idle;
        }
        else
        {
            if (currentAnim.runFrames != null && currentAnim.runFrames.Length > 0)
            {
                animationTimer += Time.deltaTime;
                // Kare hızı gerçek koşu hızına bağlı (hız güçlendirmesi, zemin, yavaşlama)
                float push = Mathf.Clamp01(input.magnitude); // Faz 12 H5: joystick az itilince yavaş yürür, bacaklar da yavaş
                float speedRatio = Mathf.Clamp(movement.CurrentSpeed * push / Mathf.Max(0.1f, referenceSpeed), 0.45f, 1.6f);
                float frameTime = runFrameRate / speedRatio;

                if (animationTimer >= frameTime)
                {
                    animationTimer = 0f;
                    currentRunFrame++;

                    if (currentRunFrame >= currentAnim.runFrames.Length)
                    {
                        currentRunFrame = 0;
                    }
                }

                spriteRenderer.sprite = currentAnim.runFrames[currentRunFrame];
            }
            else
            {
                spriteRenderer.sprite = currentAnim.idle;
            }
        }
    }

}