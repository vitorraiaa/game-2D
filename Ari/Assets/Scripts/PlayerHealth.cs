using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events; // ← NECESSÁRIO PARA A BARRA

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHP = 3;
    public float invulAfterHurt = 0.2f;

    [Header("Morte / Desaparecer")]
    public string deathStateName = "Death";
    public bool destroyOnDeath = true;
    public float deathCleanupDelay = 0.8f;
    public float musicFadeOnDeath = 0.3f;

    [Header("Game Over")]
    public string gameOverSceneName = "EndGameScene";
    public float gameOverDelay = 1.0f;

    [Header("Morte por queda")]
    public float killY = -20f;

    // ← NECESSÁRIO PARA A BARRA DE VIDA
    public UnityEvent<int, int> OnHealthChanged;

    int hp;
    bool dead;
    float invulTimer;
    Animator anim;

    void Awake()
    {
        hp = maxHP;
        anim = GetComponent<Animator>();
        
        // ← NECESSÁRIO: Notifica a UI da vida inicial
        OnHealthChanged?.Invoke(hp, maxHP);
    }

    void Update()
    {
        if (invulTimer > 0f) invulTimer -= Time.deltaTime;

        // Morte por queda
        if (!dead && transform.position.y < killY)
        {
            HandleDeath();
        }
    }

    public void TakeDamage(int amount)
    {
        if (dead || invulTimer > 0f) return;

        hp -= Mathf.Max(1, amount);

        SfxManager.Instance?.PlayHurt();

        if (anim) anim.SetTrigger("Hurt");
        invulTimer = invulAfterHurt;

        // ← NECESSÁRIO: Notifica mudança de vida
        OnHealthChanged?.Invoke(hp, maxHP);

        if (hp <= 0)
        {
            HandleDeath();
        }
    }

    void HandleDeath()
    {
        if (dead) return;
        dead = true;

        SfxManager.Instance?.StopMusic(musicFadeOnDeath);
        SfxManager.Instance?.PlayGameOver();

        if (anim) anim.SetTrigger("Dead");

        var move = GetComponent<PlayerMovement2D>(); if (move) move.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var rb = GetComponent<Rigidbody2D>(); if (rb) rb.simulated = false;

        // Remova ou comente esta linha:
        // if (destroyOnDeath)
        //     StartCoroutine(WaitAndDisappear());

        StartCoroutine(LoadGameOverAfterDelay());
    }

    IEnumerator WaitAndDisappear()
    {
        if (anim)
        {
            bool entered = false;
            float guard = 2.5f;
            while (guard > 0f)
            {
                var st = anim.GetCurrentAnimatorStateInfo(0);
                if (st.IsName(deathStateName)) { entered = true; break; }
                guard -= Time.deltaTime;
                yield return null;
            }

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
                yield return new WaitForSeconds(deathCleanupDelay);
            }
        }
        else
        {
            yield return new WaitForSeconds(deathCleanupDelay);
        }

        Destroy(gameObject);
    }

    public void OnDeathAnimationFinished()
    {
        if (destroyOnDeath)
            Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        if (hp >= maxHP) return;
        
        hp += amount;
        hp = Mathf.Min(hp, maxHP);
        
        Debug.Log($"Player curou {amount} HP! Vida: {hp}/{maxHP}");
        
        // ← NECESSÁRIO: Notifica mudança de vida
        OnHealthChanged?.Invoke(hp, maxHP);
        
        if (anim) anim.SetTrigger("Heal");
    }

    IEnumerator LoadGameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] gameOverSceneName não definido!");
        }
    }
}