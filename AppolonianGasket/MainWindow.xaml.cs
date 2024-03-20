using AppolonianGasket.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AppolonianGasket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DEFAULT_SavePathString = ".\\Images";
        private const string filename = "image_save";
        private static string SavePathString { get; set; } = "";

        private int _timer_delay = 0;
        private bool shouldContinue = true;
        Circle c1 { get; set; }
        Circle c2 { get; set; }
        Circle c3 { get; set; }

        //WriteableBitmap bmp = new WriteableBitmap(
        //    (int)SystemParameters.PrimaryScreenWidth,
        //    (int)SystemParameters.PrimaryScreenHeight,
        //    96,
        //    96,
        //    PixelFormats.Bgr24,
        //    null);

        List<Circle> allCircles { get; set; } = new List<Circle>();
        List<List<Circle>> queue { get; set; } = new List<List<Circle>>();

        double epsilon = 0.1;  // error tolerance

        public static double ScreenWidth { get; set; } = 600;
        public static double ScreenHeight { get; set; } = 600;

        int GenerationCount { get; set; } = 0;

        public MainWindow()
        {
            InitializeComponent();

            MainCanvas.Width = ScreenWidth; 
            MainCanvas.Height = ScreenHeight;

            //img.ImageSource = bmp;

            OnUserCreate();

            OnUserUpdate();


            //// Set a timer to redraw the canvas
            //var timer = new DispatcherTimer(DispatcherPriority.Normal)
            //{
            //    Interval = TimeSpan.FromMilliseconds(_timer_delay)
            //};

            //timer.Tick += (sender, args) =>
            //{
            //    Dispatcher.Invoke(() => Draw());
            //};

            //timer.Start();
        }

        public static RenderTargetBitmap GenerateImage(Canvas c, string path)
        {
            if (path == null) return null; ;

            double scale = ScreenWidth / 96;

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)(scale * (c.ActualWidth + 1)), 
                (int)(scale * (c.ActualHeight + 1)), 
                scale * 96, 
                scale * 96, 
                PixelFormats.Default);
            RenderOptions.SetBitmapScalingMode(renderBitmap, BitmapScalingMode.Fant);
            renderBitmap.Render(c);

            using (FileStream outStream = new FileStream(SavePathString, FileMode.Create))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            return renderBitmap;
        }

        public void OnUserCreate()
        {
            c1 = new Circle(0.5 * ScreenWidth, 0.5 * ScreenHeight, -1 / (0.5 * ScreenWidth));  // the outermost circle...must have a negative curvature (bend)
            Random rnd = new Random();
            double r2 = rnd.Next(100, (int)(c1.Radius * 0.5));
            Point p1 = new Point(c1.X, c1.Y);
            double angle = rnd.Next(0, (int)(1000 * Math.PI * 2.0)) / 1000;
            Vector2 unit_v = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2 v = new Vector2((float)(c1.Radius - r2) * unit_v.X, (float)(c1.Radius - r2) * unit_v.Y);

            c2 = new Circle(0.5 * ScreenWidth + v.X, 0.5 * ScreenHeight + v.Y, 1 / r2);  // the outermost circle...must have a negative curvature (bend)
            double r3 = c1.Radius - r2;
            v = new Vector2((float)(c1.Radius - r3) * -unit_v.X, (float)(c1.Radius - r3) * -unit_v.Y);  // flip the direction of the unit vector

            c3 = new Circle(0.5 * ScreenWidth + v.X, 0.5 * ScreenHeight + v.Y, 1 / r3);
            //c1 = new Circle(200, 200, -.005);
            //c2 = new Circle(100, 200, 0.01);
            //c3 = new Circle(300, 200, 0.01);

            //c1.Draw(MainCanvas);
            //c2.Draw(MainCanvas);
            //c3.Draw(MainCanvas);


            allCircles.Add(c1);
            allCircles.Add(c2);
            allCircles.Add(c3);

            queue.Add(allCircles);
            GenerationCount++;
        }

        public async Task OnUserUpdate()
        {
            while (shouldContinue)
            {
                // Current total circles
                int len1 = allCircles.Count;

                //// Generate next generation of circles
                nextGeneration();

                // new total circles
                int len2 = allCircles.Count;

                // Stop drawing when no new circles are added
                if (len1 == len2)
                {
                    Console.WriteLine("Done.");
                    shouldContinue = false;
                }
                else
                {
                    Console.WriteLine("len1: " + len1.ToString() + "    len2: " + len2.ToString() + "\n");
                }

                await Task.Run(() => Draw());

                //Thread.Sleep(_timer_delay);
            }
        }

        private Circle[] ComplexDescartes(Circle c1, Circle c2, Circle c3, double[] k4)
        {
            ComplexNumber[] calcs = new ComplexNumber[4];
            Circle[] circles = new Circle[4];

            double k1 = c1.Bend;
            double k2 = c2.Bend;
            double k3 = c3.Bend;
            ComplexNumber z1 = c1.Center;
            ComplexNumber z2 = c2.Center;
            ComplexNumber z3 = c3.Center;

            ComplexNumber zk1 = z1.Scale(k1);
            ComplexNumber zk2 = z2.Scale(k2);
            ComplexNumber zk3 = z3.Scale(k3);

            ComplexNumber sum = zk1.Add(zk2).Add(zk3);
            ComplexNumber root = zk1.Mult(zk2).Add(zk2.Mult(zk3).Add(zk1.Mult(zk3)));
            root = root.Sqrt().Scale(2.0);

            calcs[0] = sum.Add(root).Scale(1.0 / k4[0]);
            calcs[1] = sum.Sub(root).Scale(1.0 / k4[0]);
            calcs[2] = sum.Add(root).Scale(1.0 / k4[1]);
            calcs[3] = sum.Sub(root).Scale(1.0 / k4[1]);

            Circle cir1 = new Circle(calcs[0].a, calcs[0].b, k4[0]);
            Circle cir2 = new Circle(calcs[1].a, calcs[1].b, k4[0]);
            Circle cir3 = new Circle(calcs[2].a, calcs[2].b, k4[1]);
            Circle cir4 = new Circle(calcs[3].a, calcs[3].b, k4[1]);

            circles[0] = cir1;
            circles[1] = cir2;
            circles[2] = cir3;
            circles[3] = cir4;

            return circles;
        }

        private double[] Descartes(Circle c1, Circle c2, Circle c3)
        {
            double[] calcs = new double[2];

            double k1 = c1.Bend;
            double k2 = c2.Bend;
            double k3 = c3.Bend;

            double sum = k1 + k2 + k3;
            double product = Math.Abs(k1 * k2 + k2 * k3 + k1 * k3);
            double root = 2.0 * Math.Sqrt(product);

            calcs[0] = sum + root;
            calcs[1] = sum - root;

            return calcs;

        }

        private bool Validate(Circle c4, Circle c1, Circle c2, Circle c3)
        {
            if (c4.Radius < 2) 
                return false;

            foreach (Circle other in allCircles)
            {
                double d = c4.Dist(other);
                double radiusDiff = Math.Abs(c4.Radius - other.Radius);
                if (d < epsilon && radiusDiff < epsilon) 
                {
                    return false;
                }
            }

            // check if all 4 circles are mutually tangential
            if (isTangent(c4, c1) is false) return false;
            if (isTangent(c4, c2) is false) return false;
            if (isTangent(c4, c3) is false) return false;

            return true;
        } 

        private bool isTangent(Circle c1, Circle c2)
        {
            double d = c1.Dist(c2);
            double r1 = c1.Radius;
            double r2 = c2.Radius;

            bool a = Math.Abs(d - (r1 + r2)) < epsilon;
            bool b = Math.Abs(d - Math.Abs(r2 - r1)) < epsilon;

            return a || b;
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           // MainCanvas.Children.Clear();

            List<List<Circle>> nextQueue = new List<List<Circle>>();

            foreach (List<Circle> triplet in queue)
            {
                Circle c1 = triplet[0];
                Circle c2 = triplet[1];
                Circle c3 = triplet[2];

                double[] k4 = Descartes(c1, c2, c3);
                Circle[] newCircles = ComplexDescartes(c1, c2, c3, k4);

                foreach (Circle newCircle in newCircles)
                {
                    if (Validate(newCircle, c1, c2, c3))
                    {
                        allCircles.Add(newCircle);
                        List<Circle> t1 = new List<Circle>();
                        t1.Add(c1);
                        t1.Add(c2);
                        t1.Add(newCircle);

                        List<Circle> t2 = new List<Circle>();
                        t2.Add(c1);
                        t2.Add(c3);
                        t2.Add(newCircle);

                        List<Circle> t3 = new List<Circle>();
                        t3.Add(c2);
                        t3.Add(c3);
                        t3.Add(newCircle);

                        nextQueue.Add(t1);
                        nextQueue.Add(t2);
                        nextQueue.Add(t3);
                    }
                }
            }
           // Draw();
            queue = nextQueue;
            GenerationCount++;

            OnUserUpdate();
        }

        public void nextGeneration()
        {
            Console.WriteLine(GenerationCount.ToString());

            // MainCanvas.Children.Clear();

            List<List<Circle>> nextQueue = new List<List<Circle>>();

            foreach (List<Circle> triplet in queue)
            {
                Circle c1 = triplet[0];
                Circle c2 = triplet[1];
                Circle c3 = triplet[2];

                // Calculate curvature for next circle
                double[] k4 = Descartes(c1, c2, c3);

                // Generate new circles based on Descartes' theorem
                Circle[] newCircles = ComplexDescartes(c1, c2, c3, k4);

                foreach (Circle newCircle in newCircles)
                {
                    if (Validate(newCircle, c1, c2, c3))
                    {
                        allCircles.Add(newCircle);
                        // New triplets formed with the new circle for the next generation
                        List<Circle> t1 = new List<Circle>();
                        t1.Add(c1);
                        t1.Add(c2);
                        t1.Add(newCircle);

                        List<Circle> t2 = new List<Circle>();
                        t2.Add(c1);
                        t2.Add(c3);
                        t2.Add(newCircle);

                        List<Circle> t3 = new List<Circle>();
                        t3.Add(c2);
                        t3.Add(c3);
                        t3.Add(newCircle);

                        nextQueue.Add(t1);
                        nextQueue.Add(t2);
                        nextQueue.Add(t3);

                        newCircle.Draw(MainCanvas);
                    }
                }
            }
            //Thread.Sleep(_timer_delay);

            queue = nextQueue;
            GenerationCount++;
        }

        private void Draw()
        {
            Dispatcher.Invoke(() =>
            {
                //MainCanvas.Children.Clear();
                foreach (Circle item in allCircles)
                {
                    item.Draw(MainCanvas);
                }
            });

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            int count = 1;
            while (System.IO.File.Exists(SavePathString))
            {
                SavePathString = System.IO.Path.Combine(DEFAULT_SavePathString, filename + "_" + count.ToString() + ".png");
                count++;
            }

            RenderTargetBitmap rtb = null;

            try
            {

                Directory.CreateDirectory(DEFAULT_SavePathString);
                SavePathString = System.IO.Path.Combine(DEFAULT_SavePathString, filename + ".png");

                if (filename != string.Empty && SavePathString != string.Empty)
                {
                    rtb = GenerateImage(MainCanvas, DEFAULT_SavePathString);
                }

                if (rtb != null)
                {
                    var trans_rtb = new TransformedBitmap(rtb, new ScaleTransform(
                        MainCanvas.ActualWidth / rtb.PixelWidth,
                        MainCanvas.ActualHeight / rtb.PixelHeight));
 //                   img.Source = trans_rtb;
                }
            }
            catch
            {
                MessageBox.Show("Unable to save file: " + filename);
            }


        }
    }
}
