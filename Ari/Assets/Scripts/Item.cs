using UnityEngine;

public class Item : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D collider;
    private bool touchedGround = false;
    private bool pickedUp = false;
    
    public float randomForceX = 3f; // Força lateral aleatória

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
        
        Debug.Log("Item criado!");
        
        // Adiciona força aleatória para os lados (não muito exagerado)
        float randomX = Random.Range(-randomForceX, randomForceX);
        rb.linearVelocity = new Vector2(randomX, rb.linearVelocity.y);
        
        Debug.Log($"Item saiu com força lateral: {randomX}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Detecta quando toca no chão
        if (collision.CompareTag("Ground") && !pickedUp)
        {
            touchedGround = true;
            Debug.Log("Item tocou no chão! Pode pegar agora.");
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
        Destroy(gameObject); // Remove o item da cena
    }
}