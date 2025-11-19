using UnityEngine;

public enum PowerUpType { HighJump, ExtraAttack }

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpPickup : MonoBehaviour
{
    public PowerUpType type = PowerUpType.HighJump;
    public bool bobbing = true;
    public float bobbingAmplitude = 0.05f;
    public float bobbingSpeed = 2f;

    Vector3 basePos;
    bool picked; // evita múltiplos triggers

    void Awake()
    {
        basePos = transform.position;
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        Debug.Log($"[PowerUpPickup] Ativo '{name}' ({type}). Layer={LayerMask.LayerToName(gameObject.layer)} IsTrigger={col.isTrigger}");
    }

    void Update()
    {
        if (bobbing)
        {
            float y = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;
            transform.position = basePos + new Vector3(0f, y, 0f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return; // já coletado

        // Aceita colisão do Player ou de um filho do Player
        var movement = other.GetComponentInParent<PlayerMovement2D>();
        var shoot    = other.GetComponentInParent<PlayerShoot2D>();

        if (!movement && !shoot)
        {
            // Não é o player: ignora
            return;
        }

        picked = true;

        // Aplica o efeito
        switch (type)
        {
            case PowerUpType.HighJump:
                if (movement)
                {
                    movement.hasHighJumpPower = true;
                    Debug.Log("[PowerUpPickup] High Jump habilitado no Player!");
                }
                break;

            case PowerUpType.ExtraAttack:
                if (shoot)
                {
                    shoot.hasExtraAttackPower = true;
                    Debug.Log("[PowerUpPickup] Extra Attack habilitado no Player!");
                }
                break;
        }

        // SFX de coleta
        if (SfxManager.Instance) SfxManager.Instance.PlayPickup();

        // Destroi o item
        Destroy(gameObject);
    }
}
