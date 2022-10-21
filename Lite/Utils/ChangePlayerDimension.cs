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
    }
}
            