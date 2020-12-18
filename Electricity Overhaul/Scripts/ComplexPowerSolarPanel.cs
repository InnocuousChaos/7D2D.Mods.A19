using System;
using System.IO;
using Audio;
using UnityEngine;

// Token: 0x020004E2 RID: 1250
public class ComplexPowerSolarPanel : ComplexPowerSource
{
	public bool HasLight { get; private set; }

	public override ComplexPowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return ComplexPowerItem.PowerItemTypes.SolarPanel;
		}
	}

	public override string OnSound
	{
		get
		{
			return "solarpanel_on";
		}
	}

	public override string OffSound
	{
		get
		{
			return "solarpanel_off";
		}
	}

	private void CheckLightLevel()
	{
		if (this.TileEntity != null)
		{
			Chunk chunk = this.TileEntity.GetChunk();
			Vector3i localChunkPos = this.TileEntity.localChunkPos;
			this.sunLight = chunk.GetLight(localChunkPos.x, localChunkPos.y, localChunkPos.z, Chunk.LIGHT_TYPE.SUN);
		}
		this.lastHasLight = this.HasLight;
		this.HasLight = (this.sunLight == 15 && GameManager.Instance.World.IsDaytime());
		if (this.lastHasLight != this.HasLight)
		{
			this.HandleOnOffSound();
			if (!this.HasLight)
			{
				this.CurrentPower = 0;
				this.HandleDisconnect();
				return;
			}
			base.SendHasLocalChangesToRoot();
		}
	}

	protected override void TickPowerGeneration()
	{
		if (this.HasLight)
		{
			this.CurrentPower = this.MaxOutput;
		}
	}

	public override void HandleSendPower()
	{
		if (base.IsOn)
		{
			if (Time.time > this.lightUpdateTime)
			{
				this.lightUpdateTime = Time.time + 2f;
				this.CheckLightLevel();
			}
			if (this.HasLight)
			{
				if (this.CurrentPower < this.MaxPower)
				{
					this.TickPowerGeneration();
				}
				else if (this.CurrentPower > this.MaxPower)
				{
					this.CurrentPower = this.MaxPower;
				}
				if (this.ShouldAutoTurnOff())
				{
					this.CurrentPower = 0;
					base.IsOn = false;
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
				if (this.LastPowerUsed >= this.CurrentPower)
				{
					base.SendHasLocalChangesToRoot();
					this.CurrentPower = 0;
					return;
				}
				this.CurrentPower -= this.LastPowerUsed;
			}
		}
	}

	protected bool ShouldClearPower()
	{
		return this.sunLight != 15 || !GameManager.Instance.World.IsDaytime();
	}

	protected override void HandleOnOffSound()
	{
		Vector3 position = this.Position.ToVector3();
		Manager.BroadcastPlay(position, (this.isOn && this.HasLight) ? this.OnSound : this.OffSound, 0f);
		if (this.isOn && this.HasLight)
		{
			Manager.BroadcastPlay(position, this.runningSound, 0f);
			return;
		}
		Manager.BroadcastStop(position, this.runningSound);
	}

	protected override void RefreshPowerStats()
	{
		base.RefreshPowerStats();
		this.MaxPower = this.MaxOutput;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		if (ComplexPowerManager.Instance.CurrentFileVersion >= 2)
		{
			this.sunLight = _br.ReadByte();
		}
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.sunLight);
	}

	public ushort InputFromSun;

	private byte sunLight;

	private bool lastHasLight;

	private string runningSound = "solarpanel_idle";

	private float lightUpdateTime;
}
