using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Diagnostics;

namespace PIC_Simulator
{
    internal class FileReader
    {
        public Line[] lines;
        FileOpenPicker picker;
        public string alllines;
        public FileReader()
        {
            picker = new FileOpenPicker();


        }

        public async void GetLines()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".lst");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            Windows.Storage.StorageFile sampleFile = await StorageFile.GetFileFromPathAsync(file.Path);
            Debug.WriteLine(await Windows.Storage.FileIO.ReadTextAsync(sampleFile));
            //alllines = await Windows.Storage.FileIO.ReadTextAsync(file.Path);
            Debug.WriteLine(file.Path);
        }
    }
}
