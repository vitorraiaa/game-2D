using UnityEngine;
using System.Collections;

public class ExplodingSkull : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool hasExploded = false;
    
    public Sprite intactSprite;
    public Sprite[] explosionSprites;
    public float animationSpeed = 0.1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (intactSprite != null)
        {
            spriteRenderer.sprite = intactSprite;
        }
        
        Debug.Log("Caveira criada!");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasExploded)
        {
            Debug.Log("Jogador detectado! Caveira vai explodir!");
            StartCoroutine(ExplodeAnimation());
        }
    }

    IEnumerator ExplodeAnimation()
    {
        hasExploded = true;
        Debug.Log("Caveira vai explodir em 1 segundo...");
        
        // Delay antes de explodir
        yield return new WaitForSeconds(1f);
        
        Debug.Log("EXPLOSÃO!");
        
        // Animar a explosão com os 7 frames
        for (int i = 0; i < explosionSprites.Length; i++)
        {
            spriteRenderer.sprite = explosionSprites[i];
            yield return new WaitForSeconds(animationSpeed);
        }
        
        Debug.Log("Explosão terminada!");
        Destroy(gameObject);
    }
}