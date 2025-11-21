using UnityEngine;
using System.Collections;

public class PotionVisualEffect : MonoBehaviour
{
    [Header("Cores por Tipo de Poção")]
    public Color healthColor = new Color(1f, 0.4f, 0.4f, 1f);      // Vermelho claro
    public Color speedColor = new Color(1f, 0.9f, 0.3f, 1f);       // Amarelo
    public Color extraAttackColor = new Color(0.8f, 0.4f, 1f, 1f); // Roxo claro
    public Color freezeColor = new Color(0.4f, 0.8f, 1f, 1f);      // Azul claro
    public Color extraJumpColor = new Color(0.4f, 1f, 0.8f, 1f);   // Ciano claro
    
    [Header("Configurações do Efeito")]
    public float effectIntensity = 0.6f;  // Intensidade do brilho (0-1)
    public float fadeSpeed = 3f;          // Velocidade de fade in/out
    public bool pulseEffect = true;       // Efeito pulsante
    public float pulseSpeed = 3f;         // Velocidade da pulsação
    
    SpriteRenderer spriteRenderer;
    Color originalColor;
    Coroutine currentEffect;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Ativa o efeito baseado no tipo de poção
    public void ActivateEffect(PotionType type, float duration)
    {
        Color effectColor = GetColorForPotionType(type);
        
        // Se já tem um efeito rodando, cancela
        if (currentEffect != null)
        {
            StopCoroutine(currentEffect);
        }
        
        if (pulseEffect)
        {
            currentEffect = StartCoroutine(PulsingEffect(effectColor, duration));
        }
        else
        {
            currentEffect = StartCoroutine(SimpleEffect(effectColor, duration));
        }
    }

    Color GetColorForPotionType(PotionType type)
    {
        switch (type)
        {
            case PotionType.Health:
                return healthColor;
            case PotionType.Speed:
                return speedColor;
            case PotionType.ExtraAttack:
                return extraAttackColor;
            case PotionType.Freeze:
                return freezeColor;
            case PotionType.ExtraJump:
                return extraJumpColor;
            default:
                return Color.white;
        }
    }

    IEnumerator SimpleEffect(Color effectColor, float duration)
    {
        if (spriteRenderer == null) yield break;

        // Fade In
        float elapsed = 0f;
        while (elapsed < 1f / fadeSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * fadeSpeed;
            spriteRenderer.color = Color.Lerp(originalColor, effectColor, t * effectIntensity);
            yield return null;
        }

        // Mantém o efeito
        yield return new WaitForSeconds(duration);

        // Fade Out
        elapsed = 0f;
        Color currentColor = spriteRenderer.color;
        while (elapsed < 1f / fadeSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * fadeSpeed;
            spriteRenderer.color = Color.Lerp(currentColor, originalColor, t);
            yield return null;
        }
        
        spriteRenderer.color = originalColor;
        currentEffect = null;
    }

    IEnumerator PulsingEffect(Color effectColor, float duration)
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            // Calcula a pulsação (0 a 1 e volta)
            float pulse = (Mathf.Sin(elapsed * pulseSpeed) + 1f) / 2f;
            
            // Interpola entre cor original e cor do efeito
            spriteRenderer.color = Color.Lerp(originalColor, effectColor, pulse * effectIntensity);
            
            yield return null;
        }

        // Fade Out suave
        float fadeElapsed = 0f;
        Color currentColor = spriteRenderer.color;
        while (fadeElapsed < 1f / fadeSpeed)
        {
            fadeElapsed += Time.deltaTime;
            float t = fadeElapsed * fadeSpeed;
            spriteRenderer.color = Color.Lerp(currentColor, originalColor, t);
            yield return null;
        }
        
        spriteRenderer.color = originalColor;
        currentEffect = null;
    }

    // Cancela o efeito imediatamente
    public void CancelEffect()
    {
        if (currentEffect != null)
        {
            StopCoroutine(currentEffect);
            currentEffect = null;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}