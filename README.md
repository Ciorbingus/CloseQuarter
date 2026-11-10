# CloseQuarter

A low-latency, real-time 3D online multiplayer fighting game built from scratch in **C#** using **Silk.NET (OpenGL)** and a custom networking architecture.

---

## Key Features

- **Custom 3D Engine**: Built on top of Silk.NET (OpenGL), featuring skeletal animation hierarchies, dynamic arena tracking cameras, precision hitboxes, and custom GLSL shaders.
- **Hybrid Network Architecture**:
  - **TCP Gateway**: Manages user authentication, session persistence, matchmaking queues, and room state synchronization.
  - **UDP Relay (60 Hz)**: Delivers ultra-low latency real-time packet forwarding during active matches.
- **Host-Authoritative Simulation**: Eliminates state divergence through fixed-timestep physics updates (`FixedDeltaTime = 1/60s`), local input quantization, and snapshot synchronization.
- **Data Persistence**: Backed by **PostgreSQL** & **EF Core**, with secure **BCrypt** password hashing and session token caching.
- **UI & HUD**: In-game overlays, combat health bars, timer systems, and match lobbies powered by ImGui.NET.

---

## Tech Stack

- **Language**: C# (.NET 8+)
- **Graphics Framework**: Silk.NET (OpenGL), ImGui.NET
- **Networking**: Custom TCP Gateway & UDP Binary Relay Protocol
- **Database & Auth**: PostgreSQL, Entity Framework Core, BCrypt.Net
- **Math Engine**: System.Numerics

---

## Network Architecture

```text
       ┌────────────────────────┐
       │   PostgreSQL Database   │
       └───────────▲────────────┘
                   │ EF Core / BCrypt
       ┌───────────▼────────────┐
       │  CloseQuarter.Server   │
       │ ├── TCP Port 5000 (Auth/Lobby/Matchmaking)
       │ └── UDP Port 5001 (60 Hz Relay)
       └─────▲────────────▲─────┘
             │            │
 ┌───────────┴───┐    ┌───┴───────────┐
 │ Client 1 (P1) │    │ Client 2 (P2) │
 │ (Host Sim)    │    │ (Input Stream)│
 └───────────────┘    └───────────────┘
```

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL database instance running locally or remotely

### Installation & Run

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/CloseQuarter.git
   cd CloseQuarter
   ```

2. **Database Configuration**:
   Set up your PostgreSQL connection string in `CloseQuarter.Server/Data/AppDbContext.cs`:
   ```csharp
   Host=localhost;Database=closequarter_db;Username=cq_admin;Password=cq1234
   ```

3. **Start the Server**:
   ```bash
   dotnet run --project CloseQuarter.Server
   ```

4. **Launch Clients**:
   Open two terminal instances to test multiplayer locally:
   ```bash
   # Terminal 1 (Player 1)
   dotnet run --project CloseQuarter.Client

   # Terminal 2 (Player 2)
   dotnet run --project CloseQuarter.Client
   ```

---

## Controls

- **Movement**: `W` / `A` / `S` / `D` or `Arrow Keys` *(Double-tap to Dash / Sidestep)*
- **Crouch**: Hold `S` / `Down`
- **Jump**: `W` / `Up` (+ Direction)
- **Attacks**: `Z` / `F` (Left Punch), `X` / `G` (Right Punch)
- **Menu / Ready**: `Esc`
