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
    public float maxDistance = 8f;            // a bullet "morre" após percorrer isso

    [Header("Animação")]
    [Tooltip("Se ativo, o frame do Animator é dirigido pela distância percorrida (0..1). Se desativo, o Animator toca normalmente no tempo.")]
    public bool animateByDistance = true;
    [Tooltip("Nome do state padrão no Animator com o clip da bala (use exatamente o nome do state).")]
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

        // Setup seguro do corpo
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

        // Se vamos dirigir pela distância, congelamos o Animator
        if (animateByDistance) anim.speed = 0f;
        else                   anim.speed = 1f;

        // Checagem de segurança: state existe?
        if (animateByDistance && anim && !anim.HasState(LAYER_BASE, animStateHash))
        {
            Debug.LogWarning($"[Bullet] Animator não tem state '{animStateName}'. Verifique o nome no prefab.", this);
        }
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
            // Posiciona o clip no ponto proporcional à distância
            anim.Play(animStateHash, LAYER_BASE, t);
        }

        // Morre ao atingir a distância
        if (dist >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Evite destruir quando tocar no próprio player (se layers colidirem)
        if (LayerMask.NameToLayer("Player") == other.gameObject.layer) return;
        Destroy(gameObject);
    }
}
