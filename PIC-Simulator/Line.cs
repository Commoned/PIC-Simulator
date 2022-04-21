using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Simulator
{
    internal class Line
    {
        private short linenumber;
        public short instruction;
        public short codeline;
        private string readable;
        public Line(short linenumber,short codeline, short instruction, string readable)
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

        public short Linenumber
        {
            get { return linenumber; }
            set { linenumber = value; }
        }

        public string Instruction
        {
            get {
                
                return Convert.ToString(this.instruction,16).ToUpper(); 
            }
            set { linenumber = Convert.ToInt16(value); }
        }

        public short Codeline
        {
            get { return codeline; }
            set { codeline = value; }

        }

        
        
    }
}
