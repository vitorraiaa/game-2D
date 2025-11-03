using UnityEngine;
using System.Collections;

public class TrapPlant : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = true;
    
    public float triggerDelay = 0.5f;
    public float closedDuration = 5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("Animator não encontrado! Adicione um Animator ao TrapPlant");
        }
        
        Debug.Log("Planta armadilha criada!");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isOpen)
        {
            Debug.Log("Jogador detectado! Planta vai fechar...");
            StartCoroutine(TrapSequence());
        }
    }

    IEnumerator TrapSequence()
    {
        isOpen = false;
        
        // Delay antes de fechar
        yield return new WaitForSeconds(triggerDelay);
        
        // Toca animação de fechamento
        animator.SetTrigger("Close");
        Debug.Log("Planta FECHOU!");
        
        // Fica fechada por alguns segundos
        yield return new WaitForSeconds(closedDuration);
        
        // Toca animação de abertura
        animator.SetTrigger("Open");
        isOpen = true;
        Debug.Log("Planta ABRIU novamente!");
    }
}