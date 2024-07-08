using FFXIVClientStructs.FFXIV.Client.UI;

namespace DefaultRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.0")]
[SourceCode(Path = "main/DefaultRotations/Magical/PCT_Default.cs")]
[Api(2)]
public sealed class PCT_Default : PictomancerRotation
{

    private const ActionID PomMPVEActionID = (ActionID)34670;
    private IBaseAction PomMPvE = new BaseAction(PomMPVEActionID);

    private const ActionID WingMPVEActionID = (ActionID)34671;
    private IBaseAction WingMPvE = new BaseAction(WingMPVEActionID);

    private bool MogofAgesReady = false;
    private bool MogofAgesNotInCooldown = false;
    private bool PomMotifAvailable = false;
    private bool PomMuseAvailable = false;

    public override MedicineType MedicineType => MedicineType.Intelligence;
    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Emergency Logic
    // Determines emergency actions to take based on the next planned GCD action.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;

        return base.EmergencyAbility(nextGCD, out act);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        //LivingMusePve.AdjusteId == 34670 rdy to cast Pom Muse 34671 Winged Muse  , 35347 not ready to cast anything
        //SteelMusePve.AdjusteId == 34674 rdy to cast Striking Muse  , 35348 not ready to cast anything
        //ScenicMusePve.AdjusteId ==  34675, 35349 not ready to cast anything
        bool PomMotifReady = (CreatureMotifPvE.AdjustedID == 34664);
        bool PomMuseReady = (LivingMusePvE.AdjustedID == 34670);
        bool WingedMusefReady = (LivingMusePvE.AdjustedID == 34671);

        if(MogOfTheAgesPvE.Cooldown.IsCoolingDown)
        {
            MogofAgesNotInCooldown = false;
            MogofAgesReady = false;
            PomMotifAvailable = false;
            PomMuseAvailable = false;
        }

        if (!MogOfTheAgesPvE.Cooldown.IsCoolingDown)
        {
            MogofAgesNotInCooldown = true;
        }

        if (MogofAgesNotInCooldown && !PomMotifAvailable && PomMotifReady)
        {
            PomMotifAvailable = true;
        }

        if (MogofAgesNotInCooldown && !PomMuseAvailable && PomMuseReady)
        {
            PomMuseAvailable = true;
        }

        if (MogofAgesNotInCooldown && (PomMuseAvailable || PomMotifAvailable))
        {
            MogofAgesReady = true;
        }

        if (!Player.HasStatus(true, StatusID.SubtractivePalette) && (PaletteGauge >= 50) && SubtractivePalettePvE.CanUse(out act)) return true;

        //landscape to be use before mog of ages
        //if (LandscapeMotifDrawn && (PomMotifReady || PomMuseReady) && !MogOfTheAgesPvE.Cooldown.IsCoolingDown && StarryMusePvE.CanUse(out act, skipStatusProvideCheck: true, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, usedUp: true)) return true;
        if (LandscapeMotifDrawn && MogofAgesReady && !MogOfTheAgesPvE.Cooldown.IsCoolingDown && StarryMusePvE.CanUse(out act, skipStatusProvideCheck: true, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, usedUp: true)) return true;
        if (MogOfTheAgesPvE.CanUse(out act, skipStatusProvideCheck: true, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, usedUp: true)) return true;

        if (CreatureMotifDrawn && PomMuseReady && PomMPvE.CanUse(out act, skipStatusProvideCheck: true, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, usedUp: true)) return true;
        if (CreatureMotifDrawn && WingedMusefReady && WingMPvE.CanUse(out act, skipStatusProvideCheck: true, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, usedUp: true)) return true;
        if (WeaponMotifDrawn && StrikingMusePvE.CanUse(out act, skipStatusProvideCheck: true, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, usedUp: true)) return true;

        //kept just in case
        //if (LandscapeMotifDrawn && StarryMusePvE.CanUse(out act, skipStatusProvideCheck: true, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, usedUp: true)) return true;
        


        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        act = null;


        return base.MoveForwardAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool MoveForwardGCD(out IAction? act)
    {
        act = null;

        return base.MoveForwardGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {

        bool HammerMotifReady = (WeaponMotifPvE.AdjustedID == 34668);
        bool WingMotifReady = (CreatureMotifPvE.AdjustedID == 34665);
        bool PomMotifReady = (CreatureMotifPvE.AdjustedID == 34664);
        bool StarryMotifReady = (LandscapeMotifPvE.AdjustedID == 34669);
        //WeaponMotifPvE.AdjustedID == 34668 => rdy to cast HammerMotif, 34690 not ready to cast anything
        //CreatureMotifPve.AdjustedID == 34665 => rdy to cast WingMotif / 34664 =>rdy to cast PomMotif , 34689 not ready to cast anything
        //LandscapeMotifPvE .AdjustedID ==  34669 => rdy to cast StarrySkyMotif , 34691 not ready to cast anything

        if (Player.HasStatus(true, StatusID.HammerTime) && HammerStampPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true)) return true;

        if (!CreatureMotifDrawn && WingMotifReady && WingMotifPvE.CanUse(out act)) return true;
        if (!CreatureMotifDrawn && PomMotifReady && PomMotifPvE.CanUse(out act)) return true;
        if (!WeaponMotifDrawn && HammerMotifReady &&  HammerMotifPvE.CanUse(out act)) return true;
        if (!LandscapeMotifDrawn && StarryMotifReady && StarrySkyMotifPvE.CanUse(out act)) return true;
        if (Player.HasStatus(true, StatusID.SubtractivePalette))
        {
            //AOE
            if (ThunderIiInMagentaPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (StoneIiInYellowPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (BlizzardIiInCyanPvE.CanUse(out act, skipCastingCheck: true)) return true;

            //123
            if (ThunderInMagentaPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (StoneInYellowPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (BlizzardInCyanPvE.CanUse(out act, skipCastingCheck: true)) return true;

        }
        else
        {
            if (HolyInWhitePvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true)) return true;
            //AOE
            if (WaterIiInBluePvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (AeroIiInGreenPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (FireIiInRedPvE.CanUse(out act, skipCastingCheck: true)) return true;

            //123
            if (WaterInBluePvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (AeroInGreenPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (FireInRedPvE.CanUse(out act, skipCastingCheck: true)) return true;
        }


        return base.GeneralGCD(out act);
    }

    private bool AttackGCD(out IAction? act, bool burst)
    {
        act = null;

        return false;
    }
    #endregion

    #region Extra Methods
    // Extra private helper methods for determining the usability of specific abilities under certain conditions.
    // These methods simplify the main logic by encapsulating specific checks related to abilities' cooldowns and prerequisites.
    //private bool CanUseExamplePvE(out IAction? act)
    //{

    //}

    public override void DisplayStatus()
    {
        //motif
        ImGui.Text("-----Motif");
        ImGui.Text("HammerMotif " + HammerMotifPvE.AdjustedID.ToString());
        ImGui.Text("WeaponMotif adjID " + WeaponMotifPvE.AdjustedID.ToString());
        ImGui.Text("-----");
        ImGui.Text("WingMotif " + WingMotifPvE.AdjustedID.ToString());
        ImGui.Text("CreatureMotif adjID " + CreatureMotifPvE.AdjustedID.ToString());
        ImGui.Text("-----");
        ImGui.Text("StarrySkyMotif " + StarrySkyMotifPvE.AdjustedID.ToString());
        ImGui.Text("LandscapeMotif adjID " + LandscapeMotifPvE.AdjustedID.ToString());

        //muse
        ImGui.Text("-----Muse");
        ImGui.Text("PomMuse " + PomMusePvE.AdjustedID.ToString());
        ImGui.Text("LivingMuse adjID " + LivingMusePvE.AdjustedID.ToString());
        ImGui.Text("-----");
        ImGui.Text("StrikingMuse " + StrikingMusePvE.AdjustedID.ToString());
        ImGui.Text("SteelMuse adjID " + SteelMusePvE.AdjustedID.ToString());
        ImGui.Text("-----");
        ImGui.Text("StarryMuse " + StarryMusePvE.AdjustedID.ToString());
        ImGui.Text("ScenicMuse adjID " + ScenicMusePvE.AdjustedID.ToString());

        bool PomMotifReady = (CreatureMotifPvE.AdjustedID == 34664);
        bool PomMuseReady = (LivingMusePvE.AdjustedID == 34670);
        //pom starry sky
        ImGui.Text("-----Pom Starry Sky");
        ImGui.Text("PomMotifReady " + PomMotifReady.ToString());
        ImGui.Text("PomMuseReady " + PomMuseReady.ToString());
        ImGui.Text("StarryMuse " + StarryMusePvE.Cooldown.IsCoolingDown.ToString());
        ImGui.Text("MogofAges enabled " + MogOfTheAgesPvE.IsEnabled.ToString());
        ImGui.Text("MogofAges incooldown " + MogOfTheAgesPvE.IsInCooldown.ToString());
        ImGui.Text("MogofAges iscoolingdown " + MogOfTheAgesPvE.Cooldown.IsCoolingDown.ToString());
        ImGui.Text("MogofAges cooldown " + MogOfTheAgesPvE.Info.ToString());

        base.DisplayStatus();
    }
    #endregion
}