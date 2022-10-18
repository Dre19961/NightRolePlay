using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Lite.Core;
using LiteSDK;

namespace Lite.Jobs.FarmerJob
{

    class Market : Script
    {
        #region Settings
        private static Random rnd = new Random();

        private static nLog Log = new nLog("Market");

        public static int marketMultiplier;
        private static int _minMultiplier = 1;
        private static int _maxMultiplier = 5;

        public static void UpdateMultiplier()
        {
            marketMultiplier = rnd.Next(_minMultiplier, _maxMultiplier);
            Log.Write($"Обновлен коэффициент на: {marketMultiplier}");
        }

        private static List<Vector3> shape = new List<Vector3>()
        { 
            new Vector3(1961.7498, 5184.9155, 46.863087),
        };
        #endregion

        #region Инициализация Работы Фермера
        [ServerEvent(Event.ResourceStart)]
        public void Event_MarketStart()
        {
            try
            {
                #region Создание блипа, текста, колшейпа
                NAPI.Blip.CreateBlip(728, new Vector3(1961.7498, 5184.9155, 46.863087), 0.8f, 81, "Скупщик", 255, 0, true, 0, 0); // Блип на карте
                NAPI.TextLabel.CreateTextLabel("~w~Скупщик", new Vector3(1961.7498, 5184.9155, 48.963087), 10f, 0.2f, 4, new Color(35, 152, 247, 255), true, NAPI.GlobalDimension); // Над головой у Ped

                NAPI.Marker.CreateMarker(1, shape[0] + new Vector3(1, 0, 0), new Vector3(), new Vector3(), 0.5f, new Color(129, 159, 235, 255));
                var melogShape = NAPI.ColShape.CreateCylinderColShape(shape[0], 2f, 2, 0);
                melogShape.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 802);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), nLog.Type.Error);
                    }
                };
                melogShape.OnEntityExitColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), nLog.Type.Error);
                    }
                };
                #endregion
                UpdateMultiplier();
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Предметы в маркете
        //цена, номер предмета, название, предмет для покупки или для продажи (если true, то коэффициент будет умножаться на выставленную сумму)
        private static List<Product> SellItems = new List<Product>()
        {
            new Product(1500, 234, "Урожай", true),
            new Product(300, 235, "Семена", false),
        };

        private static List<Product> BuyItems = new List<Product>()
        {
            new Product(500, 235, "Семена", false),
        };
        #endregion

        #region Открыть меню Маркета
        [RemoteEvent("changePage")]
        public static void OpenMarketMenu(Player player, int page)
        {
            if (player.IsInVehicle) return;
            var hitem = nInventory.Find(Main.Players[player].UUID, ItemType.Hay);
            var shitem = nInventory.Find(Main.Players[player].UUID, ItemType.WheatSeeds);
            int hayscount = hitem != null ? hitem.Count : 0;
            int seedscount = shitem != null ? shitem.Count : 0;
            List<object> data = new List<object>()
            {
                marketMultiplier,
                hayscount,
                seedscount,
            };
            LoadPage(player, page, data);
        }
        #endregion

        #region Взаимодействие с менюшкой Маркета
        public static void LoadPage(Player player, int page, object data)
        {
            string json;
            switch (page)
            {
                case 0:
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    Trigger.ClientEvent(player, "loadPage", 0, json);
                    break;
                case 1:
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(BuyItems);
                    Trigger.ClientEvent(player, "loadPage", 1, json);
                    break;
                case 2:
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(SellItems);
                    Trigger.ClientEvent(player, "loadPage", 2, json);
                    break;
            }
        }
        #endregion

        #region BuyFarmerItem
        [RemoteEvent("buyFarmerItem")]
        public static void ButFarmerItem(Player player, int id, int count)
        {
            nItem aItem = new nItem((ItemType)id);
            var tryAdd = nInventory.TryAdd(player, new nItem(aItem.Type, count));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 2000);
                return;
            }
            var item = BuyItems.Find(x => x.ID == id);
            if (item == null)
            {
                Notify.Error(player, "Предмет не найден", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно денег", 2000);
                return;
            }
            MoneySystem.Wallet.Change(player, -price);
            nInventory.Add(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {count} {item.Name} за ${price}", 2000);
        }
        #endregion

        #region SellFarmerItem
        [RemoteEvent("sellFarmerItem")]
        public static void SellFarmerItem(Player player, int id, int count)
        {
            var aItem = nInventory.Find(Main.Players[player].UUID, (ItemType)id);
            if (aItem == null || aItem.Count < count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно предмета в инвентаре", 2000);
                return;
            }
            var item = SellItems.Find(x => x.ID == id);
            if(item == null)
            {
                Notify.Error(player, "Предмет не найден", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            MoneySystem.Wallet.Change(player, price);
            nInventory.Remove(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {count} {item.Name} за ${price}", 2000);
        }
        #endregion

        #region FarmerProduct
        private class Product
        {
            public int Price { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public bool Ordered { get; set; }

            public Product(int price, int id, string name, bool ordered)
            {
                Price = price;
                ID = id;
                Name = name;
                Ordered = ordered;
            }
        }
        #endregion
    }
}
