using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Lite.Core.nAccount;
using Lite.Core.Character;
using System.Linq;
using LiteSDK;

namespace Lite.Core
{
    class Bar : Script
    {
        private static nLog _log = new nLog("Bar");
        
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            new BarPoint("Vanila Unicorn", new Vector3(-239.2363, 244.215, 90.92064), 
                new List<BarItem>() { 
                    new BarItem("Алкогольный Напиток", "Алкоголь поможет на тулевке", ItemType.ArmDrink1, 200),
                }
            );
        }

        [RemoteEvent("server::bar:buy")]
        public static void RemoteEvent_Buy(Player player, int index)
        {
            getBar(player).Buy(player, index);
        }

        public static void Open(Player player)
        {
            getBar(player).Open(player);
        }

        public static BarPoint getBar(Player player)
        {
            if (!player.HasData("BAR_DATA")) return null;
            return player.GetData<BarPoint>("BAR_DATA");
        }

        public class BarPoint
        {
            public string Name { get; set; }
            public Vector3 Position { get; set; }
            public List<BarItem> Items { get; set; }

            [JsonIgnore]
            private Blip Blip;
            [JsonIgnore]
            private Marker Marker;
            [JsonIgnore]
            private ColShape ColShape;

            public BarPoint(string name, Vector3 pos, List<BarItem> items)
            {
                Name = name; Position = pos; Items = items;
                GTAElements();
            }
            
            public void GTAElements()
            {
                ColShape = NAPI.ColShape.CreateCylinderColShape(Position, 2f, 2f, 0);
                ColShape.OnEntityEnterColShape += (s, e) =>
                {
                    if (!e.IsInVehicle)
                    {
                        e.SetData("INTERACTIONCHECK", 2000);
                        e.SetData("BAR_DATA", this);
                    }
                };
                ColShape.OnEntityExitColShape += (s, e) =>
                {
                    if (!e.IsInVehicle)
                    {
                        e.SetData("INTERACTIONCHECK", 0);
                        e.ResetData("BAR_DATA");
                        Trigger.ClientEvent(e, "client::bar:close");
                    }
                };

                Marker = NAPI.Marker.CreateMarker(1, Position, new Vector3(), new Vector3(), 0.8f, new Color(67, 140, 239), false, 0);
                Blip = NAPI.Blip.CreateBlip(93, Position, 0.8f, 4, Name, 255, 0, true, 0, 0);
            }

            public void Open(Player player)
            {
                Trigger.ClientEvent(player, "client::bar:open", JsonConvert.SerializeObject(Items), Name.ToString());
            }

            public void Buy(Player player, int index)
            {
                BarItem barItem = Items[index];
                if (barItem != null)
                {
                    if (Main.Players[player].Money < barItem.Price)
                    {
                        Notify.Error(player, "Недостаточно средств");
                        return;
                    }
                    nItem nItem = new nItem(barItem.ItemType, 1);
                    var tryAdd = nInventory.TryAdd(player, nItem);
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Error(player, "Недостаточно места в инвентаре", 3000);
                        return;
                    }
                    nInventory.Add(player, nItem);
                    Notify.Succ(player, $"Вы купили {barItem.Name} за {barItem.Price}$");
                }
            }
        }
        public class BarItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public ItemType ItemType { get; set; }
            public int Price { get; set; }
            public BarItem(string name, string desc, ItemType item, int price)
            {
                Name = name; Description = desc; ItemType = item; Price = price;
            }
        }
    }
}
