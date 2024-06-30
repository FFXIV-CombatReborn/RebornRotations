namespace DefaultRotations.Healer;

[Rotation("Default", CombatType.PvE, GameVersion = "6.58")]
[SourceCode(Path = "main/DefaultRotations/Healer/SCH_Default.cs")]
[Api(1)]
public sealed class SCH_Default : ScholarRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use spells with cast times to heal. (Ignored if you are the only healer in party)")]
    public bool GCDHeal { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Recitation at 15 seconds remaining on Countdown.")]
    public bool PrevDUN { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Give Adloquium/Galvanize to Tank during Countdown (Requires above enabled)")]
    public bool GiveT { get; set; } = false;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Remove Aetherpact to conserve resources if party member is above this percentage")]
    public float AetherpactRemove { get; set; } = 0.9f;
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < RuinPvE.Info.CastTime + CountDownAhead
            && RuinPvE.CanUse(out var act)) return act;

        if (PrevDUN && remainTime <= 15 && !DeploymentTacticsPvE.Cooldown.IsCoolingDown && PartyMembers.Count() > 1)
        {

            if (!RecitationPvE.Cooldown.IsCoolingDown) return RecitationPvE;
            if (!PartyMembers.Any((n) => n.HasStatus(true, StatusID.Galvanize)))
            {
                if (GiveT)
                {
                    return AdloquiumPvE;
                }
            }
            else
            {
                return DeploymentTacticsPvE;
            }
        }
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region GCD Logic
    [RotationDesc(ActionID.SuccorPvE)]
    protected override bool HealAreaGCD(out IAction? act)
    {
        if (SuccorPvE.CanUse(out act)) return true;

        return base.HealAreaGCD(out act);
    }

    [RotationDesc(ActionID.AdloquiumPvE, ActionID.PhysickPvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        if (AdloquiumPvE.CanUse(out act)) return true;
        if (PhysickPvE.CanUse(out act)) return true;

        return base.HealSingleGCD(out act);
    }

    [RotationDesc(ActionID.SuccorPvE)]
    protected override bool DefenseAreaGCD(out IAction? act)
    {
        if (SuccorPvE.CanUse(out act)) return true;
        return base.DefenseAreaGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (SummonEosPvE.CanUse(out act)) return true;
        if (BioPvE.CanUse(out act) && AllHostileTargets.Where(p => p.DistanceToPlayer() < 5).Count() < 4) return true;

        //AOE
        if (ArtOfWarPvE.CanUse(out act)) return true;

        //Single
        if (RuinPvE.CanUse(out act)) return true;
        if (RuinIiPvE.CanUse(out act)) return true;

        //Add dot.
        if (BioPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD.IsTheSameTo(true, SuccorPvE, AdloquiumPvE))
        {
            if (RecitationPvE.CanUse(out act)) return true;
        }

        //Remove Aetherpact
        foreach (var item in PartyMembers)
        {
            if (item.GetHealthRatio() < AetherpactRemove) continue;
            if (item.HasStatus(true, StatusID.FeyUnion_1223))
            {
                act = AetherpactPvE;
                return true;
            }
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.SummonSeraphPvE, ActionID.ConsolationPvE, ActionID.WhisperingDawnPvE, ActionID.SacredSoilPvE, ActionID.IndomitabilityPvE)]
    protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (WhisperingDawnPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || FeyIlluminationPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || FeyBlessingPvE.Cooldown.ElapsedOneChargeAfterGCD(1))
        {
            if (SummonSeraphPvE.CanUse(out act)) return true;
        }
        if (ConsolationPvE.CanUse(out act, usedUp: true)) return true;
        if (FeyBlessingPvE.CanUse(out act)) return true;

        if (WhisperingDawnPvE.CanUse(out act)) return true;
        if (SacredSoilPvE.CanUse(out act)) return true;
        if (IndomitabilityPvE.CanUse(out act)) return true;

        return base.HealAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.AetherpactPvE, ActionID.ProtractionPvE, ActionID.SacredSoilPvE, ActionID.ExcogitationPvE, ActionID.LustratePvE, ActionID.AetherpactPvE)]
    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        var haveLink = PartyMembers.Any(p => p.HasStatus(true, StatusID.FeyUnion_1223));

        if (AetherpactPvE.CanUse(out act) && FairyGauge >= 70 && !haveLink) return true;
        if (ProtractionPvE.CanUse(out act)) return true;
        if (SacredSoilPvE.CanUse(out act)) return true;
        if (ExcogitationPvE.CanUse(out act)) return true;
        if (LustratePvE.CanUse(out act)) return true;
        if (AetherpactPvE.CanUse(out act) && !haveLink) return true;

        return base.HealSingleAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.FeyIlluminationPvE, ActionID.ExpedientPvE, ActionID.SummonSeraphPvE, ActionID.ConsolationPvE, ActionID.SacredSoilPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (FeyIlluminationPvE.CanUse(out act)) return true;
        if (ExpedientPvE.CanUse(out act)) return true;

        if (WhisperingDawnPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || FeyIlluminationPvE.Cooldown.ElapsedOneChargeAfterGCD(1) || FeyBlessingPvE.Cooldown.ElapsedOneChargeAfterGCD(1))
        {
            if (SummonSeraphPvE.CanUse(out act)) return true;
        }
        if (ConsolationPvE.CanUse(out act, usedUp: true)) return true;
        if (SacredSoilPvE.CanUse(out act)) return true;

        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.ExcogitationPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (ExcogitationPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (IsBurst)
        {
            if (ChainStratagemPvE.CanUse(out act)) return true;
        }

        if (DissipationPvE.EnoughLevel && DissipationPvE.Cooldown.WillHaveOneChargeGCD(3) && DissipationPvE.IsEnabled || AetherflowPvE.Cooldown.WillHaveOneChargeGCD(3))
        {
            if (EnergyDrainPvE.CanUse(out act, usedUp: true)) return true;
        }

        if (DissipationPvE.CanUse(out act)) return true;
        if (AetherflowPvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region Extra Methods
    public override bool CanHealSingleSpell => base.CanHealSingleSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);
    public override bool CanHealAreaSpell => base.CanHealAreaSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);

    #endregion
}
