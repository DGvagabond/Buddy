using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Buddy
{
    class Buddy : Plugin<Config>
    {
        public override PluginPriority Priority => PluginPriority.Highest;
        public override string Author => "PintTheDragon";
        public override string Name => "Buddy";
        public override Version Version => new Version(1,3,0,0);
        public override Version RequiredExiledVersion => new Version(2,9,4);

        public EventHandlers EventHandlers;
        public Dictionary<string, string> Buddies = new Dictionary<string, string>();
        public Dictionary<string, List<Player>> BuddyRequests = new Dictionary<string, List<Player>>();
        public static Buddy Singleton;

        public override void OnDisabled()
        {
            UnregisterEvents();

            base.OnDisabled();
        }

        public override void OnEnabled()
        {
            Singleton = this;

            Config.OnReload();

            RegisterEvents();

            base.OnEnabled();
        }

        public override void OnReloaded()
        {
            Config.OnReload();

            base.OnReloaded();
        }

        public void RemovePerson(string userID)
        {
            try
            {
                foreach (var item in Buddies.Where(x => x.Value == userID).ToList())
                {
                    try
                    {
                        Buddies.Remove(item.Key);
                    }
                    catch (ArgumentException) { }
                }
            }
            catch (ArgumentException) { }
        }

        private void RegisterEvents()
        {
            EventHandlers = new EventHandlers();
            Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
            Exiled.Events.Handlers.Player.Joined += EventHandlers.OnPlayerJoin;
            Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.OnRoundRestart;
            Exiled.Events.Handlers.Server.ReloadedConfigs += Config.OnReload;
        }
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
            Exiled.Events.Handlers.Player.Joined -= EventHandlers.OnPlayerJoin;
            Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.OnRoundRestart;
            Exiled.Events.Handlers.Server.ReloadedConfigs -= Config.OnReload;
            
            EventHandlers = null;
        }
    }
}
