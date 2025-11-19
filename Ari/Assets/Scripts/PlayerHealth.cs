using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHP = 3;
    public float invulAfterHurt = 0.2f; // janela anti-hit múltiplo

    [Header("Morte / Desaparecer")]
    public string deathStateName = "Death"; // nome do state no Animator
    public bool destroyOnDeath = true;      // destrói o GO ao fim da animação
    public float deathCleanupDelay = 0.8f;  // fallback se não detectar o fim do state
    public float musicFadeOnDeath = 0.3f;   // fade curto da música ao morrer

    int hp;
    bool dead;
    float invulTimer;
    Animator anim;

    void Awake()
    {
        hp = maxHP;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (invulTimer > 0f) invulTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (dead || invulTimer > 0f) return;

        hp -= Mathf.Max(1, amount);

        // SFX de dano
        SfxManager.Instance?.PlayHurt();

        if (anim) anim.SetTrigger("Hurt");
        invulTimer = invulAfterHurt;

        if (hp <= 0)
        {
            HandleDeath();
        }
    }

    void HandleDeath()
    {
        if (dead) return;
        dead = true;

        // Para música de fundo e toca game over
        SfxManager.Instance?.StopMusic(musicFadeOnDeath);
        SfxManager.Instance?.PlayGameOver();

        // Animação de morte
        if (anim) anim.SetTrigger("Dead");

        // Desativa controles / movimento / colisores, mas deixa o sprite/anim rodando
        var move = GetComponent<PlayerMovement2D>(); if (move) move.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var rb = GetComponent<Rigidbody2D>(); if (rb) rb.simulated = false;

        if (destroyOnDeath)
            StartCoroutine(WaitAndDisappear());
    }

    IEnumerator WaitAndDisappear()
    {
        // Tenta esperar o fim do state "Death" (Layer 0)
        if (anim)
        {
            // aguarda entrar no state Death
            bool entered = false;
            float guard = 2.5f; // trava de segurança
            while (guard > 0f)
            {
                var st = anim.GetCurrentAnimatorStateInfo(0);
                if (st.IsName(deathStateName)) { entered = true; break; }
                guard -= Time.deltaTime;
                yield return null;
            }

            // aguarda terminar o state Death
            if (entered)
            {
                while (true)
                {
                    var st = anim.GetCurrentAnimatorStateInfo(0);
                    if (!st.IsName(deathStateName) || st.normalizedTime >= 0.99f) break;
                    yield return null;
                }
            }
            else
            {
                // caso não entre no state, usa fallback
                yield return new WaitForSeconds(deathCleanupDelay);
            }
        }
        else
        {
            yield return new WaitForSeconds(deathCleanupDelay);
        }

        Destroy(gameObject); // some da tela e da cena
    }

    // OPCIONAL: chame este método via Animation Event no último frame do clip "Death"
    public void OnDeathAnimationFinished()
    {
        if (destroyOnDeath)
            Destroy(gameObject);
    }

    public void Heal(int amount)
    {
    if (hp >= maxHP) return; // Já está com vida cheia
    
    hp += amount;
    hp = Mathf.Min(hp, maxHP); // Não passa do máximo
    
    Debug.Log($"Player curou {amount} HP! Vida: {hp}/{maxHP}");
    
    // Feedback visual opcional
    if (anim) anim.SetTrigger("Heal");
    }
}
