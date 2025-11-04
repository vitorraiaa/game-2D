using UnityEngine;
using System.Collections;

public class FallingSpike : MonoBehaviour
{
    [Header("Componentes")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D spikeCollider;
    
    [Header("Estado")]
    private bool hasHit = false;
    private bool canDamage = true;
    
    [Header("Animação de Quebra")]
    public Sprite[] breakSprites;    // Array com sprites da quebra (drag in order)
    public float animationSpeed = 0.1f;
    
    [Header("Respawn")]
    public float respawnHeight = 10f;
    public float respawnDelay = 2f;
    
    [Header("Dano")]
    public int damage = 1;
    public bool damageOnFall = true;  // Causa dano ao cair no player?
    
    [Header("Debug")]
    public bool showDebugLogs = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spikeCollider = GetComponent<Collider2D>();
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Espinho iniciado!");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Espinho bateu em: {collision.gameObject.name}");
        
        // Se caiu no PLAYER, causa dano
        if (damageOnFall && canDamage && collision.gameObject.CompareTag("Player"))
        {
            DealDamageToPlayer(collision.gameObject);
        }
        
        // Iniciar animação de quebra (independente de quem bateu)
        StartCoroutine(CrackAnimation());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Caso você queira que cause dano como trigger também
        if (damageOnFall && canDamage && !hasHit && other.CompareTag("Player"))
        {
            DealDamageToPlayer(other.gameObject);
        }
    }

    void DealDamageToPlayer(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            canDamage = false; // Previne múltiplos danos
            
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Causou {damage} de dano ao player!");
        }
    }

    IEnumerator CrackAnimation()
    {
        hasHit = true;
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Iniciando animação de quebra...");
        
        // Parar movimento
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Static; // Congela completamente
        
        // Desabilitar collider para não interferir
        if (spikeCollider != null)
            spikeCollider.enabled = false;
        
        // Animar a quebra
        if (breakSprites != null && breakSprites.Length > 0)
        {
            for (int i = 0; i < breakSprites.Length; i++)
            {
                if (breakSprites[i] != null)
                {
                    spriteRenderer.sprite = breakSprites[i];
                    yield return new WaitForSeconds(animationSpeed);
                }
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Quebrou! Esperando {respawnDelay}s para respawn...");
        
        // Esperar antes de respawnar
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawnar
        Respawn();
    }

    void Respawn()
    {
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Respawnando!");
        
        // Reset dos estados
        hasHit = false;
        canDamage = true;
        
        // Resetar sprite para o primeiro (inteiro)
        if (breakSprites != null && breakSprites.Length > 0 && breakSprites[0] != null)
        {
            spriteRenderer.sprite = breakSprites[0];
        }
        
        // Voltar à posição inicial no topo
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(currentPos.x, respawnHeight, currentPos.z);
        
        // Reativar física
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;
        
        // Reativar collider
        if (spikeCollider != null)
            spikeCollider.enabled = true;
    }

    // Método para ser chamado externamente (opcional)
    public void ForceRespawn()
    {
        StopAllCoroutines();
        Respawn();
    }
}