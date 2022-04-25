﻿using System;
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

       public void checkZeroFlag(short register)
        {
            if (memory.memoryb1[register] == 0)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0100);
            }
        }

        public void checkDigitCarryFlag(short register, short value)
        {
            ushort digitcarrymask = 0b_0000_1111;

            short wreg = (short)(memory.memoryb1[register] & digitcarrymask);
            short maskedvalue = (short)(value & digitcarrymask);

            wreg = (short)(wreg + maskedvalue);
            if (wreg > 15)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0010);
            }
        }

        public void checkCarryFlag(short register)
        {
            if (memory.memoryb1[register] > 255)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0001);
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
            checkDigitCarryFlag(memory.memoryb1[Memory.W], value);

            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] + value);

            checkCarryFlag(memory.memoryb1[Memory.W]);
            checkZeroFlag(memory.memoryb1[Memory.W]);
            memory.updateMemView();
        }

        public void andlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] & value);

            checkZeroFlag(memory.memoryb1[Memory.W]);
            memory.updateMemView();
        }

        public void iorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short) (memory.memoryb1[Memory.W] | value);

            checkZeroFlag(memory.memoryb1[Memory.W]);
            memory.updateMemView();
        }

        public void sublw(short value)
        {
            checkDigitCarryFlag(memory.memoryb1[Memory.W] ,value);

            memory.memoryb1[Memory.W] = (short) (memory.memoryb1[Memory.W] - value);

            checkCarryFlag(memory.memoryb1[Memory.W]);
            checkZeroFlag(memory.memoryb1[Memory.W]);
            //MISSING 2nd's complement
            memory.updateMemView();
        }

        public void xorlw(short value)
        {
            memory.memoryb1[Memory.W] = (short)(memory.memoryb1[Memory.W] ^ value);

            checkZeroFlag(memory.memoryb1[Memory.W]);
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
            //MISSING Return from subfunction
        }

        public void Goto(short value)
        {
            memory.Pcl = value;
        }
    }
}
