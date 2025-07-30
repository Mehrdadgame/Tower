using UnityEngine;

/// <summary>
/// Controls the behavior of a projectile, including movement and damage.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f;        // Movement speed of the projectile
    public float lifeTime = 5f;      // Time in seconds before the projectile is automatically destroyed

    private Transform target;        // Target the projectile is heading towards
    private float damage;            // Amount of damage the projectile will deal
    private Vector3 targetPosition;  // Cached position of the target

    /// <summary>
    /// Initializes the projectile with a target and damage value.
    /// </summary>
    public void Initialize(Transform target, float damage)
    {
        this.target = target;
        this.damage = damage;

        if (target != null)
        {
            // Save the current position of the target
            targetPosition = target.position;
        }

        // Destroy this projectile after 'lifeTime' seconds
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Update target position if the target is still alive
        if (target != null)
        {
            targetPosition = target.position;
        }

        // Move toward the target position
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Check if projectile is close enough to "hit" the target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            HitTarget();
        }
    }

    /// <summary>
    /// Called when the projectile reaches or collides with the target.
    /// Deals damage and destroys the projectile.
    /// </summary>
    private void HitTarget()
    {
        if (target != null)
        {
            // Try to get a component that implements the IDamageable interface
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage); // Apply damage if valid
        }

        // Destroy the projectile after impact
        Destroy(gameObject);
    }

    /// <summary>
    /// Trigger-based collision detection to hit the target early if collides.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Only respond to collision if it’s with the assigned target
        if (other.transform == target)
        {
            HitTarget();
        }
    }
}
