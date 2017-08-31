using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public enum Sound { StartTracing, AbortTracing, Failure, Success, PotentialFailure, FinishTracing, AbortFinishTracing, EliminatorApply, MenuEnter, MenuEscape }
    public enum SoundLoop { Tracing, PathComplete, ScintStartPoint, ScintEndPoint }

    public static class SoundManager
    {
        private static SoundEffect sfxStartTracing, sfxAbortTracing, sfxFailure, sfxSuccess, sfxPotentialFailure, sfxFinishTracing, sfxAbortFinishTracing, sfxEliminator, sfxMenuEnter, sfxMenuEscape, sfxPathComplete, sfxScintStart, sfxScintEnd, sfxTracing;
        private static SoundEffectInstance sfxPathCompleteInst, sfxScintStartInst, sfxScintEndInst, sfxTracingInst;

        private static readonly Dictionary<Enum, float> defaultVolumes = new Dictionary<Enum, float>()
        {
            { Sound.StartTracing,           0.25f },
            { Sound.AbortTracing,           0.3f },
            { Sound.Failure,                0.2f },
            { Sound.Success,                0.4f },
            { Sound.PotentialFailure,       0.4f },
            { Sound.FinishTracing,          0.2f },
            { Sound.AbortFinishTracing,     0.2f },
            { Sound.EliminatorApply,        0.8f },
            { Sound.MenuEnter,              0.4f },
            { Sound.MenuEscape,             1f },
            { SoundLoop.PathComplete,       0.06f },
            { SoundLoop.ScintStartPoint,    1f },
            { SoundLoop.ScintEndPoint,      1f },
            { SoundLoop.Tracing,            0.04f }
        };

        public static bool Mute { get; set; } = false;
        private static float _masterVolume = 0.5f;
        public static float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = MathHelper.Clamp(value, 0, 1.0f);
        }

        public static void LoadContent(ContentManager Content)
        {
            sfxStartTracing = Content.Load<SoundEffect>("sfx/panel_start_tracing");
            sfxAbortTracing = Content.Load<SoundEffect>("sfx/panel_abort_tracing");
            sfxFailure = Content.Load<SoundEffect>("sfx/panel_failure");
            sfxSuccess= Content.Load<SoundEffect>("sfx/panel_success");
            sfxPotentialFailure = Content.Load<SoundEffect>("sfx/panel_potential_failure");
            sfxFinishTracing = Content.Load<SoundEffect>("sfx/panel_finish_tracing");
            sfxAbortFinishTracing = Content.Load<SoundEffect>("sfx/panel_abort_finish_tracing");
            sfxEliminator = Content.Load<SoundEffect>("sfx/eliminator_apply");
            sfxMenuEnter = Content.Load<SoundEffect>("sfx/menu_enter");
            sfxMenuEscape = Content.Load<SoundEffect>("sfx/menu_escape");
            sfxPathComplete = Content.Load<SoundEffect>("sfx/panel_path_complete");
            sfxScintStart = Content.Load<SoundEffect>("sfx/panel_scint_startpoint");
            sfxScintEnd = Content.Load<SoundEffect>("sfx/panel_scint_endpoint");
            sfxTracing = Content.Load<SoundEffect>("sfx/panel_tracing");

            sfxPathCompleteInst = sfxPathComplete.CreateInstance();
            sfxPathCompleteInst.IsLooped = true;
            sfxScintStartInst = sfxScintStart.CreateInstance();
            sfxScintStartInst.IsLooped = true;
            sfxScintEndInst = sfxScintEnd.CreateInstance();
            sfxScintEndInst.IsLooped = true;
            sfxTracingInst = sfxTracing.CreateInstance();
            sfxTracingInst.IsLooped = true;
        }

        public static void PlayOnce(Sound sound, float volume = 1f)
        {
            if (!Mute && MasterVolume > 0)
            {
                float volumeTweak = defaultVolumes[sound];
                SoundEffect soundToPlay = null;

                switch (sound)
                {
                    case Sound.StartTracing:        soundToPlay = sfxStartTracing;       break;
                    case Sound.AbortTracing:        soundToPlay = sfxAbortTracing;       break;
                    case Sound.Failure:             soundToPlay = sfxFailure;            break;
                    case Sound.Success:             soundToPlay = sfxSuccess;            break;
                    case Sound.PotentialFailure:    soundToPlay = sfxPotentialFailure;   break;
                    case Sound.FinishTracing:       soundToPlay = sfxFinishTracing;      break;
                    case Sound.AbortFinishTracing:  soundToPlay = sfxAbortFinishTracing; break;
                    case Sound.EliminatorApply:     soundToPlay = sfxEliminator;         break;
                    case Sound.MenuEnter:           soundToPlay = sfxMenuEnter;          break;
                    case Sound.MenuEscape:          soundToPlay = sfxMenuEscape;         break;
                }

                soundToPlay?.Play(volumeTweak * volume * MasterVolume, 0, 0);
            }
        }

        public static void PlayLoop(SoundLoop sound, float volume = 1f)
        {
            if(!Mute && MasterVolume > 0)
            {
                float volumeTweak = defaultVolumes[sound];
                switch (sound)
                {
                    case SoundLoop.PathComplete:
                        sfxPathCompleteInst.Volume = volumeTweak * volume * MasterVolume;
                        sfxPathCompleteInst.Play();
                        break;
                    case SoundLoop.ScintStartPoint:
                        sfxScintStartInst.Volume = volumeTweak * volume * MasterVolume;
                        sfxScintStartInst.Play();
                        break;
                    case SoundLoop.ScintEndPoint:
                        sfxScintEndInst.Volume = volumeTweak * volume * MasterVolume;
                        sfxScintEndInst.Play();
                        break;
                    case SoundLoop.Tracing:
                        sfxTracingInst.Volume = volumeTweak * volume * MasterVolume;
                        sfxTracingInst.Play();
                        break;
                }
            }
        }

        public static void StopLoop(SoundLoop sound)
        {
            switch (sound)
            {
                case SoundLoop.PathComplete:
                    if (sfxPathCompleteInst.State == SoundState.Playing)
                        sfxPathCompleteInst.Stop();
                    break;
                case SoundLoop.ScintStartPoint:
                    if (sfxScintStartInst.State == SoundState.Playing)
                        sfxScintStartInst.Stop();
                    break;
                case SoundLoop.ScintEndPoint:
                    if (sfxScintEndInst.State == SoundState.Playing)
                        sfxScintEndInst.Stop();
                    break;
                case SoundLoop.Tracing:
                    if (sfxTracingInst.State == SoundState.Playing)
                        sfxTracingInst.Stop();
                    break;
            }
        }

        public static void ChangeLoopVolume(SoundLoop sound, float volume)
        {
            float volumeTweak = defaultVolumes[sound];
            switch (sound)
            {
                case SoundLoop.PathComplete:
                    if (sfxPathCompleteInst.State == SoundState.Playing)
                        sfxPathCompleteInst.Volume = volumeTweak * volume * MasterVolume;
                    break;
                case SoundLoop.ScintStartPoint:
                    if (sfxScintStartInst.State == SoundState.Playing)
                        sfxScintStartInst.Volume = volumeTweak * volume * MasterVolume;
                    break;
                case SoundLoop.ScintEndPoint:
                    if (sfxScintEndInst.State == SoundState.Playing)
                        sfxScintEndInst.Volume = volumeTweak * volume * MasterVolume;
                    break;
                case SoundLoop.Tracing:
                    if (sfxTracingInst.State == SoundState.Playing)
                        sfxTracingInst.Volume = volumeTweak * volume * MasterVolume;
                    break;
            }
        }
    }
}
