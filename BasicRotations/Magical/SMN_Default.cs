using System.ComponentModel;

namespace DefaultRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00")]
[SourceCode(Path = "main/DefaultRotations/Magical/SMN_Default.cs")]
[Api(2)]
public sealed class SMN_Default : SummonerRotation
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
    
    // [RotationConfig(CombatType.PvE, Name = "Use Swiftcast")]
    // public SwiftType AddSwiftcast { get; set; } = SwiftType.Emerald;

    [RotationConfig(CombatType.PvE, Name = "Order")]
    public SummonOrderType SummonOrder { get; set; } = SummonOrderType.TopazEmeraldRuby;
    
    [RotationConfig(CombatType.PvE, Name = "Use radiant on cooldown. But still keeping one charge")]
    public bool RadiantOnCooldown { get; set; } = true;
    
    [RotationConfig(CombatType.PvE, Name = "Use medicine when available on burst -- need some test --")]
    public bool UseMedicine { get; set; } = false;
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
        // switch (AddSwiftcast)
        // {
        // case SwiftType.Emerald:
        //     if (InGaruda && Player.Level > 86)
        //     {
        //         if (SwiftcastPvE.CanUse(out act)) return true;
        //     }
        //     break;
        // case SwiftType.Ruby:
        //     if (InIfrit)
        //     {
        //         if (SwiftcastPvE.CanUse(out act)) return true;
        //     }
        //     break;
        // case SwiftType.All:
        //     if (InGaruda && Player.Level > 86 || InIfrit)
        //     {
        //         if (SwiftcastPvE.CanUse(out act)) return true;
        //     }
        //     break;
        // case SwiftType.No:
        //     break;
        // }
        
        if (RadiantOnCooldown && RadiantAegisPvE.Cooldown.CurrentCharges == 2 && (SummonBahamutPvE.Cooldown.IsCoolingDown && Player.Level < 100 || SummonSolarBahamutPvE.Cooldown.IsCoolingDown && Player.Level <= 100) && RadiantAegisPvE.CanUse(out act)) return true;
        if (RadiantOnCooldown && Player.Level < 88 && SummonBahamutPvE.Cooldown.IsCoolingDown && RadiantAegisPvE.CanUse(out act, false,false,false,true)) return true;

        
        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;
        
        // Adding tincture timing to rotations
        if (InBahamut || InSolarBahamut && UseMedicine && !Player.HasStatus(false, StatusID.SearingLight) && SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(0) || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(0) )
        {
            if (UseBurstMedicine(out act)) return true;
        }
        
        if (!Player.HasStatus(false, StatusID.SearingLight) && InBahamut || InSolarBahamut && SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(1) )
        {
            if (SearingLightPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }
        
        if ((InBahamut || InSolarBahamut && (SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || IsTargetBoss && IsTargetDying)) && EnergySiphonPvE.CanUse(out act)) return true;
        if ((InBahamut || InSolarBahamut && (SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || IsTargetBoss && IsTargetDying)) && EnergyDrainPvE.CanUse(out act)) return true;
        if ((InBahamut && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(4) || InPhoenix || IsTargetBoss && IsTargetDying)) && EnkindleBahamutPvE.CanUse(out act)) return true;
        if ((InSolarBahamut && (SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(4) || IsTargetBoss && IsTargetDying)) && EnkindleSolarBahamutPvE.CanUse(out act)) return true;
        if ((InBahamut && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(5) || IsTargetBoss && IsTargetDying)) && DeathflarePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if ((InSolarBahamut && (SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(5) || IsTargetBoss && IsTargetDying)) && SunflarePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (RekindlePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (MountainBusterPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((InBahamut || InSolarBahamut && Player.HasStatus(false, StatusID.SearingLight) && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(4) || !EnergyDrainPvE.Cooldown.IsCoolingDown) || !SearingLightPvE.EnoughLevel || IsTargetBoss && IsTargetDying) && PainflarePvE.CanUse(out act)) return true;

        if ((InBahamut || InSolarBahamut && Player.HasStatus(false, StatusID.SearingLight) && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(4) || !EnergyDrainPvE.Cooldown.IsCoolingDown) || !SearingLightPvE.EnoughLevel || IsTargetBoss && IsTargetDying) && FesterPvE.CanUse(out act) || NecrotizePvE.CanUse(out act)) return true;
        

        if ((InBahamut || InSolarBahamut && (SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(5) || IsTargetBoss && IsTargetDying)) && SearingFlashPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (DoesAnyPlayerNeedHeal() && !InBahamut || !InSolarBahamut && LuxSolarisPvE.CanUse(out act, skipAoeCheck: true)) return true;
        
        return base.AttackAbility(nextGCD,out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        if (SummonCarbunclePvE.CanUse(out act)) return true;

        if (IsBurst && (!SearingLightPvE.Cooldown.IsCoolingDown && SummonBahamutPvE.CanUse(out act) || SummonSolarBahamutPvE.CanUse(out act))) return true;

        if (SlipstreamPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (CrimsonStrikePvE.CanUse(out act, skipAoeCheck: true)) return true;

        
        if (PreciousBrilliancePvE.CanUse(out act)) return true;
        
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

    public bool DoesAnyPlayerNeedHeal()
    { 
        return PartyMembersAverHP < 80.0f;
    }

    #endregion
}
