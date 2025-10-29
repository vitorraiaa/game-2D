using UnityEngine;

public interface IDamageable
{
    // Dano genérico (ex.: espinho, explosão)
    void ApplyDamage(int amount, Vector2 hitPoint, Vector2 hitNormal);
}
