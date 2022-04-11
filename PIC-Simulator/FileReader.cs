using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PIC_Simulator
{
    internal class FileReader
    {
        public Line[] lines;
        FileOpenPicker picker;
        public FileReader()
        {
            picker = new FileOpenPicker();
           

        }

        public Line[] GetLines()
        {
            var file = picker.PickSingleFileAsync().GetResults();
            


            return lines;
        }
}
