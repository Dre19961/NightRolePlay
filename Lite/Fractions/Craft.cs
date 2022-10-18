using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GTANetworkAPI;
using Lite.Core;
using LiteSDK;

namespace Lite.Fractions
{
    class Craft : Script
    {
        private static nLog Log = new nLog("Craft");
        #region RecourceStart
        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                foreach (KeyValuePair<int, Vector3> craft in CraftPositions)
                {
                    API.Shared.CreateTextLabel("Крафт оружия -~n~~n~~w~Нажмите [~s~E~w~], что бы открыть меню", craft.Value + new Vector3(0, 0, 0.2f), 15.0f, 0.9f, 4, new Color(110, 179, 70, 190));
                    NAPI.Marker.CreateMarker(1, craft.Value, new Vector3(), new Vector3(), 1f, new Color(255, 0, 255, 120), false, 0);
                    ColShape shape = NAPI.ColShape.CreateCylinderColShape(craft.Value, 1f, 2f, 0);
                    shape.OnEntityEnterColShape += (s, e) =>
                    {
                        e.SetData("INTERACTIONCHECK", 5997);
                        e.SetData("CRAFT_FRAC", craft.Key);
                    };
                    shape.OnEntityExitColShape += (s, e) =>
                    {
                        e.ResetData("CRAFT_FRAC");
                    };
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.ToString(), nLog.Type.Error); }
        }
        #endregion
        public static void OpenCraftMenu(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("CRAFT_FRAC")) return;
                if (player.GetData<int>("CRAFT_FRAC") != Main.Players[player].FractionID)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не состоите в этой фракции", 2500);
                    return;
                }
                Trigger.ClientEvent(player, "openCraftMenu");
            }
            catch (Exception e) { Log.Write("OpenCraftMenu: " + e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("CraftGun:Server")]
        public static void CraftGun_Server(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), Convert.ToString(arguments[0]));
                int fracId = Main.Players[player].FractionID;
                int rang = Convert.ToInt16(arguments[2]);
                if (Main.Players[player].FractionLVL < rang)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Это оружие можно крафтить с {rang} ранга", 2500);
                    return;
                }
                int mats = Convert.ToInt16(arguments[1]);
                if (Stocks.fracStocks[fracId].Materials < mats)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В вашей фракции недостаточно материалов", 2500);
                    return;
                }
                int tryAdd = nInventory.TryAdd(player, new nItem(wType));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, $"У вас недостаточно места в инвентаре.", 3000);
                    return;
                }
                Stocks.fracStocks[fracId].Materials -= mats;
                Stocks.fracStocks[fracId].UpdateLabel();
                if (!nInventory.AmmoItems.Contains(wType))
                    nInventory.Add(player, new nItem(wType, 1, Weapons.GetSerial(true, fracId)));
                else if (wType == ItemType.BodyArmor) {
                nInventory.Add(player, new nItem(ItemType.BodyArmor, 1));
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы скрафтили Бронежилет за {mats} материлов.", 3000);
                }
                else{
                    nInventory.Add(player, new nItem(wType, 30));
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы скрафтили {Convert.ToString(arguments[3])} за {mats} материлов.", 3000);
                }
            }
            catch (Exception e) { Log.Write("CraftGun_Server: " + e.ToString(), nLog.Type.Error); }
        }
        #region Pos
        private static Dictionary<int, Vector3> CraftPositions = new Dictionary<int, Vector3>
        {
            { 10, new Vector3(1405.786, 1138.1003, 109.745636) },
            { 11, new Vector3(-80.40103, 1002.3316, 230.60844) },
            { 12, new Vector3(-1051.181, 308.6515, 62.217113) },
            { 13, new Vector3(-1520.3016, 109.991554, 50.027348) },
        };
        #endregion
    }
}