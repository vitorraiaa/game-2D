using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PlayerShoot2D : MonoBehaviour
{
    [Header("Setup")]
    public Transform shootPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 18f;
    public float fireRate = 6f;
    public float spawnMargin = 0.08f;
    public float recoilKick = 0f;

    [Header("Extra Attack (power-up)")]
    public bool hasExtraAttackPower = false;
    public GameObject extraBulletPrefab;
    public float extraBulletSpeed = 20f;
    public float extraFireRate = 2.5f;

    [Header("Animator")]
    public Animator animator;

    float cooldown;
    float cooldownExtra;
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
        cooldownExtra -= Time.deltaTime;

        // tiro normal
        bool fire = Input.GetButton("Fire1") || Input.GetMouseButton(0);
        if (fire && cooldown <= 0f) { ShootOnce(bulletPrefab, bulletSpeed, "Attack"); cooldown = 1f / Mathf.Max(0.01f, fireRate); }

        // extra attack (botão direito / E)
        bool extra = hasExtraAttackPower && (Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.E));
        if (extra && cooldownExtra <= 0f && extraBulletPrefab)
        {
            ShootOnce(extraBulletPrefab, extraBulletSpeed, "ExtraAttack");
            cooldownExtra = 1f / Mathf.Max(0.01f, extraFireRate);
        }
    }

    void ShootOnce(GameObject prefab, float speed, string animTrigger)
    {
        if (!prefab || !shootPoint) return;

        int dir = sr && sr.flipX ? -1 : 1;
        Vector2 shotDir = new Vector2(dir, 0f);

        Vector3 spawnPos = shootPoint.position;
        if (TryGetComponent<Collider2D>(out var myCol))
        {
            float halfX = myCol.bounds.extents.x;
            spawnPos += new Vector3(dir * (halfX + spawnMargin), 0f, 0f);
        }

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

        // ignorar colisão com o player
        var bulletCols = go.GetComponentsInChildren<Collider2D>();
        foreach (var bc in bulletCols)
            foreach (var pc in playerCols)
                if (bc && pc) Physics2D.IgnoreCollision(bc, pc, true);

        if (rb && recoilKick > 0f)
            rb.AddForce(new Vector2(-dir * recoilKick, 0f), ForceMode2D.Impulse);

        if (animator && !string.IsNullOrEmpty(animTrigger)) animator.SetTrigger(animTrigger);
    }
}
