namespace DefaultRotations.Melee;

[Rotation("Default PVP", CombatType.PvP, GameVersion = "7.05", Description = "Beta Rotation")]
[SourceCode(Path = "main/BasicRotations/PVPRotations/Tank/VPR_Default.PvP.cs")]
[Api(4)]
public sealed class VPR_DefaultPvP : ViperRotation
{
    private const double HealthThreshold = 0.7;

    [RotationConfig(CombatType.PvP, Name = "Sprint")]
    public bool UseSprintPvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Recuperate")]
    public bool UseRecuperatePvP { get; set; } = false;

    [Range(1, 100, ConfigUnitType.Percent, 1)]
    [RotationConfig(CombatType.PvP, Name = "RecuperateHP%%?")]
    public int RCValue { get; set; } = 75;

    [RotationConfig(CombatType.PvP, Name = "Use Purify")]
    public bool UsePurifyPvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Stun")]
    public bool Use1343PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on DeepFreeze")]
    public bool Use3219PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on HalfAsleep")]
    public bool Use3022PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Sleep")]
    public bool Use1348PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Bind")]
    public bool Use1345PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Heavy")]
    public bool Use1344PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Silence")]
    public bool Use1347PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Stop attacking while in Guard.")]
    public bool GuardCancel { get; set; } = false;

    private bool TryPurify(out IAction? action)
    {
        action = null;
        if (!UsePurifyPvP) return false;

        var purifyStatuses = new Dictionary<int, bool>
        {
            { 1343, Use1343PvP },
            { 3219, Use3219PvP },
            { 3022, Use3022PvP },
            { 1348, Use1348PvP },
            { 1345, Use1345PvP },
            { 1344, Use1344PvP },
            { 1347, Use1347PvP }
        };

        foreach (var status in purifyStatuses)
        {
            if (status.Value && Player.HasStatus(true, (StatusID)status.Key))
            {
                return PurifyPvP.CanUse(out action);
            }
        }

        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (ShouldCancelGuard()) return false;

        if (Player.GetHealthRatio() < HealthThreshold && RecuperatePvP.CanUse(out act)) return true;

        if (SnakeScalesPvP.Cooldown.IsCoolingDown && UncoiledFuryPvP.Cooldown.IsCoolingDown && RattlingCoilPvP.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (ShouldCancelGuard()) return false;

        if (IsLastGCD((ActionID)UncoiledFuryPvP.ID) && UncoiledTwinfangPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (IsLastGCD((ActionID)HuntersSnapPvP.ID) && TwinfangBitePvP.CanUse(out act)) return true;
        if (IsLastGCD((ActionID)SwiftskinsCoilPvP.ID) && TwinbloodBitePvP.CanUse(out act)) return true;
        if (IsLastGCD((ActionID)BarbarousBitePvP.ID, (ActionID)RavenousBitePvP.ID) && DeathRattlePvP.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (ShouldCancelGuard()) return false;

        return base.GeneralAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (ShouldCancelGuard()) return false;

        if (!Player.HasStatus(true, StatusID.Guard) && UseSprintPvP && !Player.HasStatus(true, StatusID.Sprint) && !InCombat && SprintPvP.CanUse(out act)) return true;

        if (Player.HasStatus(true, StatusID.HardenedScales)) return false;

        if (!Player.HasStatus(true, StatusID.Reawakened_4094))
        {
            if (SwiftskinsCoilPvP.CanUse(out act, usedUp: true)) return true;
            if (HuntersSnapPvP.CanUse(out act, usedUp: true)) return true;
        }

        if (UncoiledFuryPvP.CanUse(out act, skipAoeCheck: true)) return true;

        if (RavenousBitePvP.CanUse(out act)) return true;
        if (SwiftskinsStingPvP.CanUse(out act)) return true;
        if (PiercingFangsPvP.CanUse(out act)) return true;
        if (BarbarousBitePvP.CanUse(out act)) return true;
        if (HuntersStingPvP.CanUse(out act)) return true;
        if (SteelFangsPvP.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    private bool ShouldCancelGuard()
    {
        return GuardCancel && Player.HasStatus(true, StatusID.Guard);
    }
}