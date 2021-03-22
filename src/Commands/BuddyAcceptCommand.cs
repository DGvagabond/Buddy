using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;

namespace Buddy
{
    [CommandHandler(typeof(ClientCommandHandler))]
    class BuddyAcceptCommand : ICommand
    {
        public string Command => Buddy.Singleton.Config.GetLang("buddyAcceptCommand");

        public string[] Aliases => null;

        public string Description => "A command to accept a pending buddy request.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "";
            if (!(sender is PlayerCommandSender)) return true;
            var player = Player.Get(((CommandSender)sender).SenderId);
            response = HandleBuddyAcceptCommand(player, arguments.ToArray());
            return true;
        }

        private string HandleBuddyAcceptCommand(Player p, string[] args)
        {
            //checks
            if (!Buddy.Singleton.BuddyRequests.ContainsKey(p.UserId))
            {
                return Buddy.Singleton.Config.GetLang("noBuddyRequestsMessage");
            }

            //set the buddy
            Player buddy = null;
            try
            {
                if (!Buddy.Singleton.BuddyRequests.TryGetValue(p.UserId, out List<Player> buddies) || buddies == null) return Buddy.Singleton.Config.GetLang("errorMessage");
                if (args.Length != 1) buddy = buddies.Last();
                else
                {
                    var lower = args[0].ToLower();
                    foreach (var player in buddies.Where(player => player != null).Where(player => player.Nickname.ToLower().Contains(lower) && player.UserId != p.UserId))
                    {
                        buddy = player;
                        break;
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e.ToString());
                return Buddy.Singleton.Config.GetLang("errorMessage");
            }
            if (buddy == null || (buddy != null && Buddy.Singleton.Buddies.ContainsKey(buddy.UserId)))
            {
                Buddy.Singleton.Buddies.Remove(p.UserId);
                Buddy.Singleton.RemovePerson(p.UserId);
                return Buddy.Singleton.Config.GetLang("errorMessage");
            }
            try
            {
                if (Buddy.Singleton.Buddies.ContainsKey(p.UserId))
                {
                    Buddy.Singleton.Buddies.Remove(p.UserId);
                    Buddy.Singleton.RemovePerson(p.UserId);
                }
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e);
                return Buddy.Singleton.Config.GetLang("errorMessage");
            }

            Buddy.Singleton.Buddies[p.UserId] = buddy.UserId;
            Buddy.Singleton.Buddies[buddy.UserId] = p.UserId;
            Buddy.Singleton.BuddyRequests.Remove(p.UserId);
            buddy.SendConsoleMessage(Buddy.Singleton.Config.GetLang("buddyRequestAcceptMessage").Replace("$name", p.Nickname), "yellow");
            if (Buddy.Singleton.Config.SendBuddyAcceptedBroadcast)
                buddy.Broadcast(5, Buddy.Singleton.Config.GetLang("buddyRequestAcceptMessage").Replace("$name", p.Nickname));
            return Buddy.Singleton.Config.GetLang("successMessage");
        }
    }
}
