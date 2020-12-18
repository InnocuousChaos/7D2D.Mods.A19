using System;
using System.IO;

// Token: 0x020004E1 RID: 1249
public class ComplexPowerRangedTrap : ComplexPowerConsumer
{
	public override ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.RangedTrap;
		}
	}

	public ComplexPowerRangedTrap()
	{
		this.Stacks = new ItemStack[3];
		for (int i = 0; i < this.Stacks.Length; i++)
		{
			this.Stacks[i] = ItemStack.Empty.Clone();
		}
	}

	public bool IsLocked
	{
		get
		{
			return this.isLocked;
		}
		set
		{
			if (this.isLocked != value)
			{
				this.isLocked = value;
			}
		}
	}

	public bool TryStackItem(ItemStack itemStack)
	{
		int num = 0;
		for (int i = 0; i < this.Stacks.Length; i++)
		{
			num = itemStack.count;
			if (this.Stacks[i].IsEmpty())
			{
				this.Stacks[i] = itemStack.Clone();
				itemStack.count = 0;
				return true;
			}
			if (this.Stacks[i].itemValue.type == itemStack.itemValue.type && this.Stacks[i].CanStackPartly(ref num))
			{
				this.Stacks[i].count += num;
				itemStack.count -= num;
				if (itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AddItem(ItemStack itemStack)
	{
		if (!this.isLocked)
		{
			for (int i = 0; i < this.Stacks.Length; i++)
			{
				if (this.Stacks[i].IsEmpty())
				{
					this.Stacks[i] = itemStack;
					return true;
				}
			}
		}
		return false;
	}

	public void SetSlots(ItemStack[] _stacks)
	{
		this.Stacks = _stacks;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		this.isLocked = _br.ReadBoolean();
		this.SetSlots(GameUtils.ReadItemStack(_br));
		this.TargetType = (ComplexPowerRangedTrap.TargetTypes)_br.ReadInt32();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.isLocked);
		GameUtils.WriteItemStack(_bw, this.Stacks);
		_bw.Write((int)this.TargetType);
	}

	public ItemStack[] Stacks;

	public ComplexPowerRangedTrap.TargetTypes TargetType = ComplexPowerRangedTrap.TargetTypes.Strangers | ComplexPowerRangedTrap.TargetTypes.Zombies;

	protected bool isLocked;

	[Flags]
	public enum TargetTypes
	{
		// Token: 0x04006038 RID: 24632
		None = 0,
		// Token: 0x04006039 RID: 24633
		Self = 1,
		// Token: 0x0400603A RID: 24634
		Allies = 2,
		// Token: 0x0400603B RID: 24635
		Strangers = 4,
		// Token: 0x0400603C RID: 24636
		Zombies = 8
	}
}
