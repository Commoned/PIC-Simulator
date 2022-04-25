using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;

namespace PIC_Simulator
{
    internal class Processor
    {
        public List<Line> lines = new List<Line>();
        public List<Line> runlines = new List<Line>();
        private Memory memory;
        public bool isRunning = false;

        public DispatcherTimer Clock = new DispatcherTimer();



        public Processor(Memory memory)
        {
            Clock.Tick += Clock_Tick;
            this.Clock.Interval = new TimeSpan(20);
            this.memory = memory;
        }

        private void Clock_Tick(object sender, object e)
        {
            Step();
        }

        public void Step()
        {
            Line line = runlines[memory.Pcl];
            memory.Pcl++;
            this.Decode(line.instruction);
            
        }

       

        public void Decode(short toDecode)
        {
            ushort masklower = 0b_1111_1111;
            ushort maskhigher = 0xFF00;

            short instruction = (short)(toDecode & maskhigher);
            instruction = (short)(instruction >> 8);
            short value = (short)(toDecode & masklower) ;

            switch (instruction)
            {
                case 0x30: //movlw
                    movlw(value);
                    break;
                case 0x39: //andlw
                    andlw(value);
                    break;
                case 0x38: //iorlw
                    iorlw(value);
                    break;
                case 0x3C: //sublw
                    sublw(value);
                    break;
                case 0x3A: //xorlw
                    xorlw(value);
                    break;
                case 0x3E: //addlw
                    addlw(value);
                    break;
                case 0x28: //goto
                    Goto(value);
                    break;
                case 0x20: //call
                    call(value);
                    break;
            }
                
        }

        public void movlw(short value)
        {

            memory.memoryb1[0x10] = value;
            
            memory.updateMemView(); 
            
        }

        public void addlw(short value)
        {
            memory.memoryb1[0x10] = (short)(memory.memoryb1[0x10] + value);
            //Handles digit carry flag
            if(memory.memoryb1[Memory.W] > 255)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0010);
                memory.memoryb1[Memory.W] = 255;
            }
            //Handles zero flag
            if (memory.memoryb1[Memory.W] == 0)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0100);
            }
            //MISSING Handler for carry flag
            memory.updateMemView();
        }

        public void andlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] & value);
            //Handles zero flag
            if(memory.memoryb1[Memory.W] == 0)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0100);
            }
            memory.updateMemView();
        }

        public void iorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short) (memory.memoryb1[Memory.W] | value);
            //Handles zero flag
            if(memory.memoryb1[Memory.W] == 0)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0100);
            }
            memory.updateMemView();
        }

        public void sublw(short value)
        {
            memory.memoryb1[Memory.W] = (short) (memory.memoryb1[Memory.W] - value);
            //Handles zero flag
            if (memory.memoryb1[Memory.W] == 0)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0100);
            }
            //MISSING 2nd's complement
            //MISSING Handler for digit carry flag
            //MISSING Handler for carry flag
            memory.updateMemView();
        }

        public void xorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] ^ value);
            //Handles zero flag
            if (memory.memoryb1[Memory.W] == 0)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0100);
            }
            memory.updateMemView();
            
        }

        public void nop()
        {
            //No operation
        }

        public void retlw(short value)
        {
            memory.memoryb1[Memory.W] = value;
            nop();
        }

        public void call(short value)
        {
            //MISSING Call mit Rücksprung
        }

        public void Return()
        {

        }

        public void Goto(short value)
        {
            memory.Pcl = value;
        }
    }
}
