using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class ComplexPowerManager
{
	private const float UPDATE_TIME_SEC = 0.16f;

	private const float SAVE_TIME_SEC = 120f;

	public static byte FileVersion = 2;

	private static ComplexPowerManager instance;

	private List<ComplexPowerCircuit> PowerCircuits;

	private List<ComplexPowerItem> Circuits;

	private List<ComplexPowerSource> PowerSources;

	private List<ComplexPowerTrigger> PowerTriggers;

	private Dictionary<Vector3i, ComplexPowerItem> PowerItemDictionary = new Dictionary<Vector3i, ComplexPowerItem>();

	private float updateTime;

	private float saveTime = 120f;

	private ThreadManager.ThreadInfo dataSaveThreadInfo;

	public List<TileEntityPoweredBlock> ClientUpdateList = new List<TileEntityPoweredBlock>();

	public byte CurrentFileVersion { get; set; }

	public static ComplexPowerManager Instance
	{
		get
		{
			if (ComplexPowerManager.instance == null)
			{
				ComplexPowerManager.instance = new ComplexPowerManager();
			}
			return ComplexPowerManager.instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return ComplexPowerManager.instance != null;
		}
	}

	private ComplexPowerManager()
	{
		ComplexPowerManager.instance = this;
		this.Circuits = new List<ComplexPowerItem>();
		this.PowerSources = new List<ComplexPowerSource>();
		this.PowerTriggers = new List<ComplexPowerTrigger>();
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
			this.saveTime -= Time.deltaTime;
			if (this.saveTime <= 0f && (this.dataSaveThreadInfo == null || this.dataSaveThreadInfo.HasTerminated()))
			{
				this.saveTime = 120f;
				this.SavePowerManager();
			}
		}
		for (int k = 0; k < this.ClientUpdateList.Count; k++)
		{
			this.ClientUpdateList[k].ClientUpdate();
		}
	}

	private int savePowerDataThreaded(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameUtils.GetSaveGameDir(null, null), "power.dat");
		if (File.Exists(text))
		{
			File.Copy(text, string.Format("{0}/{1}", GameUtils.GetSaveGameDir(null, null), "power.dat.bak"), true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		Utils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	public void LoadPowerManager()
	{
		string path = string.Format("{0}/{1}", GameUtils.GetSaveGameDir(null, null), "power.dat");
		if (File.Exists(path))
		{
			try
			{
				using (FileStream fileStream = File.OpenRead(path))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(fileStream);
						this.Read(pooledBinaryReader);
					}
				}
			}
			catch (Exception)
			{
				path = string.Format("{0}/{1}", GameUtils.GetSaveGameDir(null, null), "power.dat.bak");
				if (File.Exists(path))
				{
					using (FileStream fileStream2 = File.OpenRead(path))
					{
						using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader2.SetBaseStream(fileStream2);
							this.Read(pooledBinaryReader2);
						}
					}
				}
			}
		}
	}

	public void SavePowerManager()
	{
		if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_powerDataSave"))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.Write(pooledBinaryWriter);
			}
			this.dataSaveThreadInfo = ThreadManager.StartThread("silent_powerDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.savePowerDataThreaded), null, System.Threading.ThreadPriority.Normal, pooledExpandableMemoryStream, null, false, false);
		}
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(ComplexPowerManager.FileVersion);
		bw.Write(this.Circuits.Count);
		for (int i = 0; i < this.Circuits.Count; i++)
		{
			bw.Write((byte)this.Circuits[i].PowerItemType);
			this.Circuits[i].write(bw);
		}
	}

	public void Read(BinaryReader br)
	{
		this.CurrentFileVersion = br.ReadByte();
		this.Circuits.Clear();
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			ComplexPowerItem ComplexPowerItem = ComplexPowerItem.CreateItem((ComplexPowerItem.PowerItemTypes)br.ReadByte());
			ComplexPowerItem.read(br, this.CurrentFileVersion);
			this.AddPowerNode(ComplexPowerItem, null);
		}
	}

	public void Cleanup()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SavePowerManager();
		}
		ComplexPowerManager.instance = null;
		this.Circuits.Clear();
		if (this.dataSaveThreadInfo != null)
		{
			this.dataSaveThreadInfo.WaitForEnd();
			this.dataSaveThreadInfo = null;
		}
	}

	public void AddPowerNode(ComplexPowerItem node, ComplexPowerItem parent = null)
	{
		this.Circuits.Add(node);
		this.SetParent(node, parent);
		if (node is ComplexPowerSource)
		{
			this.PowerSources.Add((ComplexPowerSource)node);
		}
		if (node is ComplexPowerTrigger)
		{
			this.PowerTriggers.Add((ComplexPowerTrigger)node);
		}
		this.PowerItemDictionary.Add(node.Position, node);
	}

	public void RemovePowerNode(ComplexPowerItem node)
	{
		for (int i = 0; i < node.Children.Count; i++)
		{
			this.SetParent(node.Children[i], null);
		}
		this.SetParent(node, null);
		this.Circuits.Remove(node);
		if (node is ComplexPowerSource)
		{
			this.PowerSources.Remove((ComplexPowerSource)node);
		}
		if (node is ComplexPowerTrigger)
		{
			this.PowerTriggers.Remove((ComplexPowerTrigger)node);
		}
		if (this.PowerItemDictionary.ContainsKey(node.Position))
		{
			this.PowerItemDictionary.Remove(node.Position);
		}
	}

	public void SetParent(ComplexPowerItem child, ComplexPowerItem parent)
	{
		if (child == null)
		{
			return;
		}
		if (child.Parent == parent)
		{
			return;
		}
		if (this.CircularParentCheck(parent, child))
		{
			return;
		}
		if (child.Parent != null)
		{
			this.RemoveParent(child);
		}
		if (parent == null)
		{
			return;
		}
		if (child != null && this.Circuits.Contains(child))
		{
			this.Circuits.Remove(child);
		}
		parent.Children.Add(child);
		child.Parent = parent;
		child.SendHasLocalChangesToRoot();
	}

	private bool CircularParentCheck(ComplexPowerItem Parent, ComplexPowerItem Child)
	{
		return Parent == Child || (Parent != null && Parent.Parent != null && this.CircularParentCheck((ComplexPowerItem)Parent.Parent, Child));
	}

	public void RemoveParent(ComplexPowerItem node)
	{
		if (node.Parent != null)
		{
			ComplexPowerItem parent = (ComplexPowerItem)node.Parent;
			node.Parent.Children.Remove(node);
			if (node.Parent.TileEntity != null)
			{
				node.Parent.TileEntity.CreateWireDataFromPowerItem();
				node.Parent.TileEntity.DrawWires();
			}
			node.Parent = null;
			this.Circuits.Add(node);
			parent.SendHasLocalChangesToRoot();
			node.HandleDisconnect();
		}
	}

	public void RemoveChild(ComplexPowerItem child)
	{
		child.Parent.Children.Remove(child);
		child.Parent = null;
		this.Circuits.Add(child);
	}

	public void SetParent(Vector3i childPos, Vector3i parentPos)
	{
		ComplexPowerItem powerItemByWorldPos = this.GetPowerItemByWorldPos(parentPos);
		ComplexPowerItem powerItemByWorldPos2 = this.GetPowerItemByWorldPos(childPos);
		this.SetParent(powerItemByWorldPos2, powerItemByWorldPos);
	}

	public ComplexPowerItem GetPowerItemByWorldPos(Vector3i position)
	{
		if (this.PowerItemDictionary.ContainsKey(position))
		{
			return this.PowerItemDictionary[position];
		}
		return null;
	}

	public void LogPowerManager()
	{
		for (int i = 0; i < this.PowerSources.Count; i++)
		{
			this.LogChildren(this.PowerSources[i]);
		}
	}

	private void LogChildren(ComplexPowerItem item)
	{
		try
		{
			Log.Out(string.Format("{0}{1}({2}) - Pos:{3} | Powered:{4}", new object[]
			{
				new string('\t', (int)((item.Depth > 100) ? 0 : (item.Depth + 1))),
				item.ToString(),
				item.Depth,
				item.Position,
				item.IsPowered
			}));
			for (int i = 0; i < item.Children.Count; i++)
			{
				this.LogChildren(item.Children[i]);
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}
}
