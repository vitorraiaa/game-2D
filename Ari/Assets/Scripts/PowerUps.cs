using UnityEngine;

public enum PowerType { HighJump, ExtraAttack }

[RequireComponent(typeof(Collider2D))]
public class PowerupPickup : MonoBehaviour
{
    public PowerType powerType;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var move = other.GetComponent<PlayerMovement2D>();
        var shoot = other.GetComponent<PlayerShoot2D>();

        switch (powerType)
        {
            case PowerType.HighJump:
                if (move) { move.hasHighJumpPower = true; }
                break;
            case PowerType.ExtraAttack:
                if (shoot) { shoot.hasExtraAttackPower = true; }
                break;
        }

        // feedback: poderia tocar sfx, spawnar partículas
        Destroy(gameObject);
    }
}
