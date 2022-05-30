using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using Windows.Storage.Streams;

namespace PIC_Simulator
{
    internal class FileReader
    {
        public List<Line> lines = new List<Line>();
        FileOpenPicker picker;

        public FileReader()
        {
            picker = new FileOpenPicker();


        }

        public async Task GetLines()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".lst");

            lines.Clear();
            string[] input = null;
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            try
            {

                if (file != null)
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    DataReader reader = DataReader.FromBuffer(buffer);
                    byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(fileContent);
                    string text = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
                    input = text.Split('\n');
                }
            }
            catch (Exception)
            {
                throw;
            }
            if (file == null) return;

            //Windows.Storage.StorageFile sampleFile = await StorageFile.GetFileFromPathAsync(file.Path);
            //var input = await FileIO.ReadLinesAsync(file,Windows.Storage.Streams.UnicodeEncoding.Utf8);
            short intValue = 0;
            string[] splits = { };
            
            
            foreach (string s in input)
            {
                
                short num;
                try
                {
                    num = Convert.ToInt16(Regex.Match(s, "\\s[0-9]{5}").Value.Trim());
                }
                catch { continue; }

                var strings = Regex.Split(s, "[0-9]{5}\\s");

                
                Regex.Match(s, "");
                if (s.StartsWith(' '))
                {
                    lines.Add(new Line(num, 0, 0, strings[1], false));
                }
                else
                {
                    var beginnums = Regex.Match(s, "[0-9A-F]{4}\\s[0-9A-F]{4}").Value;
                    splits = beginnums.Split(' ');
                    intValue = short.Parse(splits[1], System.Globalization.NumberStyles.HexNumber);
                    lines.Add(new Line(num, short.Parse(splits[0], System.Globalization.NumberStyles.HexNumber), intValue, strings[1], true));
                }
            }
                
               
        }
            
            
    }
}

