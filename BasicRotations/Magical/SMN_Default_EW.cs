﻿using System.ComponentModel;

namespace DefaultRotations.Magical;

[Rotation("Default_EW", CombatType.PvE, GameVersion = "7.0")]
[SourceCode(Path = "main/DefaultRotations/Magical/SMN_Default_EW.cs")]
[Api(2)]
public sealed class Default_EW : SummonerRotation
{
    #region Config Options
    public enum SwiftType : byte
    {
        No,
        Emerald,
        Ruby,
        All,
    }

    public enum SummonOrderType : byte
    {
        [Description("Topaz-Emerald-Ruby")]
        TopazEmeraldRuby,

        [Description("Topaz-Ruby-Emerald")]
        TopazRubyEmerald,

        [Description("Emerald-Topaz-Ruby")]
        EmeraldTopazRuby,
    }

    [RotationConfig(CombatType.PvE, Name = "Use Crimson Cyclone. Will use at any range, regardless of saftey use with caution.")]
    public bool AddCrimsonCyclone { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Use Bahamut no matter what whenever it's up lol don't wait")]
    public bool AlwaysBaha { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use Swiftcast")]
    public SwiftType AddSwiftcast { get; set; } = SwiftType.No;

    [RotationConfig(CombatType.PvE, Name = "Order")]
    public SummonOrderType SummonOrder { get; set; } = SummonOrderType.EmeraldTopazRuby;
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (SummonCarbunclePvE.CanUse(out var act)) return act;

        if (remainTime <= RuinPvE.Info.CastTime + CountDownAhead
            && RuinPvE.CanUse(out act)) return act;
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Move Logic
    [RotationDesc(ActionID.CrimsonCyclonePvE)]
    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (CrimsonCyclonePvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.MoveForwardGCD(out act);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (IsBurst && !Player.HasStatus(false, StatusID.SearingLight))
        {
            if (SearingLightPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        if ((InBahamut && SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || InPhoenix ||
            IsTargetBoss && IsTargetDying) && EnkindleBahamutPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || IsTargetBoss && IsTargetDying) && DeathflarePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (RekindlePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (MountainBusterPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || !EnergyDrainPvE.Cooldown.IsCoolingDown) ||
            !SearingLightPvE.EnoughLevel || IsTargetBoss && IsTargetDying) && PainflarePvE.CanUse(out act)) return true;

        if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || !EnergyDrainPvE.Cooldown.IsCoolingDown) ||
            !SearingLightPvE.EnoughLevel || IsTargetBoss && IsTargetDying) && FesterPvE.CanUse(out act)) return true;

        if (EnergySiphonPvE.CanUse(out act)) return true;
        if (EnergyDrainPvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        if (SummonCarbunclePvE.CanUse(out act)) return true;

        if ((Player.HasStatus(false, StatusID.SearingLight) || SearingLightPvE.Cooldown.IsCoolingDown) && AlwaysBaha && SummonBahamutPvE.CanUse(out act)) return true;

        if (SlipstreamPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (CrimsonStrikePvE.CanUse(out act, skipAoeCheck: true)) return true;

        //AOE
        if (PreciousBrilliancePvE.CanUse(out act)) return true;
        //Single
        if (GemshinePvE.CanUse(out act)) return true;

        if (!IsMoving && AddCrimsonCyclone && CrimsonCyclonePvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((Player.HasStatus(false, StatusID.SearingLight) || SearingLightPvE.Cooldown.IsCoolingDown) && SummonBahamutPvE.CanUse(out act)) return true;

        if (!SummonBahamutPvE.EnoughLevel && HasHostilesInRange && AetherchargePvE.CanUse(out act)) return true;

        if (IsMoving && (Player.HasStatus(true, StatusID.GarudasFavor) || InIfrit)
            && !Player.HasStatus(true, StatusID.Swiftcast) && !InBahamut && !InPhoenix
            && RuinIvPvE.CanUse(out act, skipAoeCheck: true)) return true;

        switch (SummonOrder)
        {
            case SummonOrderType.TopazEmeraldRuby:
            default:
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                break;

            case SummonOrderType.TopazRubyEmerald:
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                break;

            case SummonOrderType.EmeraldTopazRuby:
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                break;
        }

        if (SummonTimeEndAfterGCD() && AttunmentTimeEndAfterGCD() &&
            !Player.HasStatus(true, StatusID.Swiftcast) && !InBahamut && !InPhoenix &&
            RuinIvPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (OutburstPvE.CanUse(out act)) return true;

        if (RuinPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    public override bool CanHealSingleSpell => false;
    #endregion
}