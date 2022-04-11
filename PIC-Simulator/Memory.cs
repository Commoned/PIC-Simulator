using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Simulator
{
    
    internal class Memory
    {
        public static byte PORTA = 0x05;
        public static byte PORTB = 0x06;
        public static byte TRISA = 0x05;
        public static byte TRISB = 0x06;
        public static byte FSR = 0x04;
        public static byte TMR0 = 0x01;
        public static byte OPTION = 0x01;
        public static byte PCL = 0x02;
        public static byte STATUS = 0x03;
        public static byte EEDATA = 0x08;
        public static byte EEADR = 0x09;
        public static byte PCLATH = 0xA;
        public static byte INTCON = 0x0B;
        
        public short[] memory = new short[1024];
        public short stackpointer = 7;
        public short[] stack = new short[7];

        public Memory()
        {
            initMem();
        }

        public void initMem()
        {
            memory[2] = 0;
            memory[3] = 12;
            memory[10] = 0;
            memory[11] = 0;
            memory[0x81] = 0xFF;
            memory[0x83] = 0x18;
            memory[0x85] = 0x1F;
            memory[0x86] = 0xFF;
            memory[0x88] = 0x0;
            memory[0x8A] = 0x0;
            memory[0x8B] = 0x0;
        }

        public void resetMem()
        {
            initMem();
            // To be continued...
        }
    }


}
