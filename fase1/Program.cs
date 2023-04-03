using System;
using System.Collections.Generic;

namespace DesafioStone
{
    public class Board
    {
        public List<List<byte>> InitialState { get; set; }
        public List<List<short>> HeuristicsGrid { get; set; }
        public List<List<List<byte>>> GridList { get; set; }

        public Board()
        {
            this.InitialState = this.GetInitialState();
            this.HeuristicsGrid = this.GetHeuristicsGrid(this.InitialState);
            this.GridList = this.GetGridList(this.InitialState, 300);
        }

        private List<List<byte>> GetInitialState()
        {
            string[] lines = File.ReadAllLines("input.txt");
            return lines.Select(line => line.Split(" ").Select(str => byte.Parse(str)).ToList()).ToList();
        }

        private List<List<short>> GetHeuristicsGrid(List<List<byte>> initialBoard)
        {
            return initialBoard.Select((row, rowIndex) => row.Select((col, colIndex) => (short)(Math.Abs(rowIndex - (initialBoard.Count - 1)) + Math.Abs(colIndex - (initialBoard[0].Count - 1)))).ToList()).ToList();
        }

        private List<List<List<byte>>> GetGridList(List<List<byte>> initialGrid, int numberOfStates)
        {
            int i = 0;
            List<List<List<byte>>> stateList = new();
            stateList.Add(initialGrid);

            while (i < numberOfStates)
            {
                var newGrid = this.GetNextGrid(stateList[stateList.Count - 1]);
                stateList.Add(newGrid);
                i++;
            }

            return stateList;
        }

        private List<List<byte>> GetNextGrid(List<List<byte>> grid)
        {
            return grid.Select((row, rowIndex) => row.Select((el, colIndex) => this.GetNextCellValue(el, this.CountNeighbours(grid, rowIndex, colIndex))).ToList()).ToList();
        }

        private byte GetNextCellValue(byte cellValue, byte neighboursCount)
        {
            switch (cellValue)
            {
                case 0:
                    return neighboursCount > 1 && neighboursCount < 5 ? (byte)1 : (byte)0;
                case 1:
                    return neighboursCount <= 3 || neighboursCount >= 6 ? (byte)0 : (byte)1;
                default:
                    return cellValue;
            }

        }

        private byte CountNeighbours(List<List<byte>> grid, int rowIndex, int columnIndex)
        {
            var lastRow = grid.Count - 1;
            var lastColumn = grid[0].Count - 1;
            byte neighboursCount = 0;

            for (var x = Math.Max(0, rowIndex - 1); x <= Math.Min(rowIndex + 1, lastRow); x++)
            {
                for (var y = Math.Max(0, columnIndex - 1); y <= Math.Min(columnIndex + 1, lastColumn); y++)
                {
                    if (x != rowIndex || y != columnIndex)
                    {
                        if (grid[x][y] == 1) neighboursCount++;
                    }
                }
            }
            return neighboursCount;
        }
    }

    public class Node
    {

        public byte Row { get; set; } = 0;
        public byte Column { get; set; } = 0;
        public short H { get; set; } = 0;
        public short Turn { get; set; } = 0;
        public List<string> Path { get; set; }

        public Node()
        {
            this.Path = new();
        }

        public bool EqualsString(Node node)
        {
            return node.ToString() == this.ToString();
        }

        public Node(byte row, byte column, short h, short turn, List<string> path)
        {
            Row = row;
            Column = column;
            H = h;
            Turn = turn;
            Path = path;
        }

        public string ToString()
        {
            return $"r{this.Row}c{this.Column}t{this.Turn}";
        }

        public List<Node> getNextNodes(Board board)
        {
            var nextGrid = board.GridList[this.Turn + 1];

            List<Movement> moves = new();
            moves.Add(new Movement("U", -1, 0));
            moves.Add(new Movement("D", 1, 0));
            moves.Add(new Movement("L", 0, -1));
            moves.Add(new Movement("R", 0, 1));

            var possibleMoves = moves.Where(move => this.GetBoundaries(move, board));

            return possibleMoves.Select((possibleMove) => new Node(
                                (byte)(possibleMove.Row + this.Row),
                                (byte)(possibleMove.Column + this.Column),
                                board.HeuristicsGrid[possibleMove.Row + this.Row][possibleMove.Column + this.Column],
                                (short)(this.Turn + 1),
                                this.Path.Append(possibleMove.Direction).ToList()))
                                .ToList();
        }

        public bool GetBoundaries(Movement move, Board board)
        {
            return this.Row + move.Row >= 0 &&
                    this.Column + move.Column >= 0 &&
                    this.Row + move.Row < board.InitialState.Count &&
                    this.Column + move.Column < board.InitialState[0].Count &&
                    board.GridList[this.Turn + 1][this.Row + move.Row][this.Column + move.Column] != 1;
        }
    }

    public class Movement
    {
        public string Direction { get; set; }
        public sbyte Row { get; set; }
        public sbyte Column { get; set; }

        public Movement(string direction, sbyte row, sbyte column)
        {
            Direction = direction;
            Row = row;
            Column = column;
        }

        public Movement()
        {
        }
    }

    internal class Program
    {
        static void findPath()
        {
            Board board = new Board();
            Node startNode = new Node();
            bool foundSolution = false;

            var openSet = new PriorityQueue<Node, int>();
            openSet.Enqueue(startNode, startNode.H + startNode.Turn);

            HashSet<string> closedSet = new();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (openSet.Count > 0 && !foundSolution)
            {
                Node selectedNode = openSet.Dequeue();

                var nextNodes = selectedNode.getNextNodes(board);

                foreach (Node nextNode in nextNodes)
                {
                    if (closedSet.Contains(nextNode.ToString())) continue;

                    if (board.GridList[nextNode.Turn][nextNode.Row][nextNode.Column] == 4)
                    {
                        watch.Stop();
                        foundSolution = true;
                        System.Console.WriteLine("Solution found!!");
                        System.Console.WriteLine($"Turn {nextNode.Turn}");
                        System.Console.WriteLine(watch.ElapsedMilliseconds + "ms");

                        break;
                    }
                    openSet.Enqueue(nextNode, nextNode.H + nextNode.Turn);
                    closedSet.Add(nextNode.ToString());
                }
            }

        }

        static void Main(string[] args)
        {
            try
            {
                findPath();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }
}
