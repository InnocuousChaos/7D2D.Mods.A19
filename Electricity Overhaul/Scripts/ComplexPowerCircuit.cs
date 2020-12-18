using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;


class ComplexPowerCircuit
{
	private const float UPDATE_TIME_SEC = 0.16f;
	private const float SAVE_TIME_SEC = 120f;
	private float updateTime;
	private List<ComplexPowerItem> PowerItems;
	private List<ComplexPowerSource> PowerSources;
	private List<ComplexPowerTrigger> PowerTriggers;
	private List<ComplexPowerConsumer> PowerConsumers;
	private Dictionary<Vector3i, ComplexPowerItem> PowerItemDictionary = new Dictionary<Vector3i, ComplexPowerItem>();

	public ComplexPowerCircuit()
    {
		PowerItems = new List<ComplexPowerItem>();
		PowerSources = new List<ComplexPowerSource>();
		PowerTriggers = new List<ComplexPowerTrigger>();
		PowerConsumers = new List<ComplexPowerConsumer>();
	}

	public ComplexPowerCircuit(List<ComplexPowerItem> items) : this()
	{
		PowerItems = items;
		foreach (ComplexPowerItem item in PowerItems)
		{
			if (item is ComplexPowerSource) { PowerSources.Add(item as ComplexPowerSource); }
			if (item is ComplexPowerTrigger) { PowerTriggers.Add(item as ComplexPowerTrigger); }
			if (item is ComplexPowerConsumer) { PowerConsumers.Add(item as ComplexPowerConsumer); }
		}
	}

	public void Update()
	{
		if (GameManager.Instance.World == null || GameManager.Instance.World.Players == null || GameManager.Instance.World.Players.Count == 0)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.gameStateManager.IsGameStarted())
		{
			this.updateTime -= Time.deltaTime;
			if (this.updateTime <= 0f)
			{
				for (int i = 0; i < this.PowerSources.Count; i++)
				{
					this.PowerSources[i].Update();
				}
				for (int j = 0; j < this.PowerTriggers.Count; j++)
				{
					this.PowerTriggers[j].CachedUpdateCall();
				}
				this.updateTime = 0.16f;
			}
			//this.saveTime -= Time.deltaTime;
			//if (this.saveTime <= 0f && (this.dataSaveThreadInfo == null || this.dataSaveThreadInfo.HasTerminated()))
			//{
			//	this.saveTime = 120f;
			//	this.SavePowerManager();
			//}
		}
		//for (int k = 0; k < this.ClientUpdateList.Count; k++)
		//{
		//	this.ClientUpdateList[k].ClientUpdate();
		//}
	}



	public void AddPowerNode(ComplexPowerItem item, ComplexPowerItem parent = null)
	{
		if(PowerItems.Contains(item)) { return; }

		PowerItems.Add(item);
		if (item is ComplexPowerSource) { PowerSources.Add(item as ComplexPowerSource); }
		if (item is ComplexPowerTrigger) { PowerTriggers.Add(item as ComplexPowerTrigger); }
		if (item is ComplexPowerConsumer) { PowerConsumers.Add(item as ComplexPowerConsumer); }

		this.SetParent(item, parent);
		this.PowerItemDictionary.Add(item.Position, item);
	}

    public void RemovePowerNode(ComplexPowerItem item)
    {
        for (int i = 0; i < item.Children.Count; i++)
        {
            this.SetParent(item.Children[i], null);
        }
        this.SetParent(item, null);
        this.PowerItems.Remove(item);
        if (item is ComplexPowerSource)
        {
            this.PowerSources.Remove((ComplexPowerSource)item);
        }
        if (item is ComplexPowerTrigger)
        {
            this.PowerTriggers.Remove((ComplexPowerTrigger)item);
        }
        if (this.PowerItemDictionary.ContainsKey(item.Position))
        {
            this.PowerItemDictionary.Remove(item.Position);
        }
    }

    public void SetParent(ComplexPowerItem child, ComplexPowerItem parent)
	{
		if (child == null) { return; }
		if (child.Parent == parent) { return; }
		if (this.CircularParentCheck(parent, child)) { return; }
		//if (child.Parent != null) { this.RemoveParent(child); }
		if (parent == null) { return; }
		if (child != null && this.PowerItems.Contains(child)) { this.PowerItems.Remove(child); }

		parent.Children.Add(child);
		child.Parent = parent;
		child.SendHasLocalChangesToRoot();
	}

	private bool CircularParentCheck(ComplexPowerItem Parent, ComplexPowerItem Child)
	{
		return Parent == Child || (Parent != null && Parent.Parent != null && this.CircularParentCheck((ComplexPowerItem)Parent.Parent, Child));
	}

	public void RemoveParent(ComplexPowerItem item)
	{
		if (item.Parent is null) { return; }

		ComplexPowerItem parent = (ComplexPowerItem)item.Parent;
		item.Parent.Children.Remove(item);
		if (item.Parent.TileEntity != null)
		{
			item.Parent.TileEntity.CreateWireDataFromPowerItem();
			item.Parent.TileEntity.DrawWires();
		}
		item.Parent = null;
		this.PowerItems.Add(item);
		parent.SendHasLocalChangesToRoot();
		item.HandleDisconnect();
	}
}
