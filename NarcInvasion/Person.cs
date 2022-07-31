using System;
using System.Collections.Generic;
using System.Text;

namespace NarcInvasion
{
    class Person : GameObject
    {
        public bool IsNarc { get; set; }

        public Person(string text, double x, double y, ConsoleColor color, bool isNarc) : base(text, x, y, color)
        {
            IsNarc = isNarc;
        }

        public Person(string text, double x, double y, ConsoleColor color) : this(text, x, y, color, false) { }
    }
}
