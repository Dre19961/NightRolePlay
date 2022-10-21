using GTANetworkAPI;
using Lite;
using LiteSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Lite.Utils
{
    internal static class ExtensionPlayer
    {
        internal static bool IsSpawned(this Player player) { return player.HasData("IsSpawned") ? player.GetData<bool>("IsSpawned") : false; }
        internal static void SetSpawned(this Player player, bool value) { player.SetData<bool>("IsSpawned", value); }
    }
    internal class ChangePlayerDimension : Script
    {
        private const uint _worldDimension = 0;
        private Dictionary<Player, string> timerId = new Dictionary<Player, string>();
        [RemoteEvent("changePlayerDimension")]
        private void OnСhangePlayerDimension(Player player, uint dimension)
        {
            switch (dimension)
            {
                case _worldDimension:
                    if (!Main.Players.ContainsKey(player)) player.Kick();
                    break;
                default:
                    break;
            }
        }
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            //var derivedTypes = Assembly.GetExecutingAssembly().ExportedTypes.Where(t => t.BaseType.FullName == "GTANetworkAPI.Script");
            DaVilka.Trigger.OnPlayerSpawn += (p, c) => { p.SetSpawned(true); };
        }
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            if (!timerId.ContainsKey(player))
            {
                string id = Timers.Start(2000, () =>
                {
                    if (player.Dimension == 0 && !player.IsSpawned()) { player.Kick(); return; }
                    if (player.IsSpawned())
                    {
                        if (timerId.ContainsKey(player))
                        {
                            Timers.Stop(timerId[player]);
                            timerId.Remove(player);
                        }
                    }
                });
                timerId.Add(player, id);
            }
        }
        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDissconect(Player player, DisconnectionType disconnectionType, string d)
        {
            if (timerId.ContainsKey(player))
            {
                Timers.Stop(timerId[player]);
                timerId.Remove(player);
            }
        }
    }
}
