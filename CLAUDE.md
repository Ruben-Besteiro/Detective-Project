# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 (6000.4.0f1) detective game with two distinct gameplay modes:
- **Investigation scene** — exploration, NPC interaction, item pickup, and hypothesis deduction
- **Combat scene** — isometric action combat against a boss and minions

The project uses:
- **A\* Pathfinding Project** (Assets/AstarPathfindingProject) for enemy navigation
- **FMOD** for audio (via `FMODUnity.RuntimeManager`)
- **DOTween** for UI/portrait animations
- **Unity Input System** (new) via a generated `InputActions` class
- **TextMeshPro** for all UI text

## Working with This Project

Since this is a Unity project, there is no CLI build command — all building, running, and testing is done from the Unity Editor. Open the project in Unity 6 and use the Play button or Build menu.

Unity Test Runner (Window > General > Test Runner) runs the behaviour tree unit tests defined in `Assets/Plugins/BehaviourTree/BehaviourTreeTests.cs`.

## Code Architecture

### Dual Player Controllers

`PlayerController` (abstract, `Assets/Scripts/Player/PlayerController.cs`) is the shared base for isometric movement using `CharacterController.SimpleMove`. Two concrete subclasses cover each scene:

- `PlayerInvestigationController` — adds interaction with `Interactable` objects
- `PlayerCombatController` — adds an FSM for combat states (Idle/Move/Shoot/Melee/Dash/Dodge/Hurt), lock-on orbiting, and a targeting reticle

Both subscribe to `DialogueManager.OnDialogueStarted/Finished` and `PauseController.OnPauseStarted/Ended` to disable input during dialogue and pause.

### Persistent Singleton Managers

All singletons use the standard Unity `DontDestroyOnLoad` pattern. The key ones:

| Class | Responsibility |
|---|---|
| `GameDataManager` | Inventory, current hypothesis, player stats; orchestrates save/load |
| `FlagManager` | Boolean flag store (string → bool); implements `ISaveable` |
| `SoundManager` | Thin wrapper around FMOD `RuntimeManager.PlayOneShot` |
| `DialogueManager` | Coroutine-driven dialogue playback with typewriter effect |

### Save System

`ISaveable` is an interface with `SaveData(SaveData)` and `LoadData(SaveData)`. `GameDataManager.TrySave()` / `IE_LoadEveryISaveable()` iterate all active `MonoBehaviour` components in the scene and call these methods. `SaveData` is a flat JSON-serializable class written to `Application.persistentDataPath/save.json`.

To make a new component saveable: implement `ISaveable` and read/write fields on the `SaveData` struct.

### Dialogue System

Dialogues are `DialogueData` ScriptableObjects (create via Assets menu: *Dialogue System / Dialogue*). Each contains a list of `DialogueNode` with a discriminated-union style via `DialogueNodeType`:

- `Dialogue` — shows speaker name, left/right portraits, and typewriter text
- `Condition` — reads a flag from `FlagManager`; branches to a different `DialogueData` or continues current
- `Choice` — shows player options via `ChoiceManager`; each option can redirect to another `DialogueData`
- `FlagAction` — sets a flag in `FlagManager`

Entry point: `DialogueManager.StartDialogue(DialogueData)` (static).

### Hypothesis / Deduction System

`HypothesisData` ScriptableObjects hold a list of `HypothesisVersion`, each with a set of `requiredFlags`. `GetCurrentVersion()` returns the last version whose required flags are all set — meaning hypothesis text evolves as the player finds clues.

`HypothesisSet` ScriptableObject groups the three hypotheses (H1/H2/H3). `GameDataManager.currentHypothesis` tracks which one the player has chosen (enum `Hypotheses`).

### Interactable System

`Interactable` (abstract) uses trigger colliders and static events (`OnEnterRange` / `OnExitRange`) to notify `PlayerInvestigationController`. The controller tracks the closest interactable and shows its prompt. Concrete subclasses:

- `NpcInteractable` — triggers a `DialogueData`
- `PickupInteractable` — adds an item to `GameDataManager.inventory`
- `CheckInteractable` — inspect-only
- `LinkableInteractable` — used in the hypothesis deduction board to link clues

### Enemy / Boss System

`BossController` drives a `BossBehaviourTree` which extends the custom `BehaviourTree<T>` plugin (Assets/Plugins/BehaviourTree). The tree root is: `Sequence(PickAttackNode, Selector(AttackMoveNode | AttackProjectileNode | AttackCircleNode))`.

`MinionController` uses A\* Pathfinding (`AIPath` component) for pathfinding.

### Camera

`MainCamera` exposes static `isoForward` and `isoRight` vectors used by both player controllers to convert raw 2D input into isometric 3D world directions.
