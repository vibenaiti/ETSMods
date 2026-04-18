using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ModCore.Events;
using ModCore.Listeners;
using ModCore.Models;
using ModCore;
using ModCore.Helpers;
using ModCore.Data;
using ModCore.Services;
using System.Threading;
using ProjectM.Gameplay.Systems;
using static ModCore.Helpers.Helper;
using ModCore.Factories;
using ProjectM.Scripting;
using static ProjectM.Network.InteractEvents_Client;
using Unity.Collections;

namespace DebugMod.Managers
{
    public static class RenameManager
    {
        public static void Initialize()
        {
            GameEvents.OnPlayerRenamedEntity += HandleOnPlayerRenamedEntity;
        }

        public static void Dispose()
        {
            GameEvents.OnPlayerRenamedEntity -= HandleOnPlayerRenamedEntity;
        }

        public static void HandleOnPlayerRenamedEntity(Player player, Entity eventEntity, RenameInteractable renameInteractable)
        {
            if (TryGetEntityFromNetworkId(renameInteractable.InteractableId, out var renamedEntity))
            {
                //can add team validation later
                if (renamedEntity.Has<CanFly>() && !player.IsAdmin)
                {
                    player.ReceiveMessage("You are not allowed to rename this!".Error());
                    eventEntity.Destroy();
                }
            }
        }
    }
}
