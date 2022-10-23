using GTANetworkAPI;
using Lite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lite.Utils
{
    internal class ChangePlayerDimension : Script
    {
        private const uint _worldDimension = 0;

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
                    if (player.Dimension == 0 && !player.IsSpawned()) player.Kick();
                    //if (player.IsSpawned())
                    //{
                    //    if (timerId.ContainsKey(player))
                    //    {
                    //        Timers.Stop(timerId[player]);
                    //        timerId.Remove(player);
                    //    }
                    //}
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
            