using System;
using XRL;
using XRL.World;

[Serializable]
public class Sapient : IPart
{
    int WillpowerAmount;
    int IntelligenceAmount;
    int EgoAmount;

    public override bool SameAs(IPart p)    {
        return false;
    }

    public override bool WantEvent(int ID, int cascade)    {
        if (!base.WantEvent(ID, cascade))        {
            return ID == PooledEvent<GetDisplayNameEvent>.ID;
        }
        return true;
    }

    public override void Initialize(){
        WillpowerAmount = Math.Min(4, Math.Max(0, 16 - ParentObject.Stat("Willpower",Default:4)));
        IntelligenceAmount = Math.Min(4, Math.Max(0, 16 - ParentObject.Stat("Intelligence",Default:4)));
        EgoAmount = Math.Min(4, Math.Max(0, 16 - ParentObject.Stat("Ego",Default:4)));

        base.StatShifter.SetStatShift(ParentObject, "Willpower", WillpowerAmount, baseValue:true);
        base.StatShifter.SetStatShift(ParentObject, "Intelligence", IntelligenceAmount, baseValue:true);
        base.StatShifter.SetStatShift(ParentObject, "Ego", EgoAmount, baseValue:true);
    }

    public override bool HandleEvent(GetDisplayNameEvent E)    {
        if (!E.Object.HasProperName && !E.Object.HasTagOrProperty("NoAnimatedNamePrefix"))        {
            E.AddAdjective("sapient", 5);
        }
        return base.HandleEvent(E);
    }
}
