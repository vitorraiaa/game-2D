using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossShoot2D : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public Transform pointMelee;
    public Transform pointFire;
    public Transform player;
    public BossHealth health;

    [Header("Prefabs")]
    public GameObject meleePrefab;
    public GameObject fireBulletPrefab;
    public GameObject lightningPrefab;

    [Header("Lightning targeting")]
    [Tooltip("Altura acima do boss para spawnar o raio.")]
    public float lightningHeightOffset = 6f;
    [Tooltip("Chance de o raio cair NO player em cada fase.")]
    public float hitChancePhase1 = 0.20f;
    public float hitChancePhase2 = 0.35f;
    public float hitChancePhase3 = 0.50f;
    [Tooltip("Distância mínima/máxima em X quando NÃO acerta o player.")]
    public float minOffsetX = 1.5f;
    public float maxOffsetX = 3.5f;
    [Tooltip("Tempo de vida do prefab de raio.")]
    public float lightningLifeTime = 0.6f;

    [Header("Debug")]
    public bool debugLogs = false;
    public float maxAttackDuration = 2.0f;

    System.Action onFinished;
    bool inProgress;
    float attackStartTime;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!health)
            health = GetComponentInParent<BossHealth>();

        if (!animator)
            Debug.LogError("[BossShoot] Sem Animator.", this);
    }

    // ==== Chamado pelo BossAI2D ====
    public void RequestAttack(BossAttackKind kind, System.Action onAttackFinished)
    {
        if (inProgress || !animator)
            return;

        inProgress = true;
        onFinished = onAttackFinished;
        attackStartTime = Time.time;

        switch (kind)
        {
            case BossAttackKind.Melee:
                if (debugLogs) Debug.Log("[BossShoot] Request Melee");
                animator.SetTrigger("Attack");
                break;

            case BossAttackKind.Fire:
                if (debugLogs) Debug.Log("[BossShoot] Request Fire");
                animator.SetTrigger("MagicFire");
                break;

            case BossAttackKind.Lightning:
                if (debugLogs) Debug.Log("[BossShoot] Request Lightning");
                animator.SetTrigger("MagicLightning");
                break;
        }

        CancelInvoke(nameof(ForceEnd));
        Invoke(nameof(ForceEnd), maxAttackDuration);
    }


    // ==== EVENTS ====

    // Boss_Attack → frame do golpe
    public void OnAnimBlade()
    {
        if (!inProgress) return;
        if (!meleePrefab || !pointMelee) return;

        if (debugLogs) Debug.Log("[BossShoot] Slash!");

        var go = Instantiate(meleePrefab, pointMelee.position, Quaternion.identity);

        var b = go.GetComponent<Bullet2D>();
        if (b)
        {
            // se não configurado no prefab, garante que acerta Player
            if (b.hitMask == 0)
                b.hitMask = LayerMask.GetMask("Player");

            // direção baseada no flipX do boss
            var sr = GetComponentInChildren<SpriteRenderer>();
            int dir = (sr && sr.flipX) ? -1 : 1;

            // usa speed/maxDistance do próprio prefab
            b.Launch(new Vector2(dir, 0f), b.speed > 0 ? b.speed : 20f);
        }
    }


    // Boss_Magic_Fire
    public void OnAnimFire()
    {
        if (!inProgress) return;
        if (!fireBulletPrefab || !pointFire) return;

        if (debugLogs) Debug.Log("[BossShoot] Fire!");

        var go = Instantiate(fireBulletPrefab, pointFire.position, Quaternion.identity);
        var b = go.GetComponent<Bullet2D>();
        if (b)
        {
            if (b.hitMask == 0)
                b.hitMask = LayerMask.GetMask("Player");

            var sr = GetComponentInChildren<SpriteRenderer>();
            int dir = (sr && sr.flipX) ? -1 : 1;
            b.Launch(new Vector2(dir, 0f), b.speed > 0 ? b.speed : 10f);
        }
    }

    // Boss_Magic_Lightning
    public void OnAnimLightning()
    {
        if (!inProgress) return;
        if (!lightningPrefab || !player)
        {
            Debug.LogWarning("[BossShoot] OnAnimLightning chamado, mas faltam refs.", this);
            return;
        }

        int phase = health ? health.CurrentPhase : 1;
        float hitChance = phase switch
        {
            1 => hitChancePhase1,
            2 => hitChancePhase2,
            _ => hitChancePhase3
        };

        float offsetX = 0f;
        if (Random.value > hitChance)
        {
            float sign = (Random.value < 0.5f) ? -1f : 1f;
            float mag  = Random.Range(minOffsetX, maxOffsetX);
            offsetX = sign * mag;
        }

        Vector3 pos = new Vector3(
            player.position.x + offsetX,
            transform.position.y + lightningHeightOffset,
            0f
        );

        if (debugLogs) Debug.Log($"[BossShoot] Lightning at X={pos.x:0.00}", this);

        var go = Instantiate(lightningPrefab, pos, Quaternion.identity);

        if (lightningLifeTime > 0f)
            Destroy(go, lightningLifeTime);
    }

    // Último frame de CADA clip de ataque
    public void OnAnimAttackEnd()
    {
        if (debugLogs) Debug.Log("[BossShoot] Attack end (event)");
        Finish();
    }

    void ForceEnd()
    {
        if (!inProgress) return;
        if (debugLogs) Debug.Log("[BossShoot] Attack end (failsafe)");
        Finish();
    }

    void Finish()
    {
        inProgress = false;
        CancelInvoke(nameof(ForceEnd));

        var cb = onFinished;
        onFinished = null;
        cb?.Invoke();
    }

    void Update()
    {
        if (inProgress && Time.time - attackStartTime > maxAttackDuration * 1.5f)
            ForceEnd();
    }
}
