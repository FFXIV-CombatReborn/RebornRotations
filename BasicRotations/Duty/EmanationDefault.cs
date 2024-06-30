using RotationSolver.Basic.Rotations.Duties;

namespace DefaultRotations.Duty;

[Rotation("Emanation Default", CombatType.PvE)]

internal class EmanationDefault : EmanationRotation
{
    public override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        // 8521 8522 8523
        bool Lol1 = HostileTarget?.CastActionId == 8521;
        bool Lol2 = HostileTarget?.CastActionId == 8522;
        bool Lol3 = HostileTarget?.CastActionId == 8523;

        if (Lol1 || Lol2 || Lol3)
        {
            if (VrilPvE.CanUse(out act)) return true; // Normal
            if (VrilPvE_9345.CanUse(out act)) return true; // Extreme
            return base.EmergencyAbility(nextGCD, out act);
        }

        act = null;
        return base.EmergencyAbility(nextGCD, out act);
    }
}