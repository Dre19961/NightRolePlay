using GTANetworkAPI;
using Newtonsoft.Json;
using Lite.Core;
using LiteSDK;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace Lite.Core
{
    class CaseController : Script
    {
        public static Dictionary<int, List<CaseItem>> CasesPrizeList = new Dictionary<int, List<CaseItem>>();

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                DataTable result = MySQL.QueryRead($"SELECT * FROM `cases`");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    int UUID = Convert.ToInt32(Row["UUID"]);
                    string Cases = Row["Prize"].ToString();
                    List<CaseItem> ListCases = JsonConvert.DeserializeObject<List<CaseItem>>(Cases);
                    CasesPrizeList.Add(UUID, ListCases);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static Dictionary<String, Case> Cases = new Dictionary<String, Case>
        {

            {"DEFCASE",new Case(new Dictionary<int, CaseItem>
                {
                // { 1, new CaseItem(по счету, "Название Сферы", Раритетность, Шанс выпадения, Цена продажи,  ,"моделнейм", "название" , " Картинка" , Дефолт Шанс, Еще какой-то Шанс) },
                    { 1, new CaseItem(0,"Money",1,1,0,"1500000","8.000.000$","money1.png",0.1,0.1) },
                    { 2, new CaseItem(1,"Money",2,1,0,"3000000","20.000.000$","money1.png",0.1,0.1) },
                    { 3, new CaseItem(2,"DonatePoint",1,5,0,"50","50 NC","donate.png",0.1,0.1) },
                    { 4, new CaseItem(3,"DonatePoint",1,10,0,"100","100 NC","donate.png",0.1,0.1) },
                    { 5, new CaseItem(4,"DonatePoint",2,11,0,"250","250 NC","donate.png",0.1,0.1) },
                    { 6, new CaseItem(5,"Vip",2,6,300,"1","VIP Saphire","vip1.png",0.1,0.1) },
                    { 7, new CaseItem(6,"Vip",2,6,500,"2","VIP Ruby","vip2.png",0.1,0.1) },

                    { 8, new CaseItem(7,"Car",3,50,50,"18rs7","Audi RS7","rs7.png",0.5,0.5) },
                    { 9, new CaseItem(8,"Car",3,50,300,"divo","Bugatti Divo","divo.png",0.5,0.5) },
                })
            },
            {"VTOROI",new Case(new Dictionary<int, CaseItem>
                {
                    { 1, new CaseItem(0,"Money",1,90,0,"30000","100.000.000$","money1.png",1,1) },
                    { 2, new CaseItem(1,"Money",1,90,0,"60000","150.000.000$","money1.png",1,1) },
                    { 3, new CaseItem(2,"Money",1,90,0,"60000","250.000.000$","money1.png",1,1) },
                    { 4, new CaseItem(3,"Money",1,90,0,"60000","350.000.000$","money1.png",1,1) },
                    { 5, new CaseItem(4,"DonatePoint",2,90,0,"100","100 NC","donate.png",1,1) },
                    { 6, new CaseItem(5,"DonatePoint",2,90,0,"250","250 NC","donate.png",1,1) },
                    { 7, new CaseItem(6,"DonatePoint",2,90,0,"560","560 NC","donate.png",1,1) },
                    { 8, new CaseItem(7,"Vip",2,90,200,"2","VIP Ruby","vip.png",1,1) },
                    { 9, new CaseItem(8,"Vip",2,90,500,"3","VIP Diamond","vip.png",1,1) },

                    
                    { 10, new CaseItem(9,"Car",3,20,300,"taycants21m","Porche Taycan Turbo S","taycan21.png",0.5,0.5) },
                    { 11, new CaseItem(10,"Car",3,20,100,"avtr","Mersedes-Benz AVTR","avtr.png",0.5,0.5) },
                    { 12, new CaseItem(11,"Car",3,20,400,"bolide","Bugatti Bolide","bolide.png",0.5,0.5) },
                    { 13, new CaseItem(12,"Car",3,20,50,"r1m","Yamaha R1","r1m.png",0.5,0.5) },
                    { 14, new CaseItem(13,"Car",3,20,50,"g63","Mersedes Benz G63","g63.png",0.5,0.5) },
                    { 15, new CaseItem(14,"Car",3,20,50,"huracan","Lamborghini huracan","huracan.png",0.5,0.5) },
                    { 16, new CaseItem(15,"Car",3,20,50,"jesko","Jesko","jesko.png",0.5,0.5) },
                    { 17, new CaseItem(16,"Car",3,20,50,"lada2170","Lada 2170","lada2170.png",0.5,0.5) },
                    { 18, new CaseItem(17,"Car",3,20,300,"mvisiongt","Vision GT","visiongt.png",0.5,0.5) },
                    { 19, new CaseItem(18,"Car",3,20,300,"divo","Bugatti Divo","divo.png",0.5,0.5) },


                })
            }
        };
        [RemoteEvent("r:GetCase")]
        public void GetCase(Player player, int id)
        {
            try
            {
                List<CaseItem> Case = new List<CaseItem>();
                switch (id)
                {
                    case 1:
                        {
                            Case = Cases["DEFCASE"].Citems;
                        }
                        break;
                    case 2:
                        {
                            Case = Cases["VTOROI"].Citems;
                        }
                        break;
                }
                Trigger.ClientEvent(player, "r:setcase", JsonConvert.SerializeObject(Case));
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        [RemoteEvent("r:getWinId")]
        public void GetWinIdCase(Player player, string type, int caseid)
        {
            try
            {
                string name = GetCaseName(player, caseid);
                int id = -1;
                switch (name)
                {
                    case "DEFCASE":
                        if (Main.Accounts[player].RedBucks < 300)
                        {
                            Notify.Error(player, "Недостаточно средств");
                            return;
                        }
                        Main.Accounts[player].RedBucks -= 300;
                        id = Cases["DEFCASE"].GetRandom().id;
                        Trigger.ClientEvent(player, "r:getWinIdCallback", id, type);
                        return;
                    case "VTOROI":
                        if (Main.Accounts[player].RedBucks < 700)
                        {
                            Notify.Error(player, "Недостаточно средств");
                            return;
                        }
                        Main.Accounts[player].RedBucks -= 700;
                        id = Cases["VTOROI"].GetRandom().id;
                        Trigger.ClientEvent(player, "r:getWinIdCallback", id, type);
                        return;
                }
                Trigger.ClientEvent(player, "redset", Main.Accounts[player].RedBucks);
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                Trigger.ClientEvent(player, "r:getWinIdCallback", id, type);
                Trigger.ClientEvent(player, "SetRedBucksInMenu", Main.Accounts[player].RedBucks);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        [RemoteEvent("r:getPrize")]
        public void getPrize(Player player, string type, int id, int caseid)
        {
            try
            {
                string casename = GetCaseName(player, caseid);
                if (type != "sellList" && type != "take" && Cases[casename].Citems[id].rarity == 4)
                {
                    NAPI.Chat.SendChatMessageToAll($"Игрок {player.Name} ~r~Выбил с Кейса: {Cases[casename].Citems[id].title}");
                }
                switch (type)
                {
                    case "get":
                        {
                            GetCaseItem(player, id, casename);
                        }
                        break;
                    case "sell":
                        {
                            Main.Accounts[player].RedBucks += Cases[casename].Citems[id].cost;
                            Trigger.ClientEvent(player, "SetRedBucksInMenu", Main.Accounts[player].RedBucks);
                        }
                        break;
                    case "sellList":
                        {
                            Main.Accounts[player].RedBucks += CasesPrizeList[Main.Players[player].UUID][id].cost;
                            CasesPrizeList[Main.Players[player].UUID].RemoveAt(id);
                            SendCaseList(player);
                            Trigger.ClientEvent(player, "SetRedBucksInMenu", Main.Accounts[player].RedBucks);
                        }
                        break;
                    case "take":
                        {
                            if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID) || CasesPrizeList[Main.Players[player].UUID].ElementAt(id) == null) return;
                            if (CasesPrizeList[Main.Players[player].UUID][id].Type == "Vip")
                            {
                                if (Main.Accounts[player].VipLvl >= 1)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас уже есть VIP статус!", 3000);
                                    return;
                                }
                            }
                            RouletteTakeItem(player, CasesPrizeList[Main.Players[player].UUID][id]);
                            CasesPrizeList[Main.Players[player].UUID].RemoveAt(id);
                            SendCaseList(player);
                        }
                        break;
                }
                Trigger.ClientEvent(player, "redset", Main.Accounts[player].RedBucks);
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void RouletteTakeItem(Player player, CaseItem itemtype)
        {
            try
            {
                switch (itemtype.Type)
                {
                    case "DonatePoint":
                        Main.Accounts[player].RedBucks += itemtype.cost;
                        break;
                    case "Car":
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null)
                        {
                            var vNumber = VehicleManager.Create(player.Name, itemtype.carname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {itemtype.title} с номером {vNumber}", 5000);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена на стоянку", 5000);
                        }
                        else
                        {
                            var garage = Houses.GarageManager.Garages[house.GarageID];
                            var vNumber = VehicleManager.Create(player.Name, itemtype.carname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {itemtype.title} с номером {vNumber}", 5000);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена в Ваш гараж", 5000);
                            garage.SpawnCar(vNumber);
                        }
                        break;
                    case "Clothes":
                        Customization.AddClothes(player, (ItemType)Convert.ToInt32(itemtype.carname.Split("_")[0]), Convert.ToInt32(itemtype.carname.Split("_")[1]), Convert.ToInt32(itemtype.carname.Split("_")[2]));
                        break;
                    case "EXP":
                        Main.Players[player].EXP += Convert.ToInt32(itemtype.carname) * Group.GroupEXP[Main.Accounts[player].VipLvl] * Main.oldconfig.ExpMultiplier;
                        break;
                    case "Money":
                        MoneySystem.Wallet.Change(player, Convert.ToInt32(itemtype.carname));
                        break;
                    case "Vip":
                        Main.Accounts[player].VipLvl = Convert.ToInt32(itemtype.carname);
                        Main.Accounts[player].VipDate = DateTime.Now.AddDays(30);
                        GUI.Dashboard.sendStats(player);
                        break;

                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void GetCaseItem(Player player, int id, string casename)
        {
            try
            {
                if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID))
                {
                    List<CaseItem> item = new List<CaseItem>();
                    item.Add(Cases[casename].Citems[id]);
                    CasesPrizeList.Add(Main.Players[player].UUID, item);
                }
                else
                {
                    CasesPrizeList[Main.Players[player].UUID].Add(Cases[casename].Citems[id]);
                }
                MySQL.Query($"UPDATE `cases` SET `Prize`='{JsonConvert.SerializeObject(CasesPrizeList[Main.Players[player].UUID])}' WHERE `uuid`={Main.Players[player].UUID}");
                SendCaseList(player);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public string GetCaseName(Player player, int idcase)
        {

            string casename = null;
            switch (idcase)
            {
                case 1:
                    {
                        casename = "DEFCASE";
                    }
                    break;
                case 2:
                    {
                        casename = "VTOROI";
                    }
                    break;
            }
            return casename;
        }
        [RemoteEvent("r:SendCasePrize")]
        public static void SendCaseList(Player player)
        {
            try
            {
                if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID))
                {
                    List<CaseItem> item = new List<CaseItem>();
                    CasesPrizeList.Add(Main.Players[player].UUID, item);
                }
                Trigger.ClientEvent(player, "r:SendCasePrize", JsonConvert.SerializeObject(CasesPrizeList[Main.Players[player].UUID]), Main.Accounts[player].RedBucks);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public class CaseItem
        {
            public Int32 id { get; private set; }
            public string Type { get; private set; }
            public Int32 rarity { get; private set; }
            public Int32 chance { get; private set; }
            public Int32 cost { get; private set; }
            public String title { get; private set; }
            public String carname { get; private set; }
            public String background { get; private set; }
            public Double DefaultChance { get; private set; }
            public Double StreamerChance { get; private set; }
            public CaseItem(Int32 ID, String Type, Int32 rarity = 1, Int32 chance = 1, Int32 cost = 1, String carname = "", String Title = "Default", String background = "Default", Double DefaultChance = 0.0001, Double StreamerChance = 0.0001)
            {
                this.id = ID;
                this.Type = Type;
                this.rarity = rarity;
                this.chance = chance;
                this.cost = cost;
                this.title = Title;
                this.carname = carname;
                this.background = background;
                this.DefaultChance = DefaultChance;
                this.StreamerChance = StreamerChance;
            }
        }
        public class Case
        {
            private Dictionary<(Int32 Min, Int32 Max), CaseItem> Items { get; set; }
            private Int32 ChanceLenght { get; set; }
            public List<CaseItem> Citems { get; set; }
            public Case(Dictionary<Int32, CaseItem> Items)
            {
                this.Citems = new List<CaseItem>();
                this.ChanceLenght = 0;
                this.Items = new Dictionary<(Int32 Min, Int32 Max), CaseItem>();
                foreach (KeyValuePair<Int32, CaseItem> Item in Items)
                {
                    this.Citems.Add(Item.Value);
                    this.Items.Add((this.ChanceLenght, this.ChanceLenght + Item.Key), Item.Value);
                    this.ChanceLenght += Item.Key;
                }
            }
            public CaseItem GetRandom()
            {
                Int32 Random = new Random().Next(1, this.ChanceLenght);
                foreach (KeyValuePair<(Int32 Min, Int32 Max), CaseItem> Item in this.Items) if (Item.Key.Min <= Random && Random <= Item.Key.Max) return Item.Value;
                return null;
            }
        }
    }
}
