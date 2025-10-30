using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet2D : MonoBehaviour
{
    [Header("Hit")]
    public LayerMask hitMask;      // defina no prefab OU via Shooter
    public int damage = 1;
    public bool destroyOnHit = true;

    [Header("Move")]
    public float speed = 18f;
    public float maxDistance = 8f;

    [Header("Anim (opcional)")]
    public bool animateByDistance = true;
    public string animStateName = "Bullet_Fly";

    [Header("Debug")]
    public bool debugLogs = false;

    Vector2 startPos;
    Vector2 dir = Vector2.right;
    float traveled;
    Animator anim;
    int animStateHash;
    const int LAYER_BASE = 0;

    Collider2D col;
    Rigidbody2D rb;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;              // NÃO empurra

        // Garante um RB2D cinemático pra física rastrear o Translate
        rb = GetComponent<Rigidbody2D>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        anim = GetComponent<Animator>();
        animStateHash = Animator.StringToHash(animStateName);
    }

    void OnEnable()
    {
        startPos = transform.position;
        traveled = 0f;
        if (animateByDistance && anim) anim.speed = 0f; else if (anim) anim.speed = 1f;
        if (debugLogs) Debug.Log($"[Bullet] Spawn layer={LayerMask.LayerToName(gameObject.layer)} hitMask={hitMask.value}");
    }

    public void Launch(Vector2 direction, float speedOverride = -1f)
    {
        dir = direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;
        if (speedOverride > 0f) speed = speedOverride;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.Translate(dir * step, Space.World);
        traveled += step;

        if (animateByDistance && anim)
        {
            float t = Mathf.Clamp01(traveled / maxDistance);
            anim.Play(animStateHash, LAYER_BASE, t);
        }

        if (traveled >= maxDistance) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se a matriz de colisão bloquear, este método NEM É chamado.
        // Se entrou aqui, checamos o mask.
        int otherLayer = other.gameObject.layer;
        bool maskOk = (hitMask.value & (1 << otherLayer)) != 0;

        if (debugLogs) Debug.Log($"[Bullet] Trigger with {other.name} (layer={LayerMask.LayerToName(otherLayer)}) maskOk={maskOk}");

        if (!maskOk) return;

        // Tenta causar dano
        bool hitSomething = false;

        var eh = other.GetComponentInParent<EnemyHealth>();
        if (eh) { eh.TakeDamage(damage); hitSomething = true; }

        var ph = other.GetComponentInParent<PlayerHealth>();
        if (ph) { ph.TakeDamage(damage); hitSomething = true; }

        if (hitSomething && destroyOnHit) Destroy(gameObject);
    }
}
