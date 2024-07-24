namespace DefaultRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00")]
[SourceCode(Path = "main/DefaultRotations/Magical/RDM_Default.cs")]
[Api(2)]
public sealed class RDM_Default : RedMageRotation
{
    #region Config Options
    private static BaseAction VerthunderStartUp { get; } = new BaseAction(ActionID.VerthunderPvE, false);

    [RotationConfig(CombatType.PvE, Name = "Use Vercure for Dualcast when out of combat.")]
    public bool UseVercure { get; set; }
    
    [RotationConfig(CombatType.PvE, Name = "Cast Reprise when moving with no instacast.")]
    public bool RangedSwordplay { get; set; } = false;
    
    [RotationConfig(CombatType.PvE, Name = "DO NOT CAST EMBOLDEN/MANAFICATION OUTSIDE OF MELEE RANGE, I'M SERIOUS YOU HAVE TO MOVE UP FOR IT TO WORK IF THIS IS ON.")]
    public bool AnyonesMeleeRule { get; set; } = false;

    //Fine, ill do it myself
    [RotationConfig(CombatType.PvE, Name = "Cast manafication outside of embolden window.")]
    public bool AnyoneManafication { get; set; } = false;
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < VerthunderStartUp.Info.CastTime + CountDownAhead
            && VerthunderStartUp.CanUse(out var act)) return act;

        //Remove Swift
        StatusHelper.StatusOff(StatusID.Dualcast);
        StatusHelper.StatusOff(StatusID.Acceleration);
        StatusHelper.StatusOff(StatusID.Swiftcast);

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        bool AnyoneInRange = AllHostileTargets.Any(hostile => hostile.DistanceToPlayer() <= 4);
        
        act = null;
        if (CombatElapsedLess(4)) return false;
        if (!AnyonesMeleeRule)
        {
            if (IsBurst && HasHostilesInRange && EmboldenPvE.CanUse(out act, skipAoeCheck: true)) return true;

        }
        
        if (IsBurst && AnyoneInRange && EmboldenPvE.CanUse(out act, skipAoeCheck: true)) return true;

        //If manafication usage OUTSIDE of embolden enabled.
        if (AnyoneManafication)
        {
            if (AnyoneInRange && ManaficationPvE.CanUse(out act)) return true;     
        }
        
        //Use Manafication after embolden.  
        if (!AnyoneManafication && (Player.HasStatus(true, StatusID.Embolden, StatusID.Embolden_1297, StatusID.Embolden_2282) || IsLastAbility(ActionID.EmboldenPvE))
            && ManaficationPvE.CanUse(out act)) return true;
        

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        //Swiftcast/Acceleration usage OLD VERSION
       /* if (ManaStacks == 0 && (BlackMana < 50 || WhiteMana < 50)
            && (CombatElapsedLess(4) || !ManaficationPvE.EnoughLevel || !ManaficationPvE.Cooldown.WillHaveOneChargeGCD(0, 1)))
        {
            if (InCombat && !Player.HasStatus(true, StatusID.VerfireReady, StatusID.VerstoneReady))
            {
                if (SwiftcastPvE.CanUse(out act)) return true;
                if (AccelerationPvE.CanUse(out act, usedUp: true)) return true;
            }
        }*/

        if (IsBurst && UseBurstMedicine(out act)) return true;

        //Attack abilities.
        if (ViceOfThornsPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (PrefulgencePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ContreSixtePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (FlechePvE.CanUse(out act)) return true;

        if (EngagementPvE.CanUse(out act, usedUp: true)) return true;
        if (CorpsacorpsPvE.CanUse(out act) && !IsMoving) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    protected override bool EmergencyGCD(out IAction? act)
    {

        if (ManaStacks == 3)
        {
            if (BlackMana > WhiteMana)
            {
                if (VerholyPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }

            if (VerflarePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        // Hardcode Resolution & Scorch to avoid double melee without finishers
        if (IsLastGCD(ActionID.ScorchPvE))
        {
            if (ResolutionPvE.CanUse(out act, skipStatusProvideCheck: true, skipAoeCheck: true)) return true;
        }
        
        if (IsLastGCD(ActionID.VerholyPvE, ActionID.VerflarePvE))
        {
            if (ScorchPvE.CanUse(out act, skipStatusProvideCheck: true, skipAoeCheck: true)) return true;
        }

        //Melee AOE combo
        if (IsLastGCD(true, EnchantedMoulinetPvE) && EnchantedMoulinetDeuxPvE.CanUse(out act)) return true;
        if (IsLastGCD(true, EnchantedMoulinetDeuxPvE) && EnchantedMoulinetTroisPvE.CanUse(out act)) return true;
        if (EnchantedZwerchhauPvE.CanUse(out act)) return true;
        if (EnchantedRedoublementPvE.CanUse(out act)) return true;

        if (!CanStartMeleeCombo) return false;

        //Check if can start melee combo
        if (EnchantedMoulinetPvE.CanUse(out act))
        {
            if (BlackMana >= 50 && WhiteMana >= 50 || Player.HasStatus(true, StatusID.MagickedSwordplay)) return true;
        }
        else
        {
            if ((BlackMana >= 50 && WhiteMana >= 50 || Player.HasStatus(true, StatusID.MagickedSwordplay)) && EnchantedRipostePvE.CanUse(out act)) return true;
        }
        
        return base.EmergencyGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;

        bool didWeJustCombo = IsLastGCD([
            ActionID.ScorchPvE, ActionID.VerflarePvE, ActionID.VerholyPvE, ActionID.EnchantedZwerchhauPvE,
            ActionID.EnchantedRedoublementPvP, ActionID.EnchantedRipostePvE, ActionID.EnchantedMoulinetPvE, ActionID.EnchantedMoulinetDeuxPvE, ActionID.EnchantedMoulinetTroisPvE
        ]);
        //Grand impact usage if not interrupting melee combo
        if (!didWeJustCombo && GrandImpactPvE.CanUse(out act, skipStatusProvideCheck: Player.HasStatus(true, StatusID.GrandImpactReady), skipCastingCheck:true, skipAoeCheck: true)) return true;

        //Acceleration/Swiftcast usage on move, old method on line 61.
        if (IsMoving && !Player.HasStatus(true, StatusID.Dualcast) && HasHostilesInRange &&
            //Checks for not override previous acceleration and lose grand impact
            !Player.HasStatus(true, StatusID.Acceleration) &&
            !Player.HasStatus(true, StatusID.GrandImpactReady) &&
            //Check for melee combo 
            !didWeJustCombo &&
            //Use acceleration. If acceleration not awaliable, use switftast instead 
            (AccelerationPvE.CanUse(out act, usedUp: true) || 
                (!AccelerationPvE.CanUse(out _) && SwiftcastPvE.CanUse(out act)))) 
        {
            return true;
        }

         //Reprise logic
        if (IsMoving && RangedSwordplay && !didWeJustCombo &&
            //Check to not use Reprise when player can do melee combo, to not break it
            (ManaStacks == 0 && (BlackMana < 50 || WhiteMana < 50) &&
            //Check if dualcast active
            !Player.HasStatus(true, StatusID.Dualcast) &&
            //Bunch of checks if anything else can be used instead of Reprise
            !AccelerationPvE.CanUse(out _) &&
            !Player.HasStatus(true, StatusID.Acceleration) &&
            !SwiftcastPvE.CanUse(out _) &&
            !Player.HasStatus(true, StatusID.Swiftcast) &&
            !GrandImpactPvE.CanUse(out _) &&
            !Player.HasStatus(true, StatusID.GrandImpactReady) &&
            //If nothing else to use and player moving - fire reprise.
        (EnchantedReprisePvE.CanUse(out act) || EnchantedReprisePvE.CanUse(out act)))) return true;

        if (ManaStacks == 3) return false;
        
        if (GrandImpactPvE.CanUse(out act)) return true;
        
        if (!VerthunderIiPvE.CanUse(out _))
        {
            if (VerfirePvE.CanUse(out act)) return true;
            if (VerstonePvE.CanUse(out act)) return true;
        }

        if (ScatterPvE.CanUse(out act)) return true;
        
        if (WhiteMana < BlackMana)
        {
            if (VeraeroIiPvE.CanUse(out act) && BlackMana - WhiteMana != 5) return true;
            if (VeraeroPvE.CanUse(out act) && BlackMana - WhiteMana != 6) return true;
        }
        if (VerthunderIiPvE.CanUse(out act)) return true;
        if (VerthunderPvE.CanUse(out act)) return true;

        if (JoltPvE.CanUse(out act)) return true;

        if (UseVercure && NotInCombatDelay && VercurePvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    private bool CanStartMeleeCombo
    {
        get
        {
            if (Player.HasStatus(true, StatusID.Manafication, StatusID.Embolden, StatusID.MagickedSwordplay) ||
                             BlackMana >= 50 || WhiteMana >= 50) return true;

            if (BlackMana == WhiteMana) return false;

            else if (WhiteMana < BlackMana)
            {
                if (Player.HasStatus(true, StatusID.VerstoneReady)) return false;
            }
            else
            {
                if (Player.HasStatus(true, StatusID.VerfireReady)) return false;
            }

            if (Player.HasStatus(true, VercurePvE.Setting.StatusProvide ?? [])) return false;

            //Waiting for embolden.
            if (EmboldenPvE.EnoughLevel && EmboldenPvE.Cooldown.WillHaveOneChargeGCD(5)) return false;

            return true;
        }
    }
    #endregion
}
