using System.Diagnostics;
using System.Drawing;

namespace Screen_comparer
{
    public class Screen_comparer
    {
        static string file1 = string.Empty;
        static string file2 = string.Empty;
        static string output = string.Empty;
        static readonly Stopwatch stopwatch = new();

        public static void Main()
        {
            Console.WriteLine("Start");
            stopwatch.Start();
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) => Console.WriteLine(eventArgs.Exception.ToString());
            ParseArguments();
            ImageDiffUtil idu = new(file1, file2);
            Rectangle rect = idu.GetDiffRectangles();

            File.WriteAllText(output, rect.X + "," + rect.Y + "," + (rect.X + rect.Width) + "," + (rect.Y + rect.Height));
        }

        static void ParseArguments()
        {
            Console.WriteLine("Parsing arguments...");
            stopwatch.Restart();
            string[] arguments = Environment.GetCommandLineArgs();

            if (arguments[1].ToLower().Equals("/h") || arguments[1].ToLower().Equals("/help"))
            {
                PrintHelp();
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (arguments.Length <= 3)
            {
                PrintHelp();
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (arguments.Length > 3)
            {
                file1 = arguments[1];
                file2 = arguments[2];
                output = arguments[3];
            }
            Console.WriteLine("OK : " + stopwatch.ElapsedMilliseconds + " ms");
        }

        static void PrintHelp()
        {
            //clears the extension from the script name
            string scriptName = Environment.GetCommandLineArgs()[0];
            scriptName = scriptName[..];
            Console.WriteLine(scriptName + " get difference between two screen and return a coordinate of changes in files (Begin [x,y], End [x,y])");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine(" " + scriptName + " filename1 filename2 output");
            Console.WriteLine("");
            Console.WriteLine("filename - the files that will be compared ");
            Console.WriteLine("output - file where the result will be returned (ex : C:\\Users\\%USERNAME%\\Desktop\\output.txt)");
        }

        public class ImageDiffUtil
        {
            readonly Bitmap image1 = new(file1);
            readonly Bitmap image2 = new(file2);

            public ImageDiffUtil(string filename1, string filename2)
            {
                image1 = (Bitmap)Image.FromFile(filename1);
                image2 = (Bitmap)Image.FromFile(filename2);
            }

            public IList<Point> GetDiffPixels()
            {
                Console.WriteLine("GetDiffPixels...");
                stopwatch.Restart();
                var widthRange = Enumerable.Range(0, image1.Width);
                var heightRange = Enumerable.Range(0, image1.Height);

                var result = widthRange
                                .SelectMany(x => heightRange, (x, y) => new Point(x, y))
                                .Select(point => new
                                {
                                    Point = point,
                                    Pixel1 = image1.GetPixel(point.X, point.Y),
                                    Pixel2 = image2.GetPixel(point.X, point.Y)
                                })
                                .Where(pair => pair.Pixel1 != pair.Pixel2)
                                .Select(pair => pair.Point)
                                .ToList();
                Console.WriteLine("OK " + stopwatch.ElapsedMilliseconds + " ms");
                return result;
            }

            public Rectangle GetDiffRectangles()
            {
                Console.WriteLine("GetDiffRectangles...");
                stopwatch.Restart();
                IList<Point> differentPixels = GetDiffPixels();
                Rectangle result = new(differentPixels.Min(point => point.X),
                                        differentPixels.Min(point => point.Y),
                                        differentPixels.Max(point => point.X) - differentPixels.Min(point => point.X),
                                        differentPixels.Max(point => point.Y) - differentPixels.Min(point => point.Y));
                Console.WriteLine("Ok : " + stopwatch.ElapsedMilliseconds + " ms");
                return result;
            }
        }
    }
}