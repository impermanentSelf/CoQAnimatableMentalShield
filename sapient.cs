using System;
using XRL;
using XRL.World;

[Serializable]
public class Sapient : IPart
{
    public override bool SameAs(IPart p)    {
        return false;
    }

    public override bool WantEvent(int ID, int cascade)    {
        if (!base.WantEvent(ID, cascade))        {
            return ID == PooledEvent<GetDisplayNameEvent>.ID;
        }
        return true;
    }
    public override bool HandleEvent(GetDisplayNameEvent E)    {
        if (!E.Object.HasProperName && !E.Object.HasTagOrProperty("NoAnimatedNamePrefix"))        {
            E.AddAdjective("sapient", 5);
        }
        return base.HandleEvent(E);
    }
}
