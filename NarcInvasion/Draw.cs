using System;
using System.Collections.Generic;
using System.Text;

namespace NarcInvasion
{
    class Draw
    {
        private static readonly object ConsoleWriterLock = new object();
        private static readonly int maxX = Console.WindowWidth - 1;
        private static readonly int maxY = Console.WindowHeight - 1;

        public static void DrawObject(GameObject gameObject)
        {
            WriteLinesAt(gameObject.Lines, gameObject.X, gameObject.Y, gameObject.Color);
        }

        public static void DrawObjects(IEnumerable<GameObject> gameObjects)
        {
            foreach (GameObject gameObject in gameObjects) DrawObject(gameObject);
        }

        public static void UndrawObject(GameObject gameObject)
        {
            string spaces = "";
            foreach (string line in gameObject.Lines)
            {
                foreach (char c in line) { spaces += " "; }
                spaces += Environment.NewLine;
            }
            GameObject spacesObject = new GameObject(spaces, gameObject.X, gameObject.Y, gameObject.Color);
            DrawObject(spacesObject);
        }

        public static void UndrawObjects(List<GameObject> gameObjects)
        {
            foreach (GameObject gameObject in gameObjects) UndrawObject(gameObject);
        }

        private static void WriteLinesAt(string[] lines, double x, double y, ConsoleColor color)
        {
            foreach (string line in lines)
            {
                WriteLineAt(line, x, y, color);
                y++;
            }
        }

        private static void WriteLineAt(string s, double x, double y, ConsoleColor color)
        {
            try
            {
                lock (ConsoleWriterLock)
                {
                    foreach (char c in s)
                    {
                        if (x > 0 && x < maxX && y >= 0 && y <= maxY)
                        {
                            Console.SetCursorPosition((int)x, (int)y);
                            Console.ForegroundColor = color;
                            Console.Write(c);
                        }
                        x++;
                    }
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
