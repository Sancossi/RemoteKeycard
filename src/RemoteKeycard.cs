using Exiled.API.Features;
using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;

namespace RemoteKeycard
{
    public sealed class RKConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        // Just make an empty space as []
        public List<ItemType> Cards { get; set; } = new List<ItemType>();
    }

    public sealed class RemoteKeycard : Plugin<RKConfig>
    {
        internal static RemoteKeycard instance;

        private readonly LogicHandler _logicHandler = new LogicHandler();

        public override string Name => nameof(RemoteKeycard);
        public override string Author => "iRebbok";
        public override Version Version { get; }

        public RemoteKeycard()
        {
            // A simple way to dynamic versioning
            Version = typeof(RemoteKeycard).Assembly.GetName().Version;
            instance = this;
        }

        public override void OnDisabled() =>
            Exiled.Events.Handlers.Player.InteractingDoor -= _logicHandler.OnDoorAccess;

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.InteractingDoor += _logicHandler.OnDoorAccess;
#if DEBUG
            Log.Debug($"Allowed items for processing: {(Config.Cards?.Count > 0 ? string.Join(", ", Config.Cards) : "null")}");
#endif
        }
    }
}
