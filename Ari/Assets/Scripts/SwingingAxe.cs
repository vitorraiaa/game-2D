using UnityEngine;

public class SwingingAxe : MonoBehaviour
{
    [Header("Componentes")]
    private SpriteRenderer spriteRenderer;
    private Collider2D axeCollider;
    
    [Header("Animação")]
    public Sprite[] swingSprites;    // Array com sprites do balanço
    public float animationSpeed = 0.1f;
    private int currentFrame = 0;
    private float timeSinceLastFrame = 0f;
    
    [Header("Dano")]
    public int damage = 1;
    public bool damageOnlyOnDownswing = true; // Dano só na descida?
    public int downswingStartFrame = 0;       // Frame onde começa a descida
    public int downswingEndFrame = 3;         // Frame onde termina a descida
    
    [Header("Cooldown de Dano")]
    public float damageCooldown = 0.5f; // Previne múltiplos hits instantâneos
    private float lastDamageTime = -999f;
    
    [Header("Efeitos")]
    public bool playSwingSound = false;
    public AudioClip swingSound;
    private AudioSource audioSource;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showDamageFrames = true; // Visualizar frames de dano no editor

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        axeCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Sprite inicial
        if (swingSprites != null && swingSprites.Length > 0)
        {
            spriteRenderer.sprite = swingSprites[0];
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Swing Sprites não configurados!");
        }
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Machado armado e balançando!");
    }

    void Update()
    {
        AnimateSwing();
    }

    void AnimateSwing()
    {
        timeSinceLastFrame += Time.deltaTime;
        
        if (timeSinceLastFrame >= animationSpeed)
        {
            timeSinceLastFrame = 0f;
            
            int previousFrame = currentFrame;
            
            // Próximo frame
            currentFrame++;
            
            // Loop da animação
            if (currentFrame >= swingSprites.Length)
            {
                currentFrame = 0;
            }
            
            // Atualiza sprite
            if (swingSprites[currentFrame] != null)
            {
                spriteRenderer.sprite = swingSprites[currentFrame];
            }
            
            // Som ao começar o downswing
            if (playSwingSound && currentFrame == downswingStartFrame && audioSource != null && swingSound != null)
            {
                audioSource.PlayOneShot(swingSound);
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Verificar se pode causar dano
        if (!CanDealDamage())
            return;
        
        // Verificar se é o player
        if (collision.CompareTag("Player"))
        {
            DealDamageToPlayer(collision.gameObject);
        }
    }

    bool CanDealDamage()
    {
        // Verifica cooldown
        if (Time.time - lastDamageTime < damageCooldown)
            return false;
        
        // Se configurado para dano só no downswing
        if (damageOnlyOnDownswing)
        {
            return IsInDownswingFrame();
        }
        
        return true;
    }

    bool IsInDownswingFrame()
    {
        // Verifica se está nos frames de descida do machado
        if (downswingEndFrame >= downswingStartFrame)
        {
            // Sequência normal (ex: frames 0 a 3)
            return currentFrame >= downswingStartFrame && currentFrame <= downswingEndFrame;
        }
        else
        {
            // Sequência que passa pelo 0 (ex: frames 5, 0, 1)
            return currentFrame >= downswingStartFrame || currentFrame <= downswingEndFrame;
        }
    }

    void DealDamageToPlayer(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
            
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Machado causou {damage} de dano! Frame: {currentFrame}");
        }
    }

    // Visualização dos frames de dano no editor
    void OnDrawGizmos()
    {
        if (!showDamageFrames || !damageOnlyOnDownswing)
            return;
        
        // Desenha linha vermelha na direção do swing durante frames de dano
        if (Application.isPlaying && IsInDownswingFrame())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        else if (Application.isPlaying)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }

    // Método para pausar/retomar animação (opcional)
    public void SetAnimationEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    // Método para mudar velocidade em runtime (opcional)
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0.01f, speed);
    }
}