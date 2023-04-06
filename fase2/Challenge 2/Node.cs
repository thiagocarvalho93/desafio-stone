using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioStone
{
    public class Node
    {
        public short Row { get; set; } = 0;
        public short Column { get; set; } = 0;
        public short H { get; set; } = 0;
        public ushort Turn { get; set; } = 0;
        public List<char> Path { get; set; }
        public byte Lives { get; set; } = 6;

        public Node()
        {
            this.Path = new();
        }

        public Node(short row, short column, short h, ushort turn, List<char> path)
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
            ushort nextTurn = (ushort)(this.Turn + 1);
            var nextGrid = board.GridList[nextTurn];

            List<Node> nextNodes = new();

            foreach (Movement moveOption in moveOptions)
            {
                if (this.GetBoundaries(moveOption, board))
                {
                    short r = (short)(moveOption.Row + this.Row);
                    short c = (short)(moveOption.Column + this.Column);
                    bool lostLife = board.GridList[nextTurn][r][c] == 1;
                    int lives = lostLife ? (this.Lives - 1) : this.Lives;

                    nextNodes.Add(new Node()
                    {
                        Column = c,
                        Row = r,
                        H = board.HeuristicsGrid[r][c],
                        Path = this.Path.Append(moveOption.Direction).ToList(),
                        Lives = (byte)lives,
                        Turn = nextTurn
                    });
                }
            }

            return nextNodes;
        }

        public bool GetBoundaries(Movement move, Board board)
        {
            if (this.Row + move.Row < 0 || this.Column + move.Column < 0 || this.Row + move.Row >= board.R || this.Column + move.Column >= board.C)
                return false;
 
            if (board.GridList[this.Turn + 1][this.Row + move.Row][this.Column + move.Column] == 1)
            {
                if (this.Lives > 0) return true;
                return false;
            }
 
            return true;
        }
    }
}