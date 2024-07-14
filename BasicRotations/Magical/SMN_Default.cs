using System.ComponentModel;

namespace DefaultRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00")]
[SourceCode(Path = "main/DefaultRotations/Magical/SMN_Default.cs")]
[Api(2)]
public sealed class SMN_Default : SummonerRotation
{

	#region Config Options

	public enum SummonOrderType : byte
	{
		[Description("Topaz-Emerald-Ruby")] TopazEmeraldRuby,

		[Description("Topaz-Ruby-Emerald")] TopazRubyEmerald,

		[Description("Emerald-Topaz-Ruby")] EmeraldTopazRuby
	}

	[RotationConfig(CombatType.PvE, Name = "Use Crimson Cyclone. Will use at any range, regardless of saftey use with caution.")]
	public bool AddCrimsonCyclone { get; set; } = true;
	
	[RotationConfig(CombatType.PvE, Name = "Use Crimson Cyclone. Even When MOVING")]
	public bool AddCrimsonCycloneMoving { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Use Swiftcast on Garuda")]
	public bool AddSwiftcastOnGaruda { get; set; } = false;

	[RotationConfig(CombatType.PvE, Name = "Order")]
	public SummonOrderType SummonOrder { get; set; } = SummonOrderType.TopazEmeraldRuby;

	[RotationConfig(CombatType.PvE, Name = "Use radiant on cooldown. But still keeping one charge")]
	public bool RadiantOnCooldown { get; set; } = true;
	
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
		bool isTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
		bool isTargetDying = HostileTarget?.IsDying() ?? false;
		bool targetIsBossAndDying = isTargetBoss && isTargetDying;
		bool inBigInvocation = InBahamut || InPhoenix || InSolarBahamut;
		bool elapsedChargeAfterInvocation = SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD() || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD() || SummonPhoenixPvE.Cooldown.ElapsedOneChargeAfterGCD();
		bool elapsed1ChargeAfterInvocation = SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || SummonPhoenixPvE.Cooldown.ElapsedOneChargeAfterGCD(1);
		bool elapsed2ChargeAfterInvocation = SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(2) || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(2) || SummonPhoenixPvE.Cooldown.ElapsedOneChargeAfterGCD(2);
		bool elapsed3ChargeAfterInvocation = SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || SummonPhoenixPvE.Cooldown.ElapsedOneChargeAfterGCD(3);
		bool elapsed4ChargeAfterInvocation = SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(4) || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(4) || SummonPhoenixPvE.Cooldown.ElapsedOneChargeAfterGCD(4);
		
		if (inBigInvocation && (elapsed1ChargeAfterInvocation || targetIsBossAndDying) && EnergySiphonPvE.CanUse(out act)) return true;
		if (inBigInvocation && (elapsed1ChargeAfterInvocation || targetIsBossAndDying) && EnergyDrainPvE.CanUse(out act)) return true;
		if (inBigInvocation && (elapsed3ChargeAfterInvocation || targetIsBossAndDying) && EnkindleBahamutPvE.CanUse(out act)) return true;
		if (inBigInvocation && (elapsed3ChargeAfterInvocation || targetIsBossAndDying) && EnkindleSolarBahamutPvE.CanUse(out act)) return true;
		if (inBigInvocation && (elapsed2ChargeAfterInvocation || targetIsBossAndDying) && EnkindlePhoenixPvE.CanUse(out act)) return true;
		if (inBigInvocation && (elapsed3ChargeAfterInvocation || targetIsBossAndDying) && DeathflarePvE.CanUse(out act, skipAoeCheck: true)) return true;
		if (inBigInvocation && (elapsed3ChargeAfterInvocation || targetIsBossAndDying) && SunflarePvE.CanUse(out act, skipAoeCheck: true)) return true;

		if (RekindlePvE.CanUse(out act, skipAoeCheck: true)) return true;
		if (MountainBusterPvE.CanUse(out act, skipAoeCheck: true)) return true;

		if ((inBigInvocation && elapsed4ChargeAfterInvocation || !SearingLightPvE.EnoughLevel || isTargetBoss && isTargetDying) && PainflarePvE.CanUse(out act)) return true;
		if ((inBigInvocation && elapsed4ChargeAfterInvocation || !SearingLightPvE.EnoughLevel || isTargetBoss && isTargetDying) && FesterPvE.CanUse(out act) || NecrotizePvE.CanUse(out act)) return true;
		
		if ((elapsed4ChargeAfterInvocation || targetIsBossAndDying) && SearingFlashPvE.CanUse(out act, skipAoeCheck: true)) return true;
		if (DoesAnyPlayerNeedHeal() && !inBigInvocation && LuxSolarisPvE.CanUse(out act)) return true;

		return base.AttackAbility(nextGCD, out act);
	}
	#endregion

	protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
	{
		bool anyBigInvocationIsCoolingDown = SummonBahamutPvE.Cooldown.IsCoolingDown || SummonSolarBahamutPvE.Cooldown.IsCoolingDown || SummonPhoenixPvE.Cooldown.IsCoolingDown;
		bool elapsed1ChargeAfterInvocation = SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || SummonSolarBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || SummonPhoenixPvE.Cooldown.ElapsedOneChargeAfterGCD(1);

		if (!Player.HasStatus(false, StatusID.SearingLight) && InBahamut || InSolarBahamut && elapsed1ChargeAfterInvocation)
		{
			if (SearingLightPvE.CanUse(out act, skipAoeCheck: true)) return true;
		}
		
		if (AddSwiftcastOnGaruda && nextGCD == SlipstreamPvE && Player.Level > 86 && !InBahamut && !InPhoenix && !InSolarBahamut)
		{ 
			if (SwiftcastPvE.CanUse(out act)) return true;
		}
		
		if (RadiantOnCooldown && RadiantAegisPvE.Cooldown.CurrentCharges == 2 && (anyBigInvocationIsCoolingDown && Player.Level <= 100) && RadiantAegisPvE.CanUse(out act)) return true;
		if (RadiantOnCooldown && Player.Level < 88 && anyBigInvocationIsCoolingDown && RadiantAegisPvE.CanUse(out act)) return true;
		
		return base.EmergencyAbility(nextGCD, out act);
	}

	#region GCD Logic
	protected override bool GeneralGCD(out IAction? act)
	{
		bool inBigInvocation = InBahamut || InPhoenix || InSolarBahamut;
		bool inLittleInvocation = InIfrit || InGaruda || InTitan;	
		
		//if (SummonCarbunclePvE.CanUse(out act)) return true;
		
		if (!inLittleInvocation && SummonBahamutPvE.CanUse(out act)) return true;
		if ((Player.HasStatus(false, StatusID.SearingLight) || SearingLightPvE.Cooldown.IsCoolingDown) && SummonBahamutPvE.CanUse(out act)) return true;
		if (IsBurst && (!SearingLightPvE.Cooldown.IsCoolingDown && SummonSolarBahamutPvE.CanUse(out act))) return true;

		if (SlipstreamPvE.CanUse(out act, skipAoeCheck: true)) return true;

		if (CrimsonStrikePvE.CanUse(out act, skipAoeCheck: true)) return true;
		
		if (PreciousBrilliancePvE.CanUse(out act)) return true;

		if (GemshinePvE.CanUse(out act)) return true;

		if ((!IsMoving || AddCrimsonCycloneMoving) && AddCrimsonCyclone && CrimsonCyclonePvE.CanUse(out act, skipAoeCheck: true)) return true;
		
		if (!SummonBahamutPvE.EnoughLevel && HasHostilesInRange && AetherchargePvE.CanUse(out act)) return true;
		
		if (!InBahamut && !InPhoenix && !InSolarBahamut)
		{
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
		}
		
		if (SummonTimeEndAfterGCD() && AttunmentTimeEndAfterGCD() && !InBahamut && !InPhoenix && !InSolarBahamut && SummonEmeraldPvE.IsInCooldown && SummonTopazPvE.IsInCooldown && SummonRubyPvE.IsInCooldown &&
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
		return PartyMembersAverHP < 0.8f;
	}
	#endregion

}