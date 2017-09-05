using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public enum Sound { StartTracing, AbortTracing, Failure, Success, PotentialFailure, FinishTracing, AbortFinishTracing, EliminatorApply, MenuEnter, MenuEscape, MenuOpen, MenuTick, MenuUntick, ButtonLike, ButtonUnlike, ButtonNext, ButtonNextSuccess }
    public enum SoundLoop { Tracing, PathComplete }

    public static class SoundManager
    {
        private static SoundEffect sfxStartTracing, sfxAbortTracing, sfxFailure, sfxSuccess, sfxPotentialFailure, sfxFinishTracing, sfxAbortFinishTracing, sfxEliminator, sfxMenuEnter, sfxMenuEscape, sfxPathComplete, sfxTracing, sfxMenuOpen, sfxMenuTick, sfxMenuUntick, sfxLike, sfxUnlike, sfxNext, sfxNextSuccess;
        private static SoundEffectInstance sfxPathCompleteInst, sfxTracingInst;

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
            { Sound.MenuOpen,               0.4f },
            { Sound.MenuTick,               1f },
            { Sound.MenuUntick,             1f },
            { Sound.ButtonLike,             0.8f },
            { Sound.ButtonUnlike,           0.8f },
            { Sound.ButtonNext,             0.9f },
            { Sound.ButtonNextSuccess,      0.25f },
            { SoundLoop.PathComplete,       0.06f },
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
            sfxStartTracing =       Content.Load<SoundEffect>("sfx/panel_start_tracing");
            sfxAbortTracing =       Content.Load<SoundEffect>("sfx/panel_abort_tracing");
            sfxFailure =            Content.Load<SoundEffect>("sfx/panel_failure");
            sfxSuccess =            Content.Load<SoundEffect>("sfx/panel_success");
            sfxPotentialFailure =   Content.Load<SoundEffect>("sfx/panel_potential_failure");
            sfxFinishTracing =      Content.Load<SoundEffect>("sfx/panel_finish_tracing");
            sfxAbortFinishTracing = Content.Load<SoundEffect>("sfx/panel_abort_finish_tracing");
            sfxEliminator =         Content.Load<SoundEffect>("sfx/eliminator_apply");
            sfxMenuEnter =          Content.Load<SoundEffect>("sfx/menu_enter");
            sfxMenuEscape =         Content.Load<SoundEffect>("sfx/menu_escape");
            sfxMenuOpen =           Content.Load<SoundEffect>("sfx/menu_open");
            sfxMenuTick =           Content.Load<SoundEffect>("sfx/menu_tick");
            sfxMenuUntick =         Content.Load<SoundEffect>("sfx/menu_untick");
            sfxLike =               Content.Load<SoundEffect>("sfx/btn_like");
            sfxUnlike =             Content.Load<SoundEffect>("sfx/btn_unlike");
            sfxNext =               Content.Load<SoundEffect>("sfx/btn_next_panel");
            sfxNextSuccess =        Content.Load<SoundEffect>("sfx/btn_next_panel_success");
            sfxPathComplete =       Content.Load<SoundEffect>("sfx/panel_path_complete");
            sfxTracing =            Content.Load<SoundEffect>("sfx/panel_tracing");

            sfxPathCompleteInst = sfxPathComplete.CreateInstance();
            sfxPathCompleteInst.IsLooped = true;
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
                    case Sound.StartTracing:        soundToPlay = sfxStartTracing;          break;
                    case Sound.AbortTracing:        soundToPlay = sfxAbortTracing;          break;
                    case Sound.Failure:             soundToPlay = sfxFailure;               break;
                    case Sound.Success:             soundToPlay = sfxSuccess;               break;
                    case Sound.PotentialFailure:    soundToPlay = sfxPotentialFailure;      break;
                    case Sound.FinishTracing:       soundToPlay = sfxFinishTracing;         break;
                    case Sound.AbortFinishTracing:  soundToPlay = sfxAbortFinishTracing;    break;
                    case Sound.EliminatorApply:     soundToPlay = sfxEliminator;            break;
                    case Sound.MenuEnter:           soundToPlay = sfxMenuEnter;             break;
                    case Sound.MenuEscape:          soundToPlay = sfxMenuEscape;            break;
                    case Sound.MenuOpen:            soundToPlay = sfxMenuOpen;              break;
                    case Sound.MenuTick:            soundToPlay = sfxMenuTick;              break;
                    case Sound.MenuUntick:          soundToPlay = sfxMenuUntick;            break;
                    case Sound.ButtonLike:          soundToPlay = sfxLike;                  break;
                    case Sound.ButtonUnlike:        soundToPlay = sfxUnlike;                break;
                    case Sound.ButtonNext:          soundToPlay = sfxNext;                  break;
                    case Sound.ButtonNextSuccess:   soundToPlay = sfxNextSuccess;           break;
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
                    case SoundLoop.Tracing:
                        sfxTracingInst.Volume = volumeTweak * volume * MasterVolume;
                        sfxTracingInst.Play();
                        break;
                }
            }
        }

        public static bool IsPlaying(SoundLoop sound)
        {
            switch (sound)
            {
                case SoundLoop.Tracing:
                    return sfxTracingInst.State == SoundState.Playing;
                case SoundLoop.PathComplete:
                    return sfxPathCompleteInst.State == SoundState.Playing;
                default:
                    return false;
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
                case SoundLoop.Tracing:
                    if (sfxTracingInst.State == SoundState.Playing)
                        sfxTracingInst.Volume = volumeTweak * volume * MasterVolume;
                    break;
            }
        }
    }
}
