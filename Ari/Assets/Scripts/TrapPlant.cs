using UnityEngine;
using System.Collections;

public class TrapPlant : MonoBehaviour
{
    [Header("Componentes")]
    private Animator animator;
    private Collider2D trapCollider;
    
    [Header("Estado")]
    private bool isOpen = true;
    private bool isClosing = false;
    private GameObject playerInside;
    
    [Header("Timing")]
    public float triggerDelay = 0.5f;      // Delay antes de fechar
    public float closedDuration = 3f;      // Tempo que fica fechada
    public float openAnimationTime = 0.5f; // Duração da animação de abrir
    
    [Header("Dano")]
    public int crushDamage = 2;            // Dano ao fechar
    public bool continuousDamage = true;   // Dano contínuo enquanto fechada?
    public float damageInterval = 0.5f;    // Intervalo entre danos
    private float damageTimer = 0f;
    
    [Header("Efeitos Visuais")]
    public bool shakeBeforeClose = true;
    public float shakeAmount = 0.1f;
    private Vector3 originalPosition;
    
    [Header("Debug")]
    public bool showDebugLogs = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        trapCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
        
        if (animator == null)
        {
            Debug.LogError($"[{gameObject.name}] Animator não encontrado!");
        }
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Planta armadilha armada!");
    }

    void Update()
    {
        // Dano contínuo enquanto player está preso
        if (!isOpen && continuousDamage && playerInside != null)
        {
            damageTimer -= Time.deltaTime;
            
            if (damageTimer <= 0f)
            {
                DealDamageToPlayer(playerInside);
                damageTimer = damageInterval;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isOpen && !isClosing)
        {
            playerInside = collision.gameObject;
            
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Player detectado! Fechando em {triggerDelay}s...");
            
            StartCoroutine(TrapSequence());
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (showDebugLogs && !isOpen)
                Debug.Log($"[{gameObject.name}] Player escapou!");
            
            playerInside = null;
        }
    }

    IEnumerator TrapSequence()
    {
        isClosing = true;
        
        // Efeito de tremor antes de fechar
        if (shakeBeforeClose)
        {
            StartCoroutine(ShakeWarning());
        }
        
        // Delay antes de fechar
        yield return new WaitForSeconds(triggerDelay);
        
        // Fechar a armadilha
        CloseTrap();
        
        // Fica fechada
        yield return new WaitForSeconds(closedDuration);
        
        // Abrir novamente
        OpenTrap();
        
        yield return new WaitForSeconds(openAnimationTime);
        
        isClosing = false;
    }

    void CloseTrap()
    {
        isOpen = false;
        
        if (animator != null)
        {
            animator.SetTrigger("Close");
        }
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] FECHOU!");
        
        // Causar dano inicial ao fechar
        if (playerInside != null)
        {
            DealDamageToPlayer(playerInside);
            damageTimer = damageInterval; // Resetar timer para próximo dano
        }
    }

    void OpenTrap()
    {
        isOpen = true;
        playerInside = null;
        
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] ABRIU!");
    }

    void DealDamageToPlayer(GameObject player)
    {
        if (player == null) return;
        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(crushDamage);
            
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Causou {crushDamage} de dano ao player!");
        }
    }

    IEnumerator ShakeWarning()
    {
        float elapsed = 0f;
        float shakeDuration = triggerDelay * 0.8f; // 80% do tempo de delay
        
        while (elapsed < shakeDuration)
        {
            // Tremor aleatório
            float offsetX = Random.Range(-shakeAmount, shakeAmount);
            float offsetY = Random.Range(-shakeAmount, shakeAmount);
            
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Voltar à posição original
        transform.position = originalPosition;
    }

    // Resetar posição ao desabilitar (segurança)
    void OnDisable()
    {
        transform.position = originalPosition;
    }
}