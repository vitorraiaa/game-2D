using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossMeleeHit2D : MonoBehaviour
{
    [Header("Config")]
    public int damage = 1;
    public float lifetime = 0.15f;
    public LayerMask targetMask;   // coloque a layer do Player aqui (via Inspector)

    void Awake()
    {
        // garantir trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // só acerta quem está na máscara configurada
        if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        // MESMO método que você usou no LightningHit2D
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            // use exatamente o nome correto aqui.
            // Se no LightningHit2D você usa hp.TakeDamage(damage);
            // use o MESMO:
            hp.TakeDamage(damage);
        }
    }
}
