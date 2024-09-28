namespace DefaultRotations.Ranged;

[Rotation("zMCH Beta 2", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/BasicRotations/Ranged/zMCH_Beta_2.cs")]
[Api(4)]
public sealed class zMCH_Beta_2 : MachinistRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use hardcoded Queen timings\nSlight DPS gain if uninterrupted but possibly loses more from drift or death.")]
    private bool UseBalanceQueenTimings { get; set; }

    [RotationConfig(CombatType.PvE, Name = "Use burst medicine in countdown")]
    private bool OpenerBurstMeds { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use burst medicine when available for midfight burst phase")]
    private bool MidfightBurstMeds { get; set; } = false;
    #endregion

    private const float HYPERCHARGE_DURATION = 8f;

    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        // ReassemblePvE's duration is 5s, need to fire the first GCD before it ends
        if (remainTime < 5 && ReassemblePvE.CanUse(out var act)) return act;
        // tincture needs to be used on -2s exactly
        if (OpenerBurstMeds && remainTime <= 2 && UseBurstMedicine(out act)) return act;
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    // Determines emergency actions to take based on the next planned GCD action.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (IsBurst && MidfightBurstMeds && !CombatElapsedLessGCD(10) && TimeForBurstMeds(out act, nextGCD)) return true;
        if (IsBurst)
        {
            
            if (FullMetalFieldPvE.EnoughLevel)
            {
                // Use Wildfire before FMF in the second half of the GCD window to avoid wasting time in status
                if (WeaponRemain < 1.25f && nextGCD.IsTheSameTo(true, FullMetalFieldPvE)
                    && Player.HasStatus(true, StatusID.Hypercharged)
                    && WildfirePvE.CanUse(out act, isLastAbility: true)) return true;
            }
            // Legacy logic for <100
            else if ((IsLastAbility(false, HyperchargePvE) 
                    || Heat >= 50 
                    || Player.HasStatus(true, StatusID.Hypercharged)) 
                && ToolChargeSoon(out _) 
                && !LowLevelHyperCheck 
                && WildfirePvE.CanUse(out act)) return true;
        }

        // Reassemble Logic
        // Check next GCD action and conditions for Reassemble.
        bool isReassembleUsable =
            //Reassemble current # of charges and double proc protection
            ReassemblePvE.Cooldown.CurrentCharges > 0 && !Player.HasStatus(true, StatusID.Reassembled) &&
            (nextGCD.IsTheSameTo(true, [ChainSawPvE, ExcavatorPvE]) || nextGCD.IsTheSameTo(false, [AirAnchorPvE]) ||
             (!ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, DrillPvE)) ||
             (!DrillPvE.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShotPvE)) ||
             (!CleanShotPvE.EnoughLevel && nextGCD.IsTheSameTo(false, HotShotPvE)));
        // Attempt to use Reassemble if it's ready
        if (isReassembleUsable)
        {
            if (ReassemblePvE.CanUse(out act, skipComboCheck: true, usedUp: true)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.TacticianPvE, ActionID.DismantlePvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction act)
    {
        if (TacticianPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (DismantlePvE.CanUse(out act, skipAoeCheck: true)) return true;
        return false;
    }

    // Logic for using attack abilities outside of GCD, focusing on burst windows and cooldown management.
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        // If Wildfire is active, use Hypercharge.....Period
        if (Player.HasStatus(true, StatusID.Wildfire_1946) && HyperchargePvE.CanUse(out act)) return true;

        // don't do anything that might fuck with burst timings at 100
        if (nextGCD.IsTheSameTo(true, FullMetalFieldPvE) || IsLastGCD(true, FullMetalFieldPvE))
        {
            act = null;
            return false;
        }

        // Start Ricochet/Gauss cooldowns rolling
        if (!RicochetPvE.Cooldown.IsCoolingDown && RicochetPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (!GaussRoundPvE.Cooldown.IsCoolingDown && GaussRoundPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (IsBurst && IsLastGCD(true, DrillPvE) && BarrelStabilizerPvE.CanUse(out act)) return true;

        // Rook Autoturret/Queen Logic
        if (CanUseQueenMeow(out act, nextGCD)) return true;

        // Use Hypercharge if wildfire will not be up in 30 seconds or if you hit 100 heat and it will not break your combo
        if (!LowLevelHyperCheck
            && !Player.HasStatus(true, StatusID.Reassembled) 
            && (!WildfirePvE.Cooldown.WillHaveOneCharge(30) || Heat == 100) 
            && !(LiveComboTime <= HYPERCHARGE_DURATION && LiveComboTime > 0f)
            && ToolChargeSoon(out act)) return true;

        // Use Ricochet and Gauss if have pooled charges or is burst window
        if (IsRicochetMore)
        {
            if ((IsLastGCD(true, BlazingShotPvE, HeatBlastPvE)
                || RicochetPvE.Cooldown.RecastTimeElapsed >= 45
                || !BarrelStabilizerPvE.Cooldown.ElapsedAfter(20))
                && RicochetPvE.CanUse(out act, skipAoeCheck: true, usedUp: true))
                return true;
        }
            
        if ((IsLastGCD(true, BlazingShotPvE, HeatBlastPvE)
            || GaussRoundPvE.Cooldown.RecastTimeElapsed >= 45
            || !BarrelStabilizerPvE.Cooldown.ElapsedAfter(20))
            && GaussRoundPvE.CanUse(out act, usedUp: true, skipAoeCheck: true))
            return true;


        if (IsBurst && !FullMetalFieldPvE.EnoughLevel)
        {
            if (BarrelStabilizerPvE.CanUse(out act)) return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        // use procs asap
        if (ExcavatorPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (!ChainSawPvE.Cooldown.WillHaveOneChargeGCD(2) && FullMetalFieldPvE.CanUse(out act)) return true;

        // overheated aoe
        if (AutoCrossbowPvE.CanUse(out act)) return true;
        // overheated single
        if (HeatBlastPvE.CanUse(out act)) return true;

        // drill's aoe version
        if (BioblasterPvE.CanUse(out act, usedUp: true)) return true;

        // single target --- need to update this strange condition writing!!!
        if (!SpreadShotPvE.CanUse(out _))
        {
            // use AirAnchor if possible
            if (HotShotMasteryTrait.EnoughLevel && AirAnchorPvE.CanUse(out act)) return true;

            // for burst: use Drill after AirAnchor
            if (IsLastGCD(true, AirAnchorPvE) && EnhancedMultiweaponTrait.EnoughLevel && DrillPvE.CanUse(out act, usedUp: true)) return true;
            if (!EnhancedMultiweaponTrait.EnoughLevel && DrillPvE.CanUse(out act, usedUp: true)) return true;

            if (!AirAnchorPvE.EnoughLevel && HotShotPvE.CanUse(out act)) return true;
        }

        // ChainSaw is always used after Drill
        if (ChainSawPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // save Drill for burst
        if (EnhancedMultiweaponTrait.EnoughLevel
            && !ChainSawPvE.Cooldown.WillHaveOneCharge(6) 
            && (!(LiveComboTime <= 6) || (!CleanShotPvE.CanUse(out _) && !SlugShotPvE.CanUse(out _)))
            && DrillPvE.CanUse(out act, usedUp: true)) return true;

        // basic aoe
        if (SpreadShotPvE.CanUse(out act)) return true;

        // single target 123 combo
        if (CleanShotPvE.CanUse(out act)) return true;
        if (SlugShotPvE.CanUse(out act)) return true;
        if (SplitShotPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    // Extra private helper methods for determining the usability of specific abilities under certain conditions.
    // These methods simplify the main logic by encapsulating specific checks related to abilities' cooldowns and prerequisites.
    // Logic for Hypercharge
    private bool ToolChargeSoon(out IAction? act)
    {
        if
            //Cannot AOE
            (!SpreadShotPvE.CanUse(out _)
            &&
            // AirAnchor Enough Level % AirAnchor 
            ((AirAnchorPvE.EnoughLevel && AirAnchorPvE.Cooldown.WillHaveOneCharge(HYPERCHARGE_DURATION))
            ||
            // HotShot Charge Detection
            (!AirAnchorPvE.EnoughLevel && HotShotPvE.EnoughLevel && HotShotPvE.Cooldown.WillHaveOneCharge(HYPERCHARGE_DURATION))
            ||
            // Drill Charge Detection
            (DrillPvE.EnoughLevel && DrillPvE.Cooldown.WillHaveXCharges(DrillPvE.Cooldown.MaxCharges, HYPERCHARGE_DURATION))
            ||
            // Chainsaw Charge Detection
            (ChainSawPvE.EnoughLevel && ChainSawPvE.Cooldown.WillHaveOneCharge(HYPERCHARGE_DURATION))))
        {
            act = null;
            return false;
        }
        else
        {
            return HyperchargePvE.CanUse(out act);
        }
    }

    private bool CanUseQueenMeow(out IAction? act, IAction nextGCD)
    {
        bool QueenOne = Battery >= 60 && CombatElapsedLess(25f);
        bool QueenTwo = Battery >= 90 && !CombatElapsedLess(58f) && CombatElapsedLess(78f);
        bool QueenThree = Battery >= 100 && !CombatElapsedLess(111f) && CombatElapsedLess(131f);
        bool QueenFour = Battery >= 50 && !CombatElapsedLess(148f) && CombatElapsedLess(168f);
        bool QueenFive = Battery >= 60 && !CombatElapsedLess(178f) && CombatElapsedLess(198f);
        bool QueenSix = Battery >= 100 && !CombatElapsedLess(230f) && CombatElapsedLess(250f);
        bool QueenSeven = Battery >= 50 && !CombatElapsedLess(268f) && CombatElapsedLess(288f);
        bool QueenEight = Battery >= 70 && !CombatElapsedLess(296f) && CombatElapsedLess(316f);
        bool QueenNine = Battery >= 100 && !CombatElapsedLess(350f) && CombatElapsedLess(370f);
        bool QueenTen = Battery >= 50 && !CombatElapsedLess(388f) && CombatElapsedLess(408f);
        bool QueenEleven = Battery >= 80 && !CombatElapsedLess(416f) && CombatElapsedLess(436f);
        bool QueenTwelve = Battery >= 100 && !CombatElapsedLess(470f) && CombatElapsedLess(490f);
        bool QueenThirteen = Battery >= 50 && !CombatElapsedLess(505f) && CombatElapsedLess(525f);
        bool QueenFourteen = Battery >= 60 && !CombatElapsedLess(538f) && CombatElapsedLess(558f);
        bool QueenFifteen = Battery >= 100 && !CombatElapsedLess(590f) && CombatElapsedLess(610f);

        if (UseBalanceQueenTimings && (QueenOne || QueenTwo || QueenThree || QueenFour || QueenFive || QueenSix || QueenSeven || QueenEight || QueenNine || QueenTen || QueenEleven || QueenTwelve || QueenThirteen || QueenFourteen || QueenFifteen))
        {
            if (RookAutoturretPvE.CanUse(out act)) return true;
        }
        // take over with normal logic after queen timings run out in long fights
        else if ((!UseBalanceQueenTimings || !CombatElapsedLess(610f)) &&
            // ASAP in opener
            (CombatElapsedLessGCD(10)
            // In first ~10 seconds of 2 minute window
            || (!AirAnchorPvE.Cooldown.ElapsedAfter(10) && (BarrelStabilizerPvE.Cooldown.WillHaveOneChargeGCD(4) || !BarrelStabilizerPvE.Cooldown.ElapsedAfter(5))
            // or if about to overcap
            || (nextGCD.IsTheSameTo(true, CleanShotPvE) && Battery == 100)
            || (nextGCD.IsTheSameTo(true, AirAnchorPvE, ChainSawPvE, ExcavatorPvE) && (Battery == 90 || Battery == 100))
            )))
        {
            if (RookAutoturretPvE.CanUse(out act)) return true;
        }
        act = null;
        return false;
    }

    // Check for not burning Hypercharge below level 52 on AOE
    private bool LowLevelHyperCheck => !AutoCrossbowPvE.EnoughLevel && SpreadShotPvE.CanUse(out _);

    private bool TimeForBurstMeds(out IAction? act, IAction nextGCD)
    {
        if (AirAnchorPvE.Cooldown.WillHaveOneChargeGCD(2) && BarrelStabilizerPvE.Cooldown.WillHaveOneChargeGCD(6) && WildfirePvE.Cooldown.WillHaveOneChargeGCD(6)) return UseBurstMedicine(out act);
        act = null;
        return false;
    }

    // Keeps Ricochet and Gauss Cannon Even
    private bool IsRicochetMore => RicochetPvE.EnoughLevel && GaussRoundPvE.Cooldown.RecastTimeElapsed <= RicochetPvE.Cooldown.RecastTimeElapsed;
    #endregion
}