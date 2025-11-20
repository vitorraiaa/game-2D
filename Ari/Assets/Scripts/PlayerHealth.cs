using UnityEngine;
using UnityEngine.SceneManagement; // <-- pra carregar cena
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

    [Header("Game Over")]
    public string gameOverSceneName = "GameOver"; // cena de Game Over
    public float gameOverDelay = 1.0f;            // tempo até carregar Game Over

    [Header("Morte por queda")]
    public float killY = -20f; // se o Y do player ficar menor que isso, ele morre

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

        // 🔥 LÓGICA DE MORTE POR QUEDA
        if (!dead && transform.position.y < killY)
        {
            HandleDeath();
        }
    }

    public void TakeDamage(int amount)
    {
        if (dead || invulTimer > 0f) return;

        hp -= Mathf.Max(1, amount);
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

        // aciona animação de morte
        if (anim) anim.SetTrigger("Dead");

        // desativa controles / movimento / colisores, mas deixa o sprite/anim rodando
        var move = GetComponent<PlayerMovement2D>(); if (move) move.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var rb = GetComponent<Rigidbody2D>(); if (rb) rb.simulated = false;

        // rotina de sumir
        if (destroyOnDeath)
            StartCoroutine(WaitAndDisappear());

        // rotina de carregar Game Over
        StartCoroutine(LoadGameOverAfterDelay());
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

    IEnumerator LoadGameOverAfterDelay()
    {
        // espera um tempo (pra deixar a animação de morte aparecer bonitinha)
        yield return new WaitForSeconds(gameOverDelay);

        // carrega a cena de Game Over
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
