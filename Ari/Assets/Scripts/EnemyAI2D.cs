using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAI2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // se vazio, procura por tag "Player"
    public string playerTag = "Player";

    [Header("Movimento")]
    public float walkSpeed = 2.0f;           // velocidade quando avança
    public float advanceRange = 4.0f;        // começa a AVANÇAR quando dist <= isso (curto)
    public float stopDistance = 1.5f;        // para de avançar quando chega nisso
    public bool faceTarget = true;

    [Header("Tiro (coordenado com EnemyShoot2D)")]
    public EnemyShoot2D shooter;             // arraste o componente do mesmo GO
    public float shootRange = 10.0f;         // atira se dist <= isso (longo)

    [Header("Animator (opcional)")]
    public Animator animator;
    public string speedParam = "Speed";      // float
    public string hurtParam  = "Hurt";       // trigger (se usar)
    public string deathParam = "Death";      // trigger (se usar)

    Rigidbody2D rb;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();
        if (!shooter) shooter = GetComponent<EnemyShoot2D>();
    }

    void Start()
    {
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) target = p.transform;
        }

        // passa o shootRange para o shooter (se quiser centralizar aqui)
        if (shooter) shooter.attackRange = shootRange;
    }

    void FixedUpdate()
    {
        if (!target)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (animator) animator.SetFloat(speedParam, 0f);
            return;
        }

        Vector2 toPlayer = target.position - transform.position;
        float dist = toPlayer.magnitude;

        // 1) Olhar para o player (opcional)
        if (faceTarget && sr)
        {
            if (toPlayer.x != 0) sr.flipX = toPlayer.x < 0f;
        }

        // 2) Movimento: só avança se o player estiver perto (<= advanceRange)
        float vx = 0f;
        if (dist <= advanceRange && dist > stopDistance)
        {
            float dir = Mathf.Sign(toPlayer.x);      // 1 ou -1
            vx = dir * walkSpeed;
        }

        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        if (animator) animator.SetFloat(speedParam, Mathf.Abs(vx));

        // 3) Tiro: o EnemyShoot2D já cuida do cooldown;
        //    aqui só garantimos que o "attackRange" dele está correto.
        //    (se você preferir, pode ligar/desligar o shooter por distância)
        if (shooter) shooter.attackRange = shootRange;
    }

    // Exemplo de hooks para Hurt/Death (se quiser acionar por outro script)
    public void OnHurt()  { if (animator) animator.SetTrigger(hurtParam); }
    public void OnDeath() { if (animator) animator.SetTrigger(deathParam); }
}
