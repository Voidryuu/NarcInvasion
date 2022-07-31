using System;
using System.Collections.Generic;
using System.Text;

namespace NarcInvasion
{
    public class GameObject
    {
        private string text = "";
        public string Text { 
            get { return text; } 
            set
            {
                text = value;
                string[] lines = text.Split(Environment.NewLine);
                Lines = lines;
                Height = lines.Length;

                int width = 0;
                foreach (string line in lines) { if (line.Length > width) width = line.Length; }
                Width = width;
            }
        }
        public string[] Lines { get; private set; }
        public double Height { get; private set; }
        public double Width { get; private set; }

        /// <summary>
        /// X location of the topleft corner of the object
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Y location of the topleft corner of the object
        /// </summary>
        public double Y { get; set; }
        public double XCenter { get { return X + Width / 2; } }
        public double YCenter { get { return Y + Height / 2; } }
        public ConsoleColor Color { get; set; }
        public Queue<Dialogue> Dialogue { get; set; }

        public GameObject(string text, double x, double y, ConsoleColor color)
        {
            Text = text;
            X = x;
            Y = y;
            Color = color;
            Dialogue = new Queue<Dialogue>();
        }

        internal void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    X--;
                    break;
                case Direction.Right:
                    X++;
                    break;
                case Direction.Up:
                    Y--;
                    break;
                case Direction.Down:
                    Y++;
                    break;
                default:
                    break;
            }
        }
    }
}
