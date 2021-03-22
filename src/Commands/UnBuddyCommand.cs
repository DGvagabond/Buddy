using System;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;

namespace Buddy
{
    [CommandHandler(typeof(ClientCommandHandler))]
    class UnBuddyCommand : ICommand
    {
        public string Command => Buddy.Singleton.Config.GetLang("buddyUnbuddyCommand");

        public string[] Aliases => null;

        public string Description => "A command to remove your current buddy.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "";
            if (!(sender is PlayerCommandSender)) return true;
            var player = Player.Get(((CommandSender)sender).SenderId);
            response = HandleUnBuddyCommand(player);
            return true;
        }

        private string HandleUnBuddyCommand(Player p)
        {
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
            return Buddy.Singleton.Config.GetLang("unBuddySuccess");
        }
    }
}
