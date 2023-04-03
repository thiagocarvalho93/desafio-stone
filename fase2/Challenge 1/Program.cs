using System;
using System.Collections.Generic;

namespace DesafioStone
{
    public class Board
    {
        public byte[][] InitialState { get; set; }
        public short[][] HeuristicsGrid { get; set; }
        public byte[][][] GridList { get; set; }
        public int R { get; set; } = 0;
        public int C { get; set; } = 0;
        public int LastTurnGenerated { get; set; } = 0;

        public Board()
        {
            this.InitialState = this.GetInitialState();
            this.R = this.InitialState.Count();
            this.C = this.InitialState[0].Count();
            this.HeuristicsGrid = this.GetHeuristicsGrid(this.InitialState);
            this.GridList = new byte[50000][][];
            this.GridList[0] = this.InitialState;
            this.GetNextPropagations(200);
        }

        private byte[][] GetInitialState()
        {
            string[] lines = File.ReadAllLines("input.txt");
            return lines.Select(line => line.Split(" ").Select(str => byte.Parse(str)).ToArray()).ToArray();
        }

        private short[][] GetHeuristicsGrid(byte[][] initialBoard)
        {
            System.Console.WriteLine("Calculating heuristics in advance...");
            return initialBoard.Select((row, rowIndex) => row.Select((col, colIndex) => (short)(Math.Abs(rowIndex - (this.R - 1)) + Math.Abs(colIndex - (this.C - 1)))).ToArray()).ToArray();
        }

        public void GetNextPropagations(int numberOfStates)
        {
            System.Console.WriteLine("Generating propagations in advance...");
            int initialTurn = this.LastTurnGenerated;

            while (this.LastTurnGenerated < initialTurn + numberOfStates)
            {
                this.GridList[this.LastTurnGenerated + 1] = this.GetNextGrid(this.GridList[this.LastTurnGenerated]);
                this.LastTurnGenerated++;
                System.Console.WriteLine(this.LastTurnGenerated);
            }
            System.Console.WriteLine("OK");
        }

        private byte[][] GetNextGrid(byte[][] grid)
        {
            return grid.Select((row, rowIndex) => row.Select((el, colIndex) => this.GetNextCellValue(el, this.CountNeighbours(grid, rowIndex, colIndex))).ToArray()).ToArray();
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

        private byte CountNeighbours(byte[][] grid, int rowIndex, int columnIndex)
        {
            byte neighboursCount = 0;

            for (var x = Math.Max(0, rowIndex - 1); x <= Math.Min(rowIndex + 1, this.R - 1); x++)
            {
                for (var y = Math.Max(0, columnIndex - 1); y <= Math.Min(columnIndex + 1, this.C - 1); y++)
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

            if (this.Turn >= board.LastTurnGenerated)
            {
                board.GetNextPropagations(200);
            }

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
                    this.Row + move.Row < board.R &&
                    this.Column + move.Column < board.C &&
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

            System.Console.WriteLine("Finding path...");

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
