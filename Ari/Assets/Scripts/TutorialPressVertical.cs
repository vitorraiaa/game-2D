using UnityEngine;
using TMPro;

public class TutorialPressVertical : MonoBehaviour
{
    [Header("Objeto do texto no MUNDO (TMP 3D / Canvas World Space)")]
    public GameObject worldText;           // arraste o GameObject do texto
    [Tooltip("Opcional: texto para atualizar a instrução após a 1ª tecla")]
    public TMP_Text hintLabel;             // arraste o TMP_Text (pode ser filho do worldText)

    [Header("Mensagens (opcional)")]
    [TextArea] public string initialMsg = "Aperte W/↑ para subir e S/↓ para descer";
    [TextArea] public string afterUpMsg  = "Agora aperte S/↓ para descer";
    [TextArea] public string afterDownMsg= "Agora aperte W/↑ para subir";

    [Header("Comportamento ao concluir")]
    public bool destroyAfterComplete = false;
    public float destroyDelay = 0.1f;

    private bool pressedUp;
    private bool pressedDown;
    private bool finished;

    void Reset()
    {
        worldText = gameObject;
        hintLabel = GetComponentInChildren<TMP_Text>();
    }

    void Start()
    {
        if (hintLabel != null && !string.IsNullOrEmpty(initialMsg))
            hintLabel.text = initialMsg;

        if (worldText != null)
            worldText.SetActive(true);
    }

    void Update()
    {
        if (finished) return;

        // detectar “subir”
        if (!pressedUp && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            pressedUp = true;
            if (!pressedDown && hintLabel) hintLabel.text = afterUpMsg;
        }

        // detectar “descer”
        if (!pressedDown && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            pressedDown = true;
            if (!pressedUp && hintLabel) hintLabel.text = afterDownMsg;
        }

        // concluiu quando apertou pelo menos uma vez cada direção
        if (pressedUp && pressedDown)
            Complete();
    }

    private void Complete()
    {
        finished = true;

        if (worldText != null)
        {
            worldText.SetActive(false);
            if (destroyAfterComplete)
                Destroy(worldText, destroyDelay);
        }
    }
}
