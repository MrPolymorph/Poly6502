using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Poly6502.Visualiser.Models;

namespace Poly6502.Visualiser
{
    public class LogLoader
    {
        public LogLoader()
        {
            
        }

        public async Task<List<LogLine>> LoadLog(string file)
        {
            var text = await File.ReadAllTextAsync(file);

            var lines = text.Split('\n');

            var logLines = new List<LogLine>();
            
            foreach (var line in lines)
            {
                if(string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                    continue;
                
                byte data1 = 0;
                byte data2 = 0;
                byte.TryParse(line.Substring(9, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out data1);
                byte.TryParse(line.Substring(12, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out data2);
                var ll = new LogLine()
                {
                    ProgramCounter = ushort.Parse(line.Substring(0, 4), NumberStyles.HexNumber),
                    OpCode = byte.Parse(line.Substring(6,2), NumberStyles.HexNumber),
                    Data1 = data1,
                    Data2 = data2
                };
                
                logLines.Add(ll);
            }

            return logLines;
        }
    }
}