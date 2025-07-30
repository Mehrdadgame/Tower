/* This C# code defines an interface named `IDamageable` with three methods:
1. `TakeDamage(float damage)`: This method takes a float parameter `damage` and does not return
anything. It is used to apply damage to an object that implements this interface.
2. `GetHealth()`: This method returns a float value representing the health of the object that
implements this interface.
3. `IsAlive()`: This method returns a boolean value indicating whether the object that implements
this interface is alive (true) or not (false). */
public interface IDamageable
{
    void TakeDamage(float damage);
    float GetHealth();
    bool IsAlive();
}