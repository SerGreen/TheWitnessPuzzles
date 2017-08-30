using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public class FadeTransition
    {
        readonly int timeFadeOut;
        readonly int timeFadeIn;
        readonly int timeBlack;
        float time;
        bool fadingOut;
        bool hold;

        public bool IsActive { get; private set; }
        public float Opacity => hold ? 1f : (fadingOut ? time / timeFadeOut : time / timeFadeIn);

        public event Action FadeOutComplete;

        public FadeTransition(int timeToFadeOut, int timeToFadeIn, int timeInBlackState = 0)
        {
            timeFadeOut = timeToFadeOut;
            timeFadeIn = timeToFadeIn;
            timeBlack = timeInBlackState;
        }
        
        public void Restart()
        {
            IsActive = true;
            time = 0;
            fadingOut = true;
            hold = false;
        }

        public void Update()
        {
            if (IsActive)
            {
                if (fadingOut)
                {
                    if (time < timeFadeOut)
                        time++;

                    if(time == timeFadeOut)
                    {
                        fadingOut = false;
                        hold = true;
                        time = timeBlack;
                        FadeOutComplete?.Invoke();
                    }
                }
                else if(hold)
                {
                    if (time > 0)
                        time--;

                    if(time == 0)
                    {
                        time = timeFadeIn;
                        hold = false;
                    }
                }
                else
                {
                    if (time > 0)
                        time--;

                    if(time == 0)
                    {
                        IsActive = false;
                    }
                }
            }
        }
    }
}
