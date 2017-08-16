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

        public bool Move(Vector2 dir, IEnumerable<Rectangle> collisionHitboxes)
        {
            bool moveSuccessful = true;

            // Remove fickle hitbox between last point and current point
            if (hitboxes.Count > 0)
                hitboxes.RemoveAt(hitboxes.Count - 1);

            prevPos = currentPos;

            // Try to move
            currentPos += dir.ToPoint();
            head.Offset(dir);

            // If head hits anythig => Undo
            if(CheckCollision(collisionHitboxes))
            {
                currentPos = prevPos;
                head.Offset(-dir);
                moveSuccessful = false;
            }

            if (moveSuccessful)
            {
                // If there was a turn, then we need to create a new part of the hitbox
                if (!ThreePointsOnSameLine(points.Last(), prevPos, currentPos))
                {
                    Rectangle hitboxPart = CreateHitbox(points.Last(), prevPos);
                    hitboxes.Add(hitboxPart);
                    points.Add(prevPos);
                }

                // If we are moving backwards and hit last point, then current point takes its place
                if (points.Count > 1 && points.Last() == currentPos)
                {
                    points.RemoveAt(points.Count - 1);
                    hitboxes.RemoveAt(hitboxes.Count - 1);
                }
            }

            // Re-create hitbox between last point and current point
            hitboxes.Add(CreateHitbox(points.Last(), currentPos));

            return moveSuccessful;
        }

        private Rectangle CreateHitbox(Point start, Point end)
        {
            int xLength = end.X - start.X;
            int yLength = end.Y - start.Y;

            Rectangle hitbox;

            // If line is vertical
            if (xLength == 0)
            {
                if (yLength > 0)
                    hitbox = new Rectangle(start.X, start.Y, LineWidth, yLength);
                else
                    hitbox = new Rectangle(end.X, end.Y + LineWidth, LineWidth, -yLength);
            }
            // If line is horizontal
            else
            {
                if (xLength > 0)
                    hitbox = new Rectangle(start.X, start.Y, xLength, LineWidth);
                else
                    hitbox = new Rectangle(end.X + LineWidth, end.Y, -xLength, LineWidth);
            }

            return hitbox;
        }

        private bool CheckCollision(IEnumerable<Rectangle> collisionHitboxes) => collisionHitboxes.Any(x => head.Intersects(x));
        private bool ThreePointsOnSameLine(Point p1, Point p2, Point p3) => (p1.Y - p2.Y) * (p1.X - p3.X) == (p1.Y - p3.Y) * (p1.X - p2.X);
    }
}
