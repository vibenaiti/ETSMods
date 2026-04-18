using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cpp2IL.Core.Extensions;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ModCore.Data;
using ModCore.Models;
using ModCore.Services;
using ModCore;
using ModCore.Helpers;
using ProjectM.Terrain;
using Stunlock.Core;

namespace ModCore.Models;

public class Player
{
	private Entity _user;
	private Entity _character;
	private ulong _steamID;

	public Entity User
	{
		get 
		{ 
			if (!_user.Exists())
			{
				RetrieveUserFromSteamId();
			}

			return _user;
		} 
		set { SetUser(value); }
	}

	public Entity Character
	{
		get
		{
			if (!_character.Exists())
			{
				RetrieveCharacterFromUser();
			}

			return _character;
		}
		set { SetCharacter(value); }
	}

	public ulong SteamID
	{
		get => _steamID == default && _user != default ? _user.Read<User>().PlatformId : _steamID;
		set => _steamID = value;
	}

	public Entity Clan => GetClan();

	public string Name => GetName();
	public string FullName => GetFullName();
	public string FullNameColored => GetFullNameColored();
	public int Level => GetLevel();
	public int Height => GetHeight();
	public int MaxLevel => GetMaxCachedLevel();
	public bool IsAdmin => GetIsAdmin();
	public bool IsOnline => GetIsOnline();
	public bool IsAlive => GetIsAlive();
	public Entity Inventory => GetInventory();
	public Equipment Equipment => GetEquipment();
	public List<Entity> EquipmentEntities => GetEquipmentEntities();
	public Entity ControlledEntity => GetControlledEntity();

	public int MatchmakingTeam { get; set; }

	public float3 Position => GetPosition();
	public float3 AimPosition => GetAimPosition();
	public int2 TilePosition => GetTilePosition();
	public Team Team => GetTeam();

	public WorldRegionType WorldZone => GetWorldZone();
	public string WorldZoneString => GetWorldZoneString();

	private void SetUser(Entity user)
	{
		if (_user != user)
		{
			if (!user.Exists())
			{
				throw new Exception("Invalid User");
			}

			_user = user;

			_steamID = _user.Read<User>().PlatformId;
			if (!_character.Exists())
			{
				_character = _user.Read<User>().LocalCharacter._Entity;
			}
		}
	}

	private void SetCharacter(Entity character)
	{
		if (character.Exists())
		{
			_character = character;
			if (!_user.Exists())
			{
				if (_character.Read<PlayerCharacter>().UserEntity.Exists())
				{
					_user = _character.Read<PlayerCharacter>().UserEntity;
					_steamID = _user.Read<User>().PlatformId;
				}
				else
				{
					throw new Exception("Tried to load a player without a valid user");
				}
			}
		}
	}

	private void RetrieveUserFromSteamId()
	{
		var entities = Helper.GetEntitiesByComponentTypes<User>();
		foreach (var entity in entities)
		{
			var user = entity.Read<User>();
			if (user.PlatformId == _steamID)
			{
				_user = entity;
				PlayerService.UserCache[_user] = this;
				return;
			}
		}
	}

	private void RetrieveCharacterFromUser()
	{
		if (_user.Exists())
		{
			var userComponent = _user.Read<User>();
			if (userComponent.LocalCharacter._Entity.Exists())
			{
				_character = userComponent.LocalCharacter._Entity;
				PlayerService.CharacterCache[_character] = this;
			}
			else
			{
				_character = default;
			}
		}
	}

	private string GetName()
	{
		var userData = User.Read<User>();
		var name = userData.CharacterName.ToString();
		if (name == "")
		{
			name = $"[No Character - {userData.PlatformId}]";
		}
		return name;
	}

	public string GetFullName()
	{
		var playerCharacter = Character.Read<PlayerCharacter>();
		if (!playerCharacter.SmartClanName.IsEmpty)
		{
			return $"{playerCharacter.SmartClanName} {Name}";
		}
		else
		{
			return Name;
		}
	}

	public string GetFullNameColored()
	{
		return GetFullName().Colorify(ExtendedColor.ClanNameColor);
	}

	private int GetHeight()
	{
		return Character.Read<TilePosition>().HeightLevel;
	}
	private int GetLevel()
	{
		return (int)Character.Read<Equipment>().GetFullLevel();
	}

	private int GetMaxCachedLevel()
	{
		var level = Level;
		Globals.PlayerToMaxLevel.TryGetValue(this, out level);
		return (int)level;
	}

	private Entity GetClan()
	{
		return User.Read<User>().ClanEntity._Entity;
	}

	private Team GetTeam()
	{
		return User.Read<Team>();
	}

	private bool GetIsAdmin()
	{
		return User.Read<User>().IsAdmin || Core.adminAuthSystem._LocalAdminList.Contains(SteamID);
	}

	private bool GetIsOnline()
	{
		if (User.Exists())
		{
			return User.Read<User>().IsConnected;
		}
		else
		{
			return false;
		}
	}
	private float3 GetAimPosition()
	{
		return User.Read<EntityInput>().AimPosition;
	}

	private float3 GetPosition()
	{
		return Character.Read<Translation>().Value;
	}

	private int2 GetTilePosition()
	{
		if (Character.Has<TilePosition>())
		{
			return Character.Read<TilePosition>().Tile;
		}
		else
		{
			return new int2(0, 0);
		}
	}


	private bool GetIsAlive()
	{
		return !Character.Read<Health>().IsDead && !Helper.HasBuff(this, Prefabs.Buff_General_Vampire_Wounded_Buff);
	}

	private Entity GetInventory()
	{
		return Character.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
	}
	
	private Equipment GetEquipment()
	{
		return Character.Read<Equipment>();
	}

	private List<Entity> GetEquipmentEntities()
	{
		var equipmentEntities = new NativeList<Entity>(Allocator.Temp);
		var equipment = Equipment;
		equipment.GetAllEquipmentEntities(equipmentEntities, true);
		var results = new List<Entity>();
		foreach (var equipmentEntity in equipmentEntities)
		{
			results.Add(equipmentEntity);
		}
		if (equipment.BagSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.BagSlot.SlotEntity._Entity);
		}
		if (equipment.ChestCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.ChestCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.CloakCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.CloakCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.FootgearCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.FootgearCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.GlovesCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.GlovesCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.LegsCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.LegsCosmeticSlot.SlotEntity._Entity);
		}
		return results;
	}
	
	public WorldRegionType GetWorldZone()
	{
		return User.Read<CurrentWorldRegion>().CurrentRegion;
	}

	public string GetWorldZoneString()
	{
		var region = User.Read<CurrentWorldRegion>().CurrentRegion;
		if (WorldRegionData.WorldRegionToString.TryGetValue(region, out var zoneName))
		{
			return zoneName;
		}
		return "";
	}

	public Entity GetControlledEntity()
	{
		return User.Read<Controller>().Controlled._Entity;
	}

	public void ReceiveMessage(object message, bool messageSpectators = false)
	{
		if (IsOnline)
		{
			var fixedMsg = new Unity.Collections.FixedString512Bytes(message.ToString());
			ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, User.Read<User>(), ref fixedMsg);
		}
	}

	public FromCharacter ToFromCharacter()
	{
		return new FromCharacter
		{
			Character = this.Character,
			User = this.User
		};
	}

	public List<Player> GetClanMembers()
	{
		List<Player> clanPlayers = new List<Player>();
		if (Clan.Exists())
		{
			NativeList<Entity> entities = new NativeList<Entity>(Allocator.Temp);
			TeamUtility.GetClanMembers(VWorld.Server.EntityManager, Clan, entities);
			foreach (var entity in entities)
			{
				if (entity.Has<User>())
				{
					Player player = PlayerService.GetPlayerFromUser(entity);
					clanPlayers.Add(player);
				}
			}
		}
		else
		{
			clanPlayers.Add(this);
		}

		return clanPlayers;
	}

	public bool IsAlliedWith(Player player)
	{
		return Team.IsAllies(Character.Read<Team>(), player.Character.Read<Team>());
	}

	public bool HasControlledEntity()
	{
		if (ControlledEntity == Character)
		{
			return true;
		}
		else
		{
			bool isDead;
			if (ControlledEntity.Has<Health>())
			{
				isDead = ControlledEntity.Read<Health>().IsDead;
			}
			else
			{
				isDead = true;
			}

			if (isDead)
			{
				return false;
			}

			return ((ControlledEntity.Exists() && ControlledEntity.Has<PrefabGUID>()));
		}
	}

	public override int GetHashCode()
	{
		return SteamID.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		Player other = (Player)obj;
		return SteamID == other.SteamID;
	}

	public override string ToString()
	{
		return Name;
	}
}
