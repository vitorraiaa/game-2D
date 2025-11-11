using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("Cena de Cutscene que será carregada")]
    public string cutsceneSceneName = "Cutscene_Dungeon"; // nome da cena de cutscene

    [Header("Evitar múltiplas ativações")]
    public bool oneShot = true;
    private bool _played = false;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] Colidiu com: {other.name} (tag: {other.tag})");

        if (_played && oneShot) 
        {
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Trigger já foi ativado, ignorando...");
            return;
        }

        if (!other.CompareTag("Player")) 
        {
            if (showDebugLogs)
                Debug.Log($"[{gameObject.name}] Não é o Player, ignorando colisão com {other.name}");
            return;
        }

        _played = true;
        
        if (showDebugLogs)
            Debug.Log($"[{gameObject.name}] ✅ TRIGGER ATIVADO! Carregando cena: {cutsceneSceneName}");
            
        SceneManager.LoadScene(cutsceneSceneName);
    }
}