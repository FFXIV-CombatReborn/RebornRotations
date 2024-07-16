using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace DefaultRotations.Ranged;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00", Description = "")]
[SourceCode(Path = "main/DefaultRotations/Ranged/MCH_Default.cs")]
[Api(2)]
public sealed class MCH_Default : MachinistRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "(Warning: Queen logic is new and untested, uncheck to test new logic) Skip Queen Logic and uses Rook Autoturret/Automaton Queen immediately whenever you get 50 battery")]
    private bool SkipQueenLogic { get; set; } = true;
    #endregion

    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 5)
        {
            if (ReassemblePvE.CanUse(out var act)) return act;
        }
        if (remainTime < 2)
        {
            if (UseBurstMedicine(out var act)) return act;
        }
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
            (nextGCD.IsTheSameTo(true, [ChainSawPvE, ExcavatorPvE, AirAnchorPvE]) || 
             (!ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, DrillPvE)) ||
             (!DrillPvE.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShotPvE)) || 
             (!CleanShotPvE.EnoughLevel && nextGCD.IsTheSameTo(true, HotShotPvE)));

        // Keeps Ricochet and Gauss cannon Even
        bool isRicochetMore = RicochetPvE.EnoughLevel && GaussRoundPvE.Cooldown.CurrentCharges <= RicochetPvE.Cooldown.CurrentCharges;
        bool isGaussMore = !RicochetPvE.EnoughLevel || GaussRoundPvE.Cooldown.CurrentCharges > RicochetPvE.Cooldown.CurrentCharges;

        // Attempt to use Reassemble if it's ready
        if (isReassembleUsable)
        {
            if (ReassemblePvE.CanUse(out act, skipComboCheck: true, usedUp: true)) return true;
        }

        // Use Ricochet
        if (isRicochetMore && ((!IsLastAction(true, GaussRoundPvE, RicochetPvE) && IsLastGCD(true, HeatBlastPvE, AutoCrossbowPvE)) || !IsLastGCD(true, HeatBlastPvE, AutoCrossbowPvE)))
        {
            if (RicochetPvE.CanUse(out act, skipAoeCheck: true, usedUp: true))
                return true;
        }

        // Use Gauss
        if (isGaussMore && ((!IsLastAction(true, GaussRoundPvE, RicochetPvE) && IsLastGCD(true, HeatBlastPvE, AutoCrossbowPvE)) || !IsLastGCD(true, HeatBlastPvE, AutoCrossbowPvE)))
        {
            if (GaussRoundPvE.CanUse(out act, usedUp: true))
                return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    // Logic for using attack abilities outside of GCD, focusing on burst windows and cooldown management.
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        // Check for not burning Hypercharge below level 52 on AOE
        bool LowLevelHyperCheck = !AutoCrossbowPvE.EnoughLevel && SpreadShotPvE.CanUse(out _);

        // If Wildfire is active, use Hypercharge.....Period
        if (Player.HasStatus(true, StatusID.Wildfire_1946))
        {
            return HyperchargePvE.CanUse(out act);
        }
        // Burst
        if (IsBurst)
        {
            if (UseBurstMedicine(out act)) return true;

            {
                if ((IsLastAbility(false, HyperchargePvE) || Heat >= 50 || Player.HasStatus(true, StatusID.Hypercharged)) && !CombatElapsedLessGCD(5) &&
                    (CombatElapsedLess(20) || ToolChargeSoon(out _)) && !LowLevelHyperCheck && WildfirePvE.CanUse(out act)) return true;
            }
        }
        // Use Hypercharge if at least 12 seconds of combat and (if wildfire will not be up in 30 seconds or if you hit 100 heat)
        if (!LowLevelHyperCheck && !CombatElapsedLess(12) && !Player.HasStatus(true, StatusID.Reassembled) && (!WildfirePvE.Cooldown.WillHaveOneCharge(30) || (Heat == 100)))
        {
            if (ToolChargeSoon(out act)) return true;
        }
        // Rook Autoturret/Queen Logic
        if (CanUseQueenMeow(out act)) return true;
        if (nextGCD.IsTheSameTo(true, CleanShotPvE, AirAnchorPvE, ChainSawPvE, ExcavatorPvE) && Battery == 100)
        {
            if (RookAutoturretPvE.CanUse(out act)) return true;
        }
        
        if (BarrelStabilizerPvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    // Defines the general logic for determining which global cooldown (GCD) action to take.
    protected override bool GeneralGCD(out IAction? act)
    {
        // Checks and executes AutoCrossbow or HeatBlast if conditions are met (overheated state).
        if (AutoCrossbowPvE.CanUse(out act)) return true;
        if (HeatBlastPvE.CanUse(out act)) return true;

        // Executes Bioblaster, and then checks for AirAnchor or HotShot, and Drill based on availability and conditions.
        if (BioblasterPvE.CanUse(out act)) return true;
        // Check if SpreadShot cannot be used
        if (!SpreadShotPvE.CanUse(out _))
        {
            // Check if AirAnchor can be used
            if (AirAnchorPvE.CanUse(out act)) return true;

            // If not at the required level for AirAnchor and HotShot can be used
            if (!AirAnchorPvE.EnoughLevel && HotShotPvE.CanUse(out act)) return true;

            // Check if Drill can be used
            if (DrillPvE.CanUse(out act, usedUp: false)) return true;
        }

        // Special condition for using ChainSaw outside of AoE checks if no action is chosen within 4 GCDs.
        if (!CombatElapsedLessGCD(1) && ChainSawPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ExcavatorPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (!ChainSawPvE.Cooldown.WillHaveOneCharge(6f) && !CombatElapsedLessGCD(6))
        {
            if (DrillPvE.CanUse(out act, usedUp: true)) return true;
        }

        // AoE actions: ChainSaw and SpreadShot based on their usability.
        if (SpreadShotPvE.CanUse(out _))
        {
            if (ChainSawPvE.CanUse(out act)) return true;
            if (ExcavatorPvE.CanUse(out act)) return true;
        }
        if (FullMetalFieldPvE.CanUse(out act)) return true;
        if (SpreadShotPvE.CanUse(out act)) return true;
        // Single target actions: CleanShot, SlugShot, and SplitShot based on their usability.
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
        float REST_TIME = 6f;
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
                     (DrillPvE.EnoughLevel && DrillPvE.Cooldown.WillHaveOneCharge(REST_TIME))
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

    private bool CanUseQueenMeow(out IAction? act)
    {
    // Define conditions under which the Rook Autoturret/Queen can be used.
    bool NoQueenLogic = SkipQueenLogic;
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
    
    if (NoQueenLogic||QueenOne||QueenTwo||QueenThree||QueenFour||QueenFive||QueenSix||QueenSeven||QueenEight||QueenNine||QueenTen||QueenEleven||QueenTwelve||QueenThirteen||QueenFourteen||QueenFifteen)
    {
        if (RookAutoturretPvE.CanUse(out act)) return true;
    }
    act = null;
    return false;
    }
    #endregion
}