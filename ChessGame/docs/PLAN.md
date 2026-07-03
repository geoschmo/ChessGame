# Chess Game ExecPlan

## Frozen Sections

These sections define the intended work. After implementation resumes, do not rewrite them casually. If a frozen section proves wrong or contradictory, stop and explain why before changing it.

### Purpose And User-Facing Goal

Build a browser-playable two-player chess game that follows the existing Connect Four and Battleship deployment pattern: standalone ASP.NET Core app, hosted under a website subfolder, with SignalR multiplayer, invite links, reconnect support, and persisted game state.

The first release is two-player only. A simple AI can be added later by reusing the same server-side chess rules engine.

### Scope

- Use the Battleship project as the primary template because it already has SignalR rooms, LiteDB persistence, player tokens, reconnect flow, and `PathBase` support.
- Use the Connect Four project as a reference for a simpler single-state broadcast model.
- Keep the app deployable as a subfolder page, for example `/chess`, through the `PathBase` configuration setting.
- Build the app with ASP.NET Core 8 Razor Pages, SignalR, LiteDB, browser JavaScript, and responsive HTML/CSS.
- Implement a server-authoritative chess game for two players, including room creation, join by invite link, reconnect, persisted game snapshots, legal move validation, and state broadcast.
- Include learning-oriented helpers and useful illegal-move explanations.
- Keep the chess rules engine independent from SignalR and UI code so future AI work can reuse it.

### Explicit Non-Goals

- Do not implement a computer opponent in the first release.
- Do not implement draw offer and accept flow in the first release unless the core two-player release is complete and the scope is intentionally updated.
- Do not implement later rule enhancements in the first release unless explicitly pulled into scope: fifty-move draw, threefold repetition, and insufficient material.
- Do not replace the ASP.NET Core Razor Pages, SignalR, or LiteDB architecture with a different stack.
- Do not broaden this work into unrelated site, portfolio, deployment, or styling changes outside the chess app.

### Relevant Files And Systems

- `Program.cs` - service registration, `PathBase`, Razor Pages, SignalR hub mapping, and database path resolution.
- `Hubs/` - SignalR hub methods and client notifications.
- `Models/` - chess room, game state, moves, persistence DTOs, and future board/rules models.
- `Services/` - room lifecycle, persistence, and future chess rules service.
- `Pages/` - Razor Pages shell for the chess game.
- `wwwroot/js/` - client connection, room flow, board rendering, move interaction, and browser token storage.
- `wwwroot/css/` - responsive chessboard and page styling.
- `docs/` - planning and implementation notes.
- LiteDB database under the resolved app data path for persisted room/game snapshots.

### Technology

- ASP.NET Core 8 Razor Pages.
- SignalR hub at `/gameHub`.
- LiteDB for persisted room/game snapshots.
- Browser client in `wwwroot/js`.
- Responsive HTML/CSS chessboard UI.

### Room And Player Flow

1. White creates a room and receives a six-character room code plus a player token.
2. The app builds an invite link with `?room=CODE`.
3. Black opens the link and joins with the room code.
4. Both players are added to the SignalR group for that room.
5. The server broadcasts state changes after join, reconnect, and every legal move.
6. Player tokens stored in browser local storage allow reconnecting to active games.
7. LiteDB stores room metadata and serialized chess state after every meaningful change.

### SignalR API

Client to server:

- `CreateRoom(playerToken?)`
- `JoinRoom(roomCode, playerToken?)`
- `CheckForActiveGame(playerToken)`
- `ReconnectToGame(playerToken)`
- `MakeMove(from, to, promotion?)`
- `RequestLegalMoves(square)`
- `Resign()`
- `OfferDraw()` later
- `AcceptDraw()` later

Server to client:

- `GameStateUpdated`
- `OpponentJoined`
- `OpponentDisconnected`
- `OpponentReconnected`
- `MoveRejected`
- `PromotionRequired`
- `GameOver`

### Chess State

Persist the current board as FEN because it captures:

- piece placement
- side to move
- castling rights
- en passant target
- halfmove clock
- fullmove number

Also persist move history for display, replay, and future AI/debugging.

Core state:

- current FEN
- side to move
- game status
- winner, if any
- move history
- room/player connection and token metadata

### Rules Engine

The server must be authoritative. The client can help with highlighting and interaction, but the server decides whether a move is legal.

Initial legal move support:

- normal piece movement
- turn enforcement
- capture rules
- king safety
- check detection
- checkmate
- stalemate
- castling
- en passant
- pawn promotion

Later rule enhancements:

- fifty-move draw
- threefold repetition
- insufficient material
- draw offer/accept flow

### Learning Features

Because the target players are learning chess, illegal moves should return useful explanations.

Examples:

- `That piece belongs to Black.`
- `Pawns move forward one square unless capturing.`
- `Your king would still be in check.`
- `You cannot castle through check.`
- `Choose a piece for promotion.`

Client learning helpers:

- highlight selected piece
- highlight legal destinations
- highlight last move
- highlight king in check
- show captured pieces
- show current turn
- show short illegal-move reason

### Ordered Implementation Steps

1. Finish scaffold with SignalR, LiteDB, room lifecycle, and subfolder deployment support.
2. Build a static responsive chessboard UI.
3. Add browser SignalR connection, create room, join room, invite link, token storage, and reconnect prompt.
4. Implement chess board representation and FEN parse/serialize.
5. Implement server-side legal move generation and validation.
6. Wire `MakeMove` to validate, persist, and broadcast `GameStateUpdated`.
7. Add special chess rules: castling, en passant, promotion, checkmate, stalemate.
8. Add focused tests around legal move validation and persistence restore.
9. Publish under the configured `PathBase`, likely `/chess`.

### Acceptance Criteria

- A player can create a room, receive a six-character room code, and get a player token.
- An invite link using `?room=CODE` allows a second player to join as Black.
- Both players receive current game state after create, join, reconnect, and legal moves.
- Browser local storage tokens allow players to reconnect to active games.
- LiteDB persists room metadata and serialized chess state after meaningful changes and can restore an active game.
- The game renders a responsive chessboard in the browser.
- The server validates chess moves authoritatively, including normal movement, captures, turn enforcement, king safety, check, checkmate, stalemate, castling, en passant, and promotion.
- Illegal moves are rejected with useful short explanations for learning players.
- Legal moves update FEN, move history, status, persistence, and both clients through `GameStateUpdated`.
- The app works when hosted under a configured `PathBase`, such as `/chess`.

### Validation Strategy

- Run `dotnet build` for the solution or project after coherent implementation changes.
- Add and run focused automated tests for FEN parse/serialize, legal move validation, special rules, game end states, and persistence restore.
- Manually verify room create, invite join, reconnect, legal move, illegal move, promotion, check/checkmate or stalemate, and refresh/reconnect flows in the browser.
- Manually verify static assets, SignalR hub routing, invite links, and generated URLs under a configured `PathBase`.
- Inspect the resulting diff against this ExecPlan before final handoff to catch scope creep or missed acceptance criteria.

## Living Sections

Update these sections during execution as discoveries, decisions, validation results, and final status change.

### Progress Log

- `2026-07-03 - Completed before ExecPlan retrofit:` ASP.NET Core Razor Pages project exists with LiteDB dependency, SignalR registration, `PathBase` support, and hub mapping.
- `2026-07-03 - Completed before ExecPlan retrofit:` Basic room lifecycle exists in `ChessRoomService`, including six-character room code generation, player tokens, join, reconnect, disconnect handling, and persistence calls.
- `2026-07-03 - Completed before ExecPlan retrofit:` LiteDB persistence service exists with indexes for game id and player tokens.
- `2026-07-03 - Completed before ExecPlan retrofit:` Core room/game persistence models exist, including FEN-backed `ChessGameState`, move history, statuses, and persisted JSON game state.
- `2026-07-03 - Completed before ExecPlan retrofit:` SignalR hub exposes create, join, active-game check, reconnect, disconnect notifications, and a placeholder `MakeMove`.
- `2026-07-03 - Completed before ExecPlan retrofit:` Landing page text identifies the app as a scaffold ready for board UI and rules engine implementation.
- `2026-07-03 - Completed:` Retrofitted this plan into frozen and living sections without changing the original work intent.
- `2026-07-03 - Completed:` Re-read the user-level Codex instructions outside the repository and reconciled this ExecPlan with the required workflow.
- `2026-07-03 - Completed:` Built ordered implementation step 2 as a standalone responsive single-page chessboard UI on `Pages/Index.cshtml`.

### Discoveries

- `2026-07-03:` The active AGENTS instructions include user-level Codex instructions outside the repository and the local repository root `AGENTS.md`.
- `2026-07-03:` The existing in-progress plan is `ChessGame/docs/PLAN.md`; there is no `docs/execplans/` directory in the repository root.
- `2026-07-03:` `MakeMove` currently returns `The chess rules engine is not implemented yet.`
- `2026-07-03:` `wwwroot/js/site.js` still contains only the default template comment, so browser SignalR flow and board interactions are not implemented yet.
- `2026-07-03:` `Pages/Index.cshtml` is a scaffold page, not the playable chess UI.
- `2026-07-03:` Persistence can be disabled if no writable database path is found because `GamePersistenceService` accepts a null path and no-ops.
- `2026-07-03:` The active Codex instructions require risks and rollback notes; this plan now keeps rollback notes with the open risks.
- `2026-07-03:` The default Razor layout adds template navigation and footer chrome that is unnecessary for a single-page game linked from the main website.
- `2026-07-03:` The static UI can render the initial position directly from FEN on the client, which gives the next SignalR slice a clear board update target.

### Decision Log

- `2026-07-03:` Keep using `ChessGame/docs/PLAN.md` as the current ExecPlan location because it is the existing in-progress plan for this repository.
- `2026-07-03:` Treat the existing milestone list as the ordered implementation plan to preserve the original intent.
- `2026-07-03:` Add explicit acceptance criteria derived from the original goal, flow, rules engine, and learning feature sections because the original plan did not have a dedicated acceptance criteria section.
- `2026-07-03:` Add explicit validation steps derived from the original test and publish milestones because the original plan did not have a dedicated validation strategy section.
- `2026-07-03:` Keep future AI, draw offer/accept flow, and later draw rules outside the first-release scope.
- `2026-07-03:` Make `Pages/Index.cshtml` a standalone page with `Layout = null` so the game surface does not depend on scaffold layout chrome.
- `2026-07-03:` Keep the room action buttons disabled in the static-board slice because SignalR browser wiring is the next ordered implementation step.

### Open Questions, Risks, And Rollback Notes

- The Battleship and Connect Four reference projects are mentioned by the original plan but are not identified by path in this repository.
- There is no test project visible yet; adding focused tests may require creating one without changing the product scope.
- Persistence silently no-ops if every candidate database path is unwritable; decide later whether that is acceptable for deployment validation.
- The exact production `PathBase` value is likely `/chess`, but deployment should verify the configured value before publish.
- The final UI design must remain focused on the playable game, not a marketing or landing page.
- Roll back failed implementation changes by reverting the targeted files from the coherent step in progress, preserving this ExecPlan and any unrelated user changes.
- For risky rule-engine changes, keep tests close to the changed behavior so regressions can be isolated and reverted without undoing room, persistence, or UI work.

### Validation Results

- `2026-07-03:` No code validation was run for this plan-only retrofit.
- `2026-07-03:` `dotnet build ChessGame.sln` succeeded with 0 warnings and 0 errors after the static board UI implementation.
- `2026-07-03:` Scanned changed tracked files for private paths and machine-specific references; no matches were found.
- `2026-07-03:` Local app responded with HTTP 200 at `http://localhost:5213/` after starting the development server.
- `2026-07-03:` `git diff --check` passed; output only included line-ending normalization warnings.

### Final Outcome

- Static responsive chessboard UI slice is complete. Implementation should resume with ordered implementation step 3: browser SignalR connection, room create/join, invite link, token storage, and reconnect prompt.
