using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 3;
    public float deathCleanupDelay = 0.8f;

    int hp;
    bool dead;
    Animator anim;

    void Awake()
    {
        hp = maxHP;
        anim = GetComponentInChildren<Animator>(); // no spriteRoot
    }

    public void TakeDamage(int amount)
    {
        if (dead) return;

        hp -= Mathf.Max(1, amount);
        if (anim) anim.SetTrigger("Hurt");

        if (hp <= 0)
        {
            dead = true;
            if (anim) anim.SetBool("Dead", true);   // <<< bool, D maiúsculo

            // Desliga AI e tiro
            var ai = GetComponent<EnemyAI2D>();      if (ai) ai.enabled = false;
            var sh = GetComponentInChildren<EnemyShoot2D>(); if (sh) sh.enabled = false;

            // Desativa colisão/movimento
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            var rb = GetComponent<Rigidbody2D>(); if (rb) rb.simulated = false;

            Destroy(gameObject, deathCleanupDelay);
        }
    }
}
