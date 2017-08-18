using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWP_Shared
{
    public static class ExtensionMethods
    {
        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            foreach (var item in source)
                yield return item;
            yield return element;
        }

        public static Point Divide(this Point point, int divisor) => new Point(point.X / divisor, point.Y / divisor);
        public static Point Multiply(this Point point, int factor) => new Point(point.X * factor, point.Y * factor);
        public static Point Add(this Point point, int summand) => new Point(point.X + summand, point.Y + summand);
        public static Point Subtract(this Point point, int subtrahend) => new Point(point.X - subtrahend, point.Y - subtrahend);
    }
}
