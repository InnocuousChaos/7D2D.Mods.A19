using System;

// Token: 0x020004E8 RID: 1256
public class ComplexPowerElectricWireRelay : ComplexPowerConsumer
{
	public override ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.ElectricWireRelay;
		}
	}
}
