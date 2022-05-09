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
        public void checkDigitCarryFlag(short register, short value, string funcType)
        {
            ushort digitcarrymask = 0b_0000_1111;

            short regvalue = (short)(memory.memoryb1[register] & digitcarrymask);
            short maskedvalue = (short)(value & digitcarrymask);

            if(funcType == "add")
            {
                regvalue = (short)(regvalue + maskedvalue);

                if (regvalue > 15)
                {
                    memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0010);
                }
                else
                {
                    ushort digitcarryflagmask = 0b_0000_0010;
                    short digitcarrystatus = (short)(memory.memoryb1[Memory.STATUS] & digitcarryflagmask);
                    if (digitcarrystatus != 0b_0000_0000)
                    {
                        memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] - 0b_0000_0010);
                    }
                }
            }

            if(funcType == "sub")
            {
                regvalue = (short)(maskedvalue - regvalue);
                if (regvalue <= 15 && regvalue >= 0)
                {
                    memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0010);
                }
                else
                {
                    ushort digitcarryflagmask = 0b_0000_0010;
                    short digitcarrystatus = (short)(memory.memoryb1[Memory.STATUS] & digitcarryflagmask);
                    if (digitcarrystatus != 0b_0000_0000)
                    {
                        memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] - 0b_0000_0010);
                    }
                }
            }
            
        }

        public void checkCarryFlag(short register, string funcType)
        {
            ushort carryflagmask = 0b_0000_0001;
            short carrystatus = (short)(memory.memoryb1[Memory.STATUS] & carryflagmask);

            if (funcType == "add")
            {
                if ((short)(memory.memoryb1[register]) > 255)
                {
                    memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0001);
                }
                else
                {
                    if (carrystatus != 0b_0000_0000)
                    {
                        memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] - 0b_0000_0001);
                    }
                }
                memory.memoryb1[register] = (short)(memory.memoryb1[register] & 0b_0000_0000_1111_1111);
            }
            
            if(funcType == "sub")
            {
                if ((short)(memory.memoryb1[register]) >= 0)
                {
                    memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0001);
                }
                else
                {
                    if (carrystatus != 0b_0000_0000)
                    {
                        memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] - 0b_0000_0001);
                    }
                }
                memory.memoryb1[register] = (short)(memory.memoryb1[register] & 0b_0000_0000_1111_1111);
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
                case 0x08: //movf
                    movf(value);
                    break;
                case 0x39: //andlw
                    andlw(value);
                    break;
                case 0x05: //andwf
                    andwf(value);
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
                case 0x07: //addwf
                    addwf(value);
                    break;
                case 0x28: //goto
                    Goto(value);
                    break;
                case 0x20: //call
                    call(value);
                    break;
                case 0x34: //retlw
                    retlw(value);
                    break;
                case 0x09: //comf
                    comf(value);
                    break;
                case 0x03: //decf
                    decf(value);
                    break;
                case 0x0A: //incf
                    incf(value);
                    break;
                case 0x04: //iorwf
                    iorwf(value);
                    break;
                case 0x02: //subwf
                    subwf(value);
                    break;
                case 0x0E: //swapf
                    swapf(value);
                    break;
                case 0x06: //xorwf
                    xorwf(value);
                    break;
                case 0x01:
                    if (value != 0) //clrf
                    {
                        clrf(value);
                    }
                    else //clrw
                    {
                        clrw();
                    }
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
                            movwf(value);
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

        public void movwf(short value)
        {
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue == 0b_1000_0000)
            {
                memory.memoryb1[freg] = memory.memoryb1[Memory.W];
            }

            memory.updateMemView();
        }

        public void movf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            memory.memoryb1[destreg] = memory.memoryb1[freg];

            checkZeroFlag(destreg);
            memory.updateMemView();
        }
        public void addlw(short value)
        {
            checkDigitCarryFlag(Memory.W, value, "add");

            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] + value);

            checkCarryFlag(Memory.W, "add");
            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void addwf(short value)
        {
            short destreg;
            short freg =        (short)(value & 0b_0111_1111);
            short destvalue =   (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            checkDigitCarryFlag(destreg, memory.memoryb1[freg], "add");

            memory.memoryb1[destreg] = (short)(memory.memoryb1[Memory.W] + memory.memoryb1[freg]);

            checkCarryFlag(destreg, "add");
            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void andlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] & value);

            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void andwf(short value)
        {
            short destreg;
            short freg =        (short)(value & 0b_0111_1111);
            short destvalue =   (short)(value & 0b_1000_0000);

            if( destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            memory.memoryb1[destreg] = (short)(memory.memoryb1[Memory.W] & memory.memoryb1[freg]);


            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void iorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short) (memory.memoryb1[Memory.W] | value);
          
            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void iorwf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            memory.memoryb1[destreg] = (short) (memory.memoryb1[Memory.W] | memory.memoryb1[freg]);

            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void sublw(short value)
        {   
            checkDigitCarryFlag(Memory.W, value, "sub");
            
            short regvalue = (short)(value - memory.memoryb1[Memory.W]);
            memory.memoryb1[Memory.W] = regvalue;
            checkCarryFlag(Memory.W, "sub");
            
            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void subwf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            checkDigitCarryFlag(Memory.W, memory.memoryb1[freg], "sub");

            short regvalue = (short)(memory.memoryb1[freg] - memory.memoryb1[Memory.W]);
            memory.memoryb1[destreg] = regvalue;
            checkCarryFlag(destreg, "sub");
            
            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void xorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] ^ value);

            checkZeroFlag(Memory.W);
            memory.updateMemView();   
        }

        public void xorwf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            memory.memoryb1[destreg] = (short)(memory.memoryb1[Memory.W] ^ memory.memoryb1[freg]);

            checkZeroFlag(destreg);
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
       
        public void clrf(short value)
        {
            short freg = (short)(value & 0b_0111_1111);

            memory.memoryb1[freg] = 0b_0000_0000;

            checkZeroFlag(freg);
            memory.updateMemView();
        }

        public void clrw()
        {
            memory.memoryb1[Memory.W] = 0b_0000_0000;

            checkZeroFlag(Memory.W);
            memory.updateMemView();
        }

        public void comf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            memory.memoryb1[destreg] = (short)(0xFF - memory.memoryb1[freg]);

            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void decf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            memory.memoryb1[destreg] = (short)((memory.memoryb1[freg] - 1) & 0b_0000_0000_1111_1111);
            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void incf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            memory.memoryb1[destreg] = (short)(memory.memoryb1[freg] + 1);

            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void swapf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            short regvalue = memory.memoryb1[destreg];
            short lowerregvalue = (short) (regvalue & 0b_0000_1111);

            memory.memoryb1[destreg] = (short) ((regvalue >> 4) + (lowerregvalue << 4));
            memory.updateMemView();
        }
    }
}

interface ICodeInterface
{
    void selectCode(int line);
}