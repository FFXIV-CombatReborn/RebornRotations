using System.ComponentModel;

namespace DefaultRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.0", Description = "Kindly created and donated by Rabbs")]
[SourceCode(Path = "main/DefaultRotations/Magical/PCT_Default.cs")]
[Api(3)]
public sealed class PCT_Default : PictomancerRotation
{
    public override MedicineType MedicineType => MedicineType.Intelligence;
    public static IBaseAction RainbowPrePull { get; } = new BaseAction((ActionID)34688);

    [RotationConfig(CombatType.PvE, Name = "Use HolyInWhite or CometInBlack while moving")]
    public bool HolyCometMoving { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Use swifcast on")]
    public MotifSwift MotifSwiftCast { get; set; } = MotifSwift.NoMotif;

    public enum MotifSwift : byte
    {

        [Description("CreatureMotif")] CreatureMotif,

        [Description("WeaponMotif")] WeaponMotif,

        [Description("LandscapeMotif")] LandscapeMotif,

        [Description("AllMotif")] AllMotif,

        [Description("NoMotif(ManualSwifcast")]
        NoMotif
    }

    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        IAction act;
        if (!InCombat)
        {
            if (!CreatureMotifDrawn)
            {
                if (PomMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return act;
                if (WingMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return act;
                if (ClawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return act;
                if (MawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return act;
            }
            if (!WeaponMotifDrawn)
            {
                if (HammerMotifPvE.CanUse(out act)) return act;
            }
            if (!LandscapeMotifDrawn)
            {
                if (StarrySkyMotifPvE.CanUse(out act) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return act;
            }
        }
        if (remainTime < RainbowDripPvE.Info.CastTime + CountDownAhead)
        {
            if (StrikingMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && WeaponMotifDrawn) return act;
        }
        if (remainTime < RainbowDripPvE.Info.CastTime + 0.4f + CountDownAhead)
        {
            if (RainbowPrePull.CanUse(out act, skipAoeCheck: true, skipCastingCheck: true, skipStatusProvideCheck: true)) return act;
        }
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Additional oGCD Logic

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat)
        {
            switch (MotifSwiftCast)
            {
                case MotifSwift.CreatureMotif:
                    if (nextGCD == PomMotifPvE || nextGCD == WingMotifPvE || nextGCD == MawMotifPvE || nextGCD == ClawMotifPvE)
                    {
                        if (SwiftcastPvE.CanUse(out act)) return true;
                    }
                    break;
                case MotifSwift.WeaponMotif:
                    if (nextGCD == HammerMotifPvE)
                    {
                        if (SwiftcastPvE.CanUse(out act)) return true;
                    }
                    break;
                case MotifSwift.LandscapeMotif:
                    if (nextGCD == StarrySkyMotifPvE)
                    {
                        if (SwiftcastPvE.CanUse(out act)) return true;
                    }
                    break;
                case MotifSwift.AllMotif:
                    if (nextGCD == PomMotifPvE || nextGCD == WingMotifPvE || nextGCD == MawMotifPvE || nextGCD == ClawMotifPvE)
                    {
                        if (SwiftcastPvE.CanUse(out act)) return true;
                    }
                    else
                    if (nextGCD == HammerMotifPvE)
                    {
                        if (SwiftcastPvE.CanUse(out act)) return true;
                    }
                    else
                    if (nextGCD == StarrySkyMotifPvE)
                    {
                        if (SwiftcastPvE.CanUse(out act)) return true;
                    }
                    break;
                case MotifSwift.NoMotif:
                    break;
            }
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.SmudgePvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (SmudgePvE.CanUse(out act)) return true;
        return base.AttackAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.AddlePvE, ActionID.TemperaCoatPvE, ActionID.TemperaGrassaPvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (AddlePvE.CanUse(out act)) return true;
        if (TemperaCoatPvE.CanUse(out act)) return true;
        if (TemperaGrassaPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (Player.HasStatus(true, StatusID.StarryMuse))
        {
            if (Player.HasStatus(true, StatusID.SubtractiveSpectrum) && !Player.HasStatus(true, StatusID.SubtractivePalette))
            {
                if (SubtractivePalettePvE.CanUse(out act)) return true;
            }

            if (CreatureMotifDrawn)
            {
                if (FangedMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && CreatureMotifDrawn && LivingMusePvE.AdjustedID == FangedMusePvE.ID) return true;
            }

            if (RetributionOfTheMadeenPvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && MadeenPortraitReady) return true;
        }

        if (!Player.HasStatus(true, StatusID.SubtractivePalette) && (PaletteGauge >= 50 || Player.HasStatus(true, StatusID.SubtractiveSpectrum)) && SubtractivePalettePvE.CanUse(out act)) return true;

        if (InCombat)
        {
            if (ScenicMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && LandscapeMotifDrawn && CreatureMotifDrawn && CombatTime > 5) return true;
            if (RetributionOfTheMadeenPvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && MadeenPortraitReady) return true;
            if (MogOfTheAgesPvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && MooglePortraitReady) return true;
            if (StrikingMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && WeaponMotifDrawn) return true;
            if (PomMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && CreatureMotifDrawn && LivingMusePvE.AdjustedID == PomMusePvE.ID) return true;
            if (WingedMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && CreatureMotifDrawn && LivingMusePvE.AdjustedID == WingedMusePvE.ID) return true;
            if (ClawedMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && CreatureMotifDrawn && LivingMusePvE.AdjustedID == ClawedMusePvE.ID) return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool MoveForwardGCD(out IAction? act)
    {
        act = null;

        return base.MoveForwardGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        bool IsTargetDying = HostileTarget?.IsDying() ?? false;

        if (CombatTime < 5)
        {
            if (StrikingMusePvE.CanUse(out act, skipCastingCheck: true, skipStatusProvideCheck: true, skipComboCheck: true, skipAoeCheck: true, usedUp: true) && WeaponMotifDrawn) return true;
            if (HolyInWhitePvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Paint > 0) return true;
            if (!CreatureMotifDrawn)
            {
                if (PomMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
                if (WingMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
                if (ClawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
                if (MawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
            }
        }

        if (Player.HasStatus(true, StatusID.StarryMuse))
        {
            if (CometInBlackPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Paint > 0 && Player.HasStatus(true, StatusID.MonochromeTones)) return true;
        }

        if (StarPrismPvE.CanUse(out act, skipAoeCheck: true) && Player.HasStatus(true, StatusID.Starstruck)) return true;

        if (RainbowDripPvE.CanUse(out act, skipAoeCheck: true) && Player.HasStatus(true, StatusID.RainbowBright)) return true;

        // white/black paint use while moving
        if (IsMoving)
        {
            if (HammerStampPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Player.HasStatus(true, StatusID.HammerTime) && InCombat) return true;
            if (HolyCometMoving)
            {
                if (CometInBlackPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Paint > 0 && Player.HasStatus(true, StatusID.MonochromeTones)) return true;
                if (HolyInWhitePvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Paint > 0) return true;
            }
        }

        if (HammerStampPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Player.HasStatus(true, StatusID.HammerTime) && InCombat) return true;

        if (!InCombat)
        {
            if (!CreatureMotifDrawn)
            {
                if (PomMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
                if (WingMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
                if (ClawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
                if (MawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
            }
            if (!WeaponMotifDrawn)
            {
                if (HammerMotifPvE.CanUse(out act)) return true;
            }
            if (!LandscapeMotifDrawn)
            {
                if (StarrySkyMotifPvE.CanUse(out act) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return true;
            }

            if (RainbowDripPvE.CanUse(out act)) return true;
        }

        if (InCombat && !Player.HasStatus(true, StatusID.StarryMuse) && !Player.HasStatus(true, StatusID.Hyperphantasia) && !IsTargetDying && (HasSwift || !HasHostilesInMaxRange) && (!CreatureMotifDrawn || !WeaponMotifDrawn && !Player.HasStatus(true, StatusID.HammerTime) || !LandscapeMotifDrawn))
        {
            bool swiftmode = HasSwift || !SwiftcastPvE.Cooldown.IsCoolingDown;
            switch (MotifSwiftCast)
            {
                case MotifSwift.CreatureMotif:
                    if (!CreatureMotifDrawn)
                    {
                        if (PomMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
                        if (WingMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
                        if (ClawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
                        if (MawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
                    }
                    if (!WeaponMotifDrawn && !Player.HasStatus(true, StatusID.HammerTime))
                    {
                        if (HammerMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CombatTime > 23) return true;
                    }
                    if (!LandscapeMotifDrawn)
                    {
                        if (StarrySkyMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return true;
                    }
                    break;
                case MotifSwift.WeaponMotif:
                    if (!WeaponMotifDrawn && !Player.HasStatus(true, StatusID.HammerTime))
                    {
                        if (HammerMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CombatTime > 23) return true;
                    }
                    if (!LandscapeMotifDrawn)
                    {
                        if (StarrySkyMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return true;
                    }
                    if (!CreatureMotifDrawn)
                    {
                        if (PomMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
                        if (WingMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
                        if (ClawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
                        if (MawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
                    }
                    break;
                case MotifSwift.LandscapeMotif:
                    if (!LandscapeMotifDrawn)
                    {
                        if (StarrySkyMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return true;
                    }
                    if (!WeaponMotifDrawn && !Player.HasStatus(true, StatusID.HammerTime))
                    {
                        if (HammerMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CombatTime > 23) return true;
                    }
                    if (!CreatureMotifDrawn)
                    {
                        if (PomMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
                        if (WingMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
                        if (ClawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
                        if (MawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
                    }
                    break;
                case MotifSwift.AllMotif:
                    if (!CreatureMotifDrawn)
                    {
                        if (PomMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
                        if (WingMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
                        if (ClawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
                        if (MawMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
                    }
                    if (!WeaponMotifDrawn && !Player.HasStatus(true, StatusID.HammerTime))
                    {
                        if (HammerMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && CombatTime > 23) return true;
                    }
                    if (!LandscapeMotifDrawn)
                    {
                        if (StarrySkyMotifPvE.CanUse(out act, skipCastingCheck: swiftmode) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return true;
                    }
                    break;
                case MotifSwift.NoMotif:
                    if (!LandscapeMotifDrawn)
                    {
                        if (StarrySkyMotifPvE.CanUse(out act) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return true;
                    }
                    if (!WeaponMotifDrawn && !Player.HasStatus(true, StatusID.HammerTime))
                    {
                        if (HammerMotifPvE.CanUse(out act) && CombatTime > 23) return true;
                    }
                    if (!CreatureMotifDrawn)
                    {
                        if (PomMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
                        if (WingMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
                        if (ClawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
                        if (MawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
                    }
                    break;
            }
        }

        if (!LandscapeMotifDrawn && ScenicMusePvE.Cooldown.RecastTimeRemainOneCharge <= 15 && !Player.HasStatus(true, StatusID.StarryMuse) && !Player.HasStatus(true, StatusID.Hyperphantasia))
        {
            if (StarrySkyMotifPvE.CanUse(out act) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return true;
        }
        if (!CreatureMotifDrawn && (LivingMusePvE.Cooldown.HasOneCharge || LivingMusePvE.Cooldown.RecastTimeRemainOneCharge <= CreatureMotifPvE.Info.CastTime) && !Player.HasStatus(true, StatusID.StarryMuse) && !Player.HasStatus(true, StatusID.Hyperphantasia))
        {
            if (PomMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == PomMotifPvE.ID) return true;
            if (WingMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == WingMotifPvE.ID) return true;
            if (ClawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == ClawMotifPvE.ID) return true;
            if (MawMotifPvE.CanUse(out act) && CreatureMotifPvE.AdjustedID == MawMotifPvE.ID) return true;
            ;
        }
        if (!WeaponMotifDrawn && !Player.HasStatus(true, StatusID.HammerTime) && (SteelMusePvE.Cooldown.HasOneCharge || SteelMusePvE.Cooldown.RecastTimeRemainOneCharge <= WeaponMotifPvE.Info.CastTime) && !Player.HasStatus(true, StatusID.StarryMuse) && !Player.HasStatus(true, StatusID.Hyperphantasia))
        {
            if (HammerMotifPvE.CanUse(out act)) return true;
        }

        //white paint over cap protection
        if (Paint == 5)
        {
            if (CometInBlackPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Paint > 0 && Player.HasStatus(true, StatusID.MonochromeTones)) return true;
            if (HolyInWhitePvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true) && Paint > 0) return true;
        }
        //aoe
        //
        if (ThunderIiInMagentaPvE.CanUse(out act) && Player.HasStatus(true, StatusID.SubtractivePalette) && Player.HasStatus(true, StatusID.AetherhuesIi)) return true;
        if (StoneIiInYellowPvE.CanUse(out act) && Player.HasStatus(true, StatusID.SubtractivePalette) && Player.HasStatus(true, StatusID.Aetherhues)) return true;
        if (BlizzardIiInCyanPvE.CanUse(out act) && Player.HasStatus(true, StatusID.SubtractivePalette)) return true;


        if (WaterIiInBluePvE.CanUse(out act) && Player.HasStatus(true, StatusID.AetherhuesIi)) return true;
        if (AeroIiInGreenPvE.CanUse(out act) && Player.HasStatus(true, StatusID.Aetherhues)) return true;
        if (FireIiInRedPvE.CanUse(out act)) return true;

        //single target
        //

        if (ThunderInMagentaPvE.CanUse(out act) && Player.HasStatus(true, StatusID.SubtractivePalette) && Player.HasStatus(true, StatusID.AetherhuesIi)) return true;
        if (StoneInYellowPvE.CanUse(out act) && Player.HasStatus(true, StatusID.SubtractivePalette) && Player.HasStatus(true, StatusID.Aetherhues)) return true;
        if (BlizzardInCyanPvE.CanUse(out act) && Player.HasStatus(true, StatusID.SubtractivePalette)) return true;


        if (WaterInBluePvE.CanUse(out act) && Player.HasStatus(true, StatusID.AetherhuesIi)) return true;
        if (AeroInGreenPvE.CanUse(out act) && Player.HasStatus(true, StatusID.Aetherhues)) return true;
        if (FireInRedPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    private bool AttackGCD(out IAction? act, bool burst)
    {
        act = null;

        return false;
    }
    #endregion

}