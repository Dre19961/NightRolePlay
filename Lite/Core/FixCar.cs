using GTANetworkAPI;
using System.Collections.Generic;
using LiteSDK;

namespace Lite.Core
{
    class FixCar : Script
    {
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        // добавляем в этот лист координаты починок
        public static List<Vector3> Coords = new List<Vector3>()
        {
             new Vector3(499.41925, -1335.4012, 29.323275),
            new Vector3(470.56805, -1023.6964, 27.06632),
            new Vector3(-357.53308, -115.3788, 37.575314),
            new Vector3(-208.78374, -1391.2856, 31.428516),
            new Vector3(1422.9178, -1504.2079, 61.135494),
            new Vector3(1170.4064, -316.0126, 69.358505),
            new Vector3(949.84125, -120.291916, 74.52971),
            new Vector3(-83.48184, 80.813995, 71.708275),
            new Vector3(-618.18097, 283.46234, 81.85261),
            new Vector3(-763.66986, -242.42183, 37.412857),
            new Vector3(-1250.933, -637.87415, 26.076738),
            new Vector3(-955.3644, -1287.4888, 5.2011504),
            new Vector3(-2036.6466, -274.43417, 23.563978),
            new Vector3(-1787.2352, 811.90045, 138.67274),
            new Vector3(-2316.1365, 438.07343, 174.6444),
            new Vector3(-2194.2644, 3306.5925, 32.901608),
            new Vector3(436.82632, 3577.888, 32.118565),
            new Vector3(1411.2416, 3620.774, 33.774376),
            new Vector3(2414.7856, 4991.822, 45.085464),
            new Vector3(1596.1798, 6449.076, 24.19714),
            new Vector3(420.57037, 6534.813, 26.58816),
            new Vector3(-82.2954, 6496.208, 30.370892),
            new Vector3(-472.5898, 5967.3286, 30.186281),
            new Vector3(-2193.8367, 4268.8706, 47.464153),
            new Vector3(-3169.4941, 1101.7515, 19.637533),
            new Vector3(-2951.2207, 54.670444, 10.488505),
            new Vector3(-65.53547, 892.3283, 234.44601),
            new Vector3(672.58405, 622.73706, 127.7909),
            new Vector3(-430.57852, 1203.2225, 324.63828),
            new Vector3(1397.203, 1059.7372, 113.213684),
            new Vector3(537.5262, -183.3282, 53.304474),
            new Vector3(-1273.1737, 251.44083, 62.401295),
            new Vector3(2579.7612, 427.82645, 107.33549),
            new Vector3(1719.5271, 4801.248, 40.562683)
        };
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            for (int i = 0; i < Coords.Count; i++)
            {
                Cols.Add(i, NAPI.ColShape.CreateCylinderColShape(Coords[i], 1.5f, 2, 0));
                Cols[i].OnEntityEnterColShape += auto_onEntityEnterColshape;
                Cols[i].OnEntityExitColShape += auto_onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~w~Починка авто. \n ~w~Стоимость: {price}$"), Coords[i] + new Vector3(0, 0, 1), 10F, 0.6F, 0, new Color(0, 180, 0));
                NAPI.Marker.CreateMarker(1, Coords[i] - new Vector3(0, 0, 2), new Vector3(), new Vector3(), 4, new Color(129, 159, 235));
                NAPI.Blip.CreateBlip(544, Coords[i], 0.6f, 4, Main.StringToU16("Починка авто"), 255, 0, true, 0, 0);
            }
        }
        
        private static int price = 20000; // цену на починку редактировать тут

        private void auto_onEntityEnterColshape(ColShape shape, Player entity)
        {
            NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 135);
        }
        private void auto_onEntityExitColshape(ColShape shape, Player entity)
        {
            NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
        }
        [RemoteEvent("server::lscustomsrepair")]
        public static void mechanicfixcar(Player player)
        {
                if (!player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспортном средстве", 3000);
                    return;
                }
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас недостаточно денег", 3000);
                    return;
                }
                VehicleManager.RepairCar(player.Vehicle);
                MoneySystem.Wallet.Change(player, -price);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно починили транспорт за {price}$", 3000);
        }
        public static void MechanicFixCar(Player player)
        {
            if (player.GetData<int>("INTERACTIONCHECK") != 135) return;
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспортном средстве", 3000);
                return;
            }
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас недостаточно денег", 3000);
                return;
            }
            VehicleManager.RepairCar(player.Vehicle);
            MoneySystem.Wallet.Change(player, -price);
            NAPI.Data.ResetEntityData(player, "INTERACTIONCHECK");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно починили транспорт за {price}$", 3000);
        }
    }
}
