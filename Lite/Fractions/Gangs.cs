using System.Collections.Generic;
using GTANetworkAPI;
using Lite.Core;
using LiteSDK;
using System;

namespace Lite.Fractions
{
    class Gangs : Script
    {
        private static nLog Log = new nLog("Gangs");

        public static Dictionary<int, Vector3> ExitPoints = new Dictionary<int, Vector3>()
        {
        };

        public static List<Vector3> DrugPoints = new List<Vector3>()
        {
            /*new Vector3(8.621573, 3701.914, 39.51624),
            new Vector3(3804.169, 4444.753, 3.977164),*/
        };

        [ServerEvent(Event.ResourceStart)]
        public void Event_OnResourceStart()
        {
            try
            {
                var BallasCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(157.59778, -1712.1833, 25.791407), 1, 2, 0);
                BallasCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 941); }  catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                BallasCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };
                API.Shared.CreateTextLabel("Крафт оружия -~n~~n~~w~Нажмите [~s~E~w~], что бы открыть меню", new Vector3(157.59778, -1712.1833, 25.791407) + new Vector3(0, 0, 0.2f), 15.0f, 0.9f, 4, new Color(110, 179, 70, 190));
                NAPI.Marker.CreateMarker(1, new Vector3(157.59778, -1712.1833, 25.791407), new Vector3(), new Vector3(), 1f, new Color(255, 0, 255, 120), false, 0);

                var GrooveCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(-47.40061, -1393.4213, 29.382431), 1, 2, 0);
                GrooveCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 940); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                GrooveCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };
                API.Shared.CreateTextLabel("Крафт оружия -~n~~n~~w~Нажмите [~s~E~w~], что бы открыть меню", new Vector3(-47.40061, -1393.4213, 29.382431) + new Vector3(0, 0, 0.2f), 15.0f, 0.9f, 4, new Color(110, 179, 70, 190));
                NAPI.Marker.CreateMarker(1, new Vector3(-47.40061, -1393.4213, 29.382431), new Vector3(), new Vector3(), 1f, new Color(255, 0, 255, 120), false, 0);
              
                var VagosCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(811.9413, -2314.7085, 30.464725), 1, 2, 0);
                VagosCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 942); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                VagosCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };
                API.Shared.CreateTextLabel("Крафт оружия -~n~~n~~w~Нажмите [~s~E~w~], что бы открыть меню", new Vector3(811.9413, -2314.7085, 30.464725) + new Vector3(0, 0, 0.2f), 15.0f, 0.9f, 4, new Color(110, 179, 70, 190));
                NAPI.Marker.CreateMarker(1, new Vector3(811.9413, -2314.7085, 30.464725), new Vector3(), new Vector3(), 1f, new Color(255, 0, 255, 120), false, 0);
            
                var CripsCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(1361.1051, -2091.7688, 47.20812), 1, 2, 0);
                CripsCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 943); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                CripsCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };
                API.Shared.CreateTextLabel("Крафт оружия -~n~~n~~w~Нажмите [~s~E~w~], что бы открыть меню", new Vector3(1361.1051, -2091.7688, 47.20812) + new Vector3(0, 0, 0.2f), 15.0f, 0.9f, 4, new Color(110, 179, 70, 190));
                NAPI.Marker.CreateMarker(1, new Vector3(1361.1051, -2091.7688, 47.20812), new Vector3(), new Vector3(), 1f, new Color(255, 0, 255, 120), false, 0);
             
                var BloodsCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(929.69104, -1526.7542, 31.112059), 1, 2, 0);
                BloodsCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 944); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                BloodsCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };
                API.Shared.CreateTextLabel("Крафт оружия -~n~~n~~w~Нажмите [~s~E~w~], что бы открыть меню", new Vector3(929.69104, -1526.7542, 31.112059) + new Vector3(0, 0, 0.2f), 15.0f, 0.9f, 4, new Color(110, 179, 70, 190));
                NAPI.Marker.CreateMarker(1, new Vector3(929.69104, -1526.7542, 31.112059), new Vector3(), new Vector3(), 1f, new Color(255, 0, 255, 120), false, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OpenCraftGang(Player player, int id)
        {
            if (Main.Players[player].FractionID != id)
            {
                Notify.Error(player, "Вы не состоите в этой фракции");
                return;
            }
            if (!Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Notify.Error(player, "Склад закрыт");
                return;
            }
            Trigger.ClientEvent(player, "OpenStock_GANG");
                
        }
        public static void OpenBallasPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Ballas", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 2, Main.Players[player].FractionID, "Дарнелл Стивенс");
        }
        public static void OpenGroovePed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Groove", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 1, Main.Players[player].FractionID, "Фрэнк Грув");
        }
        public static void OpenVagosPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Vagos", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 3, Main.Players[player].FractionID, "Эмма Вагос");
        }
        public static void OpenCripsPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Crips", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 4, Main.Players[player].FractionID, "Адам Крипс");
        }
        public static void OpenBloodsPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Bloods", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 5, Main.Players[player].FractionID, "Мигель Бладс");
        }
        [RemoteEvent("GangBuyGuns")]
        public static void callback_gangguns(Player player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        if (Main.Players[player].FractionLVL < Main.Players[player].FractionID)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 120)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Pistol, 1));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 120;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Pistol, Weapons.GetSerial(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Пистолет");
                        return;
                    case 1:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 150)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd1 = nInventory.TryAdd(player, new nItem(ItemType.CompactRifle, 1));
                        if (tryAdd1 == -1 || tryAdd1 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 150;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.CompactRifle, Weapons.GetSerial(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Компактную винтовку");
                        return;
                    case 2:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd2 = nInventory.TryAdd(player, new nItem(ItemType.AssaultRifle, 1));
                        if (tryAdd2 == -1 || tryAdd2 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 200;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.AssaultRifle, Weapons.GetSerial(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Штурмовую Винтовку");
                        return;
                    case 3:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd4 = nInventory.TryAdd(player, new nItem(ItemType.PistolAmmo, 50));
                        if (tryAdd4 == -1 || tryAdd4 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 50;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.PistolAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 9x19mm");
                        return;
                    case 4:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd5 = nInventory.TryAdd(player, new nItem(ItemType.RiflesAmmo, 50));
                        if (tryAdd5 == -1 || tryAdd5 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 50;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.RiflesAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 62x39mm");
                        return;
                    case 5:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd7 = nInventory.TryAdd(player, new nItem(ItemType.RiflesAmmo, 50));
                        if (tryAdd7 == -1 || tryAdd7 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 50;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.RiflesAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 12ga Rifles");
                        return;
                    case 6:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 100)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd9 = nInventory.TryAdd(player, new nItem(ItemType.BodyArmor, 1));
                        if (tryAdd9 == -1 || tryAdd9 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 100;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, 50.ToString()));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 7:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd10 = nInventory.TryAdd(player, new nItem(ItemType.Gusenberg, 1));
                        if (tryAdd10 == -1 || tryAdd10 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 200;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Gusenberg, Weapons.GetSerial(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Автомат Томпсона");
                        return;
                    case 8:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 130)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd11 = nInventory.TryAdd(player, new nItem(ItemType.Revolver, 1));
                        if (tryAdd11 == -1 || tryAdd11 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 130;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Revolver, Weapons.GetSerial(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Револьвер");
                        return;
                }
            }
            catch (Exception e) { Log.Write("Fbigun: " + e.Message, nLog.Type.Error); }
        }
    }
}
