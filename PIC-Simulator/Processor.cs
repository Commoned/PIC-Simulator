using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PIC_Simulator
{
    internal class Processor
    {
        public List<Line> lines = new List<Line>();
        public List<Line> runlines = new List<Line>();
        private Memory memory;
        public bool isRunning = false;
        public string test="";
        
            
            
            
        public Processor(Memory memory)
        {
            short intValue = 0;
            string[] splits = { };
            var test2 = test.Split('\n');
            foreach(string s in test2)
            {
                short num;
                try
                {
                    num = Convert.ToInt16(Regex.Match(s, "\\s[0-9]{5}").Value.Trim());
                }
                catch { continue; }

                var strings = Regex.Split(s, "[0-9]{5}");


                Regex.Match(s,"");
                if (s.StartsWith(' '))
                {
                    lines.Add(new Line(num,0,0, strings[1]));
                }
                else
                {
                    var beginnums = Regex.Match(s,"[0-9A-F]{4}\\s[0-9A-F]{4}").Value;
                    splits = beginnums.Split(' ');
                    intValue = short.Parse(splits[1], System.Globalization.NumberStyles.HexNumber);
                    lines.Add(new Line(num,Convert.ToInt16(splits[0]),intValue,strings[1]));
                }
                if(intValue != 0)
                {
                    runlines.Add(new Line(num, Convert.ToInt16(splits[0]), intValue, strings[1]));
                }
            }
            this.memory = memory;
        }

        public void Run()
        {
            var lastinst = runlines.Last();

            for(memory.Pcl = 0; memory.Pcl<= lastinst.codeline; memory.Pcl++)
            {
                var line = runlines[memory.Pcl];
                if (line.instruction != 0)
                {
                    Decode(line.instruction);
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
                    andlw();
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
    }
}
