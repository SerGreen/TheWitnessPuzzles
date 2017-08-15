using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWP_Shared
{
    class SolutionLine
    {
        private List<Point> points = new List<Point>();
        private Point currentPos;
        private Point prevPos;

        private List<Rectangle> hitboxes = new List<Rectangle>();
        public IReadOnlyList<Rectangle> Hitboxes { get; }
        private Rectangle head;
        public Rectangle Head { get => head; }

        public int LineWidth { get; }

        public SolutionLine(Point start)
        {
            points.Add(start);
            prevPos = currentPos = start;
            Hitboxes = hitboxes.AsReadOnly();

            LineWidth = 10;

            head = new Rectangle(currentPos.X, currentPos.Y, LineWidth, LineWidth);
        }

        public void Move(Vector2 dir, IEnumerable<Rectangle> collisionHitboxes)
        {
            prevPos = currentPos;

            // Try to move X
            currentPos.X += (int) dir.X;
            head.Offset(dir.X, 0);

            // If head hits anythig => Undo
            if(CheckCollision(collisionHitboxes))
            {
                currentPos = prevPos;
                head.Offset(-dir.X, 0);
            }

            // Try to move Y
            Point tempPos = currentPos;
            currentPos.Y += (int) dir.Y;
            head.Offset(0, dir.Y);

            // If head hits anythig => Undo
            if (CheckCollision(collisionHitboxes))
            {
                currentPos = tempPos;
                head.Offset(0, -dir.Y);
            }

            // If there was no turning, then we can skip creation of new point
            if (!ThreePointsOnSameLine(points.Last(), prevPos, currentPos))
            {
                // Calculate new hitbox of the line
                Point last = points.Last();
                int xLength = prevPos.X - last.X;
                int yLength = prevPos.Y - last.Y;

                Rectangle hitbox;

                // Vertical line
                if (xLength == 0)
                    hitbox = new Rectangle(last.X - LineWidth / 2, last.Y - LineWidth / 2, LineWidth, yLength + LineWidth / 2);
                // Horizontal line
                else
                    hitbox = new Rectangle(last.X - LineWidth / 2, last.Y - LineWidth / 2, xLength + LineWidth / 2, LineWidth);

                hitboxes.Add(hitbox);
                points.Add(prevPos);
            }

            // If we are moving backwards and hit last point, then current point takes its place
            if (points.Count > 1 && points.Last() == currentPos)
            {
                points.RemoveAt(points.Count - 1);
                hitboxes.RemoveAt(hitboxes.Count - 1);
            }
        }

        private bool CheckCollision(IEnumerable<Rectangle> collisionHitboxes) => collisionHitboxes.Any(x => head.Intersects(x));
        private bool ThreePointsOnSameLine(Point p1, Point p2, Point p3) => (p1.Y - p2.Y) * (p1.X - p3.X) == (p1.Y - p3.Y) * (p1.X - p2.X);
    }
}
