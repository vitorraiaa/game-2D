using UnityEngine;

public enum BossAttackKind { Melee, Fire, Lightning }

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI2D : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Animator animator;
    public BossHealth health;
    public BossShoot2D shooter;

    [Header("Ranges")]
    public float sightRange  = 14f;
    public float midRange    = 7f;   // melee
    public float attackRange = 12f;  // magia

    [Header("Alinhamento vertical")]
    public float verticalWindow = 0.7f;

    [Header("Cooldowns base (s)")]
    public Vector2 cdMeleeRangeBase     = new(1.6f, 2.3f);
    public Vector2 cdFireRangeBase      = new(4.5f, 6.5f);
    public Vector2 cdLightningRangeBase = new(6.5f, 9.0f);

    [Header("Cooldown global entre ataques (s)")]
    public Vector2 cdGlobalRangeBase = new(1.4f, 2.0f);

    [Header("Move")]
    public float walkSpeed = 1.5f;
    public float runSpeed  = 3.0f;
    public float accel     = 14f;

    [Header("Escape (anti-spam)")]
    public float escapeDuration = 0.6f;
    public float escapeSpeedMul = 2.5f;

    [Header("Debug")]
    public bool debugLogs = false;

    Rigidbody2D rb;
    SpriteRenderer sr;

    float nextFire, nextLightning, nextMelee;
    float nextAnyAttackTime;
    bool attacking;
    bool escaping;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!health)   health   = GetComponent<BossHealth>();
        if (!shooter)  shooter  = GetComponentInChildren<BossShoot2D>(true);

        float t = Time.time;
        nextMelee         = t + Rand(cdMeleeRangeBase);
        nextFire          = t + Rand(cdFireRangeBase);
        nextLightning     = t + Rand(cdLightningRangeBase);
        nextAnyAttackTime = t + Rand(cdGlobalRangeBase);
    }

    void OnEnable()
    {
        if (health != null)
            health.onEscapeRequested += HandleEscapeRequested;
    }

    void OnDisable()
    {
        if (health != null)
            health.onEscapeRequested -= HandleEscapeRequested;
    }

    void FixedUpdate()
    {
        if (!player)
        {
            MoveHoriz(0f);
            return;
        }

        // Se está escapando, a coroutine cuida do movimento
        if (escaping)
            return;

        float dx   = player.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        if (sr) sr.flipX = (dx < 0f);

        bool aligned      = IsVerticallyAligned();
        bool inSight      = dist <= sightRange;
        bool canAttackPos = inSight && aligned;
        bool inMeleeRange  = canAttackPos && dist <= midRange;
        bool inRangedRange = canAttackPos && dist <= attackRange;

        if (attacking)
        {
            MoveHoriz(0f);
            return;
        }

        float now = Time.time;
        BossAttackKind? choice = null;

        if (canAttackPos && now >= nextAnyAttackTime)
        {
            int phase = health ? health.CurrentPhase : 1;

            bool meleeReady     = inMeleeRange  && now >= nextMelee;
            bool fireReady      = inRangedRange && now >= nextFire;
            bool lightningReady = inRangedRange && now >= nextLightning;

            // pesos — melee MUITO mais comum
            float meleeW     = 0f;
            float fireW      = 0f;
            float lightningW = 0f;

            if (meleeReady)
                meleeW = 1.0f; // base alto

            if (fireReady)
            {
                fireW = phase switch
                {
                    1 => 0.10f,
                    2 => 0.25f,
                    _ => 0.40f
                };
            }

            if (lightningReady)
            {
                lightningW = phase switch
                {
                    1 => 0.05f,
                    2 => 0.18f,
                    _ => 0.35f
                };
            }

            // se melee está pronto, deixa magia ainda mais rara
            if (meleeReady)
            {
                fireW      *= 0.5f;
                lightningW *= 0.5f;
            }

            float total = meleeW + fireW + lightningW;
            if (total > 0f)
            {
                float r = Random.value * total;
                if (r < meleeW) choice = BossAttackKind.Melee;
                else if (r < meleeW + fireW) choice = BossAttackKind.Fire;
                else choice = BossAttackKind.Lightning;
            }
        }

        if (choice.HasValue && shooter)
        {
            attacking = true;
            if (debugLogs) Debug.Log($"[BossAI] Attack {choice.Value} (dist={dist:0.0})", this);

            shooter.RequestAttack(choice.Value, () =>
            {
                attacking = false;
                if (debugLogs) Debug.Log("[BossAI] Attack finished", this);
            });

            int phase = health ? health.CurrentPhase : 1;

            nextAnyAttackTime = now + ScaleByPhase(cdGlobalRangeBase, phase);

            switch (choice.Value)
            {
                case BossAttackKind.Melee:
                    nextMelee = now + ScaleByPhase(cdMeleeRangeBase, phase, 1f, 0.9f, 0.8f);
                    break;
                case BossAttackKind.Fire:
                    nextFire = now + ScaleByPhase(cdFireRangeBase, phase, 1f, 0.8f, 0.6f);
                    break;
                case BossAttackKind.Lightning:
                    nextLightning = now + ScaleByPhase(cdLightningRangeBase, phase, 1f, 0.8f, 0.6f);
                    break;
            }
        }

        if (!attacking)
        {
            float targetSpeedX = 0f;

            if (!inSight)
                targetSpeedX = Mathf.Sign(dx) * walkSpeed;
            else
            {
                if (dist > midRange)
                    targetSpeedX = Mathf.Sign(dx) * runSpeed;
                else if (!canAttackPos)
                    targetSpeedX = Mathf.Sign(dx) * walkSpeed;
            }

            MoveHoriz(targetSpeedX);
        }
    }

    // ===== Escape =====

    void HandleEscapeRequested()
    {
        if (escaping || !player || health == null)
            return;

        if (debugLogs) Debug.Log("[BossAI] Escape solicitado (anti-spam)", this);
        StartCoroutine(EscapeRoutine());
    }

    System.Collections.IEnumerator EscapeRoutine()
    {
        escaping = true;
        attacking = false;

        health.SetInvulnerable(true);

        float dir = Mathf.Sign(transform.position.x - player.position.x); // foge do player
        float escapeSpeed = runSpeed * escapeSpeedMul;
        float endTime = Time.time + escapeDuration;

        if (animator) animator.SetFloat("Speed", Mathf.Abs(escapeSpeed));

        while (Time.time < endTime)
        {
            float newX = Mathf.MoveTowards(
                rb.linearVelocity.x,
                dir * escapeSpeed,
                accel * 2f * Time.fixedDeltaTime
            );
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            yield return new WaitForFixedUpdate();
        }

        // freia
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (animator) animator.SetFloat("Speed", 0f);

        health.SetInvulnerable(false);
        escaping = false;
    }

    // ===== Helpers =====

    bool IsVerticallyAligned()
    {
        if (!player) return false;
        return Mathf.Abs(player.position.y - transform.position.y) <= verticalWindow;
    }

    void MoveHoriz(float targetXVel)
    {
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetXVel, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        if (animator) animator.SetFloat("Speed", Mathf.Abs(newX));
    }

    static float Rand(Vector2 range) => Random.Range(range.x, range.y);

    static float ScaleByPhase(Vector2 baseRange, int phase,
                              float m1 = 1f, float m2 = 0.9f, float m3 = 0.8f)
    {
        float m = phase switch
        {
            1 => m1,
            2 => m2,
            _ => m3
        };
        return Random.Range(baseRange.x * m, baseRange.y * m);
    }
}
