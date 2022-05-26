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
        bool isSkip;
        public List<int> brkpnts = new List<int>();
        short currentBank = 0;




        public Processor(ICodeInterface codeInterface,Memory memory)
        {
            this.codeInterface = codeInterface;
            Clock.Tick += Clock_Tick;
            
            this.Clock.Interval = new TimeSpan(quartz);
            
            this.memory = memory;
        }

        public void Clock_Tick(object sender, object e)
        {
            codeInterface.selectCode(runlines[memory.programmcounter].Linenumber - 1);
            codeInterface.portTrigger(memory.memoryb1[1, Memory.TRISA], memory.memoryb1[1, Memory.TRISB]);
            if(brkpnts.Contains(runlines[memory.programmcounter].Linenumber - 1))
            {
                Clock.Stop();
                
                return;
            }
            Step();
            
        }

        public void mirrorRegs()
        {
            if(currentBank == 0)
            {
                memory.memoryb1[1, Memory.STATUS] = memory.memoryb1[0, Memory.STATUS];
            }
            else
            {
                memory.memoryb1[0, Memory.STATUS] = memory.memoryb1[1, Memory.STATUS];
            }
        }

        public void Step()
        {
            mirrorRegs();
            Line line = runlines[memory.programmcounter];
            
            //memory.Pcl++;
            memory.increasePc();

            if((memory.memoryb1[0, Memory.STATUS] & 0b_0100000) == 0b_100000)
            {
                currentBank = 1;
            }
            else
            {
                currentBank = 0;
            }

            memory.commandcounter++;

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
            if (memory.memoryb1[currentBank,register] == 0)
            {
                memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] | 0b_0000_0100);
            }
            else
            {
                ushort zeroflagmask = 0b_0000_0100;
                short zerostatus = (short)(memory.memoryb1[currentBank,Memory.STATUS] & zeroflagmask);

                if(zerostatus != 0b_0000_0000)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_1111_1011);
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

            short regvalue = (short)(memory.memoryb1[currentBank,register] & digitcarrymask);
            short maskedvalue = (short)(value & digitcarrymask);

            if(funcType == "add")
            {
                regvalue = (short)(regvalue + maskedvalue);

                if (regvalue > 15)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] | 0b_0000_0010);
                }
                else
                {
                    ushort digitcarryflagmask = 0b_0000_0010;
                    short digitcarrystatus = (short)(memory.memoryb1[currentBank,Memory.STATUS] & digitcarryflagmask);
                    if (digitcarrystatus != 0b_0000_0000)
                    {
                        memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_1111_1101);
                    }
                }
            }

            if(funcType == "sub")
            {
                regvalue = (short)(maskedvalue - regvalue);
                if (regvalue <= 15 && regvalue >= 0)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] | 0b_0000_0010);
                }
                else
                {
                    ushort digitcarryflagmask = 0b_0000_0010;
                    short digitcarrystatus = (short)(memory.memoryb1[currentBank,Memory.STATUS] & digitcarryflagmask);
                    if (digitcarrystatus != 0b_0000_0000)
                    {
                        memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_1111_1101);
                    }
                }
            }
            
        }

        public void checkCarryFlag(short register, string funcType)
        {
            ushort carryflagmask = 0b_0000_0001;
            short carrystatus = (short)(memory.memoryb1[currentBank,Memory.STATUS] & carryflagmask);

            if (funcType == "add")
            {
                if ((short)(memory.memoryb1[currentBank,register]) > 255 && carrystatus == 0b_0000_0000)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] | 0b_0000_0001);
                }
                if ((short) (memory.memoryb1[currentBank,register]) <=255 && carrystatus == 0b_0000_0001)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_1111_1110);
                }

                memory.memoryb1[currentBank,register] = (short)(memory.memoryb1[currentBank,register] & 0b_0000_0000_1111_1111);
            }
            
            if (funcType == "sub")
            {
                if ((short)(memory.memoryb1[currentBank,register]) >= 0 && carrystatus == 0b_0000_0000)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] | 0b_0000_0001);
                }
                if((short)(memory.memoryb1[currentBank,register]) < 0 && carrystatus == 0b_0000_0001)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_1111_1110);
                }

                memory.memoryb1[currentBank,register] = (short)(memory.memoryb1[currentBank,register] & 0b_0000_0000_1111_1111);
            } 

            if (funcType == "rl")
            {
                short regcarry = (short)(memory.memoryb1[currentBank,register] & 0b_0000_0001_0000_0000);
                short reglowbit = (short)(memory.memoryb1[currentBank,register] & 0b_0000_0000_0000_0001);
                
                if(reglowbit == 0b_0000_0000_0000_0000 && carrystatus == 0b_0000_0001)
                {
                    memory.memoryb1[currentBank,register] = (short)(memory.memoryb1[currentBank,register] | 0b_0000_0001);
                }
                if(regcarry == 0b_0000_0001_0000_0000 && carrystatus == 0b_0000_0000)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] | 0b_0000_0001);
                }
                if(regcarry != 0b_0000_0001_0000_0000 && carrystatus == 0b_0000_0001)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_1111_1110);
                }
                
                memory.memoryb1[currentBank,register] = (short)(memory.memoryb1[currentBank,register] & 0b_0000_0000_1111_1111);
            }

            if (funcType == "rr")
            {
                short reglowbit = (short)(memory.memoryb1[currentBank,register] & 0b_0000_0000_0000_0001);

                if (reglowbit == 0b_0000_0000_0000_0001 && carrystatus == 0b_0000_0000)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] | 0b_0000_0001);
                }
                if (reglowbit == 0b_0000_0000_0000_0000 && carrystatus == 0b_0000_0001)
                {
                    memory.memoryb1[currentBank,Memory.STATUS] = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_1111_1110);
                }

                memory.memoryb1[currentBank,register] = (short)(memory.memoryb1[currentBank,register] & 0b_0000_0000_1111_1111);
            }
        }
        /// <summary>
        /// handles Data Latch for PORTA and PORTB manipulation
        /// </summary>
        /// <param name="bank"></param> 0 or 1 
        /// <param name="destination"></param> detination Register (PORTA or PORTB / TRISA or TRISB)
        /// <param name="value"></param> value to insert to Register
        /// <returns></returns>
        public short handleDataLatch(short bank,short destination, short value)
        {
            char[] bitmask;
            char[] valarray = Convert.ToString(value,2).PadLeft(16,'0').ToCharArray();
            char[] returnval = new char[16] {'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', };
            char[] trisval = new char[16] { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', };
            if (bank == 0 && destination == Memory.PORTA)
            {
                bitmask = Convert.ToString(memory.memoryb1[1, Memory.TRISA],2).PadLeft(16, '0').ToCharArray();
                for(int i = bitmask.Length-1; i>=0; i--)
                {
                    if(bitmask[i] == '0')
                    {
                        returnval[i] = valarray[i];
                    }
                    else
                    {
                        trisval[i] = valarray[i];
                    }
                }
                memory.trisaLatch = (short)(memory.trisaLatch | short.Parse(string.Join("", trisval)));
                return short.Parse(string.Join("",returnval));
            }
            if (bank == 0 && destination == Memory.PORTB)
            {
                bitmask = Convert.ToString(memory.memoryb1[1, Memory.TRISB],2).PadLeft(16, '0').ToCharArray();
                for (int i = bitmask.Length - 1; i >= 0; i--)
                {
                    if (bitmask[i] == '0')
                    {
                        returnval[i] = valarray[i];
                    }
                    else
                    {
                        trisval[i] = valarray[i];
                    }
                }
                memory.trisbLatch = (short)(memory.trisbLatch | short.Parse(string.Join("", trisval)));
                return short.Parse(string.Join("", returnval));
            }

            if(bank == 1 && destination == Memory.TRISA)
            {
                bitmask = Convert.ToString(value, 2).PadLeft(16, '0').ToCharArray();
                trisval = Convert.ToString(memory.trisaLatch, 2).PadLeft(16, '0').ToCharArray();
                returnval = Convert.ToString(memory.memoryb1[0,Memory.PORTA],2).PadLeft(16, '0').ToCharArray();
                for (int i = bitmask.Length - 1; i >= 0; i--)
                {
                    if (bitmask[i] == '0')
                    {
                        returnval[i] = trisval[i];
                    }
                }
                string test = string.Join("", returnval);
                memory.memoryb1[0,Memory.PORTA] = (short)(int.Parse(string.Join("", returnval)));
            }
            if (bank == 1 && destination == Memory.TRISB)
            {
                bitmask = Convert.ToString(value,2).PadLeft(16, '0').ToCharArray();
                trisval = Convert.ToString(memory.trisbLatch,2).PadLeft(16, '0').ToCharArray();
                returnval = Convert.ToString(memory.memoryb1[0, Memory.PORTB], 2).PadLeft(16, '0').ToCharArray();
                for (int i = bitmask.Length - 1; i >= 0; i--)
                {
                    if (bitmask[i] == '0')
                    {
                        returnval[i] = trisval[i];
                    }
                }
                string test = string.Join("", returnval);
                memory.memoryb1[0, Memory.PORTB] = (short)(int.Parse(string.Join("", returnval)));
            }
            return 0;
        }

        public short checkIndirect(short value)
        {
            short regval = 0;
            if ((value & 0b_01111111) == 0) // Wenn 0 dann indirekte Adressierung
            {
                regval = memory.memoryb1[currentBank,Memory.FSR];

                if ((value & 0b_10000000) != 0)
                {
                    regval = (short)(regval | 0b_10000000);
                }
                else
                {
                    regval = (short)(regval & 0b_11111111);
                }
            }
            else
            {
                regval = value;
            }
            return regval;
        }

        public void Decode(short toDecode)
        {
            ushort masklower = 0b_1111_1111;
            ushort maskhigher = 0xFF00;

            short instruction = (short)(toDecode & maskhigher);
            instruction = (short)(instruction >> 8);
            short value = (short)(toDecode & masklower) ;

            if(isSkip)
            {
                nop();
                isSkip = false;
                return;
            }            

            switch (instruction)
            {
                case 0x30: //movlw
                    movlw(value);
                    break;
                case 0x08: //movf
                    value = checkIndirect(value);
                    movf(value);
                    break;
                case 0x39: //andlw
                    andlw(value);
                    break;
                case 0x05: //andwf
                    value = checkIndirect(value);
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
                    value = checkIndirect(value);
                    addwf(value);
                    break;
                case 0x28: //goto
                case 0x29:
                case 0x2A:
                case 0x2B:
                case 0x2C:
                case 0x2D:
                case 0x2E:
                case 0x2F:
                    Goto(value, instruction);
                    break;
                case 0x20: //call
                    call(value, instruction);
                    break;
                case 0x34: //retlw
                    retlw(value);
                    break;
                case 0x09: //comf
                    value = checkIndirect(value);
                    comf(value);
                    break;
                case 0x03: //decf
                    value = checkIndirect(value);
                    decf(value);
                    break;
                case 0x0A: //incf
                    value = checkIndirect(value);
                    incf(value);
                    break;
                case 0x04: //iorwf
                    value = checkIndirect(value);
                    iorwf(value);
                    break;
                case 0x02: //subwf
                    value = checkIndirect(value);
                    subwf(value);
                    break;
                case 0x0E: //swapf
                    value = checkIndirect(value);
                    swapf(value);
                    break;
                case 0x06: //xorwf
                    value = checkIndirect(value);
                    xorwf(value);
                    break;
                case 0x0D: //rlf
                    value = checkIndirect(value);
                    rlf(value);
                    break;
                case 0x0c: //rrf
                    value = checkIndirect(value);
                    rrf(value);
                    break;
                case 0x0F: //incfsz
                    isSkip = incfsz(value);
                    break;
                case 0x0B: //decfsz
                    isSkip = decfsz(value);
                    break;  
                case 0x01:
                    if (value != 0) //clrf
                    {
                        value = checkIndirect(value);
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
                            value = checkIndirect(value);
                            movwf(value);
                            break;
                    }
                    break;
                default:
                    if ((instruction & 0b_000011100) == 0b_010100)
                    {
                        value = checkIndirect(value);
                        bsf(instruction, value);
                    }
                    if ((instruction & 0b_000011100) == 0b_010000)
                    {
                        value = checkIndirect(value);
                        bcf(instruction, value);
                    }
                    if((instruction & 0b_000011100) == 0b_011000)
                    {
                        value = checkIndirect(value);
                        isSkip = btfsc(instruction,value);
                    }
                    if ((instruction & 0b_000011100) == 0b_011100)
                    {
                        value = checkIndirect(value);
                        isSkip = btfss(instruction, value);
                    }
                    break;
            }
            

        }

        public void movlw(short value)
        {
            memory.memoryb1[0,Memory.W] = value;
            
            memory.updateMemView(); 
        }

        public void movwf(short value)
        {
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);
            
            if(currentBank == 0 && (freg == Memory.PORTA || freg == Memory.PORTB))
            {
                value = handleDataLatch(currentBank,freg, memory.memoryb1[0,Memory.W]);
            }
            if (currentBank == 1 && (freg == Memory.TRISA || freg == Memory.TRISB))
            {
                _ = handleDataLatch(currentBank,freg, memory.memoryb1[0,Memory.W]);
            }

            if (destvalue == 0b_1000_0000)
            {
                memory.memoryb1[currentBank,freg] = (short)(memory.memoryb1[0,Memory.W] );
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

            if (currentBank == 0 && (freg == Memory.PORTA || freg == Memory.PORTB))
            {
                value = handleDataLatch(currentBank, destreg, memory.memoryb1[currentBank, freg]);
            }
            if (currentBank == 1 && (freg == Memory.TRISA || freg == Memory.TRISB))
            {
                _ = handleDataLatch(currentBank, destreg, memory.memoryb1[currentBank, freg]);
            }

            memory.memoryb1[currentBank,destreg] = memory.memoryb1[currentBank,freg];

            checkZeroFlag(destreg);
            memory.updateMemView();
        }
        
        public void addlw(short value)
        {
            checkDigitCarryFlag(Memory.W, value, "add");

            memory.memoryb1[0,Memory.W] = (short)(memory.memoryb1[0,Memory.W] + value);

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

            checkDigitCarryFlag(destreg, memory.memoryb1[currentBank,freg], "add");

            memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[0,Memory.W] + memory.memoryb1[currentBank,freg]);

            checkCarryFlag(destreg, "add");
            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void andlw(short value)
        {
            memory.memoryb1[0,Memory.W] = (short)(memory.memoryb1[0,Memory.W] & value);

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

            memory.memoryb1[currentBank, destreg] = (short)(memory.memoryb1[0, Memory.W] & memory.memoryb1[currentBank, freg]);

            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void iorlw(short value)
        {
            memory.memoryb1[0,Memory.W] = (short) (memory.memoryb1[0,Memory.W] | value);
          
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

            memory.memoryb1[currentBank,destreg] = (short) (memory.memoryb1[0,Memory.W] | memory.memoryb1[currentBank,freg]);

            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void sublw(short value)
        {   
            checkDigitCarryFlag(Memory.W, value, "sub");
            
            short regvalue = (short)(value - memory.memoryb1[0,Memory.W]);
            memory.memoryb1[0,Memory.W] = regvalue;
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

            checkDigitCarryFlag(Memory.W, memory.memoryb1[currentBank,freg], "sub");

            short regvalue = (short)(memory.memoryb1[currentBank,freg] - memory.memoryb1[0,Memory.W]);
            memory.memoryb1[currentBank,destreg] = regvalue;
            checkCarryFlag(destreg, "sub");
            
            checkZeroFlag(destreg);
            memory.updateMemView();
        }

        public void xorlw(short value)
        {
            memory.memoryb1[0,Memory.W] = (short)(memory.memoryb1[0,Memory.W] ^ value);

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

            memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[0,Memory.W] ^ memory.memoryb1[currentBank,freg]);

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
            memory.memoryb1[0,Memory.W] = value;
            //memory.memoryb1[currentBank, Memory.PCL] = memory.pop();
            memory.setPc(memory.pop());
            nop();
            memory.updateMemView();
        }

        public void call(short value, short instruction)
        {
            short pc = (short)(value ^ ((short)(instruction & 0b_0000_0111) << 8));
            short pclath = (short)(memory.memoryb1[currentBank, Memory.PCLATH] & 0b_0001_1000);
            pc = (short)(pc ^ (pclath << 8));

            //memory.push((short)(pc));
            memory.push((short)(memory.memoryb1[currentBank, Memory.PCL]));
            //memory.Pcl = pc;
            memory.setPc(pc);
            nop();
            memory.updateMemView();
        }

        public void Return()
        {
            //memory.Pcl = memory.pop();
            memory.setPc(memory.pop());
            nop();
            memory.updateMemView();
        }

        public void Goto(short value, short instruction)
        {
            short pc = (short) (value ^ ((short)(instruction & 0b_0000_0111) << 8));
            short pclath = (short)(memory.memoryb1[currentBank, Memory.PCLATH] & 0b_0001_1000);
            pc = (short)(pc ^ (pclath << 8));

            //memory.Pcl = pc;
            memory.setPc(pc);
            nop();
            memory.updateMemView();
        }
       
        public void clrf(short value)
        {
            short freg = (short)(value & 0b_0111_1111);

            if (currentBank == 0 && (freg == Memory.PORTA || freg == Memory.PORTB))
            {
                value = handleDataLatch(currentBank, freg, 0);
            }
            if (currentBank == 1 && (freg == Memory.TRISA || freg == Memory.TRISB))
            {
                _ = handleDataLatch(currentBank, freg, 0);
            }

            memory.memoryb1[currentBank,freg] = 0b_0000_0000;

            checkZeroFlag(freg);
            memory.updateMemView();
        }

        public void clrw()
        {
            memory.memoryb1[0,Memory.W] = 0b_0000_0000;

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

            memory.memoryb1[currentBank,destreg] = (short)(0xFF - memory.memoryb1[currentBank,freg]);

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

            memory.memoryb1[currentBank,destreg] = (short)((memory.memoryb1[currentBank,freg] - 1) & 0b_0000_0000_1111_1111);
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

            memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,freg] + 1);

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

            short regvalue = memory.memoryb1[currentBank,destreg];
            short lowerregvalue = (short) (regvalue & 0b_0000_1111);

            memory.memoryb1[currentBank,destreg] = (short) ((regvalue >> 4) + (lowerregvalue << 4));
            memory.updateMemView();
        }

        public void rlf(short value)
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

            memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,freg] << 1);

            checkCarryFlag(destreg, "rl");
            memory.updateMemView();
        }

        public void rrf(short value)
        {
            short destreg;
            short freg = (short)(value & 0b_0111_1111);
            short destvalue = (short)(value & 0b_1000_0000);
            short startingcarryvalue = (short)(memory.memoryb1[currentBank,Memory.STATUS] & 0b_0000_0001);

            if (destvalue != 0b_1000_0000)
            {
                destreg = Memory.W;
            }
            else
            {
                destreg = freg;
            }

            checkCarryFlag(freg, "rr");
            memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,freg] >> 1);
            if(startingcarryvalue == 0b_0000_0001)
            {
                memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] + 0b_1000_0000);
            }

            memory.updateMemView();
        }
    
        public void bsf(short instruction,short value)
        {
            short destreg = (short)(value & 0b_0111_1111);
            short bitdestvalue = (short)(value & 0b_1000_0000);
            
            if (bitdestvalue != 0)
            {
                bitdestvalue = (short)(1 + ((instruction & 0b_0011) << 1));
            }
            else
            {
                bitdestvalue = (short)((instruction & 0b_0011) << 1);
            }
            if(destreg == Memory.PORTA) // Für DATA LATCH
            {
                if(memory.checkBit(memory.memoryb1[1,Memory.TRISA],bitdestvalue))
                {
                    memory.trisaLatch = memory.setBit(memory.trisaLatch, bitdestvalue);
                    return;
                }
            }
            if (destreg == Memory.PORTB)
            {
                if (memory.checkBit(memory.memoryb1[1, Memory.TRISB], bitdestvalue))
                {
                    memory.trisbLatch = memory.setBit(memory.trisbLatch, bitdestvalue);
                    return;
                }
            }

            switch (bitdestvalue)
            {
                case 0:
                   memory.memoryb1[currentBank, destreg] = (short)(memory.memoryb1[currentBank, destreg] | 0b_01);
                    break;
                case 1:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] | 0b_010);
                    break;
                case 2:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] | 0b_100);
                    break;
                case 3:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] | 0b_1000);
                    break;
                case 4:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] | 0b_10000);
                    break;
                case 5:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] | 0b_100000);
                    break;
                case 6:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] | 0b_1000000);
                    break;
                case 7:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] | 0b_10000000);
                    break;
            }
            memory.updateMemView();
        }

        public void bcf(short instruction, short value)
        {
            short destreg = (short)(value & 0b_0111_1111);
            short bitdestvalue = (short)(value & 0b_1000_0000);
            if (bitdestvalue != 0)
            {
                bitdestvalue = (short)(1 + ((instruction & 0b_0011) << 1));
            }
            else
            {
                bitdestvalue = (short)((instruction & 0b_0011) << 1);
            }
            
            if (destreg == Memory.PORTA) // Für DATA LATCH
            {
                if (currentBank == 1) // zieht aus trisaLatch, da PORT Pin auf ausgang gesetzt wird
                {
                    if (memory.checkBit(memory.trisaLatch, bitdestvalue))
                    {
                        memory.trisaLatch = memory.setBit(memory.memoryb1[memory.trisaLatch, Memory.PORTA], bitdestvalue);
                    }
                    else
                    {
                        memory.trisaLatch = memory.clrBit(memory.memoryb1[memory.trisaLatch, Memory.PORTA], bitdestvalue);
                    }
                }
                else
                {
                    if (memory.checkBit(memory.memoryb1[1, Memory.TRISA], bitdestvalue))
                    {
                        memory.trisaLatch = memory.clrBit(memory.trisaLatch, bitdestvalue);
                        return;
                    }
                }
            }
            if (destreg == Memory.PORTB)
            {
                if (currentBank == 1)
                {
                    if (memory.checkBit(memory.trisbLatch, bitdestvalue))
                    {
                        memory.trisbLatch = memory.setBit(memory.memoryb1[memory.trisbLatch, Memory.PORTB], bitdestvalue);
                    }
                    else
                    {
                        memory.trisbLatch = memory.clrBit(memory.memoryb1[memory.trisbLatch, Memory.PORTB], bitdestvalue);
                    }
                }
                else
                {
                    if (memory.checkBit(memory.memoryb1[1, Memory.TRISB], bitdestvalue))
                    {
                        memory.trisbLatch = memory.clrBit(memory.trisbLatch, bitdestvalue);
                        return;
                    }
                }
            }
            

            switch (bitdestvalue)
            {
                case 0:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_11111110);
                    break;
                case 1:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_11111101);
                    break;
                case 2:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_11111011);
                    break;
                case 3:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_11110111);
                    break;
                case 4:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_11101111);
                    break;
                case 5:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_11011111);
                    break;
                case 6:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_10111111);
                    break;
                case 7:
                    memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,destreg] & 0b_101111111);
                    break;

            }
            mirrorRegs();
            memory.updateMemView();
        }

        public bool btfsc(short instruction, short value)
        {
            short destreg = (short)(value & 0b_0111_1111);
            short bitdestvalue = (short)(value & 0b_1000_0000);
            if (bitdestvalue != 0)
            {
                bitdestvalue = (short)(1 + ((instruction & 0b_0011) << 1));
            }
            else
            {
                bitdestvalue = (short)((instruction & 0b_0011) << 1);
            }

            short valtest = 0;

            switch (bitdestvalue)
            {
                case 0:
                   valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_01);
                    break;
                case 1:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_010);
                    break;
                case 2:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_100);
                    break;
                case 3:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_1000);
                    break;
                case 4:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_10000);
                    break;
                case 5:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_100000);
                    break;
                case 6:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_1000000);
                    break;
                case 7:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_10000000);
                    break;
            }
            if(valtest == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool btfss(short instruction, short value)
        {
            short destreg = (short)(value & 0b_0111_1111);
            short bitdestvalue = (short)(value & 0b_1000_0000);
            if (bitdestvalue != 0)
            {
                bitdestvalue = (short)(1 + ((instruction & 0b_0011) << 1));
            }
            else
            {
                bitdestvalue = (short)((instruction & 0b_0011) << 1);
            }

            short valtest = 0;

            switch (bitdestvalue)
            {
                case 0:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_01);
                    break;
                case 1:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_010);
                    break;
                case 2:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_100);
                    break;
                case 3:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_1000);
                    break;
                case 4:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_10000);
                    break;
                case 5:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_100000);
                    break;
                case 6:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_1000000);
                    break;
                case 7:
                    valtest = (short)(memory.memoryb1[currentBank,destreg] & 0b_10000000);
                    break;
            }
            if (valtest == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool decfsz(short value)
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

            memory.memoryb1[currentBank,destreg] = (short)((memory.memoryb1[currentBank,freg] - 1) & 0b_0000_0000_1111_1111);
            memory.updateMemView();

            if (memory.memoryb1[currentBank,destreg] == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool incfsz(short value)
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
            if (memory.memoryb1[currentBank, destreg] == 0)
            {
                return true;
            }

            if ((short)(memory.memoryb1[currentBank,freg] + 1) <= 0b_1111_1111)
            {
                memory.memoryb1[currentBank,destreg] = (short)(memory.memoryb1[currentBank,freg] + 1);
            }
            
            if ((short)(memory.memoryb1[currentBank,freg] + 1) > 0b_1111_1111)
            {
                memory.memoryb1[currentBank,destreg] = (short)(0b_0000_0000);
                
            }
            memory.updateMemView();
            return false;

            
        }
    }
}

interface ICodeInterface
{
    void selectCode(int line);
    void portTrigger(short trisa, short trisb);
}