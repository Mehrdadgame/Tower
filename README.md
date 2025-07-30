🛡️ Tower Defense Game (Unity)
A modular and scalable Tower Defense game built in Unity 6.1.0f1. Designed with clean architecture principles, using SOLID and multiple design patterns to make it maintainable and extensible.

▶️ How to Play
Left-Click on a tower button to select it.

Left-Click on a valid ground tile to place the tower.

Right-Click cancels current selection.

Towers will automatically target enemies within their range.

You earn money by killing enemies and can use it to place more towers.

⚙️ Requirements
Unity Version: 6.1.0f1

URP/HDRP compatible (optional)

Input System: Unity Input System or Legacy Input

📁 Project Structure
Here’s how the Scripts folder is organized:

graphql
Copy
Edit
Assets/
├── Scripts/
│   ├── Enemy/                # Enemy behavior and logic
│   ├── Interface/            # Interfaces (IDamageable, IUpgradeable, IShootable)
│   ├── Manager/              # GameManager, WaveManager, UIManager, etc.
│   ├── ObjectPool/           # Object Pool system for reusing GameObjects
│   ├── Projectile/           # Projectile behavior
│   ├── ScriptableObject Data/ # TowerData, EnemyData definitions
│   └── Tower/                # Tower logic, placement, range visualizer
🔁 Design Principles & Patterns Used
✅ SOLID Principles
Single Responsibility:
Each class has one responsibility
(e.g., Tower handles shooting, Enemy handles movement and health)

Open/Closed:
TowerData & EnemyData allow adding new towers/enemies without changing core code

Liskov Substitution:
Interfaces like IDamageable, IUpgradeable allow safe polymorphism

Interface Segregation:
IShootable, IDamageable are focused, specific interfaces

Dependency Inversion:
Code relies on abstractions like interfaces, not implementations

🧠 Design Patterns
Singleton:
GameManager, SoundManager manage global game state

Observer:
Event system updates UI on enemy death, tower placement, etc.

Strategy:
Tower behavior can change (e.g., different fire logic or upgrades)

Factory:
Towers/Enemies are created using ScriptableObject-based factories

Object Pool:
Reuses bullets, enemies, and other frequently spawned objects

🏰 How to Create a New Tower
Towers are defined using ScriptableObjects for modularity.

Right-click in Project → Create → TowerData

Set values:

Range

Fire Rate

Cost

Tower Prefab (must include Tower.cs and projectile spawn point)

Projectile Prefab

Assign this data to a UI Button for placement via TowerPlacement system.

💡 Each tower prefab must:

Contain a Tower.cs script

Have a spawn point for projectiles

Optional: use TowerRangeVisualizer to show range circle when selected

👾 How to Create a New Enemy
Right-click in Project → Create → EnemyData

Set values:

Max Health

Speed

Reward on death

Enemy prefab (must include Enemy.cs and collider)

💡 Each enemy prefab must:

Include Enemy.cs

Follow the path defined in the level

Implement IDamageable interface

🌊 How to Create Waves of Enemies
Waves are managed via WaveManager using enemy lists or ScriptableObjects.

🛠 Basic Setup:

csharp
Copy
Edit
[System.Serializable]
public class WaveData {
    public List<EnemyData> enemiesToSpawn;
    public float spawnInterval;
    public float delayBeforeNextWave;
}
The WaveManager loops through the enemies and spawns them at set intervals.

✅ Use Object Pool to optimize performance
✅ Customize wave delay and enemy combinations for difficulty scaling
✅ Use UI to display countdown to next wave

📏 Range Visualizer (Optional)
TowerRangeVisualizer.cs creates a circle around the tower showing its shooting range:

On hover or placement, range circle appears

Material changes based on tower upgrade or selection

🧪 Testing & Debugging
Use Unity Gizmos to visualize spawn points and tower ranges.

Enable debug logs for projectile hits, enemy deaths, and gold earned.

Use Unity Timeline/Animator for enemy death animations if needed.


