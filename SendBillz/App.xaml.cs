using SendBillz.Services;
using PdfSharpCore.Fonts;

namespace SendBillz
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            GlobalFontSettings.FontResolver = new CustomFontResolver();
            MainPage = new MainPage(); 
        }
    }
}
