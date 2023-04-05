using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioStone
{
    public class Node
    {
        public int Row { get; set; } = 0;
        public int Column { get; set; } = 0;
        public short H { get; set; } = 0;
        public ushort Turn { get; set; } = 0;
        public List<char> Path { get; set; }

        public Node()
        {
            this.Path = new();
        }

        public Node(byte row, byte column, short h, ushort turn, List<char> path)
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

        public void WriteNodePathFile(string filePath)
        {
            string text = string.Join(" ", this.Path);
            File.WriteAllText(filePath, text);
        }

        public List<Node> getNextNodes(Board board, List<Movement> moveOptions)
        {
            var nextGrid = board.GridList[this.Turn + 1];

            List<Node> nextNodes = new();

            foreach (Movement moveOption in moveOptions)
            {
                if (this.GetBoundaries(moveOption, board))
                {
                    int r = moveOption.Row + this.Row;
                    int c = moveOption.Column + this.Column;
                    nextNodes.Add(new Node()
                    {
                        Column = c,
                        Row = r,
                        H = board.HeuristicsGrid[r][c],
                        Path = this.Path.Append(moveOption.Direction).ToList(),
                        Turn = (ushort)(this.Turn + 1)
                    });
                }
            }

            return nextNodes;
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

}