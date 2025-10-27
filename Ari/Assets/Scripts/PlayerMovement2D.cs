using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 3.5f;          // velocidade ao andar (sem Shift)
    public float runSpeed  = 8f;            // velocidade ao correr (Shift)
    public float aceleracaoNoChao = 60f;
    public float aceleracaoNoAr   = 40f;

    [Header("Pulo")]
    public float forcaPulo = 14f;
    public float coyoteTime = 0.12f;        // tolerância depois de sair do chão
    public float jumpBuffer = 0.12f;        // tolerância antes de tocar o chão
    public float gravidadeDescida = 2.5f;   // pulo “pesado” descendo
    public float gravidadeSubida  = 2.0f;   // corta a subida ao soltar o botão

    [Header("Limites de Pulo")]
    public bool limitarAlturaDoPulo = true; // liga/desliga limite
    [Tooltip("Tempo máximo (s) que o player pode continuar SUBINDO após iniciar o pulo.")]
    public float tempoMaxSubida = 0.12f;    // 0.10–0.14 dá um pulo curtinho e consistente

    [Header("Chão")]
    public Transform groundCheck;           // filho “GroundCheck”
    public float groundCheckRadius = 0.12f; // raio do sensor
    public LayerMask groundMask;            // Layer “Ground”

    [Header("Animator (auto)")]
    public Animator animator;               // preenchido automaticamente

    // --- internos ---
    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;

    float inputX;                 // -1, 0, 1
    float coyoteTimer;
    float jumpBufferTimer;

    // controle do limite de subida
    float tempoDesdeInicioDoPulo;
    bool emSubidaLimitada;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (!animator) animator = GetComponent<Animator>();

        // cria/acha GroundCheck se faltar
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
    }

    void Update()
    {
        // input horizontal “cru” (resposta rápida no teclado)
        inputX = Input.GetAxisRaw("Horizontal");

        // intenção de pulo (Jump OU Space)
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBuffer;
        jumpBufferTimer -= Time.deltaTime;

        // vira sprite
        if (inputX != 0) sr.flipX = inputX < 0;

        // alimenta Animator
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
        if (noChao) coyoteTimer = coyoteTime;
        else        coyoteTimer -= Time.fixedDeltaTime;

        // Walk por padrão; Run com Shift pressionado
        bool runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = runHeld ? runSpeed : walkSpeed;

        // aceleração suave no X
        float alvoX = inputX * targetSpeed;
        float acel  = noChao ? aceleracaoNoChao : aceleracaoNoAr;
        float novoX = Mathf.MoveTowards(rb.linearVelocity.x, alvoX, acel * Time.fixedDeltaTime);

        // pulo (coyote + buffer)
        bool podePularAgora = (coyoteTimer > 0f) && (jumpBufferTimer > 0f);
        if (podePularAgora)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            rb.linearVelocity = new Vector2(novoX, forcaPulo);
            if (animator) animator.SetTrigger("Jump");

            // inicia janela de subida limitada
            if (limitarAlturaDoPulo)
            {
                emSubidaLimitada = true;
                tempoDesdeInicioDoPulo = 0f;
            }
            // aplicamos pulo e saímos deste frame de física
            return;
        }

        // Gravidade variável (sensação melhor de pulo)
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

        // Limita a duração da SUBIDA (altura máxima efetiva)
        if (limitarAlturaDoPulo && emSubidaLimitada)
        {
            tempoDesdeInicioDoPulo += Time.fixedDeltaTime;

            bool soltouBotao = !Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space);
            bool estourouJanela = tempoDesdeInicioDoPulo >= tempoMaxSubida;

            if (soltouBotao || estourouJanela)
            {
                // se ainda está subindo, zera o Y para parar de ganhar altura
                if (rb.linearVelocity.y > 0f)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

                emSubidaLimitada = false;
            }
        }
        // Se começar a cair, encerra a janela de subida limitada
        if (rb.linearVelocity.y <= 0f) emSubidaLimitada = false;

        // aplica X mantendo Y
        rb.linearVelocity = new Vector2(novoX, rb.linearVelocity.y);
    }

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
