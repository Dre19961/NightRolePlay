using GTANetworkAPI;
using Newtonsoft.Json;
using LiteSDK;
using Lite.GUI;
using System;
using System.Collections.Generic;

namespace Lite.Core
{
    class Parking : Script
    {
        private static nLog RLog = new nLog("Parking");
        private static ColShape shape;
        private static Marker intmarker;
        [ServerEvent(Event.ResourceStart)]
        public static void EnterShapeRealtor()
        {
            try
            {
                #region #Creating Marker
                intmarker = NAPI.Marker.CreateMarker(1, new Vector3(219.48419, -801.5308, 29.6229) + new Vector3(0, 0, 0.1), new Vector3(), new Vector3(), 1.5f, new Color(255, 225, 64), false, 0);
                shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(219.48419, -801.5308, 29.6229), 1, 2, 0);
                shape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        //if (intmarker.HasData("USE")) return;
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 1234);
                        //intmarker.SetData("USE", true);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shape.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                        //intmarker.SetData("USE", false);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                #endregion
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }
        public static void interactPressed(Player player, int index)
        {
            switch (index)
            {
                case 1234:
                    try
                    {
                        OpenCarsMenu(player);
                        return;
                    }
                    catch (Exception e) { RLog.Write("interactParkovka: " + e.Message, nLog.Type.Error); }
                    return;
            }
        }
        public static void OpenCarsMenu(Player player)
        {
            Menu menu = new Menu("cars", false, false);
            menu.Callback = callback_cars;
            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Машины";
            menu.Add(menuItem);
            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                menuItem = new Menu.Item(v, Menu.MenuItem.Button);
                menuItem.Text = $"{VehicleHandlers.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[v].Model)} - {v}";
                menu.Add(menuItem);
            }
            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_cars(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    MenuManager.Close(player);
                    if (item.ID == "close") return;
                    OpenSelectedCarMenu(player, item.ID);
                }
                catch (Exception e) { RLog.Write("callback_carsParkovka: " + e.Message + e.Message, nLog.Type.Error); }
            });
        }

        public static void OpenSelectedCarMenu(Player player, string number)
        {
            Menu menu = new Menu("selectedcar", false, false);
            menu.Callback = callback_selectedcar;
            var vData = VehicleManager.Vehicles[number];

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = number;
            menu.Add(menuItem);

            menuItem = new Menu.Item("model", Menu.MenuItem.Card);
            menuItem.Text = VehicleHandlers.VehiclesName.GetRealVehicleName(vData.Model);
            menu.Add(menuItem);

            var vClass = VehicleHandlers.VehiclesName.GetRealVehicleName(vData.Model);

            menuItem = new Menu.Item("spawn", Menu.MenuItem.Button);
            menuItem.Text = $"Забрать авто со штрафстоянки 500$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_selectedcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "spawn":
                    var number = menu.Items[0].Text;
                    var vData = VehicleManager.Vehicles[number];
                    if (number != null)
                    {
                        if (VehicleManager.Vehicles[number].Spawn == 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не находится на штрафстоянке", 3000);
                            return;
                        }
                        if (Main.Players[player].Money < 500)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {500 - Main.Players[player].Money}$)", 3000);
                            return;
                        }
                        var pos = new Vector3(1, 0, 0);
                        int i = 0;
                        foreach (var Check in Checkpoints1)
                        {
                            var nextCheck = Jobs.WorkManager.rnd.Next(0, Checkpoints1.Count - 1);
                            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vData.Model), Checkpoints1[nextCheck].Position, new Vector3(0.9629243, -0.2101888, 69.269585), 0, 0, number);
                            MoneySystem.Wallet.Change(player, -500);
                            veh.SetSharedData("PETROL", vData.Fuel);
                            veh.SetData("ACCESS", "PERSONAL");
                            veh.SetData("OWNER", player);
                            veh.SetData("ITEMS", vData.Items);
                            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
                            VehicleStreaming.SetEngineState(veh, false);
                            VehicleStreaming.SetLockStatus(veh, false);
                            VehicleManager.ApplyCustomization(veh);
                            VehicleManager.Vehicles[number].Spawn = 0;
                            VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(pos);
                            VehicleManager.Save(number);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваше авто было эвакуировано со штрафcтоянки", 3000);
                            i++;
                            return;
                        }
                    }
                    return;


            }
        }

        #region Checks
        private static List<Checkpoint> Checkpoints1 = new List<Checkpoint>()
        {
           new Checkpoint(new Vector3(223.00449, -784.19507, 30.759344), 71.222595),
           new Checkpoint(new Vector3(219.45573, -794.3235, 30.75051), 30.75051),
           new Checkpoint(new Vector3(221.27374, -789.38727, 30.756048), 30.756048),
           new Checkpoint(new Vector3(225.84679, -793.75085, 30.664787), -108.343315),
           new Checkpoint(new Vector3(228.07092, -786.1933, 30.699955), 30.699955),
           new Checkpoint(new Vector3(230.05434, -781.2544, 30.701187), 30.701187),
        };
        internal class Checkpoint
        {
            public Vector3 Position { get; }
            public double Heading { get; }

            public Checkpoint(Vector3 pos, double rot)
            {
                Position = pos;
                Heading = rot;
            }
        }
        #endregion
    }
}
