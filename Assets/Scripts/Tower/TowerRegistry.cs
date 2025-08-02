using System.Collections.Generic;
using UnityEngine;

/* The `public class TowerRegistry : Singleton<TowerRegistry>` line is defining a class named
`TowerRegistry` that inherits from a generic Singleton class. This pattern is commonly used to
ensure that only one instance of the `TowerRegistry` class exists throughout the program, providing
a global point of access to its methods and properties. The Singleton pattern restricts the
instantiation of a class to a single object and provides a way to access that instance globally. */
public class TowerRegistry : Singleton<TowerRegistry>
{
    private static readonly List<Tower> activeTowers = new List<Tower>();

    protected override void Awake()
    {
        base.Awake();
        activeTowers.Clear();

    }

    /// <summary>
    /// The RegisterTower function adds a Tower object to a list of active towers if it is not already in
    /// the list.
    /// </summary>
    /// <param name="Tower">The `RegisterTower` method takes a parameter of type `Tower`, which is likely
    /// a class representing a tower object in a game or simulation. This method is used to register a
    /// tower by adding it to a collection of active towers if it is not already present in the
    /// collection.</param>
    public static void RegisterTower(Tower tower)
    {
        if (!activeTowers.Contains(tower))
        {
            activeTowers.Add(tower);
            Debug.Log("[TowerRegistry] Awake called, activeTowers cleared.");
        }
    }

    /// <summary>
    /// The function UnregisterTower removes a specified tower from a collection of active towers.
    /// </summary>
    /// <param name="Tower">The `UnregisterTower` method is a static method that takes a `Tower` object as
    /// a parameter. The method removes the specified `Tower` object from the `activeTowers`
    /// collection.</param>
    public static void UnregisterTower(Tower tower)
    {
        activeTowers.Remove(tower);
    }

    /// <summary>
    /// The function `GetNearestTower` finds and returns the nearest tower to a given position within a
    /// specified maximum range.
    /// </summary>
    /// <param name="Vector3">A Vector3 is a data structure in Unity that represents a point or direction
    /// in 3D space. It consists of three float values (x, y, z) that define the position or direction in
    /// the 3D world. In the context of the `GetNearestTower` method you</param>
    /// <param name="maxRange">The `maxRange` parameter in the `GetNearestTower` method represents the
    /// maximum distance within which a tower can be considered as the nearest tower to a given position.
    /// Towers beyond this range will not be considered.</param>
    /// <returns>
    /// The `GetNearestTower` method returns the tower that is nearest to the given `position` within the
    /// specified `maxRange`.
    /// </returns>
    public static Tower GetNearestTower(Vector3 position, float maxRange)
    {
        Tower nearestTower = null;
        float minSqrDistance = maxRange * maxRange;

        foreach (var tower in activeTowers)
        {
            if (tower == null) continue;

            float sqrDistance = Vector3.SqrMagnitude(tower.transform.position - position);
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestTower = tower;
            }
        }

        return nearestTower;
    }

    public static List<Tower> GetActiveTowers() => new List<Tower>(activeTowers);
}