using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioStone
{
    public struct Movement
    {
        public char Direction { get; set; }
        public sbyte Row { get; set; }
        public sbyte Column { get; set; }

        public Movement(char direction, sbyte row, sbyte column)
        {
            Direction = direction;
            Row = row;
            Column = column;
        }

        public Movement()
        {
        }
    }

}