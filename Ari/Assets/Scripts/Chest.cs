using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Chest : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Light2D lightComponent;
    private bool isOpen = false;
    private bool playerNearby = false;
    
    public Sprite closedSprite;
    public Sprite openSprite;
    public GameObject itemPrefab;
    public float lightIntensity = 1f;
    public float jumpForce = 5f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lightComponent = GetComponent<Light2D>();
        
        // IMPORTANTE: Desligar a luz no começo
        if (lightComponent != null)
        {
            lightComponent.enabled = false;
        }
        
        // Começa com sprite fechada
        if (closedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
    }

    void Update()
    {
        // Se o jogador está perto e aperta ESPAÇO
        if (playerNearby && Input.GetKeyDown(KeyCode.Space) && !isOpen)
        {
            OpenChest();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNearby = true;
            
            // SÓ LIGAR A LUZ SE NÃO ESTIVER ABERTO
            if (lightComponent != null && !isOpen)
            {
                lightComponent.enabled = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNearby = false;
            
            // DESLIGAR A LUZ
            if (lightComponent != null)
            {
                lightComponent.enabled = false;
            }
        }
    }

    void OpenChest()
    {
        isOpen = true;
        Debug.Log("Baú aberto!");
        
        // DESLIGAR A LUZ quando abre
        if (lightComponent != null)
        {
            lightComponent.enabled = false;
            Debug.Log("Luz desligada ao abrir!");
        }
        
        // Trocar para sprite aberto
        if (openSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = openSprite;
        }
        
        // Spawn o item
        if (itemPrefab != null)
        {
            GameObject item = Instantiate(itemPrefab, transform.position + Vector3.up, Quaternion.identity);
            
            Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
            if (itemRb != null)
            {
                itemRb.linearVelocity = new Vector2(0, jumpForce);
            }
        }
    }
}