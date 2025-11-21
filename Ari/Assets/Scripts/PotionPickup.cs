using UnityEngine;

public class PotionPickup : MonoBehaviour
{
    [Header("Tipo de Poção")]
    public PotionType potionType;
    
    [Header("Efeitos")]
    public int healAmount = 1;              // Para poção de vida
    public float speedMultiplier = 1.5f;    // Para poção de velocidade
    public float speedDuration = 5f;        // Duração do speed
    public float freezeDuration = 3f;       // Duração do gelo
    public float freezeRadius = 5f;         // Raio do gelo
    public float jumpMultiplier = 1.5f;     // Para poção de pulo
    public float jumpDuration = 5f;         // Duração do pulo extra
    
    [Header("Efeitos Visuais")]
    public GameObject pickupEffect;         // Partículas ao pegar
    public AudioClip pickupSound;           // Som ao pegar
    
    [Header("Rotação (Opcional)")]
    public bool rotatePotion = true;
    public float rotationSpeed = 50f;
    
    [Header("Flutuação (Opcional)")]
    public bool floatPotion = true;
    public float floatAmplitude = 0.3f;
    public float floatSpeed = 2f;
    private Vector3 startPosition;
    
    [Header("Debug")]
    public bool showDebugLogs = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotação da poção
        if (rotatePotion)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        
        // Flutuação da poção
        if (floatPotion)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyPotionEffect(collision.gameObject);
            
            // Efeito visual
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            // Som
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Destruir a poção
            Destroy(gameObject);
        }
    }

    void ApplyPotionEffect(GameObject player)
    {
        // ✨ NOVO: Ativar efeito visual
        PotionVisualEffect visualEffect = player.GetComponent<PotionVisualEffect>();
        if (visualEffect != null)
        {
            float duration = GetEffectDuration();
            visualEffect.ActivateEffect(potionType, duration);
        }

        switch (potionType)
        {
            case PotionType.Health:
                ApplyHealthPotion(player);
                break;
                
            case PotionType.Speed:
                ApplySpeedPotion(player);
                break;
                
            case PotionType.ExtraAttack:
                ApplyExtraAttackPotion(player);
                break;
                
            case PotionType.Freeze:
                ApplyFreezePotion(player);
                break;
                
            case PotionType.ExtraJump:
                ApplyExtraJumpPotion(player);
                break;
        }
    }

// ✨ NOVO: Método auxiliar para pegar a duração correta
float GetEffectDuration()
{
    switch (potionType)
    {
        case PotionType.Health:
            return 2f; // Efeito rápido só visual
        case PotionType.Speed:
            return speedDuration;
        case PotionType.ExtraAttack:
            return 10f; // Ou quanto tempo durar o extra attack
        case PotionType.Freeze:
            return freezeDuration;
        case PotionType.ExtraJump:
            return jumpDuration;
        default:
            return 5f;
    }
}

    void ApplyHealthPotion(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        
        if (health != null)
        {
            health.Heal(healAmount);
            
            if (showDebugLogs)
                Debug.Log($"Poção de vida coletada! +{healAmount} HP");
        }
    }

    void ApplySpeedPotion(GameObject player)
    {
        PlayerMovement2D movement = player.GetComponent<PlayerMovement2D>();
        
        if (movement != null)
        {
            PlayerPotionEffects effects = player.GetComponent<PlayerPotionEffects>();
            if (effects == null)
            {
                effects = player.AddComponent<PlayerPotionEffects>();
            }
            
            effects.ApplySpeedBoost(speedMultiplier, speedDuration);
            
            if (showDebugLogs)
                Debug.Log($"Poção de velocidade! x{speedMultiplier} por {speedDuration}s");
        }
    }

    void ApplyExtraAttackPotion(GameObject player)
    {
        PlayerShoot2D shooter = player.GetComponent<PlayerShoot2D>();
        
        if (shooter != null)
        {
            PlayerPotionEffects effects = player.GetComponent<PlayerPotionEffects>();
            if (effects == null)
            {
                effects = player.AddComponent<PlayerPotionEffects>();
            }
            
            effects.EnableExtraAttack();
            
            if (showDebugLogs)
                Debug.Log("Extra Attack ativado!");
        }
    }

    void ApplyFreezePotion(GameObject player)
    {
        Debug.Log($"[POÇÃO GELO] Iniciando congelamento...");
        Debug.Log($"[POÇÃO GELO] Posição do player: {player.transform.position}");
        Debug.Log($"[POÇÃO GELO] Raio de busca: {freezeRadius}");
        
        // Congela todos os inimigos próximos
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, freezeRadius);
        
        Debug.Log($"[POÇÃO GELO] Encontrados {colliders.Length} colliders no raio");
        
        int frozenCount = 0;
        foreach (Collider2D col in colliders)
        {
            Debug.Log($"[POÇÃO GELO] Verificando: {col.gameObject.name} - Tag: {col.tag}");
            
            if (col.CompareTag("Enemy"))
            {
                Debug.Log($"[POÇÃO GELO] {col.gameObject.name} é um inimigo! Congelando...");
                
                EnemyFreezable freezable = col.GetComponent<EnemyFreezable>();
                if (freezable == null)
                {
                    Debug.Log($"[POÇÃO GELO] Adicionando EnemyFreezable em {col.gameObject.name}");
                    freezable = col.gameObject.AddComponent<EnemyFreezable>();
                }
                
                freezable.Freeze(freezeDuration);
                frozenCount++;
            }
        }
        
        Debug.Log($"[POÇÃO GELO] Total congelado: {frozenCount} inimigos por {freezeDuration}s");
        
        if (frozenCount == 0)
        {
            Debug.LogWarning("[POÇÃO GELO] NENHUM INIMIGO FOI CONGELADO! Verifique:");
            Debug.LogWarning("1. Os inimigos têm a tag 'Enemy'?");
            Debug.LogWarning("2. Os inimigos estão dentro do raio?");
            Debug.LogWarning($"3. Raio atual: {freezeRadius}");
        }
    }

    void ApplyExtraJumpPotion(GameObject player)
    {
        PlayerMovement2D movement = player.GetComponent<PlayerMovement2D>();
        
        if (movement != null)
        {
            PlayerPotionEffects effects = player.GetComponent<PlayerPotionEffects>();
            if (effects == null)
            {
                effects = player.AddComponent<PlayerPotionEffects>();
            }
            
            effects.ApplyJumpBoost(jumpMultiplier, jumpDuration);
            
            if (showDebugLogs)
                Debug.Log($"Poção de pulo! x{jumpMultiplier} por {jumpDuration}s");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (potionType == PotionType.Freeze)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawSphere(transform.position, freezeRadius);
        }
    }
}

// Enum para os tipos de poção
public enum PotionType
{
    Health,      // Poção vermelha - cura vida
    Speed,       // Poção amarela - aumenta velocidade
    ExtraAttack, // Poção roxa - libera extra attack
    Freeze,      // Poção azul - congela inimigos
    ExtraJump    // Poção ciano - aumenta pulo
}