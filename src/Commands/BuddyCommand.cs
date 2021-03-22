using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;

namespace Buddy
{
    [CommandHandler(typeof(ClientCommandHandler))]
    class BuddyCommand : ICommand
    {
        public string Command => Buddy.Singleton.Config.GetLang("buddyCommand");

        public string[] Aliases => null;

        public string Description => "Allows you to pair up with another player and play on the same team.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "";
            var args = arguments.ToArray();
            if (!(sender is PlayerCommandSender)) return true;
            var player = Player.Get(((CommandSender)sender).SenderId);
            if (args.Length != 1)
            {
                response = Buddy.Singleton.Config.GetLang("invalidUsage");
                return true;
            }
            try
            {
                response = HandleBuddyCommand(player, args);
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                response = Buddy.Singleton.Config.GetLang("errorMessage");
            }
            return true;
        }

        private string HandleBuddyCommand(Player p, string[] args)
        {
            //get the player who the request was sent to
            var lower = args[0].ToLower();
            var buddy = Player.List.Where(player => player != null).FirstOrDefault(player => player.Nickname.ToLower().Contains(lower) && player.UserId != p.UserId);
            if (buddy == null)
            {
                return Buddy.Singleton.Config.GetLang("playerNotFoundMessage");
            }
            if(Buddy.Singleton.BuddyRequests.ContainsKey(p.UserId) && Buddy.Singleton.BuddyRequests.TryGetValue(p.UserId, out var buddies) && buddies.Any(player => player.UserId == buddy.UserId) && !Buddy.Singleton.Buddies.ContainsKey(buddy.UserId))
            {
                Buddy.Singleton.Buddies[p.UserId] = buddy.UserId;
                Buddy.Singleton.Buddies[buddy.UserId] = p.UserId;
                Buddy.Singleton.BuddyRequests.Remove(p.UserId);
                buddy.SendConsoleMessage(Buddy.Singleton.Config.GetLang("buddyRequestAcceptMessage").Replace("$name", p.Nickname), "yellow");
                if (Buddy.Singleton.Config.SendBuddyAcceptedBroadcast)
                    buddy.Broadcast(5, Buddy.Singleton.Config.GetLang("buddyRequestAcceptMessage").Replace("$name", p.Nickname));
                return Buddy.Singleton.Config.GetLang("successMessage");
            }
            if (!Buddy.Singleton.BuddyRequests.ContainsKey(buddy.UserId)) Buddy.Singleton.BuddyRequests[buddy.UserId] = new List<Player>();
            Buddy.Singleton.BuddyRequests[buddy.UserId].Add(p);
            buddy.SendConsoleMessage(Buddy.Singleton.Config.GetLang("BuddyMessagePrompt").Replace("$name", p.Nickname), "yellow");
            if (Buddy.Singleton.Config.SendBuddyRequestBroadcast && !Round.IsStarted)
                buddy.Broadcast(5, Buddy.Singleton.Config.GetLang("broadcastBuddyRequest").Replace("$name", p.Nickname));
            return Buddy.Singleton.Config.GetLang("buddyRequestSentMessage");
        }
    }
}
