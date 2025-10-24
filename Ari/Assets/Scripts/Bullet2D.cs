using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet2D : MonoBehaviour
{
    public float speed = 18f;
    public float lifeTime = 2f;
    public int damage = 1;

    Rigidbody2D rb;
    Vector2 moveDir = Vector2.right;
    float timer;
    bool launched;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Garantias úteis
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.None;
    }

    void OnEnable()
    {
        timer = 0f;
        launched = false; // até chamar Launch
    }

    /// <summary>Lança a bala numa direção. speedOverride opcional.</summary>
    public void Launch(Vector2 dir, float speedOverride = -1f)
    {
        if (dir.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("[Bullet] Direção zero; usando Vector2.right");
            dir = Vector2.right;
        }

        moveDir = dir.normalized;
        if (speedOverride > 0f) speed = speedOverride;

        launched = true;
        rb.isKinematic = false; // só pra garantir
        rb.linearVelocity = moveDir * speed;
        // Debug:
        // Debug.Log($"[Bullet] Launch dir={moveDir} speed={speed}");
    }

    void FixedUpdate()
    {
        if (launched)
        {
            // mantém a velocidade constante
            rb.linearVelocity = moveDir * speed;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime) Destroy(gameObject);

        // Sanidade: se parado enquanto deveria mover, loga
        if (launched && rb.linearVelocity.sqrMagnitude < 0.0001f)
        {
            // Debug.LogWarning("[Bullet] Velocidade ~0; checar Rigidbody2D (BodyType/Constraints) e speed.");
        }
    }

    // Se o collider NÃO é Trigger, use Collision:
    void OnCollisionEnter2D(Collision2D col)
    {
        // if (col.collider.CompareTag("Enemy")) { /* aplicar dano */ }
        Destroy(gameObject);
    }

    // Se marcar IsTrigger no collider, comente o método acima e descomente este:
    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     // if (other.CompareTag("Enemy")) { /* aplicar dano */ }
    //     Destroy(gameObject);
    // }
}
