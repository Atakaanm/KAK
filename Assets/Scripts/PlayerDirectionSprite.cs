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

    private Vector2 lastDirection = Vector2.down;
    private float animationTimer = 0f;
    private int currentRunFrame = 0;

    void Update()
    {
        if (movement == null || spriteRenderer == null)
            return;

        Vector2 input = movement.MovementInput;
        bool isMoving = input.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            lastDirection = input.normalized;
        }

        DirectionAnimation currentAnim = GetAnimationForDirection(lastDirection);

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

                if (animationTimer >= runFrameRate)
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

    DirectionAnimation GetAnimationForDirection(Vector2 dir)
    {
        dir.Normalize();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f) return east;
        if (angle >= 22.5f && angle < 67.5f) return northEast;
        if (angle >= 67.5f && angle < 112.5f) return north;
        if (angle >= 112.5f && angle < 157.5f) return northWest;
        if (angle >= 157.5f || angle < -157.5f) return west;
        if (angle >= -157.5f && angle < -112.5f) return southWest;
        if (angle >= -112.5f && angle < -67.5f) return south;
        if (angle >= -67.5f && angle < -22.5f) return southEast;

        return south;
    }
}