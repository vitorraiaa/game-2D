using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PlayerShoot2D : MonoBehaviour
{
    [Header("Setup")]
    public Transform shootPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 18f;

    [Header("Main Shot Cooldown")]
    [Tooltip("Tiros por segundo segurando o botão. Valores menores = menos spam.")]
    public float fireRate = 3f;          // antes 6f, agora mais cadenciado
    public float spawnMargin = 0.08f;    // distância extra além do collider do player
    public float recoilKick = 0f;

    [Header("Extra Attack")]
    public bool hasExtraAttackPower = false;   // ligado pelo pickup
    public GameObject extraBulletPrefab;       // prefab especial
    public float extraBulletSpeed = 14f;
    [Tooltip("Cooldown em segundos entre usos do ataque extra (tecla E).")]
    public float extraAttackCooldown = 2.5f;

    [Header("Animator (opcional)")]
    public Animator animator;

    // internos
    float fireCooldownTimer;
    float extraCooldownTimer;

    SpriteRenderer sr;
    Rigidbody2D rb;
    Collider2D[] playerCols;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();
        playerCols = GetComponentsInChildren<Collider2D>();
    }

    void Update()
    {
        // avança timers
        if (fireCooldownTimer > 0f)  fireCooldownTimer  -= Time.deltaTime;
        if (extraCooldownTimer > 0f) extraCooldownTimer -= Time.deltaTime;

        // tiro normal (segurando botão / mouse)
        bool fireMain = Input.GetButton("Fire1") || Input.GetMouseButton(0);
        if (fireMain && fireCooldownTimer <= 0f)
        {
            ShootOnce();
            fireCooldownTimer = 1f / Mathf.Max(0.01f, fireRate);
        }

        // ataque extra (E) com cooldown próprio
        if (hasExtraAttackPower && Input.GetKeyDown(KeyCode.E) && extraCooldownTimer <= 0f)
        {
            ShootExtra();
            extraCooldownTimer = extraAttackCooldown;
        }
    }

    void ShootOnce()
    {
        if (!bulletPrefab || !shootPoint)
        {
            Debug.LogWarning("[PlayerShoot] Faltou bulletPrefab ou shootPoint.");
            return;
        }

        // SFX de tiro
        if (SfxManager.Instance) SfxManager.Instance.PlayShoot();

        SpawnBullet(bulletPrefab, bulletSpeed);

        if (animator)
            animator.SetTrigger("Attack");
    }

    void ShootExtra()
    {
        if (!extraBulletPrefab || !shootPoint)
        {
            Debug.LogWarning("[PlayerShoot] Faltou extraBulletPrefab ou shootPoint.");
            return;
        }

        // SFX de tiro (também no extra, se quiser diferenciar, crie PlayShootExtra)
        if (SfxManager.Instance) SfxManager.Instance.PlayShoot();

        SpawnBullet(extraBulletPrefab, extraBulletSpeed);

        if (animator)
            animator.SetTrigger("ExtraAttack");
    }

    void SpawnBullet(GameObject prefab, float speed)
    {
        int dir = (sr && sr.flipX) ? -1 : 1;
        Vector2 shotDir = new Vector2(dir, 0f);

        // posição de spawn na frente do player
        Vector3 spawnPos = shootPoint.position;
        if (TryGetComponent<Collider2D>(out var myCol))
        {
            float halfX = myCol.bounds.extents.x;
            spawnPos += new Vector3(dir * (halfX + spawnMargin), 0f, 0f);
        }
        else
        {
            spawnPos += new Vector3(dir * 0.2f, 0f, 0f);
        }

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);

        // layer da bala do player
        go.layer = LayerMask.NameToLayer("BulletPlayer");

        // configura Bullet2D
        var bullet = go.GetComponent<Bullet2D>();
        if (bullet)
        {
            // garante que só acerta inimigos/boss
            bullet.hitMask = LayerMask.GetMask("Enemy");
            bullet.Launch(shotDir, speed);
        }

        // ignora colisão com o próprio player
        var bulletCols = go.GetComponentsInChildren<Collider2D>();
        foreach (var bc in bulletCols)
        {
            if (!bc) continue;
            foreach (var pc in playerCols)
            {
                if (pc) Physics2D.IgnoreCollision(bc, pc, true);
            }
        }

        // recuo opcional
        if (rb && recoilKick > 0f)
            rb.AddForce(new Vector2(-dir * recoilKick, 0f), ForceMode2D.Impulse);
    }
}
