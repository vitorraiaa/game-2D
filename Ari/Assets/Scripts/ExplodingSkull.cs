using UnityEngine;
using System.Collections;

public class ExplodingSkull : MonoBehaviour
{
    [Header("Componentes")]
    private SpriteRenderer spriteRenderer;
    private Collider2D skullCollider;
    
    [Header("Estado")]
    private bool hasExploded = false;
    private bool playerInRange = false;
    private GameObject playerInTrigger;
    
    [Header("Sprites")]
    public Sprite intactSprite;
    public Sprite[] explosionSprites; // Array com 7 sprites da explosão
    
    [Header("Timing")]
    public float detectionDelay = 1f; // Delay antes de explodir
    public float animationSpeed = 0.1f;
    
    [Header("Dano")]
    public int explosionDamage = 2;
    public float damageRadius = 2f; // Raio de dano da explosão
    public bool showDamageRadius = true; // Mostrar o raio no editor
    
    [Header("Efeitos Visuais")]
    public bool blinkBeforeExplode = true;
    public Color blinkColor = Color.red;
    
    [Header("Debug")]
    public bool showDebugLogs = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        skullCollider = GetComponent<Collider2D>();
        
        // Sprite inicial
        if (intactSprite != null)
        {
            spriteRenderer.sprite = intactSprite;
        }
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Caveira armada!");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasExploded)
        {
            playerInRange = true;
            playerInTrigger = collision.gameObject;
            
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Player detectado! Iniciando contagem...");
            
            StartCoroutine(ExplodeSequence());
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            playerInTrigger = null;
            
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Player saiu do alcance!");
        }
    }

    IEnumerator ExplodeSequence()
    {
        hasExploded = true;
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Explodindo em {detectionDelay} segundos...");
        
        // Efeito de piscar antes de explodir
        if (blinkBeforeExplode)
        {
            StartCoroutine(BlinkWarning());
        }
        
        // Delay antes de explodir
        yield return new WaitForSeconds(detectionDelay);
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] EXPLOSÃO!");
        
        // Causar dano em todos na área
        DealExplosionDamage();
        
        // Desabilitar collider para não interferir
        if (skullCollider != null)
            skullCollider.enabled = false;
        
        // Animar a explosão
        if (explosionSprites != null && explosionSprites.Length > 0)
        {
            for (int i = 0; i < explosionSprites.Length; i++)
            {
                if (explosionSprites[i] != null)
                {
                    spriteRenderer.sprite = explosionSprites[i];
                    yield return new WaitForSeconds(animationSpeed);
                }
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Explosão concluída! Destruindo...");
        
        // Destruir a caveira
        Destroy(gameObject);
    }

    void DealExplosionDamage()
    {
        // Encontrar todos os colliders em um raio
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        
        foreach (Collider2D hit in hitColliders)
        {
            // Verificar se é o player
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamage);
                    
                    if (showDebugLogs)
                        Debug.Log($"[{gameObject.name}] Causou {explosionDamage} de dano ao player!");
                    
                    // Opcional: Aplicar knockback
                    ApplyKnockback(hit.gameObject);
                }
            }
        }
    }

    void ApplyKnockback(GameObject player)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        
        if (playerRb != null)
        {
            // Direção do knockback (para longe da explosão)
            Vector2 direction = (player.transform.position - transform.position).normalized;
            
            // Aplicar força
            float knockbackForce = 8f;
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0);
            playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator BlinkWarning()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        float blinkDuration = detectionDelay;
        float blinkInterval = 0.15f;
        float elapsed = 0f;
        
        // Acelera o piscar conforme se aproxima da explosão
        while (elapsed < blinkDuration)
        {
            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval);
            
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
            
            elapsed += blinkInterval * 2;
            blinkInterval = Mathf.Max(0.05f, blinkInterval * 0.9f); // Acelera
        }
        
        spriteRenderer.color = originalColor;
    }

    // Visualizar o raio de dano no editor
    void OnDrawGizmosSelected()
    {
        if (showDamageRadius)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, damageRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
}