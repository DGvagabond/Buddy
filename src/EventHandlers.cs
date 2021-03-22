using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Buddy
{
    class EventHandlers
    {
        private readonly RoleType[] _scpRoles = { RoleType.Scp049, RoleType.Scp079, RoleType.Scp096, RoleType.Scp106, RoleType.Scp173, RoleType.Scp93953, RoleType.Scp93989 };
        private readonly Random _rnd = new Random();

        public void OnPlayerJoin(VerifiedEventArgs ev)
        {
            Timing.RunCoroutine(SendJoinMessage(ev.Player));
            Timing.RunCoroutine(SendBroadcast(ev.Player));
        }

        public IEnumerator<float> SendJoinMessage(Player p)
        {
            yield return Timing.WaitForSeconds(1f);
            if (!Buddy.Singleton.Buddies.ContainsKey(p.UserId))
            {
                p.SendConsoleMessage(Buddy.Singleton.Config.GetLang("BuddyMessage"), "yellow");
            }
            else
            {
                if (!Buddy.Singleton.Buddies.TryGetValue(p.UserId, out var buddy1) || buddy1 == null)
                {
                    Buddy.Singleton.Buddies.Remove(p.UserId);
                    Buddy.Singleton.RemovePerson(p.UserId);
                }
                else
                {
                    var player = Player.Get(buddy1);
                    if (player == null) yield break;
                    p.SendConsoleMessage(Buddy.Singleton.Config.GetLang("broadcastBuddy").Replace("$buddy", player.Nickname), "yellow");
                }
            }
        }

        private IEnumerator<float> SendBroadcast(Player p)
        {
            yield return Timing.WaitForSeconds(2f);
            if (!Buddy.Singleton.Buddies.ContainsKey(p.UserId) && Buddy.Singleton.Config.SendInfoBroadcast)
            {
                p.Broadcast(5, Buddy.Singleton.Config.GetLang("useBuddyCommandBroadcast"));
            }

            if (!Buddy.Singleton.Buddies.ContainsKey(p.UserId) || !Buddy.Singleton.Config.SendBuddyBroadcast)
                yield break;
            if (!Buddy.Singleton.Buddies.TryGetValue(p.UserId, out var buddy1) || buddy1 == null)
            {
                Buddy.Singleton.Buddies.Remove(p.UserId);
                Buddy.Singleton.RemovePerson(p.UserId);
            }
            else
            {
                var player = Player.Get(buddy1);
                if (player == null) yield break;
                p.Broadcast(5, Buddy.Singleton.Config.GetLang("broadcastBuddy").Replace("$buddy", player.Nickname));
            }
        }

        public void OnRoundStart()
        {
            Timing.RunCoroutine(SetRoles());
        }

        public void OnRoundRestart()
        {
            if (Buddy.Singleton.Config.ResetBuddiesEveryRound)
                Buddy.Singleton.Buddies = new Dictionary<string, string>();
        }

        private IEnumerator<float> SetRoles()
        {
            yield return Timing.WaitForSeconds(1f);

            List<string> doneIDs = new List<string>();
            IEnumerable<string> onlinePlayers = Player.List.Select(x => x.UserId);

            IEnumerable<string> hubs = Buddy.Singleton.Buddies.Values;
            for (var i = 0; i < hubs.Count(); i++)
            {
                var id = hubs.ElementAt(i);
                var player = Player.Get(id);
                if (player == null) continue;
                //check if player has a buddy
                if (Buddy.Singleton.Buddies.ContainsKey(player.UserId))
                {
                    try
                    {
                        if (!Buddy.Singleton.Buddies.TryGetValue(player.UserId, out var buddy1) || buddy1 == null || !onlinePlayers.Contains(id) || !onlinePlayers.Contains(buddy1))
                        {
                            Buddy.Singleton.Buddies.Remove(id);
                            if (buddy1 != null) Buddy.Singleton.Buddies.Remove(buddy1);
                            else Buddy.Singleton.RemovePerson(id);
                            doneIDs.Add(buddy1);
                            doneIDs.Add(id);
                            continue;
                        }
                        if (doneIDs.Contains(id) || doneIDs.Contains(buddy1)) continue;
                        var buddy = Player.Get(buddy1);
                        if (buddy == null) continue;
                        //take action if they have different roles
                        if (player.Role != buddy.Role &&
                            /* massive check for scientist/guard combo */
                            !(!Buddy.Singleton.Config.DisallowGuardScientistCombo && (player.Role == RoleType.FacilityGuard && buddy.Role == RoleType.Scientist || player.Role == RoleType.Scientist && buddy.Role == RoleType.FacilityGuard))
                            )
                        {
                            //SCPs take priority
                            if (buddy.Team == Team.SCP) continue;

                            //if force exact role is on we can just set the buddy to the other player's role
                            if (Buddy.Singleton.Config.ForceExactRole)
                            {
                                buddy.SetRole(player.Role);
                                doneIDs.Add(buddy1);
                                doneIDs.Add(id);
                                continue;
                            }
                            //if they are an scp, we need to remove another scp first
                            if (player.Team == Team.SCP)
                            {
                                //loop through every scp and swap the buddy with one of them
                                var setRole = false;
                                foreach (var hub in Player.List)
                                {
                                    var player1 = hub;
                                    //check if the player is an scp
                                    if (player1.UserId == id || player1.UserId == buddy1 ||
                                        Buddy.Singleton.Buddies.ContainsKey(player1.UserId) ||
                                        player1.Team != Team.SCP) continue;
                                    //set the buddy to that player's role and set the player to classd
                                    buddy.SetRole(player1.Role);
                                    player1.SetRole(RoleType.ClassD);
                                    setRole = true;
                                    doneIDs.Add(buddy1);
                                    doneIDs.Add(id);
                                    break;
                                }
                                //if their role is not set (their buddy is the sole scp), set them to a random scp
                                if (!setRole)
                                {
                                    var roles = new List<RoleType>(_scpRoles);
                                    roles.Remove(player.Role);
                                    buddy.SetRole(roles[_rnd.Next(roles.Count)]);
                                    doneIDs.Add(buddy1);
                                    doneIDs.Add(id);
                                }
                                continue;
                            }
                            //if they are not an scp, we can just set them to the same role as their buddy
                            buddy.SetRole(player.Role);
                            doneIDs.Add(buddy1);
                            doneIDs.Add(id);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        try
                        {
                            Buddy.Singleton.Buddies.Remove(id);
                        }
                        catch (ArgumentException) { }
                        Buddy.Singleton.RemovePerson(id);
                        doneIDs.Add(id);
                        Log.Error(e);
                    }
                }

            }
        }
    }
}
