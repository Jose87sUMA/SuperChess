# ♟️ SuperChess

![Unity](https://img.shields.io/badge/Unity-2022-black)
![C#](https://img.shields.io/badge/C%23-.NET-blue)
![AI](https://img.shields.io/badge/AI-Minimax%20%2B%20Alpha--Beta-green)
![Status](https://img.shields.io/badge/Status-Completed-success)

SuperChess is a hybrid turn-based strategy game that combines classical chess with a card-based system that temporarily modifies game rules. The project was developed in Unity 2022 using C# as part of a Bachelor’s Thesis in Software Engineering.

The objective of SuperChess is to preserve the strategic depth of chess while introducing controlled unpredictability through cards, modern game design principles, and artificial intelligence.

---

## Table of Contents

- Project Overview
- Core Features
- Game Modes
- Technology Stack
- Setup and Execution
- Testing and Validation
- Future Work
- Academic Context
- License

---

## Project Overview

SuperChess is built on top of a fully compliant chess engine and extends it with a deck-building card system. Cards allow players to temporarily alter game rules, enhance pieces, or trigger special effects, encouraging creativity and adaptability rather than memorization of openings.

The game supports local play, AI opponents, and online multiplayer.

---

## Core Features

### Chess Engine
- Full implementation of official chess rules
- Legal move validation
- Check, checkmate, and stalemate detection
- Castling
- En passant capture
- Pawn promotion
- Fully playable without cards as a classic chess game

### Card System
- Pre-match deck construction
- Turn-based card usage
- Temporary rule and board-state modifications
- Discard and reshuffle mechanics inspired by deck-building games

### Artificial Intelligence
- Minimax algorithm with alpha-beta pruning
- Multiple difficulty levels
- Position-aware and card-aware evaluation
- AI capable of deciding when and how to play cards

---

## Game Modes

- Local multiplayer (two players on the same device)
- Player versus AI
- Online multiplayer using a client-server architecture

---

## Technology Stack

- Game Engine: Unity 2022
- Language: C#
- AI: Minimax with alpha-beta pruning
- Networking: Custom client-server implementation
- Development Methodology: Scrum
- Version Control: Unity Version Control
- IDE: JetBrains Rider

---

## Setup and Execution

### Requirements
- Unity Hub
- Unity 2022.x
- Windows, Linux, or macOS

### Steps
1. Clone the repository
2. Open the project in Unity Hub
3. Load the main scene from the Scenes folder
4. Press Play in the Unity Editor

---

## Testing and Validation

- Manual validation of all chess rules
- Exhaustive testing of special moves
- AI-versus-AI simulations
- Card effect edge-case testing
- Multiplayer synchronization testing
- User playtesting sessions for usability feedback

---

## Future Work

- Additional card sets and synergies
- Improved AI heuristics
- Ranked online matchmaking
- Campaign or story mode
- Visual polish and animations
- Performance optimizations for deeper AI searches

---

## Academic Context

Title: Development of a Chess Variation with Cards and Artificial Intelligence Elements  
Author: José Alejandro Sarmiento  
Degree: Bachelor’s Degree in Software Engineering  
University: University of Málaga  
Department of Languages and Computer Science  
Year: 2025

---

## License

This project was developed for academic purposes.  
Please contact the author for reuse or redistribution.
