using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Threading;



// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace PIC_Simulator
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page, ICodeInterface
    {
        Memory memory;
        Processor processor;
        private FileReader filereader;
        bool autoCheck;

        public MainPage()
        {
            this.DataContext = this;
            memory = new Memory();
            processor = new Processor(this, memory);
            filereader = new FileReader();
            DataContext = memory;

            this.InitializeComponent();
            CodeStack.ItemsSource = processor.lines;


        }





        private async void openButton_Click(object sender, RoutedEventArgs e)
        {
            processor.lines.Clear();
            processor.runlines.Clear();
            await filereader.GetLines();

            processor.lines = filereader.lines;
            CodeStack.ItemsSource = null;
            Thread.Sleep(200);
            CodeStack.ItemsSource = processor.lines;
            foreach (Line line in processor.lines)
            {
                if (line.executable)
                {
                    processor.runlines.Add(line);
                }
            }
            memory.initMem();

            Start_Button.IsEnabled = true;



        }

        public void selectCode(int line)
        {
            this.CodeStack.SelectedIndex = line;
            this.CodeStack.ScrollIntoView(this.CodeStack.SelectedItem,ScrollIntoViewAlignment.Leading);
        }

        public void portTrigger(short trisa, short trisb)
        {
            CheckBox[] raboxes = {ra0, ra1, ra2, ra3, ra4, ra5, ra6, ra7};
            CheckBox[] rbboxes = { rb0, rb1, rb2, rb3, rb4, rb5, rb6, rb7 };

            autoCheck = true;

            for (int i = 0; i < raboxes.Length; i++)
            {
                if(memory.checkBit(memory.memoryb1[1,Memory.TRISA],i))
                {
                    raboxes[i].IsEnabled = true;
                }
                else
                {
                    raboxes[i].IsEnabled=false;
                }
            }
            for (int i = 0; i < rbboxes.Length; i++)
            {
                if (memory.checkBit(memory.memoryb1[1, Memory.TRISB], i))
                {
                    rbboxes[i].IsEnabled = true;
                }
                else
                {
                    rbboxes[i].IsEnabled = false;
                }
            }
            for (int i = 0; i < raboxes.Length; i++)
            {
                if (memory.checkBit(memory.memoryb1[0, Memory.PORTA], i))
                {
                    raboxes[i].IsChecked = true;
                }
                else
                {
                    raboxes[i].IsChecked = false;
                }
            }
            for (int i = 0; i < rbboxes.Length; i++)
            {
                if (memory.checkBit(memory.memoryb1[0, Memory.PORTB], i))
                {
                    rbboxes[i].IsChecked = true;
                }
                else
                {
                    rbboxes[i].IsChecked = false;
                }
            }

            autoCheck = false;
        }



        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
        }

        private void settings_close_Click(object sender, RoutedEventArgs e)
        {
            if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!processor.isRunning)
            {
                Start_Button.Background = (SolidColorBrush)Resources["RedColor"];
                Start_Button.Content = "\uE71A";
                processor.Clock.Start();
                processor.isRunning = true;
            }
            else
            {

                Start_Button.Background = (SolidColorBrush)Resources["GreenColor"];
                Start_Button.Content = "\uE768";
                processor.Clock.Stop();
                processor.isRunning = false;
            }

        }

        private void Skip_Button_Click(object sender, RoutedEventArgs e)
        {
            processor.Clock.Stop();
            processor.Clock_Tick(this, this);
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            memory.resetMem();
        }


        private void Breakpoint_Click(object sender, RoutedEventArgs e)
        {
            var but = (Button)sender;
            var test = but.Background;
            if ((string)but.Content == "X")
            {
                but.Background = (SolidColorBrush)Resources["RedColor"];
                but.Content = "O";

                var item = (sender as FrameworkElement).DataContext;
                processor.brkpnts.Add(CodeStack.Items.IndexOf(item));
            }
            else
            {
                but.Background = (SolidColorBrush)Resources["GreenColor"];
                but.Content = "X";

                var item = (sender as FrameworkElement).DataContext;
                processor.brkpnts.Remove(CodeStack.Items.IndexOf(item));
            }
    
        }
        
        private void CheckBoxRA_Checked(object sender, RoutedEventArgs e)
        {
            if(autoCheck)
            {
                return;
            }
            var box = (CheckBox)sender;
            switch(box.Content)
            {
                case "0":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_01);
                    break;
                case "1":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_010);
                    break;
                case "2":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_0100);
                    break;
                case "3":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_01000);
                    break;
                case "4":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_010000);
                    break;
                case "5":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_0100000);
                    break;
                case "6":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_01000000);
                    break;
                case "7":
                    memory.memoryb1[0, Memory.PORTA] = (short)(memory.memoryb1[0, Memory.PORTA] ^ 0b_010000000);
                    break;

            }
            memory.updateMemView();
        }
        private void CheckBoxRB_Checked(object sender, RoutedEventArgs e)
        {
            if (autoCheck)
            {
                return;
            }
            var box = (CheckBox)sender;
            switch (box.Content)
            {
                case "0":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_01);
                    break;
                case "1":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_010);
                    break;
                case "2":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_0100);
                    break;
                case "3":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_01000);
                    break;
                case "4":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_010000);
                    break;
                case "5":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_0100000);
                    break;
                case "6":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_01000000);
                    break;
                case "7":
                    memory.memoryb1[0, Memory.PORTB] = (short)(memory.memoryb1[0, Memory.PORTB] ^ 0b_010000000);
                    break;

            }
            memory.updateMemView();

        }
        
    } 
}
