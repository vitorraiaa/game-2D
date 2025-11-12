using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 200;
    public float deathCleanupDelay = 2.0f;

    [Header("Fases (por vida restante)")]
    [Tooltip("Percentual para entrar na Fase 2 (ex.: 0.70 = 70%).")]
    public float phase2Threshold = 0.70f;
    [Tooltip("Percentual para entrar na Fase 3 (ex.: 0.35 = 35%).")]
    public float phase3Threshold = 0.35f;

    [Header("Anti-spam do Player")]
    [Tooltip("Janela (s) para contar hits em sequência.")]
    public float comboWindow = 1.0f;
    [Tooltip("Se levar esse número de hits dentro da janela, pede 'escape'.")]
    public int comboThreshold = 4;

    public event Action<int> onPhaseChanged;
    public event Action onEscapeRequested;

    int hp;
    int currentPhase = 1;
    bool dead;
    bool invulnerable;

    float lastHitTime;
    int comboCount;

    Animator anim;

    void Awake()
    {
        hp = maxHP;
        anim = GetComponentInChildren<Animator>();
    }

    public int CurrentPhase => currentPhase;

    public void SetInvulnerable(bool value)
    {
        invulnerable = value;
    }

    public void Damage(int amount)
    {
        if (dead || invulnerable) return;

        int dmg = Mathf.Max(1, amount);
        hp -= dmg;

        if (anim) anim.SetTrigger("Hurt");

        // Controle de combos do player
        float now = Time.time;
        if (now - lastHitTime <= comboWindow)
            comboCount++;
        else
            comboCount = 1;

        lastHitTime = now;

        if (comboCount >= comboThreshold)
        {
            comboCount = 0;
            onEscapeRequested?.Invoke();   // BossAI decide o que fazer
        }

        CheckPhase();

        if (hp <= 0)
        {
            dead = true;
            if (anim) anim.SetBool("Dead", true);

            foreach (var c in GetComponentsInChildren<Collider2D>())
                c.enabled = false;

            var rb = GetComponent<Rigidbody2D>();
            if (rb) rb.simulated = false;

            // Triggera a cutscene de vitória
            FindObjectOfType<VictoryCutsceneTrigger>()?.OnBossDefeated();

            Destroy(gameObject, deathCleanupDelay);
        }
    }

    void CheckPhase()
    {
        float p = (float)hp / maxHP;
        if (currentPhase == 1 && p <= phase2Threshold)
        {
            currentPhase = 2;
            onPhaseChanged?.Invoke(2);
        }
        else if (currentPhase == 2 && p <= phase3Threshold)
        {
            currentPhase = 3;
            onPhaseChanged?.Invoke(3);
        }
    }

    // Caso queira usar no cenário depois
    public void Stagger(float duration = 1.5f)
    {
        if (dead) return;
        // Mantido para uso futuro se quiser
    }
}
