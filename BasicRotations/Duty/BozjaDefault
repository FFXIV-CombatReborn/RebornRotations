using RotationSolver.Basic.Rotations.Duties;

namespace DefaultRotations.Duty;

[Rotation("Bozja Default",  CombatType.PvE)]
internal class BozjaDefault : BozjaRotation
{
    public override bool DefenseSingleGCD(out IAction? act)
    {
        if (LostStoneskinPvE.CanUse(out act)) return true;
        return base.DefenseSingleGCD(out act);
    }

    public override bool DefenseAreaGCD(out IAction? act)
    {
        if (LostStoneskinIiPvE.CanUse(out act)) return true;
        return base.DefenseAreaGCD(out act);
    }

    public override bool EmergencyGCD(out IAction? act)
    {
        #region Bozja
        //if (LostSpellforge.CanUse(out act)) return true;
        //if (LostSteelsting.CanUse(out act)) return true;
        //if (LostRampage.CanUse(out act)) return true;
        //if (LostBurst.CanUse(out act)) return true;

        //if (LostBravery.CanUse(out act)) return true;
        //if (LostBubble.CanUse(out act)) return true;
        //if (LostShell2.CanUse(out act)) return true;
        //if (LostShell.CanUse(out act)) return true;
        //if (LostProtect2.CanUse(out act)) return true;
        //if (LostProtect.CanUse(out act)) return true;

        ////Add your own logic here.
        //if (LostFlarestar.CanUse(out act)) return true;
        //if (LostSeraphStrike.CanUse(out act)) return true;

        #endregion
        return base.EmergencyGCD(out act);
    }
}
