using UnityEngine;

public class EnemyShoot2D : MonoBehaviour
{
    [Header("Refs")]
    public Transform shootPoint;
    public GameObject bulletPrefab;
    public Animator animator; // do sprite

    [Header("Config")]
    public float bulletSpeed   = 12f;
    public float fireCooldown  = 0.8f;
    public float fireFailSafeDelay = 0.18f; // dispara se o evento não vier
    public bool  useAnimEvent  = true;      // deixe ON (usa OnAnimFire do clip)

    float nextFireTime;
    bool waitingFailSafe;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    public void TryAttack()
    {
        if (Time.time < nextFireTime) return;
        if (!bulletPrefab || !shootPoint)
        {
            Debug.LogWarning("[EnemyShoot] bulletPrefab/shootPoint faltando", this);
            return;
        }

        nextFireTime = Time.time + fireCooldown;

        if (useAnimEvent && animator)
        {
            if (!HasParam(animator, "Attack"))
                Debug.LogWarning("[EnemyShoot] Animator sem Trigger 'Attack'", animator);

            animator.SetTrigger("Attack");

            // se o evento não vier, dispare no delay de segurança
            if (!waitingFailSafe)
                Invoke(nameof(FailSafeFire), fireFailSafeDelay);
            waitingFailSafe = true;
        }
        else
        {
            // sem animação/evento → dispara direto
            OnAnimFire();
        }
    }

    // Chamado pelo Animation Event no clipe Attack
    public void OnAnimFire()
    {
        waitingFailSafe = false; // evento chegou → cancela failsafe
        CancelInvoke(nameof(FailSafeFire));
        DoFire();
    }

    void FailSafeFire()
    {
        if (!waitingFailSafe) return; // evento já chegou
        waitingFailSafe = false;
        DoFire();
    }

    void DoFire()
    {
        if (!bulletPrefab || !shootPoint) return;

        var go = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        go.layer = LayerMask.NameToLayer("BulletEnemy");

        var b = go.GetComponent<Bullet2D>();
        if (b)
        {
            b.hitMask = LayerMask.GetMask("Player");

            var sr = GetComponentInChildren<SpriteRenderer>();
            int dir = (sr && sr.flipX) ? -1 : 1;

            b.Launch(new Vector2(dir, 0f), bulletSpeed);
        }
        else
        {
            Debug.LogWarning("[EnemyShoot] Bullet2D ausente no prefab", bulletPrefab);
        }
    }

    static bool HasParam(Animator a, string p)
    {
        foreach (var x in a.parameters) if (x.name == p) return true;
        return false;
    }
}
