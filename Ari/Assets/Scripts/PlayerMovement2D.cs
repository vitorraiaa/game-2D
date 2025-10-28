using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 3.5f;          // andar (sem Shift)
    public float runSpeed  = 8f;            // correr (Shift)
    public float aceleracaoNoChao = 60f;
    public float aceleracaoNoAr   = 40f;

    [Header("Pulo")]
    public float forcaPulo = 14f;
    public float coyoteTime   = 0.12f;      // tolerância depois de sair do chão
    public float jumpBuffer   = 0.12f;      // tolerância antes de tocar o chão
    public float gravidadeDescida = 2.5f;   // mais pesado descendo
    public float gravidadeSubida  = 2.0f;   // corta subida ao soltar o botão

    [Header("Limite de Altura do Pulo")]
    public bool  limitarAlturaDoPulo = true;
    [Tooltip("Tempo máximo (s) que continua SUBINDO após iniciar o pulo.")]
    public float tempoMaxSubida = 0.12f;

    [Header("High Jump (power-up)")]
    public bool  hasHighJumpPower = false;  // deve ser ligado por power-up
    public float highJumpForce    = 18f;
    [Tooltip("Permite usar o high jump alguns ms após sair do chão.")]
    public float highJumpCoyote   = 0.10f;

    [Header("Chão")]
    public Transform groundCheck;           // filho “GroundCheck”
    public float groundCheckRadius = 0.12f;
    public LayerMask groundMask;

    [Header("Escada (Climb)")]
    public string ladderLayerName = "Ladder";
    public float climbSpeed = 3.0f;         // velocidade na escada
    public float descendExtra = 1.0f;       // multiplicador ao segurar S/↓
    public float climbExitHorizontalBoost = 0f; // impulso X ao sair (opcional)

    [Header("Animator (auto)")]
    public Animator animator;

    // — internos —
    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;

    int ladderLayer;          // índice da layer Ladder
    bool emLadderZone = false;
    bool isClimbing    = false;

    float inputX;             // -1, 0, 1
    float inputY;             // escada

    float coyoteTimer;
    float jumpBufferTimer;

    // controle do limite de subida
    float tempoDesdeInicioDoPulo;
    bool  emSubidaLimitada;

    // high jump no ar (um por voo)
    bool canAirHighJump = false;
    bool wasGrounded    = false;

    // Animator hashes
    int hSpeed, hIsGrounded, hYVelocity, hJump, hHighJump, hIsClimbing;

    // ———————————————————————————————————————————————————————————

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (!animator) animator = GetComponent<Animator>();

        // GroundCheck automático, se faltar
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

        // Rigidbody2D base
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Ladder layer
        ladderLayer = LayerMask.NameToLayer(ladderLayerName);

        // Animator hashes
        hSpeed       = Animator.StringToHash("Speed");
        hIsGrounded  = Animator.StringToHash("IsGrounded");
        hYVelocity   = Animator.StringToHash("YVelocity");
        hJump        = Animator.StringToHash("Jump");
        hHighJump    = Animator.StringToHash("HighJump");
        hIsClimbing  = Animator.StringToHash("IsClimbing");
    }

    void Update()
    {
        // leitura de inputs
        inputX = Input.GetAxisRaw("Horizontal");    // -1,0,1
        inputY = Input.GetAxisRaw("Vertical");      // usado na escada

        // vira sprite
        if (inputX != 0) sr.flipX = inputX < 0;

        // buffer de pulo (Jump/Space)
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBuffer;
        jumpBufferTimer -= Time.deltaTime;

        // Animator (sempre alimenta)
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

        // reset por toque no chão (libera 1 high jump aéreo por voo)
        if (noChao && !wasGrounded)
            canAirHighJump = hasHighJumpPower;
        wasGrounded = noChao;

        if (noChao) coyoteTimer = coyoteTime;
        else        coyoteTimer -= Time.fixedDeltaTime;

        // — CLIMB (escada) tem prioridade quando ativo —
        if (isClimbing)
        {
            rb.gravityScale = 0f;

            float y = inputY * climbSpeed;
            if (y < 0f) y *= (1f + descendExtra);

            // controle horizontal leve (opcional: zere se quiser)
            float x = inputX * walkSpeed * 0.2f;

            rb.linearVelocity = new Vector2(x, y);
            return; // não processa físico “terrestre”
        }
        else
        {
            // fora da escada → gravidade normal
            rb.gravityScale = 1f;
        }

        // Walk por padrão; Run com Shift
        bool runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = runHeld ? runSpeed : walkSpeed;

        // aceleração suave no X
        float alvoX = inputX * targetSpeed;
        float acel  = noChao ? aceleracaoNoChao : aceleracaoNoAr;
        float novoX = Mathf.MoveTowards(rb.linearVelocity.x, alvoX, acel * Time.fixedDeltaTime);

        // pulo (coyote + buffer)
        bool querPular = (jumpBufferTimer > 0f);
        bool podePularAgora = (coyoteTimer > 0f) && querPular;

        if (podePularAgora)
        {
            jumpBufferTimer = 0f;
            coyoteTimer     = 0f;

            rb.linearVelocity = new Vector2(novoX, forcaPulo);
            if (animator) animator.SetTrigger(hJump);

            // inicia janela de subida limitada
            if (limitarAlturaDoPulo)
            {
                emSubidaLimitada = true;
                tempoDesdeInicioDoPulo = 0f;
            }
            return; // já aplicamos pulo neste frame
        }

        // Gravidade variável para sensação melhor de pulo
        if (rb.linearVelocity.y < 0f)
        {
            // caindo → mais pesado
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (gravidadeDescida - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space))
        {
            // subindo mas soltou o botão → corta ascensão
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (gravidadeSubida - 1f) * Time.fixedDeltaTime;
        }

        // Limita a duração da SUBIDA (altura efetiva do pulo)
        if (limitarAlturaDoPulo && emSubidaLimitada)
        {
            tempoDesdeInicioDoPulo += Time.fixedDeltaTime;

            bool soltouBotao   = !Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space);
            bool estourouJanela = tempoDesdeInicioDoPulo >= tempoMaxSubida;

            if (soltouBotao || estourouJanela)
            {
                if (rb.linearVelocity.y > 0f)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                emSubidaLimitada = false;
            }
        }
        if (rb.linearVelocity.y <= 0f) emSubidaLimitada = false;

        // aplica X mantendo Y
        rb.linearVelocity = new Vector2(novoX, rb.linearVelocity.y);
    }

    void LateUpdate()
    {
        // Liga/desliga climb conforme input vertical dentro da Ladder
        if (emLadderZone)
        {
            // se pressionar ↑/↓, entra; se soltar totalmente, sai
            if (!isClimbing && Mathf.Abs(inputY) > 0.05f)
                isClimbing = true;
            else if (isClimbing && Mathf.Abs(inputY) <= 0.05f)
                isClimbing = false;
        }
        else
        {
            isClimbing = false;
        }
    }

    // ————— Escada (Trigger) —————
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

    // ————— API pública para power-ups / entradas de jogo —————

    /// <summary>Concede o power-up de High Jump (permite pulo alto do chão e um no ar por voo).</summary>
    public void GrantHighJump()
    {
        hasHighJumpPower = true;
        // libera o uso aéreo assim que tocar no chão (feito em FixedUpdate)
    }

    /// <summary>
    /// Tenta executar um High Jump:
    /// - Do chão (ou micro-coyote), sempre que o power-up estiver ativo.
    /// - No ar, apenas uma vez por voo (canAirHighJump).
    /// Chame isto a partir do seu sistema de input/ação (ex.: ao pressionar Q ou botão dedicado).
    /// </summary>
    public void TryHighJump()
    {
        if (!hasHighJumpPower) return;

        bool noChao = EstaNoChao();

        // High jump do chão (ou micro-coyote)
        if (noChao || coyoteTimer > -highJumpCoyote)
        {
            float novoX = rb.linearVelocity.x; // mantém inércia atual
            rb.linearVelocity = new Vector2(novoX, highJumpForce);
            if (animator) animator.SetTrigger(hHighJump);

            if (limitarAlturaDoPulo)
            {
                emSubidaLimitada = true;
                tempoDesdeInicioDoPulo = 0f;
            }
            // se quiser permitir também o do ar depois deste, deixe canAirHighJump como está
            return;
        }

        // High jump no ar (duplo pulo) — apenas uma vez até tocar o chão
        if (!noChao && canAirHighJump)
        {
            float vx = rb.linearVelocity.x;
            // zera Y pra dar "novo impulso" limpo
            rb.linearVelocity = new Vector2(vx, 0f);
            rb.linearVelocity = new Vector2(vx, highJumpForce);

            if (animator) animator.SetTrigger(hHighJump);

            if (limitarAlturaDoPulo)
            {
                emSubidaLimitada = true;
                tempoDesdeInicioDoPulo = 0f;
            }
            canAirHighJump = false;
        }
    }

    // ————— Utilitários —————
    bool EstaNoChao()
    {
        // 1) OverlapCircle no GroundCheck
        if (groundCheck)
        {
            if (Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask))
                return true;
        }
        // 2) Fallback: BoxCast sob o colisor
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
