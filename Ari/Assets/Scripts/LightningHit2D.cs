using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LightningHit2D : MonoBehaviour
{
    public int damage = 1;
    public LayerMask playerMask;
    bool hit;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        if (playerMask == 0)
            playerMask = LayerMask.GetMask("Player");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hit) return;
        if (((1 << other.gameObject.layer) & playerMask) == 0)
            return;

        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);   // garante que bate no método certo
            hit = true;
        }
    }
}
