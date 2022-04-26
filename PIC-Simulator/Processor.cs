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

        /// <summary>
        /// Checks Zero Flag
        /// Sets zero flag if register value is zero
        /// Unsets zero flag if register value is not zero but zero flag is set
        /// </summary>
        /// <param name="register">Register which is checked</param>
       public void checkZeroFlag(short register)
        {
            if (memory.memoryb1[register] == 0)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0100);
            }
            else
            {
                ushort zeroflagmask = 0b_0000_0100;
                short zerostatus = (short)(memory.memoryb1[Memory.STATUS] & zeroflagmask);

                if(zerostatus != 0b_0000_0000)
                {
                    memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] - 0b_0000_0100);
                }
            }
        }

        /// <summary>
        /// Checks Digit Carry Flag
        /// Sets digit carry flag if bit 4 overflows
        /// Unsets digit carry flag if there is no overflow
        /// </summary>
        /// <param name="register">Register which is checked</param>
        /// <param name="value">Value for calculation</param>
        public void checkDigitCarryFlag(short register, short value)
        {
            ushort digitcarrymask = 0b_0000_1111;

            short reg = (short)(memory.memoryb1[register] & digitcarrymask);
            short maskedvalue = (short)(value & digitcarrymask);

            reg = (short)(reg + maskedvalue);
            if (reg > 15)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0010);
            }
            else
            {
                ushort digitcarryflagmask = 0b_0000_0010;
                short digitcarrystatus = (short)(memory.memoryb1[Memory.STATUS] & digitcarryflagmask);
                if(digitcarrystatus != 0b_0000_0000)
                {
                    memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] - 0b_0000_0010);
                }
            }
        }

        public void checkCarryFlag(short register)
        {
            if (memory.memoryb1[register] > 255)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0001);
                memory.memoryb1[register] = (short)(memory.memoryb1[register] & 0b_0000_0000_1111_1111);
            }
            else
            {
                ushort carryflagmask = 0b_0000_0001;
                short carrystatus = (short)(memory.memoryb1[Memory.STATUS] & carryflagmask);
                if (carrystatus != 0b_0000_0000)
                {
                    memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] - 0b_0000_0001);
                }
            }
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
            memory.memoryb1[Memory.W] = value;
            
            memory.updateMemView(); 
        }

        public void addlw(short value)
        {
            checkDigitCarryFlag(Memory.W, value);

            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] + value);

            checkCarryFlag(Memory.W);
            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void andlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] & value);

            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void iorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short) (memory.memoryb1[Memory.W] | value);

            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void sublw(short value)
        {
            checkDigitCarryFlag(Memory.W ,value);

            short wreg = (short)(memory.memoryb1[Memory.W] - value);
            if (wreg < 0)
            {
                memory.memoryb1[Memory.W] = (short) ((short)(wreg ^ 0b_1111_1111_1111_1111) + 1);
            }

            checkCarryFlag(Memory.W);
            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void xorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] ^ value);

            checkZeroFlag(Memory.W);
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
            //MISSING Call mit Rücksprung stack[stackpointer]
        }

        public void Return()
        {
            //MISSING Return from subfunction
        }

        public void Goto(short value)
        {
            memory.Pcl = value;
        }
    }
}
