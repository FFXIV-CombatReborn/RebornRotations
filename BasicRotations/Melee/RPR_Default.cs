namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.PvE, GameVersion = "6.58", Description = "Additonal contributions to this rotation thanks to Toshi!")]
[SourceCode(Path = "main/DefaultRotations/Melee/RPR_Default.cs")]
[Api(1)]
public sealed class RPR_Default : ReaperRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "[Beta Option] Pool Shroud for Arcane Circle.")]
    public bool EnshroudPooling { get; set; } = false;
    #endregion

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < HarpePvE.Info.CastTime + CountDownAhead
            && HarpePvE.CanUse(out var act)) return act;

        if (SoulsowPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        bool IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        bool IsTargetDying = HostileTarget?.IsDying() ?? false;
        bool NoEnshroudPooling = !EnshroudPooling && Shroud >= 50;
        bool YesEnshroudPooling = EnshroudPooling && Shroud >= 50 && (!PlentifulHarvestPvE.EnoughLevel || Player.HasStatus(true, StatusID.ArcaneCircle) || ArcaneCirclePvE.Cooldown.WillHaveOneCharge(8) || !Player.HasStatus(true, StatusID.ArcaneCircle) && ArcaneCirclePvE.Cooldown.WillHaveOneCharge(65) && !ArcaneCirclePvE.Cooldown.WillHaveOneCharge(50) || !Player.HasStatus(true, StatusID.ArcaneCircle) && Shroud >= 90);

        if (IsBurst)
        {
            if (UseBurstMedicine(out act))
            {
                if (CombatElapsedLess(10))
                {
                    if (!CombatElapsedLess(5)) return true;
                }
                else
                {
                    if (ArcaneCirclePvE.Cooldown.WillHaveOneCharge(5)) return true;
                }
            }
            if ((HostileTarget?.HasStatus(true, StatusID.DeathsDesign) ?? false)
                && ArcaneCirclePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (IsTargetBoss && IsTargetDying || NoEnshroudPooling || YesEnshroudPooling)
        {
            if (EnshroudPvE.CanUse(out act)) return true;
        }

        if (HasEnshrouded && (Player.HasStatus(true, StatusID.ArcaneCircle) || LemureShroud < 3))
        {
            if (LemuresScythePvE.CanUse(out act, usedUp: true)) return true;
            if (LemuresSlicePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (PlentifulHarvestPvE.EnoughLevel && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && !Player.HasStatus(true, StatusID.BloodsownCircle_2972) || !PlentifulHarvestPvE.EnoughLevel)
        {
            if (GluttonyPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (!Player.HasStatus(true, StatusID.BloodsownCircle_2972) && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && (GluttonyPvE.EnoughLevel && !GluttonyPvE.Cooldown.WillHaveOneChargeGCD(4) || !GluttonyPvE.EnoughLevel || Soul == 100))
        {
            if (GrimSwathePvE.CanUse(out act)) return true;
            if (BloodStalkPvE.CanUse(out act)) return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        if (SoulsowPvE.CanUse(out act)) return true;

        if (WhorlOfDeathPvE.CanUse(out act)) return true;
        if (ShadowOfDeathPvE.CanUse(out act)) return true;

        if (HasEnshrouded)
        {
            if (ShadowOfDeathPvE.CanUse(out act)) return true;

            if (LemureShroud > 1)
            {
                if (PlentifulHarvestPvE.EnoughLevel && ArcaneCirclePvE.Cooldown.WillHaveOneCharge(9) &&
                   (LemureShroud == 4 && (HostileTarget?.WillStatusEnd(30, true, StatusID.DeathsDesign) ?? false) || LemureShroud == 3 && (HostileTarget?.WillStatusEnd(50, true, StatusID.DeathsDesign) ?? false)))
                {
                    if (ShadowOfDeathPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;
                }

                if (Reaping(out act)) return true;
            }
            if (LemureShroud == 1)
            {
                if (CommunioPvE.EnoughLevel)
                {
                    if (!IsMoving && CommunioPvE.CanUse(out act, skipAoeCheck: true))
                    {
                        return true;
                    }
                    else
                    {
                        if (ShadowOfDeathPvE.CanUse(out act, skipAoeCheck: IsMoving)) return true;
                    }
                }
                else
                {
                    if (Reaping(out act)) return true;
                }
            }
        }

        if (HasSoulReaver)
        {
            if (GuillotinePvE.CanUse(out act)) return true;
            if (Player.HasStatus(true, StatusID.EnhancedGibbet))
            {
                if (GibbetPvE.CanUse(out act, skipComboCheck: true)) return true;
            }
            else
            {
                if (GallowsPvE.CanUse(out act, skipComboCheck: true)) return true;
            }
        }

        if (!CombatElapsedLessGCD(2) && PlentifulHarvestPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (SoulScythePvE.CanUse(out act, usedUp: true)) return true;
        if (SoulSlicePvE.CanUse(out act, usedUp: true)) return true;

        if (NightmareScythePvE.CanUse(out act)) return true;
        if (SpinningScythePvE.CanUse(out act)) return true;

        if (InfernalSlicePvE.CanUse(out act)) return true;
        if (WaxingSlicePvE.CanUse(out act)) return true;
        if (SlicePvE.CanUse(out act)) return true;

        if (InCombat && !HasSoulReaver && HarvestMoonPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (HarpePvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region Extra Methods
    private bool Reaping(out IAction? act)
    {
        if (GrimReapingPvE.CanUse(out act)) return true;
        if (Player.HasStatus(true, StatusID.EnhancedCrossReaping) || !Player.HasStatus(true, StatusID.EnhancedVoidReaping))
        {
            if (CrossReapingPvE.CanUse(out act)) return true;
        }
        else
        {
            if (VoidReapingPvE.CanUse(out act)) return true;
        }
        return false;
    }
    #endregion 
}