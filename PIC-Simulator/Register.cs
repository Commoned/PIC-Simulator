using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Simulator
{
    public class Register 
    {
        private string index;
        public short value;
        public Memory mem;

        public Register(int index, short value, Memory mem)
        {
            this.index = index.ToString();
            this.value = value;
            this.mem = mem;
        }
        

        public string Index
            {
                get => index;
            }
        public short Value
        {
            get {
                return value;
            }
            set => this.value = value;
        }
    }
}