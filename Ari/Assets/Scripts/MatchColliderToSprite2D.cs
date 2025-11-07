using UnityEngine;

/// <summary>
/// Ajusta um BoxCollider2D para casar com o sprite atual (bom para projéteis animados).
/// Coloque este script NO MESMO OBJETO que tem o SpriteRenderer + BoxCollider2D.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MatchColliderToSprite2D : MonoBehaviour
{
    public bool updateEveryFrame = true;

    SpriteRenderer sr;
    BoxCollider2D box;

    void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();

        if (!sr.sprite)
            Debug.LogWarning("[MatchColliderToSprite2D] Sprite inicial vazio.", this);

        UpdateCollider();
    }

    void LateUpdate()
    {
        if (updateEveryFrame)
            UpdateCollider();
    }

    public void UpdateCollider()
    {
        if (!sr || !box || sr.sprite == null)
            return;

        // bounds do sprite em espaço local
        var b = sr.sprite.bounds;

        box.offset = b.center;
        box.size   = b.size;
    }
}
