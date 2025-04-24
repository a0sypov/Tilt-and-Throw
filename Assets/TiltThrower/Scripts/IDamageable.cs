using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
    
    int Health { get; }
    
    event System.Action<GameObject> OnDestroyed;
}