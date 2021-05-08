using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hypersync
{
    public class cpoint
    {
        public int hLeft { get; set; }
        public int hTop { get; set; }
        public int printed { get; set; }
        public string printedtext { get; set; }

        public cpoint(int spot)
        {
            hLeft = 0;
            hTop = Console.CursorTop + spot;
            printed = 0;
            printedtext = "";
        }
    }

    public class DisplayManager
    {
        cpoint[] positions = new[] { new cpoint(0), new cpoint(1), new cpoint(2), new cpoint(4) }; // Fourth row is error/warning output

        public void WriteToConsole(string Text, int row, ConsoleColor? consoleColor = null)
        {
            // FYI, If this method is used more will need add clear extra
            Console.SetCursorPosition(positions[row].printed, this.positions[row].hTop);
            positions[row].printed += Text.Length;
            positions[row].printedtext += Text;
            if (!object.ReferenceEquals(null, consoleColor)) { Console.ForegroundColor = consoleColor.Value; }
            Console.Write(Text);
            Console.ResetColor();
        }
        public void AppendToConsole(string Text, ConsoleColor? consoleColor = null)
        {
            if (!object.ReferenceEquals(null, consoleColor)) { Console.ForegroundColor = consoleColor.Value; }
            Console.Write(Text);
            Console.ResetColor();
        }

        public void AppendLineToConsole(string Text, ConsoleColor? consoleColor = null)
        {
            if (!object.ReferenceEquals(null, consoleColor)) { Console.ForegroundColor = consoleColor.Value; }
            Console.WriteLine(Text);
            Console.ResetColor();
        }

        public void hWriteToConsole(string Text, int row, ConsoleColor? consoleColor = null)
        {
            if (Text == positions[row].printedtext)
                return;

            this.ClearExtra(positions[row].printed, Text.Length, row);
            this.GoTopLeft(row);

            positions[row].printed = 0;
            positions[row].printedtext = "";
            this.WriteToConsole(Text, row, consoleColor);
        }

        public void ClearExtra(int lenOld, int lenNew, int row)
        {
            if (lenOld > lenNew)
            {
                int pLeft = lenNew;
                int pTop = this.positions[row].hTop;
                int lines;

                string pad = new String(' ', lenOld-lenNew);

                if( lenNew >= Console.BufferWidth)
                {
                    lines = (int)(lenNew / Console.BufferWidth);
                    pTop += lines;
                    pLeft = lenNew - (lines * Console.BufferWidth);
                }

                Console.SetCursorPosition(pLeft, pTop);
                Console.Write(pad);
            }
        }

        public void GoTopLeft(int row)
        {
            Console.SetCursorPosition(this.positions[row].hLeft, this.positions[row].hTop);
        }

        public void Clear(int row)
        {
            this.GoTopLeft(row);
            if (this.positions[row].printed > 0) {
                Console.WriteLine(new String(' ', this.positions[row].printed));
                this.positions[row].printed = 0;
                this.GoTopLeft(row);
            }
        }

        public void WriteError(string Text)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            this.hWriteToConsole(Text, 3, ConsoleColor.White);
        }

        public void WriteWarning(string Text)
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            this.hWriteToConsole(Text, 3, ConsoleColor.White);
        }

    }
}
