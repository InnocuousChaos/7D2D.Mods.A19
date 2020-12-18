using System;
using System.IO;
using Audio;
using UnityEngine;

// Token: 0x020004E5 RID: 1253
public class ComplexPowerTrigger : ComplexPowerConsumer
{
	public override ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.Trigger;
		}
	}

	public ComplexPowerTrigger.TriggerPowerDelayTypes TriggerPowerDelay
	{
		get
		{
			return this.triggerPowerDelay;
		}
		set
		{
			this.triggerPowerDelay = value;
		}
	}

	public ComplexPowerTrigger.TriggerPowerDurationTypes TriggerPowerDuration
	{
		get
		{
			return this.triggerPowerDuration;
		}
		set
		{
			this.triggerPowerDuration = value;
		}
	}

	public virtual bool IsActive
	{
		get
		{
			if (this.TriggerType == ComplexPowerTrigger.TriggerTypes.Switch)
			{
				return this.isTriggered;
			}
			return this.isActive || this.parentTriggered;
		}
	}

	public virtual bool IsTriggered
	{
		get
		{
			return this.isTriggered;
		}
		set
		{
			if (this.TriggerType == ComplexPowerTrigger.TriggerTypes.Switch)
			{
				this.lastTriggered = this.isTriggered;
				this.isTriggered = value;
				if (this.isTriggered && !this.lastTriggered)
				{
					this.isActive = true;
				}
				base.SendHasLocalChangesToRoot();
				if (!this.isTriggered && this.lastTriggered)
				{
					this.HandleDisconnectChildren();
					this.isActive = false;
					return;
				}
			}
			else
			{
				this.isTriggered = value;
				if (this.isTriggered && !this.lastTriggered)
				{
					ComplexPowerTrigger.TriggerTypes triggerType = this.TriggerType;
					if (triggerType != ComplexPowerTrigger.TriggerTypes.Motion)
					{
						if (triggerType == ComplexPowerTrigger.TriggerTypes.TripWire)
						{
							Manager.BroadcastPlay(this.Position.ToVector3(), "trip_wire_trigger", 0f);
						}
					}
					else
					{
						Manager.BroadcastPlay(this.Position.ToVector3(), "motion_sensor_trigger", 0f);
					}
					base.SendHasLocalChangesToRoot();
				}
				this.lastTriggered = this.isTriggered;
				if (this.IsPowered && !this.isActive && this.delayStartTime == -1f)
				{
					this.lastPowerTime = Time.time;
					this.delayStartTime = -1f;
					switch (this.TriggerPowerDelay)
					{
						case ComplexPowerTrigger.TriggerPowerDelayTypes.OneSecond:
							this.delayStartTime = 1f;
							break;
						case ComplexPowerTrigger.TriggerPowerDelayTypes.TwoSecond:
							this.delayStartTime = 2f;
							break;
						case ComplexPowerTrigger.TriggerPowerDelayTypes.ThreeSecond:
							this.delayStartTime = 3f;
							break;
						case ComplexPowerTrigger.TriggerPowerDelayTypes.FourSecond:
							this.delayStartTime = 4f;
							break;
						case ComplexPowerTrigger.TriggerPowerDelayTypes.FiveSecond:
							this.delayStartTime = 5f;
							break;
					}
					if (this.delayStartTime == -1f)
					{
						this.isActive = true;
						this.SetupDurationTime();
					}
				}
				this.parentTriggered = false;
			}
		}
	}

	protected void SetupDurationTime()
	{
		this.lastPowerTime = Time.time;
		switch (this.TriggerPowerDuration)
		{
			case ComplexPowerTrigger.TriggerPowerDurationTypes.Always:
				this.powerTime = -1f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.Triggered:
				this.powerTime = 0f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.OneSecond:
				this.powerTime = 1f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.TwoSecond:
				this.powerTime = 2f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.ThreeSecond:
				this.powerTime = 3f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.FourSecond:
				this.powerTime = 4f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.FiveSecond:
				this.powerTime = 5f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.SixSecond:
				this.powerTime = 6f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.SevenSecond:
				this.powerTime = 7f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.EightSecond:
				this.powerTime = 8f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.NineSecond:
				this.powerTime = 9f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.TenSecond:
				this.powerTime = 10f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.FifteenSecond:
				this.powerTime = 15f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.ThirtySecond:
				this.powerTime = 30f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.FourtyFiveSecond:
				this.powerTime = 45f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.OneMinute:
				this.powerTime = 60f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.FiveMinute:
				this.powerTime = 300f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.TenMinute:
				this.powerTime = 600f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.ThirtyMinute:
				this.powerTime = 1800f;
				return;
			case ComplexPowerTrigger.TriggerPowerDurationTypes.SixtyMinute:
				this.powerTime = 3600f;
				return;
			default:
				return;
		}
	}

	public override bool PowerChildren()
	{
		return true;
	}

	public override void HandlePowerReceived(ref ushort power)
	{
		ushort num = (ushort)Mathf.Min((int)this.RequiredPower, (int)power);
		num = (ushort)Mathf.Min((int)num, (int)this.RequiredPower);
		this.isPowered = (num == this.RequiredPower);
		power -= num;
		if (power <= 0)
		{
			return;
		}
		this.CheckForActiveChange();
		if (this.PowerChildren())
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				if (this.Children[i] is ComplexPowerTrigger)
				{
					ComplexPowerTrigger ComplexPowerTrigger = this.Children[i] as ComplexPowerTrigger;
					this.HandleParentTriggering(ComplexPowerTrigger);
					if ((this.TriggerType == ComplexPowerTrigger.TriggerTypes.Motion || this.TriggerType == ComplexPowerTrigger.TriggerTypes.PressurePlate || this.TriggerType == ComplexPowerTrigger.TriggerTypes.TripWire) && (ComplexPowerTrigger.TriggerType == ComplexPowerTrigger.TriggerTypes.Motion || ComplexPowerTrigger.TriggerType == ComplexPowerTrigger.TriggerTypes.PressurePlate || ComplexPowerTrigger.TriggerType == ComplexPowerTrigger.TriggerTypes.TripWire))
					{
						ComplexPowerTrigger.HandlePowerReceived(ref power);
					}
					else if (this.IsActive)
					{
						ComplexPowerTrigger.HandlePowerReceived(ref power);
					}
				}
				else if (this.IsActive)
				{
					this.Children[i].HandlePowerReceived(ref power);
				}
				if (power <= 0)
				{
					return;
				}
			}
		}
	}

	protected virtual void CheckForActiveChange()
	{
		if (this.powerTime == 0f && this.lastTriggered && !this.isTriggered)
		{
			this.isActive = false;
			this.HandleDisconnectChildren();
			base.SendHasLocalChangesToRoot();
			this.powerTime = -1f;
		}
	}

	protected virtual void HandleSingleUseDisable()
	{
		ComplexPowerTrigger.TriggerTypes triggerType = this.TriggerType;
		if (triggerType == ComplexPowerTrigger.TriggerTypes.PressurePlate || triggerType - ComplexPowerTrigger.TriggerTypes.Motion <= 1)
		{
			this.lastTriggered = this.isTriggered;
			this.isTriggered = false;
		}
	}

	protected virtual void HandleSoundDisable()
	{
	}

	public override void HandlePowerUpdate(bool parentIsOn)
	{
		if (this.TileEntity != null)
		{
			((TileEntityPoweredTrigger)this.TileEntity).Activate(this.isPowered && parentIsOn, this.isTriggered);
			this.TileEntity.SetModified();
		}
		for (int i = 0; i < this.Children.Count; i++)
		{
			if (this.Children[i] is ComplexPowerTrigger)
			{
				ComplexPowerTrigger child = this.Children[i] as ComplexPowerTrigger;
				this.HandleParentTriggering(child);
				this.Children[i].HandlePowerUpdate(this.isPowered && parentIsOn);
			}
			else if (this.IsActive)
			{
				this.Children[i].HandlePowerUpdate(this.isPowered && parentIsOn);
			}
		}
		this.hasChangesLocal = true;
		this.HandleSingleUseDisable();
	}

	protected void HandleParentTriggering(ComplexPowerTrigger child)
	{
		if (!this.IsActive)
		{
			child.SetTriggeredByParent(false);
			return;
		}
		if ((this.TriggerType == ComplexPowerTrigger.TriggerTypes.Motion || this.TriggerType == ComplexPowerTrigger.TriggerTypes.PressurePlate || this.TriggerType == ComplexPowerTrigger.TriggerTypes.TripWire) && (child.TriggerType == ComplexPowerTrigger.TriggerTypes.Motion || child.TriggerType == ComplexPowerTrigger.TriggerTypes.PressurePlate || child.TriggerType == ComplexPowerTrigger.TriggerTypes.TripWire))
		{
			child.SetTriggeredByParent(true);
			return;
		}
		child.SetTriggeredByParent(false);
	}

	public void SetTriggeredByParent(bool triggered)
	{
		this.parentTriggered = triggered;
	}

	public virtual void CachedUpdateCall()
	{
		ComplexPowerTrigger.TriggerTypes triggerType = this.TriggerType;
		if (triggerType == ComplexPowerTrigger.TriggerTypes.PressurePlate || triggerType - ComplexPowerTrigger.TriggerTypes.Motion <= 1)
		{
			if (!this.hasChangesLocal)
			{
				if (this.isTriggered != this.lastTriggered)
				{
					base.SendHasLocalChangesToRoot();
				}
				this.CheckForActiveChange();
				this.HandleSingleUseDisable();
			}
			if (this.delayStartTime >= 0f)
			{
				if (Time.time - this.lastPowerTime >= this.delayStartTime)
				{
					base.SendHasLocalChangesToRoot();
					this.delayStartTime = -1f;
					this.isActive = true;
					this.SetupDurationTime();
				}
			}
			else if (this.powerTime > 0f && !this.parentTriggered && Time.time - this.lastPowerTime >= this.powerTime)
			{
				this.isActive = false;
				this.HandleDisconnectChildren();
				base.SendHasLocalChangesToRoot();
				this.powerTime = -1f;
			}
			this.hasChangesLocal = false;
			this.HandleSoundDisable();
		}
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		this.TriggerType = (ComplexPowerTrigger.TriggerTypes)_br.ReadByte();
		if (this.TriggerType == ComplexPowerTrigger.TriggerTypes.Switch)
		{
			this.isTriggered = _br.ReadBoolean();
		}
		else
		{
			this.isActive = _br.ReadBoolean();
		}
		if (this.TriggerType != ComplexPowerTrigger.TriggerTypes.Switch)
		{
			this.TriggerPowerDelay = (ComplexPowerTrigger.TriggerPowerDelayTypes)_br.ReadByte();
			this.TriggerPowerDuration = (ComplexPowerTrigger.TriggerPowerDurationTypes)_br.ReadByte();
			this.delayStartTime = _br.ReadSingle();
			this.powerTime = _br.ReadSingle();
		}
		if (this.TriggerType == ComplexPowerTrigger.TriggerTypes.Motion)
		{
			this.TargetType = (ComplexPowerTrigger.TargetTypes)_br.ReadInt32();
		}
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.TriggerType);
		if (this.TriggerType == ComplexPowerTrigger.TriggerTypes.Switch)
		{
			_bw.Write(this.isTriggered);
		}
		else
		{
			_bw.Write(this.isActive);
		}
		if (this.TriggerType != ComplexPowerTrigger.TriggerTypes.Switch)
		{
			_bw.Write((byte)this.TriggerPowerDelay);
			_bw.Write((byte)this.TriggerPowerDuration);
			_bw.Write(this.delayStartTime);
			_bw.Write(this.powerTime);
		}
		if (this.TriggerType == ComplexPowerTrigger.TriggerTypes.Motion)
		{
			_bw.Write((int)this.TargetType);
		}
	}

	public virtual void HandleDisconnectChildren()
	{
		this.HandlePowerUpdate(false);
		for (int i = 0; i < this.Children.Count; i++)
		{
			this.Children[i].HandleDisconnect();
		}
	}

	public override void HandleDisconnect()
	{
		this.parentTriggered = (this.isActive = false);
		base.HandleDisconnect();
	}

	public void ResetTrigger()
	{
		this.delayStartTime = -1f;
		this.powerTime = -1f;
		this.isActive = false;
		this.HandleDisconnectChildren();
		base.SendHasLocalChangesToRoot();
	}

	public ComplexPowerTrigger.TriggerTypes TriggerType;

	public byte Parameter;

	protected ComplexPowerTrigger.TriggerPowerDelayTypes triggerPowerDelay;

	protected ComplexPowerTrigger.TriggerPowerDurationTypes triggerPowerDuration = ComplexPowerTrigger.TriggerPowerDurationTypes.Triggered;

	public ComplexPowerTrigger.TargetTypes TargetType = ComplexPowerTrigger.TargetTypes.Self | ComplexPowerTrigger.TargetTypes.Allies;

	protected float delayStartTime = -1f;

	protected float powerTime;

	protected float lastPowerTime = -1f;

	protected bool lastTriggered;

	protected bool isTriggered;

	protected bool parentTriggered;

	protected bool isActive;

	public enum TriggerTypes
	{
		// Token: 0x0400603E RID: 24638
		Switch,
		// Token: 0x0400603F RID: 24639
		PressurePlate,
		// Token: 0x04006040 RID: 24640
		TimerRelay,
		// Token: 0x04006041 RID: 24641
		Motion,
		// Token: 0x04006042 RID: 24642
		TripWire
	}

	public enum TriggerPowerDelayTypes
	{
		// Token: 0x04006044 RID: 24644
		Instant,
		// Token: 0x04006045 RID: 24645
		OneSecond,
		// Token: 0x04006046 RID: 24646
		TwoSecond,
		// Token: 0x04006047 RID: 24647
		ThreeSecond,
		// Token: 0x04006048 RID: 24648
		FourSecond,
		// Token: 0x04006049 RID: 24649
		FiveSecond
	}

	public enum TriggerPowerDurationTypes
	{
		// Token: 0x0400604B RID: 24651
		Always,
		// Token: 0x0400604C RID: 24652
		Triggered,
		// Token: 0x0400604D RID: 24653
		OneSecond,
		// Token: 0x0400604E RID: 24654
		TwoSecond,
		// Token: 0x0400604F RID: 24655
		ThreeSecond,
		// Token: 0x04006050 RID: 24656
		FourSecond,
		// Token: 0x04006051 RID: 24657
		FiveSecond,
		// Token: 0x04006052 RID: 24658
		SixSecond,
		// Token: 0x04006053 RID: 24659
		SevenSecond,
		// Token: 0x04006054 RID: 24660
		EightSecond,
		// Token: 0x04006055 RID: 24661
		NineSecond,
		// Token: 0x04006056 RID: 24662
		TenSecond,
		// Token: 0x04006057 RID: 24663
		FifteenSecond,
		// Token: 0x04006058 RID: 24664
		ThirtySecond,
		// Token: 0x04006059 RID: 24665
		FourtyFiveSecond,
		// Token: 0x0400605A RID: 24666
		OneMinute,
		// Token: 0x0400605B RID: 24667
		FiveMinute,
		// Token: 0x0400605C RID: 24668
		TenMinute,
		// Token: 0x0400605D RID: 24669
		ThirtyMinute,
		// Token: 0x0400605E RID: 24670
		SixtyMinute
	}

	[Flags]
	public enum TargetTypes
	{
		// Token: 0x04006060 RID: 24672
		None = 0,
		// Token: 0x04006061 RID: 24673
		Self = 1,
		// Token: 0x04006062 RID: 24674
		Allies = 2,
		// Token: 0x04006063 RID: 24675
		Strangers = 4,
		// Token: 0x04006064 RID: 24676
		Zombies = 8
	}
}
