using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Simulator
{
    internal class Line
    {
        private int linenumber;
        private int instruction;
        private int codeline;
        private string readable;
        public Line(int linenumber,int instruction, int codeline, string readable)
        {
            this.linenumber = linenumber;
            this.instruction = instruction;
            this.codeline = codeline;  
            this.readable = readable;   
        }

        public string Readable
        {
            get { return readable; }
            set { readable = value; }
        }

        public int Linenumber
        {
            get { return linenumber; }
            set { linenumber = value; }
        }

        public int Instruction
        {
            get { return instruction; }
            set
                { linenumber = value; }
        }

        public int Codeline
        {
            get { return codeline; }
            set { codeline = value; }

        }

        
        
    }
}
