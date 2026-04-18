using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ModCore.Models;
public class CompositeZone
{
	private List<RectangleZone> Zones = new();

	public CompositeZone()
	{
	}

	public CompositeZone(List<RectangleZone> zones)
	{
		Zones = zones;
	}

	public void AddZone(RectangleZone zone)
	{
		Zones.Add(zone);
	}

	public void ClearZones()
	{
		Zones.Clear();
	}

	public bool Contains(Player player)
	{
		foreach (var zone in Zones)
		{
			if (zone.Contains(player))
			{
				return true;
			}
		}
		return false;
	}

	public bool Contains(float3 position)
	{
		foreach (var zone in Zones)
		{
			if (zone.Contains(position))
			{
				return true;
			}
		}
		return false;
	}

	public bool Contains(Entity entity)
	{
		foreach (var zone in Zones)
		{
			if (zone.Contains(entity))
			{
				return true;
			}
		}
		return false;
	}

	// Overloaded Contains method for Player with an expansion parameter
	public bool Contains(Player player, float expansion)
	{
		foreach (var zone in Zones)
		{
			if (zone.Contains(player, expansion))
			{
				return true;
			}
		}
		return false;
	}

	// Overloaded Contains method for float3 position with an expansion parameter
	public bool Contains(float3 position, float expansion)
	{
		foreach (var zone in Zones)
		{
			if (zone.Contains(position, expansion))
			{
				return true;
			}
		}
		return false;
	}

	// Overloaded Contains method for Entity with an expansion parameter
	public bool Contains(Entity entity, float expansion)
	{
		foreach (var zone in Zones)
		{
			if (zone.Contains(entity, expansion))
			{
				return true;
			}
		}
		return false;
	}
}

