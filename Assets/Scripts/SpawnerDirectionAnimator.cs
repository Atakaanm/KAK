using System.Collections;
using UnityEngine;

public class SpawnerDirectionAnimator : MonoBehaviour
{
    [System.Serializable]
    public class DirectionAnimation
    {
        public Sprite idle;
        public Sprite[] attackFrames;
    }

    public Transform target;
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
    public float attackFrameRate = 0.08f;
    
    // Merminin kaçıncı frame'de çıkması gerektiğini senkronize etmek için opsiyonel
    // Şu an için animasyon bitesiye kadar kendi kendine akacak.
    
    private Vector2 lastDirection = Vector2.down;
    private bool isAttacking = false;

    void Update()
    {
        if (target == null || spriteRenderer == null || isAttacking)
            return;

        Vector2 dir = target.position - transform.position;
        if (dir.sqrMagnitude > 0.01f)
        {
            lastDirection = dir.normalized;
        }

        // Saldırı durumunda değilsek hedefe bakarak Idle kal
        DirectionAnimation currentAnim = GetAnimationForDirection(lastDirection);
        if (currentAnim != null && currentAnim.idle != null)
        {
            spriteRenderer.sprite = currentAnim.idle;
        }
    }

    public void PlayAttackVisual()
    {
        if (!isAttacking && gameObject.activeInHierarchy)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        
        DirectionAnimation currentAnim = GetAnimationForDirection(lastDirection);

        if (currentAnim != null && currentAnim.attackFrames != null && currentAnim.attackFrames.Length > 0)
        {
             for (int i = 0; i < currentAnim.attackFrames.Length; i++)
             {
                 spriteRenderer.sprite = currentAnim.attackFrames[i];
                 yield return new WaitForSeconds(attackFrameRate);
             }
        }
        else
        {
             // Eğer attack frame'leri yoksa eskisi gibi kısa bir bekleme yap
             yield return new WaitForSeconds(0.15f);
        }

        isAttacking = false;
    }

    void OnDisable()
    {
        isAttacking = false;
        StopAllCoroutines();
    }

    DirectionAnimation GetAnimationForDirection(Vector2 dir)
    {
        return DirectionUtil.Pick(dir, east, northEast, north, northWest, west, southWest, south, southEast);
    }
}