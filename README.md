# Bevosos — Server

> Multiplayer server for **Bevosos**, a digital implementation of the card game *Bears vs Babies*. Built with C# and WCF, featuring real-time game sessions, lobby management, social systems, and reconnection logic.

---

## What is Bevosos?

Bevosos is a  playable online card game based on *Bears vs Babies*, supporting **2 to 4 players** per match. The game features custom card art commissioned from an artist, complete account management, and a social layer built over WCF.

> **Frontend repo:** [BevososFrontend](https://github.com/AquilezE/BevososFrontend)

---

## Features

### Game
- 2–4 player real-time matches over WCF callbacks
- Limited Bears vs Babies ruleset implementation
- Lobby creation, matchmaking, and in-game chat
- Custom commissioned card artwork

### Disconnection & Reconnection Logic
- **Graceful disconnect:** player automatically forfeits on voluntary leave
- **Crash recovery:** 50-second reconnection window on unexpected disconnect
- If reconnection window expires, player forfeits and game continues

### Accounts & Auth
- Full registration and login with email verification
- Guest session support for quick play without an account
- Persistent user profiles

### Social System
- Friend requests and friend list
- **Real-time online/offline presence** — entirely over WCF polling (Requirements limitation)
- Block system with edge case handling:
  - Blocked users cannot search for you
  - If a user sends a friend request and gets blocked mid-request, the request is invalidated
- ¨Player reporting and chat moderation

### Persistence
- User accounts and credentials
- Match history
- Friends, blocked users
- Player reports

---

## Architecture

```
BevososServer/
├── Contracts/          # WCF service contracts and data contracts
├── BevososService/     # Service implementations (game logic, social, auth)
├── DataAccess/         # Database layer (Entity Framework)
├── Host/               # WCF host entry point
├── TEST/               # Unit tests
└── EXCEPTIONTESTS/     # Exception handling and edge case tests
```

The server is structured around a clear separation of concerns:
- **Contracts** define all service interfaces and DTOs independently
- **BevososService** implements game state, lobby management, and social features
- **DataAccess** handles all persistence through a dedicated layer
- **Host** bootstraps the WCF service host

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# (.NET Framework) |
| Communication | WCF (Windows Communication Foundation) |
| Real-time callbacks | WCF Duplex Channels |
| Database ORM | Entity Framework |
| Testing | xUnit (unit + exception coverage) |

---

## Testing

The project includes two test suites:
- `TEST/` — xUnit unit tests for core game logic and service methods
- `EXCEPTIONTESTS/` — dedicated xUnit suite for edge cases, error handling, and fault contracts

---

## Team

Built as a university project at **Universidad Veracruzana** in one semester.

- [@AquilezE](https://github.com/AquilezE)
- [@Cuaju](https://github.com/Cuaju)
- Card art commissioned from @erre.rein on IG

---
