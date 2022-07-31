using System;
using System.Collections.Generic;
using System.Text;

namespace NarcInvasion
{
    public class Dialogue : GameObject
    {
        public Answer Answer { get; set; }

        public string SoundPath { get; set; }

        public Dialogue(string text, double x, double y, ConsoleColor color, Answer answer, string soundPath) : base(text, x, y, color)
        {
            Answer = answer;
            SoundPath = soundPath;
        }

        public Dialogue(string text, double x, double y, ConsoleColor color) : this(text, x, y, color, null, null) { }
        public Dialogue(string text, double x, double y, ConsoleColor color, Answer answer) : this(text, x, y, color, answer, null) { }
        public Dialogue(string text, double x, double y, ConsoleColor color, string soundPath) : this(text, x, y, color, null, soundPath) { }
    }
}
