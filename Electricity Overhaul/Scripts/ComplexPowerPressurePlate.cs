using System;
using Audio;

// Token: 0x020004E6 RID: 1254
public class ComplexPowerPressurePlate : ComplexPowerTrigger
{
	public override ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.PressurePlate;
		}
	}

	public bool Pressed
	{
		get
		{
			return this.pressed;
		}
		set
		{
			this.pressed = value;
			if (this.pressed && !this.lastPressed)
			{
				Manager.BroadcastPlay(this.Position.ToVector3(), "pressureplate_down", 0f);
			}
			this.lastPressed = this.pressed;
		}
	}

	protected override void CheckForActiveChange()
	{
		base.CheckForActiveChange();
		if (!this.pressed && this.lastPressed)
		{
			Manager.BroadcastPlay(this.Position.ToVector3(), "pressureplate_up", 0f);
			if (this.powerTime == 0f)
			{
				this.isActive = false;
				this.HandleDisconnectChildren();
				base.SendHasLocalChangesToRoot();
				this.powerTime = -1f;
			}
		}
	}

	protected override void HandleSoundDisable()
	{
		base.HandleSoundDisable();
		this.lastPressed = this.pressed;
		this.pressed = false;
	}

	protected bool pressed;

	protected bool lastPressed;
}
