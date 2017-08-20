using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    [Flags]
    enum PanelStates
    {
        None = 0,
        Solved = 1,
        ErrorHappened = 2,
        ErrorBlink = 4,
        LineFade = 8,
        EliminationStarted = 16,
        EliminationFinished = 32
    }

    class PanelState
    {
        public PanelStates State { get; private set; }

        const int fadeOutErrorMaxTime = 400;
        const int fadeOutNeutralMaxTime = 80;
        float fadeOutTime = 0;
        public float LineFadeOpacity => fadeOutTime / (State.HasFlag(PanelStates.ErrorHappened) ? fadeOutErrorMaxTime : fadeOutNeutralMaxTime);
        public bool IsFading => fadeOutTime > 0;

        const int errorBlinkMaxTime = 600;
        int errorBlinkTime = 0;
        public float BlinkOpacity { get; private set; } = 1f;
        float blinkSpeed = 0.1f;
        bool blinkOpacityDown = true;

        const int eliminationTimeMax = 70;
        int eliminationTime = 0;
        const int eliminationFadeTimeMax = 80;
        float eliminationFadeTime = eliminationFadeTimeMax;
        public float EliminationFadeOpacity => eliminationFadeTime / eliminationFadeTimeMax;
        public event Action EliminationFinished;

        public PanelState()
        {
            State = PanelStates.None;
            EliminationFinished = new Action(EliminationFinishedHandler);
        }

        public void Update()
        {
            // If fadeOutTime > 0 and elimination either not started or already finished
            if(fadeOutTime > 0 
                && (!State.HasFlag(PanelStates.EliminationStarted) 
                    || (State.HasFlag(PanelStates.EliminationStarted | PanelStates.EliminationFinished))))
            {
                fadeOutTime--;
                if (fadeOutTime == 0)
                    State &= ~PanelStates.LineFade;
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

                if (errorBlinkTime == 0 && BlinkOpacity == 1f)
                    State &= ~PanelStates.ErrorBlink;
            }

            if(eliminationTime > 0)
            {
                eliminationTime--;
                if (eliminationTime == 0)
                    EliminationFinished.Invoke();
            }

            if(eliminationFadeTime < eliminationFadeTimeMax)
                eliminationFadeTime++;
        }

        public void InvokeFadeOut(bool isError)
        {
            if(isError)
            {
                State |= PanelStates.ErrorHappened | PanelStates.ErrorBlink | PanelStates.LineFade;
                fadeOutTime = fadeOutErrorMaxTime;
                errorBlinkTime = errorBlinkMaxTime;
            }
            else
            {
                State |= PanelStates.LineFade;
                fadeOutTime = fadeOutNeutralMaxTime;
            }
        }

        public void SetSuccess()
        {
            var tempState = PanelStates.Solved | (State & (PanelStates.EliminationFinished | PanelStates.EliminationStarted));
            ResetToNeutral();
            State = tempState;
        }

        public void ResetToNeutral()
        {
            State = PanelStates.None;
            fadeOutTime = 0;
            errorBlinkTime = 0;
            blinkOpacityDown = true;
            eliminationFadeTime = eliminationFadeTimeMax;
            eliminationTime = 0;
        }

        public void InitializeElimination()
        {
            State |= PanelStates.EliminationStarted;
            eliminationTime = eliminationTimeMax;
        }

        private void EliminationFinishedHandler()
        {
            State |= PanelStates.EliminationFinished;
            eliminationFadeTime = 0;
        }
    }
}
