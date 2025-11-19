using UnityEngine;
using System.Collections;

public class EnemyFreezable : MonoBehaviour
{
    private bool isFrozen = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    // Componentes do inimigo que serão desabilitados
    private MonoBehaviour[] enemyScripts;
    private Rigidbody2D rb;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Pegar todos os scripts do inimigo (exceto este)
        enemyScripts = GetComponents<MonoBehaviour>();
    }

    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            StartCoroutine(FreezeCoroutine(duration));
        }
    }

    IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        
        // Efeito visual - azul claro
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.8f, 1f, 1f);
        }
        
        // Parar movimento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        // Desabilitar scripts do inimigo
        foreach (var script in enemyScripts)
        {
            if (script != null && script != this)
            {
                script.enabled = false;
            }
        }
        
        Debug.Log($"{gameObject.name} congelado por {duration}s!");
        
        // Esperar
        yield return new WaitForSeconds(duration);
        
        // Restaurar
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // Reabilitar scripts
        foreach (var script in enemyScripts)
        {
            if (script != null && script != this)
            {
                script.enabled = true;
            }
        }
        
        isFrozen = false;
        Debug.Log($"{gameObject.name} descongelou!");
    }
}