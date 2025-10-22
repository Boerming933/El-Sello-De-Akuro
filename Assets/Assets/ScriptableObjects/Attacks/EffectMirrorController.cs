using UnityEngine;

public class EffectMirrorController : MonoBehaviour
{
    public bool shouldMirrorAnimation = false;  // Controls position/scale flip
    public bool shouldFlipSprite = false;        // Controls sprite visual flip

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            // Handle animation position/scale mirroring
            if (shouldMirrorAnimation)
            {
                Vector3 pos = transform.localPosition;
                pos.x = -pos.x;
                transform.localPosition = pos;

                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            else
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            // Handle sprite visual flipping (independent)
           // spriteRenderer.flipX = shouldFlipSprite;
        }
    }
}
