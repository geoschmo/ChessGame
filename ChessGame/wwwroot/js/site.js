const initialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

const pieceGlyphs = {
  K: "♔",
  Q: "♕",
  R: "♖",
  B: "♗",
  N: "♘",
  P: "♙",
  k: "♚",
  q: "♛",
  r: "♜",
  b: "♝",
  n: "♞",
  p: "♟"
};

const pieceNames = {
  K: "white king",
  Q: "white queen",
  R: "white rook",
  B: "white bishop",
  N: "white knight",
  P: "white pawn",
  k: "black king",
  q: "black queen",
  r: "black rook",
  b: "black bishop",
  n: "black knight",
  p: "black pawn"
};

document.addEventListener("DOMContentLoaded", () => {
  const board = document.getElementById("chessBoard");
  if (!board) {
    return;
  }

  renderBoard(board, initialFen);
});

function renderBoard(board, fen) {
  board.replaceChildren();

  const position = parseFenPosition(fen);
  const files = ["a", "b", "c", "d", "e", "f", "g", "h"];

  for (let rank = 8; rank >= 1; rank -= 1) {
    for (let fileIndex = 0; fileIndex < files.length; fileIndex += 1) {
      const squareName = `${files[fileIndex]}${rank}`;
      const piece = position[squareName];
      const square = document.createElement("button");
      const isLight = (rank + fileIndex) % 2 === 0;

      square.type = "button";
      square.className = `square ${isLight ? "light" : "dark"}`;
      square.dataset.square = squareName;
      square.setAttribute("role", "gridcell");
      square.setAttribute("aria-label", piece ? `${squareName}, ${pieceNames[piece]}` : `${squareName}, empty`);

      if (piece) {
        const pieceElement = document.createElement("span");
        pieceElement.className = `piece ${piece === piece.toUpperCase() ? "white-piece" : "black-piece"}`;
        pieceElement.setAttribute("aria-hidden", "true");
        pieceElement.textContent = pieceGlyphs[piece];
        square.appendChild(pieceElement);
      }

      square.addEventListener("click", () => selectSquare(board, square));
      board.appendChild(square);
    }
  }
}

function parseFenPosition(fen) {
  const [placement] = fen.split(" ");
  const ranks = placement.split("/");
  const files = ["a", "b", "c", "d", "e", "f", "g", "h"];
  const position = {};

  ranks.forEach((rankPlacement, rankIndex) => {
    let fileIndex = 0;
    const rank = 8 - rankIndex;

    for (const value of rankPlacement) {
      const emptyCount = Number.parseInt(value, 10);
      if (Number.isInteger(emptyCount)) {
        fileIndex += emptyCount;
        continue;
      }

      position[`${files[fileIndex]}${rank}`] = value;
      fileIndex += 1;
    }
  });

  return position;
}

function selectSquare(board, square) {
  const selected = board.querySelector(".square.selected");
  if (selected && selected !== square) {
    selected.classList.remove("selected");
  }

  square.classList.toggle("selected");
}
