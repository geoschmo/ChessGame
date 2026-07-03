# Chess Game Plan

## Goal

Build a browser-playable two-player chess game that follows the existing Connect Four and Battleship deployment pattern: standalone ASP.NET Core app, hosted under a website subfolder, with SignalR multiplayer, invite links, reconnect support, and persisted game state.

The first release is two-player only. A simple AI can be added later by reusing the same server-side chess rules engine.

## Existing Pattern To Follow

- Use the Battleship project as the primary template because it already has SignalR rooms, LiteDB persistence, player tokens, reconnect flow, and `PathBase` support.
- Use the Connect Four project as a reference for a simpler single-state broadcast model.
- Keep the app deployable as a subfolder page, for example `/chess`, through the `PathBase` configuration setting.

## Technology

- ASP.NET Core 8 Razor Pages
- SignalR hub at `/gameHub`
- LiteDB for persisted room/game snapshots
- Browser client in `wwwroot/js`
- Responsive HTML/CSS chessboard UI

## Main Folders

- `Hubs/` - SignalR hub methods and client notifications
- `Models/` - chess room, game state, moves, persistence DTOs
- `Services/` - room lifecycle, persistence, chess rules service
- `wwwroot/js/` - client connection, board rendering, move interaction
- `wwwroot/css/` - board and page styling
- `docs/` - planning and implementation notes

## Room And Player Flow

1. White creates a room and receives a six-character room code plus a player token.
2. The app builds an invite link with `?room=CODE`.
3. Black opens the link and joins with the room code.
4. Both players are added to the SignalR group for that room.
5. The server broadcasts state changes after join, reconnect, and every legal move.
6. Player tokens stored in browser local storage allow reconnecting to active games.
7. LiteDB stores room metadata and serialized chess state after every meaningful change.

## SignalR API

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

## Chess State

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

## Rules Engine

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

## Learning Features

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

## First Implementation Milestones

1. Finish scaffold with SignalR, LiteDB, room lifecycle, and subfolder deployment support.
2. Build a static responsive chessboard UI.
3. Add browser SignalR connection, create room, join room, invite link, token storage, and reconnect prompt.
4. Implement chess board representation and FEN parse/serialize.
5. Implement server-side legal move generation and validation.
6. Wire `MakeMove` to validate, persist, and broadcast `GameStateUpdated`.
7. Add special chess rules: castling, en passant, promotion, checkmate, stalemate.
8. Add focused tests around legal move validation and persistence restore.
9. Publish under the configured `PathBase`, likely `/chess`.

## Future AI Direction

Keep the chess rules engine independent from SignalR and UI code. A later single-player AI should be able to ask the engine for legal moves, evaluate board positions, and submit moves through the same move application path used by multiplayer games.
