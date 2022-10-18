using GTANetworkAPI;
using Lite.Core;
using LiteSDK;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lite.Fractions
{
    class GiveLic : Script
    {
        private static nLog RLog = new nLog("IssuanceLicences");
        private static GTANetworkAPI.ColShape shapeMed;
        private static GTANetworkAPI.Marker markerMed;
        private static GTANetworkAPI.TextLabel lableMed;
        private static Vector3 Med = new Vector3(357.37302, -1414.5204, 32.509308);
        private static GTANetworkAPI.ColShape shapeGun;
        private static GTANetworkAPI.Marker markerGun;
        private static GTANetworkAPI.TextLabel lableGun;
        private static Vector3 Gun = new Vector3(441.8187, -981.6356, 30.06968);
        private static int PriceMed = 15000; // цена на мед.карту
        private static int PriceGun = 25000; // цена на лицензию на оружие 

        [ServerEvent(Event.ResourceStart)]
        public static void EnterShapeRealtor()
        {
            try
            {
                #region Creating Marker & Colshape
                //мед.карта
                markerMed = NAPI.Marker.CreateMarker(1, Med - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.5f, new Color(66, 170, 255), false, 0);
                shapeMed = NAPI.ColShape.CreateCylinderColShape(Med - new Vector3(0, 0, 0.7), 0.5f, 2, 0);
                lableMed = NAPI.TextLabel.CreateTextLabel("~b~Получение мед.карты", Med + new Vector3(0, 0, 0.4), 5F, 0.3F, 0, new Color(255, 255, 255), true, 0);
                NAPI.TextLabel.CreateTextLabel("~w~Цена:15.000$~g~$", Med + new Vector3(0, 0, 0.2), 5F, 0.3F, 0, new Color(255, 255, 255), true, 0);
                shapeMed.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 228);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shapeMed.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                //Лицензия на оружие
                markerGun = NAPI.Marker.CreateMarker(1, Gun - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.5f, new Color(66, 170, 255), false, 0);
                shapeGun = NAPI.ColShape.CreateCylinderColShape(Gun - new Vector3(0, 0, 0.7), 0.5f, 2, 0);
                lableGun = NAPI.TextLabel.CreateTextLabel("~b~Получение лицензии на оружие", Gun + new Vector3(0, 0, 0.4), 5F, 0.3F, 0, new Color(255, 255, 255), true, 0);
                NAPI.TextLabel.CreateTextLabel("~w~Цена:25.000~g~$", Gun + new Vector3(0, 0, 0.2), 5F, 0.3F, 0, new Color(255, 255, 255), true, 0);
                shapeGun.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 229);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shapeGun.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                #endregion

                RLog.Write("Loaded", nLog.Type.Info);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }
        public static void MedLic(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Licenses[7])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть мед.карта.", 3000);
                    return;
                }
                if (!MoneySystem.Wallet.Change(player, -PriceMed))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств.", 3000);
                    return;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили мед.карту", 3000);
                Main.Players[player].Licenses[7] = true;
                GUI.Dashboard.sendStats(player);
            }
            catch (Exception e) { RLog.Write("GiveLic: " + e.Message, nLog.Type.Error); }
        }
        public static void GunLic(Player player)
        {
            try
            {

                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Licenses[6])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть лицензия на оружие.", 3000);
                    return;
                }
                if (!MoneySystem.Wallet.Change(player, -PriceGun))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств.", 3000);
                    return;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили лицензию на оружие.", 3000);
                Main.Players[player].Licenses[6] = true;
                GUI.Dashboard.sendStats(player);
            }
            catch (Exception e) { RLog.Write("GiveLic: " + e.Message, nLog.Type.Error); }
        }
    }
}