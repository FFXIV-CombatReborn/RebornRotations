namespace DefaultRotations.Tank;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00", Description = "Additional Contributions from Sascha")]
[SourceCode(Path = "main/DefaultRotations/Tank/WAR_Default.cs")]
[Api(3)]
public sealed class WAR_Default : WarriorRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Only use Nascent Flash if Tank Stance is off")]
    public bool NeverscentFlash { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use Bloodwhetting/Raw intuition on single enemies")]
    public bool SoloIntuition { get; set; } = false;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Nascent Flash Heal Threshold")]
    public float FlashHeal { get; set; } = 0.6f;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Thrill Of Battle Heal Threshold")]
    public float ThrillOfBattleHeal { get; set; } = 0.6f;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Equilibrium Heal Threshold")]
    public float EquilibriumHeal { get; set; } = 0.6f;

    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= CountDownAhead)
        {
            if (HasTankStance)
            {
                if (ProvokePvE.CanUse(out var act)) return act;
            }
            else
            {
                if (TomahawkPvE.CanUse(out var act)) return act;
            }
        }
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (InfuriatePvE.CanUse(out act, gcdCountForAbility: 3)) return true;

        if (CombatElapsedLessGCD(1)) return false;

        if (UseBurstMedicine(out act)) return true;

        if (Player.HasStatus(false, StatusID.SurgingTempest)
            && !Player.WillStatusEndGCD(2, 0, true, StatusID.SurgingTempest)
            || !MythrilTempestPvE.EnoughLevel)
        {
            if (BerserkPvE.CanUse(out act)) return true;

        }

        if (IsBurstStatus)
        {
            if (InfuriatePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (CombatElapsedLessGCD(4)) return false;

        if (OrogenyPvE.CanUse(out act)) return true;

        if (UpheavalPvE.CanUse(out act)) return true;

        if (Player.HasStatus(false, StatusID.Wrathful) && PrimalWrathPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (OnslaughtPvE.CanUse(out act, usedUp: IsBurstStatus) &&
           !IsMoving &&
           !IsLastAction(true, OnslaughtPvE) &&
           !IsLastAction(true, UpheavalPvE) &&
            Player.HasStatus(false, StatusID.SurgingTempest))
        {
            return true;
        }


        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act)) return true;
        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        if (Player.GetHealthRatio() < ThrillOfBattleHeal)
        {
            if (ThrillOfBattlePvE.CanUse(out act)) return true;
        }

        if (!Player.HasStatus(true, StatusID.Holmgang_409))
        {
            if (Player.GetHealthRatio() < EquilibriumHeal)
            {
                if (EquilibriumPvE.CanUse(out act)) return true;
            }
        }
        return base.GeneralAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.RawIntuitionPvE, ActionID.VengeancePvE, ActionID.RampartPvE, ActionID.RawIntuitionPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        bool RawSingleTargets = SoloIntuition;
        act = null;

        if (Player.HasStatus(true, StatusID.Holmgang_409) && Player.GetHealthRatio() < 0.3f) return false;

        if (RawIntuitionPvE.CanUse(out act) && (RawSingleTargets || NumberOfHostilesInRange > 2)) return true;

        if (!Player.WillStatusEndGCD(0, 0, true, StatusID.Bloodwhetting, StatusID.RawIntuition)) return false;

        if (ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && VengeancePvE.CanUse(out act)) return true;

        if (((VengeancePvE.Cooldown.IsCoolingDown && VengeancePvE.Cooldown.ElapsedAfter(60)) || !VengeancePvE.EnoughLevel) && RampartPvE.CanUse(out act)) return true;

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.ShakeItOffPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;

        if (ShakeItOffPvE.Cooldown.IsCoolingDown && !ShakeItOffPvE.Cooldown.WillHaveOneCharge(60)
            || ReprisalPvE.Cooldown.IsCoolingDown && !ReprisalPvE.Cooldown.WillHaveOneCharge(50)) return false;

        if (ShakeItOffPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.DefenseAreaAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        if (IsLastAction(false, InnerReleasePvE))
        {
            if (FellCleavePvE.CanUse(out act, skipStatusProvideCheck: true)) return true;
        }

        if (Player.HasStatus(false, StatusID.SurgingTempest) &&
       (IsBurstStatus || !Player.HasStatus(false, StatusID.NascentChaos) || BeastGauge > 80))
        {
            if (SteelCyclonePvE.CanUse(out act)) return true;
            if (InnerBeastPvE.CanUse(out act)) return true;
        }

        if (!Player.WillStatusEndGCD(3, 0, true, StatusID.SurgingTempest))
        {
            if (!IsMoving && PrimalRendPvE.CanUse(out act, skipAoeCheck: true))
            {
                if (PrimalRendPvE.Target.Target?.DistanceToPlayer() < 2) return true;
            }

            // New check for Primal Ruination
            if (Player.HasStatus(false, StatusID.PrimalRuinationReady) && !Player.HasStatus(false, StatusID.InnerRelease))
            {
                if (PrimalRuinationPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }

        }

        if (MythrilTempestPvE.CanUse(out act)) return true;
        if (OverpowerPvE.CanUse(out act)) return true;
        if (StormsEyePvE.CanUse(out act)) return true;
        if (StormsPathPvE.CanUse(out act)) return true;
        if (MaimPvE.CanUse(out act)) return true;
        if (HeavySwingPvE.CanUse(out act)) return true;

        if (TomahawkPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    [RotationDesc(ActionID.NascentFlashPvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        if (!NeverscentFlash && NascentFlashPvE.CanUse(out act)
            && (InCombat && NascentFlashPvE.Target.Target?.GetHealthRatio() < FlashHeal)) return true;

        if (NeverscentFlash && NascentFlashPvE.CanUse(out act)
            && (InCombat && !Player.HasStatus(true, StatusID.Defiance) && NascentFlashPvE.Target.Target?.GetHealthRatio() < FlashHeal)) return true;

        return base.HealSingleGCD(out act);
    }
    #endregion

    #region Extra Methods
    private static bool IsBurstStatus => !Player.WillStatusEndGCD(0, 0, false, StatusID.InnerStrength);
    #endregion
}
