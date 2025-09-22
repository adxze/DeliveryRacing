## Developer & Contributions

Adiguna S Ligawan (Game Developer & Systems Designer)
  <br>

## About

Delivery Rush is a high-octane arcade delivery game that combines the thrill of high-speed driving with strategic package delivery missions. Built with innovative sprite stacking technology to achieve a unique pseudo-3D look using 2D pixel art, the game features realistic car physics with velocity-based movement, acceleration curves, and responsive turning mechanics. Navigate through busy city streets, evade traffic, escape police chases, and deliver packages on time to earn high scores in this fast-paced, endlessly replayable arcade experience.
<br>

## Key Features

**Sprite Stacking Technology**: Innovative visual technique that creates stunning pseudo-3D graphics from 2D pixel art, giving the game a unique and stylish appearance.

**Realistic Car Physics**: Velocity-based movement system with proper acceleration, braking, drifting, and turning mechanics that make every delivery feel authentic and exciting.

**High-Speed Delivery Gameplay**: Intense time-based missions where you must pickup and deliver packages while avoiding traffic, police, and other obstacles.

**Arcade-Style Action**: Fast-paced, pick-up-and-play gameplay with tight controls, combo systems, and high score chasing for maximum replayability.

<br>

<table>
  <tr>
    <td align="left" width="50%">
      <img width="100%" alt="gif1" src="https://github.com/yourusername/delivery-rush/blob/main/gameplay1.gif">
    </td>
    <td align="right" width="50%">
      <img width="100%" alt="gif2" src="https://github.com/yourusername/delivery-rush/blob/main/gameplay2.gif">
    </td>
  </tr>
</table>

## Scene Flow 

```mermaid
flowchart LR
  mm[Main Menu]
  gp[Gameplay]
  pause[Pause Menu]
  complete[Delivery Complete]
  fail[Mission Failed]
  results[Results Screen]

  mm -- "Start Game" --> gp
  gp -- "Pause" --> pause
  pause -- "Resume" --> gp
  pause -- "Main Menu" --> mm
  gp -- "Delivery Success" --> complete
  gp -- "Time Out/Crash" --> fail
  complete --> results
  fail --> results
  results -- "Next Level" --> gp
  results -- "Main Menu" --> mm

```

## Layer / Module Design 

```mermaid
---
config:
  theme: neutral
  look: neo
---
graph TD
    subgraph "Game Initialization"
        Start([Game Start])
        Boot[Boot Layer]
        Settings[Load Settings]
    end
    subgraph "Main Menu System"
        MM[Main Menu]
        CarSelect[Car Selection]
        LevelSelect[Level Select]
        Options[Options Menu]
    end
    subgraph "Gameplay Core"
        GP[Gameplay Scene]
        CarPhysics[Car Physics]
        Movement[Movement System]
        Camera[Camera Controller]
    end
    subgraph "Game Mechanics"
        Delivery[Delivery System]
        Traffic[Traffic AI]
        Score[Score Manager]
        Timer[Timer System]
    end
    subgraph "Visual Systems"
        SpriteStack[Sprite Stacking]
        VFX[Visual Effects]
        UI[UI System]
        Indicators[HUD Indicators]
    end
    subgraph "End States"
        Complete[Delivery Complete]
        Failed[Mission Failed]
        Results[Results Display]
        Leaderboard[High Scores]
    end
    Start --> Boot
    Boot --> Settings
    Settings --> MM
    MM -->|Play| CarSelect
    MM -->|Options| Options
    CarSelect --> LevelSelect
    LevelSelect --> GP
    Options --> MM
    GP --> CarPhysics
    CarPhysics --> Movement
    Movement --> Camera
    GP --> Delivery
    GP --> Traffic
    GP --> Score
    GP --> Timer
    Movement --> SpriteStack
    GP --> VFX
    GP --> UI
    UI --> Indicators
    Timer -->|Time Up| Failed
    Delivery -->|Success| Complete
    Movement -->|Crash| Failed
    Complete --> Results
    Failed --> Results
    Results --> Leaderboard
    Results -->|Retry| GP
    Results -->|Menu| MM
    classDef initStyle fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef menuStyle fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef gameplayStyle fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef mechanicsStyle fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef visualStyle fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    classDef endStyle fill:#ffebee,stroke:#b71c1c,stroke-width:2px
    class Start,Boot,Settings initStyle
    class MM,CarSelect,LevelSelect,Options menuStyle
    class GP,CarPhysics,Movement,Camera gameplayStyle
    class Delivery,Traffic,Score,Timer mechanicsStyle
    class SpriteStack,VFX,UI,Indicators visualStyle
    class Complete,Failed,Results,Leaderboard endStyle

```

## Modules and Features

A high-speed arcade delivery game featuring innovative sprite stacking for pseudo-3D visuals, realistic car physics with velocity and acceleration, and intense traffic evasion gameplay that creates thrilling delivery missions through busy city streets.

| 📂 Name | 🎬 Scene | 📋 Responsibility |
|---------|----------|-------------------|
| **CameraFollow** | **Gameplay** | - Follow player car smoothly<br/>- Handle camera shake effects<br/>- Manage zoom levels for speed effects |
| **CarController** | **Gameplay** | - Handle car physics (velocity, acceleration, braking)<br/>- Process steering and turning mechanics<br/>- Manage drift and traction systems |
| **Interaction** | **Gameplay** | - Handle delivery pickups and drop-offs<br/>- Process collision with traffic<br/>- Manage checkpoint interactions |
| **MovementInput** | **Gameplay** | - Capture player input (keyboard/gamepad)<br/>- Process acceleration and steering inputs<br/>- Handle handbrake and boost controls |
| **OffScreenTargetIndicator** | **UI/Gameplay** | - Show direction to delivery targets<br/>- Display distance to objectives<br/>- Indicate off-screen traffic threats |
| **PlayerMovementClassic** | **Gameplay** | - Classic arcade car movement physics<br/>- Handle speed limits and boost mechanics<br/>- Process collision responses |
| **ScoreManager** | **Gameplay** | - Track delivery times and bonuses<br/>- Calculate score based on performance<br/>- Manage combo multipliers |
| **UIScript** | **UI System** | - Display speedometer and timer<br/>- Show delivery status and objectives<br/>- Update score and combo displays |
| **carNew** | **Gameplay** | - Store car properties and stats<br/>- Handle car customization data<br/>- Manage vehicle damage states |
| **emptyTruckSprites** | **Visual** | - Manage sprite stacking layers<br/>- Handle truck visual components<br/>- Process damage visuals |
| **mouse** | **Input** | - Handle mouse input for menus<br/>- Process mouse camera controls<br/>- Manage UI interactions |

<br>

## Game Flow Chart

```mermaid
---
config:
  theme: redux
  look: neo
---
flowchart TD
  start([Game Start])
  start --> select[Select Car]
  select --> mission[Start Mission]
  
  mission --> pickup{Pickup Location}
  pickup --> drive1[Drive to Pickup]
  drive1 --> traffic1{Traffic Check}
  
  traffic1 -->|Clear| arrive1[Arrive at Pickup]
  traffic1 -->|Collision| damage1[Take Damage]
  traffic1 -->|Evade| boost1[Use Boost]
  
  damage1 --> health{Health Check}
  health -->|Destroyed| fail[Mission Failed]
  health -->|Damaged| continue1[Continue]
  
  boost1 --> arrive1
  continue1 --> arrive1
  
  arrive1 --> load[Load Package]
  load --> timer[Start Timer]
  
  timer --> delivery{Delivery Location}
  delivery --> drive2[Drive to Delivery]
  
  drive2 --> traffic2{Traffic Check}
  traffic2 -->|Clear| arrive2[Arrive at Delivery]
  traffic2 -->|Collision| damage2[Take Damage]
  traffic2 -->|Police Chase| chase[Evade Police]
  
  damage2 --> health2{Health Check}
  health2 -->|Destroyed| fail
  health2 -->|Damaged| continue2[Continue]
  
  chase --> wanted{Escape?}
  wanted -->|Yes| continue2
  wanted -->|No| caught[Busted]
  caught --> fail
  
  continue2 --> arrive2
  
  arrive2 --> drop[Drop Package]
  drop --> time{Time Bonus?}
  
  time -->|Under Time| bonus[Award Bonus]
  time -->|Over Time| standard[Standard Pay]
  
  bonus --> score[Calculate Score]
  standard --> score
  
  score --> complete[Mission Complete]
  complete --> next{Continue?}
  
  next -->|Yes| mission
  next -->|No| results[Show Results]
  
  fail --> results
  results --> menu[Main Menu]

```

<br>

## Event Signal Diagram

```mermaid
classDiagram
    %% --- Core Driving ---
    class CarController {
        +OnAccelerate(force: float)
        +OnBrake(force: float)
        +OnSteer(angle: float)
        +OnDrift()
        +OnBoost()
        +OnCollision(object: GameObject)
    }

    class MovementInput {
        +OnInputReceived(input: Vector2)
        +OnHandbrakePressed()
        +OnBoostPressed()
        +OnHornPressed()
    }

    class PlayerMovementClassic {
        +OnSpeedChanged(speed: float)
        +OnDirectionChanged(direction: Vector3)
        +OnDriftStart()
        +OnDriftEnd()
    }

    %% --- Game Systems ---
    class ScoreManager {
        +OnDeliveryComplete(time: float)
        +OnBonusEarned(type: string)
        +OnComboIncreased(multiplier: int)
        +OnScoreUpdated(total: int)
    }

    class Interaction {
        +OnPackagePickup()
        +OnPackageDelivered()
        +OnCheckpointReached()
        +OnTrafficHit()
    }

    class CameraFollow {
        +OnCameraShake(intensity: float)
        +OnZoomChange(level: float)
        +OnTargetChanged(target: Transform)
    }

    %% --- UI Systems ---
    class UIScript {
        +OnSpeedUpdate(speed: float)
        +OnTimerUpdate(time: float)
        +OnObjectiveUpdate(text: string)
        +OnDamageReceived(amount: float)
    }

    class OffScreenTargetIndicator {
        +OnTargetOffScreen(position: Vector3)
        +OnTargetOnScreen()
        +OnDistanceUpdate(distance: float)
    }

    %% --- Visual ---
    class emptyTruckSprites {
        +OnSpriteStackUpdate(angle: float)
        +OnDamageVisual(level: int)
        +OnSpeedEffect(speed: float)
    }

    %% --- Relations ---
    MovementInput --> CarController : sends input
    CarController --> PlayerMovementClassic : applies physics
    PlayerMovementClassic --> CameraFollow : updates position
    CarController --> Interaction : detects collision
    Interaction --> ScoreManager : awards points
    PlayerMovementClassic --> UIScript : updates HUD
    Interaction --> OffScreenTargetIndicator : sets target
    CarController --> emptyTruckSprites : updates visuals

```

<br>



## Play The Game

<a href="#">Play Now</a>
<br>

![Delivery Rush Demo](https://raw.githubusercontent.com/yourusername/delivery-rush/main/DeliveryRushSlide.png)
