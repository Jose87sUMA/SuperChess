# SuperChess – Technical Architecture

This document describes the internal architecture, design decisions, and code organization of SuperChess. It is intended for developers who want to understand, maintain, or extend the project.

---

## Architectural Overview

SuperChess follows a modular, event-driven architecture designed to clearly separate responsibilities between game logic, presentation, AI, and networking.

The system is structured around a central game controller that orchestrates turn flow while delegating specialized responsibilities to independent subsystems.

---

## Design Patterns Used

### Model–View–Controller (MVC)
- Model: Chess engine, card system, game state
- View: UI, board rendering, animations
- Controller: Turn management, player input, AI decisions

This separation allows the chess engine to be tested and reused independently of the UI.

### Observer Pattern
- Board state changes notify:
  - UI
  - AI
  - Network synchronization layer
- Reduces tight coupling between systems

### Data-Driven Design
- Cards are defined as data objects
- Effects are modular and extendable
- Avoids hardcoded gameplay logic

---

## High-Level System Architecture

+------------------+  
|     UI Layer     |  
| (HUD, Menus)     |  
+--------+---------+  
         |  
         v  
+--------+---------+  
| Game Controller  |  
| Turn Flow & Rules|  
+--------+---------+  
         |  
   +-----+-----+  
   |           |  
   v           v  
Chess Engine   Card System  
(Board Logic)  (Effects)  
   |           |  
   +-----+-----+  
         |  
         v  
Artificial Intelligence  

---

## Core Systems

### Chess Engine
Responsibilities:
- Board representation
- Piece movement rules
- Move validation
- Special moves
- Check and checkmate detection
- End-game conditions

Key characteristics:
- Deterministic
- Independent from UI and networking
- Can operate without the card system enabled

---

### Card System
Responsibilities:
- Deck construction
- Card drawing and discarding
- Effect execution
- Temporary rule modification

Design notes:
- Cards do not directly manipulate the board
- Effects are routed through controlled interfaces
- Supports stacking and expiration of effects

---

### Turn Controller
Responsibilities:
- Turn sequencing
- Enforcing action order:
  - Draw card
  - Optional card play
  - Mandatory piece move
- Triggering end-of-turn evaluations

This component acts as the main authority during gameplay.

---

## Artificial Intelligence

### Algorithms
- Minimax
- Alpha-beta pruning
- Quiescence search

### AI Decision Pipeline
1. Generate legal chess moves
2. Generate valid card actions
3. Simulate combined actions
4. Evaluate resulting game state
5. Select optimal action based on difficulty

### Evaluation Function Factors
- Material balance
- King safety
- Piece mobility
- Board control
- Active card effects
- Potential value of unused cards

Difficulty is controlled by search depth, heuristic complexity, and card aggressiveness.

---

## Networking Architecture

SuperChess uses a custom client-server model.

### Characteristics
- Authoritative server
- Deterministic game state replication
- Message-based communication

### Synchronized Events
- Piece movements
- Card usage
- Turn changes
- Game state updates

Unity’s deprecated UNet system was intentionally avoided.

---

## Project Structure

Assets/
├── Scripts/
│   ├── Chess/
│   │   ├── Board.cs
│   │   ├── Piece.cs
│   │   ├── MoveValidator.cs
│   │   └── SpecialMoves/
│   ├── Cards/
│   │   ├── CardData.cs
│   │   ├── CardEffects/
│   │   ├── DeckManager.cs
│   │   └── HandManager.cs
│   ├── AI/
│   │   ├── Minimax.cs
│   │   ├── Evaluation.cs
│   │   └── DifficultyConfig.cs
│   ├── Networking/
│   │   ├── Client.cs
│   │   ├── Server.cs
│   │   └── NetworkMessages.cs
│   ├── UI/
│   │   ├── Menus/
│   │   ├── HUD/
│   │   └── DeckBuilderUI.cs
│   └── Core/
│       ├── GameManager.cs
│       └── TurnController.cs
├── Scenes/
├── Prefabs/
├── Audio/
└── Resources/

---

## Extensibility Guidelines

- New cards should be added as data definitions plus isolated effect classes
- AI heuristics can be extended without modifying the chess engine
- UI changes should not affect core gameplay logic
- Networking messages should remain deterministic and minimal

---

## Notes for Contributors

- Avoid coupling UI logic with game rules
- Keep the chess engine deterministic
- Prefer composition over inheritance for gameplay effects
- Test all rule changes in both offline and online modes
