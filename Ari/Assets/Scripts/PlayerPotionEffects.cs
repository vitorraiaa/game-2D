using UnityEngine;
using System.Collections;

public class PlayerPotionEffects : MonoBehaviour
{
    private PlayerMovement2D movement;
    private PlayerShoot2D shooter;
    
    private bool speedBoostActive = false;
    private bool jumpBoostActive = false;
    private bool extraAttackActive = false;
    
    private float originalWalkSpeed;
    private float originalRunSpeed;
    private float originalJumpForce;

    void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        shooter = GetComponent<PlayerShoot2D>();
        
        // Salvar valores originais
        if (movement != null)
        {
            originalWalkSpeed = movement.walkSpeed;
            originalRunSpeed = movement.runSpeed;
            originalJumpForce = movement.forcaPulo;
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostActive)
        {
            StopCoroutine("SpeedBoostCoroutine");
        }
        
        StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    public void ApplyJumpBoost(float multiplier, float duration)
    {
        if (jumpBoostActive)
        {
            StopCoroutine("JumpBoostCoroutine");
        }
        
        StartCoroutine(JumpBoostCoroutine(multiplier, duration));
    }

    public void EnableExtraAttack()
    {
        if (shooter != null)
        {
            shooter.hasExtraAttackPower = true;
            extraAttackActive = true;
            Debug.Log("Extra Attack ATIVADO!");
        }
    }

    IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        speedBoostActive = true;
        
        // Aplicar boost
        if (movement != null)
        {
            movement.walkSpeed = originalWalkSpeed * multiplier;
            movement.runSpeed = originalRunSpeed * multiplier;
        }
        
        Debug.Log($"Speed boost ativo! ({duration}s)");
        
        // Esperar
        yield return new WaitForSeconds(duration);
        
        // Restaurar valores originais
        if (movement != null)
        {
            movement.walkSpeed = originalWalkSpeed;
            movement.runSpeed = originalRunSpeed;
        }
        
        speedBoostActive = false;
        Debug.Log("Speed boost terminou!");
    }

    IEnumerator JumpBoostCoroutine(float multiplier, float duration)
    {
        jumpBoostActive = true;
        
        // Aplicar boost
        if (movement != null)
        {
            movement.forcaPulo = originalJumpForce * multiplier;
        }
        
        Debug.Log($"Jump boost ativo! ({duration}s)");
        
        // Esperar
        yield return new WaitForSeconds(duration);
        
        // Restaurar valor original
        if (movement != null)
        {
            movement.forcaPulo = originalJumpForce;
        }
        
        jumpBoostActive = false;
        Debug.Log("Jump boost terminou!");
    }

    // Getters para UI (opcional)
    public bool IsSpeedBoostActive() => speedBoostActive;
    public bool IsJumpBoostActive() => jumpBoostActive;
    public bool IsExtraAttackActive() => extraAttackActive;
}