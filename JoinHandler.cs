﻿using MEC;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace PintBuddy
{
    internal class JoinHandler : IEventHandlerPlayerJoin
    {
        private Buddy buddyPlugin;

        public JoinHandler(Buddy buddyPlugin)
        {
            this.buddyPlugin = buddyPlugin;
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (buddyPlugin.Round.Duration != 0) return;
            Timing.RunCoroutine(sendJoinMessage(ev.Player));
        }
        public IEnumerator<float> sendJoinMessage(Player p)
        {
            yield return Timing.WaitForSeconds(1f);
            p.SendConsoleMessage(buddyPlugin.prefixedMessage, "yellow");
        }
    }
}