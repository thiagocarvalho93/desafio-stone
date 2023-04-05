const fs = require("fs");
// The file "input.txt" must be in the same directory

class QElement {
  constructor(element, priority) {
    this.element = element;
    this.priority = priority;
  }
}

class PriorityQueue {
  constructor() {
    this.items = [];
  }

  enqueue(element, priority) {
    var qElement = new QElement(element, priority);
    var contain = false;

    for (var i = 0; i < this.items.length; i++) {
      if (this.items[i].priority > qElement.priority) {
        this.items.splice(i, 0, qElement);
        contain = true;
        break;
      }
    }

    if (!contain) {
      this.items.push(qElement);
    }
  }

  dequeue() {
    if (this.isEmpty()) return "Underflow";
    return this.items.shift();
  }

  isEmpty() {
    return this.items.length == 0;
  }
}

class Board {
  constructor(obj) {
    obj = obj || {};
    this.colSize = obj.colSize || 65;
    this.rowSize = obj.rowSize || 85;
    this.initialState = obj.initialState || this.readInitialState();
    this.stateList = [this.initialState];
    this.heuristicsGrid = obj.heuristicsGrid || [];

    this.generateStates();
    this.calculateHeuristics();
  }

  generateStates() {
    let n = 0;
    while (n < 500) {
      let newState = this.nextBoard(this.stateList[this.stateList.length - 1]);
      this.stateList.push(newState);
      n++;
    }
  }

  calculateHeuristics() {
    this.heuristicsGrid = this.initialState.map((col, colIndex) =>
      col.map(
        (row, rowIndex) =>
          Math.abs(colIndex - (this.initialState.length - 1)) +
          Math.abs(rowIndex - (this.initialState[0].length - 1))
      )
    );
  }

  readInitialState() {
    try {
      const input = fs.readFileSync("input.txt", "utf-8");
      const nums = input.match(/\d+/g).map(Number);
      const state = [];
      for (let i = 0; i < nums.length; i += this.rowSize) {
        state.push(nums.slice(i, i + this.rowSize));
      }
      return state;
    } catch (err) {
      console.error(`Failed to read initial state from file: ${err}`);
      return [];
    }
  }

  nextBoard(boardState) {
    boardState = boardState.map((row, rowIndex) =>
      row.map((el, columnIndex) =>
        this.switchValue(el, this.checkNeighboursCount(boardState, rowIndex, columnIndex))
      )
    );
    return boardState;
  }

  switchValue(value, neighboursCount) {
    switch (value) {
      case 0:
        return neighboursCount > 1 && neighboursCount < 5 ? 1 : 0;
      case 1:
        return neighboursCount <= 3 || neighboursCount >= 6 ? 0 : 1;
      default:
        return value;
    }
  }

  checkNeighboursCount(state, rowIndex, columnIndex) {
    var lastRow = state.length - 1;
    var lastColumn = state[0].length - 1;
    var neighboursCount = 0;

    for (var x = Math.max(0, rowIndex - 1); x <= Math.min(rowIndex + 1, lastRow); x++) {
      for (var y = Math.max(0, columnIndex - 1); y <= Math.min(columnIndex + 1, lastColumn); y++) {
        if (x !== rowIndex || y !== columnIndex) {
          if (state[x][y] === 1) {
            neighboursCount++;
          }
        }
      }
    }
    return neighboursCount;
  }
}

class Node {
  constructor(obj) {
    obj = obj || {};
    this.row = obj.row || 0;
    this.column = obj.column || 0;
    this.h = obj.h || 0;
    this.turn = obj.turn || 0;
    this.path = obj.path || [];
  }

  toString() {
    return `r${this.row}c${this.column}t${this.turn}`;
  }

  equals(node) {
    return this.row == node.row && this.column == node.column && this.turn == node.turn;
  }

  getNextNodes(board) {
    const { stateList } = board;
    const { row, column, turn, path } = this;

    const nextNodes = [
      { move: "U", row: -1, column: 0 },
      { move: "D", row: 1, column: 0 },
      { move: "L", row: 0, column: -1 },
      { move: "R", row: 0, column: 1 },
    ]
      .filter(
        ({ row: r, column: c }) =>
          row + r >= 0 &&
          column + c >= 0 &&
          row + r < board.initialState.length &&
          column + c < board.initialState[0].length &&
          stateList[turn + 1][row + r][column + c] !== 1
      )
      .map(
        ({ move, row: r, column: c, parentNode }) =>
          new Node({
            row: r + row,
            column: c + column,
            turn: turn + 1,
            parentNode,
            h: board.heuristicsGrid[r + row][c + column],
            path: [...path, move],
          })
      );

    return nextNodes;
  }

  writePathFile(filePath) {
    let string = this.path.join(" ");
    fs.writeFileSync(filePath, string, (err) => {
      if (err) throw err;
      else {
        console.log(`The solution file was created in ${path}.`);
      }
    });
  }
}

// A* algorithm implementation
function findPath() {
  let board = new Board();
  let startNode = new Node();
  let foundSolution = false;

  let openSet = new PriorityQueue();
  let closedSet = new Set();

  openSet.enqueue(startNode, startNode.h + startNode.turn);

  let startTime = new Date();
  console.log("Processing solution...");

  while (!openSet.isEmpty() && !foundSolution) {
    nodeSelected = openSet.dequeue();

    // Get next nodes
    let nextNodes = nodeSelected.element.getNextNodes(board);

    for (const nextNode of nextNodes) {
      if (closedSet.has(nextNode.toString())) continue;

      // Check if game won
      if (board.stateList[nextNode.turn][nextNode.row][nextNode.column] == 4) {
        foundSolution = true;
        console.log("Solution found!");
        console.log(`Solution turn ${nextNode.turn}`);
        console.log(`Execution time: ${new Date() - startTime}ms`);

        const isValidSolution = validateSolution(board, nextNode.path);

        if (isValidSolution) {
          nextNode.writePathFile("solution.txt");
        }
        break;
      }
      openSet.enqueue(nextNode, nextNode.h + nextNode.turn);
      closedSet.add(nextNode.toString());
    }
  }
}

function validateSolution(board, solution) {
  var position = [0, 0];
  console.log("Checking solution...");

  for (let [index, move] of solution.entries()) {
    switch (move) {
      case "U":
        position[0]--;
        break;
      case "D":
        position[0]++;
        break;
      case "R":
        position[1]++;
        break;
      case "L":
        position[1]--;
        break;
      default:
        break;
    }

    if (board.stateList[index + 1][position[0]][position[1]] == 1) {
      console.log(`Solution failed on turn ${index + 1}, row ${position[0]}, col ${position[1]}.`);
      return false;
    }
  }

  if (board.stateList[solution.length - 1][position[0]][position[1]] == 4) {
    console.log("Solution is valid! :)");
    return true;
  } else {
    console.log("Solution is not valid... :(");
    return false;
  }
}

try {
  findPath();
} catch (err) {
  console.log(err);
}
