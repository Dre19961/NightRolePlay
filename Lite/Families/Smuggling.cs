using GTANetworkAPI;
using LiteSDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lite.Families
{
    class Smuggling : Script
    {
        private static nLog Log = new nLog("SmuGGling");
        public static Dictionary<int, SmugglingPlace> SmugglingList = new Dictionary<int, SmugglingPlace>();
        public static int LastWar = -1;
        public static Blip BlipWar;
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            SmugglingList.Add(1, new SmugglingPlace(1, "Завод", 3000, 30000, 30000, new List<Vector3>() { new Vector3(2675.6956, 1430.528, 24.500776), new Vector3(2701.8616, 1468.208, 24.501114), new Vector3(2721.5884, 1587.1068, 24.500952) }, new List<GTANetworkAPI.Object>() { }, new List<ColShape>() { }, new List<Marker>() { }, false));
            SmugglingList.Add(2, new SmugglingPlace(2, "Порт", 5000, 2000, 2000, new List<Vector3>() { new Vector3(184.71274, -3316.2795, 5.6998444), new Vector3(204.21385, -3092.5366, 5.7611227) }, new List<GTANetworkAPI.Object>() { }, new List<ColShape>() { }, new List<Marker>() { }, false));
            SmugglingList.Add(3, new SmugglingPlace(3, "Лесопилка", 4000, 5000, 5000, new List<Vector3>() { new Vector3(-580.6563, 5307.997, 70.223816), new Vector3(-479.47742, 5316.8047, 80.61016), new Vector3(-548.0319, 5273.3975, 74.14236) }, new List<GTANetworkAPI.Object>() { }, new List<ColShape>() { }, new List<Marker>() { }, false));
            SmugglingList.Add(4, new SmugglingPlace(4, "HumanLabs", 18000, 200, 200, new List<Vector3>() { new Vector3(3532.7048, 3767.1724, 29.946386), new Vector3(3613.9194, 3720.5137, 35.161114), }, new List<GTANetworkAPI.Object>() { }, new List<ColShape>() { }, new List<Marker>() { }, false));

            Timers.StartTask("checkWar", 60000, () => CheckWar());
        }
        public static bool CanWar(Player player)
        {
            if ((Main.Players[player].FamilyRank != 0 && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] != 0) || (LastWar == 1 && Main.Players[player].FamilyRank != 0 && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 0)) return false;
            else return true;
        }
        public static void TakeSmuggling()
        {
            if (!SmugglingList[LastWar].Status)
            {
                Timers.Stop("takeSmuggling");
                return;
            }
            foreach (var player in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(player)) continue;
                if (!player.GetData<bool>("IS_WAR_SMUGGLING")) continue;
                if (!CanWar(player)) continue;
                var count = 0;
                var rnd = new Random();
                var money = 0;
                switch (LastWar)
                {
                    case 1:
                        if (rnd.Next(0, 101) > 15)
                        {
                            money = rnd.Next(20000, 60000);
                            MoneySystem.Wallet.Change(player, money);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {money}$", 3000);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы ничего не должны", 3000);
                            return;
                        }
                        break;
                    case 2:
                        count = 2;
                        Core.nInventory.Add(player, new nItem(ItemType.Drugs, count));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {count} наркотических веществ", 3000);
                        if (rnd.Next(0, 101) >= 97)
                        {
                            Core.nInventory.Add(player, new nItem(ItemType.HeavySniper, 1));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: контрабандную снайперскую винтовку", 3000);
                        }
                        else if (rnd.Next(0, 101) > 90)
                        {
                            money = rnd.Next(20000, 44000);
                            MoneySystem.Wallet.Change(player, money);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {money}$", 3000);
                        }
                        break;
                    case 3:
                        count = 4;
                        var bullet = rnd.Next(200, 205);
                        Core.nInventory.Add(player, new nItem((ItemType)bullet, count));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {count} шт. {Core.nInventory.ItemsNames[bullet]}", 3000);

                        if (rnd.Next(0, 101) >= 99)
                        {
                            var gun = rnd.Next(100, 150);
                            Core.nInventory.Add(player, new nItem((ItemType)gun, 1, "xxxxx"));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {Core.nInventory.ItemsNames[gun]}", 3000);
                        }
                        else if (rnd.Next(0, 101) > 90)
                        {
                            money = rnd.Next(15000, 22000);
                            MoneySystem.Wallet.Change(player, money);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {money}$", 3000);
                        }
                        break;
                    case 4:
                        count = 1;
                        Core.nInventory.Add(player, new nItem(ItemType.HealthKit, count));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {count} аптечку", 3000);
                        if (rnd.Next(0, 101) >= 90)
                        {
                            Core.nInventory.Add(player, new nItem(ItemType.Drugs, 1));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: пакетик с веществами", 3000);
                        }
                        else if (rnd.Next(0, 101) > 85)
                        {
                            money = rnd.Next(110000, 160000);
                            MoneySystem.Wallet.Change(player, money);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы нашли: {money}$", 3000);
                        }
                        break;
                }
                SmugglingList[LastWar].Left -= count;
                if (SmugglingList[LastWar].Left < 0) StopWar();
            }
        }
        public static void CheckWar()
        {
            if ((DateTime.Now.Hour == 9 || DateTime.Now.Hour == 16 || DateTime.Now.Hour == 21) && DateTime.Now.Minute == 55)
            {
                NAPI.Task.Run(() =>
                {
                    var id = new Random().Next(1, 5);
                    while (id == LastWar) id = new Random().Next(1, 5);
                    LastWar = id;

                    BlipWar = NAPI.Blip.CreateBlip(303, SmugglingList[LastWar].Position[0], 1.1f, 6, Main.StringToU16("Война за Контрабанду"), 255, 0, true, 0, 0);

                    foreach (var player in Main.Players.Keys.ToList())
                    {
                        if (!Main.Players.ContainsKey(player)) continue;
                        if (!CanWar(player)) continue;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Через 5 минут пройдёт Война за Контрабанду на локации {SmugglingList[LastWar].Name}", 3000);
                        NAPI.Chat.SendChatMessageToPlayer(player, $"~o~[Наводчик] ~w~ Через 5 минут пройдёт Война за Контрабанду на локации ~o~{SmugglingList[LastWar].Name}");
                    }
                });
            }
            else if ((DateTime.Now.Hour == 10 || DateTime.Now.Hour == 17 || DateTime.Now.Hour == 22) && DateTime.Now.Minute == 0) StartWar();
            else if ((DateTime.Now.Hour == 10 || DateTime.Now.Hour == 17 || DateTime.Now.Hour == 22) && DateTime.Now.Minute == 15) StopWar();
        }
        public static void StopWar()
        {
            if (LastWar == -1) return;
            NAPI.Task.Run(() =>
            {
                foreach (var war in SmugglingList[LastWar].Object) war.Delete();
                foreach (var war in SmugglingList[LastWar].ColShape) war.Delete();
                foreach (var war in SmugglingList[LastWar].Marker) war.Delete();
                if (BlipWar != null) NAPI.Task.Run(() => { BlipWar.Delete(); });

                SmugglingList[LastWar].Status = false;

                foreach (var player in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(player)) continue;
                    if (!CanWar(player)) continue;
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Война за Контрабанду на локации {SmugglingList[LastWar].Name} окончена!", 3000);
                    NAPI.Chat.SendChatMessageToPlayer(player, $"~o~[Наводчик] ~w~ Война за Контрабанду на локации ~o~{SmugglingList[LastWar].Name} ~w~окончена!");
                    player.SetData("IS_WAR_SMUGGLING", false);
                }
            });
        }
        public static void StartWar()
        {
            if (LastWar == -1) return;
            NAPI.Task.Run(() =>
            {
                var obj = "";
                if (LastWar == 1) obj = "ex_prop_crate_ammo_bc";
                else if (LastWar == 2) obj = "ex_prop_crate_bull_bc_02";
                else if (LastWar == 3) obj = "ex_prop_crate_ammo_bc";
                else if (LastWar == 4) obj = "ex_prop_crate_med_bc";

                foreach (var pos in SmugglingList[LastWar].Position)
                {
                    SmugglingList[LastWar].Object.Add(NAPI.Object.CreateObject(NAPI.Util.GetHashKey(obj), pos - new Vector3(0, 0, 0.9), new Vector3(0, 0, 0), 255, 0));
                    SmugglingList[LastWar].ColShape.Add(NAPI.ColShape.CreateCylinderColShape(pos - new Vector3(0, 0, 1.2), 4.6f, 8f, 0));
                    SmugglingList[LastWar].Marker.Add(NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 7.2), new Vector3(), new Vector3(), 8f, new Color(255, 0, 0), false, 0));
                };
                BlipWar = NAPI.Blip.CreateBlip(303, SmugglingList[LastWar].Position[0], 1.1f, 6, Main.StringToU16("Война за Контрабанду"), 255, 0, true, 0, 0);

                SmugglingList[LastWar].Status = true;
                SmugglingList[LastWar].Left = SmugglingList[LastWar].Limit;
                Timers.StartTask("takeSmuggling", SmugglingList[LastWar].Interval, () => TakeSmuggling());
                foreach (var col in SmugglingList[LastWar].ColShape)
                {
                    col.OnEntityEnterColShape += (s, e) =>
                    {
                        try
                        {
                            if (!Main.Players.ContainsKey(e)) return;
                            if (SmugglingList[LastWar].Status == false) return;
                            e.SetData("IS_WAR_SMUGGLING", true);
                        }
                        catch (Exception ex) { Log.Write("Enter.colshape.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    col.OnEntityExitColShape += (s, e) =>
                    {
                        try
                        {
                            e.SetData("IS_WAR_SMUGGLING", false);
                        }
                        catch (Exception ex) { Log.Write("Exit.colshape.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                    };
                };

                foreach (var player in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(player)) continue;
                    if (!CanWar(player)) continue;
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Война за Контрабанду на локации {SmugglingList[LastWar].Name} началась!", 3000);
                    NAPI.Chat.SendChatMessageToPlayer(player, $"~o~[Наводчик] ~w~ Война за Контрабанду на локации ~o~{SmugglingList[LastWar].Name} ~w~началась!");
                }
            });
        }
    }
    public class SmugglingPlace
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Interval { get; set; }
        public int Limit { get; set; }
        public int Left { get; set; }
        public List<Vector3> Position { get; set; }
        public List<GTANetworkAPI.Object> Object { get; set; }
        public List<ColShape> ColShape { get; set; }
        public List<Marker> Marker { get; set; }
        public bool Status { get; set; }

        public SmugglingPlace(int id, string name, int interval, int limit, int left, List<Vector3> pos, List<GTANetworkAPI.Object> obj, List<ColShape> colshape, List<Marker> marker, bool status)
        {
            ID = id;
            Name = name;
            Interval = interval;
            Limit = limit;
            Left = left;
            Position = pos;
            Object = obj;
            ColShape = colshape;
            Marker = marker;
            Status = status;
        }
    }
}
