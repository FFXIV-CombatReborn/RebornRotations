namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.PvE, GameVersion = "7.01")]
[SourceCode(Path = "main/DefaultRotations/Melee/DRG_Default.cs")]
[Api(3)]

public sealed class DRG_Default : DragoonRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use Doom Spike for damage uptime if out of melee range even if it breaks combo")]
    public bool DoomSpikeWhenever { get; set; } = true;
    #endregion

    #region Additional oGCD Logic

    [RotationDesc]
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.WingedGlidePvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (WingedGlidePvE.CanUse(out act)) return true;

        return false;
    }

    [RotationDesc(ActionID.ElusiveJumpPvE)]
    protected override bool MoveBackAbility(IAction nextGCD, out IAction? act)
    {
        if (ElusiveJumpPvE.CanUse(out act)) return true;

        return false;
    }

    [RotationDesc(ActionID.FeintPvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (FeintPvE.CanUse(out act)) return true;
        return false;
    }
    #endregion

    #region oGCD Logic
    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        if (IsBurst && InCombat)
        {
            if ((Player.HasStatus(true, StatusID.BattleLitany) || Player.HasStatus(true, StatusID.LanceCharge) || LOTDEndAfter(1000)) && nextGCD.IsTheSameTo(true, HeavensThrustPvE, DrakesbanePvE)
            || (Player.HasStatus(true, StatusID.BattleLitany) && Player.HasStatus(true, StatusID.LanceCharge) && LOTDEndAfter(1000) && nextGCD.IsTheSameTo(true, ChaoticSpringPvE, LanceBarragePvE, WheelingThrustPvE, FangAndClawPvE))
            || (nextGCD.IsTheSameTo(true, HeavensThrustPvE, DrakesbanePvE) && (LanceChargePvE.IsInCooldown || BattleLitanyPvE.IsInCooldown)))
            {
                if (LifeSurgePvE.CanUse(out act, usedUp: true)) return true;
            }

            if (LanceChargePvE.CanUse(out act)) return true;

            if (BattleLitanyPvE.CanUse(out act)) return true;
        }

        return base.GeneralAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (Player.HasStatus(true, StatusID.LanceCharge))
        {
            if (GeirskogulPvE.CanUse(out act)) return true;
        }

        if (BattleLitanyPvE.EnoughLevel && Player.HasStatus(true, StatusID.BattleLitany) && Player.HasStatus(true, StatusID.LanceCharge)
            || !BattleLitanyPvE.EnoughLevel && Player.HasStatus(true, StatusID.LanceCharge))
        {
            if (DragonfireDivePvE.CanUse(out act)) return true;
        }

        if ((Player.HasStatus(true, StatusID.BattleLitany) || Player.HasStatus(true, StatusID.LanceCharge) || LOTDEndAfter(1000))
            || nextGCD.IsTheSameTo(true, RaidenThrustPvE, DraconianFuryPvE))
        {
            if (WyrmwindThrustPvE.CanUse(out act, usedUp: true)) return true;
        }

        if (JumpPvE.CanUse(out act)) return true;
        if (HighJumpPvE.CanUse(out act)) return true;

        if (StardiverPvE.CanUse(out act)) return true;
        if (MirageDivePvE.CanUse(out act)) return true;
        if (NastrondPvE.CanUse(out act)) return true;
        if (StarcrossPvE.CanUse(out act)) return true;
        if (RiseOfTheDragonPvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        bool doomSpikeRightNow = DoomSpikeWhenever;

        if (CoerthanTormentPvE.CanUse(out act)) return true;
        if (SonicThrustPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;
        if (DoomSpikePvE.CanUse(out act, skipComboCheck: doomSpikeRightNow)) return true;

        if (DrakesbanePvE.CanUse(out act)) return true;

        if (FangAndClawPvE.CanUse(out act)) return true;
        if (WheelingThrustPvE.CanUse(out act)) return true;

        if (FullThrustPvE.CanUse(out act)) return true;
        if (ChaosThrustPvE.CanUse(out act)) return true;

        if (SpiralBlowPvE.CanUse(out act)) return true;
        if (DisembowelPvE.CanUse(out act)) return true;
        if (LanceBarragePvE.CanUse(out act)) return true;
        if (VorpalThrustPvE.CanUse(out act)) return true;

        if (RaidenThrustPvE.CanUse(out act)) return true;
        if (TrueThrustPvE.CanUse(out act)) return true;

        if (PiercingTalonPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion
}
