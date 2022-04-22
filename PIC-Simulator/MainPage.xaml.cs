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
    public sealed partial class MainPage : Page
    {
        Memory memory;
        Processor processor;
        private FileReader filereader;
        
        public MainPage()
        {
            this.DataContext = this;
            memory = new Memory();
            processor = new Processor(memory);
            DataContext = memory;
            
            this.InitializeComponent();
            CodeStack.ItemsSource = processor.lines;
        }

       

        

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            // Daniels stuff
            memory.initMem();


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
                processor.Clock.Start();
                processor.isRunning = true;
            }
            else
            {

                Start_Button.Background = (SolidColorBrush)Resources["GreenColor"];
                processor.Clock.Stop();
                processor.isRunning =false;
            }
            
        }
    }
}
