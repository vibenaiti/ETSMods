using ProjectM;
using ProjectM.Network;
using ModCore.Helpers;
using ModCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;
using System.Net.WebSockets;

namespace ModCore.Services
{
	public static class PlayerService
    {
		public static readonly Dictionary<Entity, Player> UserCache = new Dictionary<Entity, Player>();
		public static readonly Dictionary<Entity, Player> CharacterCache = new Dictionary<Entity, Player>();
		public static readonly Dictionary<ulong, Player> SteamIdCache = new Dictionary<ulong, Player>();
		public static readonly HashSet<Player> OnlinePlayersWithUsers = new();
		public static readonly HashSet<Player> OnlinePlayersWithCharacters = new();
		public static Action OnOnlinePlayerAmountChanged;
		public static bool Initialized = false;

		public abstract class PlayerData
		{
			public abstract ulong SteamID { get; set; }
		}

		public static void Initialize()
		{
			LoadAllPlayers();
			Initialized = true;
		}

		public static void Dispose()
		{
			UserCache.Clear();
			CharacterCache.Clear();
			SteamIdCache.Clear();
			OnlinePlayersWithUsers.Clear();
			OnlinePlayersWithCharacters.Clear();
			Initialized = false;
		}

		public static Player GetAnyPlayer()
		{
			if (OnlinePlayersWithUsers.Count > 0)
			{
				return OnlinePlayersWithUsers.FirstOrDefault();
			}
			else
			{
				return UserCache.Values.FirstOrDefault();
			}
		}

		public static void LoadAllPlayers()
		{
			var users = Helper.GetEntitiesByComponentTypes<User>(EntityQueryOptions.IncludeDisabled);
			foreach (var user in users)
			{
				try
				{
					var userData = user.Read<User>();
					var player = GetPlayerFromUser(user); //fill cache
					if (player.IsOnline)
					{
						OnlinePlayersWithUsers.Add(player);
						if (player.Character.Exists())
						{
							OnlinePlayersWithCharacters.Add(player);
						}
					}
				}
				catch (Exception e)
				{

				}
			}
			OnOnlinePlayerAmountChanged?.Invoke();
		}

		public static Player GetPlayerFromUser(Entity User)
		{
			Player player;
			if (UserCache.ContainsKey(User))
			{
				player = UserCache[User];
			}
			else
			{
				if (User.Exists())
				{
					player = new Player
					{
						User = User
					};
					UserCache[User] = player;
					if (player.Character.Exists())
					{
						CharacterCache[player.Character] = player;
					}
					SteamIdCache[player.SteamID] = player;
				}
				else
				{
					throw new Exception("Tried to create a player from a non-existent user");
				}
			}
			return player;
		}

		public static Player GetPlayerFromCharacter(Entity Character)
		{
			Player player;
			if (CharacterCache.ContainsKey(Character))
			{
				player = CharacterCache[Character];
			}
			else
			{
				player = new Player
				{
					Character = Character
				};
				if (Character.Exists())
				{
					CharacterCache[Character] = player;
				}
				UserCache[player.User] = player;
				SteamIdCache[player.SteamID] = player;
			}
			
			return player;
		}

		public static Player GetPlayerFromSteamId(ulong SteamId)
		{
			Player player;
			if (SteamIdCache.ContainsKey(SteamId))
			{
				player = SteamIdCache[SteamId];
			}
			else
			{
				TryGetPlayerFromString(SteamId.ToString(), out player);
			}
			if (player == null)
			{
				player = new Player();
				player.SteamID = SteamId;
			}
			return player;
		}

		public static bool TryGetPlayerFromUserIndex(int userIndex, out Player player)
		{
			var singleton = SingletonAccessor<UserInfoBufferSingleton>.Create(VWorld.Server.EntityManager);
			if (UserInfoUtility.TryGetInfoForUserWithUserIndex(VWorld.Server.EntityManager, singleton, userIndex, out var result))
			{
				player = GetPlayerFromSteamId(result.PlatformId);
				return true;
			}
			player = null;
			return false;
		}

		public static bool TryGetPlayerFromString(string input, out Player player)
		{
			if (ulong.TryParse(input, out ulong platformID))
			{
				return SteamIdCache.TryGetValue(platformID, out player);
			}
			else
			{
				var userEntities = Helper.GetEntitiesByComponentTypes<User>(EntityQueryOptions.IncludeDisabled);
				foreach (var userEntity in userEntities)
				{
					var user = userEntity.Read<User>();
					if ((user.PlatformId == platformID) || user.CharacterName.ToString().ToLower() == input.ToLower())
					{
						if (UserCache.TryGetValue(userEntity, out player))
						{
							return true;
						}
						else
						{
							player = new Player
							{
								User = userEntity,
							};
							UserCache[userEntity] = player;
							if (player.Character.Exists())
							{
								CharacterCache[player.Character] = player;
							}
							SteamIdCache[player.SteamID] = player;
							return true;
						}
					}
				}
			}

			player = default;
			return false;
		}
	}
}
