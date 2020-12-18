﻿using System;
using UnityEngine;

// Token: 0x020004DA RID: 1242
public class ComplexPowerBatteryBank : ComplexPowerSource
{
	public override ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.BatteryBank;
		}
	}

	public override string OnSound
	{
		get
		{
			return "batterybank_start";
		}
	}

	public override string OffSound
	{
		get
		{
			return "batterybank_stop";
		}
	}

	public override bool CanParent(PowerItem parent)
	{
		return true;
	}

	public override bool IsPowered
	{
		get
		{
			return this.isOn || this.isPowered;
		}
	}

	protected bool ParentPowering
	{
		get
		{
			if (this.Parent == null)
			{
				return false;
			}
			if (this.Parent is ComplexPowerSolarPanel)
			{
				ComplexPowerSolarPanel ComplexPowerSolarPanel = this.Parent as ComplexPowerSolarPanel;
				return ComplexPowerSolarPanel.HasLight && ComplexPowerSolarPanel.IsOn;
			}
			if (this.Parent is ComplexPowerSource)
			{
				return (this.Parent as ComplexPowerSource).IsOn;
			}
			if (this.Parent is ComplexPowerTrigger)
			{
				return this.Parent.IsPowered && (this.Parent as ComplexPowerTrigger).IsActive;
			}
			return this.Parent.IsPowered;
		}
	}

	public override void Update()
	{
		if (this.Parent != null && this.LastPowerReceived > 0)
		{
			if (this.LastInputAmount > 0 && base.IsOn)
			{
				this.AddPowerToBatteries((int)this.LastInputAmount);
			}
			return;
		}
		base.Update();
	}

	public override void HandleSendPower()
	{
		if (base.IsOn && !this.ParentPowering)
		{
			if (this.CurrentPower < this.MaxPower)
			{
				this.TickPowerGeneration();
			}
			else if (this.CurrentPower > this.MaxPower)
			{
				this.CurrentPower = this.MaxPower;
			}
			if (this.CurrentPower <= 0)
			{
				this.CurrentPower = 0;
				if (this.isPowered)
				{
					this.HandleDisconnect();
					this.hasChangesLocal = true;
				}
			}
			else
			{
				this.isPowered = true;
			}
			if (this.hasChangesLocal)
			{
				this.LastPowerUsed = 0;
				ushort num = (ushort)Mathf.Min((int)this.MaxOutput, (int)this.CurrentPower);
				ushort num2 = num;
				World world = GameManager.Instance.World;
				for (int i = 0; i < this.Children.Count; i++)
				{
					num = num2;
					this.Children[i].HandlePowerReceived(ref num2);
					this.LastPowerUsed += (ushort)(num - num2);
				}
			}
			this.CurrentPower -= (ushort)Mathf.Min((int)this.CurrentPower, (int)this.LastPowerUsed);
		}
	}

	public override void HandlePowerReceived(ref ushort power)
	{
		this.LastPowerUsed = 0;
		if (this.LastPowerReceived != power)
		{
			this.LastPowerReceived = power;
			this.hasChangesLocal = true;
			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].HandleDisconnect();
			}
		}
		if (power <= 0)
		{
			return;
		}
		if (base.IsOn && power > 0)
		{
			ushort power2 = (ushort)Mathf.Min((int)this.InputPerTick, (int)power);
			this.AddPowerToBatteries((int)power2);
			power -= this.LastInputAmount;
		}
		if (this.PowerChildren())
		{
			for (int j = 0; j < this.Children.Count; j++)
			{
				this.Children[j].HandlePowerReceived(ref power);
				if (power <= 0)
				{
					return;
				}
			}
		}
	}

	protected void AddPowerToBatteries(int power)
	{
		int num = power;
		int b = power / (int)this.InputPerTick * (int)this.ChargePerInput;
		for (int i = this.Stacks.Length - 1; i >= 0; i--)
		{
			if (!this.Stacks[i].IsEmpty())
			{
				int num2 = (int)this.Stacks[i].itemValue.UseTimes;
				if (num2 > 0)
				{
					ushort num3 = (ushort)Mathf.Min(num2, b);
					num -= (int)(num3 * this.InputPerTick);
					this.Stacks[i].itemValue.UseTimes -= (float)num3;
				}
				if (num == 0)
				{
					break;
				}
			}
		}
		int num4 = power - num;
		if (this.LastInputAmount != (ushort)num4)
		{
			base.SendHasLocalChangesToRoot();
			this.LastInputAmount = (ushort)num4;
		}
	}

	protected override void TickPowerGeneration()
	{
		base.TickPowerGeneration();
		ushort num = (ushort)(this.MaxPower - this.CurrentPower);
		ushort num2 = (ushort)(num / this.OutputPerCharge);
		if (num >= this.OutputPerCharge)
		{
			for (int i = 0; i < this.Stacks.Length; i++)
			{
				int num3 = (int)Mathf.Min((float)this.Stacks[i].itemValue.MaxUseTimes - this.Stacks[i].itemValue.UseTimes, (float)num2);
				if (num3 > 0)
				{
					this.Stacks[i].itemValue.UseTimes += (float)num3;
					this.CurrentPower += (ushort)(num3 * (int)this.OutputPerCharge);
					return;
				}
			}
		}
	}

	public override bool PowerChildren()
	{
		return true;
	}

	public override void HandlePowerUpdate(bool isOn)
	{
		if (this.Parent != null && this.LastPowerReceived > 0 && this.PowerChildren())
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].HandlePowerUpdate(isOn);
			}
		}
	}

	public override void HandleDisconnect()
	{
		if (this.isPowered)
		{
			this.IsPoweredChanged(false);
		}
		this.isPowered = false;
		this.HandlePowerUpdate(false);
		for (int i = 0; i < this.Children.Count; i++)
		{
			this.Children[i].HandleDisconnect();
		}
		this.LastInputAmount = 0;
		this.LastPowerReceived = 0;
		if (this.TileEntity != null)
		{
			this.TileEntity.SetModified();
		}
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[(int)this.BlockID];
		if (block.Properties.Values.ContainsKey("InputPerTick"))
		{
			this.InputPerTick = ushort.Parse(block.Properties.Values["InputPerTick"]);
		}
		if (block.Properties.Values.ContainsKey("ChargePerInput"))
		{
			this.ChargePerInput = ushort.Parse(block.Properties.Values["ChargePerInput"]);
		}
		if (block.Properties.Values.ContainsKey("OutputPerCharge"))
		{
			this.OutputPerCharge = ushort.Parse(block.Properties.Values["OutputPerCharge"]);
		}
		if (block.Properties.Values.ContainsKey("MaxPower"))
		{
			this.MaxPower = ushort.Parse(block.Properties.Values["MaxPower"]);
		}
	}

	public ushort LastInputAmount;

	public ushort LastPowerReceived;

	public ushort InputPerTick;

	public ushort ChargePerInput;

	public ushort OutputPerCharge;
}
