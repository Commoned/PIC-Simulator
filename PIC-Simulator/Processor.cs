using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using System.Threading;

namespace PIC_Simulator
{
    internal class Processor
    {
        public List<Line> lines = new List<Line>();
        public List<Line> runlines = new List<Line>();
        private Memory memory;
        public bool isRunning = false;
        ICodeInterface codeInterface;
        public DispatcherTimer Clock = new DispatcherTimer();
        public int quartz = 20;



        public Processor(ICodeInterface codeInterface,Memory memory)
        {
            this.codeInterface = codeInterface;
            Clock.Tick += Clock_Tick;
            
            this.Clock.Interval = new TimeSpan(0,0,0,0,1);
            
            this.memory = memory;
        }

        public void Clock_Tick(object sender, object e)
        {
            codeInterface.selectCode(runlines[memory.Pcl].Linenumber -1);
            Step();
            
        }

        public void Step()
        {
            Line line = runlines[memory.Pcl];
            memory.Pcl++;
            this.Decode(line.instruction);
            line = null;
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
            if ((ushort)(memory.memoryb1[register]) > 255)
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
                case 0x34:
                    retlw(value);
                    break;
                case 0x00:
                    switch (value)
                    {
                        case 0x08:
                            Return();
                            break;
                        case 0x00:
                            nop();
                            break;
                        default:
                            //movwf(value);
                            break;
                    }
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
            memory.memoryb1[Memory.W] = wreg;
            checkCarryFlag(Memory.W);
            if (wreg < 0)
            {
                memory.memoryb1[Memory.W] = (short) ((short)(wreg ^ 0b_1111_1111_1111_1111) + 1);
            }

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
            memory.updateMemView();
            //No operation
        }

        public void retlw(short value)
        {
            memory.memoryb1[Memory.W] = value;
            memory.memoryb1[Memory.PCL] = memory.pop();
            nop();
            memory.updateMemView();
        }

        public void call(short value)
        {
            memory.push((short)(memory.memoryb1[Memory.PCL]));
            memory.Pcl = value;
            nop();
            memory.updateMemView();
            //MISSING Call mit Rücksprung stack[stackpointer]
        }

        public void Return()
        {
            memory.memoryb1[Memory.PCL] = memory.pop();
            nop();
            memory.updateMemView();
        }

        public void Goto(short value)
        {
            memory.Pcl = value;
            memory.updateMemView();
        }
    }
}

interface ICodeInterface
{
    void selectCode(int line);
}