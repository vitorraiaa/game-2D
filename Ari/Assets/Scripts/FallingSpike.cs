using UnityEngine;
using System.Collections;

public class FallingSpike : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool hasHit = false;
    
    public Sprite[] breakSprites;    // Array com 5 sprites da quebra
    public float animationSpeed = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        Debug.Log("Espinho caindo!");
    }

    public float respawnHeight = 10f;  // Altura para respa dar
    public float respawnDelay = 2f;    // Tempo antes de respa dar

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasHit)
        {
            Debug.Log($"Espinho bateu em: {collision.gameObject.name}");
            StartCoroutine(CrackAnimation());
        }
    }

    IEnumerator CrackAnimation()
    {
        hasHit = true;
        Debug.Log("Espinho QUEBRANDO!");
        
        // Parar movimento
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        
        // Animar a quebra
        for (int i = 0; i < breakSprites.Length; i++)
        {
            spriteRenderer.sprite = breakSprites[i];
            yield return new WaitForSeconds(animationSpeed);
        }
        
        Debug.Log("Esperando para respa dar...");
        yield return new WaitForSeconds(respawnDelay);
        
        // Respa dar
        Respawn();
    }

    void Respawn()
    {
        Debug.Log("Espinho respawning!");
        hasHit = false;
        
        // Resetar sprite para a inicial
        if (breakSprites.Length > 0)
        {
            spriteRenderer.sprite = breakSprites[0];
        }
        
        // Voltar ao topo
        transform.position = new Vector3(transform.position.x, respawnHeight, 0);
        
        // Ativar gravidade novamente
        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;
    }
}