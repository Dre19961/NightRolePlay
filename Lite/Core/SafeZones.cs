using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using LiteSDK;

namespace Lite.Core
{
    class SafeZones : Script
    {
        private static nLog Log = new nLog("SafeZones");
        public static void CreateSafeZone(Vector3 position, int height, int width, bool showNotify = true)
        {
            var colShape = NAPI.ColShape.Create2DColShape(position.X, position.Y, height, width, 0);
            colShape.OnEntityEnterColShape += (shape, player) =>
            {
                try
                {
                    Trigger.ClientEvent(player, "GreenZone", true);
                    Trigger.ClientEvent(player, "safeZone", true);
                }
                catch (Exception e) { Log.Write($"SafeZoneEnter: {e.Message}", nLog.Type.Error); }

            };
            colShape.OnEntityExitColShape += (shape, player) =>
            {
                try
                {
                    Trigger.ClientEvent(player, "GreenZone", false);
                    Trigger.ClientEvent(player, "safeZone", false);
                }
                catch (Exception e) { Log.Write($"SafeZoneExit: {e.Message}", nLog.Type.Error); }
            };
        }

        [ServerEvent(Event.ResourceStart)]
        public void Event_onResourceStart()
        {
            CreateSafeZone(new Vector3(355.5395, -1409.0018, 31.822496), 150, 150); //ems
            CreateSafeZone(new Vector3(-561.2012, -194.0378, 37.10239), 150, 150); //major
            CreateSafeZone(new Vector3(113.6499, -752.8422, 44.63474), 85, 85); //fib
            CreateSafeZone(new Vector3(-371.19, -236.16, 35.90), 50, 50); // spawn
            CreateSafeZone(new Vector3(-570.4495, -394.9133, 33.9366), 50, 50);
            CreateSafeZone(new Vector3(1216.343, -2989.767, 4.769878), 200, 200);//container
            CreateSafeZone(new Vector3(4970.415, -5861.8257, 19.880571), 3500, 3500);//ostrov 
            CreateSafeZone(new Vector3(-818.4709, -1339.8889, 5.0302863), 200, 200);//newspawn
            CreateSafeZone(new Vector3(-622.933, -2186.7117, 6.1091094), 200, 200);//rinok
            CreateSafeZone(new Vector3(1089.9122, 206.23521, -50.119774), 300, 300);//casino
            CreateSafeZone(new Vector3(922.29596, 47.001987, 79.98637), 100, 100);//casinoexit
            CreateSafeZone(new Vector3(427.82538, -979.7156, 29.5901), 200, 200);//лспд
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public static void SetEnterInteractionCheck(ColShape shape, Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (player.HasData("INTERACTIONCHECK") && player.GetData<int>("INTERACTIONCHECK") <= 0) return;
            if (player.HasData("CUFFED") && player.GetData<bool>("CUFFED")) return;
            if (player.HasData("IS_DYING") || player.HasData("FOLLOWING")) return;

            if (player.HasData("GARAGEID"))
            {
                Houses.House house = Houses.HouseManager.GetHouse(player);
                if (house == null) return;
                if (player.GetData<int>("GARAGEID") != house.GarageID) return;
            }
            Trigger.ClientEvent(player, "playerInteractionCheck", true);
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public static void SetExitInteractionCheck(ColShape shape, Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Trigger.ClientEvent(player, "playerInteractionCheck", false);
        }
    }
}
