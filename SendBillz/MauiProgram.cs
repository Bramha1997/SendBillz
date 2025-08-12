using Microsoft.Extensions.Logging;

namespace SendBillz
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("NotoSans-Bold.ttf", "NotoSansBoldFont");
                    fonts.AddFont("NotoSans-BoldItalic.ttf", "NotoSansBoldItalicFont");
                    fonts.AddFont("NotoSans-Light.ttf", "NotoSansLightFont");
                    fonts.AddFont("NotoSans-LightItalic.ttf", "NotoSansLightItalicFont");
                    fonts.AddFont("NotoSans-Medium.ttf", "NotoSansMediumFont");
                    fonts.AddFont("NotoSans-Regular.ttf", "NotoSansRegularFont");
                    fonts.AddFont("NotoSans-SemiBold.ttf", "NotoSansSemiBoldFont");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
