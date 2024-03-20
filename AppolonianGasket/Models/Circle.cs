using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppolonianGasket.Models
{
    public class Circle
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ComplexNumber Center { get; set; }

        // curvature of the circle
        public double Bend { get; set; }
        public double Radius { get; set; }


        public Circle(double x, double y, double bend)
        {
            this.Center = new ComplexNumber(x, y);
            Bend = bend;
            Radius = Math.Abs(1.0 / this.Bend);
        }

        public void Draw(Canvas c)
        {
            DrawingHelpersLibrary.DrawingHelpers.DrawCircleHollow(c, this.Center.a, this.Center.b, Brushes.Black, 2 * Radius, 1);
        }

        // Determine the distance between two circle centerpoints
        public double Dist (Circle other)
        {
            return Math.Sqrt((this.Center.a - other.Center.a) * (this.Center.a - other.Center.a) + (this.Center.b - other.Center.b) * (this.Center.b - other.Center.b));
        }
    }
}
