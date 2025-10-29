using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bullet2D : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 18f;

    [Header("Vida por distância")]
    public float maxDistance = 8f; // a bullet "morre" após percorrer isso

    [Header("Alvo")]
    [Tooltip("Layer que a bala acerta (ex.: Enemy para bala do player, Player para bala do inimigo).")]
    public string hitLayer = "Enemy";
    public int damage = 1;

    [Header("Animação")]
    [Tooltip("Se ativo, o frame do Animator é dirigido pela distância percorrida (0..1).")]
    public bool animateByDistance = true;
    [Tooltip("Nome do state padrão no Animator com o clip da bala.")]
    public string animStateName = "Bullet_Fly";

    Rigidbody2D rb;
    Animator anim;

    Vector2 startPos;
    Vector2 moveDir = Vector2.right;
    bool launched;
    int animStateHash;
    const int LAYER_BASE = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Setup do corpo
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.None;

        animStateHash = Animator.StringToHash(animStateName);
    }

    void OnEnable()
    {
        launched = false;
        startPos = rb.position;

        anim.speed = animateByDistance ? 0f : 1f;

        if (animateByDistance && anim && !anim.HasState(LAYER_BASE, animStateHash))
            Debug.LogWarning($"[Bullet] Animator não tem state '{animStateName}'.", this);
    }

    /// Lança a bala numa direção (normalmente (±1,0)).
    public void Launch(Vector2 dir, float speedOverride = -1f)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        moveDir = dir.normalized;
        if (speedOverride > 0f) speed = speedOverride;

        launched = true;
        rb.linearVelocity = moveDir * speed;
    }

    void FixedUpdate()
    {
        if (!launched) return;

        // Mantém a velocidade constante na direção
        rb.linearVelocity = moveDir * speed;

        // Progresso por distância
        float dist = Vector2.Distance(startPos, rb.position);

        if (animateByDistance && anim)
        {
            float t = Mathf.Clamp01(dist / maxDistance);  // 0..1
            anim.Play(animStateHash, LAYER_BASE, t);
        }

        // Morre ao atingir a distância
        if (dist >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora se a layer não é o alvo
        if (other.gameObject.layer != LayerMask.NameToLayer(hitLayer))
        {
            return;
        }

        // Procura alguém que implementa IDamageable no alvo
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            // Ponto de impacto aproximado e "normal" na direção do voo
            Vector2 hitPoint = Physics2D.ClosestPoint(transform.position, other);
            Vector2 hitNormal = moveDir; // empurra na direção do voo

            damageable.ApplyDamage(damage, hitPoint, hitNormal);
        }

        Destroy(gameObject);
    }
}
