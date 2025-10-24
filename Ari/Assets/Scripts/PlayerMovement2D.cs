using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 8f;
    public float aceleracaoNoChao = 60f;
    public float aceleracaoNoAr = 40f;

    [Header("Pulo")]
    public float forcaPulo = 14f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    public float gravidadeDescida = 2.5f;
    public float gravidadeSubida = 2.0f;

    [Header("Chão")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundMask;

    [Header("Animator (auto)")]
    public Animator animator; // será preenchido automaticamente

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;

    float inputX;
    float coyoteTimer;
    float jumpBufferTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // >>> AUTO-LINK DO ANIMATOR <<<
        if (!animator) animator = GetComponent<Animator>();

        // Auto-setup GroundCheck se faltar
        if (!groundCheck)
        {
            var found = transform.Find("GroundCheck");
            if (found) groundCheck = found;
            else
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, -0.6f, 0f);
                groundCheck = go.transform;
            }
        }

        if (groundMask == 0) groundMask = LayerMask.GetMask("Ground");
        rb.freezeRotation = true;
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBuffer;

        jumpBufferTimer -= Time.deltaTime;

        if (inputX != 0) sr.flipX = inputX < 0;

        // >>> ATUALIZA OS PARÂMETROS DO ANIMATOR <<<
        if (animator)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", EstaNoChao());
            animator.SetFloat("YVelocity", rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        bool noChao = EstaNoChao();
        if (noChao) coyoteTimer = coyoteTime; else coyoteTimer -= Time.fixedDeltaTime;

        float alvoX = inputX * velocidade;
        float acel = noChao ? aceleracaoNoChao : aceleracaoNoAr;
        float novoX = Mathf.MoveTowards(rb.linearVelocity.x, alvoX, acel * Time.fixedDeltaTime);

        bool podePularAgora = (coyoteTimer > 0f) && (jumpBufferTimer > 0f);
        if (podePularAgora)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            rb.linearVelocity = new Vector2(novoX, forcaPulo);
            if (animator) animator.SetTrigger("Jump");
            return;
        }

        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (gravidadeDescida - 1f) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0f && !Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (gravidadeSubida - 1f) * Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(novoX, rb.linearVelocity.y);
    }

    bool EstaNoChao()
    {
        if (groundCheck)
            if (Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask))
                return true;

        if (col)
        {
            Bounds b = col.bounds;
            float extra = 0.05f;
            var hit = Physics2D.BoxCast(b.center, b.size, 0f, Vector2.down, extra, groundMask);
            if (hit.collider != null) return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
