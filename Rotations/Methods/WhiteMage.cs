﻿using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using ShinraCo.Settings;
using ShinraCo.Spells.Main;
using Resource = ff14bot.Managers.ActionResourceManager.WhiteMage;

namespace ShinraCo.Rotations
{
    public sealed partial class WhiteMage
    {
        private WhiteMageSpells MySpells { get; } = new WhiteMageSpells();

        #region Damage

        private async Task<bool> FluidAura()
        {
            if (!StopDamage && !Core.Player.CurrentTarget.IsBoss() && 
                !Core.Player.CurrentTarget.HasAura("Bind", false, 600))
            {
                return await MySpells.FluidAura.Cast();
            }
            return false;
        }

        
        private async Task<bool> Stone()
        {
            if (!ActionManager.HasSpell(MySpells.StoneII.Name) && !StopDamage)
            {
                return await MySpells.Stone.Cast();
            }
            return false;
        }

        private async Task<bool> StoneII()
        {
            if (!ActionManager.HasSpell(MySpells.StoneIII.Name) && !StopDamage)
            {
                return await MySpells.StoneII.Cast();
            }
            return false;
        }

        private async Task<bool> StoneIII()
        {
            if (!ActionManager.HasSpell(MySpells.StoneIV.Name) && !StopDamage)
            {
                return await MySpells.StoneIII.Cast();
            }
            return false;
        }

        private async Task<bool> StoneIV()
        {
            if (!ActionManager.HasSpell(MySpells.Glare.Name) && !StopDamage)
            {
                return await MySpells.StoneIV.Cast();
            }
            return false;
        }

        private async Task<bool> Glare()
        {
            if (!StopDamage)
            {
                return await MySpells.Glare.Cast();
            }
            return false;
        }
        
        #endregion

        #region DoT

        private async Task<bool> Aero()
        {
            if (!ActionManager.HasSpell(MySpells.AeroII.Name) && !StopDots &&
                !Core.Player.CurrentTarget.HasAura(MySpells.Aero.Name, true, 3000))
            {
                return await MySpells.Aero.Cast();
            }
            return false;
        }

        private async Task<bool> AeroII()
        {
            if (!StopDots && !Core.Player.CurrentTarget.HasAura(MySpells.AeroII.Name, true, 3000))
            {
                return await MySpells.AeroII.Cast();
            }
            return false;
        }

        private async Task<bool> Dia()
        {
            if (!StopDots && !Core.Player.CurrentTarget.HasAura(MySpells.Dia.Name, true, 3000))
            {
                return await MySpells.Dia.Cast();
            }
            return false;
        }
        
        #endregion

        #region AoE

        private async Task<bool> Holy()
        {
            var count = ShinraEx.Settings.CustomAoE ? ShinraEx.Settings.CustomAoECount : 3;

            if (!MovementManager.IsMoving && (ShinraEx.Settings.RotationMode == Modes.Multi || Helpers.EnemiesNearPlayer(8) >= count))
            {
                if (ShinraEx.Settings.WhiteMageThinAir && ActionManager.CanCast(MySpells.Holy.Name, Core.Player))
                {
                    if (await MySpells.ThinAir.Cast(null, false))
                    {
                        await Coroutine.Wait(3000, () => Core.Player.HasAura(MySpells.ThinAir.Name));
                    }
                }
                if (!StopDamage) return await MySpells.Holy.Cast();
            }
            return false;
        }
        
        private async Task<bool> AfflatusMisery()
        {
            var count = ShinraEx.Settings.CustomAoE ? ShinraEx.Settings.CustomAoECount : 3;
            
            // This require bloodlilies to be implemented
            if (!MovementManager.IsMoving && (ShinraEx.Settings.RotationMode == Modes.Multi || Helpers.EnemiesNearPlayer(5) >= count))
            {
                if (ShinraEx.Settings.WhiteMageThinAir && ActionManager.CanCast(MySpells.AfflatusMisery.Name, Core.Player))
                {
                    if (await MySpells.ThinAir.Cast(null, false))
                    {
                        await Coroutine.Wait(3000, () => Core.Player.HasAura(MySpells.ThinAir.Name));
                    }
                }
                if (!StopDamage) return await MySpells.AfflatusMisery.Cast();
            }
            
            return false;
        }
        
        #endregion

        #region Buff

        private async Task<bool> PresenceOfMind()
        {
            if (ShinraEx.Settings.WhiteMagePartyHeal && ShinraEx.Settings.WhiteMagePresenceOfMind)
            {
                if (Helpers.HealManager.Count(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMagePresenceOfMindPct) >=
                    ShinraEx.Settings.WhiteMagePresenceOfMindCount)
                {
                    return await MySpells.PresenceOfMind.Cast(null, false);
                }
            }
            return false;
        }

        private async Task<bool> Temperance()
        {
            if (ShinraEx.Settings.WhiteMagePartyHeal && ShinraEx.Settings.WhiteMageTemperance)
            {
                if (Helpers.HealManager.Count(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageTemperancePct) >=
                    ShinraEx.Settings.WhiteMageTemperanceCount)
                {
                    return await MySpells.Temperance.Cast(null, false);
                }
            }
            return false;
        }
        
        #endregion

        #region Heal

        private async Task<bool> UpdateHealing()
        {
            return ShinraEx.Settings.WhiteMagePartyHeal && !await Helpers.UpdateHealManager();
        }

        private async Task<bool> StopCasting()
        {
            if (ShinraEx.Settings.WhiteMageInterruptOverheal && Core.Player.IsCasting)
            {
                var target = GameObjectManager.GetObjectByObjectId(Core.Player.SpellCastInfo.TargetId);
                var spellName = Core.Player.SpellCastInfo.Name;
                var freeCure = Core.Player.HasAura(155) ? ShinraEx.Settings.WhiteMageCurePct : ShinraEx.Settings.WhiteMageCureIIPct;

                if (target != null)
                {
                    if (spellName == MySpells.Cure.Name && target.CurrentHealthPercent >= ShinraEx.Settings.WhiteMageCurePct + 10 ||
                        spellName == MySpells.CureII.Name && target.CurrentHealthPercent >= freeCure + 10 ||
                        spellName == MySpells.Raise.Name && target.HasAura("Raise"))
                    {
                        var debugSetting = spellName == MySpells.Cure.Name ? ShinraEx.Settings.WhiteMageCurePct
                            : ShinraEx.Settings.WhiteMageCureIIPct;
                        Helpers.Debug($@"Target HP: {target.CurrentHealthPercent}, Setting: {debugSetting}, Adjusted: {debugSetting + 10}");
                        Logging.Write(Colors.Yellow, $@"[ShinraEx] Interrupting >>> {spellName}");
                        ActionManager.StopCasting();
                        await Coroutine.Wait(500, () => !Core.Player.IsCasting);
                    }
                }
            }
            return false;
        }

        private async Task<bool> Cure()
        {
            if (ShinraEx.Settings.WhiteMageCure)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageCurePct)
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageCurePct ? Core.Player : null;

                if (target != null)
                {
                    if (ShinraEx.Settings.WhiteMageCureII && Core.Player.HasAura(155))
                    {
                        return await MySpells.CureII.Cast(target);
                    }
                    return await MySpells.Cure.Cast(target);
                }
            }
            return false;
        }

        private async Task<bool> CureII()
        {
            if (ShinraEx.Settings.WhiteMageCureII)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageCureIIPct)
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageCureIIPct ? Core.Player : null;

                if (target != null)
                {
                    return await MySpells.CureII.Cast(target);
                }
            }
            return false;
        }

        private async Task<bool> CureIII()
        {
            if (ShinraEx.Settings.WhiteMageCureIII && ShinraEx.Settings.WhiteMagePartyHeal && UseAoEHeals)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageCureIIIPct)
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageCureIIIPct ? Core.Player : null;

                var count = Helpers.FriendsNearPlayer(ShinraEx.Settings.WhiteMageCureIIIPct);

                if (target != null && count > 2)
                {
                    return await MySpells.CureIII.Cast(target);
                }
            }
            return false;
        }
        
        private async Task<bool> Tetragrammaton()
        {
            if (ShinraEx.Settings.WhiteMageTetragrammaton)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageTetragrammatonPct)
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageTetragrammatonPct ? Core.Player : null;

                if (target != null)
                {
                    return await MySpells.Tetragrammaton.Cast(target, false);
                }
            }
            return false;
        }

        private async Task<bool> Benediction()
        {
            if (ShinraEx.Settings.WhiteMageBenediction)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageBenedictionPct)
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageBenedictionPct ? Core.Player : null;

                if (target != null)
                {
                    return await MySpells.Benediction.Cast(target, false);
                }
            }
            return false;
        }

        private async Task<bool> Regen()
        {
            if (ShinraEx.Settings.WhiteMageRegen)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageRegenPct &&
                                                               !hm.HasAura(MySpells.Regen.Name))
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageRegenPct && !Core.Player.HasAura(MySpells.Regen.Name) 
                        ? Core.Player : null;

                if (target != null)
                {
                    return await MySpells.Regen.Cast(target);
                }
            }
            return false;
        }

        private async Task<bool> Medica()
        {
            if (ShinraEx.Settings.WhiteMageMedica && ShinraEx.Settings.WhiteMagePartyHeal && UseAoEHeals)
            {
                var count = Helpers.FriendsNearPlayer(ShinraEx.Settings.WhiteMageMedicaPct);

                if (count > 2)
                {
                    return await MySpells.Medica.Cast();
                }
            }
            return false;
        }

        private async Task<bool> MedicaII()
        {
            if (ShinraEx.Settings.WhiteMageMedicaII && ShinraEx.Settings.WhiteMagePartyHeal && UseAoEHeals &&
                !Core.Player.HasAura(MySpells.MedicaII.Name, true))
            {
                var count = Helpers.FriendsNearPlayer(ShinraEx.Settings.WhiteMageMedicaIIPct);

                if (count > 2)
                {
                    return await MySpells.MedicaII.Cast();
                }
            }
            return false;
        }

        private async Task<bool> AfflatusSolace()
        {
            if (ShinraEx.Settings.WhiteMageAfflatusSolace && ShinraEx.Settings.WhiteMagePartyHeal && Resource.Lily >= 1)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageAfflatusSolacePct)
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageAfflatusSolacePct ? Core.Player : null;
                
                if (target != null)
                {
                    return await MySpells.AfflatusSolace.Cast(target);
                }
            }
            return false;
        }
        
        private async Task<bool> AfflatusRapture()
        {
            if (ShinraEx.Settings.WhiteMageAfflatusRapture && ShinraEx.Settings.WhiteMagePartyHeal && UseAoEHeals &&
                Resource.Lily >= 1)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal
                    ? Helpers.HealManager.FirstOrDefault(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageAfflatusRapturePct)
                    : Core.Player.CurrentHealthPercent < ShinraEx.Settings.WhiteMageAfflatusRapturePct ? Core.Player : null;

                if (target != null)
                {
                    return await MySpells.AfflatusRapture.Cast(target);
                }
            }
            return false;
        }
        
        private async Task<bool> Assize()
        {
            if (ShinraEx.Settings.WhiteMageAssize && ShinraEx.Settings.WhiteMagePartyHeal && UseAoEHeals && 
                Core.Player.CurrentManaPercent < 85)
            {
                var count = Helpers.FriendsNearPlayer(ShinraEx.Settings.WhiteMageAssizePct);

                if (count > 2)
                {
                    return await MySpells.Assize.Cast(null, false);
                }
            }
            return false;
        }

        private async Task<bool> PlenaryIndulgence()
        {
            if (ShinraEx.Settings.WhiteMagePlenary && ShinraEx.Settings.WhiteMagePartyHeal && UseAoEHeals)
            {
                var count = Helpers.FriendsNearPlayer(ShinraEx.Settings.WhiteMagePlenaryPct);

                if (count > 2)
                {
                    return await MySpells.PlenaryIndulgence.Cast(null, false);
                }
            }
            return false;
        }

        private async Task<bool> Raise()
        {
            if (ShinraEx.Settings.WhiteMageRaise &&
                (ShinraEx.Settings.WhiteMageSwiftcast && ActionManager.CanCast(MySpells.Role.Swiftcast.Name, Core.Player) ||
                 !Helpers.HealManager.Any(hm => hm.CurrentHealthPercent < ShinraEx.Settings.WhiteMageCurePct)))
            {
                var target = Helpers.RessManager.FirstOrDefault(pm => !pm.HasAura("Raise"));

                if (target != null)
                {
                    if (ShinraEx.Settings.WhiteMageSwiftcast && ActionManager.CanCast(MySpells.Raise.Name, target))
                    {
                        if (await MySpells.Role.Swiftcast.Cast(null, false))
                        {
                            await Coroutine.Wait(1000, () => Core.Player.HasAura(MySpells.Role.Swiftcast.Name));
                        }
                    }
                    return await MySpells.Raise.Cast(target);
                }
            }
            return false;
        }

        #endregion

        #region Role

        private async Task<bool> Esuna()
        {
            if (ShinraEx.Settings.WhiteMageEsuna)
            {
                var target = ShinraEx.Settings.WhiteMagePartyHeal ? Helpers.HealManager.FirstOrDefault(hm => hm.HasDispellable())
                    : Core.Player.HasDispellable() ? Core.Player : null;

                if (target != null)
                {
                    return await MySpells.Role.Esuna.Cast(target);
                }
            }
            return false;
        }

        private async Task<bool> LucidDreaming()
        {
            if (ShinraEx.Settings.WhiteMageLucidDreaming && Core.Player.CurrentManaPercent < ShinraEx.Settings.WhiteMageLucidDreamingPct)
            {
                return await MySpells.Role.LucidDreaming.Cast(null, false);
            }
            return false;
        }
        
        #endregion

        #region Custom

        private bool StopDamage => ShinraEx.Settings.WhiteMageStopDamage && !Core.Player.HasAura(MySpells.ThinAir.Name) &&
                                   Core.Player.CurrentManaPercent <= ShinraEx.Settings.WhiteMageStopDamagePct;

        private bool StopDots => ShinraEx.Settings.WhiteMageStopDots && !Core.Player.HasAura(MySpells.ThinAir.Name) &&
                                 Core.Player.CurrentManaPercent <= ShinraEx.Settings.WhiteMageStopDotsPct;

        private bool UseAoEHeals => ShinraEx.LastSpell.Name != MySpells.Medica.Name && ShinraEx.LastSpell.Name != MySpells.MedicaII.Name &&
                                    ShinraEx.LastSpell.Name != MySpells.Assize.Name &&
                                    ShinraEx.LastSpell.Name != MySpells.PlenaryIndulgence.Name;

        #endregion
    }
}