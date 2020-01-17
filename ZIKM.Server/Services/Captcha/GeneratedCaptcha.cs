using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Text;
using SixLabors.Primitives;
using System;
using System.IO;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Services.Captcha {
    /// <summary>
    /// Generate simple captcha in png format
    /// </summary>
    internal class GeneratedCaptcha : ICaptcha {
        /// <summary>
        /// Get instance of generating captcha class
        /// </summary>
        public static GeneratedCaptcha Instance { get; } = new GeneratedCaptcha();

        private readonly Font font;
        private readonly TextGraphicsOptions options = new TextGraphicsOptions { Antialias = true };
        private readonly PointF location = new PointF(15, 20);

        /// <summary>
        /// Create class for generating captcha
        /// </summary>
        private GeneratedCaptcha() {
            FontFamily fontFamily = new FontCollection()
                .Install(Path.Combine(Directory.GetCurrentDirectory(), "LiberationSerif-Italic.ttf"));
            
            font = new Font(fontFamily, 48);
        }

        /// <summary>
        /// Generate image with code
        /// </summary>
        /// <param name="code">Code</param>
        /// <returns></returns>
        private Stream Generate(string code) {
            using Image<Rgba32> image = new Image<Rgba32>(null, 200, 80, Rgba32.DarkBlue);
            image.Mutate(x => x.ApplyProcessor(
                new DrawTextProcessor(
                    options,
                    code, 
                    font, 
                    Brushes.Solid(Rgba32.LightBlue),
                    null, 
                    location)));

            Stream stream = new MemoryStream();
            image.Save(stream, new PngEncoder());
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public byte[] GetCaptcha(out string code) {
            Random random = new Random();
            code = random.Next(0, 9999999).ToString("0000000");

            using Stream stream = Generate(code);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
