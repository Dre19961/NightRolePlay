using GTANetworkAPI;
using LiteSDK;
using Lite.Core;
using System;
using System.Collections.Generic;

namespace Lite.Core
{
    class Hijacking : Script
    {
        public static Random rnd = new Random();
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        private static ColShape _shape;
        public static Vehicle _veh;
        private static TextLabel _text;
        private static Marker _marker;
        private static string _number;
        private static ColShape _cols;
        private static nLog Log = new nLog("HIJACKING");
        private static Blip _blip;
        private static List<Vector3> Checkpoints = new List<Vector3>()
        {
            new Vector3(166.16774, 2229.5964, 89.60551),
            new Vector3(166.16774, 2229.5964, -1000.60551),
        };
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                foreach (Vector3 vec in Checkpoints)
                {
                    NAPI.Marker.CreateMarker(1, vec - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.7f, new Color(255, 255, 0));
                }
                NAPI.Blip.CreateBlip(380, new Vector3(166.16774, 2229.5964, 89.60551), 0.9f, 1, Main.StringToU16("Угон авто"), 255, 0, true, 0, 0);
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Checkpoints[0], 1, 2, 0));
                Cols[0].SetData("INTERACT", 1872);
                Cols[0].OnEntityEnterColShape += Shape_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += Shape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~w~Нажмите Е"), new Vector3(Checkpoints[0].X, Checkpoints[0].Y, Checkpoints[0].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(Checkpoints[1], 1, 2, 0));
                Cols[1].SetData("INTERACT", 1872);
                Cols[1].OnEntityEnterColShape += Shape_onEntityEnterColShape;
                Cols[1].OnEntityExitColShape += Shape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~w~Нажмите Е"), new Vector3(Checkpoints[1].X, Checkpoints[1].Y, Checkpoints[1].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        private void Shape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 1872);
            }
            catch (Exception ex) { Log.Write("Shape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }
        private void Shape_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("Shape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }
        public static void StartHijacking(Player player)
        {
            try
            {
                if (player.HasData("HijackingPlayer"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже взяли заказ на угон авто.", 3000);
                    return;
                }
                if (Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2 || Main.Players[player].FractionID == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в крим.организации.", 3000);
                    return;
                }
                var rand = rnd.Next(0, 5);
                var rand2 = rnd.Next(0, 13);
                var vehs = vehicles[rand2];
                _number = VehicleManager.GenerateNumber();
                var pos = SpawnPosition[rand];
                _veh = NAPI.Vehicle.CreateVehicle(vehs, pos, SpawnRotation[rand], 0, 0, _number, 255, false, false, 0);
                Trigger.ClientEvent(player, "BlipsHijacking", true, pos + new Vector3(rand2, rand, rand2));
                _veh.SetData("Hijacking", true);
                VehicleStreaming.SetEngineState(_veh, false);
                player.SetData("HijackingPlayer", true);
                NAPI.Data.SetEntityData(player, "VEHICLE_ONEMSTIMER", 0);
                NAPI.Data.SetEntityData(player, "VEHICLE_ONEMS", Timers.StartTask(1000, () => timer_playerStartHikacking(player)));
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы взяли заказ на угон авто. Теперь едьте и найдите его.", 3000);
                player.SendChatMessage($"Авто которое вам надо угнать - ~o~{vehs}. ~w~Регистрационный номер - ~o~{_number}. Цвет - черый.");
                NAPI.Notification.SendNotificationToPlayer(player, $"Здарова, сегодня у нас {vehs}, цвет: Черный, номера: {_number}.");
                return;
            }
            catch (Exception e) { Log.Write("StartHijacking : " + e.Message); }
        }
        private static void timer_playerStartHikacking(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("VEHICLE_ONEMS")) return;
                    if (NAPI.Data.GetEntityData(player, "VEHICLE_ONEMSTIMER") > 1800)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы не успели сдать авто вовремя.", 3000);
                        if (_cols != null) _cols.Delete();
                        if (_text != null) _text.Delete();
                        if (_veh != null) _veh.Delete();
                        if (_marker != null) _marker.Delete();
                        if (_blip != null) _blip.Delete();
                        Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                        player.ResetData("HijackingPlayer");
                        player.ResetData("VEHICLE_ONEMSTIMER");
                        player.ResetData("VEHICLE_ONEMS");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "VEHICLE_ONEMSTIMER", NAPI.Data.GetEntityData(player, "VEHICLE_ONEMSTIMER") + 1);
                }
                catch (Exception e) { Log.Write("exitVehicleTimer: " + e.Message); }
            });
        }
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("HijackingPlayer"))
                {
                    NAPI.Task.Run(() =>
                    {
                        if (_cols != null) _cols.Delete();
                        if (_text != null) _text.Delete();
                        if (_veh != null) _veh.Delete();
                        if (_marker != null) _marker.Delete();
                        if (_blip != null) _blip.Delete();
                        player.ResetData("HijackingPlayer");
                        Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                    });
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("HijackingPlayer"))
                {
                    if (_cols != null) _cols.Delete();
                    if (_text != null) _text.Delete();
                    if (_marker != null) _marker.Delete();
                    if (_blip != null) _blip.Delete();
                    Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerEnterVehicle)]
        public static void OnPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (vehicle.HasData("Hijacking"))
                {
                    if (Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 0 || Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 1)
                    {
                        var rand = rnd.Next(0, 3);
                        var pos = new Vector3(-62.455975, 6444.0454, 30.370638);
                        _cols = NAPI.ColShape.CreateCylinderColShape(pos, 7f, 3f, 0);
                        _marker = NAPI.Marker.CreateMarker(1, pos, new Vector3(), new Vector3(), 7f, new Color(0, 0, 255));
                        _cols.OnEntityEnterColShape += EnterCheckpoint;
                        _text = NAPI.TextLabel.CreateTextLabel("Сдача авто", pos + new Vector3(0, 0, 2.5), 7f, 7f, 4, new Color(0, 0, 255), false, 0);
                        Trigger.ClientEvent(player, "createWaypoint", pos.X, pos.Y);
                        Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                    }
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                        if (Main.Players[p].FractionID == 7 || Main.Players[p].FractionID == 9 || Main.Players[p].FractionID == 14 || Main.Players[p].FractionID == 18)
                        {
                            p.SendChatMessage($"~g~[Фракция] Только что было угнано авто - {_veh.DisplayName}. C регистрационным номером - {_number}. Цвет - черый.");
                            NAPI.Notification.SendNotificationToPlayer(p, "Поступил угон авто.");
                            _blip = NAPI.Blip.CreateBlip(468, player.Vehicle.Position, 1, 38, "Угон авто", 255, 0, true, 0, 0);
                        }
                        else return;
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        public static void EnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!player.IsInVehicle) return;
                if (player.IsInVehicle && player.Vehicle.HasData("Hijacking"))
                {
                    player.Vehicle.Delete();
                    var count = 247 * rnd.Next(0, 12) + rnd.Next(100, 400);
                    MoneySystem.Wallet.Change(player, +count);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно довезли авто и получили {count}.", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                    player.ResetData("HijackingPlayer");
                    NAPI.Task.Run(() =>
                    {
                        if (_cols != null) _cols.Delete();
                        if (_text != null) _text.Delete();
                        if (_marker != null) _marker.Delete();
                        if (_blip != null) _blip.Delete();
                    });
                }
                return;
            }
            catch (Exception e) { Log.Write("EnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }
        #region Lists
        private static List<VehicleHash> vehicles = new List<VehicleHash>()
        {
             VehicleHash.Issi2,
             VehicleHash.Rhapsody,
             VehicleHash.Bati,
             VehicleHash.Tribike3,
             VehicleHash.Akuma,
             VehicleHash.Carbonrs,
             VehicleHash.Manchez,
             VehicleHash.Blade,
             VehicleHash.Buccaneer,
             VehicleHash.Buccaneer2,
             VehicleHash.Chino,
             VehicleHash.Coquette3,
             VehicleHash.Sabregt,
             VehicleHash.Blazer,
             VehicleHash.Baller
        };
        private static List<Vector3> SpawnPosition = new List<Vector3>()
        {
             new Vector3(-1290.945, -3422.2166, 13.787236),
             new Vector3(-11.737893, -1785.1013, 27.968487),
             new Vector3(158.92671, -1893.8419, 22.952347),
             new Vector3(226.35698, -1799.7167, 27.753403),
             new Vector3(134.9038, -1553.0101, 29.15972),
             new Vector3(55.477787, -1552.3198, 29.310167)
        };
        private static List<Vector3> SpawnRotation = new List<Vector3>()
        {
            new Vector3(0.21983007, -9.55005, 59.86459),
            new Vector3(0.12679951, -7.8193703, 31.451086),
            new Vector3(-0.20716837, -7.7772408, -115.13205),
            new Vector3(2.6844351, -7.2517166, 53.820446),
            new Vector3(1.3398341, -7.794012, 54.266647),
            new Vector3(0.26782653, -4.7209334, -129.82812)
        };
        #endregion
    }
}