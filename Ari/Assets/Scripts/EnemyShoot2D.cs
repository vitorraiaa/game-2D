using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyShoot2D : MonoBehaviour
{
    [Header("Tiro")]
    public Transform shootPoint;
    public GameObject bulletPrefab;     // use seu Bullet2D
    public float bulletSpeed = 10f;
    public float fireCooldown = 1.2f;   // intervalo entre tiros
    public int bulletDamage = 1;

    [Header("Alvo")]
    public Transform target;            // arraste o Player; ou buscaremos
    public float attackRange = 8f;
    public float fireLead = 0f;         // 0 = mira direta

    [Header("Animator (opcional)")]
    public Animator animator;

    float cd;
    SpriteRenderer sr;

    int hAttack;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();
        hAttack = Animator.StringToHash("Attack");
        if (!target) { var p = GameObject.FindGameObjectWithTag("Player"); if (p) target = p.transform; }
    }

    void Update()
    {
        cd -= Time.deltaTime;
        if (!target || !bulletPrefab || !shootPoint) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= attackRange && cd <= 0f)
        {
            // vira para o alvo
            if (sr) sr.flipX = (target.position.x < transform.position.x);
            if (animator) animator.SetTrigger(hAttack);

            // Disparo real (direto; se preferir, chame via Animation Event)
            Fire();
            cd = fireCooldown;
        }
    }

    public void Fire()
    {
        if (!bulletPrefab || !shootPoint) return;

        Vector2 dir = (target ? (Vector2)(target.position - shootPoint.position) : Vector2.left);
        dir.Normalize();

        var go = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        // se seu Bullet2D tem dano, exponha um campo; aqui ilustramos simples:
        var b = go.GetComponent<Bullet2D>();
        if (b) b.Launch(dir, bulletSpeed);
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) { rb.gravityScale = 0f; rb.linearVelocity = dir * bulletSpeed; }
        // layer da bala inimiga
        go.layer = LayerMask.NameToLayer("BulletEnemy");
    }
}
