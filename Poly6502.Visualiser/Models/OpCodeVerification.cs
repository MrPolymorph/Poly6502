namespace Poly6502.Visualiser.Models
{
    public class OpCodeVerification
    {
        public LogLine Expected { get; set; }
        public LogLine Actual { get; set; }
        public bool Pass { get; set; }
    }
}