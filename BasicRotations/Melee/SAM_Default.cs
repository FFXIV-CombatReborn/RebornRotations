namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.PvE, GameVersion = "7.00")]
[SourceCode(Path = "main/DefaultRotations/Melee/SAM_Default.cs")]
[Api(2)]
public sealed class SAM_Default : SamuraiRotation
{
    #region Config Options

    [Range(0, 85, ConfigUnitType.None, 5)]
    [RotationConfig(CombatType.PvE, Name = "Use Kenki above.")]
    public int AddKenki { get; set; } = 50;

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

    #region oGCD Logic

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        // IkishotenPvE logic combined with the delayed opener:
        // you should weave the tincture in manually after rsr lands the first gcd (usually Gekko)
        // and that's the only chance for tincture weaving during opener
        if (Kenki <= 50 && !CombatElapsedLessGCD(2) && IkishotenPvE.CanUse(out act)) return true;

        // from old version - didn't touch this, didn't test this, never saw Hagakure button pressed personally !!! check later !!!
        if ((HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false) &&
            (HostileTarget?.WillStatusEnd(32, true, StatusID.Higanbana) ?? false) &&
            !(HostileTarget?.WillStatusEnd(28, true, StatusID.Higanbana) ?? false) &&
            SenCount == 1 && IsLastAction(true, YukikazePvE) && !HaveMeikyoShisui)
        {
            if (HagakurePvE.CanUse(out act)) return true;
        }

        // moon: fugetsu buff; flower: fuka buff
        // only use senei/guren when the two buffs are up for max dps and correct rotation order
        if (HasMoon && HasFlower)
        {
            if (HissatsuGurenPvE.CanUse(out act, skipAoeCheck: !HissatsuSeneiPvE.EnoughLevel)) return true;
            if (HissatsuSeneiPvE.CanUse(out act)) return true;
        }

        // Shoha is combined from two spells since 7.0, use it in both single target and aoe
        if (ShohaPvE.CanUse(out act, skipAoeCheck: true)) return true; // need to check rsr code for upgrade and remove aoecheck here !!! check later !!!

        // dump shinten
        if (Kenki >= 50 && IkishotenPvE.Cooldown.WillHaveOneCharge(10) || Kenki >= AddKenki || IsTargetBoss && IsTargetDying)
        {
            if (ZanshinPvE.CanUse(out act, skipAoeCheck: true)) return true; // need to check rsr code for upgrade and remove aoecheck here !!! check later !!!
            if (HissatsuKyutenPvE.CanUse(out act)) return true;
            if (HissatsuShintenPvE.CanUse(out act)) return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }
    
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
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

    protected override bool GeneralGCD(out IAction? act)
    {
        // use 2nd finisher combo spell first
        if (KaeshiNamikiriPvE.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;

        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        // use 2nd finisher combo spell first
        if (KaeshiGokenPvE.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;
        if (KaeshiSetsugekkaPvE.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;
        if (TendoKaeshiGokenPvE.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;
        if (TendoKaeshiSetsugekkaPvE.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;

        // burst finisher
        if ((!IsTargetBoss || (HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false)) && HasMoon && HasFlower
            && OgiNamikiriPvE.CanUse(out act, skipAoeCheck: true)) return true;

        // 1/2/3 sen finishers
        if (SenCount == 1 && IsTargetBoss && !IsTargetDying) // 1 sen + has two buffs + not aoe = put dot on boss (not dying)
        {
            if (HasMoon && HasFlower && !FugaPvE.CanUse(out _) && HiganbanaPvE.CanUse(out act)) return true;
        }
        if (SenCount == 2) // 2 sen aoe combo finisher
        {
            if (TendoGokenPvE.CanUse(out act)) return true;
            if (TenkaGokenPvE.CanUse(out act, skipAoeCheck: !MidareSetsugekkaPvE.EnoughLevel)) return true;
        }
        if (SenCount == 3) // 3 sen single target combo finisher
        {
            // need to test for low level content !!! check later !!!
            if (TendoSetsugekkaPvE.CanUse(out act)) return true;
            if (MidareSetsugekkaPvE.CanUse(out act)) return true;
        }

        // aoe 12 combo's 2
        if ((!HasMoon || IsMoonTimeLessThanFlower || !OkaPvE.EnoughLevel) && MangetsuPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasGetsu)) return true;
        if ((!HasFlower || !IsMoonTimeLessThanFlower) && OkaPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasKa)) return true;

        if (!HasSetsu && YukikazePvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && HasGetsu && HasKa)) return true;
        // single target 123 combo's 3 or used 3 directly during burst when MeikyoShisui is active
        if (GekkoPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasGetsu)) return true;
        if (KashaPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasKa)) return true;

        // single target 123 combo's 2
        if ((!HasMoon || IsMoonTimeLessThanFlower || !ShifuPvE.EnoughLevel) && JinpuPvE.CanUse(out act)) return true;
        if ((!HasFlower || !IsMoonTimeLessThanFlower) && ShifuPvE.CanUse(out act)) return true;

        // initiate aoe
        if (FukoPvE.CanUse(out act, skipComboCheck: true)) return true; // fuga doesn't becomes fuko automatically
        if (!FukoPvE.EnoughLevel && FugaPvE.CanUse(out act, skipComboCheck: true)) return true;

        // MeikyoShisui buff is not active - not bursting - single target 123 combo's 1
        if (!HaveMeikyoShisui)
        {
            // target in range
            if (HakazePvE.CanUse(out act)) return true; // hakaze becomes gyofu automatically, maybe it's something in rsr

            // target out of range
            if (EnpiPvE.CanUse(out act)) return true;
        }

        return base.GeneralGCD(out act);
    }

    #endregion

    #region Extra Methods
    private static bool HaveMeikyoShisui => Player.HasStatus(true, StatusID.MeikyoShisui);
 
    #endregion
}