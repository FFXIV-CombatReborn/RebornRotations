namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/BasicRotations/Melee/NIN_Default.cs")]
[Api(4)]

public sealed class NIN_Default : NinjaRotation
{
    #region Config Options
    // Configuration properties for rotation behavior.
    [RotationConfig(CombatType.PvE, Name = "Use Hide")]
    public bool UseHide { get; set; } = true;
    [RotationConfig(CombatType.PvE, Name = "Use Unhide")]
    public bool AutoUnhide { get; set; } = true;

    public bool IsShadowWalking = Player.HasStatus(true, StatusID.ShadowWalker);
    #endregion

    #region CountDown Logic
    // Logic to determine the action to take during the countdown phase before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        var realInHuton = IsLastAction(false, HutonPvE);
        // Clears ninjutsu setup if countdown is more than 6 seconds or if Suiton is the aim but shouldn't be.
        if (remainTime > 6) ClearNinjutsu();

        // Decision-making for ninjutsu actions based on remaining time until combat starts.
        if (DoNinjutsu(out var act))
        {
            if (act == SuitonPvE && remainTime > CountDownAhead) return null;
            return act;
        }

        else if (remainTime < 5)
        {
            SetNinjutsu(SuitonPvE);
        }
        else if (remainTime < 6)
        {
            // If within 10 seconds to start, consider using Hide or setting up Huton.
            if (_ninActionAim == null && TenPvE.Cooldown.IsCoolingDown && HidePvE.CanUse(out act)) return act;

        }
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Ninjutsu Logic
    // Sets the target ninjutsu action to be performed next.
    // If the action is null, or currently set to Rabbit Medium (indicating a failed Ninjutsu attempt), it exits early.
    // If the current action aim is not null and the last action matches certain conditions, it exits early.
    // Finally, updates the current ninjutsu action aim if it's different from the incoming action.
    private void SetNinjutsu(IBaseAction act)
    {

        if (act == null || AdjustId(ActionID.NinjutsuPvE) == ActionID.RabbitMediumPvE) return;

        if (_ninActionAim != null && IsLastAction(false, TenPvE, JinPvE, ChiPvE, FumaShurikenPvE_18873, FumaShurikenPvE_18874, FumaShurikenPvE_18875)) return;

        if (_ninActionAim != act)
        {
            _ninActionAim = act;
        }
    }

    // Clears the ninjutsu action aim, effectively resetting any planned ninjutsu action.
    private void ClearNinjutsu()
    {
        if (_ninActionAim != null)
        {
            _ninActionAim = null;
        }
    }

    // Logic for choosing which ninjutsu action to set up next, based on various game state conditions.
    private bool ChoiceNinjutsu(out IAction? act)
    {
        act = null;

        if (!JinPvE.CanUse(out _) || !ChiPvE.CanUse(out _) || !TenPvE.CanUse(out _)) return false;
        // Ensures that the action ID currently considered for Ninjutsu is actually valid for Ninjutsu execution.
        if (AdjustId(ActionID.NinjutsuPvE) != ActionID.NinjutsuPvE) return false;
        // If more than 4.5 seconds have passed since the last action, it clears any pending Ninjutsu to avoid stale actions.
        if (TimeSinceLastAction.TotalSeconds > 4.5) ClearNinjutsu();

        // Checks for Kassatsu status to prioritize high-impact Ninjutsu due to its buff.
        if (Player.HasStatus(true, StatusID.Kassatsu))
        {
            // Attempts to set high-damage AoE Ninjutsu if available under Kassatsu's effect.
            // These are prioritized due to Kassatsu's enhancement of Ninjutsu abilities.
            if (GokaMekkyakuPvE.CanUse(out _) && ChiPvE.CanUse(out _) && TenPvE.CanUse(out _))
            {
                SetNinjutsu(GokaMekkyakuPvE);
                return false;
            }
            if (HyoshoRanryuPvE.CanUse(out _) && TenPvE.CanUse(out _) && JinPvE.CanUse(out _))
            {
                SetNinjutsu(HyoshoRanryuPvE);
                return false;
            }

            if (HutonPvE.CanUse(out _) && TenPvE.CanUse(out _) && ChiPvE.CanUse(out _) && JinPvE.CanUse(out _))
            {
                SetNinjutsu(HutonPvE);
                return false;
            }

            if (KatonPvE.CanUse(out _) && ChiPvE.CanUse(out _) && TenPvE.CanUse(out _))
            {
                SetNinjutsu(KatonPvE);
                return false;
            }

            if (RaitonPvE.CanUse(out _) && TenPvE.CanUse(out _) && ChiPvE.CanUse(out _))
            {
                SetNinjutsu(RaitonPvE);
                return false;
            }
        }
        else
        {
            // If Suiton is active but no specific Ninjutsu is currently aimed, it clears the Ninjutsu aim.
            // This check is relevant for managing Suiton's effect, particularly for enabling Trick Attack.
            if (Player.HasStatus(true, StatusID.Suiton)
                && _ninActionAim == SuitonPvE && NoNinjutsu)
            {
                ClearNinjutsu();
            }

            // Chooses buffs or AoE actions based on combat conditions and cooldowns.
            // For instance, setting Huton for speed buff or choosing AoE Ninjutsu like Katon or Doton based on enemy positioning.
            // Also considers using Suiton for vulnerability debuff on the enemy if conditions are optimal.

            //Aoe
            if (KatonPvE.CanUse(out _) && ChiPvE.CanUse(out _) && TenPvE.CanUse(out _))
            {
                if (!Player.HasStatus(true, StatusID.Doton) && !IsMoving && !IsLastGCD(false, DotonPvE) && (!TenChiJinPvE.Cooldown.WillHaveOneCharge(6)) || !TenChiJinPvE.Cooldown.IsCoolingDown && TenPvE.CanUse(out _) && ChiPvE.CanUse(out _) && JinPvE.CanUse(out _))
                    SetNinjutsu(DotonPvE);
                else SetNinjutsu(KatonPvE);
                return false;
            }

            //Vulnerable
            if (IsBurst && TrickAttackPvE.Cooldown.WillHaveOneCharge(18) && SuitonPvE.CanUse(out _) && !Player.HasStatus(true, StatusID.Suiton) && TenPvE.CanUse(out _) && ChiPvE.CanUse(out _) && JinPvE.CanUse(out _))
            {
                SetNinjutsu(SuitonPvE);
                return false;
            }

            //Single
            if (TenPvE.CanUse(out _, usedUp: InTrickAttack && !Player.HasStatus(false, StatusID.RaijuReady)))
            {
                if (RaitonPvE.CanUse(out _) && TenPvE.CanUse(out _) && ChiPvE.CanUse(out _))
                {
                    SetNinjutsu(RaitonPvE);
                    return false;
                }

                if (!ChiPvE.EnoughLevel && FumaShurikenPvE.CanUse(out _))
                {
                    SetNinjutsu(FumaShurikenPvE);
                    return false;
                }
            }
        }

        // If the last action performed matches any of a list of specific actions, it clears the Ninjutsu aim.
        // This serves as a reset/cleanup mechanism to ensure the decision logic starts fresh for the next cycle.
        if (IsLastAction(false, DotonPvE, SuitonPvE,
            RabbitMediumPvE, FumaShurikenPvE, KatonPvE, RaitonPvE,
            HyotonPvE, HutonPvE, DotonPvE, SuitonPvE, GokaMekkyakuPvE, HyoshoRanryuPvE))
        {
            ClearNinjutsu();
        }
        return false; // Indicates that no specific Ninjutsu action was chosen in this cycle.
    }
    #endregion

    #region Ninjutsu Execution
    // Attempts to perform a ninjutsu action, based on the current game state and conditions.
    private bool DoNinjutsu(out IAction? act)
    {
        act = null;

        //TenChiJin
        if (Player.HasStatus(true, StatusID.TenChiJin))
        {
            uint tenId = AdjustId(TenPvE.ID);
            uint chiId = AdjustId(ChiPvE.ID);
            uint jinId = AdjustId(JinPvE.ID);

            //First
            if (tenId == FumaShurikenPvE_18873.ID
                && !IsLastAction(false, FumaShurikenPvE_18875, FumaShurikenPvE_18873))
            {
                //AOE
                if (KatonPvE.CanUse(out _))
                {
                    if (FumaShurikenPvE_18875.CanUse(out act)) return true;
                }
                //Single
                if (FumaShurikenPvE_18873.CanUse(out act)) return true;
            }

            //Second
            else if (tenId == KatonPvE_18876.ID && !IsLastAction(false, KatonPvE_18876))
            {
                if (KatonPvE_18876.CanUse(out act, skipAoeCheck: true)) return true;
            }
            //Others
            else if (chiId == RaitonPvE_18877.ID && !IsLastAction(false, RaitonPvE_18877))
            {
                if (RaitonPvE_18877.CanUse(out act, skipAoeCheck: true)) return true;
            }
            else if (chiId == DotonPvE_18880.ID && !IsLastAction(false, DotonPvE_18880) && !Player.HasStatus(true, StatusID.Doton))
            {
                if (DotonPvE_18880.CanUse(out act, skipAoeCheck: true)) return true;
            }
            else if (jinId == SuitonPvE_18881.ID && !IsLastAction(false, SuitonPvE_18881))
            {
                if (SuitonPvE_18881.CanUse(out act, skipAoeCheck: true)) return true;
            }
        }

        //Keep Kassatsu in Burst.
        if (!Player.WillStatusEnd(3, false, StatusID.Kassatsu)
            && Player.HasStatus(false, StatusID.Kassatsu) && !InTrickAttack) return false;
        if (_ninActionAim == null) return false;

        var id = AdjustId(ActionID.NinjutsuPvE);

        //Failed
        if ((uint)id == RabbitMediumPvE.ID)
        {
            ClearNinjutsu();
            act = null;
            return false;
        }
        //First
        else if (id == ActionID.NinjutsuPvE)
        {
            //Can't use.
            if (!Player.HasStatus(true, StatusID.Kassatsu, StatusID.TenChiJin)
                && !TenPvE.CanUse(out _, usedUp: true)
                && !IsLastAction(false, _ninActionAim.Setting.Ninjutsu![0]))
            {
                return false;
            }
            act = _ninActionAim.Setting.Ninjutsu![0];
            return true;
        }
        //Second
        else if ((uint)id == _ninActionAim.ID)
        {
            if (_ninActionAim.CanUse(out act, skipAoeCheck: true)) return true;
            if (_ninActionAim.ID == DotonPvE.ID && !InCombat)
            {
                act = _ninActionAim;
                return true;
            }
        }
        //Third
        else if ((uint)id == FumaShurikenPvE.ID)
        {
            if (_ninActionAim.Setting.Ninjutsu!.Length > 1
                && !IsLastAction(false, _ninActionAim.Setting.Ninjutsu![1]))
            {
                act = _ninActionAim.Setting.Ninjutsu![1];
                return true;
            }
        }
        //Finished
        else if ((uint)id == KatonPvE.ID || (uint)id == RaitonPvE.ID || (uint)id == HyotonPvE.ID)
        {
            if (_ninActionAim.Setting.Ninjutsu!.Length > 2
                && !IsLastAction(false, _ninActionAim.Setting.Ninjutsu![2]))
            {
                act = _ninActionAim.Setting.Ninjutsu![2];
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Move Logic
    // Defines logic for actions to take when moving forward during combat.
    // This attribute associates the method with the Forked Raiju PvE action, 
    // indicating it's a relevant ability when considering movement-based actions.
    [RotationDesc(ActionID.ForkedRaijuPvE)]
    protected override bool MoveForwardGCD(out IAction? act)
    {
        // Checks if Forked Raiju, a movement-friendly ability, can be used. 
        // If so, sets it as the action to perform, returning true to indicate an action has been selected.
        if (ForkedRaijuPvE.CanUse(out act)) return true;

        // If Forked Raiju is not available or not the best option, 
        // falls back to the base class's logic for choosing a move-forward action.
        return base.MoveForwardGCD(out act);
    }
    #endregion

    #region oGCD Logic
    // Determines the emergency abilities to use, overriding the base class implementation.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        // Initializes the action to null, indicating no action has been chosen yet.
        act = null;

        // If Ninjutsu is available or not in combat, defers to the base class's emergency ability logic.
        if (!NoNinjutsu || !InCombat) return base.EmergencyAbility(nextGCD, out act);

        // First priority is given to Kassatsu if it's available, allowing for an immediate powerful Ninjutsu.
        if (KassatsuPvE.CanUse(out act)) return true;

        if (TenriJindoPvE.CanUse(out act)) return true;

        // If in a burst phase and not just starting combat, checks if Mug is available to generate additional Ninki.
        if (IsBurst && !CombatElapsedLess(5) && MugPvE.CanUse(out act)) return true;

        // Prioritizes using Suiton and Trick Attack for maximizing damage, especially outside the initial combat phase.
        if (!CombatElapsedLess(6))
        {
            // Attempts to use Trick Attack if it's available.
            if (KunaisBanePvE.CanUse(out act, skipAoeCheck: true, skipStatusProvideCheck: IsShadowWalking)) return true;
            if (!KunaisBanePvE.EnoughLevel && TrickAttackPvE.CanUse(out act, skipStatusProvideCheck: IsShadowWalking)) return true;

            // If Trick Attack is on cooldown but will not be ready soon, considers using Meisui to recover Ninki.
            if (TrickAttackPvE.Cooldown.IsCoolingDown && !TrickAttackPvE.Cooldown.WillHaveOneCharge(19) && MeisuiPvE.CanUse(out act)) return true;
        }

        // If none of the specific conditions are met, falls back to the base class's emergency ability logic.
        return base.EmergencyAbility(nextGCD, out act);
    }

    // Defines attack abilities to use during combat, overriding the base class implementation.
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        // If Ninjutsu is available or not in combat, it exits early, indicating no attack action to perform.
        if (!NoNinjutsu || !InCombat) return false;

        // If the player is not moving, is within Trick Attack's effective window, and Ten Chi Jin hasn't recently been used,
        // then Ten Chi Jin is set as the next action to perform.
        if (!IsMoving && InTrickAttack && !TenPvE.Cooldown.ElapsedAfter(30) && TenChiJinPvE.CanUse(out act)) return true;

        // If more than 5 seconds have passed in combat, checks if Bunshin is available to use.
        if (!CombatElapsedLess(5) && BunshinPvE.CanUse(out act)) return true;

        // Special handling if within Trick Attack's effective window:
        if (InTrickAttack)
        {
            // If Dream Within A Dream is not yet available, checks if Assassinate can be used.
            if (!DreamWithinADreamPvE.EnoughLevel)
            {
                if (AssassinatePvE.CanUse(out act)) return true;
            }
            else
            {
                // If Dream Within A Dream is available, it's set as the next action.
                if (DreamWithinADreamPvE.CanUse(out act)) return true;
            }
        }

        // Checks for the use of Hellfrog Medium or Bhavacakra under certain conditions:
        // - Not in the Mug's effective window or within Trick Attack's window
        // - Certain cooldown conditions are met, or specific statuses are active.
        if ((!InMug || InTrickAttack)
            && (!BunshinPvE.Cooldown.WillHaveOneCharge(10) || Player.HasStatus(false, StatusID.PhantomKamaitachiReady) || MugPvE.Cooldown.WillHaveOneCharge(2)))
        {
            if (HellfrogMediumPvE.CanUse(out act)) return true;
            if (BhavacakraPvE.CanUse(out act)) return true;
            if (TenriJindoPvE.CanUse(out act)) return true;
        }
        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(nextGCD, out act)) return true;
        // If none of the conditions are met, it falls back to the base class's implementation for attack ability.
        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    // Main method for determining the general action to take during the combat's global cooldown phase.
    protected override bool GeneralGCD(out IAction? act)
    {
        var hasRaijuReady = Player.HasStatus(true, StatusID.RaijuReady);

        if ((InTrickAttack || InMug) && NoNinjutsu && !hasRaijuReady
            && !Player.HasStatus(true, StatusID.TenChiJin)
            && PhantomKamaitachiPvE.CanUse(out act)) return true;

        if (ChoiceNinjutsu(out act)) return true;
        if ((!InCombat || !CombatElapsedLess(7)) && DoNinjutsu(out act)) return true;

        //No Ninjutsu
        if (NoNinjutsu)
        {
            if (!CombatElapsedLess(10) && FleetingRaijuPvE.CanUse(out act)) return true;
            if (hasRaijuReady) return false;
        }

        //AOE
        if (HakkeMujinsatsuPvE.CanUse(out act)) return true;
        if (DeathBlossomPvE.CanUse(out act)) return true;

        //Single
        if (!InTrickAttack && Kazematoi < 4 && ArmorCrushPvE.CanUse(out act)) return true;
        if (AeolianEdgePvE.CanUse(out act)) return true;
        if (GustSlashPvE.CanUse(out act)) return true;
        if (SpinningEdgePvE.CanUse(out act)) return true;

        //Range
        if (!Player.HasStatus(true, StatusID.Mudra))
        {
            if (ThrowingDaggerPvE.CanUse(out act)) return true;
        }

        if (AutoUnhide)
        {
            StatusHelper.StatusOff(StatusID.Hidden);
        }
        if (!InCombat && _ninActionAim == null && UseHide
            && TenPvE.Cooldown.IsCoolingDown && HidePvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    // Holds the next ninjutsu action to perform.
    private IBaseAction? _ninActionAim = null;
    #endregion
}
