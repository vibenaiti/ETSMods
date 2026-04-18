using ProjectM.Shared;
using ProjectM;
using ModCore.Factories;
using ModCore.Models;
using Unity.Entities;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ProjectM.Network;
using static ProjectM.Network.InteractEvents_Client;
using Stunlock.Core;
using ProjectM.Terrain;
using ProjectM.CastleBuilding;
using static ModCore.Events.GameEvents;

namespace ModCore.Events;
public static class GameEvents
{
	public delegate void PlayerFirstSpawnHandler(Player player);
	public static PlayerFirstSpawnHandler OnPlayerFirstSpawn;

	public delegate void PlayerRespawnHandler(Player player);
	public static PlayerRespawnHandler OnPlayerRespawn;

	public delegate void PlayerUnstuckHandler(Player player, Entity eventEntity);
	public static PlayerUnstuckHandler OnPlayerUnstuck;

	public delegate void PlayerDeathHandler(Player player, DeathEvent deathEvent);
	public static PlayerDeathHandler OnPlayerDeath;

	public delegate void PlayerDownedHandler(Player player, Entity killer);
	public static PlayerDownedHandler OnPlayerDowned;

	public delegate void PlayerShapeshiftHandler(Player player, Entity eventEntity);
	public static PlayerShapeshiftHandler OnPlayerShapeshift;

	public delegate void PlayerResetHandler(Player player);
	public static PlayerResetHandler OnPlayerReset;

	public delegate void PlayerChatCommandHandler(Player player, CommandAttribute command);
	public static PlayerChatCommandHandler OnPlayerChatCommand;

	public delegate void PlayerUsedConsumableHandler(Player player, Entity eventEntity, InventoryBuffer item);
	public static PlayerUsedConsumableHandler OnPlayerUsedConsumable;

	public delegate void PlayerBuffedHandler(Player player, Entity buffEntity, PrefabGUID prefabGUID);
	public static PlayerBuffedHandler OnPlayerBuffed;

	public delegate void PlayerBuffRemovedHandler(Player player, Entity buffEntity, PrefabGUID prefabGUID);
	public static PlayerBuffRemovedHandler OnPlayerBuffRemoved;

	public delegate void UnitBuffedHandler(Entity unit, Entity buffEntity);
	public static UnitBuffedHandler OnUnitBuffed;

	public delegate void UnitBuffRemovedHandler(Entity unit, Entity buffEntity);
	public static UnitBuffRemovedHandler OnUnitBuffRemoved;

	public delegate void PlayerWillLoseGallopBuffHandler(Player player, Entity eventEntity);
	public static PlayerWillLoseGallopBuffHandler OnPlayerWillLoseGallopBuff;

	public delegate void PlayerMountedHandler(Player player, Entity eventEntity);
	public static PlayerMountedHandler OnPlayerMounted;

	public delegate void PlayerDismountedHandler(Player player, Entity eventEntity);
	public static PlayerDismountedHandler OnPlayerDismounted;

	public delegate void PlayerStartedCastingHandler(Player player, Entity eventEntity);
	public static PlayerStartedCastingHandler OnPlayerStartedCasting;

	public delegate void PlayerStartedCharacterCrafting(Player player, Entity eventEntity, StartCharacterCraftItemEvent startCharacterCraftItemEvent);
	public static PlayerStartedCharacterCrafting OnPlayerStartedCharacterCrafting;

	public delegate void PlayerStartedCrafting(Player player, Entity eventEntity, StartCraftItemEvent startCraftItemEvent);
	public static PlayerStartedCrafting OnPlayerStartedCrafting;

	public delegate void PlayerConnectedHandler(Player player);
	public static PlayerConnectedHandler OnPlayerConnected;

	public delegate void PlayerDisconnectedHandler(Player player);
	public static PlayerDisconnectedHandler OnPlayerDisconnected;

	public delegate void PlayerInvitedToClanHandler(Player player, Entity eventEntity);
	public static PlayerInvitedToClanHandler OnPlayerInvitedToClan;

	public delegate void PlayerRespondedToClanInviteHandler(Player player, Entity eventEntity, ClanEvents_Client.ClanInviteResponse clanInviteResponse);
	public static PlayerRespondedToClanInviteHandler OnPlayerRespondedToClanInvite;

	public delegate void PlayerKickedFromClanHandler(Player player, Entity eventEntity, ClanEvents_Client.Kick_Request clanKickEvent);
	public static PlayerKickedFromClanHandler OnPlayerKickedFromClan;

	public delegate void PlayerLeftClanHandler(Player player, Entity eventEntity, ClanEvents_Client.LeaveClan leaveClanEvent);
	public static PlayerLeftClanHandler OnPlayerLeftClan;

	public delegate void PlayerCreatedClanHandler(Player player, Entity eventEntity, ClanEvents_Client.CreateClan_Request createClanEvent);
	public static PlayerCreatedClanHandler OnPlayerCreatedClan;

	public delegate void UnitDeathHandler(Entity unit, DeathEvent deathEvent);
	public static UnitDeathHandler OnUnitDeath;

	public delegate void ItemWasDroppedHandler(Player player, Entity eventEntity, PrefabGUID itemType, int slotIndex);
	public static ItemWasDroppedHandler OnItemWasDropped;

	public delegate void ItemWasPickedUpHandler(Player player, Entity eventEntity, PrefabGUID itemType, int slotIndex);
	public static ItemWasPickedUpHandler OnItemWasPickedUp;

	public delegate void PlayerDamageDealtHandler(Player player, Entity eventEntity, DealDamageEvent dealDamageEvent);
	public static PlayerDamageDealtHandler OnPlayerDamageDealt;

	public delegate void UnitDamageDealtHandler(Entity unit, Entity eventEntity, DealDamageEvent dealDamageEvent);
	public static UnitDamageDealtHandler OnUnitDamageDealt;

	public delegate void PlayerHealthChanged(Entity source, Entity eventEntity, StatChangeEvent statChangeEvent, Player target, PrefabGUID ability);
	public static PlayerHealthChanged OnPlayerHealthChanged;

	public delegate void UnitHealthChanged(Entity source, Entity eventEntity, StatChangeEvent statChangeEvent, Entity target, PrefabGUID ability);
	public static UnitHealthChanged OnUnitHealthChanged;

	public delegate void PlayerProjectileCreatedHandler(Player player, Entity projectile);
	public static PlayerProjectileCreatedHandler OnPlayerProjectileCreated;

	public delegate void UnitProjectileCreatedHandler(Entity unit, Entity projectile);
	public static UnitProjectileCreatedHandler OnUnitProjectileCreated;

	public delegate void PlayerAoeCreatedHandler(Player player, Entity aoe);
	public static PlayerAoeCreatedHandler OnPlayerAoeCreated;

	public delegate void UnitAoeCreatedHandler(Entity unit, Entity aoe);
	public static UnitAoeCreatedHandler OnUnitAoeCreated;

	public delegate void PlayerChatMessageHandler(Player player, Entity eventEntity, ChatMessageEvent chatMessageEvent);
	public static PlayerChatMessageHandler OnPlayerChatMessage;

	public delegate void DelayedSpawnEventHandler(Unit unit);
	public static DelayedSpawnEventHandler OnDelayedUnitSpawn;
	
	public delegate void SpawnEventHandler(Unit unit);
	public static SpawnEventHandler OnUnitSpawn;

	public delegate void PlayerSpawningHandler(Player player, SpawnCharacter spawnCharacter);
	public static PlayerSpawningHandler OnPlayerSpawning;

    public delegate void PlayerHasNoControlledEntityHandler(Player player);
    public static PlayerHasNoControlledEntityHandler OnPlayerHasNoControlledEntity;

	public delegate void PlayerHitColliderCastCreated(Player player, Entity hitCastCollider);
	public static PlayerHitColliderCastCreated OnPlayerHitColliderCastCreated;

	public delegate void UnitHitCastColliderCreated(Entity unit, Entity hitCastCollider);
	public static UnitHitCastColliderCreated OnUnitHitCastColliderCreated;

	public delegate void PlayerHitColliderCastUpdate(Player player, Entity hitCastCollider);
	public static PlayerHitColliderCastUpdate OnPlayerHitColliderCastUpdate;

	public delegate void UnitHitCastColliderUpdate(Entity unit, Entity hitCastCollider);
	public static UnitHitCastColliderUpdate OnUnitHitCastColliderUpdate;

	public delegate void PlayerPlacedStructure(Player player, Entity eventEntity, BuildTileModelEvent buildTileModelEvent);
	public static PlayerPlacedStructure OnPlayerPlacedStructure;

	public delegate void PlayerInteractedWithPrisoner(Player player, Entity eventEntity, InteractWithPrisonerEvent interactWithPrisonerEvent);
	public static PlayerInteractedWithPrisoner OnPlayerInteractedWithPrisoner;

	public delegate void PlayerPurchasedItem(Player player, Entity eventEntity, TraderPurchaseEvent traderPurchaseEvent);
	public static PlayerPurchasedItem OnPlayerPurchasedItem;

	public delegate void GameFrameUpdateHandler();
	public static GameFrameUpdateHandler OnGameFrameUpdate;

	public delegate void AggroPostUpdateHandler(Entity entity);
	public static AggroPostUpdateHandler OnAggroPostUpdate;

	public delegate void ClanStatusPostUpdateHandler();
	public static ClanStatusPostUpdateHandler OnClanStatusPostUpdate;

	public delegate void PlayerMapIconPostUpdateHandler(Player player, Entity mapIconEntity);
	public static PlayerMapIconPostUpdateHandler OnPlayerMapIconPostUpdate;

	public delegate void PlayerInteractedHandler(Player player, Interactor interactor);
	public static PlayerInteractedHandler OnPlayerInteracted;

	public delegate void PlayerFinishedCasting(Player player, AbilityPostCastFinishedEvent abilityPostCastFinishedEvent);
	public static PlayerFinishedCasting OnPlayerFinishedCasting;

	public delegate void ServerStarted();
	public static ServerStarted OnServerStart;

	public delegate void PlayerSignedUp(Player player);
	public static PlayerSignedUp OnPlayerSignedUp;

	public delegate void PlayerSpecialChat(Player player, string message);
	public static PlayerSpecialChat OnPlayerSpecialChat;

	public delegate void PlayerRequestedLeave(Player player);
	public static PlayerRequestedLeave OnPlayerRequestedLeave;

	public delegate void PlayerRenamedEntity(Player player, Entity eventEntity, RenameInteractable renameInteractable);
	public static PlayerRenamedEntity OnPlayerRenamedEntity;

	public delegate void PlayerTransferredItem(Player player, Entity eventEntity, MoveItemBetweenInventoriesEvent moveItemBetweenInventoriesEvent);
	public static PlayerTransferredItem OnPlayerTransferredItem;

	public delegate void PlayerTransferredAllItems(Player player, Entity eventEntity, MoveAllItemsBetweenInventoriesEvent moveItemBetweenInventoriesEvent);
	public static PlayerTransferredAllItems OnPlayerTransferredAllItems;

	public delegate void PlayerTransferredAllItemsV2(Player player, Entity eventEntity, MoveAllItemsBetweenInventoriesEventV2 moveAllItemsBetweenInventoriesEventV2);
	public static PlayerTransferredAllItemsV2 OnPlayerTransferredAllItemsV2;

	public delegate void PlayerSmartMergedItems(Player player, Entity eventEntity, SmartMergeItemsBetweenInventoriesEvent smartMergeItemsBetweenInventoriesEvent);
	public static PlayerSmartMergedItems OnPlayerSmartMergedItems;

	public delegate void PlayerSetMapMarker(Player player, Entity eventEntity, SetMapMarkerEvent setMapMarkerEvent);
	public static PlayerSetMapMarker OnPlayerSetMapMarker;

	public delegate void PlayerRemovedPvpProtection(Player player, Entity eventEntity);
	public static PlayerRemovedPvpProtection OnPlayerRemovedPvpProtection;

	public delegate void PlayerInteractedWithCastleHeart(Player player, Entity eventEntity, CastleHeartInteractEvent castleHeartInteractEvent);
	public static PlayerInteractedWithCastleHeart OnPlayerInteractedWithCastleHeart;

	public delegate void PlayerEnteredRegion(Player player, Entity eventEntity, CurrentWorldRegionChangedEvent currentWorldRegionChangedEvent);
	public static PlayerEnteredRegion OnPlayerEnteredRegion;

	public delegate void PlayerUnlockedResearch(Player player, Entity eventEntity, UnlockResearchEvent unlockResearchEvent);
	public static PlayerUnlockedResearch OnPlayerUnlockedResearch;

	public delegate void PlayerSharedAllPassives(Player player, Entity eventEntity, ShareAllSpellSchoolPassives shareAllSpellSchoolPassives);
	public static PlayerSharedAllPassives OnPlayerSharedAllPassives;

	public delegate void CastleHeartChangedState(Entity eventEntity, CastleHeartEvent castleHeartEvent);
	public static CastleHeartChangedState OnCastleHeartChangedState;

	public delegate void PlayerEquippedItem(Player player, Entity eventEntity, EquipItemEvent equipItemEvent);
	public static PlayerEquippedItem OnPlayerEquippedItem;

	public delegate void PlayerEquippedItemFromInventory(Player player, Entity eventEntity, EquipItemFromInventoryEvent equipItemFromInventoryEvent);
	public static PlayerEquippedItemFromInventory OnPlayerEquippedItemFromInventory;

	public delegate void PlayerUnequippedItem(Player player, Entity eventEntity, UnequipItemEvent unequipItemEvent);
	public static PlayerUnequippedItem OnPlayerUnequippedItem;

	public delegate void PlayerTransferredEquipmentToEquipment(Player player, Entity eventEntity, EquipmentToEquipmentTransferEvent equipmentToEquipmentTransferEvent);
	public static PlayerTransferredEquipmentToEquipment OnPlayerTransferredEquipmentToEquipment;

	public delegate void PlayerUsedWaygate(Player player, Entity eventEntity, TeleportEvents_ToServer.TeleportToWaypointEvent teleportToWaypointEvent);
	public static PlayerUsedWaygate OnPlayerUsedWaygate;

	public delegate void PlayerInventoryChanged(Player player, Entity eventEntity, InventoryChangedEvent inventoryChangedEvent);
	public static PlayerInventoryChanged OnPlayerInventoryChanged;

	public delegate void PlayerEquipmentChanged(Player player, Entity eventEntity, EquipmentChangedEvent equipmentChangedEvent);
	public static PlayerEquipmentChanged OnPlayerEquipmentChanged;

	/*public delegate void PlayerStoppedInteractingWithObject(Player player, Entity eventEntity, StopInteractingWithObjectEvent stopInteractingWithObjectEvent);
	public static PlayerStoppedInteractingWithObject OnPlayerStoppedInteractingWithObject;*/
}
