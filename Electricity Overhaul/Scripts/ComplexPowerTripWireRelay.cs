using System;

// Token: 0x020004E7 RID: 1255
public class ComplexPowerTripWireRelay : ComplexPowerTrigger
{
	public override ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.TripWireRelay;
		}
	}
}
