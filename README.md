# Codenames Multiplayer Game

## Table of Contents

1.  [Overview](#overview)
2.  [Features](#features)
3.  [Architecture](#architecture)
4.  [Game Logic Flowchart](#flowchart)
5.  [Screenshots](#screenshots)
6.  [Installation](#installation)

## 🎲 Overview
This project brings the classic board game Codenames to the web, allowing for real-time, collaborative gameplay. It solves the problem of needing to be physically present to enjoy the game with friends.

## ⚡Features
- **Real-time Gameplay**: SignalR enables instant communication between players in a game room.
- **Live Game Sessions**: Player actions, team interactions, and game state updates are tracked and broadcast in real time.
- **Codenames Game Mechanics**: The entire game logic, including word selection, guessing, and scoring, is implemented in code.
- **Azure Hosting**: The application is deployed and hosted in Azure for scalability and reliability using a dedicated SignalR resource.

## 🏗 Architecture
The project follows an **N-layer architecture**, ensuring clean separation of concerns:
- **WebUI** – ASP.NET Core MVC front-end for game interaction.
- **Core** – Contains game logic and business rules.
- **Data Access** – Handles database interactions.
- **Models** – Defines shared data structures across layers.

## 🧩 Game Logic Flowchart
The following flowchart illustrates the core logic implemented in the game:

![codenames_flowchart2 drawio](https://github.com/user-attachments/assets/35958568-0864-4649-bbc5-3bbdaf4c4ed2)

## 📸 Screenshots

[WiP]

## 🚀 Installation
[WiP]
### Prerequisites
- .NET 8.0 SDK

### Setup
```sh
# Clone the repository
git clone https://github.com/94adi/CodeNames.git
cd codenames


# Start the application
dotnet run --project WebUI
