// EnemyHealth.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public int maxHP = 3;
    public bool invincibleOnHurt = true;
    public float invincibleTime = 0.2f;

    [Header("Knockback (opcional)")]
    public float knockbackForce = 0f;

    [Header("Animator (opcional)")]
    public Animator animator;
    public string hurtParam  = "Hurt";   // Trigger
    public string deathParam = "Death";  // Trigger

    int hp;
    bool dead;
    bool invincible;
    Rigidbody2D rb;
    Collider2D[] cols;
    MonoBehaviour[] enemyBehaviours; // coloque aqui EnemyAI2D, EnemyShoot2D, etc.

    void Awake()
    {
        hp = maxHP;
        if (!animator) animator = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
        cols = GetComponentsInChildren<Collider2D>();

        // Desligaremos isso no death:
        enemyBehaviours = new MonoBehaviour[] {
            GetComponent<EnemyAI2D>(),
            GetComponent<EnemyShoot2D>()
        };
    }

    public void ApplyDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (dead || invincible) return;

        hp -= Mathf.Max(1, amount);

        if (hp <= 0)
        {
            Die();
            return;
        }

        if (animator) animator.SetTrigger(hurtParam);

        if (knockbackForce > 0f && rb)
            rb.AddForce(hitNormal.normalized * knockbackForce, ForceMode2D.Impulse);

        if (invincibleOnHurt) StartCoroutine(InvFrames());
    }

    System.Collections.IEnumerator InvFrames()
    {
        invincible = true;
        yield return new WaitForSeconds(invincibleTime);
        invincible = false;
    }

    void Die()
    {
        if (dead) return;
        dead = true;

        // desliga IA / tiro
        foreach (var b in enemyBehaviours) if (b) b.enabled = false;

        // desabilita colisão para não atrapalhar
        foreach (var c in cols) c.enabled = false;

        // zera velocidade
        if (rb) rb.linearVelocity = Vector2.zero;

        if (animator)
        {
            animator.SetTrigger(deathParam);
            // destrói ao fim da animação via Animation Event chamando OnDeathAnimationEnd()
            // ou usa um fallback de tempo:
            Destroy(gameObject, 1.0f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Chame este método por Animation Event no último frame da animação de morte (opcional)
    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
}
