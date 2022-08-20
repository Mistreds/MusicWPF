using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MusicLibrary
{
    public static class ConvertImage
    {
        public static BitmapImage ToBitmapImage(byte[] array)//Делаем из потока байтов картинку
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // here
                    image.StreamSource = ms;
                    image.Rotation = Rotation.Rotate0;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.EndInit();
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }
        private static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public static System.Windows.Media.SolidColorBrush GetColor(BitmapImage image)
        {
            var bitmap = BitmapImage2Bitmap(image);
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = 0; i<image.PixelHeight; i++)
            {
                for (int j = 0; j<image.PixelWidth; j++)
                {
                    r+=bitmap.GetPixel(i, j).R;
                    g+=bitmap.GetPixel(i, j).G;
                    b+=bitmap.GetPixel(i, j).B;
                }
            }

            r=r/(image.PixelHeight*image.PixelWidth);
            g=g/(image.PixelHeight*image.PixelWidth);
            b=b/(image.PixelHeight*image.PixelWidth);
            //Color s=Color.FromArgb(r,g,b);
            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(150, (byte)r, (byte)g, (byte)b);
            return new System.Windows.Media.SolidColorBrush(color);
        }
    }
}
