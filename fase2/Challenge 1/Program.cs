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
            this.GridList = new byte[10000][][];
            this.GridList[0] = this.InitialState;
            this.GetNextPropagations(50);
        }

        private byte[][] GetInitialState()
        {
            string[] lines = File.ReadAllLines("input1.txt");
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
                this.GridList[this.LastTurnGenerated + 1] = this.GetNextGridParallel(this.GridList[this.LastTurnGenerated]);
                this.LastTurnGenerated++;
                System.Console.WriteLine(this.LastTurnGenerated);
            }
            System.Console.WriteLine("OK");
        }

        private byte[][] GetNextGrid(byte[][] grid)
        {
            return grid.Select((row, rowIndex) => row.Select((el, colIndex) => this.GetNextCellValue(el, this.CountNeighbours(grid, rowIndex, colIndex))).ToArray()).ToArray();
        }

        private byte[][] GetNextGridParallel(byte[][] grid)
        {
            // Split the rows into chunks
            int chunkSize = this.R / Environment.ProcessorCount;
            int lastChunkSize = this.R - chunkSize * (Environment.ProcessorCount - 1);
            List<Tuple<int, int>> chunks = new List<Tuple<int, int>>();
            int start = 0;
            for (int i = 0; i < Environment.ProcessorCount - 1; i++)
            {
                chunks.Add(new Tuple<int, int>(start, start + chunkSize));
                start += chunkSize;
            }
            chunks.Add(new Tuple<int, int>(start, start + lastChunkSize));

            // Create a list of tasks, one for each chunk
            List<Task<byte[][]>> tasks = new List<Task<byte[][]>>();
            foreach (Tuple<int, int> chunk in chunks)
            {
                int chunkStart = chunk.Item1;
                int chunkEnd = chunk.Item2;
                tasks.Add(Task.Run(() =>
                {
                    byte[][] chunkResult = new byte[chunkEnd - chunkStart][];
                    for (int i = chunkStart; i < chunkEnd; i++)
                    {
                        byte[] row = grid[i];
                        byte[] newRow = new byte[this.C];
                        for (int j = 0; j < this.C; j++)
                        {
                            newRow[j] = this.GetNextCellValue(row[j], this.CountNeighbours(grid, i, j));
                        }
                        chunkResult[i - chunkStart] = newRow;
                    }
                    return chunkResult;
                }));
            }

            // Wait for all tasks to complete and merge the results
            Task.WaitAll(tasks.ToArray());
            byte[][] result = new byte[this.R][];
            int index = 0;
            foreach (Task<byte[][]> task in tasks)
            {
                byte[][] chunkResult = task.Result;
                foreach (byte[] row in chunkResult)
                {
                    result[index] = row;
                    index++;
                }
            }
            return result;
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

        public void RemoveUntilTurn(int turn)
        {
            for (int i = 0; i < turn; i++)
            {
                this.GridList[i] = new byte[0][];
            }
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

        public Node(byte row, byte column, short h, short turn, List<string> path)
        {
            Row = row;
            Column = column;
            H = h;
            Turn = turn;
            Path = path;
        }

        public bool EqualsString(Node node)
        {
            return node.ToString() == this.ToString();
        }


        public string ToString()
        {
            return $"r{this.Row}c{this.Column}t{this.Turn}";
        }

        public void WriteSolutionFile(string filePath)
        {
            string text = string.Join(" ", this.Path);
            File.WriteAllText(filePath, text);
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

                // Generate new propagations on demand
                if (selectedNode.Turn >= board.LastTurnGenerated)
                {
                    // Clear memory
                    // Remove boards that will no longer be used
                    var worstTurn = openSet.UnorderedItems.MinBy(x => x.Element.Turn).Element.Turn;
                    board.RemoveUntilTurn(worstTurn - 1);
                    // Discard elements with more than double the best Heuristic
                    var topElement = openSet.Peek();
                    var newSet = new PriorityQueue<Node, int>();
                    foreach (var node in openSet.UnorderedItems)
                    {
                        if (node.Element.H <= topElement.H * 2)
                        {
                            newSet.Enqueue(node.Element, node.Priority);
                        }
                    }
                    board.GetNextPropagations(200);

                    System.Console.WriteLine("Finding path...");
                }

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

                        nextNode.WriteSolutionFile("solution.txt");

                        break;
                    }
                    openSet.Enqueue(nextNode, nextNode.H + nextNode.Turn);
                    closedSet.Add(nextNode.ToString());
                }
            }
        }

        //TODO: Solution tester
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
