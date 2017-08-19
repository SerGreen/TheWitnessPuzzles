using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using TheWitnessPuzzles;
using TWP_Shared;
using Microsoft.Xna.Framework.Input.Touch;

namespace TWP_Android
{
    public class TWPGameAndroid : TWPGame
    {
        List<GestureSample> gestures;

        public TWPGameAndroid(Puzzle panel = null) : base(true, panel)
        {
        }

        protected override void Update(GameTime gameTime)
        {
            gestures = GetGestures().ToList();
            base.Update(gameTime);
        }

        private IEnumerable<GestureSample> GetGestures()
        {
            while (TouchPanel.IsGestureAvailable)
                yield return TouchPanel.ReadGesture();
        }

        protected override Vector2 GetMoveVector()
        {
            Vector2 result = Vector2.Zero;

            foreach (var gesture in gestures.Where(x => x.GestureType == GestureType.FreeDrag))
                result += gesture.Delta * moveSensitivity;

            return result;
        }

        protected override Point? GetTapPosition()
        {
            try
            {
                return gestures.First(x => x.GestureType == GestureType.Tap).Position.ToPoint();
            }
            catch (InvalidOperationException) { }

            return null;
        }
    }
}