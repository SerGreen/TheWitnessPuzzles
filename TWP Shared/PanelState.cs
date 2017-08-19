using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    enum PanelStates { Neutral, Error, Solved }

    class PanelState
    {
        public PanelStates State { get; private set; }

        int fadeOutErrorMaxTime = 400;
        int fadeOutNeutralMaxTime = 80;
        float fadeOutTime = 0;
        public float FadeOpacity => fadeOutTime / (State == PanelStates.Error ? errorBlinkMaxTime : fadeOutNeutralMaxTime);
        public bool IsFading => fadeOutTime > 0;

        int errorBlinkMaxTime = 400;
        int errorBlinkTime = 0;
        public float BlinkOpacity { get; private set; } = 1f;
        float blinkSpeed = 0.1f;
        bool blinkOpacityDown = true;

        public PanelState(PanelStates state = PanelStates.Neutral) => State = state;

        public void Update()
        {
            if(fadeOutTime > 0)
            {
                fadeOutTime--;
                if (fadeOutTime == 0)
                    State = PanelStates.Neutral;
            }

            if(errorBlinkTime > 0)
            {
                errorBlinkTime--;
                if (blinkOpacityDown)
                {
                    BlinkOpacity = Math.Max(0, BlinkOpacity - blinkSpeed);
                    if (BlinkOpacity == 0)
                        blinkOpacityDown = false;
                }
                else
                {
                    BlinkOpacity = Math.Min(1f, BlinkOpacity + blinkSpeed);
                    if (BlinkOpacity == 1f)
                        blinkOpacityDown = true;
                }

                if (errorBlinkTime == 0)
                    State = PanelStates.Neutral;
            }
        }

        public void InvokeFadeOut(bool isError)
        {
            if(isError)
            {
                State = PanelStates.Error;
                fadeOutTime = fadeOutErrorMaxTime;
                errorBlinkTime = errorBlinkMaxTime;
            }
            else
            {
                State = PanelStates.Neutral;
                fadeOutTime = fadeOutNeutralMaxTime;
            }
        }

        public void SetSuccess()
        {
            State = PanelStates.Solved;
        }

        public void ResetToNeutral()
        {
            State = PanelStates.Neutral;
            fadeOutTime = 0;
            errorBlinkTime = 0;
            blinkOpacityDown = true;
        }
    }
}
