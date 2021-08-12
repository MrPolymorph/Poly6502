namespace Poly6502.Visualiser.ViewModels
{
    public class RamViewModel : ViewModelBase
    {
        private readonly Ram.Ram _ram;
        
        public RamViewModel(Ram.Ram ram)
        {
            _ram = ram;
        }
        
        
    }
}