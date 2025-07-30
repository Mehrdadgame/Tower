Tower Defense Game – Unity 6.1

🛠️ Requirements

Unity Version: 2023.1 or later (tested on Unity 6.1)

URP or Standard Render Pipeline (compatible with both)

Input System: Old (default) or New Input System (click-based only)

📦 Project Structure

The project is organized using SOLID principles and Design Patterns for clarity and scalability.

Scripts/
├── Enemy/ → Handles enemy movement, health, and damage
├── Tower/ → Handles tower shooting, range, upgrades
├── Projectile/ → Controls bullet/projectile behavior
├── Manager/ → GameManager (singleton), SoundManager, etc.
├── ObjectPool/ → Reusable object pool for projectiles and enemies
├── Interfaces/ → IDamageable, IShootable, IUpgradeable, etc.
├── ScriptableObjects/
│ ├── Towers/ → TowerData assets (prefab, damage, cost, etc.)
│ └── Enemies/ → EnemyData assets (health, speed, reward)
├── UI/ (optional) → UI logic for tower buttons, panels, etc.
└── Events/ (optional) → Custom event system or observer pattern support

🎮 How to Play

Press Play in Unity Editor.

Click on any available Tower button from the UI.

Move your mouse over a green placement area (TowerPlacementArea).

Left-click to place a tower at the selected spot.

Right-click to cancel tower placement.

Towers automatically target and shoot at enemies within range.

📏 Tower Placement & Range

Only allowed on TowerPlacementArea components.

Range visualization handled by TowerRangeVisualizer using custom materials.

Range color changes depending on:

Normal: base color

Selected: highlight color

Upgraded: upgraded color

🧠 Design Patterns Used

✅ SOLID Principles

Single Responsibility: Tower, Enemy, and Projectile handle only their own logic.

Open/Closed: TowerData and EnemyData support new tower/enemy types without modifying code.

Liskov Substitution: Interfaces (IDamageable, IUpgradeable) ensure interchangeable components.

Interface Segregation: Interfaces are minimal and focused.

Dependency Inversion: Game logic depends on interfaces, not concrete classes.

✅ Design Patterns

Singleton: GameManager, SoundManager

Strategy: Tower and Enemy behavior abstraction via data

Factory: ScriptableObjects used to instantiate towers and enemies

Observer (optional): Event system for UI updates

Object Pool: Efficient spawning of projectiles and enemies

🧪 Extending the Project

To add a new Tower:

Create a new TowerData (ScriptableObject)

Assign prefab, range, fire rate, and visuals

To add a new Enemy:

Create EnemyData with different speed, health, and reward

You can also use RangeIndicatorController prefab for custom range visuals.
