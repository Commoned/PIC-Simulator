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
    public sealed partial class MainPage : Page , ICodeInterface
    {
        Memory memory;
        Processor processor;
        private FileReader filereader;
        
        public MainPage()
        {
            this.DataContext = this;
            memory = new Memory();
            processor = new Processor(this,memory);
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
            foreach(Line line in processor.lines)
            {
                if(line.executable)
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
            if(!processor.isRunning)
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
                processor.isRunning =false;
            }
            
        }

        private void Skip_Button_Click(object sender, RoutedEventArgs e)
        {
            processor.Clock.Stop();
            processor.Clock_Tick(this,this);
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            memory.resetMem();
        }

        private void TextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var block = (Border)sender;
            var text = (TextBlock)block.Child;
            Pop_Reg.Text = text.Text;
            Click_Popup.IsOpen = true;


            
            tempSender = sender;
        }

        private void Pop_Save_Click(object sender, RoutedEventArgs e)
        {
            var tempBorder = (Border)tempSender;
            var text = (TextBlock)tempBorder.Child;
            var toSave = Pop_Reg.Text;

            text.Text = toSave;
            
            memory.updateMemView();
            Click_Popup.IsOpen = false;
        }
    }
}
