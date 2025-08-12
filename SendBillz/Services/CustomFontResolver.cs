using PdfSharpCore.Fonts;
using System.Reflection;

namespace SendBillz.Services
{
    public class CustomFontResolver : IFontResolver
    {
        private readonly Dictionary<string, string> _fontResourceNames = new()
        {
            { "NotoSansRegular", "SendBillz.Resources.Fonts.NotoSans-Regular.ttf" },
            { "NotoSansBold", "SendBillz.Resources.Fonts.NotoSans-Bold.ttf" },
            { "NotoSansSemiBold", "SendBillz.Resources.Fonts.NotoSans-SemiBold.ttf" },
            // Add more fonts if needed, e.g.:
            // { "MyFont#Bold", "SendBillz.Resources.Fonts.MyFont-Bold.ttf" }
        };

        public string DefaultFontName => throw new NotImplementedException();

        public byte[] GetFont(string faceName)
        {
            if (!_fontResourceNames.TryGetValue(faceName, out string resourceName))
                return null;

            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException($"Cannot find font resource '{resourceName}'.");

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Adjust familyName if needed (case-insensitive)
            if (string.Equals(familyName, "NotoSansRegularFont", StringComparison.OrdinalIgnoreCase))
            {
                // Simplified example for just regular font
                return new FontResolverInfo("NotoSansRegular");
            }

            // Fallback to default system font or throw
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }
    }
}
