using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MusicLibrary
{
    public static class ConvertImage
    {
        public static BitmapImage ToBitmapImage(byte[] array)//Делаем из потока байтов картинку
        {
          
                using var ms = new System.IO.MemoryStream(array);
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
        public static async Task<System.Windows.Media.SolidColorBrush> GetColor(BitmapImage image)
        {
            var bitmap = BitmapImage2Bitmap(image);
            var pixh = image.PixelHeight;
            var pixw = image.PixelWidth;
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            await Task.Run(() => { 
          
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = 0; i<pixw; i++)
            {
                for (int j = 0; j<pixh; j++)
                {
                    r+=bitmap.GetPixel(i, j).R;
                    g+=bitmap.GetPixel(i, j).G;
                    b+=bitmap.GetPixel(i, j).B;
                    }
                }

            r=r/(pixh*pixw);
            g=g/(pixh*pixw);
            b=b/(pixh*pixw);
            //Color s=Color.FromArgb(r,g,b);
            color = System.Windows.Media.Color.FromArgb(150, (byte)r, (byte)g, (byte)b);
            GC.Collect();  
            });
            return new System.Windows.Media.SolidColorBrush(color);
        }
    }
}
