using UnityEngine;

public class TutorialHideOnShift : MonoBehaviour
{
    [Header("Objeto do texto no MUNDO (TMP 3D / Canvas World Space)")]
    public GameObject worldText; // arraste aqui o GameObject do seu texto

    [Header("Opções")]
    public bool anyShift = true;         // true: aceita LeftShift ou RightShift
    public KeyCode specificKey = KeyCode.LeftShift; // usado se anyShift = false
    public bool oneShot = true;          // evita reaparecer/reativar
    public bool destroyAfterHide = false;
    public float destroyDelay = 0.1f;

    private bool _hidden;

    void Reset()
    {
        worldText = gameObject; // por padrão, usa o próprio objeto
    }

    void Start()
    {
        if (worldText != null) worldText.SetActive(true);
    }

    void Update()
    {
        if (_hidden) return;

        bool pressed = anyShift
            ? (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            : Input.GetKeyDown(specificKey);

        if (pressed)
            HideNow();
    }

    private void HideNow()
    {
        _hidden = true;
        if (worldText != null)
        {
            worldText.SetActive(false);

            if (destroyAfterHide)
                Destroy(worldText, destroyDelay);
        }

        if (!oneShot) _hidden = false; // permitir mostrar de novo se reativar em outro fluxo
    }
}
