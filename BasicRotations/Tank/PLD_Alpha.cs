
namespace DefaultRotations.Tank;

[Rotation("PLD Alpha", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/DefaultRotations/Tank/PLD_Default.cs")]
[Api(3)]
public class PLD_Alpha : PaladinRotation
{
    #region Config Options

    [RotationConfig(CombatType.PvE, Name = "Use Hallowed Ground with Cover")]
    private bool HallowedWithCover { get; set; } = true;
    
    [Range(1, 8,ConfigUnitType.Pixels)]
    [RotationConfig(CombatType.PvE,Name = "How many GCDs to delay burst by (Assumes you open with Holy Spirit, 2 is best for melee opening) ")]
    private int AdjustedBurst { get; set; } = 3;
    
    [RotationConfig(CombatType.PvE, Name = "Prioritize Atonement Combo During Fight or Flight outside of Opener (Might not good for Dungeons Packs)")]
    private bool PrioritizeAtonementCombo { get; set; } = false;
    
    [RotationConfig(CombatType.PvE, Name = "Use Holy Spirit First (For if you want to MinMax it)")]
    private bool MinMaxHolySpirit { get; set; } = false;
    
    [RotationConfig(CombatType.PvE, Name = "Use Divine Veil at 15 seconds remaining on Countdown")]
    private bool UseDivineVeilPre { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use Holy Circle or Holy Spirit when out of melee range")]
    private bool UseHolyWhenAway { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use Shield Bash when Low Blow is cooling down")]
    private bool UseShieldBash { get; set; } = true;
    
    [RotationConfig(CombatType.PvE,Name = "Allow the Use of Shield Lob")]
    private bool UseShieldLob { get; set; } = true;
    
    [RotationConfig(CombatType.PvE, Name = "Maximize Damage if Target if considered dying")]
    private bool BurstTargetIfConsideredDying { get; set; } = false;
    
    [Range(0, 100, ConfigUnitType.Pixels)]
    [RotationConfig(CombatType.PvE, Name = "Use Sheltron at minimum X Oath to prevent over cap (Set to 0 to disable)")]
    private int WhenToSheltron { get; set; } = 100;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Health threshold for Intervention (Set to 0 to disable)")]
    private float InterventionRatio { get; set; } = 0.6f;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Health threshold for Cover (Set to 0 to disable)")]
    private float CoverRatio { get; set; } = 0.3f;

    private bool HasAtonementReady => Player.HasStatus(true, StatusID.AtonementReady);
    private bool HasSupplicationReady => Player.HasStatus(true, StatusID.SupplicationReady);
    private bool HasSepulchreReady => Player.HasStatus(true, StatusID.SepulchreReady);
    private bool HasHonorReady => Player.HasStatus(true, StatusID.BladeOfHonorReady);
    private bool TargetIsDying => (HostileTarget?.IsDying() ?? false) && BurstTargetIfConsideredDying;

    private bool HolySpiritFirst(out IAction? act)
    {
        act = null;
        if (MinMaxHolySpirit && HasDivineMight && HolySpiritPvE.CanUse(out act)) return true;
        return false;
    }

    private const ActionID ConfiteorPvEActionId = (ActionID)16459;
    private new readonly IBaseAction ConfiteorPvE = new BaseAction(ConfiteorPvEActionId);
    #endregion
    
    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {   
        if (remainTime < HolySpiritPvE.Info.CastTime + CountDownAhead
            && HolySpiritPvE.CanUse(out var act)) return act;

        if (remainTime < 15 && UseDivineVeilPre
            && DivineVeilPvE.CanUse(out act)) return act;
        
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        {
            if (InCombat)
            {
                if (HasHonorReady && BladeOfHonorPvE.CanUse(out act, skipAoeCheck: true)) return true;

                if ((OathGauge >= WhenToSheltron) && (WhenToSheltron) > 0 && UseOath(out act)) return true;
            }

            if (Player.HasStatus(true, StatusID.Cover) && HallowedWithCover && HallowedGroundPvE.CanUse(out act)) return true;

            if ((Player.HasStatus(true, StatusID.Rampart) || Player.HasStatus(true, StatusID.Sentinel)) &&
                InterventionPvE.CanUse(out act) &&
                InterventionPvE.Target.Target?.GetHealthRatio() < 0.6) return true;

            if (CoverPvE.CanUse(out act) && CoverPvE.Target.Target?.DistanceToPlayer() < 10 &&
                CoverPvE.Target.Target?.GetHealthRatio() < CoverRatio) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.ReprisalPvE, ActionID.DivineVeilPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {

        if (DivineVeilPvE.CanUse(out act)) return true;
        if (!Player.HasStatus(true, StatusID.Bulwark) && ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (PassageOfArmsPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.SentinelPvE, ActionID.RampartPvE, ActionID.BulwarkPvE, ActionID.SheltronPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {

        // If the player has the Hallowed Ground status, don't use any abilities.
        if (!Player.HasStatus(true, StatusID.HallowedGround))
        {
            // If Bulwark can be used, use it and return true.
            if (BulwarkPvE.CanUse(out act, skipAoeCheck: true)) return true;

            // If Oath can be used, use it and return true.
            if (UseOath(out act)) return true;

            // If Rampart is not cooling down or has been cooling down for more than 60 seconds, and Sentinel can be used, use Sentinel and return true.
            if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && SentinelPvE.CanUse(out act)) return true;

            // If Sentinel is at an enough level and is cooling down for more than 60 seconds, or if Sentinel is not at an enough level, and Rampart can be used, use Rampart and return true.
            if ((SentinelPvE.EnoughLevel && SentinelPvE.Cooldown.IsCoolingDown && SentinelPvE.Cooldown.ElapsedAfter(60) || !SentinelPvE.EnoughLevel) && RampartPvE.CanUse(out act)) return true;

            // If Reprisal can be used, use it and return true.
            if (ReprisalPvE.CanUse(out act)) return true;

        }

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (WeaponRemain > 0.42f)
        {
            act = null;
            /*
            if (InCombat && !UseAdjustedBurst)
            {
                if (UseBurstMedicine(out act)) return true;
                if (IsBurst && !CombatElapsedLess(5) && FightOrFlightPvE.CanUse(out act)) return true;
            }
            */
            
            if (InCombat && !CombatElapsedLess(30) || (InCombat && !CombatElapsedLessGCD(AdjustedBurst)) || FightOrFlightPvE.Cooldown.IsCoolingDown && CombatElapsedLess(30))
            {
                if (FightOrFlightPvE.CanUse(out act)) return true;
                if (RequiescatPvE.CanUse(out act, skipAoeCheck: true, usedUp: HasFightOrFlight)) return true;
            }
            if (CombatElapsedLessGCD(AdjustedBurst + 1)) return false;

            if (CircleOfScornPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (SpiritsWithinPvE.CanUse(out act, skipAoeCheck: true)) return true;

            if (!IsMoving && IntervenePvE.CanUse(out act, skipAoeCheck: true, usedUp: HasFightOrFlight)) return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act) 
    {
        //Minimizes Accidents in EX and Savage Hopefully
        if (IsInHighEndDuty && !InCombat){act = null; return false;}
        
        // if (Player.TargetObject?.TargetObjectId == Player.GameObjectId && HolySpiritPvE.CanUse(out act)) return true;
        
        if (HasHonorReady && BladeOfHonorPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (Player.HasStatus(true, StatusID.Requiescat))
        {
            if ((Player.Level >= 90) && (Player.StatusStack(true, StatusID.Requiescat) < 4))
            {
                if (!TargetIsDying && PrioritizeAtonementCombo && !CombatElapsedLess(30) && (Player.StatusTime(true, StatusID.FightOrFlight) > 12) && AtonementCombo(out act)) return true;
                //if (BladeOfValorPvE.CanUse(out act, skipAoeCheck: true)) return true;
                //if (BladeOfTruthPvE.CanUse(out act, skipAoeCheck: true)) return true;
                //if (BladeOfFaithPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (ConfiteorPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            if ((Player.Level >= 80) && (Player.StatusStack(true, StatusID.Requiescat) > 3))
            {
                if (!TargetIsDying && PrioritizeAtonementCombo && !CombatElapsedLess(30) && (Player.StatusTime(true, StatusID.FightOrFlight) > 12) && AtonementCombo(out act)) return true;
                if (ConfiteorPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            if (HolyCirclePvE.CanUse(out act)) return true;
            if (HolySpiritPvE.CanUse(out act)) return true;
        }
        
        //if ((Player.TargetObject?.GetHealthRatio() < 0.05f) && AtonementCombo(out act)) return true;
        
        //AOE
        if (HasDivineMight && HolyCirclePvE.CanUse(out act)) return true;
        if (ProminencePvE.CanUse(out act)) return true;
        if (TotalEclipsePvE.CanUse(out act)) return true;

        //Single
        if (!CombatElapsedLess(8) && HasFightOrFlight && GoringBladePvE.CanUse(out act)) return true; // Dot
        if (UseShieldBash && ShieldBashPvE.CanUse(out act)) return true;
        
        if (Player.HasStatus(true, StatusID.FightOrFlight) && AtonementCombo(out act)) return true;

        if (FightOrFlightPvE.Cooldown.WillHaveOneChargeGCD(4) &&
            (HasSepulchreReady || HasSupplicationReady))
        {
            if (HolySpiritFirst(out act)) return true;
            if (SepulchrePvE.CanUse(out act)) return true;
            if (!HasSupplicationReady)
                if (HasDivineMight && HolySpiritPvE.CanUse(out act)) return true;
            if (RageOfHalonePvE.CanUse(out act))
            {
                if (SupplicationPvE.CanUse(out act)) return true;
                return true;
            }
            if (RiotBladePvE.CanUse(out act)) return true;
            if (FastBladePvE.CanUse(out act)) return true;
        }
        
        if (FightOrFlightPvE.Cooldown.WillHaveOneChargeGCD(2) && (HasSupplicationReady || !HasDivineMight))
        {
            if (RageOfHalonePvE.CanUse(out act)) return true;
            if (RiotBladePvE.CanUse(out act)) return true;
            if (FastBladePvE.CanUse(out act)) return true;
        }
            
        
        if (FightOrFlightPvE.Cooldown.WillHaveOneChargeGCD(2) && HasDivineMight)
        {
            if (RageOfHalonePvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (HolySpiritFirst(out act)) return true;
            if (AtonementPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (HasDivineMight && HolySpiritPvE.CanUse(out act)) return true;
        }

        
        if (((HasSupplicationReady || HasSepulchreReady || ((!HasSupplicationReady && !HasSepulchreReady && !HasAtonementReady && HasDivineMight)) &&
            FightOrFlightPvE.Cooldown.WillHaveOneChargeGCD(5))))
        {
            if (HolySpiritFirst(out act)) return true;
            if (AtonementPvE.CanUse(out act)) return true;
            if (SupplicationPvE.CanUse(out act)) return true;
            if (SepulchrePvE.CanUse(out act)) return true;
            if (HasDivineMight && (HolySpiritPvE.CanUse(out act))) return true;
            if (RageOfHalonePvE.CanUse(out act)) return true;
            if (RiotBladePvE.CanUse(out act)) return true;
            if (FastBladePvE.CanUse(out act)) return true;
        }
        
        if (!(HasSupplicationReady || HasSepulchreReady || HasDivineMight) && RageOfHalonePvE.CanUse(out act)) return true;
        
        if (HolySpiritFirst(out act)) return true;

        if (AtonementPvE.CanUse(out act, skipAoeCheck: true)) return true;
       
        if (HasSupplicationReady && (Player.StatusTime(true,StatusID.SupplicationReady) > 25) && (RiotBladePvE.CanUse(out act) || FastBladePvE.CanUse(out act))) return true;
        
        if (HasSupplicationReady && SupplicationPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (HasSepulchreReady && SepulchrePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (HasDivineMight && HolySpiritPvE.CanUse(out act)) return true;
        
        if (RiotBladePvE.CanUse(out act) || FastBladePvE.CanUse(out act)) return true;
        
        //Range
        if (UseHolyWhenAway)
        {
            if (HolyCirclePvE.CanUse(out act)) return true;
            if (HolySpiritPvE.CanUse(out act)) return true;
        }
        
        if (UseShieldLob && ShieldLobPvE.CanUse(out act)) return true;
        
        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    
    private bool AtonementCombo(out IAction? act) => HolySpiritFirst(out act) || GoringBladePvE.CanUse(out act) || AtonementPvE.CanUse(out act) || SupplicationPvE.CanUse(out act) || SepulchrePvE.CanUse(out act) || HasDivineMight && HolyCirclePvE.CanUse(out act) ||HasDivineMight && HolySpiritPvE.CanUse(out act);

    private bool UseOath(out IAction? act)
    {
        act = null; 
        if ((InterventionPvE.Target.Target?.GetHealthRatio() <= InterventionRatio) && InterventionPvE.CanUse(out act)) return true;
        if (SheltronPvE.CanUse(out act)) return true;
        return false;
    }
    #endregion
}