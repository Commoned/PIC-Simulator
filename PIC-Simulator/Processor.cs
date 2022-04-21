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
        string test =
@"
                    00001           ;TPicSim1
                    00002           ;Programm zum Test des 16F84-Simulators.
                    00003           ;Es werden alle Literal-Befehle geprüft
                    00004           ;(c) St. Lehmann
                    00005           ;Ersterstellung: 23.03.2016
                    00006           ;19.05.2020
                    00007           ;mod. 18.10.2018 Version HSO
                    00008           ;
                    00009           list c=132          ;Zeilenlänge im LST auf 132 Zeichen setzen
                    00010           
                    00011           
                    00012           ;Definition des Prozessors
                    00013           device 16F84
                    00014             
                    00015           ;Festlegen des Codebeginns
                    00016           org 0
                    00017  start    
0000 3011           00018           movlw 11h           ;in W steht nun 11h, Statusreg. unverändert
0001 3930           00019           andlw 30h           ;W = 10h, C=x, DC=x, Z=0
0002 380D           00020           iorlw 0Dh           ;W = 1Dh, C=x, DC=x, Z=0
0003 3C3D           00021           sublw 3Dh           ;W = 20h, C=1, DC=1, Z=0
0004 3A20           00022           xorlw 20h           ;W = 00h, C=1, DC=1, Z=1
0005 3E25           00023           addlw 25h           ;W = 25h, C=0, DC=0, Z=0
                    00024             
                    00025           
                    00026  ende     
0006 2806           00027           goto ende           ;Endlosschleife, verhindert Nirwana
                    00028           
                    00029             

";
        
            
            
            
        public Processor()
        {
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
                    var splits = beginnums.Split(' ');
                    int intValue = int.Parse(splits[1], System.Globalization.NumberStyles.HexNumber);
                    lines.Add(new Line(num,Convert.ToInt16(splits[0]),intValue,strings[1]));
                }
                
            }

        }


    }
}
