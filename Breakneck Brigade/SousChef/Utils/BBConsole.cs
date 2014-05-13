using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SousChef
{
    public class BBConsole
    {
        public ConsoleColor Color { get; set; }
        private string clrstr;
        private string _program;
        BBLock Lock = new BBLock();

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
            lock (Lock)
            {
                this.Program = program;
                this.Color = color;
                Prompt();
            }
        }
        public void Prompt()
        {
            lock (Lock)
            {
                ClearLine();
                Console.ForegroundColor = Color;
                Console.Write(Program + "> ");
                Console.ResetColor();
            }
        }
        public void ClearLine()
        {
            lock (Lock)
            {
                Console.Write(clrstr);
            }
        }
        public void WriteLine(string line)
        {
            lock (Lock)
            {
                ClearLine();
                Console.WriteLine(line);
                Prompt();
            }
        }

        public void BeginWrite()
        {
            Monitor.Enter(Lock);
            ClearLine();
        }
        public void Write(string text, ConsoleColor color)
        {
            Lock.AssertHeld();
            Console.ForegroundColor = color;
            Write(text);
            Console.ResetColor();
        }
        public void Write(string text)
        {
            Lock.AssertHeld();
            Console.Write(text);
        }
        public void EndWrite()
        {
            Prompt();
            Monitor.Exit(Lock);
        }
    }
}
