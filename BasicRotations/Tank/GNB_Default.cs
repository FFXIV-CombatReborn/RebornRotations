namespace DefaultRotations.Tank;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00")]
[SourceCode(Path = "main/DefaultRotations/Tank/GNB_Default.cs")]
[Api(3)]
public sealed class GNB_Default : GunbreakerRotation
{
    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= 0.7 && LightningShotPvE.CanUse(out var act)) return act;
        if (remainTime <= 1.2 && UseBurstMedicine(out act)) return act;
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if (InCombat && CombatElapsedLess(30))
        {
            if (!CombatElapsedLessGCD(2) && NoMercyPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (Player.HasStatus(true, StatusID.NoMercy) && BloodfestPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.HeartOfLightPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (!Player.HasStatus(true, StatusID.NoMercy) && HeartOfLightPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (!Player.HasStatus(true, StatusID.NoMercy) && ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.HeartOfStonePvE, ActionID.NebulaPvE, ActionID.RampartPvE, ActionID.CamouflagePvE, ActionID.ReprisalPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        //10
        if (CamouflagePvE.CanUse(out act)) return true;
        //15
        if (HeartOfStonePvE.CanUse(out act)) return true;

        //30
        if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && NebulaPvE.CanUse(out act)) return true;
        //20
        if (NebulaPvE.Cooldown.IsCoolingDown && NebulaPvE.Cooldown.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;

        if (ReprisalPvE.CanUse(out act)) return true;

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.AuroraPvE)]
    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (AuroraPvE.CanUse(out act, usedUp: true)) return true;
        return base.HealSingleAbility(nextGCD, out act);
    }
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        //if (IsBurst && CanUseNoMercy(out act)) return true;

        if (!CombatElapsedLessGCD(5) && NoMercyPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (JugularRipPvE.CanUse(out act)) return true;

        if (DangerZonePvE.CanUse(out act) && !DoubleDownPvE.EnoughLevel)
        {

            if (!IsFullParty && !(DangerZonePvE.Target.Target?.IsBossFromTTK() ?? false)) return true;

            if (!GnashingFangPvE.EnoughLevel && (Player.HasStatus(true, StatusID.NoMercy) || !NoMercyPvE.Cooldown.WillHaveOneCharge(15))) return true;

            if (Player.HasStatus(true, StatusID.NoMercy) && GnashingFangPvE.Cooldown.IsCoolingDown) return true;

            if (!Player.HasStatus(true, StatusID.NoMercy) && !GnashingFangPvE.Cooldown.WillHaveOneCharge(20)) return true;
        }

        if (Player.HasStatus(true, StatusID.NoMercy) && CanUseBowShock(out act)) return true;

        //if (TrajectoryPvE.CanUse(out act) && !IsMoving) return true;
        if (GnashingFangPvE.Cooldown.IsCoolingDown && DoubleDownPvE.Cooldown.IsCoolingDown && Ammo == 0 && BloodfestPvE.CanUse(out act)) return true;

        if (AbdomenTearPvE.CanUse(out act)) return true;
        if (EyeGougePvE.CanUse(out act)) return true;
        if (FatedBrandPvE.CanUse(out act)) return true;
        if (HypervelocityPvE.CanUse(out act)) return true;
        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act)) return true;
        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        bool areDDTargetsInRange = AllHostileTargets.Any(hostile => hostile.DistanceToPlayer() < 4.5f);

        if (Player.HasStatus(true, StatusID.NoMercy) && BloodfestPvE.CanUse(out act)) return true;

        if (IsLastGCD(false, NobleBloodPvE) && LionHeartPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (IsLastGCD(false, ReignOfBeastsPvE) && NobleBloodPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (IsLastAction(false, EyeGougePvE) && ReignOfBeastsPvE.CanUse(out act)) return true;

        if (Player.HasStatus(true, StatusID.NoMercy) && SonicBreakPvE.CanUse(out act)) return true;

        if (areDDTargetsInRange)
        {
            if (Player.HasStatus(true, StatusID.NoMercy) && CanUseDoubleDown(out act)) return true;
            if (Player.HasStatus(true, StatusID.NoMercy) && IsLastGCD(ActionID.DoubleDownPvE) && BlastingZonePvE.CanUse(out act)) return true;
        }

        if (NoMercyPvE.Cooldown.IsCoolingDown && BloodfestPvE.Cooldown.IsCoolingDown && BlastingZonePvE.CanUse(out act)) return true;

        if (CanUseGnashingFang(out act)) return true;

        if (SavageClawPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (WickedTalonPvE.CanUse(out act, skipComboCheck: true)) return true;

        if (CanUseBurstStrike(out act)) return true;

        if (FatedCirclePvE.CanUse(out act)) return true;
        if (DemonSlaughterPvE.CanUse(out act)) return true;
        if (DemonSlicePvE.CanUse(out act)) return true;

        if (Ammo == MaxAmmo && IsLastGCD(ActionID.BrutalShellPvE) && BurstStrikePvE.CanUse(out act)) return true;

        if (SolidBarrelPvE.CanUse(out act)) return true;
        if (BrutalShellPvE.CanUse(out act)) return true;
        if (KeenEdgePvE.CanUse(out act)) return true;

        if (LightningShotPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    public override bool CanHealSingleSpell => false;

    public override bool CanHealAreaSpell => false;

    //private bool CanUseNoMercy(out IAction act)
    //{
    //    if (!NoMercy.CanUse(out act, CanUseOption.OnLastAbility)) return false;

    //    if (!IsFullParty && !IsTargetBoss && !IsMoving && DemonSlice.CanUse(out _)) return true;

    //    if (!BurstStrike.EnoughLevel) return true;

    //    if (BurstStrike.EnoughLevel)
    //    {
    //        if (IsLastGCD((ActionID)KeenEdge.ID) && Ammo == 1 && !GnashingFang.IsCoolingDown && !BloodFest.IsCoolingDown) return true;
    //        else if (Ammo == (Level >= 88 ? 3 : 2)) return true;
    //        else if (Ammo == 2 && GnashingFang.IsCoolingDown) return true;
    //    }

    //    act = null;
    //    return false;
    //}

    private bool CanUseGnashingFang(out IAction? act)
    {
        if (GnashingFangPvE.CanUse(out act))
        {
            //AOE Check: Mobs = NO, Boss = YES
            if (DemonSlicePvE.CanUse(out _)) return false;

            if (Player.HasStatus(true, StatusID.NoMercy) || !NoMercyPvE.Cooldown.WillHaveOneCharge(55)) return true;

            if (Ammo > 0 && !NoMercyPvE.Cooldown.WillHaveOneCharge(17) && NoMercyPvE.Cooldown.WillHaveOneCharge(35)) return true;

            if (Ammo <= 3 && IsLastGCD((ActionID)BrutalShellPvE.ID) && NoMercyPvE.Cooldown.WillHaveOneCharge(3)) return true;

            if (Ammo == 1 && !NoMercyPvE.Cooldown.WillHaveOneCharge(55) && BloodfestPvE.Cooldown.WillHaveOneCharge(5)) return true;

            if (Ammo == 1 && !NoMercyPvE.Cooldown.WillHaveOneCharge(55) && (!BloodfestPvE.Cooldown.IsCoolingDown && BloodfestPvE.EnoughLevel || !BloodfestPvE.EnoughLevel)) return true;
        }
        return false;
    }

    /*private bool CanUseSonicBreak(out IAction act)
    {
        if (SonicBreakPvE.CanUse(out act))
        {
            
            if (!GnashingFangPvE.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            if (!DoubleDownPvE.EnoughLevel && Player.HasStatus(true, StatusID.ReadyToRip)
                && GnashingFangPvE.Cooldown.IsCoolingDown) return true;

        }
        return false;
    }*/

    private bool CanUseDoubleDown(out IAction? act)
    {
        if (DoubleDownPvE.CanUse(out act, skipAoeCheck: true))
        {
            if (SonicBreakPvE.Cooldown.IsCoolingDown && Player.HasStatus(true, StatusID.NoMercy)) return true;
            if (Player.HasStatus(true, StatusID.NoMercy) && !NoMercyPvE.Cooldown.WillHaveOneCharge(55) && BloodfestPvE.Cooldown.WillHaveOneCharge(5)) return true;

        }
        return false;
    }

    private bool CanUseBurstStrike(out IAction act)
    {
        if (BurstStrikePvE.CanUse(out act))
        {
            if (DemonSlicePvE.CanUse(out _)) return false;

            if (SonicBreakPvE.Cooldown.IsCoolingDown && SonicBreakPvE.Cooldown.WillHaveOneCharge(0.5f) && GnashingFangPvE.EnoughLevel) return false;

            if (Player.HasStatus(true, StatusID.NoMercy) &&
            AmmoComboStep == 0 &&
                !GnashingFangPvE.Cooldown.WillHaveOneCharge(1)) return true;

            if (!CartridgeChargeIiTrait.EnoughLevel && Ammo == 2) return true;

            if (IsLastGCD((ActionID)BrutalShellPvE.ID) &&
                (Ammo == MaxAmmo ||
                BloodfestPvE.Cooldown.WillHaveOneCharge(6) && Ammo <= 2 && !NoMercyPvE.Cooldown.WillHaveOneCharge(10) && BloodfestPvE.EnoughLevel)) return true;

        }
        return false;
    }

    private bool CanUseBowShock(out IAction act)
    {
        if (BowShockPvE.CanUse(out act, skipAoeCheck: true))
        {
            //AOE CHECK
            if (DemonSlicePvE.CanUse(out _) && !IsFullParty) return true;

            if (!SonicBreakPvE.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            if (Player.HasStatus(true, StatusID.NoMercy) && SonicBreakPvE.Cooldown.IsCoolingDown) return true;
        }
        return false;
    }
    #endregion
}