using SendBillz.Services;
using PdfSharpCore.Fonts;

namespace SendBillz
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            InitializeComponent();

            GlobalFontSettings.FontResolver = new CustomFontResolver();
            MainPage = new NavigationPage(mainPage);
        }
    }
}
