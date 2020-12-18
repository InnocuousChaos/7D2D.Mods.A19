using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XNode;

// Token: 0x020004DF RID: 1247
public class ComplexPowerItem : PowerItem
{
	public virtual bool IsPowered
	{
		get
		{
			return this.isPowered;
		}
	}

	public ComplexPowerItem()
	{
		this.Children = new List<ComplexPowerItem>();
	}

	public virtual bool CanParent(ComplexPowerItem newParent)
	{
		return true;
	}

	public virtual int InputCount
	{
		get
		{
			return 1;
		}
	}

	public virtual ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.Consumer;
		}
	}

	public virtual void AddTileEntity(TileEntityPowered tileEntityPowered)
	{
		if (this.TileEntity == null)
		{
			this.TileEntity = tileEntityPowered;
			this.TileEntity.CreateWireDataFromPowerItem();
		}
		this.TileEntity.MarkWireDirty();
	}

	public void RemoveTileEntity(TileEntityPowered tileEntityPowered)
	{
		if (this.TileEntity == tileEntityPowered)
		{
			this.TileEntity = null;
		}
	}

	public virtual ComplexPowerItem GetRoot()
	{
		if (this.Parent != null)
		{
			return (ComplexPowerItem)this.Parent.GetRoot();
		}
		return this;
	}

	public virtual void read(BinaryReader _br, byte _version)
	{
		this.BlockID = _br.ReadUInt16();
		this.SetValuesFromBlock();
		this.Position = NetworkUtils.ReadVector3i(_br);
		if (_br.ReadBoolean())
		{
			ComplexPowerManager.Instance.SetParent(this, ComplexPowerManager.Instance.GetPowerItemByWorldPos(NetworkUtils.ReadVector3i(_br)));
		}
		int num = (int)_br.ReadByte();
		this.Children.Clear();
		for (int i = 0; i < num; i++)
		{
			ComplexPowerItem powerItem = ComplexPowerItem.CreateItem((ComplexPowerItem.PowerItemTypes)_br.ReadByte());
			powerItem.read(_br, _version);
			ComplexPowerManager.Instance.AddPowerNode(powerItem, this);
		}
	}

	public void RemoveSelfFromParent()
	{
		ComplexPowerManager.Instance.RemoveParent(this);
	}

	public virtual void write(BinaryWriter _bw)
	{
		_bw.Write(this.BlockID);
		NetworkUtils.Write(_bw, this.Position);
		_bw.Write(this.Parent != null);
		if (this.Parent != null)
		{
			NetworkUtils.Write(_bw, this.Parent.Position);
		}
		_bw.Write((byte)this.Children.Count);
		for (int i = 0; i < this.Children.Count; i++)
		{
			_bw.Write((byte)this.Children[i].PowerItemType);
			this.Children[i].write(_bw);
		}
	}

	public virtual bool PowerChildren()
	{
		return true;
	}

	protected virtual void IsPoweredChanged(bool newPowered)
	{
	}

	public virtual void HandlePowerReceived(ref ushort power)
	{
		ushort num = (ushort)Mathf.Min((int)this.RequiredPower, (int)power);
		bool flag = num == this.RequiredPower;
		if (flag != this.isPowered)
		{
			this.isPowered = flag;
			this.IsPoweredChanged(flag);
			if (this.TileEntity != null)
			{
				this.TileEntity.SetModified();
			}
		}
		power -= num;
		if (power <= 0)
		{
			return;
		}
		if (this.PowerChildren())
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].HandlePowerReceived(ref power);
				if (power <= 0)
				{
					return;
				}
			}
		}
	}

	internal ComplexPowerItem GetChild(Vector3 childPosition)
	{
		Vector3i other = new Vector3i(childPosition);
		for (int i = 0; i < this.Children.Count; i++)
		{
			if (this.Children[i].Position == other)
			{
				return this.Children[i];
			}
		}
		return null;
	}

	internal bool HasChild(Vector3 child)
	{
		Vector3i other = new Vector3i(child);
		for (int i = 0; i < this.Children.Count; i++)
		{
			if (this.Children[i].Position == other)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void HandlePowerUpdate(bool isOn)
	{
	}

	public virtual void HandleDisconnect()
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
	}

	public static ComplexPowerItem CreateItem(ComplexPowerItem.PowerItemTypes itemType)
	{
		switch (itemType)
		{
			case ComplexPowerItem.PowerItemTypes.Consumer:
				return new ComplexPowerConsumer();
			case ComplexPowerItem.PowerItemTypes.ConsumerToggle:
				return new ComplexPowerConsumerToggle();
			case ComplexPowerItem.PowerItemTypes.Trigger:
				return new ComplexPowerTrigger();
			case ComplexPowerItem.PowerItemTypes.Timer:
				return new ComplexPowerTimerRelay();
			case ComplexPowerItem.PowerItemTypes.Generator:
				return new ComplexPowerGenerator();
			case ComplexPowerItem.PowerItemTypes.SolarPanel:
				return new ComplexPowerSolarPanel();
			case ComplexPowerItem.PowerItemTypes.BatteryBank:
				return new ComplexPowerBatteryBank();
			case ComplexPowerItem.PowerItemTypes.RangedTrap:
				return new ComplexPowerRangedTrap();
			case ComplexPowerItem.PowerItemTypes.ElectricWireRelay:
				return new ComplexPowerElectricWireRelay();
			case ComplexPowerItem.PowerItemTypes.TripWireRelay:
				return new ComplexPowerTripWireRelay();
			case ComplexPowerItem.PowerItemTypes.PressurePlate:
				return new ComplexPowerPressurePlate();
			default:
				return new ComplexPowerItem();
		}
	}

	public virtual void SetValuesFromBlock()
	{
		Block block = Block.list[(int)this.BlockID];
		if (block.Properties.Values.ContainsKey("RequiredPower"))
		{
			this.RequiredPower = ushort.Parse(block.Properties.Values["RequiredPower"]);
		}
	}

	public void ClearChildren()
	{
		for (int i = 0; i < this.Children.Count; i++)
		{
			ComplexPowerManager.Instance.RemoveChild(this.Children[i]);
		}
		if (this.TileEntity != null)
		{
			this.TileEntity.DrawWires();
		}
	}

	public void SendHasLocalChangesToRoot()
	{
		this.hasChangesLocal = true;
		for (ComplexPowerItem parent = this.Parents[0]; parent != null; parent = parent.Parents[0])
		{
			parent.hasChangesLocal = true;
		}
	}

	public TileEntityPowered TileEntity;
	public Vector3i Position;
	public List<ComplexPowerItem> Parents;
	public List<ComplexPowerItem> Children;

	protected bool isPowered;

	public ushort Depth = ushort.MaxValue;

	public ushort BlockID;

	protected bool hasChangesLocal;

	public ushort RequiredPower = 5;




	public enum PowerItemTypes
	{
		// Token: 0x0400602B RID: 24619
		None,
		// Token: 0x0400602C RID: 24620
		Consumer,
		// Token: 0x0400602D RID: 24621
		ConsumerToggle,
		// Token: 0x0400602E RID: 24622
		Trigger,
		// Token: 0x0400602F RID: 24623
		Timer,
		// Token: 0x04006030 RID: 24624
		Generator,
		// Token: 0x04006031 RID: 24625
		SolarPanel,
		// Token: 0x04006032 RID: 24626
		BatteryBank,
		// Token: 0x04006033 RID: 24627
		RangedTrap,
		// Token: 0x04006034 RID: 24628
		ElectricWireRelay,
		// Token: 0x04006035 RID: 24629
		TripWireRelay,
		// Token: 0x04006036 RID: 24630
		PressurePlate
	}
}
