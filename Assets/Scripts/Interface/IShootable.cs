using UnityEngine;
/* This code defines an interface named `IShootable` in C#. The interface has two methods: `Shoot`
which takes a `Transform` parameter named `target` and does not return anything, and `CanShoot`
which returns a boolean value. Any class that implements this interface must provide implementations
for these two methods. */

public interface IShootable
{
    void Shoot(Transform target);
    bool CanShoot();
}