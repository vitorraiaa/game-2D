using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 3.5f;
    public float runSpeed  = 8f;
    public float aceleracaoNoChao = 60f;
    public float aceleracaoNoAr   = 40f;

    [Header("Pulo")]
    public float forcaPulo = 14f;
    public float coyoteTime   = 0.12f;
    public float jumpBuffer   = 0.12f;
    public float gravidadeDescida = 2.5f;  // mais pesado descendo
    public float gravidadeSubida  = 2.0f;  // corta subida ao SOLTAR o botão

    [Header("Limite de Altura do Pulo")]
    public bool  limitarAlturaDoPulo = true;
    [Tooltip("Tempo máx. (s) de SUBIDA do pulo normal.")]
    public float tempoMaxSubida = 0.12f;

    [Header("High Jump (via pickup)")]
    public bool  hasHighJumpPower = false; // setado pelo pickup
    public float highJumpForce    = 18f;
    [Tooltip("Tolera alguns ms após sair do chão para aceitar o high jump.")]
    public float highJumpCoyote   = 0.10f;
    [Tooltip("Janela em segundos em que NÃO cortamos a subida do high jump.")]
    public float highJumpNoCutTime = 0.15f;
    [Tooltip("Tempo máx. (s) de SUBIDA específico do high jump.")]
    public float tempoMaxSubidaHigh = 0.20f;

    [Header("Chão")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundMask;

    [Header("Escada (Climb)")]
    public string ladderLayerName = "Ladder";
    public float climbSpeed = 3.0f;
    public float descendExtra = 1.0f;
    public float climbExitHorizontalBoost = 0f;

    [Header("Animator (auto)")]
    public Animator animator;

    // — internos —
    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;

    int ladderLayer;
    bool emLadderZone = false;
    bool isClimbing   = false;

    float inputX;   // -1,0,1
    float inputY;   // vertical (escada)

    float coyoteTimer;
    float jumpBufferTimer;

    // controle do limite de subida
    float tempoDesdeInicioDoPulo;
    bool  emSubidaLimitada;
    bool  emHighJumpSubida;     // estamos na fase de SUBIDA de um high jump?
    float noCutTimer;           // janela anti-corte de subida (high jump)

    // fila de high jump (tecla Q)
    bool queuedHighJump = false;

    // Animator hashes
    int hSpeed, hIsGrounded, hYVelocity, hJump, hHighJump, hIsClimbing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (!animator) animator = GetComponent<Animator>();

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
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        ladderLayer = LayerMask.NameToLayer(ladderLayerName);

        hSpeed       = Animator.StringToHash("Speed");
        hIsGrounded  = Animator.StringToHash("IsGrounded");
        hYVelocity   = Animator.StringToHash("YVelocity");
        hJump        = Animator.StringToHash("Jump");
        hHighJump    = Animator.StringToHash("HighJump");
        hIsClimbing  = Animator.StringToHash("IsClimbing");
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (inputX != 0) sr.flipX = inputX < 0;

        // Jump buffer
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBuffer;
        jumpBufferTimer -= Time.deltaTime;

        // Enfileira High Jump ao apertar Q
        if (hasHighJumpPower && Input.GetKeyDown(KeyCode.Q))
            queuedHighJump = true;

        // Animator
        if (animator)
        {
            animator.SetFloat(hSpeed, Mathf.Abs(rb.linearVelocity.x));
            bool grounded = EstaNoChao();
            animator.SetBool (hIsGrounded, grounded);
            animator.SetFloat(hYVelocity, rb.linearVelocity.y);
            animator.SetBool (hIsClimbing, isClimbing);
        }
    }

    void FixedUpdate()
    {
        bool noChao = EstaNoChao();
        if (noChao) coyoteTimer = coyoteTime;
        else        coyoteTimer -= Time.fixedDeltaTime;

        // — CLIMB —
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            float y = inputY * climbSpeed;
            if (y < 0f) y *= (1f + descendExtra);
            float x = inputX * walkSpeed * 0.2f;
            rb.linearVelocity = new Vector2(x, y);
            return;
        }
        else rb.gravityScale = 1f;

        // Walk / Run
        bool runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = runHeld ? runSpeed : walkSpeed;

        // aceleração X
        float alvoX = inputX * targetSpeed;
        float acel  = noChao ? aceleracaoNoChao : aceleracaoNoAr;
        float novoX = Mathf.MoveTowards(rb.linearVelocity.x, alvoX, acel * Time.fixedDeltaTime);

        // — HIGH JUMP (prioridade) —
        if (queuedHighJump)
        {
            if (noChao || coyoteTimer > -highJumpCoyote)
            {
                queuedHighJump    = false;
                emHighJumpSubida  = true;
                noCutTimer        = highJumpNoCutTime;   // janela anti-corte
                tempoDesdeInicioDoPulo = 0f;             // reinicia janela de subida

                rb.linearVelocity = new Vector2(novoX, highJumpForce);
                if (animator) animator.SetTrigger(hHighJump);

                emSubidaLimitada = limitarAlturaDoPulo; // usamos limite, mas com tempo maior (abaixo)
                return;
            }
            // (Se quiser permitir high jump no ar, adicione aqui a variante aérea)
        }

        // — PULO NORMAL (space) —
        bool querPular = (jumpBufferTimer > 0f);
        bool podePularAgora = (coyoteTimer > 0f) && querPular;

        if (podePularAgora)
        {
            jumpBufferTimer = 0f;
            coyoteTimer     = 0f;

            emHighJumpSubida = false;      // é pulo normal
            noCutTimer       = 0f;         // sem janela anti-corte no pulo normal
            tempoDesdeInicioDoPulo = 0f;

            rb.linearVelocity = new Vector2(novoX, forcaPulo);
            if (animator) animator.SetTrigger(hJump);

            emSubidaLimitada = limitarAlturaDoPulo;
            return;
        }

        // — GRAVIDADE VARIÁVEL —
        if (rb.linearVelocity.y < 0f)
        {
            // caindo → mais pesado
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (gravidadeDescida - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f)
        {
            // Durante a janela anti-corte do HIGH JUMP, não aplicamos o short-hop cut
            bool podeCortarSubida = (noCutTimer <= 0f);
            if (podeCortarSubida && !Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (gravidadeSubida - 1f) * Time.fixedDeltaTime;
            }
        }

        // — LIMITE DE SUBIDA —
        if (limitarAlturaDoPulo && emSubidaLimitada && rb.linearVelocity.y > 0f)
        {
            tempoDesdeInicioDoPulo += Time.fixedDeltaTime;

            // tempo de subida alvo depende se é high jump ou não
            float limite = emHighJumpSubida ? tempoMaxSubidaHigh : tempoMaxSubida;

            bool soltouBotao    = !Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space);
            bool estourouJanela = tempoDesdeInicioDoPulo >= limite;

            // Enquanto houver noCutTimer (high jump), ignoramos o "soltouBotao"
            bool deveCortar = estourouJanela || (soltouBotao && noCutTimer <= 0f);

            if (deveCortar)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                emSubidaLimitada = false;
                emHighJumpSubida = false;
            }
        }

        if (rb.linearVelocity.y <= 0f)
        {
            emSubidaLimitada = false;
            emHighJumpSubida = false;
        }

        // aplica X mantendo Y
        rb.linearVelocity = new Vector2(novoX, rb.linearVelocity.y);

        // atualiza janela anti-corte (se ativa)
        if (noCutTimer > 0f)
        {
            noCutTimer -= Time.fixedDeltaTime;
            if (noCutTimer < 0f) noCutTimer = 0f;
        }
    }

    void LateUpdate()
    {
        // Liga/desliga climb conforme input vertical dentro da Ladder
        if (emLadderZone)
        {
            if (!isClimbing && Mathf.Abs(inputY) > 0.05f)
                isClimbing = true;
            else if (isClimbing && Mathf.Abs(inputY) <= 0.05f)
                isClimbing = false;
        }
        else isClimbing = false;
    }

    // — Escada (Trigger) —
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == ladderLayer)
            emLadderZone = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == ladderLayer)
        {
            emLadderZone = false;
            if (isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = 1f;
                if (climbExitHorizontalBoost != 0f)
                    rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * climbExitHorizontalBoost, rb.linearVelocity.y);
            }
        }
    }

    // — Utilitários —
    bool EstaNoChao()
    {
        if (groundCheck)
        {
            if (Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask))
                return true;
        }
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
