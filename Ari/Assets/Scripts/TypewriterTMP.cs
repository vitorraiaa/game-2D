using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterTMP : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;

    [Header("Texto e velocidade")]
    [TextArea(3, 10)] public string fullText = "Seu texto aqui...";
    public float charsPerSecond = 35f;       // velocidade básica

    [Header("Opções")]
    public bool useUnscaledTime = true;      // ignora Time.timeScale (menus/pausas)
    public bool skipWithAnyKey = true;       // clique/tecla revela tudo

    [Header("Pausas por pontuação (segundos)")]
    public float pauseComma = 0.08f;
    public float pauseDot = 0.18f;
    public float pauseDash = 0.12f;
    public float pauseEllipsis = 0.25f;      // "..."

    private Coroutine typing;

    void Reset()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        StartTyping();
    }

    public void StartTyping(string overrideText = null)
    {
        if (overrideText != null) fullText = overrideText;
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(TypeRoutine());
    }

    public bool IsFinished =>
        tmp && tmp.maxVisibleCharacters >= tmp.textInfo.characterCount;

    private IEnumerator TypeRoutine()
    {
        if (!tmp) tmp = GetComponent<TextMeshProUGUI>();
        if (!tmp) yield break;

        tmp.text = fullText;
        tmp.ForceMeshUpdate();
        tmp.maxVisibleCharacters = 0;

        int total = tmp.textInfo.characterCount;
        int shown = 0;

        while (shown < total)
        {
            // pular para o fim
            if (skipWithAnyKey && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                tmp.maxVisibleCharacters = total;
                break;
            }

            shown++;
            tmp.maxVisibleCharacters = shown;

            // tempo base por caractere
            float dt = 1f / Mathf.Max(1f, charsPerSecond);
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(dt);
            else yield return new WaitForSeconds(dt);

            // pausa extra por pontuação
            char c = tmp.textInfo.characterInfo[Mathf.Clamp(shown - 1, 0, total - 1)].character;

            // ellipsis "..." tem prioridade
            bool didEllipsisPause = false;
            if (c == '.' && shown - 3 >= 0)
            {
                char c1 = tmp.textInfo.characterInfo[shown - 2].character;
                char c2 = tmp.textInfo.characterInfo[shown - 3].character;
                if (c1 == '.' && c2 == '.')
                {
                    if (useUnscaledTime) yield return new WaitForSecondsRealtime(pauseEllipsis);
                    else yield return new WaitForSeconds(pauseEllipsis);
                    didEllipsisPause = true;
                }
            }

            if (!didEllipsisPause)
            {
                float extra = 0f;
                if (c == ',') extra = pauseComma;
                else if (c == '.' || c == '!' || c == '?') extra = pauseDot;
                else if (c == '-' || c == '—') extra = pauseDash;

                if (extra > 0f)
                {
                    if (useUnscaledTime) yield return new WaitForSecondsRealtime(extra);
                    else yield return new WaitForSeconds(extra);
                }
            }
        }
    }
}
