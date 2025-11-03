using UnityEngine;

public class SwingingAxe : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    public Sprite[] swingSprites;    // Array com 6 sprites do balanço
    public float animationSpeed = 0.1f;
    private int currentFrame = 0;
    private float timeSinceLastFrame = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (swingSprites.Length > 0)
        {
            spriteRenderer.sprite = swingSprites[0];
        }
        
        Debug.Log("Machado criado e balançando!");
    }

    void Update()
    {
        // Controla a animação
        timeSinceLastFrame += Time.deltaTime;
        
        if (timeSinceLastFrame >= animationSpeed)
        {
            timeSinceLastFrame = 0f;
            
            // Próximo frame
            currentFrame++;
            
            // Volta ao início quando chega no final
            if (currentFrame >= swingSprites.Length)
            {
                currentFrame = 0;
            }
            
            // Atualiza sprite
            spriteRenderer.sprite = swingSprites[currentFrame];
        }
    }
}