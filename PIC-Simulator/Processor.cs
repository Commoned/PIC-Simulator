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
                    andlw();
                    break;
                case 0x28:
                    Goto(value);
                    break;
            }
                
        }

        public void movlw(short value)
        {

            memory.memoryb1[0x10] = value;
            
            memory.Memoryb1 = null; // STUPID BESSERE LÖSUNG SUCHEN
            
        }

        public void addlw(short value)
        {
            memory.memoryb1[0x10] = (short)(memory.memoryb1[0x10] + value);
            if(memory.memoryb1[0x10] > 255)
            {
                memory.memoryb1[Memory.STATUS] = (short)(memory.memoryb1[Memory.STATUS] + 0b_0000_0010);
                memory.memoryb1[0x10] = 255;
            }
            memory.Memoryb1 = null;
        }

        public void andlw()
        {

        }

        public void iorlw()
        {

        }

        public void sublw()
        {

        }

        public void xorlw()
        {

        }

        public void Goto(short value)
        {
            memory.Pcl = value;
        }
    }
}
