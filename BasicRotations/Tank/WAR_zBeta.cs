namespace DefaultRotations.Tank;

[Rotation("Beta", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Tank/WAR_zBeta.cs")]
[Api(1)]
public sealed class WAR_zBeta : WarriorRotation
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
        // If Infuriate can be used, use it and return true.
        if (InfuriatePvE.CanUse(out act, gcdCountForAbility: 3)) return true;

        // If less than one global cooldown (GCD) has passed in combat, don't use any abilities.
        if (CombatElapsedLessGCD(1)) return false;

        // If a burst medicine can be used, use it and return true.
        if (UseBurstMedicine(out act)) return true;

        // If the player has the Surging Tempest status and it will not end in the next 6 GCDs, or if the player does not have a high enough level for Mythril Tempest, use Berserk.
        if (Player.HasStatus(false, StatusID.SurgingTempest)
            && !Player.WillStatusEndGCD(6, 0, true, StatusID.SurgingTempest)
            || !MythrilTempestPvE.EnoughLevel)
        {
            if (BerserkPvE.CanUse(out act)) return true;
        }

        // If the player is in a burst status, use Infuriate.
        if (IsBurstStatus)
        {
            if (InfuriatePvE.CanUse(out act, usedUp: true)) return true;
        }

        // If less than four GCDs have passed in combat, don't use any abilities.
        if (CombatElapsedLessGCD(4)) return false;

        // If Orogeny can be used, use it and return true.
        if (OrogenyPvE.CanUse(out act)) return true;

        // If Upheaval can be used, use it and return true.
        if (UpheavalPvE.CanUse(out act)) return true;

        // If Onslaught can be used and the player is not moving, use it and return true.
        if (OnslaughtPvE.CanUse(out act, usedUp: IsBurstStatus) && !IsMoving && !IsLastAction(true, OnslaughtPvE)) return true;

        // If the player's status includes moving forward and a move forward ability can be used, use it and return true.
        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act)) return true;
        return base.AttackAbility(nextGCD, out act);
    }


    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        // If the player's health ratio is less than configured setting, consider using healing abilities.
        if (Player.GetHealthRatio() < ThrillOfBattleHeal)
        {
            // If Thrill of Battle can be used, use it and return true.
            if (ThrillOfBattlePvE.CanUse(out act)) return true;
        }

        // If the player's health ratio is less than configured setting, consider using healing abilities.
        if (Player.GetHealthRatio() < EquilibriumHeal)
        {

            // If Equilibrium can be used, use it and return true.
            if (EquilibriumPvE.CanUse(out act)) return true;
        }
        return base.GeneralAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.RawIntuitionPvE, ActionID.VengeancePvE, ActionID.RampartPvE, ActionID.RawIntuitionPvE, ActionID.ReprisalPvE)]
    // This method is responsible for determining the defensive abilities to use in a single-target situation.
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        bool RawSingleTargets = SoloIntuition;
        act = null;

        // If the player currently has the Holmgang status and their health ratio is less than 0.3 (30%), don't use any defensive abilities.
        if (Player.HasStatus(true, StatusID.Holmgang_409) && Player.GetHealthRatio() < 0.3f) return false;

        // If Raw Intuition can be used and there are more than 2 hostiles in range or SoloIntuition Config Option is checked, use it.
        if (RawIntuitionPvE.CanUse(out act) && (RawSingleTargets || NumberOfHostilesInRange > 2)) return true;

        // If the player's Bloodwhetting or Raw Intuition status will not end in the next GCD, don't use any defensive abilities.
        if (!Player.WillStatusEndGCD(0, 0, true, StatusID.Bloodwhetting, StatusID.RawIntuition)) return false;

        // If Reprisal can be used, use it.
        if (ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // If Rampart is not cooling down or has been cooling down for more than 60 seconds, and Vengeance can be used, use Vengeance.
        if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && VengeancePvE.CanUse(out act)) return true;

        // If Vengeance is cooling down and has been cooling down for more than 60 seconds, or if Vengeance is not at a high enough level, and Rampart can be used, use Rampart.
        if (((VengeancePvE.Cooldown.IsCoolingDown && VengeancePvE.Cooldown.ElapsedAfter(60)) || !VengeancePvE.EnoughLevel) && RampartPvE.CanUse(out act)) return true;


        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.ShakeItOffPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        // Initialize the action to null.
        act = null;

        // If Shake It Off is cooling down and won't have a charge in the next 60 seconds, or if Reprisal is cooling down and won't have a charge in the next 50 seconds, don't use any defensive abilities.
        if (ShakeItOffPvE.Cooldown.IsCoolingDown && !ShakeItOffPvE.Cooldown.WillHaveOneCharge(60)
            || ReprisalPvE.Cooldown.IsCoolingDown && !ReprisalPvE.Cooldown.WillHaveOneCharge(50)) return false;

        // If Shake It Off can be used, use it.
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

        // If the player's Surging Tempest status will not end in the next 3 GCDs, consider using certain abilities.
        if (!Player.WillStatusEndGCD(3, 0, true, StatusID.SurgingTempest))
        {
            // If the player is not moving, is in a burst status, and Primal Rend can be used on a target within a distance of 1, use Primal Rend.
            if (!IsMoving && PrimalRendPvE.CanUse(out act, skipAoeCheck: true))
            {
                if (PrimalRendPvE.Target.Target?.DistanceToPlayer() < 1) return true;
            }
            // If the player is in a burst status, does not have the Nascent Chaos status, or has a Beast Gauge greater than 80, consider using Steel Cyclone or Inner Beast.
            if (IsBurstStatus || !Player.HasStatus(false, StatusID.NascentChaos) || BeastGauge > 80)
            {
                if (SteelCyclonePvE.CanUse(out act)) return true;
                if (InnerBeastPvE.CanUse(out act)) return true;
            }
        }

        // If any of the following abilities can be used, use them and return true.
        if (MythrilTempestPvE.CanUse(out act)) return true;
        if (OverpowerPvE.CanUse(out act)) return true;
        if (StormsEyePvE.CanUse(out act)) return true;
        if (StormsPathPvE.CanUse(out act)) return true;
        if (MaimPvE.CanUse(out act)) return true;
        if (HeavySwingPvE.CanUse(out act)) return true;

        // If Tomahawk can be used, use it and return true.
        if (TomahawkPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    [RotationDesc(ActionID.NascentFlashPvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        // If Nascent Flash can be used and the player is in combat and the target's health ratio is less than 0.6, use Nascent Flash.
        // This means Nascent Flash is used when the player is in combat and the target is at 60% health or less.
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
