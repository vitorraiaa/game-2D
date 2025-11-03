using UnityEngine;

public class Item : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D collider;
    private bool touchedGround = false;
    private bool pickedUp = false;
    
    [Header("Item Physics")]
    public float randomForceX = 3f; // Força lateral aleatória
    public float lifeTime = 30f; // Tempo de vida do item (segundos)
    
    [Header("Visual Effects")]
    public float bounceForce = 5f; // Força do bounce quando toca o chão
    public bool enableBounce = true;
    
    private float spawnTime;
    private bool hasBounced = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
        spawnTime = Time.time;
        
        Debug.Log("Item criado!");
        
        // Adiciona força aleatória para os lados (não muito exagerado)
        float randomX = Random.Range(-randomForceX, randomForceX);
        rb.linearVelocity = new Vector2(randomX, rb.linearVelocity.y);
        
        Debug.Log($"Item saiu com força lateral: {randomX}");
        
        // Destroi o item após o tempo de vida
        Destroy(gameObject, lifeTime);
    }
    
    void Update()
    {
        // Pisca quando está perto de desaparecer
        if (Time.time - spawnTime > lifeTime - 5f)
        {
            float blinkSpeed = 5f;
            bool shouldShow = Mathf.Sin(Time.time * blinkSpeed) > 0;
            GetComponent<SpriteRenderer>().enabled = shouldShow;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Detecta quando toca no chão
        if (collision.CompareTag("Ground") && !pickedUp)
        {
            touchedGround = true;
            Debug.Log("Item tocou no chão! Pode pegar agora.");
            
            // Adiciona um bounce quando toca o chão pela primeira vez
            if (enableBounce && !hasBounced)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
                hasBounced = true;
            }
        }
        
        // Detecta quando o Player passa por cima
        if (collision.CompareTag("Player") && touchedGround && !pickedUp)
        {
            PickUp();
        }
    }

    void PickUp()
    {
        pickedUp = true;
        Debug.Log("Item pego!");
        
        // Aqui você pode adicionar efeitos visuais, sons, etc.
        // Por exemplo, uma animação de coleta ou partículas
        
        Destroy(gameObject); // Remove o item da cena
    }
    
    // Método para ser chamado externamente se necessário
    public bool IsPickedUp()
    {
        return pickedUp;
    }
    
    public bool HasTouchedGround()
    {
        return touchedGround;
    }
}