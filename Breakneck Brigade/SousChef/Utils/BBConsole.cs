using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class BBConsole
    {
        public ConsoleColor Color { get; set; }
        private string clrstr;
        private string _program;
        public string Program
        {
            get
            {
                return _program;
            }
            set
            {
                _program = value;
                clrstr = "\r" + new string(' ', Program.Length + 2) + "\r";
            }
        }
        public BBConsole(string program, ConsoleColor color)
        {
            this.Program = program;
            this.Color = color;
            Prompt();
        }
        public void Prompt()
        {
            Console.ForegroundColor = Color;
            Console.Write(Program + "> ");
            Console.ResetColor();
        }
        public void ClearLine()
        {
            Console.Write(clrstr);
        }
        public void WriteLine(string line)
        {
            ClearLine();
            Console.WriteLine(line);
            Prompt();
        }
    }
}
