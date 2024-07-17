namespace DefaultRotations.Melee;

[Rotation("WIP DT MNK", CombatType.PvE, GameVersion = "7.00", Description = "Uses Lunar Solar Opener from The Balance")]
[SourceCode(Path = "main/DefaultRotations/Melee/MNK_Default.cs")]
[Api(2)]

public sealed class MNK_Default : MonkRotation
{

    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use Form Shift")]
    public bool AutoFormShift { get; set; } = true;
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 0.2)
        {
            if (ThunderclapPvE.CanUse(out var act)) return act; // Gap closer at the end of countdown
        }
        if (remainTime < 15)
        {
            if (FormShiftPvE.CanUse(out var act)) return act; // FormShift to prep opening
        }

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat)
        {
            if (UseBurstMedicine(out act)) return true;
            if (IsBurst && !CombatElapsedLessGCD(2) && RiddleOfFirePvE.CanUse(out act)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;

        if (CombatElapsedLessGCD(3)) return false; // Prevents the use of abilities if 3 GCDs have not been used

        if (EarthsReplyPvE.CanUse(out act, skipAoeCheck: true) && (Player.CurrentHp < Player.MaxHp)) return true; // Earths Reply

        if (BeastChakras.Contains(BeastChakra.NONE) && Player.HasStatus(true, StatusID.RaptorForm)
            && (!RiddleOfFirePvE.EnoughLevel || Player.HasStatus(false, StatusID.RiddleOfFire) && !Player.WillStatusEndGCD(3, 0, false, StatusID.RiddleOfFire)
            || RiddleOfFirePvE.Cooldown.WillHaveOneChargeGCD(1) && (PerfectBalancePvE.Cooldown.ElapsedAfter(60) || !PerfectBalancePvE.Cooldown.IsCoolingDown)))
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true; // Perfect Balance
        }

        if (BrotherhoodPvE.CanUse(out act, skipAoeCheck: true)) return true; // Brotherhood
        if (EnlightenmentPvE.CanUse(out act)) return true; // Enlightment
        if (HowlingFistPvE.CanUse(out act)) return true; // Howling Fist
        if (SteelPeakPvE.CanUse(out act)) return true; // Steel Peak
        
        //if (HowlingFistPvE.CanUse(out act, skipAoeCheck: true)) return true; // Howling Fist AOE

        if (RiddleOfWindPvE.CanUse(out act)) return true; // Riddle Of Wind

        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    private bool OpoOpoForm(out IAction? act)
    {
        if (ArmOfTheDestroyerPvE.CanUse(out act)) return true; // Arm Of The Destoryer
        if (LeapingOpoPvE.CanUse(out act)) return true; // Leaping Opo
        if (DragonKickPvE.CanUse(out act)) return true; // Dragon Kick
        if (BootshinePvE.CanUse(out act)) return true; //Bootshine
        return false;
    }

    private bool UseLunarPerfectBalance => (HasSolar || Player.HasStatus(false, StatusID.PerfectBalance))
        && (!Player.WillStatusEndGCD(0, 0, false, StatusID.RiddleOfFire) || Player.HasStatus(false, StatusID.RiddleOfFire) || RiddleOfFirePvE.Cooldown.WillHaveOneChargeGCD(2)) && PerfectBalancePvE.Cooldown.WillHaveOneChargeGCD(3);

    private bool RaptorForm(out IAction? act)
    {
        if (FourpointFuryPvE.CanUse(out act)) return true; //Fourpoint Fury
        /*if ((Player.WillStatusEndGCD(3, 0, true, StatusID.DisciplinedFist)
            || Player.WillStatusEndGCD(7, 0, true, StatusID.DisciplinedFist)
            && UseLunarPerfectBalance) && TwinSnakesPvE.CanUse(out act)) return true; //Twin Snakes*/
        if (RisingRaptorPvE.CanUse(out act)) return true; //Rising Raptor
        if (TwinSnakesPvE.CanUse(out act)) return true; //Twin Snakes
        if (TrueStrikePvE.CanUse(out act)) return true; //True Strike
        return false;
    }

    private bool CoerlForm(out IAction? act)
    {
        if (RockbreakerPvE.CanUse(out act)) return true; // Rockbreaker
        //if (UseLunarPerfectBalance && DemolishPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;
        //&& (DemolishPvE.Target.Target?.WillStatusEndGCD(7, 0, true, StatusID.Demolish) ?? false)) return true;
        if (PouncingCoeurlPvE.CanUse(out act)) return true; // Pouncing Coeurl
        if (DemolishPvE.CanUse(out act)) return true; // Demolish
        if (SnapPunchPvE.CanUse(out act)) return true; // Snap Punch
        return false;
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (WindsReplyPvE.CanUse(out act, skipAoeCheck: true)) return true; // Winds Reply
        if (FiresReplyPvE.CanUse(out act, skipAoeCheck: true)) return true; // Fires Reply

        if (PerfectBalanceActions(out act)) return true;

        if (Player.HasStatus(true, StatusID.CoeurlForm))
        {
            if (CoerlForm(out act)) return true; // Use Coeurl Form GCDs if in Coeurl Form
        }

        if (Player.HasStatus(true, StatusID.RiddleOfFire)
            && !RiddleOfFirePvE.Cooldown.ElapsedAfterGCD(2) && (PerfectBalancePvE.Cooldown.ElapsedAfter(60) || !PerfectBalancePvE.Cooldown.IsCoolingDown))
        {
            if (OpoOpoForm(out act)) return true;
        }

        if (Player.HasStatus(true, StatusID.RaptorForm))
        {
            if (RaptorForm(out act)) return true; // Use Raptor Form GCDs if in Raptor Form
        }

        if (OpoOpoForm(out act)) return true; // Fallback to Use OpoOpo Form GCDs 

        if (Chakra < 5 && ForbiddenMeditationPvE.CanUse(out act)) return true;

        if (Player.HasStatus(true, StatusID.MeditativeBrotherhood) && Chakra >= 5 && ForbiddenMeditationPvE.CanUse(out act)) return true;

        if (AutoFormShift && FormShiftPvE.CanUse(out act)) return true; // Form Shift GCD use

        return base.GeneralGCD(out act);
    }

    private bool PerfectBalanceActions(out IAction? act) // Controls actions during Perfect Balance buff
    {
        if (!BeastChakras.Contains(BeastChakra.NONE))
        {
            if (HasSolar && HasLunar)
            {
                if (PhantomRushPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (TornadoKickPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            if (BeastChakras.Contains(BeastChakra.RAPTOR))
            {
                if (RisingPhoenixPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (FlintStrikePvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            else
            {
                if (ElixirBurstPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (ElixirFieldPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
        }
        else if (Player.HasStatus(true, StatusID.PerfectBalance) && (ElixirBurstPvE.EnoughLevel || ElixirFieldPvE.EnoughLevel))
        {
            //Sometimes, no choice
            if (HasSolar || BeastChakras.Count(c => c == BeastChakra.OPOOPO) > 1)
            {
                if (LunarNadi(out act)) return true;
            }
            else if (BeastChakras.Contains(BeastChakra.COEURL) || BeastChakras.Contains(BeastChakra.RAPTOR))
            {
                if (SolarNadi(out act)) return true;
            }

            //Add Solar Nadi if Lunar Nadi is present.
            if (HasLunar)
            {
                if (SolarNadi(out act)) return true;
            }
            if (LunarNadi(out act)) return true;
        }

        act = null;
        return false;
    }

    bool LunarNadi(out IAction? act)
    {
        if (OpoOpoForm(out act)) return true;
        return false;
    }

    bool SolarNadi(out IAction? act)
    {
        //Emergency usage of status.
        /*if (!BeastChakras.Contains(BeastChakra.RAPTOR)
            && HasLunar
            && Player.WillStatusEndGCD(1, 0, true, StatusID.DisciplinedFist))
        {
            if (RaptorForm(out act)) return true;
        }
        if (!BeastChakras.Contains(BeastChakra.COEURL)
            && (HostileTarget?.WillStatusEndGCD(1, 0, true, StatusID.Demolish) ?? false))
        {
            if (CoerlForm(out act)) return true;
        }*/

        if (!BeastChakras.Contains(BeastChakra.OPOOPO))
        {
            if (OpoOpoForm(out act)) return true;
        }
        if (HasLunar && !BeastChakras.Contains(BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }
        if (!BeastChakras.Contains(BeastChakra.COEURL))
        {
            if (CoerlForm(out act)) return true;
        }
        if (!BeastChakras.Contains(BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }

        return CoerlForm(out act);
    }
    #endregion

    #region Extra Methods

    private static bool NoForm => ((!Player.HasStatus(false, StatusID.OpoopoForm) && !Player.HasStatus(false, StatusID.RaptorForm) && !Player.HasStatus(false, StatusID.CoeurlForm)) || Player.HasStatus(true, StatusID.FormlessFist) || Player.HasStatus(true, StatusID.PerfectBalance));

    #endregion
}
