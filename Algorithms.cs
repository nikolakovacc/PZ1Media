using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace GraphicsApp_PZ1
{
    public static class GraphicsAlgorithms
    {

        // ПУНКТ 6: Алгоритм Брезенгема для лінії (цілочисельний)

        public static void DrawBresenhamLine(Bitmap bmp, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < bmp.Width && y0 >= 0 && y0 < bmp.Height)
                {
                    bmp.SetPixel(x0, y0, color);
                }

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }


        // ПУНКТ 6: Алгоритм Брезенгема для кола

        public static void DrawBresenhamCircle(Bitmap bmp, int xc, int yc, int r, Color color)
        {
            int x = 0;
            int y = r;
            int d = 3 - 2 * r;

            void Put8Pixels(int cx, int cy, int px, int py)
            {
                int[,] pts = {
                    { cx + px, cy + py }, { cx - px, cy + py },
                    { cx + px, cy - py }, { cx - px, cy - py },
                    { cx + py, cy + px }, { cx - py, cy + px },
                    { cx + py, cy - px }, { cx - py, cy - px }
                };

                for (int i = 0; i < 8; i++)
                {
                    int xk = pts[i, 0];
                    int yk = pts[i, 1];
                    if (xk >= 0 && xk < bmp.Width && yk >= 0 && yk < bmp.Height)
                    {
                        bmp.SetPixel(xk, yk, color);
                    }
                }
            }

            Put8Pixels(xc, yc, x, y);

            while (y >= x)
            {
                x++;
                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                {
                    d = d + 4 * x + 6;
                }
                Put8Pixels(xc, yc, x, y);
            }
        }


        // ПУНКТ 4 та 6: Алгоритм генерації множини Мандельброта (фрактал)
        public static Bitmap GenerateMandelbrot(int width, int height, int maxIter = 100)
        {
            Bitmap bmp = new Bitmap(width, height);
            double xmin = -2.0, xmax = 1.0;
            double ymin = -1.2, ymax = 1.2;

            for (int py = 0; py < height; py++)
            {
                double cy = ymin + (py / (double)height) * (ymax - ymin);
                for (int px = 0; px < width; px++)
                {
                    double cx = xmin + (px / (double)width) * (xmax - xmin);
                    double zx = 0, zy = 0;
                    int iter = 0;

                    while (zx * zx + zy * zy <= 4.0 && iter < maxIter)
                    {
                        double xtemp = zx * zx - zy * zy + cx;
                        zy = 2 * zx * zy + cy;
                        zx = xtemp;
                        iter++;
                    }

                    Color pixelColor;
                    if (iter == maxIter)
                    {
                        pixelColor = Color.Black;
                    }
                    else
                    {
                        // Кольорова палітра на основі кількості ітерацій
                        int r = (int)(Math.Sin(0.16 * iter + 4) * 127 + 128);
                        int g = (int)(Math.Sin(0.13 * iter + 2) * 127 + 128);
                        int b = (int)(Math.Sin(0.10 * iter + 1) * 127 + 128);
                        pixelColor = Color.FromArgb(r, g, b);
                    }

                    bmp.SetPixel(px, py, pixelColor);
                }
            }

            return bmp;
        }


        // ПУНКТ 7: Афінні перетворення точок фігури

        public static PointF[] ApplyAffineTransform(PointF[] points, float dx, float dy, float scaleX, float scaleY, float angleDeg, PointF pivot)
        {
            PointF[] result = new PointF[points.Length];
            double rad = angleDeg * Math.PI / 180.0;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            for (int i = 0; i < points.Length; i++)
            {
                // 1. Поворот відносно обраної точки pivot
                float xShift = points[i].X - pivot.X;
                float yShift = points[i].Y - pivot.Y;

                double xRot = xShift * cosA - yShift * sinA;
                double yRot = xShift * sinA + yShift * cosA;

                // 2. Масштабування
                double xScaled = xRot * scaleX;
                double yScaled = yRot * scaleY;

                // 3. Зсув (переміщення) та повернення до опорної точки
                float xFinal = (float)(xScaled + pivot.X + dx);
                float yFinal = (float)(yScaled + pivot.Y + dy);

                result[i] = new PointF(xFinal, yFinal);
            }

            return result;
        }


        // ПУНКТ 11: Зміна яскравості та освітленості (контрасту)

        public static Bitmap AdjustBrightnessContrast(Bitmap src, int brightness, float contrast)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);

                    // Контраст та яскравість
                    int r = (int)(((c.R - 128) * contrast) + 128 + brightness);
                    int g = (int)(((c.G - 128) * contrast) + 128 + brightness);
                    int b = (int)(((c.B - 128) * contrast) + 128 + brightness);

                    r = Math.Clamp(r, 0, 255);
                    g = Math.Clamp(g, 0, 255);
                    b = Math.Clamp(b, 0, 255);

                    dst.SetPixel(x, y, Color.FromArgb(c.A, r, g, b));
                }
            }
            return dst;
        }

        // ПУНКТ 12: Перетворення колірних моделей

        public static Bitmap ConvertToGrayscale(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);
                    // Формула світності (Luma)
                    int gray = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                    gray = Math.Clamp(gray, 0, 255);
                    dst.SetPixel(x, y, Color.FromArgb(c.A, gray, gray, gray));
                }
            }
            return dst;
        }

        public static Bitmap ConvertToHSVRepresentation(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);
                    float h = c.GetHue(); // 0..360
                    float s = c.GetSaturation(); // 0..1
                    float v = c.GetBrightness(); // 0..1

                    // Візуалізація: H відображаємо у тон, насиченість та яскравість масштабуємо
                    int r = (int)(h / 360f * 255);
                    int g = (int)(s * 255);
                    int b = (int)(v * 255);

                    dst.SetPixel(x, y, Color.FromArgb(c.A, r, g, b));
                }
            }
            return dst;
        }


        // ПУНКТ 13: Алгоритми стиснення даних


        // 1. Алгоритм RLE (Run-Length Encoding)
        public static byte[] CompressRLE(byte[] data)
        {
            using MemoryStream ms = new MemoryStream();
            int i = 0;
            while (i < data.Length)
            {
                byte b = data[i];
                byte count = 1;
                while (i + 1 < data.Length && data[i + 1] == b && count < 255)
                {
                    count++;
                    i++;
                }
                ms.WriteByte(count);
                ms.WriteByte(b);
                i++;
            }
            return ms.ToArray();
        }

        // 2. Алгоритм LZW (Lempel-Ziv-Welch)
        public static byte[] CompressLZW(byte[] input)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 256; i++)
            {
                dictionary.Add(((char)i).ToString(), i);
            }

            string current = string.Empty;
            List<int> compressedCodes = new List<int>();

            foreach (byte b in input)
            {
                char c = (char)b;
                string combined = current + c;
                if (dictionary.ContainsKey(combined))
                {
                    current = combined;
                }
                else
                {
                    compressedCodes.Add(dictionary[current]);
                    if (dictionary.Count < 65535)
                    {
                        dictionary.Add(combined, dictionary.Count);
                    }
                    current = c.ToString();
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                compressedCodes.Add(dictionary[current]);
            }

            // Запис результату (кожен код як 2 байти ushort)
            byte[] result = new byte[compressedCodes.Count * 2];
            for (int i = 0; i < compressedCodes.Count; i++)
            {
                result[i * 2] = (byte)(compressedCodes[i] & 0xFF);
                result[i * 2 + 1] = (byte)((compressedCodes[i] >> 8) & 0xFF);
            }

            return result;
        }

        // 3. Алгоритм JPEG (вбудований кодек зі змінною якістю)
        public static byte[] CompressJPEG(Bitmap bmp, long quality = 75)
        {
            using MemoryStream ms = new MemoryStream();
            ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            bmp.Save(ms, jpegEncoder, encoderParams);
            return ms.ToArray();
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null!;
        }
    }
}
