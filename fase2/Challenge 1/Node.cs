using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioStone
{
    public class Node
    {
        public byte Row { get; set; } = 0;
        public byte Column { get; set; } = 0;
        public short H { get; set; } = 0;
        public short Turn { get; set; } = 0;
        public List<char> Path { get; set; }

        public Node()
        {
            this.Path = new();
        }

        public Node(byte row, byte column, short h, short turn, List<char> path)
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

            var possibleMoves = moveOptions.Where(move => this.GetBoundaries(move, board));

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

}