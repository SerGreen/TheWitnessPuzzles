using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    class FadeInAnimation : FadeOutAnimation
    {
        public FadeInAnimation(int timeToFade) : base(timeToFade) { }

        public override float Opacity => 1f - base.Opacity;
    }
}
