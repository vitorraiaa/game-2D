using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bullet2D : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 18f;          // velocidade horizontal

    [Header("Vida por distância")]
    public float maxDistance = 8f;     // morre após percorrer essa distância em X
    [Tooltip("State do Animator que contém o clip das 9 sprites da bala.")]
    public string animStateName = "Bullet_Fly";

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    Vector2 startPos;
    Vector2 moveDir = Vector2.right;
    bool launched;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();

        // Configuração estável de física para projétil 2D lateral
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Evita qualquer empurrão vertical ao nascer/colidir
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
    }

    void OnEnable()
    {
        launched = false;
        startPos = rb.position;

        // Vamos dirigir o tempo do clip manualmente (distância → 0..1)
        if (anim)
        {
            anim.speed = 0f;                     // congela o avanço automático
            anim.Play(animStateName, 0, 0f);     // garante começar no primeiro frame
        }
    }

    /// <summary>Lança a bala numa direção (use (±1, 0)).</summary>
    public void Launch(Vector2 dir, float speedOverride = -1f)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        moveDir = dir.normalized;
        if (speedOverride > 0f) speed = speedOverride;

        launched = true;

        // Flip visual quando atira para a esquerda
        if (sr) sr.flipX = (moveDir.x < 0f);

        // Movimento horizontal puro
        rb.linearVelocity = new Vector2(moveDir.x * speed, 0f);
    }

    void FixedUpdate()
    {
        if (!launched) return;

        // Mantém a velocidade somente no X (Y sempre 0)
        rb.linearVelocity = new Vector2(moveDir.x * speed, 0f);

        // Progresso 0..1 baseado na distância horizontal percorrida
        float distX = Mathf.Abs(rb.position.x - startPos.x);
        float t = Mathf.Clamp01(distX / maxDistance);

        // Posiciona o clip no ponto proporcional à distância
        if (anim) anim.Play(animStateName, 0, t);

        // Morre ao atingir a distância máxima
        if (t >= 1f)
            Destroy(gameObject);
    }

    // Se usar collider como Trigger (recomendado para não empurrar nada):
    void OnTriggerEnter2D(Collider2D other)
    {
        // Evita destruir ao tocar no próprio player (se suas layers permitirem contato)
        int playerLayer = LayerMask.NameToLayer("Player");
        if (other.gameObject.layer == playerLayer) return;

        Destroy(gameObject);
    }

    // Caso use collider NÃO-Trigger, você pode usar esta alternativa:
    // void OnCollisionEnter2D(Collision2D col)
    // {
    //     if (col.collider.gameObject.layer == LayerMask.NameToLayer("Player")) return;
    //     Destroy(gameObject);
    // }
}
