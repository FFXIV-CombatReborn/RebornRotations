namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Melee/DRG_Default.cs")]
[Api(1)]

public sealed class DRG_Default : DragoonRotation
{/*
    #region Config Options
    [RotationDesc(ActionID.WingedGlidePvE, ActionID.DragonfireDivePvE)]

    [RotationConfig(CombatType.PvE, Name = "Break Single Target Combo to AOE when time to AOE")]
    public bool DoomSpikeWhenever { get; set; } = true;
    #endregion

    #region Move Logic
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction act)
    {
        if (SpineshatterDivePvE.CanUse(out act)) return true;
        if (DragonfireDivePvE.CanUse(out act, skipAoeCheck: true)) return true;

        return false;
    }

    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD.IsTheSameTo(true, FullThrustPvE, CoerthanTormentPvE)
            || Player.HasStatus(true, StatusID.LanceCharge) && nextGCD.IsTheSameTo(false, FangAndClawPvE))
        {
            if (LifeSurgePvE.CanUse(out act, usedUp: true)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }



    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (IsBurst && InCombat)
        {
            if (LanceChargePvE.CanUse(out act, skipAoeCheck: true) && Player.HasStatus(true, StatusID.PowerSurge)) return true;
            if (LanceChargePvE.CanUse(out act, skipAoeCheck: true) && !Player.HasStatus(true, StatusID.PowerSurge)) return true;

            if (DragonSightPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (BattleLitanyPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (NastrondPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (StardiverPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (HighJumpPvE.EnoughLevel)
        {
            if (HighJumpPvE.CanUse(out act)) return true;
        }
        else
        {
            if (JumpPvE.CanUse(out act)) return true;
        }

        if (GeirskogulPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (SpineshatterDivePvE.CanUse(out act, usedUp: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceChargePvE.Cooldown.ElapsedOneChargeAfterGCD(3)) return true;
        }
        if (Player.HasStatus(true, StatusID.PowerSurge) && SpineshatterDivePvE.Cooldown.CurrentCharges != 1 && SpineshatterDivePvE.CanUse(out act)) return true;

        if (MirageDivePvE.CanUse(out act)) return true;

        if (DragonfireDivePvE.CanUse(out act, skipAoeCheck: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceChargePvE.Cooldown.ElapsedOneChargeAfterGCD(3)) return true;
        }

        if (WyrmwindThrustPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        bool doomSpikeRightNow = DoomSpikeWhenever;

        if (CoerthanTormentPvE.CanUse(out act)) return true;
        if (SonicThrustPvE.CanUse(out act)) return true;
        if (DoomSpikePvE.CanUse(out act, skipComboCheck: doomSpikeRightNow)) return true;


        if (WheelingThrustPvE.CanUse(out act)) return true;
        if (FangAndClawPvE.CanUse(out act)) return true;


        if (FullThrustPvE.CanUse(out act)) return true;
        if (ChaosThrustPvE.CanUse(out act)) return true;

        if (Player.WillStatusEndGCD(5, 0, true, StatusID.PowerSurge_2720))
        {
            if (DisembowelPvE.CanUse(out act)) return true;
        }

        if (VorpalThrustPvE.CanUse(out act)) return true;
        if (TrueThrustPvE.CanUse(out act)) return true;
        if (PiercingTalonPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion
*/}
