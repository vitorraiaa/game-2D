using UnityEngine;
using UnityEngine.Tilemaps;

public class AcidTile : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugLogs = true;

    void Start()
    {
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] AcidTile script iniciado!");
        
        // Verifica se tem CompositeCollider2D configurado (que é o trigger principal)
        CompositeCollider2D compositeCol = GetComponent<CompositeCollider2D>();
        if (compositeCol == null)
        {
            Debug.LogError($"[{gameObject.name}] ERRO: Não tem CompositeCollider2D!");
        }
        else if (!compositeCol.isTrigger)
        {
            Debug.LogError($"[{gameObject.name}] ERRO: CompositeCollider2D não está marcado como Trigger!");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] CompositeCollider2D configurado corretamente como Trigger");
        }
        
        // Verifica se o TilemapCollider2D está configurado para usar o Composite
        TilemapCollider2D tilemapCol = GetComponent<TilemapCollider2D>();
        if (tilemapCol != null && tilemapCol.usedByComposite)
        {
            Debug.Log($"[{gameObject.name}] TilemapCollider2D está usando Composite corretamente");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] TilemapCollider2D não está marcado como 'Used By Composite'!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] ✅ TRIGGER ATIVADO! Algo entrou no ácido: {collision.gameObject.name} (Tag: '{collision.tag}')");
        
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                if (showDebugLogs)
                    Debug.Log($"[{gameObject.name}] ☠️ PLAYER CAIU NO ÁCIDO - MORTE INSTANTÂNEA!");
                
                // Mata instantaneamente
                playerHealth.TakeDamage(999);
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[{gameObject.name}] Player não tem componente PlayerHealth!");
            }
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] ❌ Objeto detectado mas não é Player (tag: '{collision.tag}')");
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Debug contínuo para verificar se algo está no trigger
        if (showDebugLogs && collision.CompareTag("Player"))
            Debug.Log($"[{gameObject.name}] 🔄 Player ainda está no ácido!");
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] ➡️ Algo saiu do ácido: {collision.gameObject.name}");
    }
}
