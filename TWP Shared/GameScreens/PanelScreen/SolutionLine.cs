using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private Rectangle startCircle;
        private float startCircleScaleAnimation = 0.1f;

        public SolutionLine(Point start, int lineWidth, Rectangle startCircleBounds)
        {
            points.Add(start);
            prevPos = currentPos = start;
            Hitboxes = hitboxes.AsReadOnly();

            LineWidth = lineWidth;
            startCircle = startCircleBounds;

            hitboxes.Add(startCircle);

            head = new Rectangle(currentPos.X, currentPos.Y, LineWidth, LineWidth);
        }

        /// <summary>
        /// Transforms SolutionLine path to sequence of node IDs
        /// </summary>
        /// <param name="puzzleWidth">Puzzle width in blocks</param>
        /// <param name="puzzleZeroPoint">Top left corner of puzzle in pixels</param>
        /// <param name="nodePadding">Distance between two adjacent nodes in pixels</param>
        /// <returns>List of node IDs through which solution line passes</returns>
        public List<int> GetSolution(int puzzleWidth, Point puzzleZeroPoint, int nodePadding)
        {
            int width = puzzleWidth + 1;
            int PointToNodeId(Point point) => point.Y * width + point.X;

            List<int> solution = points.Append(currentPos).Select(x => PointToNodeId((x - puzzleZeroPoint).Divide(nodePadding))).ToList();
            // Crunch-fix ¯\_(ツ)_/¯
            if (solution.Count > 1 && solution[solution.Count - 1] == solution[solution.Count - 2])
                solution.RemoveAt(solution.Count - 1);

            for (int i = solution.Count - 2; i >= 0; i--)
            {
                int curID = solution[i];
                int nextID = solution[i + 1];

                // If two neighbour nodes are not adjacent (ID difference should be either 1 or (panel.width + 1)
                // Then fill the list with missing nodes
                int diff = nextID - curID;
                int diffAbs = Math.Abs(diff);
                if (diffAbs != width && diffAbs != 1)
                {
                    // Node ID that should be after curID
                    int desiredID;
                    int step;

                    // Vertical line
                    if (diffAbs > width)
                    {
                        step = diff > 0 ? width : -width;
                        desiredID = curID + step;
                    }
                    // Horizontal line
                    else
                    {
                        step = diff > 0 ? 1 : -1;
                        desiredID = curID + step;
                    }

                    // Fill the gap
                    while (solution[i + 1] != desiredID)
                        solution.Insert(i + 1, solution[i + 1] - step);
                }
            }

            return solution;
        }

        public bool Move(Vector2 moveVector, IEnumerable<Rectangle> collisionHitboxes)
        {
            bool moveSuccessful = true;

            // Remove fickle hitbox between last point and current point
            if (hitboxes.Count > 1)
                hitboxes.RemoveAt(hitboxes.Count - 1);

            // Remove start circle hitbox if line is just started, so it will not interfere it's own line motion
            bool startCircleWasRemoved = false;
            if (hitboxes.Count < 2)
            {
                hitboxes.RemoveAt(0);
                startCircleWasRemoved = true;
            }

            prevPos = currentPos;

            // Try to move
            currentPos += moveVector.ToPoint();
            head.Offset(moveVector);

            // If head hits anythig => Undo
            if(CheckCollision(collisionHitboxes))
            {
                currentPos = prevPos;
                head.Offset(-moveVector);
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

            // Re-insert start circle hitbox, so other line will not overlap
            if (startCircleWasRemoved)
                hitboxes.Insert(0, startCircle);

            return moveSuccessful;
        }
        
        /// <summary>
        /// When we tried moving with moveVector and hit a wall, check if there's a corner nearby
        /// </summary>
        /// <param name="moveVector">2D motion Vector</param>
        /// <param name="collisionHitboxes">Panel hitboxes, except the hitboxes of line itself</param>
        /// <returns>New motion vector that will move head towards corner</returns>
        public Vector2 GetMoveVectorNearCorner(Vector2 moveVector, IEnumerable<Rectangle> collisionHitboxes)
        {
            // We put small rectangles at corners to check if edge is near the head
            // Size of small rectangles
            int testSize = LineWidth / 6;
            
            // Technical flag (means that we've found a corner in first tried direction when X or Y was 0)
            bool zeroDir = true;

            // If we were trying moving horizontally, then check corners to the right or left
            if (Math.Abs(moveVector.X) > Math.Abs(moveVector.Y))
            {
                Rectangle down, up;

                // Were moving Right
                if (moveVector.X > 0)
                {
                    down = new Rectangle(head.X + head.Width, head.Y + head.Height - testSize, testSize, testSize);
                    up = new Rectangle(head.X + head.Width, head.Y, testSize, testSize);
                }
                // Were moving Left
                else
                {
                    down = new Rectangle(head.X - testSize, head.Y + head.Height - testSize, testSize, testSize);
                    up = new Rectangle(head.X - testSize, head.Y, testSize, testSize);
                }

                // Down
                if (moveVector.Y >= 0)
                {
                    foreach (var hitbox in collisionHitboxes)
                        if (down.Intersects(hitbox))
                            if (moveVector.Y == 0)
                            {
                                // If Y is 0 we should check other direction too
                                zeroDir = false;
                                break;
                            }
                            else
                                return Vector2.Zero;
                }
                // Up
                if (moveVector.Y <= 0)
                {
                    foreach (var hitbox in collisionHitboxes)
                        if (up.Intersects(hitbox))
                            if (moveVector.Y != 0 || !zeroDir)
                                return Vector2.Zero;
                            else
                                break;
                }

                // If Y was zero, but we found corner nearby, then make Y = 1 in the direction of corner
                if (moveVector.Y == 0)
                    if (zeroDir)
                        moveVector.Y = 1;
                    else
                        moveVector.Y = -1;

                // If we haven't triggered return yet, that means we have corner nearby
                return new Vector2(0, Math.Sign(moveVector.Y) * Math.Max(1, Math.Abs(moveVector.Y) / 2));
            }
            // If we were trying moving vertically, then check corners to the top or bottom
            else
            {
                Rectangle left, right;

                // Were moving Down
                if (moveVector.Y > 0)
                {
                    left = new Rectangle(head.X, head.Y + head.Height, testSize, testSize);
                    right = new Rectangle(head.X + head.Width - testSize, head.Y + head.Height, testSize, testSize);
                }
                // Were moving Up
                else
                {
                    left = new Rectangle(head.X, head.Y - testSize, testSize, testSize);
                    right = new Rectangle(head.X + head.Width - testSize, head.Y - testSize, testSize, testSize);
                }

                // Right
                if (moveVector.X >= 0)
                {
                    foreach (var hitbox in collisionHitboxes)
                        if (right.Intersects(hitbox))
                            if (moveVector.X == 0)
                            {
                                // If X is 0 we should check other direction too
                                zeroDir = false;
                                break;
                            }
                            else
                                return Vector2.Zero;
                }
                // Left
                if (moveVector.X <= 0)
                {
                    foreach (var hitbox in collisionHitboxes)
                        if (left.Intersects(hitbox))
                            if (moveVector.X != 0 || !zeroDir)
                                return Vector2.Zero;
                            else
                                break;
                }

                // If X was zero, but we found corner nearby, then make X = 1 in the direction of corner
                if (moveVector.X == 0)
                    if (zeroDir)
                        moveVector.X = 1;
                    else
                        moveVector.X = -1;

                // If we haven't triggered return yet, that means we have corner nearby
                return new Vector2(Math.Sign(moveVector.X) * Math.Max(1, Math.Abs(moveVector.X) / 2), 0);
            }
        }

        private Rectangle CreateHitbox(Point start, Point end, int extraLength = 0)
        {
            int xLength = end.X - start.X;
            int yLength = end.Y - start.Y;

            Rectangle hitbox;

            // If line is vertical
            if (xLength == 0)
            {
                if (yLength > 0) // Down
                    hitbox = new Rectangle(start.X, start.Y, LineWidth, yLength + extraLength);
                else             // Up
                    hitbox = new Rectangle(end.X, end.Y + LineWidth - extraLength, LineWidth, -yLength + extraLength);
            }
            // If line is horizontal
            else
            {
                if (xLength > 0) // Right
                    hitbox = new Rectangle(start.X, start.Y, xLength + extraLength, LineWidth);
                else             // Left
                    hitbox = new Rectangle(end.X + LineWidth - extraLength, end.Y, -xLength + extraLength, LineWidth);
            }

            return hitbox;
        }

        private bool CheckCollision(IEnumerable<Rectangle> collisionHitboxes) => collisionHitboxes.Any(x => head.Intersects(x));
        private bool ThreePointsOnSameLine(Point p1, Point p2, Point p3) => (p1.Y - p2.Y) * (p1.X - p3.X) == (p1.Y - p3.Y) * (p1.X - p2.X);

        public void Draw(SpriteBatch sb, Texture2D texCircle, Texture2D texPixel, Color? color = null)
        {
            // hitboxes[0] is the start circle hitbox
            for (int i = 1; i < hitboxes.Count; i++)
            {
                Point location;
                bool isHorizontal;
                if (hitboxes[i].Height == LineWidth && hitboxes[i].Width == LineWidth)
                    if (hitboxes[i].Y == head.Y)
                        isHorizontal = true;
                    else
                        isHorizontal = false;
                else if (hitboxes[i].Height == LineWidth)
                    isHorizontal = true;
                else
                    isHorizontal = false;

                if (isHorizontal)
                {
                    if (hitboxes[i].X < (i == hitboxes.Count - 1 ? head.X : hitboxes[i + 1].X))
                        location = new Point(hitboxes[i].Location.X + LineWidth / 2, hitboxes[i].Location.Y);
                    else
                        location = new Point(hitboxes[i].Location.X - LineWidth / 2, hitboxes[i].Location.Y);

                    sb.Draw(texCircle, new Rectangle(new Point(location.X - LineWidth / 2, location.Y), new Point(LineWidth)), color ?? Color.White);
                    sb.Draw(texCircle, new Rectangle(new Point(location.X + hitboxes[i].Width - LineWidth / 2, location.Y), new Point(LineWidth)), color ?? Color.White);
                }
                else
                {
                    if (hitboxes[i].Y < (i == hitboxes.Count - 1 ? head.Y : hitboxes[i + 1].Y))
                        location = new Point(hitboxes[i].Location.X, hitboxes[i].Location.Y + LineWidth / 2);
                    else
                        location = new Point(hitboxes[i].Location.X, hitboxes[i].Location.Y - LineWidth / 2);
                }

                sb.Draw(texPixel, new Rectangle(location, hitboxes[i].Size), color ?? Color.White);
            }

            sb.Draw(texCircle, head, color ?? Color.White);

            //sb.Draw(texCircle, startCircle, color ?? Color.White);
            float scale = (float) startCircle.Width / texCircle.Width;
            sb.Draw(texCircle, startCircle.Center.ToVector2(), null, color ?? Color.White, 0, texCircle.Bounds.Center.ToVector2(), scale * startCircleScaleAnimation, SpriteEffects.None, 0);
            if (startCircleScaleAnimation < 1.0f)
                startCircleScaleAnimation += 0.15f;
        }
    }
}
