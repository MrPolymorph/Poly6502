using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Poly6502.Microprocessor.Flags;
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
                
                byte lo = 0;
                byte hi = 0;
                byte p;
                byte.TryParse(line.Substring(9, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out lo);
                byte.TryParse(line.Substring(12, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hi);
                byte.TryParse(line.Substring(65, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out p);
                var ll = new LogLine()
                {
                    ProgramCounter = ushort.Parse(line.Substring(0, 4), NumberStyles.HexNumber),
                    OpCode = byte.Parse(line.Substring(6,2), NumberStyles.HexNumber),
                    LoByte = lo,
                    HiByte = hi,
                    Flags = (StatusRegister)p
                };
                
                logLines.Add(ll);
            }

            return logLines;
        }
    }
}