using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI2D : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;
    public Transform spriteRoot;
    public EnemyShoot2D shooter;
    public Animator animator;

    [Header("Ranges")]
    public float sightRange = 10f;
    public float attackRange = 6f; // ataca quando dist <= attackRange
    public float chaseRange  = 4f; // avança quando dist <= chaseRange

    [Header("Alinhamento vertical")]
    [Tooltip("Diferença máxima de Y para considerar 'mesma altura' (em unidades de mundo).")]
    public float verticalWindow = 0.6f; // ajuste fino: 0.4–0.8 costuma ficar bom

    [Header("Move")]
    public bool canWalk = true;
    public float walkSpeed = 1.8f;
    public float accel = 12f;

    [Header("Orientação")]
    public bool faceTarget = true;

    [Header("Debug")]
    public bool debugLogs = false;

    Rigidbody2D rb;
    SpriteRenderer sr;
    float desiredXVel;
    bool allowMove = true; // para “stun” do hurt

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        if (!spriteRoot) spriteRoot = transform;
        if (!animator)   animator   = spriteRoot.GetComponent<Animator>();

        // auto-descobrir o Shooter se não estiver setado no Inspector
        if (!shooter) shooter = GetComponentInChildren<EnemyShoot2D>(includeInactive: true);

        sr = spriteRoot.GetComponentInChildren<SpriteRenderer>();
        rb.freezeRotation = true;
    }

    public void EnableMovement(bool enable) => allowMove = enable;

    bool IsVerticallyAligned()
    {
        if (!target) return false;
        float dy = Mathf.Abs(target.position.y - transform.position.y);
        return dy <= verticalWindow;
    }

    void FixedUpdate()
    {
        if (!target)
        {
            desiredXVel = 0f;
            ApplyMoveAndAnim();
            return;
        }

        float dx   = target.position.x - rb.position.x;
        float dist = Mathf.Abs(dx);
        bool aligned = IsVerticallyAligned();

        // Agora tudo (ver/atacar/perseguir) depende de também estar alinhado na vertical
        bool inSight  = (dist <= sightRange)  && aligned;
        bool inAttack = (dist <= attackRange) && aligned;
        bool inChase  = (dist <= chaseRange)  && aligned;

        // Atirar (não depende de andar)
        if (inSight && inAttack && shooter)
        {
            if (debugLogs) Debug.Log($"[EnemyAI] Attack: dist={dist:0.00} aligned={aligned}");
            shooter.TryAttack();
        }

        // Caminhar só se permitido e dentro da janela
        if (allowMove && canWalk && inSight && inChase)
        {
            float dir = Mathf.Sign(dx);
            desiredXVel = dir * walkSpeed;
        }
        else
        {
            desiredXVel = 0f;
        }

        if (faceTarget && sr)
            sr.flipX = (dx < 0f);

        ApplyMoveAndAnim();
    }

    void ApplyMoveAndAnim()
    {
        float targetX = allowMove ? desiredXVel : 0f;
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        if (animator) animator.SetFloat("Speed", Mathf.Abs(newX));
    }

    public void OnHurt()
    {
        if (animator) animator.SetTrigger("Hurt");
    }

    public void OnDeath()
    {
        if (animator) animator.SetBool("dead", true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.cyan;   Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;Gizmos.DrawWireSphere(transform.position, chaseRange);

        // linhas verdes mostrando a janela vertical
        Gizmos.color = Color.green;
        Vector3 a = transform.position + Vector3.up * verticalWindow;
        Vector3 b = transform.position - Vector3.up * verticalWindow;
        Gizmos.DrawLine(a + Vector3.left*2f, a + Vector3.right*2f);
        Gizmos.DrawLine(b + Vector3.left*2f, b + Vector3.right*2f);
    }
}
