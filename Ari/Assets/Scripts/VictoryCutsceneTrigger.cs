using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryCutsceneTrigger : MonoBehaviour
{
    [Header("Cena de vitória que será carregada")]
    [Tooltip("Nome da cena da cutscene nas Build Settings")]
    public string victorySceneName = "Cutscene_Victory";

    [Header("Tempo de espera antes de carregar (segundos)")]
    public float delaySeconds = 3f;

    [Header("Evitar múltiplas ativações")]
    public bool oneShot = true;
    private bool _triggered;

    [Header("Debug")]
    public bool showDebugLogs = true;

    /// <summary>
    /// Chame este método quando o boss morrer.
    /// Ex.: no script do Boss: FindObjectOfType<VictoryCutsceneTrigger>().OnBossDefeated();
    /// ou via Animation Event.
    /// </summary>
    public void OnBossDefeated()
    {
        if (_triggered && oneShot) return;
        _triggered = true;

        if (showDebugLogs)
            Debug.Log($"[VictoryCutsceneTrigger] Boss derrotado. Carregando '{victorySceneName}' em {delaySeconds:0.##}s...");

        StartCoroutine(LoadCutsceneAfterDelay());
    }

    private System.Collections.IEnumerator LoadCutsceneAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        if (showDebugLogs)
            Debug.Log($"[VictoryCutsceneTrigger] Carregando cena: {victorySceneName}");

        // Certifique-se que a cena está nas Build Settings (File > Build Settings)
        SceneManager.LoadScene(victorySceneName, LoadSceneMode.Single);
    }
}
