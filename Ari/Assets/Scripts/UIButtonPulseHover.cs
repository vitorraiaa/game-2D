using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonPulseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referências")]
    public Graphic targetGraphic;        // Image ou Text (TMP usa TMP_Text que herda de Graphic via TMP_SubMeshUI)
    public Transform targetTransform;    // Normalmente o próprio transform

    [Header("Pulso (idle)")]
    public bool pulseEnabled = true;
    public float pulseMinScale = 0.95f;
    public float pulseMaxScale = 1.05f;
    public float pulseSpeed = 2.0f;      // Hz do “vai e volta”

    [Header("Hover")]
    public float hoverScale = 1.08f;
    public Color hoverColor = new Color(1f, 0.95f, 0.5f, 1f); // levemente amarelado
    public float tweenSpeed = 10f;       // rapidez do “lerp”

    // estado interno
    private Vector3 _baseScale;
    private Color _baseColor;
    private bool _hovering;

    void Reset()
    {
        targetTransform = transform;
        targetGraphic = GetComponent<Graphic>();
    }

    void Awake()
    {
        if (targetTransform == null) targetTransform = transform;
        _baseScale = targetTransform.localScale;

        if (targetGraphic != null) _baseColor = targetGraphic.color;
    }

    void Update()
    {
        // 1) Pulso “idle”
        float targetScale = _baseScale.x;
        if (pulseEnabled && !_hovering)
        {
            float t = (Mathf.Sin(Time.time * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f; // 0..1
            float s = Mathf.Lerp(pulseMinScale, pulseMaxScale, t);
            targetScale = _baseScale.x * s;
        }

        // 2) Hover ajusta alvo de escala/cor
        if (_hovering)
            targetScale = _baseScale.x * hoverScale;

        // Lerp suave para a escala alvo
        Vector3 desired = new Vector3(targetScale, targetScale, _baseScale.z);
        targetTransform.localScale = Vector3.Lerp(targetTransform.localScale, desired, Time.deltaTime * tweenSpeed);

        // Lerp suave para a cor (se tiver Graphic)
        if (targetGraphic)
        {
            Color desiredColor = _hovering ? hoverColor : _baseColor;
            targetGraphic.color = Color.Lerp(targetGraphic.color, desiredColor, Time.deltaTime * tweenSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
    }
}