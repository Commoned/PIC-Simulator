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
        TextBlock tempTB;
        ThumbConverter converter;

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
            try
            {
                selectCode(processor.runlines[memory.Pcl].Linenumber - 1);
            }
            catch (Exception ex)
            {
                Start_Button.IsEnabled = false;
                Skip_Button.IsEnabled = false;
            }

        }

        public void selectCode(int line)
        {
            this.CodeStack.SelectedIndex = line;
            this.CodeStack.ScrollIntoView(this.CodeStack.SelectedItem,ScrollIntoViewAlignment.Default);
            
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
            if (!QuartzPopup.IsOpen) { QuartzPopup.IsOpen = true; }
            Quarzslider.Value = memory.quarztakt;
            Freq.Text = string.Format("{0:n}", (1 / (memory.quarztakt) * 4)) + " MHz";
        }

        private void settings_close_Click(object sender, RoutedEventArgs e)
        {
            if (QuartzPopup.IsOpen) { QuartzPopup.IsOpen = false; }
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
            if(processor.eeWriting)
            {
                memory.memoryb1[1, Memory.EECON1] = memory.setBit(memory.memoryb1[1, Memory.EECON1], 3); // If Writing is interrupted error
            }
            memory.resetMem();
            processor.isSleeping = false;
            
            selectCode(processor.runlines[memory.Pcl].Linenumber - 1);
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
                but.Background = (SolidColorBrush)Resources["GrayColor"];
                but.Content = "X";
                Start_Button.Background = (SolidColorBrush)Resources["GreenColor"];
                Start_Button.Content = "\uE768";
                processor.Clock.Stop();
                processor.isRunning = false;


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

        private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            RegeditPopup.IsOpen = true;
            Border border = (Border)sender;
            
            tempTB = (TextBlock)border.Child;

            
            

            RegVal.Text = tempTB.Text;
            


        }

        private void RegSave_Click(object sender, RoutedEventArgs e)
        {
            RegeditPopup.IsOpen = false;
            try
            {
                if(RegNum.Text.ToUpper() == "INTCON")
                {
                    memory.memoryb1[0, Memory.INTCON] = short.Parse(RegVal.Text, System.Globalization.NumberStyles.HexNumber); 
                }
                if (memory.checkBit(short.Parse(RegNum.Text, System.Globalization.NumberStyles.HexNumber), 7))
                {
                    
                    memory.memoryb1[1, short.Parse(RegNum.Text, System.Globalization.NumberStyles.HexNumber) & 0b_01111111] = short.Parse(RegVal.Text, System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    memory.memoryb1[0, short.Parse(RegNum.Text, System.Globalization.NumberStyles.HexNumber)] = short.Parse(RegVal.Text, System.Globalization.NumberStyles.HexNumber);
                }
                
            }
            catch (Exception ex)
            {
                RegeditPopup.IsOpen = true;
            }
            memory.updateMemView();
        }

        private void RegClose_Click(object sender, RoutedEventArgs e)
        {
            RegeditPopup.IsOpen = false;
            
        }
    
 



        private void Quarzslider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            
            memory.quarztakt = Quarzslider.Value;
            // umrechnen quarztakt in MHz


            Freq.Text = string.Format("{0:n}",(1/(memory.quarztakt)*4))+" MHz";
        }

        private void WDTChecker_Checked(object sender, RoutedEventArgs e)
        {
            memory.WDTE = 1;
            WDTChecker.Content = "WDT aktiv";
        }

        private void WDTChecker_Unchecked(object sender, RoutedEventArgs e)
        {
            memory.WDTE = 0;
            WDTChecker.Content = "WDT inaktiv";
        }

        private void ViewEEPROM_Click(object sender, RoutedEventArgs e)
        {
            EEPROMPopup.IsOpen = true;
            memory.eepromViewOpen = true;
        }

        private void closeEEPROM_Click(object sender, RoutedEventArgs e)
        {
            EEPROMPopup.IsOpen = false;
            memory.eepromViewOpen = false;
        }

        private void resetTime_Click(object sender, RoutedEventArgs e)
        {
            memory.runtime = 0;
            memory.updateMemView();
        }
    }
    public class ThumbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string val = string.Format("{0:n}",value);
            return val+" \u00B5s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

