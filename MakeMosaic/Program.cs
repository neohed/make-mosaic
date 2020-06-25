using ImageProcessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MakeMosaic
{
    class Program
    {
        const string imageFolder = @"..\..\Images\";
        const int pixelateSize = 10;
        const string pixelatedImageAppend = "_pix.jpg";

        static void Main(string[] args)
        {
            GetBBColours();
        }

        static void GetBBColours()
        {
            var img = Image.FromFile(Path.Combine(imageFolder, "output-onlineimagetools.png"));
            var id = GetColours(img, 60);
            HashSet<string> colours = new HashSet<string>();
            foreach (var color in id.Colours)
            {
                if (!colours.Contains(color))
                {
                    colours.Add(color);
                }
            }
            string output = JsonConvert.SerializeObject(colours);
            File.WriteAllText("BreakingBad_smoke_2.json", output);
        }

        static void GeneratePixelatedDataForImages(string ext)
        {
            var images = GetImages(ext);

            foreach (string image in images.Where(i => !i.EndsWith(pixelatedImageAppend)))
            {
                string fn = Path.GetFileNameWithoutExtension(image);

                var img = LoadImage(image);
                img.Pixelate(pixelateSize).Save(imageFolder + fn + pixelatedImageAppend);
                img.Dispose();
            }

            images = GetImages(ext);

            foreach (string image in images.Where(i => i.EndsWith(pixelatedImageAppend)))
            {
                string fn = Path.GetFileNameWithoutExtension(image);

                var img = Image.FromFile(image);
                var id = GetColours(img, pixelateSize);
                id.Name = fn;
                string output = JsonConvert.SerializeObject(id);
                File.WriteAllText(imageFolder + fn + ".json", output);
            }
        }

        static string[] GetImages(string ext)
        {
            string[] fileEntries = Directory.GetFiles(imageFolder, "*." + ext);
            return fileEntries;
        }

        static ImageFactory LoadImage(string image)
        {
            // Initialize the ImageFactory using the overload to preserve EXIF metadata.
            ImageFactory imageFactory = new ImageFactory(preserveExifData: true);

            byte[] photoBytes = File.ReadAllBytes(image);

            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    imageFactory.Load(inStream);

                    return imageFactory;
                }
            }
        }

        static ImageData GetColours(Image img, int pixSize)
        {
            using (Bitmap bmp = new Bitmap(img))
            {
                int width = bmp.Width;
                int height = bmp.Height;
                List<string> colours = new List<string>();

                for (int y = pixSize / 2; y < height; y += pixSize)
                {
                    for (int x = pixSize / 2; x < width; x += pixSize)
                    {
                        Color clr = bmp.GetPixel(x, y);
                        int red = clr.R;
                        int green = clr.G;
                        int blue = clr.B;
                        string RGB = red.ToString("X2")
                            + green.ToString("X2")
                            + blue.ToString("X2");

                        colours.Add("#"
                            + ShortRGB(RGB)
                            );
                    }
                }

                ImageData id = new ImageData();
                id.Colours = colours.ToArray();
                id.Width = (int)Math.Ceiling((decimal)width / pixSize);

                return id;
            }
        }

        static string ShortRGB(string rgb)
        {
            if (rgb[0] == rgb[1] &&
                rgb[2] == rgb[3] &&
                rgb[4] == rgb[5])
            {
                return rgb.Substring(0, 1) + rgb.Substring(2, 1) + rgb.Substring(4, 1);
            }

            return rgb;
        }
    }

    internal class ImageData
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public string[] Colours { get; set; }
    }
}
