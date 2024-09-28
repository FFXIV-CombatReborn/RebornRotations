namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00", Description = "Uses Lunar Solar Opener from The Balance")]
[SourceCode(Path = "main/BasicRotations/Melee/MNK_Default.cs")]
[Api(4)]

public sealed class MNK_Default : MonkRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use Form Shift")]
    public bool AutoFormShift { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Auto Use Perfect Balance (single target full auto mode, turn me off if you want total control of PB)")]
    public bool AutoPB_Boss { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Auto Use Perfect Balance (aoe aggressive PB dump, turn me off if you don't want to waste PB in boss fight)")]
    public bool AutoPB_AOE { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Enable TEA Checker.")]
    public bool EnableTEAChecker { get; set; } = false; 
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        // gap closer at the end of countdown
        if (remainTime <= 0.5 && ThunderclapPvE.CanUse(out var act)) return act; // need to face target to trigger
        // true north before pull
        if (remainTime <= 2 && TrueNorthPvE.CanUse(out act)) return act;
        // turn on 5 chakra at -5 prepull 
        if (remainTime <= 5 && Chakra < 5 && ForbiddenMeditationPvE.CanUse(out act)) return act;
        // formShift to prep opening
        if (remainTime < 15 && FormShiftPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (EnableTEAChecker && Target.Name.ToString() == "Jagd Doll" && Target.GetHealthRatio() < 0.25)
        {
            return false;
        }
        
        // PerfectBalancePvE after first gcd + TheForbiddenChakraPvE after second gcd
        // fail to weave both after first gcd - rsr doesn't have enough time to react to both spells
        // you pot -2s (real world -3s) prepull or after 2nd gcd!!! 
        // there is a small chance PB is not pressed in time if put in AttackAbility
        // start the fight 8 yarms away from boss for double weaving
        // 'The form shift and meditation prepull are implied. Prepull pot should win out, but choosing to press it in the first few weave slots shouldn¡¯t result in more than a single digit loss'
        // 'there may be a delay before it can be used. Pushing it to the 2nd weave slot should avoid this.'
        if (AutoPB_Boss && InCombat && CombatElapsedLess(3) && PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        //if (CombatElapsedLessGCD(1) && TheForbiddenChakraPvE.CanUse(out act)) return true; // if it weaves one day in the future...

        // need this to connect the first three buffs
        if (IsLastAbility(true, BrotherhoodPvE) && RiddleOfFirePvE.CanUse(out act)) return true; // Riddle Of Fire

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.ThunderclapPvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (ThunderclapPvE.CanUse(out act)) return true;
        return base.MoveForwardAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.FeintPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (FeintPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.MantraPvE)]
    protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (MantraPvE.CanUse(out act)) return true;
        return base.HealAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.RiddleOfEarthPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (RiddleOfEarthPvE.CanUse(out act, usedUp: true)) return true;
        return base.DefenseSingleAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (EnableTEAChecker && Target.Name.ToString() == "Jagd Doll" && Target.GetHealthRatio() < 0.25)
        {
            return false;
        }

        // you need to position yourself in the centre of the mobs if they are large, that range is only 3 yarms
        if (AutoPB_AOE && NumberOfHostilesInRange >= 2)
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        }

        // opener 2nd burst
        if (AutoPB_Boss
            && Player.HasStatus(true, StatusID.RiddleOfFire) && Player.HasStatus(true, StatusID.Brotherhood)
            && IsLastGCD(true, DragonKickPvE, LeapingOpoPvE, BootshinePvE) // PB must follow an Opo
            && !Player.HasStatus(true, StatusID.FormlessFist) && !Player.HasStatus(true, StatusID.FiresRumination) && !Player.HasStatus(true, StatusID.WindsRumination))
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        }

        // odd min burst
        if (AutoPB_Boss
            && Player.HasStatus(true, StatusID.RiddleOfFire)
            && !PerfectBalancePvE.Cooldown.JustUsedAfter(20)
            && IsLastGCD(true, DragonKickPvE, LeapingOpoPvE, BootshinePvE)) // PB must follow an Opo 
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        }

        // even min burst
        if (AutoPB_Boss
            && !Player.HasStatus(true, StatusID.RiddleOfFire)
            && RiddleOfFirePvE.Cooldown.WillHaveOneChargeGCD(3) && BrotherhoodPvE.Cooldown.WillHaveOneCharge(3)
            && IsLastGCD(true, DragonKickPvE, LeapingOpoPvE, BootshinePvE)) // PB must follow an Opo 
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        }

        // 'TFC is used in the first weave slot to avoid any chakra overcap from the following gcds.'
        // dump 5 stacks of chakara 
        if (NumberOfHostilesInRange >= 2)
        {
            if (EnlightenmentPvE.CanUse(out act, skipAoeCheck: true)) return true; // Enlightment
            if (HowlingFistPvE.CanUse(out act, skipAoeCheck: true)) return true; // Howling Fist
        }
        else
            if (TheForbiddenChakraPvE.CanUse(out act)) return true;

        // use bh when bh and rof are ready (opener) or ask bh to wait for rof's cd to be close and then use bh
        if (!CombatElapsedLessGCD(2)
            && ((BrotherhoodPvE.IsInCooldown && RiddleOfFirePvE.IsInCooldown) || Math.Abs(BrotherhoodPvE.Cooldown.CoolDownGroup - RiddleOfFirePvE.Cooldown.CoolDownGroup) < 3)
            && BrotherhoodPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // rof needs to be used on cd or after x gcd in opener
        if (!CombatElapsedLessGCD(3) && RiddleOfFirePvE.CanUse(out act)) return true; // Riddle Of Fire
        // 'Use on cooldown, unless you know your killtime. You should aim to get as many casts of RoW as you can, and then shift those usages to align with burst as much as possible without losing a use.'
        if (!CombatElapsedLessGCD(3) && RiddleOfWindPvE.CanUse(out act)) return true; // Riddle Of Wind

        // what's this? check later
        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    // 'More opos in the fight is better than... in lunar PBs'
    private bool OpoOpoForm(out IAction? act)
    {
        if (ArmOfTheDestroyerPvE.CanUse(out act)) return true; // Arm Of The Destoryer - aoe
        if (LeapingOpoPvE.CanUse(out act)) return true; // Leaping Opo
        if (DragonKickPvE.CanUse(out act)) return true; // Dragon Kick
        if (BootshinePvE.CanUse(out act)) return true; //Bootshine - low level
        return false;
    }

    private bool RaptorForm(out IAction? act)
    {
        if (FourpointFuryPvE.CanUse(out act)) return true; //Fourpoint Fury - aoe
        if (RisingRaptorPvE.CanUse(out act)) return true; //Rising Raptor
        if (TwinSnakesPvE.CanUse(out act)) return true; //Twin Snakes
        if (TrueStrikePvE.CanUse(out act)) return true; //True Strike - low level
        return false;
    }

    private bool CoerlForm(out IAction? act)
    {
        if (RockbreakerPvE.CanUse(out act)) return true; // Rockbreaker - aoe
        if (PouncingCoeurlPvE.CanUse(out act)) return true; // Pouncing Coeurl
        if (DemolishPvE.CanUse(out act)) return true; // Demolish
        if (SnapPunchPvE.CanUse(out act)) return true; // Snap Punch - low level
        return false;
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (EnableTEAChecker && Target.Name.ToString() == "Jagd Doll" && Target.GetHealthRatio() < 0.25)
        {
            return false;
        }
        
        // bullet proofed finisher - use when during burst
        // or if burst was missed, and next burst is not arriving in time, use it better than waste it, otherwise, hold it for next rof
        if (!BeastChakras.Contains(BeastChakra.NONE) && (Player.HasStatus(true, StatusID.RiddleOfFire) || RiddleOfFirePvE.Cooldown.JustUsedAfter(42)))
        {
            // for some reason phantom doesn't count as a variation of masterful like the others
            if (PhantomRushPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (TornadoKickPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (CelestialRevolutionPvE.CanUse(out act, skipAoeCheck: true)) return true; // shouldn't need this but who know what button the user may press
            if (MasterfulBlitzPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        // 'Because Fire¡¯s Reply grants formless, we have an imposed restriction that we prefer not to use it while under PB, or if we have a formless already.' + 'Cast Fire's Reply after an opo gcd'
        // need to test and see if IsLastGCD(false, ...) is better
        if ((!Player.HasStatus(true, StatusID.PerfectBalance) && !Player.HasStatus(true, StatusID.FormlessFist) && IsLastGCD(true, DragonKickPvE, LeapingOpoPvE, BootshinePvE) || Player.WillStatusEnd(5, true, StatusID.FiresRumination)) && FiresReplyPvE.CanUse(out act, skipAoeCheck: true)) return true; // Fires Reply
        // 'Cast Wind's Reply literally anywhere in the window'
        if (!Player.HasStatus(true, StatusID.PerfectBalance) && WindsReplyPvE.CanUse(out act, skipAoeCheck: true)) return true; // Winds Reply

        // Opo needs to follow each PB
        // 'This means ¡°bookending¡± any PB usage with opos and spending formless on opos.'
        if (Player.HasStatus(true, StatusID.FormlessFist) && OpoOpoForm(out act)) return true;
        //if (Player.StatusStack(true, StatusID.PerfectBalance) == 3 && OpoOpoForm(out act)) return true;

        if (Player.HasStatus(true, StatusID.PerfectBalance) && !HasSolar)
        {
            // SolarNadi - fill the missing one - this order is needed for opener
            if (!BeastChakras.Contains(BeastChakra.RAPTOR) && RaptorForm(out act)) return true;
            if (!BeastChakras.Contains(BeastChakra.COEURL) && CoerlForm(out act)) return true;
            if (!BeastChakras.Contains(BeastChakra.OPOOPO) && OpoOpoForm(out act)) return true;
        }

        if (Player.HasStatus(true, StatusID.PerfectBalance) && HasSolar)
        {
            // 'we still want to prioritize pressing as many opo gcds as possible'
            // LunarNadi
            if (OpoOpoForm(out act)) return true;
        }

        // whatever you have, press it from left to right
        if (CoerlForm(out act)) return true;
        if (RaptorForm(out act)) return true;
        if (OpoOpoForm(out act)) return true;

        // out of range or nothing to do, recharge chakra first
        if (Chakra < 5 && (ForbiddenMeditationPvE.CanUse(out act) || SteeledMeditationPvE.CanUse(out act))) return true;

        // out of range or nothing to do, refresh buff second, but dont keep refreshing or it draws too much attention
        if (AutoFormShift && !Player.HasStatus(true, StatusID.PerfectBalance) && !Player.HasStatus(true, StatusID.FormlessFist) && FormShiftPvE.CanUse(out act)) return true; // Form Shift GCD use

        // i'm clever and i can do kame hame ha, so i won't stand still and keep refreshing form shift
        if (EnlightenmentPvE.CanUse(out act, skipAoeCheck: true)) return true; // Enlightment
        if (HowlingFistPvE.CanUse(out act, skipAoeCheck: true)) return true; // Howling Fist

        return base.GeneralGCD(out act);
    }
    #endregion
}
