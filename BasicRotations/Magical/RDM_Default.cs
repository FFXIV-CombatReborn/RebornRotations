namespace DefaultRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/DefaultRotations/Magical/RDM_Default.cs")]
[Api(3)]
public sealed class RDM_Default : RedMageRotation
{
    #region Config Options
    private static BaseAction VerthunderStartUp { get; } = new BaseAction(ActionID.VerthunderPvE, false);

    [RotationConfig(CombatType.PvE, Name = "Use Vercure for Dualcast when out of combat.")]
    public bool UseVercure { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Cast Reprise when moving with no instacast.")]
    public bool RangedSwordplay { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "DO NOT CAST EMBOLDEN/MANAFICATION OUTSIDE OF MELEE RANGE, I'M SERIOUS YOU HAVE TO MOVE UP FOR IT TO WORK IF THIS IS ON.")]
    public bool AnyonesMeleeRule { get; set; } = false;

    //Fine, ill do it myself
    [RotationConfig(CombatType.PvE, Name = "Cast manafication outside of embolden window (use at own risk).")]
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
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)

    //When we removed emergencyGCD vercure/verraise start overwriting all logic below. Need to do something about it.

    //No bugs in this section (mostly™). Extra Methods is fucked up tho, need to good look of experienced rotation dev.

    {
        bool AnyoneInRange = AllHostileTargets.Any(hostile => hostile.DistanceToPlayer() <= 4);

        act = null;

        if (CombatElapsedLess(4)) return false;

        //COMMENT FOR MYSELF FROM FUTURE - WHY THE HELL EMBOLDEN DONT WORK WITHOUT skipAoeCheck:true???
        if (!AnyonesMeleeRule)
        {
            if (IsBurst && HasHostilesInRange && EmboldenPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }
        else
        {
            if (IsBurst && AnyoneInRange && EmboldenPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        //If manafication usage OUTSIDE of embolden enabled.
        if (AnyoneManafication)
        {
            if (AnyoneInRange && ManaficationPvE.CanUse(out act)) return true;
        }

        //Use Manafication after embolden.  
        if (!AnyoneManafication && (Player.HasStatus(true, StatusID.Embolden) || IsLastAbility(ActionID.EmboldenPvE)) &&
                 ManaficationPvE.CanUse(out act)) return true;

        //Swiftcast/Acceleration usage OLD VERSION
        // if (ManaStacks == 0 && (BlackMana < 50 || WhiteMana < 50)
        //     && (CombatElapsedLess(4) || !ManaficationPvE.EnoughLevel || !ManaficationPvE.Cooldown.WillHaveOneChargeGCD(0, 1)))
        // {
        //     if (InCombat && !Player.HasStatus(true, StatusID.VerfireReady, StatusID.VerstoneReady))
        //     {
        //         if (SwiftcastPvE.CanUse(out act)) return true;
        //         if (AccelerationPvE.CanUse(out act, usedUp: true)) return true;
        //     }
        // }

        //Melee combo interrupt protection (i hate this too)
        bool checkmelee = IsLastGCD(new[]
        {
        ActionID.ResolutionPvE,
        ActionID.ScorchPvE,
        ActionID.VerflarePvE,
        ActionID.VerholyPvE,
        ActionID.RedoublementPvE,
        ActionID.EnchantedRedoublementPvE,
        ActionID.ZwerchhauPvE,
        ActionID.EnchantedZwerchhauPvE,
        ActionID.RipostePvE,
        ActionID.EnchantedRipostePvE,
        ActionID.EnchantedMoulinetTroisPvE,
        ActionID.EnchantedMoulinetDeuxPvE,
        ActionID.EnchantedMoulinetPvE,
        ActionID.MoulinetPvE
        //I dont know at this point if nextGCD.IsTheSameTo even working, but stil gonna left it in here.
    }) && !nextGCD.IsTheSameTo(new[]
        {
        ActionID.RipostePvE,
        ActionID.EnchantedRipostePvE,
        ActionID.MoulinetPvE,
        ActionID.EnchantedMoulinetPvE
    });

        //i really hate this.
        bool ambatumelee = Player.HasStatus(true, StatusID.Manafication, StatusID.MagickedSwordplay);

        //Acceleration usage on rotation with saving 1 charge for movement
        if (GrandImpactPvE.EnoughLevel && !checkmelee && !ambatumelee && //Check for enough level to use Grand Impact, or its pointless.
            !Player.HasStatus(true, StatusID.Manafication, StatusID.MagickedSwordplay) &&
            !Player.HasStatus(true, StatusID.Dualcast) && AccelerationPvE.CanUse(out act)) return true;

        //Acceleration/Swiftcast usage on move
        if (IsMoving && !Player.HasStatus(true, StatusID.Dualcast) && !checkmelee && !ambatumelee &&
            //Checks for not override previous acceleration and lose grand impact
            !Player.HasStatus(true, StatusID.Acceleration) &&
            !Player.HasStatus(true, StatusID.GrandImpactReady) && HasHostilesInRange &&
            //Use acceleration. If acceleration not available, use switfcast instead 
            (AccelerationPvE.CanUse(out act, usedUp: IsMoving) || (!AccelerationPvE.CanUse(out _) && SwiftcastPvE.CanUse(out act))))
        {
            return true;
        }


        //Reprise logic
        if (IsMoving && RangedSwordplay && !checkmelee && !ambatumelee &&
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
             EnchantedReprisePvE.CanUse(out act))) return true;

        if (IsBurst && UseBurstMedicine(out act)) return true;

        //Attack abilities.
        if (PrefulgencePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ViceOfThornsPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ContreSixtePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (FlechePvE.CanUse(out act)) return true;
        if (EngagementPvE.CanUse(out act, usedUp: true)) return true;
        if (CorpsacorpsPvE.CanUse(out act) && !IsMoving) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;

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
        if (IsLastGCD(false, EnchantedMoulinetDeuxPvE) && EnchantedMoulinetTroisPvE.CanUse(out act)) return true;
        if (IsLastGCD(false, EnchantedMoulinetPvE) && EnchantedMoulinetDeuxPvE.CanUse(out act)) return true;
        if (EnchantedRedoublementPvE.CanUse(out act)) return true;
        if (EnchantedZwerchhauPvE.CanUse(out act)) return true;


        //Check if you can start melee combo
        if (CanStartMeleeCombo)
        {
            if (EnchantedMoulinetPvE.CanUse(out act))
            {
                if (BlackMana >= 50 && WhiteMana >= 50 || Player.HasStatus(true, StatusID.MagickedSwordplay)) return true;
            }
            else
            {
                if ((BlackMana >= 50 && WhiteMana >= 50 || Player.HasStatus(true, StatusID.MagickedSwordplay)) &&
                    EnchantedRipostePvE.CanUse(out act)) return true;
            }
        }
        //Grand impact usage if not interrupting melee combo
        if (GrandImpactPvE.CanUse(out act, skipStatusProvideCheck: Player.HasStatus(true, StatusID.GrandImpactReady), skipCastingCheck: true, skipAoeCheck: true)) return true;

        if (ManaStacks == 3) return false;

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


    //why is this not working if called. Its always return false.
    // private bool _didWeJustCombo = IsLastGCD([
    //     ActionID.ScorchPvE, ActionID.VerflarePvE, ActionID.VerholyPvE, ActionID.EnchantedZwerchhauPvE,
    //     ActionID.EnchantedRedoublementPvP, ActionID.EnchantedRipostePvE, ActionID.EnchantedMoulinetPvE, ActionID.EnchantedMoulinetDeuxPvE, ActionID.EnchantedMoulinetTroisPvE
    // ]);

    private bool CanStartMeleeCombo
    {
        get
        {
            if (Player.HasStatus(true, StatusID.Dualcast)) return false;

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
