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
        return DirectionUtil.Pick(dir, east, northEast, north, northWest, west, southWest, south, southEast);
    }
}