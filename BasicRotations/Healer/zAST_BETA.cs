namespace DefaultRotations.Healer;

[Rotation("zAST Beta", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/DefaultRotations/Healer/zAST_BETA.cs")]
[Api(3)]
public sealed class zAST_BETA : AstrologianRotation
{
    #region Config Options

    [Range(4, 20, ConfigUnitType.Seconds)]
    [RotationConfig(CombatType.PvE, Name = "Use Earthly Star during countdown timer.")]
    public float UseEarthlyStarTime { get; set; } = 15;
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < MaleficPvE.Info.CastTime + CountDownAhead
            && MaleficPvE.CanUse(out var act)) return act;
        if (remainTime < 3 && UseBurstMedicine(out act)) return act;
        if (remainTime is < 4 and > 3 && AspectedBeneficPvE.CanUse(out act)) return act;
        if (remainTime < UseEarthlyStarTime
            && EarthlyStarPvE.CanUse(out act)) return act;
        if (remainTime < 30 && AstralDrawPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Additional oGCD Logic

    [RotationDesc(ActionID.HoroscopePvE, ActionID.NeutralSectPvE, ActionID.SynastryPvE)]
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if (!InCombat) return false;

        if (nextGCD.IsTheSameTo(true, AspectedHeliosPvE, HeliosPvE, HeliosConjunctionPvE))
        {
            if (HoroscopePvE.CanUse(out act)) return true;
            if (NeutralSectPvE.CanUse(out act)) return true;
        }

        if (nextGCD.IsTheSameTo(true, BeneficPvE, BeneficIiPvE, AspectedBeneficPvE))
        {
            if (SynastryPvE.CanUse(out act)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.SunSignPvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (SunSignPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.ExaltationPvE, ActionID.TheSpirePvE, ActionID.TheBolePvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat && TheSpirePvE.CanUse(out act)) return true;
        if (InCombat && TheBolePvE.CanUse(out act)) return true;

        if (ExaltationPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.CelestialOppositionPvE, ActionID.StellarDetonationPvE, ActionID.HoroscopePvE, ActionID.HoroscopePvE_16558, ActionID.LadyOfCrownsPvE, ActionID.HeliosConjunctionPvE)]
    protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (CelestialOppositionPvE.CanUse(out act)) return true;

        if (StellarDetonationPvE.CanUse(out act)) return true;

        if (HoroscopePvE.CanUse(out act)) return true;

        if (HoroscopePvE_16558.CanUse(out act)) return true;

        if (LadyOfCrownsPvE.CanUse(out act)) return true;

        if (HeliosConjunctionPvE.CanUse(out act)) return true;
        return base.HealAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.TheArrowPvE, ActionID.TheEwerPvE, ActionID.EssentialDignityPvE,
        ActionID.CelestialIntersectionPvE)]
    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (MicrocosmosPvE.CanUse(out act)) return true;

        if (InCombat && TheArrowPvE.CanUse(out act)) return true;

        if (InCombat && TheEwerPvE.CanUse(out act)) return true;

        if (EssentialDignityPvE.CanUse(out act)) return true;

        if (CelestialIntersectionPvE.CanUse(out act, usedUp: true)) return true;

        return base.HealSingleAbility(nextGCD, out act);
    }
    #endregion

    #region oGCD Logic

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat && TheBalancePvE.CanUse(out act)) return true;
        if (InCombat && TheSpearPvE.CanUse(out act)) return true;
        if (AstralDrawPvE.CanUse(out act)) return true;
        if (UmbralDrawPvE.CanUse(out act)) return true;
        if (MinorArcanaPvE.CanUse(out act)) return true;
        return base.GeneralAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (IsBurst && !IsMoving
            && DivinationPvE.CanUse(out act)) return true;

        if (AstralDrawPvE.CanUse(out act, usedUp: IsBurst)) return true;

        if (InCombat)
        {
            if (IsMoving && LightspeedPvE.CanUse(out act)) return true;

            if (!IsMoving)
            {
                if (!Player.HasStatus(true, StatusID.EarthlyDominance, StatusID.GiantDominance))
                {
                    if (EarthlyStarPvE.CanUse(out act)) return true;
                }
            }

            {
                if (LordOfCrownsPvE.CanUse(out act)) return true;
            }
        }

        if (OraclePvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    [RotationDesc(ActionID.AspectedHeliosPvE, ActionID.HeliosPvE)]
    protected override bool HealAreaGCD(out IAction? act)
    {
        if (HeliosConjunctionPvE.CanUse(out act)) return true;

        if (HeliosPvE.CanUse(out act)) return true;
        return base.HealAreaGCD(out act);
    }

    [RotationDesc(ActionID.AspectedBeneficPvE, ActionID.BeneficIiPvE, ActionID.BeneficPvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        if (AspectedBeneficPvE.CanUse(out act) && (IsMoving)) return true;

        if (BeneficIiPvE.CanUse(out act)) return true;

        if (BeneficPvE.CanUse(out act)) return true;

        return base.HealSingleGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (MacrocosmosPvE.CanUse(out act)) return true;

        if (GravityIiPvE.CanUse(out act)) return true;

        if (CombustIiiPvE.CanUse(out act)) return true;

        if (FallMaleficPvE.CanUse(out act)) return true;

        if (CombustIiiPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

        return base.GeneralGCD(out act);
    }

    #endregion
}
