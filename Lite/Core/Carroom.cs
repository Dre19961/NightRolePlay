using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Lite.Core.nAccount;
using Lite.Core.Character;
using Lite.VehicleHandlers;
using System.Linq;
using LiteSDK;

namespace Lite.Core
{
    class CarRoom : Script
    {
        private static nLog Log = new nLog("CARROOM");

        public static Vector3 CamPosition = new Vector3(-406.2917, 1188.7247, 324.49088); // Позиция камеры
        public static Vector3 CamRotation = new Vector3(0, 0, 159.62071); // Rotation камеры
        public static Vector3 CarSpawnPos = new Vector3(-409.19492, 1179.4828, 325.49442); // Место для спавна машины в автосалоне
        public static Vector3 CarSpawnRot = new Vector3(0.21460006, -0.7271854, -71.297745); // Rotation для спавна машины в автосалоне

        public static void onPlayerDissonnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!player.HasData("CARROOMTEST")) return;

                var veh = player.GetData<Vehicle>("CARROOMTEST");
                veh.Delete();

                RemoteEvent_carroomCancel(player);

                player.ResetData("CARROOMTEST");

            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void enterCarroom(Player player, string name)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player)) return;
            uint dim = Dimensions.RequestPrivateDimension(player);
            NAPI.Entity.SetEntityDimension(player, dim);
            Main.Players[player].ExteriorPos = player.Position;
            NAPI.Entity.SetEntityPosition(player, new Vector3(CamPosition.X, CamPosition.Y - 2, CamPosition.Z));
            //player.FreezePosition = true;
            Trigger.ClientEvent(player, "freeze", true);

            player.SetData("INTERACTIONCHECK", 0);
            Trigger.ClientEvent(player, "carRoom");

            OpenCarromMenu(player, BusinessManager.BizList[player.GetData<int>("CARROOMID")].Type);
        }


        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void Event_OnPlayerExitVehicleAttempt(Player player, Vehicle vehicle)
        {
            try
            {
                if (!player.HasData("CARROOMTEST")) return;

                var veh = player.GetData<Vehicle>("CARROOMTEST");
                veh.Delete();

                uint dim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, dim);
                Main.Players[player].ExteriorPos = player.Position;
                NAPI.Entity.SetEntityPosition(player, new Vector3(CamPosition.X, CamPosition.Y - 2, CamPosition.Z));

                Trigger.ClientEvent(player, "carRoom");
                OpenCarromMenu(player, BusinessManager.BizList[player.GetData<int>("CARROOMID")].Type);
            }
            catch (Exception e)
            {
                Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error);
            }
        }

        [RemoteEvent("carroomTestDrive")]
        public static void RemoteEvent_carroomTestDrive(Player player, string vName, int color1, int color2, int color3)
        {
            try
            {
                if (!player.HasData("CARROOMID")) return;

                Trigger.ClientEvent(player, "destroyCamera");

                var mydim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, mydim);
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vName);
                var veh = NAPI.Vehicle.CreateVehicle(vh, new Vector3(-409.19492, 1179.4828, 325.49442), new Vector3(0.21460006, -0.7271854, -71.297745), 0, 0);
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, color1, color2, color3);
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, color1, color2, color3);
                veh.Dimension = mydim;
                veh.NumberPlate = "TESTDRIVE";
                veh.SetData("BY", player.Name);
                VehicleStreaming.SetEngineState(veh, true);
                player.SetIntoVehicle(veh, 0);
                player.SetData("CARROOMTEST", veh);
            }
            catch (Exception e)
            {
                Log.Write("TestDrive: " + e.Message, nLog.Type.Error);
            }
        }

        #region Menu
        private static Dictionary<string, Color> carColors = new Dictionary<string, Color>
        {
            { "Черный", new Color(0, 0, 0) },
            { "Белый", new Color(225, 225, 225) },
            { "Красный", new Color(230, 0, 0) },
            { "Оранжевый", new Color(255, 115, 0) },
            { "Желтый", new Color(240, 240, 0) },
            { "Зеленый", new Color(0, 230, 0) },
            { "Голубой", new Color(0, 205, 255) },
            { "Синий", new Color(0, 0, 230) },
            { "Фиолетовый", new Color(190, 60, 165) },
        };

        public static void OpenCarromMenu(Player player, int biztype)
        {
            var bizid = player.GetData<int>("CARROOMID");
            Business biz = BusinessManager.BizList[player.GetData<int>("CARROOMID")];
            var prices = new List<int>();

            if (biz.Type == 19)
            {
                player.SetSharedData("CARROOM-DONATE", true);
                biztype = 5;
            }
            else
            {
                player.SetSharedData("CARROOM-DONATE", false);
                biztype -= 2;
            }

            foreach (var p in biz.Products)
            {
                prices.Add(p.Price);
            }

            Trigger.ClientEvent(player, "openAuto", JsonConvert.SerializeObject(BusinessManager.CarsNames[biztype]), JsonConvert.SerializeObject(prices));
        }

        private static string BuyVehicle(Player player, Business biz, string vName, string color)
        {
            var prod = biz.Products.FirstOrDefault(p => p.Name == vName);
            string vNumber = "none";
            //Если нет лицензии на вождение автомобилем
            //Если тип бизнеса мотосалон и нет лицензии на вождение мотоциклов
            if(Main.Players[player].Licenses[0] == false && biz.Type == 5)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет лицензии на управление транспорта категории [A]", 2500);
                return vNumber;
            }
            if (biz.Type != 19)
            {
                // Check products available
                if (Main.Players[player].Money < prod.Price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return vNumber;
                }
                if (!BusinessManager.takeProd(biz.ID, 1, vName, prod.Price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Транспортного средства больше нет на складе", 3000);
                    return vNumber;
                }

                MoneySystem.Wallet.Change(player, -prod.Price);

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", prod.Price, $"buyCar({vName})");
            }
            else if (biz.Type == 19)
            {
                Account acc = Main.Accounts[player];

                if (acc.RedBucks < prod.Price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно Redbucks!", 3000);
                    return vNumber;
                }
                acc.RedBucks -= prod.Price;
                GameLog.Money(acc.Login, "server", prod.Price, "donateAutoroom");
            }

            vNumber = VehicleManager.Create(player.Name, vName, carColors[color], carColors[color], new Color(0, 0, 0));
            nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{vNumber}_{VehicleManager.Vehicles[vNumber].KeyNum}"));

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {VehicleHandlers.VehiclesName.GetRealVehicleName(vName)} с номером {vNumber} ", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Автомобиль доставлен в ваш гараж!", 5000);

            return vNumber;
        }


        [RemoteEvent("carroomBuy")]
        public static void RemoteEvent_carroomBuy(Player player, string vName, string color)
        {
            try
            {
                Business biz = BusinessManager.BizList[player.GetData<int>("CARROOMID")];
                NAPI.Entity.SetEntityPosition(player, new Vector3(biz.EnterPoint.X, biz.EnterPoint.Y, biz.EnterPoint.Z + 1.5));
                Trigger.ClientEvent(player, "freeze", false);
                //player.FreezePosition = false;

                Main.Players[player].ExteriorPos = new Vector3();
                Trigger.ClientEvent(player, "destroyCamera");
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);

                var house = Houses.HouseManager.GetHouse(player, true);
                if (house == null || house.GarageID == 0)
                {
                    // Player without garage
                    string vNumber = BuyVehicle(player, biz, vName, color);
                    if (vNumber != "none")
                    {
                        // VehicleManager.Spawn(vNumber, biz.UnloadPoint, 90, player);
                    }
                }
                else
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    // Проверка свободного места в гараже
                    if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ваши гаражи полны", 3000);
                        return;
                    }
                    string vNumber = BuyVehicle(player, biz, vName, color);
                    if (vNumber != "none")
                    {
                        garage.SpawnCar(vNumber);
                    }
                }
                foreach (nItem items in nInventory.Items[Main.Players[player].UUID])
                {
                    if (items.FastSlots > 0)
                    {
                        if (FastSlots.Carabine.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 24818, new Vector3(-0.1, -0.15, -0.13), new Vector3(0.0, 0.0, 3.5), items);
                        }
                        if (FastSlots.Shot.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 24818, new Vector3(-0.1, -0.15, 0.11), new Vector3(-180.0, 0.0, 0.0), items);
                        }
                        if (FastSlots.SMG.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 58271, new Vector3(0.08, 0.03, -0.1), new Vector3(-80.77, 0.0, 0.0), items);
                        }
                        if (FastSlots.Pistol.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 51826, new Vector3(0.02, 0.06, 0.1), new Vector3(-100.0, 0.0, 0.0), items);
                        }
                    }
                }
            }
            catch (Exception e) { Log.Write("CarroomBuy: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("carroomCancel")]
        public static void RemoteEvent_carroomCancel(Player player)
        {
            try
            {
                if (!player.HasData("CARROOMID")) return;
                var enterPoint = BusinessManager.BizList[player.GetData<int>("CARROOMID")].EnterPoint;
                NAPI.Entity.SetEntityPosition(player, new Vector3(enterPoint.X, enterPoint.Y, enterPoint.Z + 1.5));
                Main.Players[player].ExteriorPos = new Vector3();
                Trigger.ClientEvent(player, "freeze", false);
                //player.FreezePosition = false;
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);
                player.ResetData("CARROOMID");
                foreach (nItem items in nInventory.Items[Main.Players[player].UUID])
                {
                    if (items.FastSlots > 0)
                    {
                        if (FastSlots.Carabine.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 24818, new Vector3(-0.1, -0.15, -0.13), new Vector3(0.0, 0.0, 3.5), items);
                        }
                        if (FastSlots.Shot.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 24818, new Vector3(-0.1, -0.15, 0.11), new Vector3(-180.0, 0.0, 0.0), items);
                        }
                        if (FastSlots.SMG.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 58271, new Vector3(0.08, 0.03, -0.1), new Vector3(-80.77, 0.0, 0.0), items);
                        }
                        if (FastSlots.Pistol.Contains(items.Type))
                        {
                            BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[items.Type], 51826, new Vector3(0.02, 0.06, 0.1), new Vector3(-100.0, 0.0, 0.0), items);
                        }
                    }
                }

                if (!player.HasData("CARROOMTEST")) Trigger.ClientEvent(player, "destroyCamera");
            }
            catch (Exception e) { Log.Write("carroomCancel: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}
