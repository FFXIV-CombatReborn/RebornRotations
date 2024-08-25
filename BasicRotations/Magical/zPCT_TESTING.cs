namespace DefaultRotations.Magical;

[Rotation("zPCT TESTING ONLY", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/DefaultRotations/Magical/zPCT_TESTING.cs")]
[Api(3)]
public sealed class zPCT_TESTING : PictomancerRotation
{
    [RotationConfig(CombatType.PvE, Name = "Use HolyInWhite or CometInBlack while moving")]
    public bool HolyCometMoving { get; set; } = true;

    [Range(1, 5, ConfigUnitType.None, 1)]
    [RotationConfig(CombatType.PvE, Name = "Paint overcap protection. How many paint do you need to be at before using a paint? (Setting is ignored when you have Hyperphantasia)")]
    public int HolyCometMax { get; set; } = 5;

    [RotationConfig(CombatType.PvE, Name = "Use swiftcast on Rainbow Drip (Priority over below settings)")]
    public bool RainbowDripSwift { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Use swiftcast on Motif")]
    public bool MotifSwiftCastSwift { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Which Motif")]
    public CanvasFlags MotifSwiftCast { get; set; } = CanvasFlags.Pom;

    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        IAction act;
        if (!InCombat)
        {
            if (!CreatureMotifDrawn)
            {
                if (PomMotifPvE.CanUse(out act, skipCastingCheck: true)) return act;
            }
            if (!WeaponMotifDrawn)
            {
                if (HammerMotifPvE.CanUse(out act, skipCastingCheck: true)) return act;
            }
            if (!LandscapeMotifDrawn)
            {
                if (StarrySkyMotifPvE.CanUse(out act, skipCastingCheck: true) && !Player.HasStatus(true, StatusID.Hyperphantasia)) return act;
            }
        }
        if (remainTime < RainbowDripPvE.Info.CastTime + CountDownAhead)
        {
            if (StrikingMusePvE.CanUse(out act, skipCastingCheck: true) && WeaponMotifDrawn) return act;
        }

        if (remainTime < RainbowDripPvE.Info.CastTime + 0.4f + CountDownAhead)
        {
            if (RainbowDripPvE.CanUse(out act, skipCastingCheck: true)) return act;
        }

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Additional oGCD Logic

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat)
        {
            if (RainbowDripSwift)
            {
                if (nextGCD == RainbowDripPvE)
                {
                    if (SwiftcastPvE.CanUse(out act)) return true;
                }
            }
        }

        if (InCombat)
        {
            if (MotifSwiftCastSwift)
            {
                switch (MotifSwiftCast)
                {
                    case CanvasFlags.Pom:
                        if (nextGCD == PomMotifPvE)
                        {
                            if (SwiftcastPvE.CanUse(out act)) return true;
                        }
                        break;
                    case CanvasFlags.Wing:
                        if (nextGCD == WingMotifPvE)
                        {
                            if (SwiftcastPvE.CanUse(out act)) return true;
                        }
                        break;
                    case CanvasFlags.Claw:
                        if (nextGCD == ClawMotifPvE)
                        {
                            if (SwiftcastPvE.CanUse(out act)) return true;
                        }
                        break;
                    case CanvasFlags.Maw:
                        if (nextGCD == MawMotifPvE)
                        {
                            if (SwiftcastPvE.CanUse(out act)) return true;
                        }
                        break;
                    case CanvasFlags.Weapon:
                        if (nextGCD == HammerMotifPvE)
                        {
                            if (SwiftcastPvE.CanUse(out act)) return true;
                        }
                        break;
                    case CanvasFlags.Landscape:
                        if (nextGCD == StarrySkyMotifPvE)
                        {
                            if (SwiftcastPvE.CanUse(out act)) return true;
                        }
                        break;
                }
            }
        }
        return base.EmergencyAbility(nextGCD, out act);
    }


    [RotationDesc(ActionID.SmudgePvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {

        if (SmudgePvE.CanUse(out act)) return true;

        return base.MoveForwardAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.TemperaCoatPvE, ActionID.TemperaGrassaPvE, ActionID.AddlePvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        // Mitigations
        if (TemperaCoatPvE.CanUse(out act)) return true;
        if (TemperaGrassaPvE.CanUse(out act)) return true;
        if (AddlePvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    #endregion

    #region oGCD Logic

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        // Bursts
        if (SubtractivePalettePvE.CanUse(out act)) return true;
        if (RetributionOfTheMadeenPvE.CanUse(out act)) return true;
        if (FangedMusePvE.CanUse(out act, usedUp: true)) return true;
        if (ClawedMusePvE.CanUse(out act, usedUp: true)) return true;

        //Expert Muses
        if (MogOfTheAgesPvE.CanUse(out act)) return true;
        if (WingedMusePvE.CanUse(out act, usedUp: true)) return true;

        //Advanced Muses
        if (StarryMusePvE.CanUse(out act)) return true;
        if (StrikingMusePvE.CanUse(out act)) return true;
        if (ClawedMusePvE.CanUse(out act, usedUp: true)) return true;
        if (WingedMusePvE.CanUse(out act, usedUp: true)) return true;
        if (PomMusePvE.CanUse(out act, usedUp: true)) return true;

        //Basic Muses
        //if (ScenicMusePvE.CanUse(out act)) return true;
        //if (SteelMusePvE.CanUse(out act, usedUp: true)) return true;
        //if (LivingMusePvE.CanUse(out act, usedUp: true)) return true;
        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    protected override bool GeneralGCD(out IAction? act)
    {
        // Weapon Painting Burst
        if (HammerStampPvE.CanUse(out act, skipCastingCheck: true, skipComboCheck: true)) return true;

        if (HolyCometMoving && IsMoving)
        {
            if (HolyInWhitePvE.CanUse(out act, skipCastingCheck: true)) return true;
        }

        //Use up paint if in Hyperphantasia
        if (Player.HasStatus(true, StatusID.Hyperphantasia))
        {
            if (CometInBlackPvE.CanUse(out act, skipCastingCheck: true)) return true;
        }

        //Paint overcap protection
        if (Paint == HolyCometMax)
        {
            if (CometInBlackPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (HolyInWhitePvE.CanUse(out act, skipCastingCheck: true)) return true;
        }

        // Landscape Paining Burst
        if (RainbowDripPvE.CanUse(out act)) return true;
        if (StarPrismPvE.CanUse(out act, skipCastingCheck: true)) return true;

        //Advanced Paintings
        if (StarrySkyMotifPvE.CanUse(out act)) return true;
        if (HammerMotifPvE.CanUse(out act)) return true;
        if (MawMotifPvE.CanUse(out act)) return true;
        if (ClawMotifPvE.CanUse(out act)) return true;
        if (WingMotifPvE.CanUse(out act)) return true;
        if (PomMotifPvE.CanUse(out act)) return true;

        //Basic Paintings
        //if (LandscapeMotifPvE.CanUse(out act)) return true;
        //if (WeaponMotifPvE.CanUse(out act)) return true;
        //if (CreatureMotifPvE.CanUse(out act)) return true;

        //AOE Subtractive Inks
        if (ThunderIiInMagentaPvE.CanUse(out act)) return true;
        if (StoneIiInYellowPvE.CanUse(out act)) return true;
        if (BlizzardIiInCyanPvE.CanUse(out act)) return true;

        //AOE Additive Inks
        if (WaterIiInBluePvE.CanUse(out act)) return true;
        if (AeroIiInGreenPvE.CanUse(out act)) return true;
        if (FireIiInRedPvE.CanUse(out act)) return true;

        //ST Subtractive Inks
        if (ThunderInMagentaPvE.CanUse(out act)) return true;
        if (StoneInYellowPvE.CanUse(out act)) return true;
        if (BlizzardInCyanPvE.CanUse(out act)) return true;

        //ST Additive Inks
        if (WaterInBluePvE.CanUse(out act)) return true;
        if (AeroInGreenPvE.CanUse(out act)) return true;
        if (FireInRedPvE.CanUse(out act)) return true;
        return base.GeneralGCD(out act);
    }

    #endregion
}
