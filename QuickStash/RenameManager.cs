using ModCore;
using ModCore.Events;
using ModCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using static ProjectM.Network.InteractEvents_Client;

namespace QuickStash
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
            if (!eventEntity.Exists()) return;

            if (renameInteractable.NewName.ToString().EndsWith("99"))
            {
                var newName = renameInteractable.NewName.ToString();
                newName = newName.Substring(0, newName.Length - 2) + "*";
                renameInteractable.NewName = newName;
                eventEntity.Write(renameInteractable);
            } 
            else if (renameInteractable.NewName.ToString().EndsWith("88"))
            {
                var newName = renameInteractable.NewName.ToString();
                newName = newName.Substring(0, newName.Length - 2) + "-";
                renameInteractable.NewName = newName;
                eventEntity.Write(renameInteractable);
            }
        }
    }
}
