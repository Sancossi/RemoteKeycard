using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System;
using System.Collections.Generic;

namespace RemoteKeycard
{
    internal sealed class LogicHandler
    {
        // Allowed items for remote access
        private List<ItemType> AllowedTypes => RemoteKeycard.instance.Config.Cards;
        private Item[] _cache;

        public void OnDoorAccess(InteractingDoorEventArgs ev)
        {
#if DEBUG
            var nickName = ev.Player.Nickname ?? "Null";
            var userId = ev.Player.UserId ?? "Null";
            Log.Debug($"Player {nickName} ({userId}) is trying to open the door");
#pragma warning disable CS0618 // Type or member is obsolete
            Log.Debug($"Door permission: {(string.IsNullOrEmpty(ev.Door.permissionLevel) ? "None" : ev.Door.permissionLevel)}");
#pragma warning restore CS0618 // Type or member is obsolete
#endif
            if (ev.IsAllowed || ev.Door.destroyed || ev.Door.locked)
#if DEBUG
            {
                Log.Debug($"Door is locked or destroyed or the player {nickName} ({userId}) has access to open it");
                return;
            }
            else
                Log.Debug("Further processing allowed...");
#else
                return;
#endif

            var playerIntentory = ev.Player.Inventory.items;

#if DEBUG
            Log.Debug($"Player inventory is null: {playerIntentory == null}");
#endif

            foreach (var item in playerIntentory)
            {
#if DEBUG
                Log.Debug($"Processing an item in the player’s inventory: {item.id} ({(int)item.id})");
#endif

                if (AllowedTypes?.Count > 0 && !AllowedTypes.Contains(item.id))
                    continue;

                var gameItem = Array.Find(GetItems(), i => i.id == item.id);

#if DEBUG
                Log.Debug($"Game item is null: {gameItem == null}");
                Log.Debug($"Game item processing: C {gameItem.itemCategory} ({(int)gameItem.itemCategory}) | T {item.id} ({(int)item.id}) | P {string.Join(", ", gameItem.permissions)}");
#endif

                // Relevant for items whose type was not found
                if (gameItem == null)
                    continue;

                if (gameItem.permissions == null || gameItem.permissions.Length == 0)
                    continue;

                foreach (var itemPerm in gameItem.permissions)
                    if (ev.Door.backwardsCompatPermissions.TryGetValue(itemPerm, out var flag) && ev.Door.PermissionLevels.HasPermission(flag))
                    {
#if DEBUG
                        Log.Debug($"Item has successfully passed permission validation: {gameItem.id} ({(int)gameItem.id})");
#endif
                        ev.IsAllowed = true;
                        continue;
                    }
            }
        }

        public Item[] GetItems()
        {
#pragma warning disable IDE0074 // Use compound assignment
            return _cache ?? (_cache = UnityEngine.Object.FindObjectOfType<Inventory>().availableItems);
#pragma warning restore IDE0074 // Use compound assignment
        }
    }
}
