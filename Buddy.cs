﻿using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace Buddy
{
    class BuddyPlugin : Plugin<Config>
    {
        public string VERSION = "1.1.6";

        public EventHandlers EventHandlers;

        public string BuddyMessage = "Hey! If you would like to play with a friend, type $buddyCMD <friend's name>.";

        public string BuddyMessagePrompt = "Hey! $name wants to play with you. Type $buddyAcceptCMD to accept!";

        public string buddyCommand = "buddy";

        public string buddyAcceptCommand = "baccept";

        public string buddyUnbuddyCommand = "unbuddy";

        public string alreadyHaveBuddyMessage = "You already have a buddy.";

        public string playerNotFoundMessage = "The player was not found.";

        public string buddyRequestSentMessage = "Request sent!";

        public string noBuddyRequestsMessage = "You do not have any buddy requests.";

        public string errorMessage = "An error occured.";

        public string buddyRequestAcceptMessage = "$name accepted your buddy request! Type $unBuddyCMD to get rid of your buddy.";

        public string successMessage = "Success! Type $unBuddyCMD to get rid of your buddy.";

        public string invalidUsage = "Usage: $buddyCMD <friend's name>";

        public string unBuddySuccess = "You no longer have a buddy.";

        public string useBuddyCommandBroadcast = "If you want to play on the same team as a friend, open up your console with the ~ key.";

        public string broadcastBuddy = "Your buddy is $buddy.";

        public string broadcastBuddyRequest = "$name wants to play with you. Open the console with ~ to accept their request.";

        public Dictionary<string, string> buddies = new Dictionary<string, string>();

        public Dictionary<string, Player> buddyRequests = new Dictionary<string, Player>();

        public string prefixedMessage = "";

        private Boolean shouldSetRoundStartedTrue = false;

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
            Exiled.Events.Handlers.Player.Joined -= EventHandlers.OnPlayerJoin;
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= EventHandlers.OnConsoleCommand;
            Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.OnRoundRestart;
            Log.Info("Buddy v"+VERSION+" (by PintTheDragon) has unloaded.");
        }

        public override void OnEnabled()
        {
            Config.Reload();

            if (!Config.IsEnabled)
            {
                this.OnDisabled();
                Log.Info("Disregard any further messages about the plugin being enabled. It has been disabled.");
                return;
            }
            this.prefixedMessage = this.BuddyMessage.Replace("$buddyCMD", "." + buddyCommand);
            this.invalidUsage = this.invalidUsage.Replace("$buddyCMD", "." + buddyCommand);
            this.buddyRequestAcceptMessage = this.buddyRequestAcceptMessage.Replace("$unBuddyCMD", "." + buddyUnbuddyCommand);
            this.successMessage = this.successMessage.Replace("$unBuddyCMD", "." + buddyUnbuddyCommand);
            EventHandlers = new EventHandlers(this);
            if (shouldSetRoundStartedTrue)
            {
                EventHandlers.RoundStarted = true;
                shouldSetRoundStartedTrue = false;
            }
            Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
            Exiled.Events.Handlers.Player.Joined += EventHandlers.OnPlayerJoin;
            Exiled.Events.Handlers.Server.SendingConsoleCommand += EventHandlers.OnConsoleCommand;
            Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.OnRoundRestart;
            Log.Info("Buddy v" + VERSION + " (by PintTheDragon) has loaded.");
        }

        public override void OnReloaded()
        {
            //I'm going to assume a reload happens during the middle of a game
            shouldSetRoundStartedTrue = true;
        }
    }
}
