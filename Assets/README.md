# Apocalypse

A Unity 3D action game built with Unity 6000.3 and Universal Render Pipeline (URP).

## 🎮 Project Info

- **Engine:** Unity 6000.3
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Input System:** New Input System
- **Project Type:** 3D Action Game

## 📁 Project Structure

```
/Assets
├── /Scripts           # All C# scripts
├── /Scenes            # Game scenes
├── /Prefabs           # Prefabs
├── /Materials         # Materials
├── /Models            # 3D models
└── /Other             # Third-party assets
```

## 🔧 Key Features

- Loot system with rarity-based drops
- Enemy AI with navigation
- Player controller with New Input System
- URP rendering with post-processing
- Physics-based gameplay mechanics

## 🚀 Getting Started

### Prerequisites
- Unity Hub
- Unity 6000.3 or later
- Git

### Setup
1. Clone the repository
   ```bash
   git clone <your-repo-url>
   ```

2. Open in Unity Hub
   - Click "Open"
   - Navigate to the cloned project folder
   - Unity will import all assets

3. Open the main scene
   - Navigate to `Assets/Scenes/Apocalypse.unity`
   - Press Play to test

## 📦 Main Packages

- **URP** (Universal Render Pipeline) - 17.3.0
- **Cinemachine** - 3.1.2
- **Input System** - 1.16.0
- **AI Navigation** - 2.0.9
- **Post Processing** - 3.5.1
- **Visual Effect Graph** - 17.3.0
- **Timeline** - 1.8.9

## 🎯 Core Systems

### Loot System
- Rarity-based loot drops (Common, Uncommon, Rare, Epic, Legendary)
- Gear score system (100-500)
- Physics-based loot drops with ground detection
- Auto-pickup system

### Game Managers
- GameManager - Central game state management
- LootManager - Loot spawn and inventory
- [Add other managers as needed]

## 🔨 Development

### Coding Guidelines
- Place all scripts in `/Assets/Scripts`
- Use self-explanatory names
- Add comments for public methods
- Use constant fields instead of magic numbers
- Follow project structure conventions

### Git Workflow
1. Create a feature branch
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes and commit
   ```bash
   git add .
   git commit -m "Description of changes"
   ```

3. Push to GitHub
   ```bash
   git push origin feature/your-feature-name
   ```

4. Create a Pull Request on GitHub

## 📝 Documentation

Additional documentation can be found in the `/Pages` directory within Bezi.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your fork
5. Create a Pull Request

## 📄 License

[Add your license here]

## 👥 Authors

[Add your name/team here]

---

**Built with Unity 🎮**
