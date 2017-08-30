using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public class FadeOutAnimation
    {
        readonly int timeMax;
        readonly int timeBeforeFade;
        float time;
        bool isFading;

        public virtual float Opacity => isFading ? time / timeMax : 1f;
        public bool IsActive { get; private set; }

        public event Action FadeComplete;

        public FadeOutAnimation(int timeToFade, int timeToWaitBeforeFade = 0)
        {
            timeMax = Math.Max(1, timeToFade);
            timeBeforeFade = Math.Max(0, timeToWaitBeforeFade);
            time = timeBeforeFade > 0 ? timeBeforeFade : timeMax;
            isFading = false;
            IsActive = false;
        }

        public void Restart()
        {
            time = timeBeforeFade > 0 ? timeBeforeFade : timeMax;
            isFading = timeBeforeFade == 0;
            IsActive = true;
        }

        public void Update()
        {
            if(IsActive)
            {
                if (time > 0)
                    time--;

                if (time == 0)
                {
                    if (isFading)
                    {
                        IsActive = false;
                        FadeComplete?.Invoke();
                    }
                    else
                    {
                        isFading = true;
                        time = timeMax;
                    }
                }
            }
        }
    }
}
