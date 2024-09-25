namespace DefaultRotations.Ranged;

[Rotation("zMCH Beta", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/DefaultRotations/Ranged/zMCH_Beta.cs")]
[Api(4)]
public sealed class zMCH_Beta : MachinistRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use new beta Queen logic (attempt to align with burst and otherwise avoid overcap). \nUnchecking this will use the 'experimental' hardcoded timings from the default rotation.")]
    private bool NewQueenLogic { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Prioritize Barrel Stabilizer use")]
    private bool BSPrio { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Delay Drill for combo GCD if have one charge and about to break combo")]
    private bool HoldDrillForCombo { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Delay Hypercharge for combo GCD if about to break combo")]
    private bool HoldHCForCombo { get; set; } = true;
    #endregion

    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        // ReassemblePvE's duration is 5s, need to fire the first GCD before it ends
        if (remainTime < 5 && ReassemblePvE.CanUse(out var act)) return act;
        // tincture needs to be used on -2s exactly
        if (remainTime <= 2 && UseBurstMedicine(out act)) return act;
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    // Determines emergency actions to take based on the next planned GCD action.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
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

    // Logic for using attack abilities outside of GCD, focusing on burst windows and cooldown management.
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        // Keeps Ricochet and Gauss cannon Even
        bool isRicochetMore = RicochetPvE.EnoughLevel && GaussRoundPvE.Cooldown.RecastTimeElapsed <= RicochetPvE.Cooldown.RecastTimeElapsed;

        // Start Ricochet/Gauss cooldowns rolling
        if (!RicochetPvE.Cooldown.IsCoolingDown && RicochetPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (!GaussRoundPvE.Cooldown.IsCoolingDown && GaussRoundPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // Check for not burning Hypercharge below level 52 on AOE
        bool LowLevelHyperCheck = !AutoCrossbowPvE.EnoughLevel && SpreadShotPvE.CanUse(out _);

        // If Wildfire is active, use Hypercharge.....Period
        if (Player.HasStatus(true, StatusID.Wildfire_1946) && HyperchargePvE.CanUse(out act)) return true;

        // Rook Autoturret/Queen Logic
        if (CanUseQueenMeow(out act, nextGCD)) return true;

        if (IsBurst && BSPrio && BarrelStabilizerPvE.CanUse(out act)) return true;

        // Burst
        if (IsBurst)
        {
            if ((IsLastAbility(false, HyperchargePvE) || Heat >= 50 || Player.HasStatus(true, StatusID.Hypercharged)) && ToolChargeSoon(out _) && !LowLevelHyperCheck && WildfirePvE.CanUse(out act)) return true;
        }
        // Use Hypercharge if wildfire will not be up in 30 seconds or if you hit 100 heat
        if (!LowLevelHyperCheck && !Player.HasStatus(true, StatusID.Reassembled) && (!WildfirePvE.Cooldown.WillHaveOneCharge(30) || (Heat == 100)))
        {
            if (!HoldHCForCombo || !(LiveComboTime <= 8f && LiveComboTime > 0f) && ToolChargeSoon(out act)) return true;
        }

        // Use Ricochet and Gauss if have pooled charges or is burst window
        if (isRicochetMore && RicochetPvE.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;
        if (GaussRoundPvE.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

        if (IsBurst && BarrelStabilizerPvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
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

            // for opener: only use the first charge of Drill after AirAnchor when there are two
            if (EnhancedMultiweaponTrait.EnoughLevel && DrillPvE.CanUse(out act, usedUp: false)) return true;
            if (!EnhancedMultiweaponTrait.EnoughLevel && DrillPvE.CanUse(out act, usedUp: true)) return true;

            if (!AirAnchorPvE.EnoughLevel && HotShotPvE.CanUse(out act)) return true;
        }

        // ChainSaw is always used after Drill
        if (ChainSawPvE.CanUse(out act, skipAoeCheck: true)) return true;
        // use combo finisher asap
        if (ExcavatorPvE.CanUse(out act, skipAoeCheck: true)) return true;
        // use FMF after ChainSaw combo in 'alternative opener'
        if (FullMetalFieldPvE.CanUse(out act)) return true;

        // dont use the second charge of Drill if it's in opener, also save Drill for burst  --- need to combine this with the logic above!!!
        if (EnhancedMultiweaponTrait.EnoughLevel 
            && !CombatElapsedLessGCD(6) 
            && !ChainSawPvE.Cooldown.WillHaveOneCharge(6) 
            && (!HoldDrillForCombo || !(LiveComboTime <= 5) || (!CleanShotPvE.CanUse(out _) && !SlugShotPvE.CanUse(out _)))
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
        float REST_TIME = 8f;
        if
                     //Cannot AOE
                     (!SpreadShotPvE.CanUse(out _)
                     &&
                     // AirAnchor Enough Level % AirAnchor 
                     ((AirAnchorPvE.EnoughLevel && AirAnchorPvE.Cooldown.WillHaveOneCharge(REST_TIME))
                     ||
                     // HotShot Charge Detection
                     (!AirAnchorPvE.EnoughLevel && HotShotPvE.EnoughLevel && HotShotPvE.Cooldown.WillHaveOneCharge(REST_TIME))
                     ||
                     // Drill Charge Detection
                     (DrillPvE.EnoughLevel && !DrillPvE.Cooldown.WillHaveXCharges(DrillPvE.Cooldown.MaxCharges, REST_TIME))
                     ||
                     // Chainsaw Charge Detection
                     (ChainSawPvE.EnoughLevel && ChainSawPvE.Cooldown.WillHaveOneCharge(REST_TIME))))
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
        // Define conditions under which the Rook Autoturret/Queen can be used.
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

        if (
             (NewQueenLogic && 
                (!CombatElapsedLess(610f)
                || WildfirePvE.Cooldown.WillHaveOneChargeGCD(4) 
                || !WildfirePvE.Cooldown.ElapsedAfter(10)
                || (nextGCD.IsTheSameTo(true, CleanShotPvE) && Battery == 100) 
                || (nextGCD.IsTheSameTo(true, HotShotPvE, AirAnchorPvE, ChainSawPvE, ExcavatorPvE) && (Battery == 90 || Battery == 100))) 
            || !NewQueenLogic && (QueenOne || QueenTwo || QueenThree || QueenFour || QueenFive || QueenSix || QueenSeven || QueenEight || QueenNine || QueenTen || QueenEleven || QueenTwelve || QueenThirteen || QueenFourteen || QueenFifteen)))
        {
            if (RookAutoturretPvE.CanUse(out act)) return true;
        }
        act = null;
        return false;
    }
    #endregion
}