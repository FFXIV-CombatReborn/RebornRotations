namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.PvE, GameVersion = "7.05")]
[SourceCode(Path = "main/BasicRotations/Melee/SAM_Default.cs")]
[Api(4)]
public sealed class SAM_Default : SamuraiRotation
{
    #region Config Options

    [Range(0, 85, ConfigUnitType.None, 5)]
    [RotationConfig(CombatType.PvE, Name = "Use Kenki above.")]
    public int AddKenki { get; set; } = 50;

    [RotationConfig(CombatType.PvE, Name = "Prevent Higanbana use if theres more than one target")]
    public bool HiganbanaTargets { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Enable TEA Checker.")]
    public bool EnableTEAChecker { get; set; } = false; 
    #endregion

    #region Countdown Logic

    protected override IAction? CountDownAction(float remainTime)
    {
        // pre-pull: can be changed to -9 and -5 instead of 5 and 2, but it's hard to be universal !!! check later !!!
        if (remainTime <= 5 && MeikyoShisuiPvE.CanUse(out var act)) return act;
        if (remainTime <= 2 && TrueNorthPvE.CanUse(out act)) return act;
        return base.CountDownAction(remainTime);
    }

    #endregion

    #region Additional oGCD Logic

    [RotationDesc(ActionID.HissatsuGyotenPvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (HissatsuGyotenPvE.CanUse(out act)) return true;
        return base.MoveForwardAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.FeintPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (FeintPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.ThirdEyePvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (TengentsuPvE.CanUse(out act)) return true;
        if (ThirdEyePvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(nextGCD, out act);
    }

    #endregion

    #region oGCD Logic

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (EnableTEAChecker && Target.Name.ToString() == "Jagd Doll" && Target.GetHealthRatio() < 0.25)
        {
            return false;
        }
        
        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        // IkishotenPvE logic combined with the delayed opener:
        // you should weave the tincture in manually after rsr lands the first gcd (usually Gekko)
        // and that's the only chance for tincture weaving during opener
        if (!CombatElapsedLessGCD(2) && IkishotenPvE.CanUse(out act)) return true;
        if (ShohaPvE.CanUse(out act)) return true;
        // from old version - didn't touch this, didn't test this, never saw Hagakure button pressed personally !!! check later !!!
        if ((HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false) &&
            (HostileTarget?.WillStatusEnd(32, true, StatusID.Higanbana) ?? false) &&
            !(HostileTarget?.WillStatusEnd(28, true, StatusID.Higanbana) ?? false) &&
            SenCount == 1 && IsLastAction(true, YukikazePvE) && !HaveMeikyoShisui)
        {
            if (HagakurePvE.CanUse(out act)) return true;
        }

        if (ZanshinPvE.CanUse(out act)) return true; // need to check rsr code for upgrade and remove aoecheck here !!! check later !!!
        if (HissatsuGurenPvE.CanUse(out act)) return true;
        if (HissatsuSeneiPvE.CanUse(out act)) return true;

        if (HissatsuKyutenPvE.CanUse(out act)) return true;
        if (HissatsuShintenPvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (EnableTEAChecker && Target.Name.ToString() == "Jagd Doll" && Target.GetHealthRatio() < 0.25)
        {
            return false;
        }
        
        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        // from old version - didn't touch this, didn't test this, personally i doubt it's working !!! check later !!!
        if (HasHostilesInRange && IsLastGCD(true, YukikazePvE, MangetsuPvE, OkaPvE) &&
            (!IsTargetBoss || (HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false) && !(HostileTarget?.WillStatusEnd(40, true, StatusID.Higanbana) ?? false) || !HasMoon && !HasFlower || IsTargetBoss && IsTargetDying))
        {
            if (MeikyoShisuiPvE.CanUse(out act, usedUp: true)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    #endregion

    #region GCD Logic

    StatusID[] SamBuffs = [StatusID.Fugetsu, StatusID.Fuka];

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (EnableTEAChecker && Target.Name.ToString() == "Jagd Doll" && Target.GetHealthRatio() < 0.25)
        {
            return false;
        }

        if ((!HiganbanaTargets || (HiganbanaTargets && NumberOfAllHostilesInRange < 2)) && (HostileTarget?.WillStatusEnd(18, true, StatusID.Higanbana) ?? false) && HiganbanaPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;


        if (MidareSetsugekkaPvE.CanUse(out act)) return true;

        if (TenkaGokenPvE.CanUse(out act)) return true;
        if (TendoGokenPvE.CanUse(out act)) return true;
        if (TendoSetsugekkaPvE.CanUse(out act)) return true;
        if (TendoKaeshiGokenPvE.CanUse(out act)) return true;
        if (TendoKaeshiSetsugekkaPvE.CanUse(out act)) return true;
        // use 2nd finisher combo spell first
        if (KaeshiNamikiriPvE.CanUse(out act, usedUp: true)) return true;

        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        // use 2nd finisher combo spell first
        if (KaeshiGokenPvE.CanUse(out act, usedUp: true)) return true;
        if (KaeshiSetsugekkaPvE.CanUse(out act, usedUp: true)) return true;
        if (TendoKaeshiGokenPvE.CanUse(out act, usedUp: true)) return true;
        if (TendoKaeshiSetsugekkaPvE.CanUse(out act, usedUp: true)) return true;

        // burst finisher
        if ((!IsTargetBoss || (HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false)) && HasMoon && HasFlower
            && OgiNamikiriPvE.CanUse(out act)) return true;

        if (TendoSetsugekkaPvE.CanUse(out act)) return true;
        if (MidareSetsugekkaPvE.CanUse(out act)) return true;

        // aoe 12 combo's 2
        if ((!HasMoon || IsMoonTimeLessThanFlower || !OkaPvE.EnoughLevel) && MangetsuPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasGetsu)) return true;
        if ((!HasFlower || !IsMoonTimeLessThanFlower) && OkaPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasKa)) return true;

        if (!HasSetsu && SamBuffs.All(SamBuffs => Player.HasStatus(true, SamBuffs)) && 
            YukikazePvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && HasGetsu && HasKa)) return true;

        // single target 123 combo's 3 or used 3 directly during burst when MeikyoShisui is active, while also trying to start with the one that player is in position for extra DMG
        if (GekkoPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasGetsu) && GekkoPvE.Target.Target != null && CanHitPositional(EnemyPositional.Rear, GekkoPvE.Target.Target)) return true;
        if (KashaPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasKa) && KashaPvE.Target.Target != null && CanHitPositional(EnemyPositional.Flank, KashaPvE.Target.Target)) return true;

        if (GekkoPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasGetsu)) return true;
        if (KashaPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasKa)) return true;

        // single target 123 combo's 2, while also trying to start with the one that player is in position for extra DMG
        if (!HasGetsu && JinpuPvE.CanUse(out act) && JinpuPvE.Target.Target != null && (CanHitPositional(EnemyPositional.Rear, JinpuPvE.Target.Target) || (!HasMoon && HasFlower))) return true;
        if (!HasKa && ShifuPvE.CanUse(out act) && ShifuPvE.Target.Target != null && (CanHitPositional(EnemyPositional.Flank, ShifuPvE.Target.Target) || (!HasFlower && HasMoon))) return true;

        if ((!HasMoon || IsMoonTimeLessThanFlower || !ShifuPvE.EnoughLevel) && JinpuPvE.CanUse(out act)) return true;
        if ((!HasFlower || !IsMoonTimeLessThanFlower) && ShifuPvE.CanUse(out act)) return true;

        // initiate aoe
        if (FukoPvE.CanUse(out act, skipComboCheck: true)) return true; // fuga doesn't becomes fuko automatically
        if (!FukoPvE.EnoughLevel && FugaPvE.CanUse(out act, skipComboCheck: true)) return true;

        // MeikyoShisui buff is not active - not bursting - single target 123 combo's 1
        if (!HaveMeikyoShisui)
        {
            // target in range
            if (HakazePvE.CanUse(out act)) return true;

            // target out of range
            if (EnpiPvE.CanUse(out act)) return true;
        }

        return base.GeneralGCD(out act);
    }

    #endregion
}
