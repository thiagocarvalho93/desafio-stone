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
            this.GetNextPropagations(300);
        }

        private byte[][] GetInitialState()
        {
            string[] lines = File.ReadAllLines("input.txt");
            return lines.Select(line => line.Split(" ").Select(str => byte.Parse(str)).ToArray()).ToArray();
        }

        private short[][] GetHeuristicsGrid(byte[][] initialBoard)
        {
            System.Console.WriteLine("Calculating heuristics in advance...");
            var grid = initialBoard.Select((row, rowIndex) => row.Select((col, colIndex) => (short)(Math.Abs(rowIndex - (this.R - 1)) + Math.Abs(colIndex - (this.C - 1)))).ToArray()).ToArray();

            return grid;
        }

        public void GetNextPropagations(int numberOfStates)
        {
            System.Console.WriteLine("Generating propagations in advance...");
            int initialTurn = this.LastTurnGenerated;

            while (this.LastTurnGenerated < initialTurn + numberOfStates)
            {
                this.GridList[this.LastTurnGenerated + 1] = this.GetNextGridParallel(this.GridList[this.LastTurnGenerated]);
                this.LastTurnGenerated++;
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

}