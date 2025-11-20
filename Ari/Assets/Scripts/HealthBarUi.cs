using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Componentes da Barra")]
    public Image healthFillImage;
    
    void Start()
    {
        // Encontra o PlayerHealth automaticamente
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Conecta o evento automaticamente
            playerHealth.OnHealthChanged.AddListener(OnPlayerHealthChanged);
            
            // Atualiza a barra com a vida inicial
            OnPlayerHealthChanged(playerHealth.maxHP, playerHealth.maxHP);
        }
        else
        {
            Debug.LogError("HealthBarUI não encontrou PlayerHealth na scene!");
        }
    }
    
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthFillImage == null) return;
        
        float fillAmount = (float)currentHealth / maxHealth;
        healthFillImage.fillAmount = fillAmount;
    }
    
    public void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }
}