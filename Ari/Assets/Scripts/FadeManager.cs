using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup canvasGroup;   // arraste o CanvasGroup do FadePanel

    [Header("Config")]
    public float duration = 0.6f;     // tempo do fade
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Reset()
    {
        // tenta pegar automaticamente
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
    }

    public IEnumerator FadeIn()  // tela escura -> visível
    {
        yield return Fade(1f, 0f);
    }

    public IEnumerator FadeOut() // visível -> tela escura
    {
        yield return Fade(0f, 1f);
    }

    private IEnumerator Fade(float from, float to)
    {
        Debug.Log($"Fazendo fade de {from} para {to}");
        float t = 0f;
        canvasGroup.blocksRaycasts = true; // bloqueia clique durante transição
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = curve.Evaluate(t);
            canvasGroup.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        canvasGroup.alpha = to;
        canvasGroup.blocksRaycasts = (to > 0.99f); // só bloqueia se estiver escuro
    }
}
