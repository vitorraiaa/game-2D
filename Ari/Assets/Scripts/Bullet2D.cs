using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet2D : MonoBehaviour
{
    [Header("Hit")]
    [Tooltip("Quais layers essa bala pode acertar (Player, Enemy, Boss etc).")]
    public LayerMask hitMask;
    public int damage = 1;
    public bool destroyOnHit = true;

    [Header("Movimento")]
    public float speed = 18f;
    public float maxDistance = 8f;

    [Header("Animação (opcional)")]
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
        col.isTrigger = true;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        anim = GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(animStateName))
            animStateHash = Animator.StringToHash(animStateName);
    }

    void OnEnable()
    {
        startPos = transform.position;
        traveled = 0f;

        if (animateByDistance && anim) anim.speed = 0f;
        else if (anim)                anim.speed = 1f;

        if (debugLogs)
        {
            Debug.Log($"[Bullet] Spawn em {transform.position}, layer={LayerMask.LayerToName(gameObject.layer)}, hitMask={hitMask.value}");
        }
    }

    public void Launch(Vector2 direction, float speedOverride = -1f)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        dir = direction.normalized;
        if (speedOverride > 0f) speed = speedOverride;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.Translate(dir * step, Space.World);
        traveled += step;

        if (animateByDistance && anim && maxDistance > 0.01f)
        {
            float t = Mathf.Clamp01(traveled / maxDistance);
            anim.Play(animStateHash, LAYER_BASE, t);
        }

        if (traveled >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        bool maskOk = (hitMask.value & (1 << otherLayer)) != 0;

        if (debugLogs)
        {
            Debug.Log($"[Bullet] Trigger com {other.name} (layer={LayerMask.LayerToName(otherLayer)}) maskOk={maskOk}");
        }

        if (!maskOk)
            return;

        bool hitSomething = false;

        // Inimigo comum
        var eh = other.GetComponentInParent<EnemyHealth>();
        if (eh)
        {
            eh.TakeDamage(damage);
            hitSomething = true;
        }

        // Boss
        var bh = other.GetComponentInParent<BossHealth>();
        if (bh)
        {
            bh.Damage(damage);
            hitSomething = true;
        }

        // Player (para balas inimigas)
        var ph = other.GetComponentInParent<PlayerHealth>();
        if (ph)
        {
            ph.TakeDamage(damage);
            hitSomething = true;
        }

        if (hitSomething && destroyOnHit)
            Destroy(gameObject);
    }
}
