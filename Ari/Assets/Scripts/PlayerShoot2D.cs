using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PlayerShoot2D : MonoBehaviour
{
    [Header("Setup")]
    public Transform shootPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 18f;
    public float fireRate = 6f;        // tiros por segundo
    public float spawnMargin = 0.08f;  // distância extra além do collider do player
    public float recoilKick = 0f;

    [Header("Extra Attack")]
    public bool hasExtraAttackPower = false;   // ligado pelo pickup
    public GameObject extraBulletPrefab;       // arraste aqui o prefab especial
    public float extraBulletSpeed = 14f;

    [Header("Animator (opcional)")]
    public Animator animator;

    float cooldown;
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
        cooldown -= Time.deltaTime;

        bool fireMain = Input.GetButton("Fire1") || Input.GetMouseButton(0);
        if (fireMain && cooldown <= 0f)
        {
            ShootOnce();
            cooldown = 1f / Mathf.Max(0.01f, fireRate);
        }

        // Extra Attack no 'E'
        if (hasExtraAttackPower && Input.GetKeyDown(KeyCode.E))
        {
            ShootExtra();
        }
    }

    void ShootOnce()
    {
        if (!bulletPrefab || !shootPoint)
        {
            Debug.LogWarning("[Shoot] Faltou bulletPrefab ou shootPoint.");
            return;
        }
        SpawnBullet(bulletPrefab, bulletSpeed);
        if (animator) animator.SetTrigger("Attack");
    }

    void ShootExtra()
    {
        if (!extraBulletPrefab || !shootPoint)
        {
            Debug.LogWarning("[Shoot] Faltou extraBulletPrefab ou shootPoint.");
            return;
        }
        Debug.Log("[Shoot] EXTRA ATTACK!");
        SpawnBullet(extraBulletPrefab, extraBulletSpeed);
        if (animator) animator.SetTrigger("ExtraAttack");
    }

    void SpawnBullet(GameObject prefab, float speed)
    {
        int dir = sr && sr.flipX ? -1 : 1;
        Vector2 shotDir = new Vector2(dir, 0f);

        Vector3 spawnPos = shootPoint.position;
        if (TryGetComponent<Collider2D>(out var myCol))
        {
            float halfX = myCol.bounds.extents.x;
            spawnPos += new Vector3(dir * (halfX + spawnMargin), 0f, 0f);
        }
        else spawnPos += new Vector3(dir * 0.2f, 0f, 0f);

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);

        var bullet = go.GetComponent<Bullet2D>();
        if (bullet) bullet.Launch(shotDir, speed);

        var brb = go.GetComponent<Rigidbody2D>();
        if (brb)
        {
            brb.bodyType = RigidbodyType2D.Dynamic;
            brb.simulated = true;
            brb.gravityScale = 0f;
            brb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            brb.constraints = RigidbodyConstraints2D.None;
            brb.linearVelocity = shotDir.normalized * speed;
        }
        else
        {
            Debug.LogError("[Shoot] Bullet prefab não tem Rigidbody2D.");
        }

        var bulletCols = go.GetComponentsInChildren<Collider2D>();
        foreach (var bc in bulletCols)
            foreach (var pc in playerCols)
                if (bc && pc) Physics2D.IgnoreCollision(bc, pc, true);

        if (rb && recoilKick > 0f)
            rb.AddForce(new Vector2(-dir * recoilKick, 0f), ForceMode2D.Impulse);
    }
}
