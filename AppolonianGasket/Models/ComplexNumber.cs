using System;

namespace AppolonianGasket.Models
{
    public class ComplexNumber
    {
        public  double a { get; set; }
        public double b { get; set; }

        public ComplexNumber(double a, double b) 
        {
            this.a = a;
            this.b = b;
        }

        public ComplexNumber Add(ComplexNumber other)
        {
            return new ComplexNumber(this.a + other.a, this.b + other.b);
        }

        public ComplexNumber Sub(ComplexNumber other)
        {
            return new ComplexNumber(this.a - other.a, this.b - other.b);
        }

        public ComplexNumber Scale(double value)
        {
            return new ComplexNumber(this.a * value, this.b * value);
        }

        public ComplexNumber Mult(ComplexNumber other)
        {
            double a_temp = this.a * other.a - this.b * other.b;
            double b_temp = this.a * other.b + other.a * this.b;

            return new ComplexNumber(a_temp, b_temp);
        }

        public ComplexNumber Sqrt()
        {
            double m = Math.Sqrt(a * a + b * b);
            double angle = Math.Atan2(b, a);
            m = Math.Sqrt(m);
            angle = angle / 2;

            return new ComplexNumber(m * Math.Cos(angle), m * Math.Sin(angle));
        }


    }
}
