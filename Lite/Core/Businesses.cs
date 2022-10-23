using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Lite.GUI;
using Lite.MoneySystem;
using LiteSDK;
using Lite.Fractions;

namespace Lite.Core
{
    class BusinessManager : Script
    {
        private static nLog Log = new nLog("BusinessManager");
        private static int lastBizID = -1;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM businesses");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB biz return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    Vector3 enterpoint = JsonConvert.DeserializeObject<Vector3>(Row["enterpoint"].ToString());
                    Vector3 unloadpoint = JsonConvert.DeserializeObject<Vector3>(Row["unloadpoint"].ToString());

                    Business data = new Business(Convert.ToInt32(Row["id"]), Row["owner"].ToString(), Convert.ToInt32(Row["sellprice"]), Convert.ToInt32(Row["type"]), JsonConvert.DeserializeObject<List<Product>>(Row["products"].ToString()), enterpoint, unloadpoint, Convert.ToInt32(Row["money"]),
                        Convert.ToInt32(Row["mafia"]), JsonConvert.DeserializeObject<List<Order>>(Row["orders"].ToString()));
                    var id = Convert.ToInt32(Row["id"]);
                    lastBizID = id;

                    if (data.Type == 0)
                    {
                        if (data.Products.Find(p => p.Name == "Связка ключей") == null)
                        {
                            Product product = new Product(ProductsOrderPrice["Связка ключей"], 0, 0, "Связка ключей", false);
                            data.Products.Add(product);
                            Log.Write($"product Связка ключей was added to {data.ID} biz");
                        }
                        data.Save();
                    }
                    BizList.Add(id, data);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"BUSINESSES\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void setProductForAllBiz(Player player)
        {
            if (!Group.CanUseCmd(player, "createbusiness")) return;

            foreach (var b in BizList)
            {
                var biz = BizList[b.Key];

                foreach (var prods in biz.Products)
                {
                    prods.Lefts = 10000;
                }
            }

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заполнили все бизнесы продуктами.", 3000);
            Log.Write("All Businesses has been were filled", nLog.Type.Success);
        }

        public static void SavingBusiness()
        {
            foreach (var b in BizList)
            {
                var biz = BizList[b.Key];
                biz.Save();
            }
            Log.Write("Businesses has been saved to DB", nLog.Type.Success);
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            try
            {
                SavingBusiness();
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static Dictionary<int, Business> BizList = new Dictionary<int, Business>();
        public static Dictionary<int, int> Orders = new Dictionary<int, int>(); // key - ID заказа, value - ID бизнеса

        public static List<string> BusinessTypeNames = new List<string>()
        {
            "Магазин 24/7", // 0
	        "АЗС", // 1
	        "Салон транспорта", // 2
	        "Мотосалон", // 3
	        "Магазин оружия", // 4
	        "Магазин одежды", // 5
	        "Тату-салон", // 6
	        "Парикмахерская", // 7
	        "Магазин масок", // 8
	        "Тюнинг", // 9
	        "Автомойка", // 10
	        "Магазин животных", // 11
	        "Рыболовный магазин", // 12
	        "Скупка рыбы", // 13
            "Казино", // 14
        };
        public static List<int> BlipByType = new List<int>()
        {
            52,  // Магазин 24/7
            361, // Заправка
            530, // Автосалон
            661, // Мотосалон
            110, // Мазин оружия
            73,  // Магазин одежды
            75,  // Тату-салон
            71,  // Стрижки
            442, // Магазин масок
            72,  // Тюнинг
            100, // Мойка авто
            251, // aero shop
            371, // Рыболовный магазин
            628, // Скупка рыбы
            679, // Казино
        };

        public static List<int> BlipColorByType = new List<int>()
        {
            45, // Магазин 24/7
            62, // Заправка
            45, // Автосалон
            45, // Донат Автосалон
            0, // Мазин оружия
            26, // Магазин одежды
            8,  // Тату-салон
            45, // Стрижки
            44, // Магазин масок
            0, // Тюнинг
            17, // Мойка авто
            15, // aero shop
            3,  // Рыболовный магазин
            3,  // Скупка рыбы
            45, // Казино
        };

        private static List<string> FishProducts = new List<string>()
        {
            "Удочка",
            "Улучшенная удочка",
            "Удочка MK2",
            "Наживка",
        };

        private static List<string> SellProducts = new List<string>()
        {
            "Корюшка",
            "Кунджа",
            "Лосось",
            "Окунь",
            "Осётр",
            "Скат",
            "Тунец",
            "Угорь",
            "Чёрный амур",
            "Щука",
        };

        public static List<string> PetNames = new List<string>() {
            "Husky",
            "Poodle",
            "Pug",
            "Retriever",
            "Rottweiler",
            "Shepherd",
            "Westy",
            "Cat",
            "Rabbit",
        };
        public static List<int> PetHashes = new List<int>() {
            1318032802, // Husky
            1125994524,
            1832265812,
            882848737, // Retriever
            -1788665315,
            1126154828,
            -1384627013,
            1462895032,
            -541762431,
        };
        public static List<List<string>> CarsNames = new List<List<string>>()
        {
                        new List<string>() // premium
            {
                 "18rs7",
                "2019m5",
                "amggt16",
                "bentayga",
                "bentley20",
                "bmw730",
                "bmwm2",
                "bmwz4",
                "gta5rp_veh_c63s",
                "c63scoupe",
                "camry70",
                "carrera19",
                "cls17",
                "evo9",
                "evo10",
                "f12",
                "gta5rp_veh_ferrari19",
                "gallardo",
                "giulia",
                "golf7",
                "huracan",
                "gta5rp_veh_impreza98",
                "impreza18",
                "m8",
                "maybach",
                "gta5rp_veh_minic",
                "models",
                "mustang19",
                "mustang65",
                "p90d",
                "panamera",
                "pullman",
                "rrghost",
                "rrwraith",
                "rs6",
                "teslaroad",
                "gt63",
                "bmwg05",
                "bmwg07",
                "cayen19",
                "cullinan",
                "escalade19",
                "gta5rp_veh_fx50s",
                "g63",
                "gl63",
                "gta5rp_veh_gle1",
                "jeep15",
                "lc200",
                "merc6x6",
                "navigator19",
                "rrover17",
                "suburban800",
                "svr",
                "gta5rp_veh_tahoe1",
                "urus",
                "gta5rp_veh_wald2018",
                "gta5rp_veh_x5e53",
                "x6m18",
                "gta5rp_veh_r50",
                "a45",
                "a80",
                "a90",
                "ae86",
                "bmwe28",
                "bmwe34",
                "bmwe36",
                "bmwe38",
                "bmwe39",
                "bmwe46",
                "gta5rp_veh_cam08",
                "camry35",
                "cls08",
                "e55",
                "evo6",
                "golf1",
                "gta5rp_veh_gtr33",
                "gta5rp_veh_gtr34",
                "impreza08",
                "kiastinger",
                "gta5rp_veh_m5e60",
                "mark100",
                "gta5rp_veh_nisr32",
                "nsx95",
                "s15",
                "w140",
                "chiron",
                "gemera",
                "maseratigt",
                "f150",
                "vulcan",
                "phuayra",
                "senna",
                "v250",
                "veyron",
                
                "i8",
                "exp100",
                "minifiat",
                "minicar",
                "agerars",
                
                "jesko",
                "rcf",
                "lfa",
                "slr",
                "cyber",
                "240z",
                "350z",
                
                "acursx",
               
                "carreragt",
                "civic19",
                "db11",
                "deborah",
                "elemento",
                "fordraptor",
                "ftype15",
                "gt17",
                "gtr17",
                "gtr2000",
                "impala67",
                "lambsc18",
                
                "mc720s",
                "mclaren20",
                
                "nsx18",
               
                "regera",
                "s13",
                "rx7",
                "sian",
                "sl300",
                "smalljoe",
                "veneno",
                "vision6",
                "zl1",
                "lykan",
                "avtr",
                "lada2170",
                "m4comp",
                "mlbrabus",
                "rs5",
                "taycan",
                "ocnauda8l22h",
                "180sx",
                "harley1",
                "r1m",
                "rcv213",
                "r111",
                "r62008",
                "r62010",
                "fireblade",
                "gsx18",
                "panigale",
                "zx10rr",


            }, // premium
            new List<string>() // moto
            {
               
                
            }, // donate
            new List<string>() // aero room
            {
                "Buzzard2",
                "918",
                "Mammatus",
                "Luxor2",
                "divo",
                "caliburn",
                "ocnrrvn100",
                
                "taycants21m",
                "rx7veilside",
                "wycalt",
                "monza",
                "venenor",
                "rsq8m",
                "school",
                "deluxo",
                "lada2107",
                "mvisiongt",
                "rt70",
                "bolide",
                "ffrs",
                "mercedesgls",

            }, // aero room
        };
        private static List<string> GunNames = new List<string>()
        {
            "Pistol",
            "CombatPistol",
            "Revolver",
            "HeavyPistol",

            "BullpupShotgun",

            "CombatPDW",
            "MachinePistol",
        };
        private static List<string> MarketProducts = new List<string>()
        {
            "Монтировка",
            "Фонарик",
            "Молоток",
            "Гаечный ключ",
            "Канистра бензина",
            "Чипсы",
            "Вода",
            "Пицца",
            "Сим-карта",
            "Связка ключей",
            "Рем. Комплект",
            "Аптечка",
            "Sprunk",
            "eCola",
            "Бонг",
            "Зажигалка"
        };

        public static List<List<BusinessTattoo>> BusinessTattoos = new List<List<BusinessTattoo>>()
        {
            // Torso
            new List<BusinessTattoo>()
            {
	            // Левый сосок  -   0
                // Правый сосок -   1
                // Живот        -   2
                // Левый низ спины    -   3
	            // Правый низ спины    -   4
                // Левый верх спины   -   5
                // Правый верх спины   -   6
                // Левый бок    -   7
                // Правый бок   -   8
                // Не работают: Skull of Suits, 
                //Новое
                new BusinessTattoo(new List<int>(){0,1},"In the Pocket", "mpvinewood_overlays", "MP_Vinewood_Tat_000_M", "MP_Vinewood_Tat_000_F",1500),
                new BusinessTattoo(new List<int>(){5,6}, "Jackpot", "mpvinewood_overlays", "MP_Vinewood_Tat_001_M", "MP_Vinewood_Tat_001_F",1350),
                new BusinessTattoo(new List<int>(){2}, "USSR", "gta5rptattoo_overlays", "mp_gta5rptattoo_tat_159_M", "mp_gta5rptattoo_tat_169",1350),
                new BusinessTattoo(new List<int>(){0}, "Royal Flush", "mpvinewood_overlays", "MP_Vinewood_Tat_003_M", "MP_Vinewood_Tat_003_F",1700),    new BusinessTattoo(new List<int>(){5,6}, "Wheel of Suits", "mpvinewood_overlays", "MP_Vinewood_Tat_006_M", "MP_Vinewood_Tat_006_F",2750),   new BusinessTattoo(new List<int>(){5,6}, "777", "mpvinewood_overlays", "MP_Vinewood_Tat_007_M", "MP_Vinewood_Tat_007_F",7777),  new BusinessTattoo(new List<int>(){3,4,5,6}, "Snake Eyes", "mpvinewood_overlays", "MP_Vinewood_Tat_008_M", "MP_Vinewood_Tat_008_F",3500),   new BusinessTattoo(new List<int>(){3,4,5,6}, "Till Death Do Us Part", "mpvinewood_overlays", "MP_Vinewood_Tat_009_M", "MP_Vinewood_Tat_009_F",2000),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Photo Finish", "mpvinewood_overlays", "MP_Vinewood_Tat_010_M", "MP_Vinewood_Tat_010_F",2550), new BusinessTattoo(new List<int>(){3,4,5,6}, "Life's a Gamble", "mpvinewood_overlays", "MP_Vinewood_Tat_011_M", "MP_Vinewood_Tat_011_F",4500),  new BusinessTattoo(new List<int>(){2}, "Skull of Suits", "mpvinewood_overlays", "MP_Vinewood_Tat_012_M", "MP_Vinewood_Tat_012_F",2750), new BusinessTattoo(new List<int>(){3,4,5,6}, "The Jolly Joker", "mpvinewood_overlays", "MP_Vinewood_Tat_015_M", "MP_Vinewood_Tat_015_F",2000),  new BusinessTattoo(new List<int>(){2}, "Rose & Aces", "mpvinewood_overlays", "MP_Vinewood_Tat_016_M", "MP_Vinewood_Tat_016_F",3000),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Roll the Dice", "mpvinewood_overlays", "MP_Vinewood_Tat_017_M", "MP_Vinewood_Tat_017_F",1550),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Show Your Hand", "mpvinewood_overlays", "MP_Vinewood_Tat_021_M", "MP_Vinewood_Tat_021_F",1750),   new BusinessTattoo(new List<int>(){1}, "Blood Money", "mpvinewood_overlays", "MP_Vinewood_Tat_022_M", "MP_Vinewood_Tat_022_F",2550),    new BusinessTattoo(new List<int>(){0,1}, "Lucky 7s", "mpvinewood_overlays", "MP_Vinewood_Tat_023_M", "MP_Vinewood_Tat_023_F",3550), new BusinessTattoo(new List<int>(){2}, "Cash Mouth", "mpvinewood_overlays", "MP_Vinewood_Tat_024_M", "MP_Vinewood_Tat_024_F",5000), new BusinessTattoo(new List<int>(){3,4,5,6}, "The Table", "mpvinewood_overlays", "MP_Vinewood_Tat_029_M", "MP_Vinewood_Tat_029_F",3550),    new BusinessTattoo(new List<int>(){3,4,5,6}, "The Royals", "mpvinewood_overlays", "MP_Vinewood_Tat_030_M", "MP_Vinewood_Tat_030_F",2550),   new BusinessTattoo(new List<int>(){2}, "Gambling Royalty", "mpvinewood_overlays", "MP_Vinewood_Tat_031_M", "MP_Vinewood_Tat_031_F",4750),   new BusinessTattoo(new List<int>(){3,4,5,6}, "Play Your Ace", "mpvinewood_overlays", "MP_Vinewood_Tat_032_M", "MP_Vinewood_Tat_032_F",6000),    new BusinessTattoo(new List<int>(){2}, "Refined Hustler", "mpbusiness_overlays", "MP_Buis_M_Stomach_000", "",3000), new BusinessTattoo(new List<int>(){1}, "Rich", "mpbusiness_overlays", "MP_Buis_M_Chest_000", "",1750),  new BusinessTattoo(new List<int>(){0}, "$$$", "mpbusiness_overlays", "MP_Buis_M_Chest_001", "",1750),   new BusinessTattoo(new List<int>(){3,4}, "Makin' Paper", "mpbusiness_overlays", "MP_Buis_M_Back_000", "",2000), new BusinessTattoo(new List<int>(){0,1}, "High Roller", "mpbusiness_overlays", "", "MP_Buis_F_Chest_000",1750), new BusinessTattoo(new List<int>(){0,1}, "Makin' Money", "mpbusiness_overlays", "", "MP_Buis_F_Chest_001",2500),    new BusinessTattoo(new List<int>(){1}, "Love Money", "mpbusiness_overlays", "", "MP_Buis_F_Chest_002",1750),    new BusinessTattoo(new List<int>(){2}, "Diamond Back", "mpbusiness_overlays", "", "MP_Buis_F_Stom_000",3000),   new BusinessTattoo(new List<int>(){8}, "Santo Capra Logo", "mpbusiness_overlays", "", "MP_Buis_F_Stom_001",2000),   new BusinessTattoo(new List<int>(){8}, "Money Bag", "mpbusiness_overlays", "", "MP_Buis_F_Stom_002",2000),  new BusinessTattoo(new List<int>(){3,4}, "Respect", "mpbusiness_overlays", "", "MP_Buis_F_Back_000",2000),  new BusinessTattoo(new List<int>(){3,4}, "Gold Digger", "mpbusiness_overlays", "", "MP_Buis_F_Back_001",2500),  new BusinessTattoo(new List<int>(){3,4,5,6}, "Carp Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_005", "MP_Xmas2_F_Tat_005",6250), new BusinessTattoo(new List<int>(){3,4,5,6}, "Carp Shaded", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_006", "MP_Xmas2_F_Tat_006",6250),  new BusinessTattoo(new List<int>(){1}, "Time To Die", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_009", "MP_Xmas2_F_Tat_009",1250),    new BusinessTattoo(new List<int>(){5,6}, "Roaring Tiger", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_011", "MP_Xmas2_F_Tat_011",2250),    new BusinessTattoo(new List<int>(){7}, "Lizard", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_013", "MP_Xmas2_F_Tat_013",2000), new BusinessTattoo(new List<int>(){5,6}, "Japanese Warrior", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_015", "MP_Xmas2_F_Tat_015",2100), new BusinessTattoo(new List<int>(){0}, "Loose Lips Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_016", "MP_Xmas2_F_Tat_016",1750), new BusinessTattoo(new List<int>(){0}, "Loose Lips Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_017", "MP_Xmas2_F_Tat_017",1750),   new BusinessTattoo(new List<int>(){0,1}, "Royal Dagger Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_018", "MP_Xmas2_F_Tat_018",2500), new BusinessTattoo(new List<int>(){0,1}, "Royal Dagger Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_019", "MP_Xmas2_F_Tat_019",2500),   new BusinessTattoo(new List<int>(){2,8}, "Executioner", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_028", "MP_Xmas2_F_Tat_028",2000),  new BusinessTattoo(new List<int>(){5,6}, "Bullet Proof", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_000_M", "MP_Gunrunning_Tattoo_000_F",2000), new BusinessTattoo(new List<int>(){3,4}, "Crossed Weapons", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_001_M", "MP_Gunrunning_Tattoo_001_F",2000),  new BusinessTattoo(new List<int>(){5,6}, "Butterfly Knife", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_009_M", "MP_Gunrunning_Tattoo_009_F",2250),  new BusinessTattoo(new List<int>(){2}, "Cash Money", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_010_M", "MP_Gunrunning_Tattoo_010_F",3000), new BusinessTattoo(new List<int>(){1}, "Dollar Daggers", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_012_M", "MP_Gunrunning_Tattoo_012_F",1750), new BusinessTattoo(new List<int>(){5,6}, "Wolf Insignia", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_013_M", "MP_Gunrunning_Tattoo_013_F",2250),    new BusinessTattoo(new List<int>(){5,6}, "Backstabber", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_014_M", "MP_Gunrunning_Tattoo_014_F",2250),  new BusinessTattoo(new List<int>(){0,1}, "Dog Tags", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_017_M", "MP_Gunrunning_Tattoo_017_F",2500), new BusinessTattoo(new List<int>(){3,4}, "Dual Wield Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_018_M", "MP_Gunrunning_Tattoo_018_F",2250), new BusinessTattoo(new List<int>(){5,6}, "Pistol Wings", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_019_M", "MP_Gunrunning_Tattoo_019_F",2250), new BusinessTattoo(new List<int>(){0,1}, "Crowned Weapons", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_020_M", "MP_Gunrunning_Tattoo_020_F",2500),  new BusinessTattoo(new List<int>(){5}, "Explosive Heart", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_022_M", "MP_Gunrunning_Tattoo_022_F",1750),    new BusinessTattoo(new List<int>(){0,1}, "Micro SMG Chain", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_028_M", "MP_Gunrunning_Tattoo_028_F",2500),  new BusinessTattoo(new List<int>(){2}, "Win Some Lose Some", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_029_M", "MP_Gunrunning_Tattoo_029_F",3000), new BusinessTattoo(new List<int>(){5,6}, "Crossed Arrows", "mphipster_overlays", "FM_Hip_M_Tat_000", "FM_Hip_F_Tat_000",2250),  new BusinessTattoo(new List<int>(){1}, "Chemistry", "mphipster_overlays", "FM_Hip_M_Tat_002", "FM_Hip_F_Tat_002",1750), new BusinessTattoo(new List<int>(){7}, "Feather Birds", "mphipster_overlays", "FM_Hip_M_Tat_006", "FM_Hip_F_Tat_006",200),  new BusinessTattoo(new List<int>(){5,6}, "Infinity", "mphipster_overlays", "FM_Hip_M_Tat_011", "FM_Hip_F_Tat_011",2250),    new BusinessTattoo(new List<int>(){5,6}, "Antlers", "mphipster_overlays", "FM_Hip_M_Tat_012", "FM_Hip_F_Tat_012",2250), new BusinessTattoo(new List<int>(){0,1}, "Boombox", "mphipster_overlays", "FM_Hip_M_Tat_013", "FM_Hip_F_Tat_013",2500), new BusinessTattoo(new List<int>(){6}, "Pyramid", "mphipster_overlays", "FM_Hip_M_Tat_024", "FM_Hip_F_Tat_024",1750),   new BusinessTattoo(new List<int>(){5}, "Watch Your Step", "mphipster_overlays", "FM_Hip_M_Tat_025", "FM_Hip_F_Tat_025",1750),   new BusinessTattoo(new List<int>(){2,8}, "Sad", "mphipster_overlays", "FM_Hip_M_Tat_029", "FM_Hip_F_Tat_029",3750), new BusinessTattoo(new List<int>(){3,4}, "Shark Fin", "mphipster_overlays", "FM_Hip_M_Tat_030", "FM_Hip_F_Tat_030",2250),   new BusinessTattoo(new List<int>(){5,6}, "Skateboard", "mphipster_overlays", "FM_Hip_M_Tat_031", "FM_Hip_F_Tat_031",2250),  new BusinessTattoo(new List<int>(){6}, "Paper Plane", "mphipster_overlays", "FM_Hip_M_Tat_032", "FM_Hip_F_Tat_032",1750),   new BusinessTattoo(new List<int>(){0,1}, "Stag", "mphipster_overlays", "FM_Hip_M_Tat_033", "FM_Hip_F_Tat_033",2500),    new BusinessTattoo(new List<int>(){2,8}, "Sewn Heart", "mphipster_overlays", "FM_Hip_M_Tat_035", "FM_Hip_F_Tat_035",3750),  new BusinessTattoo(new List<int>(){3}, "Tooth", "mphipster_overlays", "FM_Hip_M_Tat_041", "FM_Hip_F_Tat_041",2000), new BusinessTattoo(new List<int>(){5,6}, "Triangles", "mphipster_overlays", "FM_Hip_M_Tat_046", "FM_Hip_F_Tat_046",2250),   new BusinessTattoo(new List<int>(){1}, "Cassette", "mphipster_overlays", "FM_Hip_M_Tat_047", "FM_Hip_F_Tat_047",1750),  new BusinessTattoo(new List<int>(){5,6}, "Block Back", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_000_M", "MP_MP_ImportExport_Tat_000_F",2250), new BusinessTattoo(new List<int>(){5,6}, "Power Plant", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_001_M", "MP_MP_ImportExport_Tat_001_F",2250),    new BusinessTattoo(new List<int>(){5,6}, "Tuned to Death", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_002_M", "MP_MP_ImportExport_Tat_002_F",2250), new BusinessTattoo(new List<int>(){5,6}, "Serpents of Destruction", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_009_M", "MP_MP_ImportExport_Tat_009_F",2250),    new BusinessTattoo(new List<int>(){5,6}, "Take the Wheel", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_010_M", "MP_MP_ImportExport_Tat_010_F",2250), new BusinessTattoo(new List<int>(){5,6}, "Talk Shit Get Hit", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_011_M", "MP_MP_ImportExport_Tat_011_F",2250),  new BusinessTattoo(new List<int>(){0}, "King Fight", "mplowrider_overlays", "MP_LR_Tat_001_M", "MP_LR_Tat_001_F",1750), new BusinessTattoo(new List<int>(){0,1}, "Holy Mary", "mplowrider_overlays", "MP_LR_Tat_002_M", "MP_LR_Tat_002_F",2500),    new BusinessTattoo(new List<int>(){7}, "Gun Mic", "mplowrider_overlays", "MP_LR_Tat_004_M", "MP_LR_Tat_004_F",2000),    new BusinessTattoo(new List<int>(){6}, "Amazon", "mplowrider_overlays", "MP_LR_Tat_009_M", "MP_LR_Tat_009_F",1750), new BusinessTattoo(new List<int>(){3,4,5,6}, "Bad Angel", "mplowrider_overlays", "MP_LR_Tat_010_M", "MP_LR_Tat_010_F",6000),    new BusinessTattoo(new List<int>(){1}, "Love Gamble", "mplowrider_overlays", "MP_LR_Tat_013_M", "MP_LR_Tat_013_F",1750),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Love is Blind", "mplowrider_overlays", "MP_LR_Tat_014_M", "MP_LR_Tat_014_F",1250),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Sad Angel", "mplowrider_overlays", "MP_LR_Tat_021_M", "MP_LR_Tat_021_F",5500),    new BusinessTattoo(new List<int>(){1}, "Royal Takeover", "mplowrider_overlays", "MP_LR_Tat_026_M", "MP_LR_Tat_026_F",1750), new BusinessTattoo(new List<int>(){1}, "Turbulence", "mpairraces_overlays", "MP_Airraces_Tattoo_000_M", "MP_Airraces_Tattoo_000_F",1750),   new BusinessTattoo(new List<int>(){5,6}, "Pilot Skull", "mpairraces_overlays", "MP_Airraces_Tattoo_001_M", "MP_Airraces_Tattoo_001_F",2250),    new BusinessTattoo(new List<int>(){5,6}, "Winged Bombshell", "mpairraces_overlays", "MP_Airraces_Tattoo_002_M", "MP_Airraces_Tattoo_002_F",2250),   new BusinessTattoo(new List<int>(){3,4,5,6}, "Balloon Pioneer", "mpairraces_overlays", "MP_Airraces_Tattoo_004_M", "MP_Airraces_Tattoo_004_F",5000),    new BusinessTattoo(new List<int>(){5,6}, "Parachute Belle", "mpairraces_overlays", "MP_Airraces_Tattoo_005_M", "MP_Airraces_Tattoo_005_F",2250),    new BusinessTattoo(new List<int>(){2}, "Bombs Away", "mpairraces_overlays", "MP_Airraces_Tattoo_006_M", "MP_Airraces_Tattoo_006_F",3000),   new BusinessTattoo(new List<int>(){5,6}, "Eagle Eyes", "mpairraces_overlays", "MP_Airraces_Tattoo_007_M", "MP_Airraces_Tattoo_007_F",2250), new BusinessTattoo(new List<int>(){0}, "Demon Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_000_M", "MP_MP_Biker_Tat_000_F",1750),   new BusinessTattoo(new List<int>(){0,1}, "Both Barrels", "mpbiker_overlays", "MP_MP_Biker_Tat_001_M", "MP_MP_Biker_Tat_001_F",2500),    new BusinessTattoo(new List<int>(){2}, "Web Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_003_M", "MP_MP_Biker_Tat_003_F",3000), new BusinessTattoo(new List<int>(){0,1}, "Made In America", "mpbiker_overlays", "MP_MP_Biker_Tat_005_M", "MP_MP_Biker_Tat_005_F",2500), new BusinessTattoo(new List<int>(){3,4}, "Chopper Freedom", "mpbiker_overlays", "MP_MP_Biker_Tat_006_M", "MP_MP_Biker_Tat_006_F",2000), new BusinessTattoo(new List<int>(){5,6}, "Freedom Wheels", "mpbiker_overlays", "MP_MP_Biker_Tat_008_M", "MP_MP_Biker_Tat_008_F",2250),  new BusinessTattoo(new List<int>(){2}, "Skull Of Taurus", "mpbiker_overlays", "MP_MP_Biker_Tat_010_M", "MP_MP_Biker_Tat_010_F",3250),   new BusinessTattoo(new List<int>(){5,6}, "R.I.P. My Brothers", "mpbiker_overlays", "MP_MP_Biker_Tat_011_M", "MP_MP_Biker_Tat_011_F",2250),  new BusinessTattoo(new List<int>(){0,1}, "Demon Crossbones", "mpbiker_overlays", "MP_MP_Biker_Tat_013_M", "MP_MP_Biker_Tat_013_F",3000),    new BusinessTattoo(new List<int>(){5,6}, "Clawed Beast", "mpbiker_overlays", "MP_MP_Biker_Tat_017_M", "MP_MP_Biker_Tat_017_F",2250),    new BusinessTattoo(new List<int>(){1}, "Skeletal Chopper", "mpbiker_overlays", "MP_MP_Biker_Tat_018_M", "MP_MP_Biker_Tat_018_F",1800),  new BusinessTattoo(new List<int>(){0,1}, "Gruesome Talons", "mpbiker_overlays", "MP_MP_Biker_Tat_019_M", "MP_MP_Biker_Tat_019_F",2750), new BusinessTattoo(new List<int>(){5,6}, "Flaming Reaper", "mpbiker_overlays", "MP_MP_Biker_Tat_021_M", "MP_MP_Biker_Tat_021_F",2250),  new BusinessTattoo(new List<int>(){0,1}, "Western MC", "mpbiker_overlays", "MP_MP_Biker_Tat_023_M", "MP_MP_Biker_Tat_023_F",2750),  new BusinessTattoo(new List<int>(){0,1}, "American Dream", "mpbiker_overlays", "MP_MP_Biker_Tat_026_M", "MP_MP_Biker_Tat_026_F",2650),  new BusinessTattoo(new List<int>(){0}, "Bone Wrench", "mpbiker_overlays", "MP_MP_Biker_Tat_029_M", "MP_MP_Biker_Tat_029_F",1650),   new BusinessTattoo(new List<int>(){5,6}, "Brothers For Life", "mpbiker_overlays", "MP_MP_Biker_Tat_030_M", "MP_MP_Biker_Tat_030_F",2300),   new BusinessTattoo(new List<int>(){2}, "Gear Head", "mpbiker_overlays", "MP_MP_Biker_Tat_031_M", "MP_MP_Biker_Tat_031_F",3000), new BusinessTattoo(new List<int>(){0}, "Western Eagle", "mpbiker_overlays", "MP_MP_Biker_Tat_032_M", "MP_MP_Biker_Tat_032_F",1800), new BusinessTattoo(new List<int>(){1}, "Brotherhood of Bikes", "mpbiker_overlays", "MP_MP_Biker_Tat_034_M", "MP_MP_Biker_Tat_034_F",1850),  new BusinessTattoo(new List<int>(){2}, "Gas Guzzler", "mpbiker_overlays", "MP_MP_Biker_Tat_039_M", "MP_MP_Biker_Tat_039_F",2850),   new BusinessTattoo(new List<int>(){0,1}, "No Regrets", "mpbiker_overlays", "MP_MP_Biker_Tat_041_M", "MP_MP_Biker_Tat_041_F",2500),  new BusinessTattoo(new List<int>(){3,4}, "Ride Forever", "mpbiker_overlays", "MP_MP_Biker_Tat_043_M", "MP_MP_Biker_Tat_043_F",2100),    new BusinessTattoo(new List<int>(){0,1}, "Unforgiven", "mpbiker_overlays", "MP_MP_Biker_Tat_050_M", "MP_MP_Biker_Tat_050_F",3000),  new BusinessTattoo(new List<int>(){2}, "Biker Mount", "mpbiker_overlays", "MP_MP_Biker_Tat_052_M", "MP_MP_Biker_Tat_052_F",2500),   new BusinessTattoo(new List<int>(){1}, "Reaper Vulture", "mpbiker_overlays", "MP_MP_Biker_Tat_058_M", "MP_MP_Biker_Tat_058_F",1750),    new BusinessTattoo(new List<int>(){1}, "Faggio", "mpbiker_overlays", "MP_MP_Biker_Tat_059_M", "MP_MP_Biker_Tat_059_F",1750),    new BusinessTattoo(new List<int>(){0}, "We Are The Mods!", "mpbiker_overlays", "MP_MP_Biker_Tat_060_M", "MP_MP_Biker_Tat_060_F",1850),  new BusinessTattoo(new List<int>(){3,4,5,6}, "SA Assault", "mplowrider2_overlays", "MP_LR_Tat_000_M", "MP_LR_Tat_000_F",5500),  new BusinessTattoo(new List<int>(){3,4,5,6}, "Love the Game", "mplowrider2_overlays", "MP_LR_Tat_008_M", "MP_LR_Tat_008_F",5250),   new BusinessTattoo(new List<int>(){7}, "Lady Liberty", "mplowrider2_overlays", "MP_LR_Tat_011_M", "MP_LR_Tat_011_F",2100),  new BusinessTattoo(new List<int>(){0}, "Royal Kiss", "mplowrider2_overlays", "MP_LR_Tat_012_M", "MP_LR_Tat_012_F",1750),    new BusinessTattoo(new List<int>(){2}, "Two Face", "mplowrider2_overlays", "MP_LR_Tat_016_M", "MP_LR_Tat_016_F",3100),  new BusinessTattoo(new List<int>(){1}, "Death Behind", "mplowrider2_overlays", "MP_LR_Tat_019_M", "MP_LR_Tat_019_F",1750),  new BusinessTattoo(new List<int>(){3,4,5,6}, "Dead Pretty", "mplowrider2_overlays", "MP_LR_Tat_031_M", "MP_LR_Tat_031_F",5250), new BusinessTattoo(new List<int>(){3,4,5,6}, "Reign Over", "mplowrider2_overlays", "MP_LR_Tat_032_M", "MP_LR_Tat_032_F",5600),  new BusinessTattoo(new List<int>(){2}, "Abstract Skull", "mpluxe_overlays", "MP_LUXE_TAT_003_M", "MP_LUXE_TAT_003_F",2750), new BusinessTattoo(new List<int>(){1}, "Eye of the Griffin", "mpluxe_overlays", "MP_LUXE_TAT_007_M", "MP_LUXE_TAT_007_F",1850), new BusinessTattoo(new List<int>(){1}, "Flying Eye", "mpluxe_overlays", "MP_LUXE_TAT_008_M", "MP_LUXE_TAT_008_F",1800), new BusinessTattoo(new List<int>(){0,1}, "Ancient Queen", "mpluxe_overlays", "MP_LUXE_TAT_014_M", "MP_LUXE_TAT_014_F",2600),    new BusinessTattoo(new List<int>(){0}, "Smoking Sisters", "mpluxe_overlays", "MP_LUXE_TAT_015_M", "MP_LUXE_TAT_015_F",1750),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Feather Mural", "mpluxe_overlays", "MP_LUXE_TAT_024_M", "MP_LUXE_TAT_024_F",6250),    new BusinessTattoo(new List<int>(){0}, "The Howler", "mpluxe2_overlays", "MP_LUXE_TAT_002_M", "MP_LUXE_TAT_002_F",1750),    new BusinessTattoo(new List<int>(){0,1,2,8}, "Geometric Galaxy", "mpluxe2_overlays", "MP_LUXE_TAT_012_M", "MP_LUXE_TAT_012_F",7000),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Cloaked Angel", "mpluxe2_overlays", "MP_LUXE_TAT_022_M", "MP_LUXE_TAT_022_F",6000),   new BusinessTattoo(new List<int>(){0}, "Reaper Sway", "mpluxe2_overlays", "MP_LUXE_TAT_025_M", "MP_LUXE_TAT_025_F",1750),   new BusinessTattoo(new List<int>(){1}, "Cobra Dawn", "mpluxe2_overlays", "MP_LUXE_TAT_027_M", "MP_LUXE_TAT_027_F",1800),    new BusinessTattoo(new List<int>(){3,4,5,6}, "Geometric Design T", "mpluxe2_overlays", "MP_LUXE_TAT_029_M", "MP_LUXE_TAT_029_F",5500),  new BusinessTattoo(new List<int>(){1}, "Bless The Dead", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_000_M", "MP_Smuggler_Tattoo_000_F",1000),   new BusinessTattoo(new List<int>(){2}, "Dead Lies", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_002_M", "MP_Smuggler_Tattoo_002_F",3000),    new BusinessTattoo(new List<int>(){5,6}, "Give Nothing Back", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_003_M", "MP_Smuggler_Tattoo_003_F",2000),  new BusinessTattoo(new List<int>(){5,6}, "Never Surrender", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_006_M", "MP_Smuggler_Tattoo_006_F",2100),    new BusinessTattoo(new List<int>(){0,1}, "No Honor", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_007_M", "MP_Smuggler_Tattoo_007_F",2500),   new BusinessTattoo(new List<int>(){5,6}, "Tall Ship Conflict", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_009_M", "MP_Smuggler_Tattoo_009_F",2000), new BusinessTattoo(new List<int>(){2}, "See You In Hell", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_010_M", "MP_Smuggler_Tattoo_010_F",3000),  new BusinessTattoo(new List<int>(){5,6}, "Torn Wings", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_013_M", "MP_Smuggler_Tattoo_013_F",2100), new BusinessTattoo(new List<int>(){2}, "Jolly Roger", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_015_M", "MP_Smuggler_Tattoo_015_F",3000),  new BusinessTattoo(new List<int>(){5,6}, "Skull Compass", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_016_M", "MP_Smuggler_Tattoo_016_F",2000),  new BusinessTattoo(new List<int>(){3,4,5,6}, "Framed Tall Ship", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_017_M", "MP_Smuggler_Tattoo_017_F",5500),   new BusinessTattoo(new List<int>(){3,4,5,6}, "Finders Keepers", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_018_M", "MP_Smuggler_Tattoo_018_F",6000),    new BusinessTattoo(new List<int>(){0}, "Lost At Sea", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_019_M", "MP_Smuggler_Tattoo_019_F",1750),  new BusinessTattoo(new List<int>(){0,1}, "Dead Tales", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_021_M", "MP_Smuggler_Tattoo_021_F",2000), new BusinessTattoo(new List<int>(){5}, "X Marks The Spot", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_022_M", "MP_Smuggler_Tattoo_022_F",1750), new BusinessTattoo(new List<int>(){3,4,5,6}, "Pirate Captain", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_024_M", "MP_Smuggler_Tattoo_024_F",5500), new BusinessTattoo(new List<int>(){3,4,5,6}, "Claimed By The Beast", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_025_M", "MP_Smuggler_Tattoo_025_F",5500),   new BusinessTattoo(new List<int>(){0,1}, "Wheels of Death", "mpstunt_overlays", "MP_MP_Stunt_Tat_011_M", "MP_MP_Stunt_Tat_011_F",2000), new BusinessTattoo(new List<int>(){7}, "Punk Biker", "mpstunt_overlays", "MP_MP_Stunt_Tat_012_M", "MP_MP_Stunt_Tat_012_F",2000),    new BusinessTattoo(new List<int>(){2}, "Bat Cat of Spades", "mpstunt_overlays", "MP_MP_Stunt_Tat_014_M", "MP_MP_Stunt_Tat_014_F",3100), new BusinessTattoo(new List<int>(){0}, "Vintage Bully", "mpstunt_overlays", "MP_MP_Stunt_Tat_018_M", "MP_MP_Stunt_Tat_018_F",1750), new BusinessTattoo(new List<int>(){1}, "Engine Heart", "mpstunt_overlays", "MP_MP_Stunt_Tat_019_M", "MP_MP_Stunt_Tat_019_F",1750),  new BusinessTattoo(new List<int>(){3,4,5,6}, "Road Kill", "mpstunt_overlays", "MP_MP_Stunt_Tat_024_M", "MP_MP_Stunt_Tat_024_F",5000),   new BusinessTattoo(new List<int>(){5,6}, "Winged Wheel", "mpstunt_overlays", "MP_MP_Stunt_Tat_026_M", "MP_MP_Stunt_Tat_026_F",2000),    new BusinessTattoo(new List<int>(){0}, "Punk Road Hog", "mpstunt_overlays", "MP_MP_Stunt_Tat_027_M", "MP_MP_Stunt_Tat_027_F",1750), new BusinessTattoo(new List<int>(){3,4}, "Majestic Finish", "mpstunt_overlays", "MP_MP_Stunt_Tat_029_M", "MP_MP_Stunt_Tat_029_F",2000), new BusinessTattoo(new List<int>(){6}, "Man's Ruin", "mpstunt_overlays", "MP_MP_Stunt_Tat_030_M", "MP_MP_Stunt_Tat_030_F",2100),    new BusinessTattoo(new List<int>(){1}, "Sugar Skull Trucker", "mpstunt_overlays", "MP_MP_Stunt_Tat_033_M", "MP_MP_Stunt_Tat_033_F",1750),   new BusinessTattoo(new List<int>(){3,4,5,6}, "Feather Road Kill", "mpstunt_overlays", "MP_MP_Stunt_Tat_034_M", "MP_MP_Stunt_Tat_034_F",1250),   new BusinessTattoo(new List<int>(){5}, "Big Grills", "mpstunt_overlays", "MP_MP_Stunt_Tat_037_M", "MP_MP_Stunt_Tat_037_F",1750),    new BusinessTattoo(new List<int>(){5,6}, "Monkey Chopper", "mpstunt_overlays", "MP_MP_Stunt_Tat_040_M", "MP_MP_Stunt_Tat_040_F",2000),  new BusinessTattoo(new List<int>(){5,6}, "Brapp", "mpstunt_overlays", "MP_MP_Stunt_Tat_041_M", "MP_MP_Stunt_Tat_041_F",2000),   new BusinessTattoo(new List<int>(){0,1}, "Ram Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_044_M", "MP_MP_Stunt_Tat_044_F",2000),   new BusinessTattoo(new List<int>(){5,6}, "Full Throttle", "mpstunt_overlays", "MP_MP_Stunt_Tat_046_M", "MP_MP_Stunt_Tat_046_F",2100),   new BusinessTattoo(new List<int>(){5,6}, "Racing Doll", "mpstunt_overlays", "MP_MP_Stunt_Tat_048_M", "MP_MP_Stunt_Tat_048_F",2100), new BusinessTattoo(new List<int>(){0}, "Blackjack", "multiplayer_overlays", "FM_Tat_Award_M_003", "FM_Tat_Award_F_003",1800),   new BusinessTattoo(new List<int>(){2}, "Hustler", "multiplayer_overlays", "FM_Tat_Award_M_004", "FM_Tat_Award_F_004",3250), new BusinessTattoo(new List<int>(){5,6}, "Angel", "multiplayer_overlays", "FM_Tat_Award_M_005", "FM_Tat_Award_F_005",2100), new BusinessTattoo(new List<int>(){3,4}, "Los Santos Customs", "multiplayer_overlays", "FM_Tat_Award_M_008", "FM_Tat_Award_F_008",8400),    new BusinessTattoo(new List<int>(){1}, "Blank Scroll", "multiplayer_overlays", "FM_Tat_Award_M_011", "FM_Tat_Award_F_011",1800),    new BusinessTattoo(new List<int>(){1}, "Embellished Scroll", "multiplayer_overlays", "FM_Tat_Award_M_012", "FM_Tat_Award_F_012",1800),  new BusinessTattoo(new List<int>(){1}, "Seven Deadly Sins", "multiplayer_overlays", "FM_Tat_Award_M_013", "FM_Tat_Award_F_013",1800),   new BusinessTattoo(new List<int>(){3,4}, "Trust No One", "multiplayer_overlays", "FM_Tat_Award_M_014", "FM_Tat_Award_F_014",2100),  new BusinessTattoo(new List<int>(){5,6}, "Clown", "multiplayer_overlays", "FM_Tat_Award_M_016", "FM_Tat_Award_F_016",2000), new BusinessTattoo(new List<int>(){5,6}, "Clown and Gun", "multiplayer_overlays", "FM_Tat_Award_M_017", "FM_Tat_Award_F_017",2100), new BusinessTattoo(new List<int>(){5,6}, "Clown Dual Wield", "multiplayer_overlays", "FM_Tat_Award_M_018", "FM_Tat_Award_F_018",2000),  new BusinessTattoo(new List<int>(){6,6}, "Clown Dual Wield Dollars", "multiplayer_overlays", "FM_Tat_Award_M_019", "FM_Tat_Award_F_019",2100),  new BusinessTattoo(new List<int>(){2}, "Faith T", "multiplayer_overlays", "FM_Tat_M_004", "FM_Tat_F_004",3100), new BusinessTattoo(new List<int>(){3,4,5,6}, "Skull on the Cross", "multiplayer_overlays", "FM_Tat_M_009", "FM_Tat_F_009",6000),    new BusinessTattoo(new List<int>(){1}, "LS Flames", "multiplayer_overlays", "FM_Tat_M_010", "FM_Tat_F_010",1800),   new BusinessTattoo(new List<int>(){5}, "LS Script", "multiplayer_overlays", "FM_Tat_M_011", "FM_Tat_F_011",2100),   new BusinessTattoo(new List<int>(){2}, "Los Santos Bills", "multiplayer_overlays", "FM_Tat_M_012", "FM_Tat_F_012",3000),    new BusinessTattoo(new List<int>(){6}, "Eagle and Serpent", "multiplayer_overlays", "FM_Tat_M_013", "FM_Tat_F_013",2100),   new BusinessTattoo(new List<int>(){3,4,5,6}, "Evil Clown", "multiplayer_overlays", "FM_Tat_M_016", "FM_Tat_F_016",5750),    new BusinessTattoo(new List<int>(){3,4,5,6}, "The Wages of Sin", "multiplayer_overlays", "FM_Tat_M_019", "FM_Tat_F_019",5500),  new BusinessTattoo(new List<int>(){3,4,5,6}, "Dragon T", "multiplayer_overlays", "FM_Tat_M_020", "FM_Tat_F_020",5000),  new BusinessTattoo(new List<int>(){0,1,2,8}, "Flaming Cross", "multiplayer_overlays", "FM_Tat_M_024", "FM_Tat_F_024",6750), new BusinessTattoo(new List<int>(){0}, "LS Bold", "multiplayer_overlays", "FM_Tat_M_025", "FM_Tat_F_025",1800), new BusinessTattoo(new List<int>(){2,8}, "Trinity Knot", "multiplayer_overlays", "FM_Tat_M_029", "FM_Tat_F_029",4100),  new BusinessTattoo(new List<int>(){5,6}, "Lucky Celtic Dogs", "multiplayer_overlays", "FM_Tat_M_030", "FM_Tat_F_030",2100), new BusinessTattoo(new List<int>(){1}, "Flaming Shamrock", "multiplayer_overlays", "FM_Tat_M_034", "FM_Tat_F_034",1700),    new BusinessTattoo(new List<int>(){2}, "Way of the Gun", "multiplayer_overlays", "FM_Tat_M_036", "FM_Tat_F_036",3000),  new BusinessTattoo(new List<int>(){0,1}, "Stone Cross", "multiplayer_overlays", "FM_Tat_M_044", "FM_Tat_F_044",2100),   new BusinessTattoo(new List<int>(){3,4,5,6}, "Skulls and Rose", "multiplayer_overlays", "FM_Tat_M_045", "FM_Tat_F_045",5500),

               
            },

            // Head
            new List<BusinessTattoo>(){
	            // Передняя шея -   0
                // Левая шея    -   1
                // Правая шея   -   2
                // Задняя шея   -   3
	            // Левая щека - 4
                // Правая щека - 5

                //Новые
                new BusinessTattoo(new List<int>(){0},"Cash is King", "mpbusiness_overlays", "MP_Buis_M_Neck_000", "",1750),    new BusinessTattoo(new List<int>(){1}, "Bold Dollar Sign", "mpbusiness_overlays", "MP_Buis_M_Neck_001", "",1750),   new BusinessTattoo(new List<int>(){2}, "Script Dollar Sign", "mpbusiness_overlays", "MP_Buis_M_Neck_002", "",1750), new BusinessTattoo(new List<int>(){3}, "$100", "mpbusiness_overlays", "MP_Buis_M_Neck_003", "",1750),   new BusinessTattoo(new List<int>(){1}, "Val-de-Grace Logo", "mpbusiness_overlays", "", "MP_Buis_F_Neck_000",1750),  new BusinessTattoo(new List<int>(){2}, "Money Rose", "mpbusiness_overlays", "", "MP_Buis_F_Neck_001",1750), new BusinessTattoo(new List<int>(){2}, "Los Muertos", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_007", "MP_Xmas2_F_Tat_007",1750),    new BusinessTattoo(new List<int>(){1}, "Snake Head Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_024", "MP_Xmas2_F_Tat_024",1750), new BusinessTattoo(new List<int>(){1}, "Snake Head Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_025", "MP_Xmas2_F_Tat_025",1750),   new BusinessTattoo(new List<int>(){2}, "Beautiful Death", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_029", "MP_Xmas2_F_Tat_029",1750),    new BusinessTattoo(new List<int>(){1}, "Lock & Load", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_003_M", "MP_Gunrunning_Tattoo_003_F",1750),    new BusinessTattoo(new List<int>(){2}, "Beautiful Eye", "mphipster_overlays", "FM_Hip_M_Tat_005", "FM_Hip_F_Tat_005",1750), new BusinessTattoo(new List<int>(){1}, "Geo Fox", "mphipster_overlays", "FM_Hip_M_Tat_021", "FM_Hip_F_Tat_021",1750),   new BusinessTattoo(new List<int>(){5}, "Morbid Arachnid", "mpbiker_overlays", "MP_MP_Biker_Tat_009_M", "MP_MP_Biker_Tat_009_F",1750),   new BusinessTattoo(new List<int>(){2}, "FTW", "mpbiker_overlays", "MP_MP_Biker_Tat_038_M", "MP_MP_Biker_Tat_038_F",1750),   new BusinessTattoo(new List<int>(){1}, "Western Stylized", "mpbiker_overlays", "MP_MP_Biker_Tat_051_M", "MP_MP_Biker_Tat_051_F",1750),  new BusinessTattoo(new List<int>(){1}, "Sinner", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_011_M", "MP_Smuggler_Tattoo_011_F",1750),   new BusinessTattoo(new List<int>(){2}, "Thief", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_012_M", "MP_Smuggler_Tattoo_012_F",1750),    new BusinessTattoo(new List<int>(){1}, "Stunt Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_000_M", "MP_MP_Stunt_Tat_000_F",1750),   new BusinessTattoo(new List<int>(){5}, "Scorpion", "mpstunt_overlays", "MP_MP_Stunt_Tat_004_M", "MP_MP_Stunt_Tat_004_F",200),   new BusinessTattoo(new List<int>(){2}, "Toxic Spider", "mpstunt_overlays", "MP_MP_Stunt_Tat_006_M", "MP_MP_Stunt_Tat_006_F",200),   new BusinessTattoo(new List<int>(){2}, "Bat Wheel", "mpstunt_overlays", "MP_MP_Stunt_Tat_017_M", "MP_MP_Stunt_Tat_017_F",200),  new BusinessTattoo(new List<int>(){2}, "Flaming Quad", "mpstunt_overlays", "MP_MP_Stunt_Tat_042_M", "MP_MP_Stunt_Tat_042_F",1750),


               
            },

            // Left Arm
            new List<BusinessTattoo>()
            {
                // Кисть        -   0
                // До локтя     -   1
                // Выше локтя   -   2

                //Новое
                new BusinessTattoo(new List<int>(){1,2},"Suits", "mpvinewood_overlays", "MP_Vinewood_Tat_002_M", "MP_Vinewood_Tat_002_F",2500), new BusinessTattoo(new List<int>(){1,2}, "Get Lucky", "mpvinewood_overlays", "MP_Vinewood_Tat_005_M", "MP_Vinewood_Tat_005_F",3000),    new BusinessTattoo(new List<int>(){1}, "Vice", "mpvinewood_overlays", "MP_Vinewood_Tat_014_M", "MP_Vinewood_Tat_014_F",1800),   new BusinessTattoo(new List<int>(){1,2}, "Can't Win Them All", "mpvinewood_overlays", "MP_Vinewood_Tat_019_M", "MP_Vinewood_Tat_019_F",4000),   new BusinessTattoo(new List<int>(){1,2}, "Banknote Rose", "mpvinewood_overlays", "MP_Vinewood_Tat_026_M", "MP_Vinewood_Tat_026_F",3500),    new BusinessTattoo(new List<int>(){1}, "$100 Bill", "mpbusiness_overlays", "MP_Buis_M_LeftArm_000", "",1850),   new BusinessTattoo(new List<int>(){1,2}, "All-Seeing Eye", "mpbusiness_overlays", "MP_Buis_M_LeftArm_001", "",780), new BusinessTattoo(new List<int>(){1}, "Greed is Good", "mpbusiness_overlays", "", "MP_Buis_F_LArm_000",1800),  new BusinessTattoo(new List<int>(){1}, "Skull Rider", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_000", "MP_Xmas2_F_Tat_000",1850),    new BusinessTattoo(new List<int>(){1}, "Electric Snake", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_010", "MP_Xmas2_F_Tat_010",1800), new BusinessTattoo(new List<int>(){2}, "8 Ball Skull", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_012", "MP_Xmas2_F_Tat_012",1900),   new BusinessTattoo(new List<int>(){0}, "Time's Up Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_020", "MP_Xmas2_F_Tat_020",1300),  new BusinessTattoo(new List<int>(){0}, "Time's Up Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_021", "MP_Xmas2_F_Tat_021",1300),    new BusinessTattoo(new List<int>(){0}, "Sidearm", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_004_M", "MP_Gunrunning_Tattoo_004_F",1350),    new BusinessTattoo(new List<int>(){2}, "Bandolier", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_008_M", "MP_Gunrunning_Tattoo_008_F",1780),  new BusinessTattoo(new List<int>(){1,2}, "Spiked Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_015_M", "MP_Gunrunning_Tattoo_015_F",3800), new BusinessTattoo(new List<int>(){2}, "Blood Money", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_016_M", "MP_Gunrunning_Tattoo_016_F",1800),    new BusinessTattoo(new List<int>(){1}, "Praying Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_025_M", "MP_Gunrunning_Tattoo_025_F",1800),  new BusinessTattoo(new List<int>(){2}, "Serpent Revolver", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_027_M", "MP_Gunrunning_Tattoo_027_F",1850),   new BusinessTattoo(new List<int>(){1}, "Diamond Sparkle", "mphipster_overlays", "FM_Hip_M_Tat_003", "FM_Hip_F_Tat_003",1800),   new BusinessTattoo(new List<int>(){0}, "Bricks", "mphipster_overlays", "FM_Hip_M_Tat_007", "FM_Hip_F_Tat_007",1300),    new BusinessTattoo(new List<int>(){2}, "Mustache", "mphipster_overlays", "FM_Hip_M_Tat_015", "FM_Hip_F_Tat_015",1800),  new BusinessTattoo(new List<int>(){1}, "Lightning Bolt", "mphipster_overlays", "FM_Hip_M_Tat_016", "FM_Hip_F_Tat_016",1800),    new BusinessTattoo(new List<int>(){2}, "Pizza", "mphipster_overlays", "FM_Hip_M_Tat_026", "FM_Hip_F_Tat_026",1800), new BusinessTattoo(new List<int>(){1}, "Padlock", "mphipster_overlays", "FM_Hip_M_Tat_027", "FM_Hip_F_Tat_027",2000),   new BusinessTattoo(new List<int>(){1}, "Thorny Rose", "mphipster_overlays", "FM_Hip_M_Tat_028", "FM_Hip_F_Tat_028",2000),   new BusinessTattoo(new List<int>(){0}, "Stop", "mphipster_overlays", "FM_Hip_M_Tat_034", "FM_Hip_F_Tat_034",1250),  new BusinessTattoo(new List<int>(){2}, "Sunrise", "mphipster_overlays", "FM_Hip_M_Tat_037", "FM_Hip_F_Tat_037",1850),   new BusinessTattoo(new List<int>(){1,2}, "Sleeve", "mphipster_overlays", "FM_Hip_M_Tat_039", "FM_Hip_F_Tat_039",4500),  new BusinessTattoo(new List<int>(){2}, "Triangle White", "mphipster_overlays", "FM_Hip_M_Tat_043", "FM_Hip_F_Tat_043",1850),    new BusinessTattoo(new List<int>(){0}, "Peace", "mphipster_overlays", "FM_Hip_M_Tat_048", "FM_Hip_F_Tat_048",1300), new BusinessTattoo(new List<int>(){1,2}, "Piston Sleeve", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_004_M", "MP_MP_ImportExport_Tat_004_F",3800),  new BusinessTattoo(new List<int>(){1,2}, "Scarlett", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_008_M", "MP_MP_ImportExport_Tat_008_F",3750),   new BusinessTattoo(new List<int>(){1}, "No Evil", "mplowrider_overlays", "MP_LR_Tat_005_M", "MP_LR_Tat_005_F",1780),    new BusinessTattoo(new List<int>(){2}, "Los Santos Life", "mplowrider_overlays", "MP_LR_Tat_027_M", "MP_LR_Tat_027_F",1800),    new BusinessTattoo(new List<int>(){1,2}, "City Sorrow", "mplowrider_overlays", "MP_LR_Tat_033_M", "MP_LR_Tat_033_F",3800),  new BusinessTattoo(new List<int>(){1,2}, "Toxic Trails", "mpairraces_overlays", "MP_Airraces_Tattoo_003_M", "MP_Airraces_Tattoo_003_F",15700),  new BusinessTattoo(new List<int>(){1}, "Urban Stunter", "mpbiker_overlays", "MP_MP_Biker_Tat_012_M", "MP_MP_Biker_Tat_012_F",1850), new BusinessTattoo(new List<int>(){2}, "Macabre Tree", "mpbiker_overlays", "MP_MP_Biker_Tat_016_M", "MP_MP_Biker_Tat_016_F",2000),  new BusinessTattoo(new List<int>(){2}, "Cranial Rose", "mpbiker_overlays", "MP_MP_Biker_Tat_020_M", "MP_MP_Biker_Tat_020_F",1800),  new BusinessTattoo(new List<int>(){1,2}, "Live to Ride", "mpbiker_overlays", "MP_MP_Biker_Tat_024_M", "MP_MP_Biker_Tat_024_F",3800),    new BusinessTattoo(new List<int>(){2}, "Good Luck", "mpbiker_overlays", "MP_MP_Biker_Tat_025_M", "MP_MP_Biker_Tat_025_F",1100), new BusinessTattoo(new List<int>(){2}, "Chain Fist", "mpbiker_overlays", "MP_MP_Biker_Tat_035_M", "MP_MP_Biker_Tat_035_F",1600),    new BusinessTattoo(new List<int>(){2}, "Ride Hard Die Fast", "mpbiker_overlays", "MP_MP_Biker_Tat_045_M", "MP_MP_Biker_Tat_045_F",1800),    new BusinessTattoo(new List<int>(){1}, "Muffler Helmet", "mpbiker_overlays", "MP_MP_Biker_Tat_053_M", "MP_MP_Biker_Tat_053_F",1850),    new BusinessTattoo(new List<int>(){2}, "Poison Scorpion", "mpbiker_overlays", "MP_MP_Biker_Tat_055_M", "MP_MP_Biker_Tat_055_F",1800),   new BusinessTattoo(new List<int>(){2}, "Love Hustle", "mplowrider2_overlays", "MP_LR_Tat_006_M", "MP_LR_Tat_006_F",1800),   new BusinessTattoo(new List<int>(){1,2}, "Skeleton Party", "mplowrider2_overlays", "MP_LR_Tat_018_M", "MP_LR_Tat_018_F",3700),  new BusinessTattoo(new List<int>(){1}, "My Crazy Life", "mplowrider2_overlays", "MP_LR_Tat_022_M", "MP_LR_Tat_022_F",1850), new BusinessTattoo(new List<int>(){2}, "Archangel & Mary", "mpluxe_overlays", "MP_LUXE_TAT_020_M", "MP_LUXE_TAT_020_F",1800),   new BusinessTattoo(new List<int>(){1}, "Gabriel", "mpluxe_overlays", "MP_LUXE_TAT_021_M", "MP_LUXE_TAT_021_F",1800),    new BusinessTattoo(new List<int>(){1}, "Fatal Dagger", "mpluxe2_overlays", "MP_LUXE_TAT_005_M", "MP_LUXE_TAT_005_F",1800),  new BusinessTattoo(new List<int>(){1}, "Egyptian Mural", "mpluxe2_overlays", "MP_LUXE_TAT_016_M", "MP_LUXE_TAT_016_F",1780),    new BusinessTattoo(new List<int>(){2}, "Divine Goddess", "mpluxe2_overlays", "MP_LUXE_TAT_018_M", "MP_LUXE_TAT_018_F",1780),    new BusinessTattoo(new List<int>(){1}, "Python Skull", "mpluxe2_overlays", "MP_LUXE_TAT_028_M", "MP_LUXE_TAT_028_F",1850),  new BusinessTattoo(new List<int>(){1,2}, "Geometric Design LA", "mpluxe2_overlays", "MP_LUXE_TAT_031_M", "MP_LUXE_TAT_031_F",3800), new BusinessTattoo(new List<int>(){1}, "Honor", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_004_M", "MP_Smuggler_Tattoo_004_F",1800),    new BusinessTattoo(new List<int>(){1}, "Horrors Of The Deep", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_008_M", "MP_Smuggler_Tattoo_008_F",1850),  new BusinessTattoo(new List<int>(){1,2}, "Mermaid's Curse", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_014_M", "MP_Smuggler_Tattoo_014_F",3800),    new BusinessTattoo(new List<int>(){2}, "8 Eyed Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_001_M", "MP_MP_Stunt_Tat_001_F",1750),  new BusinessTattoo(new List<int>(){0}, "Big Cat", "mpstunt_overlays", "MP_MP_Stunt_Tat_002_M", "MP_MP_Stunt_Tat_002_F",1250),   new BusinessTattoo(new List<int>(){2}, "Moonlight Ride", "mpstunt_overlays", "MP_MP_Stunt_Tat_008_M", "MP_MP_Stunt_Tat_008_F",1800),    new BusinessTattoo(new List<int>(){1}, "Piston Head", "mpstunt_overlays", "MP_MP_Stunt_Tat_022_M", "MP_MP_Stunt_Tat_022_F",1800),   new BusinessTattoo(new List<int>(){1,2}, "Tanked", "mpstunt_overlays", "MP_MP_Stunt_Tat_023_M", "MP_MP_Stunt_Tat_023_F",3750),  new BusinessTattoo(new List<int>(){1}, "Stuntman's End", "mpstunt_overlays", "MP_MP_Stunt_Tat_035_M", "MP_MP_Stunt_Tat_035_F",1800),    new BusinessTattoo(new List<int>(){2}, "Kaboom", "mpstunt_overlays", "MP_MP_Stunt_Tat_039_M", "MP_MP_Stunt_Tat_039_F",1850),    new BusinessTattoo(new List<int>(){2}, "Engine Arm", "mpstunt_overlays", "MP_MP_Stunt_Tat_043_M", "MP_MP_Stunt_Tat_043_F",1800),    new BusinessTattoo(new List<int>(){1}, "Burning Heart", "multiplayer_overlays", "FM_Tat_Award_M_001", "FM_Tat_Award_F_001",1850),   new BusinessTattoo(new List<int>(){2}, "Racing Blonde", "multiplayer_overlays", "FM_Tat_Award_M_007", "FM_Tat_Award_F_007",1850),   new BusinessTattoo(new List<int>(){2}, "Racing Brunette", "multiplayer_overlays", "FM_Tat_Award_M_015", "FM_Tat_Award_F_015",1850), new BusinessTattoo(new List<int>(){1,2}, "Serpents", "multiplayer_overlays", "FM_Tat_M_005", "FM_Tat_F_005",1780),  new BusinessTattoo(new List<int>(){1,2}, "Oriental Mural", "multiplayer_overlays", "FM_Tat_M_006", "FM_Tat_F_006",3800),    new BusinessTattoo(new List<int>(){2}, "Zodiac Skull", "multiplayer_overlays", "FM_Tat_M_015", "FM_Tat_F_015",1800),    new BusinessTattoo(new List<int>(){2}, "Lady M", "multiplayer_overlays", "FM_Tat_M_031", "FM_Tat_F_031",1850),  new BusinessTattoo(new List<int>(){2}, "Dope Skull", "multiplayer_overlays", "FM_Tat_M_041", "FM_Tat_F_041",1800),
                

            },
            
            // RightArm
            new List<BusinessTattoo>()
            {
                // Кисть        -   0
                // До локтя     -   1
                // Выше локтя   -   2

                //Новое
                new BusinessTattoo(new List<int>(){1,2},"Lady Luck", "mpvinewood_overlays", "MP_Vinewood_Tat_004_M", "MP_Vinewood_Tat_004_F",1800), new BusinessTattoo(new List<int>(){1,2}, "The Gambler's Life", "mpvinewood_overlays", "MP_Vinewood_Tat_018_M", "MP_Vinewood_Tat_018_F",1800),   new BusinessTattoo(new List<int>(){1}, "Queen of Roses", "mpvinewood_overlays", "MP_Vinewood_Tat_025_M", "MP_Vinewood_Tat_025_F",1000), new BusinessTattoo(new List<int>(){2}, "Skull & Aces", "mpvinewood_overlays", "MP_Vinewood_Tat_028_M", "MP_Vinewood_Tat_028_F",2000),   new BusinessTattoo(new List<int>(){2}, "Dollar Skull", "mpbusiness_overlays", "MP_Buis_M_RightArm_000", "",1780),   new BusinessTattoo(new List<int>(){1}, "Green", "mpbusiness_overlays", "MP_Buis_M_RightArm_001", "",1780),  new BusinessTattoo(new List<int>(){1}, "Dollar Sign", "mpbusiness_overlays", "", "MP_Buis_F_RArm_000",1800),    new BusinessTattoo(new List<int>(){2}, "Snake Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_003", "MP_Xmas2_F_Tat_003",1780),  new BusinessTattoo(new List<int>(){2}, "Snake Shaded", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_004", "MP_Xmas2_F_Tat_004",1850),   new BusinessTattoo(new List<int>(){1}, "Death Before Dishonor", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_008", "MP_Xmas2_F_Tat_008",1800),  new BusinessTattoo(new List<int>(){1}, "You're Next Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_022", "MP_Xmas2_F_Tat_022",850), new BusinessTattoo(new List<int>(){1}, "You're Next Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_023", "MP_Xmas2_F_Tat_023",1800),  new BusinessTattoo(new List<int>(){0}, "Fuck Luck Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_026", "MP_Xmas2_F_Tat_026",1250),  new BusinessTattoo(new List<int>(){0}, "Fuck Luck Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_027", "MP_Xmas2_F_Tat_027",1250),    new BusinessTattoo(new List<int>(){0}, "Grenade", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_002_M", "MP_Gunrunning_Tattoo_002_F",1250),    new BusinessTattoo(new List<int>(){2}, "Have a Nice Day", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_021_M", "MP_Gunrunning_Tattoo_021_F",1780),    new BusinessTattoo(new List<int>(){1}, "Combat Reaper", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_024_M", "MP_Gunrunning_Tattoo_024_F",1850),  new BusinessTattoo(new List<int>(){2}, "Single Arrow", "mphipster_overlays", "FM_Hip_M_Tat_001", "FM_Hip_F_Tat_001",1800),  new BusinessTattoo(new List<int>(){1}, "Bone", "mphipster_overlays", "FM_Hip_M_Tat_004", "FM_Hip_F_Tat_004",1800),  new BusinessTattoo(new List<int>(){2}, "Cube", "mphipster_overlays", "FM_Hip_M_Tat_008", "FM_Hip_F_Tat_008",1800),  new BusinessTattoo(new List<int>(){0}, "Horseshoe", "mphipster_overlays", "FM_Hip_M_Tat_010", "FM_Hip_F_Tat_010",1250), new BusinessTattoo(new List<int>(){1}, "Spray Can", "mphipster_overlays", "FM_Hip_M_Tat_014", "FM_Hip_F_Tat_014",1800), new BusinessTattoo(new List<int>(){1}, "Eye Triangle", "mphipster_overlays", "FM_Hip_M_Tat_017", "FM_Hip_F_Tat_017",1850),  new BusinessTattoo(new List<int>(){1}, "Origami", "mphipster_overlays", "FM_Hip_M_Tat_018", "FM_Hip_F_Tat_018",1800),   new BusinessTattoo(new List<int>(){1,2}, "Geo Pattern", "mphipster_overlays", "FM_Hip_M_Tat_020", "FM_Hip_F_Tat_020",3800), new BusinessTattoo(new List<int>(){1}, "Pencil", "mphipster_overlays", "FM_Hip_M_Tat_022", "FM_Hip_F_Tat_022",1800),    new BusinessTattoo(new List<int>(){0}, "Smiley", "mphipster_overlays", "FM_Hip_M_Tat_023", "FM_Hip_F_Tat_023",1300),    new BusinessTattoo(new List<int>(){2}, "Shapes", "mphipster_overlays", "FM_Hip_M_Tat_036", "FM_Hip_F_Tat_036",1800),    new BusinessTattoo(new List<int>(){2}, "Triangle Black", "mphipster_overlays", "FM_Hip_M_Tat_044", "FM_Hip_F_Tat_044",1800),    new BusinessTattoo(new List<int>(){1}, "Mesh Band", "mphipster_overlays", "FM_Hip_M_Tat_045", "FM_Hip_F_Tat_045",1850), new BusinessTattoo(new List<int>(){1,2}, "Mechanical Sleeve", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_003_M", "MP_MP_ImportExport_Tat_003_F",3800),  new BusinessTattoo(new List<int>(){1,2}, "Dialed In", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_005_M", "MP_MP_ImportExport_Tat_005_F",3850),  new BusinessTattoo(new List<int>(){1,2}, "Engulfed Block", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_006_M", "MP_MP_ImportExport_Tat_006_F",3800), new BusinessTattoo(new List<int>(){1,2}, "Drive Forever", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_007_M", "MP_MP_ImportExport_Tat_007_F",3800),  new BusinessTattoo(new List<int>(){1}, "Seductress", "mplowrider_overlays", "MP_LR_Tat_015_M", "MP_LR_Tat_015_F",1980), new BusinessTattoo(new List<int>(){2}, "Swooping Eagle", "mpbiker_overlays", "MP_MP_Biker_Tat_007_M", "MP_MP_Biker_Tat_007_F",1800),    new BusinessTattoo(new List<int>(){2}, "Lady Mortality", "mpbiker_overlays", "MP_MP_Biker_Tat_014_M", "MP_MP_Biker_Tat_014_F",1850),    new BusinessTattoo(new List<int>(){2}, "Eagle Emblem", "mpbiker_overlays", "MP_MP_Biker_Tat_033_M", "MP_MP_Biker_Tat_033_F",1980),  new BusinessTattoo(new List<int>(){1}, "Grim Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_042_M", "MP_MP_Biker_Tat_042_F",1850),    new BusinessTattoo(new List<int>(){2}, "Skull Chain", "mpbiker_overlays", "MP_MP_Biker_Tat_046_M", "MP_MP_Biker_Tat_046_F",1800),   new BusinessTattoo(new List<int>(){1,2}, "Snake Bike", "mpbiker_overlays", "MP_MP_Biker_Tat_047_M", "MP_MP_Biker_Tat_047_F",3800),  new BusinessTattoo(new List<int>(){2}, "These Colors Don't Run", "mpbiker_overlays", "MP_MP_Biker_Tat_049_M", "MP_MP_Biker_Tat_049_F",1800),    new BusinessTattoo(new List<int>(){2}, "Mum", "mpbiker_overlays", "MP_MP_Biker_Tat_054_M", "MP_MP_Biker_Tat_054_F",1850),   new BusinessTattoo(new List<int>(){1}, "Lady Vamp", "mplowrider2_overlays", "MP_LR_Tat_003_M", "MP_LR_Tat_003_F",1780), new BusinessTattoo(new List<int>(){2}, "Loving Los Muertos", "mplowrider2_overlays", "MP_LR_Tat_028_M", "MP_LR_Tat_028_F",1850),    new BusinessTattoo(new List<int>(){1}, "Black Tears", "mplowrider2_overlays", "MP_LR_Tat_035_M", "MP_LR_Tat_035_F",1850),   new BusinessTattoo(new List<int>(){1}, "Floral Raven", "mpluxe_overlays", "MP_LUXE_TAT_004_M", "MP_LUXE_TAT_004_F",1800),   new BusinessTattoo(new List<int>(){1,2}, "Mermaid Harpist", "mpluxe_overlays", "MP_LUXE_TAT_013_M", "MP_LUXE_TAT_013_F",3800),  new BusinessTattoo(new List<int>(){2}, "Geisha Bloom", "mpluxe_overlays", "MP_LUXE_TAT_019_M", "MP_LUXE_TAT_019_F",1780),   new BusinessTattoo(new List<int>(){1}, "Intrometric", "mpluxe2_overlays", "MP_LUXE_TAT_010_M", "MP_LUXE_TAT_010_F",1780),   new BusinessTattoo(new List<int>(){2}, "Heavenly Deity", "mpluxe2_overlays", "MP_LUXE_TAT_017_M", "MP_LUXE_TAT_017_F",1750),    new BusinessTattoo(new List<int>(){2}, "Floral Print", "mpluxe2_overlays", "MP_LUXE_TAT_026_M", "MP_LUXE_TAT_026_F",1800),  new BusinessTattoo(new List<int>(){1,2}, "Geometric Design RA", "mpluxe2_overlays", "MP_LUXE_TAT_030_M", "MP_LUXE_TAT_030_F",3800), new BusinessTattoo(new List<int>(){1}, "Crackshot", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_001_M", "MP_Smuggler_Tattoo_001_F",1800),    new BusinessTattoo(new List<int>(){2}, "Mutiny", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_005_M", "MP_Smuggler_Tattoo_005_F",1980),   new BusinessTattoo(new List<int>(){1,2}, "Stylized Kraken", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_023_M", "MP_Smuggler_Tattoo_023_F",3800),    new BusinessTattoo(new List<int>(){1}, "Poison Wrench", "mpstunt_overlays", "MP_MP_Stunt_Tat_003_M", "MP_MP_Stunt_Tat_003_F",1750), new BusinessTattoo(new List<int>(){2}, "Arachnid of Death", "mpstunt_overlays", "MP_MP_Stunt_Tat_009_M", "MP_MP_Stunt_Tat_009_F",1850), new BusinessTattoo(new List<int>(){2}, "Grave Vulture", "mpstunt_overlays", "MP_MP_Stunt_Tat_010_M", "MP_MP_Stunt_Tat_010_F",1780), new BusinessTattoo(new List<int>(){1,2}, "Coffin Racer", "mpstunt_overlays", "MP_MP_Stunt_Tat_016_M", "MP_MP_Stunt_Tat_016_F",3800),    new BusinessTattoo(new List<int>(){0}, "Biker Stallion", "mpstunt_overlays", "MP_MP_Stunt_Tat_036_M", "MP_MP_Stunt_Tat_036_F",1250),    new BusinessTattoo(new List<int>(){1}, "One Down Five Up", "mpstunt_overlays", "MP_MP_Stunt_Tat_038_M", "MP_MP_Stunt_Tat_038_F",1850),  new BusinessTattoo(new List<int>(){1,2}, "Seductive Mechanic", "mpstunt_overlays", "MP_MP_Stunt_Tat_049_M", "MP_MP_Stunt_Tat_049_F",3800),  new BusinessTattoo(new List<int>(){2}, "Grim Reaper Smoking Gun", "multiplayer_overlays", "FM_Tat_Award_M_002", "FM_Tat_Award_F_002",1850), new BusinessTattoo(new List<int>(){1}, "Ride or Die RA", "multiplayer_overlays", "FM_Tat_Award_M_010", "FM_Tat_Award_F_010",1800),  new BusinessTattoo(new List<int>(){1,2}, "Brotherhood", "multiplayer_overlays", "FM_Tat_M_000", "FM_Tat_F_000",3800),   new BusinessTattoo(new List<int>(){1,2}, "Dragons", "multiplayer_overlays", "FM_Tat_M_001", "FM_Tat_F_001",3800),   new BusinessTattoo(new List<int>(){2}, "Dragons and Skull", "multiplayer_overlays", "FM_Tat_M_003", "FM_Tat_F_003",1850),   new BusinessTattoo(new List<int>(){1,2}, "Flower Mural", "multiplayer_overlays", "FM_Tat_M_014", "FM_Tat_F_014",3800),  new BusinessTattoo(new List<int>(){1,2,0}, "Serpent Skull RA", "multiplayer_overlays", "FM_Tat_M_018", "FM_Tat_F_018",4500),    new BusinessTattoo(new List<int>(){2}, "Virgin Mary", "multiplayer_overlays", "FM_Tat_M_027", "FM_Tat_F_027",1850), new BusinessTattoo(new List<int>(){1}, "Mermaid", "multiplayer_overlays", "FM_Tat_M_028", "FM_Tat_F_028",1850), new BusinessTattoo(new List<int>(){1}, "Dagger", "multiplayer_overlays", "FM_Tat_M_038", "FM_Tat_F_038",1800),  new BusinessTattoo(new List<int>(){2}, "Lion", "multiplayer_overlays", "FM_Tat_M_047", "FM_Tat_F_047",1800),

                
              
            },

            // LeftLeg
            new List<BusinessTattoo>()
            {
	            // До колена    -   0
                // Выше колена  -   1

                //Новое
                new BusinessTattoo(new List<int>(){0},"One-armed Bandit", "mpvinewood_overlays", "MP_Vinewood_Tat_013_M", "MP_Vinewood_Tat_013_F",1850),    new BusinessTattoo(new List<int>(){0}, "8-Ball Rose", "mpvinewood_overlays", "MP_Vinewood_Tat_027_M", "MP_Vinewood_Tat_027_F",2500),    new BusinessTattoo(new List<int>(){0}, "Single", "mpbusiness_overlays", "", "MP_Buis_F_LLeg_000",1850), new BusinessTattoo(new List<int>(){0}, "Spider Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_001", "MP_Xmas2_F_Tat_001",1850), new BusinessTattoo(new List<int>(){0}, "Spider Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_002", "MP_Xmas2_F_Tat_002",1850),   new BusinessTattoo(new List<int>(){0}, "Patriot Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_005_M", "MP_Gunrunning_Tattoo_005_F",1850),  new BusinessTattoo(new List<int>(){1}, "Stylized Tiger", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_007_M", "MP_Gunrunning_Tattoo_007_F",1800), new BusinessTattoo(new List<int>(){0,1}, "Death Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_011_M", "MP_Gunrunning_Tattoo_011_F",3500),  new BusinessTattoo(new List<int>(){1}, "Rose Revolver", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_023_M", "MP_Gunrunning_Tattoo_023_F",1850),  new BusinessTattoo(new List<int>(){0}, "Squares", "mphipster_overlays", "FM_Hip_M_Tat_009", "FM_Hip_F_Tat_009",1800),   new BusinessTattoo(new List<int>(){0}, "Charm", "mphipster_overlays", "FM_Hip_M_Tat_019", "FM_Hip_F_Tat_019",1850), new BusinessTattoo(new List<int>(){0}, "Black Anchor", "mphipster_overlays", "FM_Hip_M_Tat_040", "FM_Hip_F_Tat_040",1800),  new BusinessTattoo(new List<int>(){0}, "LS Serpent", "mplowrider_overlays", "MP_LR_Tat_007_M", "MP_LR_Tat_007_F",1850), new BusinessTattoo(new List<int>(){0}, "Presidents", "mplowrider_overlays", "MP_LR_Tat_020_M", "MP_LR_Tat_020_F",1800), new BusinessTattoo(new List<int>(){0}, "Rose Tribute", "mpbiker_overlays", "MP_MP_Biker_Tat_002_M", "MP_MP_Biker_Tat_002_F",1850),  new BusinessTattoo(new List<int>(){0}, "Ride or Die LL", "mpbiker_overlays", "MP_MP_Biker_Tat_015_M", "MP_MP_Biker_Tat_015_F",1800),    new BusinessTattoo(new List<int>(){0}, "Bad Luck", "mpbiker_overlays", "MP_MP_Biker_Tat_027_M", "MP_MP_Biker_Tat_027_F",1850),  new BusinessTattoo(new List<int>(){0}, "Engulfed Skull", "mpbiker_overlays", "MP_MP_Biker_Tat_036_M", "MP_MP_Biker_Tat_036_F",1850),    new BusinessTattoo(new List<int>(){1}, "Scorched Soul", "mpbiker_overlays", "MP_MP_Biker_Tat_037_M", "MP_MP_Biker_Tat_037_F",1850), new BusinessTattoo(new List<int>(){1}, "Ride Free", "mpbiker_overlays", "MP_MP_Biker_Tat_044_M", "MP_MP_Biker_Tat_044_F",1850), new BusinessTattoo(new List<int>(){1}, "Bone Cruiser", "mpbiker_overlays", "MP_MP_Biker_Tat_056_M", "MP_MP_Biker_Tat_056_F",1850),  new BusinessTattoo(new List<int>(){0,1}, "Laughing Skull", "mpbiker_overlays", "MP_MP_Biker_Tat_057_M", "MP_MP_Biker_Tat_057_F",3500),  new BusinessTattoo(new List<int>(){0}, "Death Us Do Part", "mplowrider2_overlays", "MP_LR_Tat_029_M", "MP_LR_Tat_029_F",1850),  new BusinessTattoo(new List<int>(){0}, "Serpent of Death", "mpluxe_overlays", "MP_LUXE_TAT_000_M", "MP_LUXE_TAT_000_F",1850),   new BusinessTattoo(new List<int>(){0}, "Cross of Roses", "mpluxe2_overlays", "MP_LUXE_TAT_011_M", "MP_LUXE_TAT_011_F",1850),    new BusinessTattoo(new List<int>(){0}, "Dagger Devil", "mpstunt_overlays", "MP_MP_Stunt_Tat_007_M", "MP_MP_Stunt_Tat_007_F",1780),  new BusinessTattoo(new List<int>(){1}, "Dirt Track Hero", "mpstunt_overlays", "MP_MP_Stunt_Tat_013_M", "MP_MP_Stunt_Tat_013_F",1800),   new BusinessTattoo(new List<int>(){0,1}, "Golden Cobra", "mpstunt_overlays", "MP_MP_Stunt_Tat_021_M", "MP_MP_Stunt_Tat_021_F",3500),    new BusinessTattoo(new List<int>(){0}, "Quad Goblin", "mpstunt_overlays", "MP_MP_Stunt_Tat_028_M", "MP_MP_Stunt_Tat_028_F",1800),   new BusinessTattoo(new List<int>(){0}, "Stunt Jesus", "mpstunt_overlays", "MP_MP_Stunt_Tat_031_M", "MP_MP_Stunt_Tat_031_F",1850),   new BusinessTattoo(new List<int>(){0}, "Dragon and Dagger", "multiplayer_overlays", "FM_Tat_Award_M_009", "FM_Tat_Award_F_009",1850),   new BusinessTattoo(new List<int>(){0}, "Melting Skull", "multiplayer_overlays", "FM_Tat_M_002", "FM_Tat_F_002",1850),   new BusinessTattoo(new List<int>(){0}, "Dragon Mural", "multiplayer_overlays", "FM_Tat_M_008", "FM_Tat_F_008",1850),    new BusinessTattoo(new List<int>(){0}, "Serpent Skull LL", "multiplayer_overlays", "FM_Tat_M_021", "FM_Tat_F_021",1850),    new BusinessTattoo(new List<int>(){0}, "Hottie", "multiplayer_overlays", "FM_Tat_M_023", "FM_Tat_F_023",1850),  new BusinessTattoo(new List<int>(){0}, "Smoking Dagger", "multiplayer_overlays", "FM_Tat_M_026", "FM_Tat_F_026",1850),  new BusinessTattoo(new List<int>(){0}, "Faith LL", "multiplayer_overlays", "FM_Tat_M_032", "FM_Tat_F_032",1850),    new BusinessTattoo(new List<int>(){0,1}, "Chinese Dragon", "multiplayer_overlays", "FM_Tat_M_033", "FM_Tat_F_033",3500),    new BusinessTattoo(new List<int>(){0}, "Dragon LL", "multiplayer_overlays", "FM_Tat_M_035", "FM_Tat_F_035",1800),   new BusinessTattoo(new List<int>(){0}, "Grim Reaper", "multiplayer_overlays", "FM_Tat_M_037", "FM_Tat_F_037",1850),


              
            },
            
            // RightLeg
            new List<BusinessTattoo>()
            {
	            // До колена    -   0
                // Выше колена  -   1

                //Новое
                new BusinessTattoo(new List<int>(){0},"Cash is King", "mpvinewood_overlays", "MP_Vinewood_Tat_020_M", "MP_Vinewood_Tat_020_F",2500),    new BusinessTattoo(new List<int>(){0}, "Diamond Crown", "mpbusiness_overlays", "", "MP_Buis_F_RLeg_000",1800),  new BusinessTattoo(new List<int>(){0}, "Floral Dagger", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_014", "MP_Xmas2_F_Tat_014",1750),  new BusinessTattoo(new List<int>(){0}, "Combat Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_006_M", "MP_Gunrunning_Tattoo_006_F",1800),   new BusinessTattoo(new List<int>(){0}, "Restless Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_026_M", "MP_Gunrunning_Tattoo_026_F",1850), new BusinessTattoo(new List<int>(){1}, "Pistol Ace", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_030_M", "MP_Gunrunning_Tattoo_030_F",16850),    new BusinessTattoo(new List<int>(){0}, "Grub", "mphipster_overlays", "FM_Hip_M_Tat_038", "FM_Hip_F_Tat_038",1800),  new BusinessTattoo(new List<int>(){0}, "Sparkplug", "mphipster_overlays", "FM_Hip_M_Tat_042", "FM_Hip_F_Tat_042",1800), new BusinessTattoo(new List<int>(){0}, "Ink Me", "mplowrider_overlays", "MP_LR_Tat_017_M", "MP_LR_Tat_017_F",1800), new BusinessTattoo(new List<int>(){0}, "Dance of Hearts", "mplowrider_overlays", "MP_LR_Tat_023_M", "MP_LR_Tat_023_F",1850),    new BusinessTattoo(new List<int>(){0,1}, "Dragon's Fury", "mpbiker_overlays", "MP_MP_Biker_Tat_004_M", "MP_MP_Biker_Tat_004_F",3500),   new BusinessTattoo(new List<int>(){0}, "Western Insignia", "mpbiker_overlays", "MP_MP_Biker_Tat_022_M", "MP_MP_Biker_Tat_022_F",1800),  new BusinessTattoo(new List<int>(){1}, "Dusk Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_028_M", "MP_MP_Biker_Tat_028_F",1800),    new BusinessTattoo(new List<int>(){1}, "American Made", "mpbiker_overlays", "MP_MP_Biker_Tat_040_M", "MP_MP_Biker_Tat_040_F",1850), new BusinessTattoo(new List<int>(){0}, "STFU", "mpbiker_overlays", "MP_MP_Biker_Tat_048_M", "MP_MP_Biker_Tat_048_F",1800),  new BusinessTattoo(new List<int>(){0}, "San Andreas Prayer", "mplowrider2_overlays", "MP_LR_Tat_030_M", "MP_LR_Tat_030_F",1850),    new BusinessTattoo(new List<int>(){0}, "Elaborate Los Muertos", "mpluxe_overlays", "MP_LUXE_TAT_001_M", "MP_LUXE_TAT_001_F",1850),  new BusinessTattoo(new List<int>(){0}, "Starmetric", "mpluxe2_overlays", "MP_LUXE_TAT_023_M", "MP_LUXE_TAT_023_F",1750),    new BusinessTattoo(new List<int>(){0,1}, "Homeward Bound", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_020_M", "MP_Smuggler_Tattoo_020_F",3500), new BusinessTattoo(new List<int>(){0}, "Demon Spark Plug", "mpstunt_overlays", "MP_MP_Stunt_Tat_005_M", "MP_MP_Stunt_Tat_005_F",1850),  new BusinessTattoo(new List<int>(){1}, "Praying Gloves", "mpstunt_overlays", "MP_MP_Stunt_Tat_015_M", "MP_MP_Stunt_Tat_015_F",1850),    new BusinessTattoo(new List<int>(){0}, "Piston Angel", "mpstunt_overlays", "MP_MP_Stunt_Tat_020_M", "MP_MP_Stunt_Tat_020_F",1850),  new BusinessTattoo(new List<int>(){1}, "Speed Freak", "mpstunt_overlays", "MP_MP_Stunt_Tat_025_M", "MP_MP_Stunt_Tat_025_F",1800),   new BusinessTattoo(new List<int>(){0}, "Wheelie Mouse", "mpstunt_overlays", "MP_MP_Stunt_Tat_032_M", "MP_MP_Stunt_Tat_032_F",1750), new BusinessTattoo(new List<int>(){0,1}, "Severed Hand", "mpstunt_overlays", "MP_MP_Stunt_Tat_045_M", "MP_MP_Stunt_Tat_045_F",3500),    new BusinessTattoo(new List<int>(){0}, "Brake Knife", "mpstunt_overlays", "MP_MP_Stunt_Tat_047_M", "MP_MP_Stunt_Tat_047_F",1750),   new BusinessTattoo(new List<int>(){0}, "Skull and Sword", "multiplayer_overlays", "FM_Tat_Award_M_006", "FM_Tat_Award_F_006",1850), new BusinessTattoo(new List<int>(){0}, "The Warrior", "multiplayer_overlays", "FM_Tat_M_007", "FM_Tat_F_007",1850), new BusinessTattoo(new List<int>(){0}, "Tribal", "multiplayer_overlays", "FM_Tat_M_017", "FM_Tat_F_017",1800),  new BusinessTattoo(new List<int>(){0}, "Fiery Dragon", "multiplayer_overlays", "FM_Tat_M_022", "FM_Tat_F_022",1850),    new BusinessTattoo(new List<int>(){0}, "Broken Skull", "multiplayer_overlays", "FM_Tat_M_039", "FM_Tat_F_039",1850),    new BusinessTattoo(new List<int>(){0,1}, "Flaming Skull", "multiplayer_overlays", "FM_Tat_M_040", "FM_Tat_F_040",3400), new BusinessTattoo(new List<int>(){0}, "Flaming Scorpion", "multiplayer_overlays", "FM_Tat_M_042", "FM_Tat_F_042",1850),    new BusinessTattoo(new List<int>(){0}, "Indian Ram", "multiplayer_overlays", "FM_Tat_M_043", "FM_Tat_F_043",1850),


             
            }

        };
   public static Dictionary<string, Dictionary<int, List<Tuple<int, string, int>>>> Tuning = new Dictionary<string, Dictionary<int, List<Tuple<int, string, int>>>>()
        {

        };
        public static Dictionary<int, Dictionary<string, int>> TuningPrices = new Dictionary<int, Dictionary<string, int>>()
        {
            { 0, new Dictionary<string, int>() { // Глушитель
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
            }},
             { 1, new Dictionary<string, int>() { //Пороги
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
            }},
              { 2, new Dictionary<string, int>() { // Капот
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
                { "16", 900 },
                { "17", 900 },
                { "18", 900 },
                { "19", 900 },
                { "20", 900 },
            }},
             { 3, new Dictionary<string, int>() { //Спойлер
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
                { "16", 900 },
                { "17", 900 },
                { "18", 900 },
                { "19", 900 },
                { "20", 900 },
            }},
             { 4, new Dictionary<string, int>() { // Решетка
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
                { "16", 900 },
                { "17", 900 },
                { "18", 900 },
                { "19", 900 },
                { "20", 900 },
            }},
            { 5, new Dictionary<string, int>() { // Разширение
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
                { "16", 900 },
                { "17", 900 },
                { "18", 900 },
                { "19", 900 },
                { "20", 900 },
            }},
             { 6, new Dictionary<string, int>() { // Крыша
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
            }},
             { 7, new Dictionary<string, int>() { //Винил
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
                { "16", 900 },
                { "17", 900 },
                { "18", 900 },
                { "19", 900 },
                { "20", 900 },
            }},
            { 8, new Dictionary<string, int>() { // Пер Бампер
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
                { "16", 900 },
                { "17", 900 },
                { "18", 900 },
                { "19", 900 },
                { "20", 900 },
            }},
             { 9, new Dictionary<string, int>() { // Зад бамп
                { "-1", 700 },
                { "0", 900 },
                { "1", 900 },
                { "2", 900 },
                { "3", 900 },
                { "4", 900 },
                { "5", 900 },
                { "6", 900 },
                { "7", 900 },
                { "8", 900 },
                { "9", 900 },
                { "10", 900 },
                { "11", 900 },
                { "12", 900 },
                { "13", 900 },
                { "14", 900 },
                { "15", 900 },
                { "16", 900 },
                { "17", 900 },
                { "18", 900 },
                { "19", 900 },
                { "20", 900 },
            }},
            { 10, new Dictionary<string, int>() { // engine_menu
                { "-1", 888 },
                { "0", 900 },
                { "1", 1050 },
                { "2", 1200 },
                { "3", 6950 },
            }},
            { 11, new Dictionary<string, int>() { // turbo_menu
                { "-1", 1200 },
                { "0", 12500 },
            }},
            { 12, new Dictionary<string, int>() { // horn_menu
                { "-1", 500 },
                { "0", 1000 },
                { "1", 1000 },
                { "2", 1000 },
                { "3", 1000 },
                { "4", 1000 },
                { "5", 1000 },
                { "6", 1000 },
                { "7", 1000 },
                { "8", 1000 },
                { "9", 1000 },
                { "10", 1000 },
                { "11", 1000 },
                { "12", 1000 },
                { "13", 1000 },
                { "14", 1000 },
                { "15", 1000 },
                { "16", 1000 },
                { "17", 1000 },
                { "18", 1000 },
                { "19", 1000 },
                { "20", 1000 },
                { "21", 1000 },
                { "22", 1000 },
                { "23", 1000 },
                { "24", 1000 },
                { "25", 1000 },
                { "26", 1000 },
                { "27", 1000 },
                { "28", 1000 },
                { "29", 1000 },
                { "30", 1000 },
                { "31", 1000 },
                { "32", 1000 },
                { "33", 1000 },
                { "34", 1000 },
            }},
            { 13, new Dictionary<string, int>() { // transmission_menu
                { "-1", 500 },
                { "0", 1600 },
                { "1", 2050 },
                { "2", 10200 },
            }},
            { 14, new Dictionary<string, int>() { // glasses_menu
                { "0", 500 },
                { "3", 650 },
                { "2", 700 },
                { "1", 900 },
            }},
            { 15, new Dictionary<string, int>() { // suspention_menu
                { "-1", 500 },
                { "0", 300 },
                { "1", 600 },
                { "2", 900 },
                { "3", 1200 },
            }},
            { 16, new Dictionary<string, int>() { // brakes_menu
                { "-1", 700 },
                { "0", 1400 },
                { "1", 2800 },
                { "2", 3333 },
            }},
            { 17, new Dictionary<string, int>() { // lights_menu
                { "-1", 1000 },
                { "0", 3300 },
                { "1", 3300 },
                { "2", 3300 },
                { "3", 3300 },
                { "4", 3300 },
                { "5", 3300 },
                { "6", 3300 },
                { "7", 3300 },
                { "8", 3300 },
                { "9", 3300 },
                { "10", 3300 },
                { "11", 3300 },
                { "12", 3300 },
            }},
            { 18, new Dictionary<string, int>() { // numbers_menu
                { "0", 100 },
                { "1", 200 },
                { "2", 200 },
                { "3", 200 },
                { "4", 200 },
            }},
        };

        public static Dictionary<int, Dictionary<int, int>> TuningWheels = new Dictionary<int, Dictionary<int, int>>()
        {
            // спортивные
            { 0, new Dictionary<int, int>() {
                { -1, 1000 },
                { 57, 2500 },
                { 58, 2500 },
                { 59, 2500 },
                { 60, 2500 },
                { 61, 2500 },
                { 62, 2500 },
                { 63, 2500 },
                { 64, 2500 },
                { 65, 2500 },
                { 66, 2500 },
                { 67, 2500 },
                { 68, 2500 },
                { 69, 2500 },
                { 70, 2500 },
                { 71, 2500 },
                { 72, 2500 },
                { 73, 2500 },
                { 74, 2500 },
                { 75, 2500 },
                { 76, 2500 },
                { 77, 2500 },
                { 78, 2500 },
                { 79, 2500 },
                { 80, 2500 },
                { 81, 2500 },
                { 82, 2500 },
                { 83, 2500 },
                { 84, 2500 },
                { 85, 2500 },
                { 86, 2500 },
                { 87, 2500 },
                { 88, 2500 },
                { 89, 2500 },
                { 90, 2500 },
                { 91, 2500 },
                { 92, 2500 },
                { 93, 2500 },
                { 94, 2500 },
                { 95, 2500 },
                { 96, 2500 },
                { 97, 2500 },
                { 98, 2500 },
                { 99, 2500 },
                { 100, 2500 },
                { 101, 2500 },
                { 102, 2500 },
                { 103, 2500 },
                { 104, 2500 },
                { 105, 2500 },
                { 106, 2500 },
                { 107, 2500 },
                { 108, 2500 },
                { 109, 2500 },
                { 110, 2500 },
                { 111, 2500 },
                { 112, 2500 },
                { 113, 2500 },
                { 114, 2500 },
                { 115, 2500 },
                { 116, 2500 },
                { 117, 2500 },
                { 118, 2500 },
                { 119, 2500 },
                { 120, 2500 },
                { 121, 2500 },
                { 122, 2500 },
                { 123, 2500 },
                { 124, 2500 },
                { 125, 2500 },
                { 126, 2500 },
                { 127, 2500 },
                { 128, 2500 },
                { 129, 2500 },
                { 130, 2500 },
                { 131, 2500 },
                { 132, 2500 },
                { 133, 2500 },
                { 134, 2500 },
                { 135, 2500 },
                { 136, 2500 },
                { 137, 2500 },
                { 138, 2500 },
                { 139, 2500 },
                { 140, 2500 },
                { 141, 2500 },
                { 142, 2500 },
                { 143, 2500 },
                { 144, 2500 },
                { 145, 2500 },
                { 146, 2500 },
            }},
            // маслкары
            { 1, new Dictionary<int, int>() {
                { -1, 1000 },
                { 0, 2500 },
                { 1, 2500 },
                { 2, 2500 },
                { 3, 2500 },
                { 4, 2500 },
                { 5, 2500 },
                { 6, 2500 },
                { 7, 2500 },
                { 8, 2500 },
                { 9, 2500 },
                { 10, 2500 },
                { 11, 2500 },
                { 12, 2500 },
                { 13, 2500 },
                { 14, 2500 },
                { 15, 2500 },
                { 16, 2500 },
                { 17, 2500 },
            }},
            // лоурайдер
            { 2, new Dictionary<int, int>() {
                { -1, 1000 },
                { 0, 2500 },
                { 1, 2500 },
                { 2, 2500 },
                { 3, 2500 },
                { 4, 2500 },
                { 5, 2500 },
                { 6, 2500 },
                { 7, 2500 },
                { 8, 2500 },
                { 9, 2500 },
                { 10, 2500 },
                { 11, 2500 },
                { 12, 2500 },
                { 13, 2500 },
                { 14, 2500 },
            }},
            // вездеход
            { 3, new Dictionary<int, int>() {
                { -1, 1000 },
                { 0, 2500 },
                { 1, 2500 },
                { 2, 2500 },
                { 3, 2500 },
                { 4, 2500 },
                { 5, 2500 },
                { 6, 2500 },
                { 7, 2500 },
                { 8, 2500 },
                { 9, 2500 },
            }},
            // внедорожник
            { 4, new Dictionary<int, int>() {
                { -1, 1000 },
                { 0, 2500 },
                { 1, 2500 },
                { 2, 2500 },
                { 3, 2500 },
                { 4, 2500 },
                { 5, 2500 },
                { 6, 2500 },
                { 7, 2500 },
                { 8, 2500 },
                { 9, 2500 },
                { 10, 2500 },
                { 11, 2500 },
                { 12, 2500 },
                { 13, 2500 },
                { 14, 2500 },
                { 15, 2500 },
                { 16, 2500 },
            }},
            // тюннер
            { 5, new Dictionary<int, int>() {
                { -1, 1000 },
                { 0, 2500 },
                { 1, 2500 },
                { 2, 2500 },
                { 3, 2500 },
                { 4, 2500 },
                { 5, 2500 },
                { 6, 2500 },
                { 7, 2500 },
                { 8, 2500 },
                { 9, 2500 },
                { 10, 2500 },
                { 11, 2500 },
                { 12, 2500 },
                { 13, 2500 },
                { 14, 2500 },
                { 15, 2500 },
                { 16, 2500 },
                { 17, 2500 },
                { 18, 2500 },
                { 19, 2500 },
                { 20, 2500 },
                { 21, 2500 },
                { 22, 2500 },
                { 23, 2500 },
            }},
            // эксклюзивные
            { 7, new Dictionary<int, int>() {
                { -1, 1000 },
                { 0, 2500 },
                { 1, 2500 },
                { 2, 2500 },
                { 3, 2500 },
                { 4, 2500 },
                { 5, 2500 },
                { 6, 2500 },
                { 7, 2500 },
                { 8, 2500 },
                { 9, 2500 },
                { 10, 2500 },
                { 11, 2500 },
                { 12, 2500 },
                { 13, 2500 },
                { 14, 2500 },
                { 15, 2500 },
                { 16, 2500 },
                { 17, 2500 },
                { 18, 2500 },
                { 19, 2500 },
            }},
        };

        public static Dictionary<string, int> ProductsCapacity = new Dictionary<string, int>()
        {
            { "Расходники", 800 }, // tattoo shop
            { "Татуировки", 0 },
            { "Парики", 0 }, // barber-shop
            { "Бургер", 250}, // burger-shot
            { "Хот-Дог", 100},
            { "Сэндвич", 100},
            { "eCola", 100},
            { "Sprunk", 100},
            { "Монтировка", 50}, // market
            { "Фонарик", 50},
            { "Молоток", 50},
            { "Гаечный ключ", 50},
            { "Канистра бензина", 50},
            { "Чипсы", 50},
            { "Вода", 50},
            { "Пицца", 50},
            { "Сим-карта", 50},
            { "Связка ключей", 50},
            { "Рем. Комплект", 150},
            { "Аптечка", 150},
            { "Бензин", 20000}, // petrol
            { "Одежда", 7000}, // clothes
            { "Маски", 100}, // masks
            { "Запчасти", 10000}, // ls customs
            { "Средство для мытья", 200 }, // carwash
            { "Корм для животных", 20 }, // petshop
            { "Бонг", 200 }, // carwash
            { "Зажигалка", 20 }, // petshop


            { "18rs7", 10 }, // luxe
            { "2019m5", 10 },
            { "amggt16", 10 },
            { "bentayga", 10 },
            { "bentley20", 10 },
            { "bmw730", 10 },
            { "bmwm2", 10 },
            { "bmwz4", 10 },
            { "gta5rp_veh_c63s", 10 },
            { "c63scoupe", 10 },
            { "camry70", 10 },
            { "carrera19", 10 },
            { "cls17", 10 },
            { "evo9", 10 },
            { "evo10", 10 },
            { "f12", 10 },
            { "gta5rp_veh_ferrari19", 10 },
            { "gallardo", 10 },
            { "giulia", 10 },
            { "golf7", 10 },
            { "huracan", 10 },
            { "gta5rp_veh_impreza98", 10 },
            { "impreza18", 10 },
            { "m8", 10 },
            { "maybach", 10 },
            { "gta5rp_veh_minic", 10 },
            { "models", 10 },
            { "mustang19", 10 },
            { "mustang65", 10 },
            { "p90d", 10 },
            { "panamera", 10 },
            { "pullman", 10 },
            { "rrghost", 10 },
            { "rrwraith", 10 },
            { "rs6", 10 },
            { "teslaroad", 10 },
            { "gt63", 10 },
            { "bmwg05", 10 }, // luxe
            { "bmwg07", 10 },
            { "cayen19", 10 },
            { "cullinan", 10 },
            { "escalade19", 10 },
            { "gta5rp_veh_fx50s", 10 },
            { "g63", 10 },
            { "gl63", 10 },
            { "gta5rp_veh_gle1", 10 },
            { "jeep15", 10 },
            { "lc200", 10 },
            { "merc6x6", 10 },
            { "navigator19", 10 },
            { "rrover17", 10 },
            { "suburban800", 10 },
            { "svr", 10 },
            { "gta5rp_veh_tahoe1", 10 },
            { "urus", 10 },
            { "gta5rp_veh_wald2018", 10 },
            { "gta5rp_veh_x5e53", 10 },
            { "x6m18", 10 },
            { "gta5rp_veh_r50", 10 },
            { "a45", 10 }, // luxe
            { "a80", 10 },
            { "a90", 10 },
            { "ae86", 10 },
            { "bmwe28", 10 },
            { "bmwe34", 10 },
            { "bmwe36", 10 },
            { "bmwe38", 10 },
            { "bmwe39", 10 },
            { "bmwe46", 10 },
            { "gta5rp_veh_cam08", 10 },
            { "camry35", 10 },
            { "cls08", 10 },
            { "e55", 10 },
            { "evo6", 10 },
            { "golf1", 10 },
            { "gta5rp_veh_gtr33", 10 },
            { "gta5rp_veh_gtr34", 10 },
            { "impreza08", 10 },
            { "kiastinger", 10 },
            { "gta5rp_veh_m5e60", 10 },
            { "mark100", 10 },

            { "nsx95", 10 },
            { "s15", 10 },
            { "w140", 10 },
            {  "chiron", 10 },
            { "gemera", 10},
            { "maseratigt", 10 },
             {   "f150", 10 },
              {  "vulcan", 10 },
               { "phuayra", 10 } ,
                {"senna", 10 },
                {"v250", 10} ,
                {"veyron", 10},

                {"i8", 10},
                {"exp100", 10},
                {"minifiat", 10},
                {"minicar", 10},
                {"agerars", 10},

                {"jesko", 10},
                {"rcf", 10},
                {"lfa", 10},
                {"slr", 10},
                {"cyber", 10},
                {"scooby ", 10},
                {"240z ", 10},
                {"350z ", 10},

                {"acursx ", 10},

                {"carreragt", 10},
                {"civic19", 10},
                {"db11", 10},
                {"deborah", 10},
                {"elemento", 10},
                {"fordraptor", 10},
                {"ftype15", 10},
                {"gt17", 10},
                {"gtr17", 10},
                {"gtr2000", 10},
                {"impala67", 10},
                {"lambsc18", 10},

                {"mc720s", 10},
                {"mclaren20", 10},
                {"gta5rp_veh_nisr32", 10},
                {"regera", 10},
                {"s13", 10},
                {"rx7", 10},
                {"sian", 10},
                {"sl300", 10},
                {"smalljoe", 10},
                {"veneno", 10},
                {"vision6", 10},
                {"zl1", 10},
                {"lykan", 10},
                {"avtr", 10},
                {"lada2170", 10},
                {"m4comp", 10},
                {"mlbrabus", 10 },
                {"rs5", 10 },
                {"taycan", 10 },
                {"ocnauda8l22h", 10 },
               { "180sx", 10 },
                { "harley1", 10 },
                { "r1m", 10 },
                { "rcv213", 10 },
                { "r111", 10 },
                { "r62008", 10 },
                { "fireblade", 10 },
                { "gsx18", 10 },
                { "h2r", 10 },
                { "panigale", 10 },
                { "zx10rr", 10 },
                { "rsq8m", 10 },
                { "school", 10 },
                { "deluxo", 10 },
                { "lada2107", 10 },




            { "Buzzard2", 10 },
            { "918", 10 },
            { "Mammatus", 10 },
            { "Luxor2", 10 },
            { "divo", 10 },
            { "caliburn", 10 },
            { "ocnrrvn100", 10 },
            { "taycants21m", 10 },
            { "eqg", 10 },
            
            { "rx7veilside", 10 },
            { "venenor", 10 },
            { "wycalt", 10 },
            { "monza", 10 },
            { "mvisiongt", 10 },
            { "rt70", 10 },
            { "bolide", 10 },
            { "ffrs", 10 },
            { "mercedesgls", 10 },

            { "Pistol", 20}, // gun shop
            { "CombatPistol", 20},
            { "Revolver", 20},
            { "HeavyPistol", 20},
            
            { "BullpupShotgun", 20},
            
            { "CombatPDW", 20},
            { "MachinePistol", 20},
            { "Патроны", 5000},

            #region FishShop
            { "Удочка", 10 },
            { "Улучшенная удочка", 10 },
            { "Удочка MK2", 10 },
            { "Наживка", 10 },
            #endregion FishShop
            #region SellShop
            { "Корюшка", 1 },
            { "Кунджа", 1 },
            { "Лосось", 1 },
            { "Окунь", 1 },
            { "Осётр", 1 },
            { "Скат", 1 },
            { "Тунец", 1 },
            { "Угорь", 1 },
            { "Чёрный амур", 1 },
            { "Щука", 1 }
            #endregion SellShop
        };
        public static Dictionary<string, int> ProductsOrderPrice = new Dictionary<string, int>()
        {
            {"Расходники",50},
            {"Татуировки",20},
            {"Парики",20},
            {"Бургер",100},
            {"Хот-Дог",60},
            {"Сэндвич",30},
            {"eCola",20},
            {"Sprunk",30},
            {"Монтировка",1000},
            {"Фонарик",1500},
            {"Молоток",1000},
            {"Гаечный ключ",1000},
            {"Канистра бензина",1500},
            {"Чипсы",250},
            {"Вода", 250},
            {"Пицца",500},
            {"Сим-карта",1000},
            {"Связка ключей",500},
            {"Рем. Комплект",2500},
            {"Аптечка",5000},
            {"Бензин",20},
            {"Одежда",50},
            {"Маски",2000},
            {"Запчасти",400},
            {"Средство для мытья",200},
            {"Корм для животных", 450000 }, // petshop
            { "Бонг", 100000 }, // carwash
            { "Зажигалка", 25000 }, // petshop
                   { "18rs7", 30000000 }, // luxe
            { "2019m5", 20000000 },
            { "amggt16", 25000000 },
            { "bentayga", 52000000 },
            { "bentley20", 53000000 },
            { "bmw730", 25400000 },
            { "bmwm2", 26000000 },
            { "bmwz4", 29000000 },
            { "gta5rp_veh_c63s", 40000000 },
            { "c63scoupe", 38000000 },
            { "camry70", 16000000 },
            { "carrera19", 23000000 },
            { "cls17", 15000000 },
            { "evo9", 10000000 },
            { "evo10", 12000000 },
            { "f12", 35000000 },
            { "gta5rp_veh_ferrari19", 62000000 },
            { "gallardo", 53500000 },
            { "giulia", 15000000 },
            { "golf7", 8000000 },
            { "huracan", 44500000 },
            { "gta5rp_veh_impreza98", 14400000 },
            { "impreza18", 24400000 },
            { "m8", 42000000 },
            { "maybach", 51000000 },
            { "gta5rp_veh_minic", 12000000 },
            { "models", 35500000 },
            { "mustang19", 31000000 },
            { "mustang65", 25500000 },
            { "p90d", 20000000 },
            { "panamera", 38500000 },
            { "pullman", 38000000 },
            { "rrghost", 40000000 },
            { "rrwraith", 45500000 },
            { "rs6", 22000000 },
            { "teslaroad", 52000000 },
            { "gt63", 54500000 },
            { "a45", 5500000 }, // luxe
            { "a80", 7000000 },
            { "a90", 7010000 },
            { "ae86", 5000000 },
            { "bmwe28", 5500000 },
            { "bmwe34", 5600000 },
            { "bmwe36", 5400000 },
            { "bmwe38", 5200000 },
            { "bmwe39", 5800000 },
            { "bmwe46", 5000000 },
            { "gta5rp_veh_cam08", 3000000 },
            { "camry35", 450000 },
            { "cls08", 7000000 },
            { "e55", 2500000 },
            { "evo6", 5000000 },
            { "golf1", 1000000 },
            { "gta5rp_veh_gtr33", 4550000 },
            { "gta5rp_veh_gtr34", 4670000 },
            { "impreza08", 1000000 },
            { "kiastinger", 13000000 },
            { "gta5rp_veh_m5e60", 15500000 },
            { "mark100", 9000000 },
            { "gta5rp_veh_nisr32", 7800000 },
            { "nsx95", 5000000 },
            { "s15", 5000000 },
            { "w140", 5000000 },
            { "bmwg05", 2500000 }, // luxe
            { "bmwg07", 3900000 },
            { "cayen19", 2000000 },
            { "escalade19", 3000000 },
            { "gta5rp_veh_fx50s", 1500000 },
            { "g63", 3500000 },
            { "gl63", 2000000 },
            { "gta5rp_veh_gle1", 3000000 },
            { "jeep15", 2500000 },
            { "lc200", 2000000 },
            { "merc6x6", 4000000 },
            { "navigator19", 2000000 },
            { "rrover17", 3000000 },
            { "suburban800", 1500000 },
            { "svr", 3000000 },
            { "gta5rp_veh_tahoe1", 2500000 },
            { "urus", 5000000 },
            { "gta5rp_veh_wald2018", 4500000 },
            { "gta5rp_veh_x5e53", 3500000 },
            { "x6m18", 4000000 },
            { "gta5rp_veh_r50", 1850000 },
            { "cullinan", 65000000 },
            {"msprinter", 56000000 },
            {"starone", 56000000 },
            {"eqg", 56000000 },
            {"taycan21", 56000000 }, //luxe
            {"x6m2", 56000000 },
            {"panamera17turbo", 56000000 },
            {"rs72", 56000000 },
            {"ghost", 56000000 },
                        {  "chiron", 75000000 },
            { "gemera", 45000000},
                        { "maseratigt", 20000000 },
             {   "f150", 6000000 },
              {  "vulcan", 35500000 },
               { "phuayra", 18000000 } ,
                {"senna", 14000000 },
                {"v250", 12000000} ,
                {"veyron", 60000000},
                
                {"i8", 12000000},
                {"exp100", 70000000},
                {"minifiat", 6000000},
                {"minicar", 1000000},
                {"agerars", 50000000},
                
                {"jesko", 60000000},
                {"rcf", 35000000},
                {"lfa", 20000000},
                {"slr", 27000000},
                {"cyber", 35000000},
                {"scooby", 7000000},
                {"240z", 8000000},
                {"350z", 9000000},
                
                {"acursx", 7000000},
                
                {"carreragt", 10000000},
                {"civic19", 4000000},
                {"db11", 18000000},
                {"deborah", 25000000},
                {"elemento", 29000000},
                {"fordraptor", 2000000},
                {"ftype15", 5000000},
                {"gt17", 23000000},
                {"gtr17", 4000000},
                {"gtr2000", 900000},
                {"impala67", 1000000},
                {"lambsc18", 15000000},
                
                {"mc720s", 20000000},
                {"mclaren20", 19000000},
                
                {"nsx18", 900000},
                
                {"regera", 30000000},
                {"s13", 90000},
                {"rx7", 400000},
                {"sian", 28000000},
                {"sl300", 1500000},
                {"smalljoe", 6000000},
                {"veneno", 28000000},
                {"vision6", 17000000},
                {"zl1", 4000000},
                {"lykan", 25000000},
                {"avtr", 30000000},
                {"lada2170", 400000},
                {"m4comp", 8000000},
                {"mlbrabus", 9000000 },
                {"rs5", 4000000 },
                {"taycan", 30000000 },
                {"ocnauda8l22h", 30000000 },

            { "180sx", 20000000 },
            { "r1m", 1500000 },
            { "rcv213", 1700000 },
            { "r111", 2000000 },
            { "r62008", 2300000 },
            { "r62010", 2500000 },
            { "harley1", 3000000 },
            { "fireblade", 3500000 },
            { "gsx18", 4000000 },
            { "h2r", 4500000 },
            { "panigale", 3000000 },
            { "zx10rr", 6000000 },
            { "divo", 10 },
            { "caliburn", 10 },
            { "ocnrrvn100", 10 },
            { "taycants21m", 10 },
            { "rx7veilside", 10 },
            { "wycalt", 10 },
            { "monza", 10 },
            { "rsq8m", 10 },
            { "buzzard2", 10 },
            { "school", 10 },
            { "deluxo", 10 },
            { "lada2107", 10 },
            { "mvisiongt", 10 },
            { "rt70", 10 },
            { "bolide", 10 },
            { "ffrs", 10 },
            { "mercedesgls", 10 },
            {"Pistol",10000},
            {"CombatPistol",15000},
            {"Revolver",50000},
            {"HeavyPistol",12000},
            
            {"BullpupShotgun",14000},
            
            {"CombatPDW",60000},
            {"MachinePistol",30000},
            {"Патроны",7},

            #region FishShop
            { "Удочка", 2000 },
            { "Улучшенная удочка", 5000 },
            { "Удочка MK2", 9000 },
            { "Наживка", 45 },
            #endregion FishShop
            #region SellShop
            {"Корюшка",13},
            {"Кунджа",16},
            {"Лосось",10},
            {"Окунь",4},
            {"Осётр",5},
            {"Скат",12},
            {"Тунец",18},
            {"Угорь",5},
            {"Чёрный амур",15},
            {"Щука",6},
            #endregion SellShop
        };

        public static List<Product> fillProductList(int type)
        {
            List<Product> products_list = new List<Product>();
            switch (type)
            {
                case 0:
                    foreach (var name in MarketProducts)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 1, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 1:
                    products_list.Add(new Product(ProductsOrderPrice["Бензин"], 0, 0, "Бензин", false));
                    break;
                case 2:
                    foreach (var name in CarsNames[0])
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 0, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 3:
                    foreach (var name in CarsNames[1])
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 0, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 4:
                    foreach (var name in GunNames)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 5, name, false);
                        products_list.Add(product);
                    }
                    products_list.Add(new Product(ProductsOrderPrice["Патроны"], 0, 5, "Патроны", false));
                    break;
                case 5:
                    products_list.Add(new Product(100, 200, 10, "Одежда", false));
                    break;
                case 6:
                    products_list.Add(new Product(100, 100, 0, "Расходники", false));
                    products_list.Add(new Product(100, 0, 0, "Татуировки", false));
                    break;
                case 7:
                    products_list.Add(new Product(100, 100, 0, "Расходники", false));
                    products_list.Add(new Product(100, 0, 0, "Парики", false));
                    break;
                case 8:
                    products_list.Add(new Product(100, 50, 0, "Маски", false));
                    break;
                case 9:
                    products_list.Add(new Product(100, 1000, 0, "Запчасти", false));
                    break;
                case 10:
                    products_list.Add(new Product(200, 200, 0, "Средство для мытья", false));
                    break;
                case 11:
                    products_list.Add(new Product(500000, 20, 0, "Корм для животных", false));
                    break;
                case 12:
                    foreach (var name in FishProducts)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 1, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 13:
                    foreach (var name in SellProducts)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 1, name, false);
                        products_list.Add(product);
                    }
                    break;
            }
            return products_list;
        }

        public static int GetBuyingItemType(string name)
        {
            var type = -1;
            switch (name)
            {
                case "Монтировка":
                    type = (int)ItemType.Crowbar;
                    break;
                case "Фонарик":
                    type = (int)ItemType.Flashlight;
                    break;
                case "Молоток":
                    type = (int)ItemType.Hammer;
                    break;
                case "Гаечный ключ":
                    type = (int)ItemType.Wrench;
                    break;
                case "Канистра бензина":
                    type = (int)ItemType.GasCan;
                    break;
                case "Чипсы":
                    type = (int)ItemType.Сrisps;
                    break;
                case "Вода":
                    type = (int)ItemType.Sprunk;
                    break;
                case "Пицца":
                    type = (int)ItemType.Pizza;
                    break;
                case "Бургер":
                    type = (int)ItemType.Burger;
                    break;
                case "Хот-Дог":
                    type = (int)ItemType.HotDog;
                    break;
                case "Сэндвич":
                    type = (int)ItemType.Sandwich;
                    break;
                case "eCola":
                    type = (int)ItemType.eCola;
                    break;
                case "Sprunk":
                    type = (int)ItemType.Sprunk;
                    break;
                case "Связка ключей":
                    type = (int)ItemType.KeyRing;
                    break;
                case "Рем. Комплект":
                    type = (int)ItemType.RepairKit;
                    break;
                case "Аптечка":
                    type = (int)ItemType.Apteka;
                    break;
                case "Бонг":
                    type = (int)ItemType.Lighter;
                    break;
                case "Зажигалка":
                    type = (int)ItemType.Bong;
                    break;
                case "Удочка":
                    type = (int)ItemType.Rod;
                    break;
                case "Улучшенная удочка":
                    type = (int)ItemType.RodUpgrade;
                    break;
                case "Удочка MK2":
                    type = (int)ItemType.RodMK2;
                    break;
                case "Наживка":
                    type = (int)ItemType.Naz;
                    break;
            }

            return type;
        }

        public static void interactionPressed(Player player)
        {
            if (player.GetData<int>("BIZ_ID") == -1) return;
            if (player.HasData("FOLLOWING"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                return;
            }
            Business biz = BizList[player.GetData<int>("BIZ_ID")];

            if (biz.Owner != "Государство" && !Main.PlayerNames.ContainsValue(biz.Owner))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Данный {BusinessTypeNames[biz.Type]} в данный момент не работает", 3000);
                return;
            }

            switch (biz.Type)
            {
                case 0:
                case 12:
                    OpenBizShopMenu(player);
                    return;
                case 1:
                    if (!player.IsInVehicle) return;
                    Vehicle vehicle = player.Vehicle;
                    if (vehicle == null) return; //check
                    if (player.VehicleSeat != 0) return;
                    OpenPetrolMenu(player);
                    return;
                case 2:
                case 3:
                    if (player.HasData("FOLLOWER"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Отпустите человека", 3000);
                        return;
                    }
                    foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                    {
                        if (item.FastSlots > 0)
                        {
                            if (FastSlots.Carabine.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Shot.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.SMG.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Pistol.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                        }
                    }
                    player.SetData("CARROOMID", biz.ID);
                    CarRoom.enterCarroom(player, biz.Products[0].Name);

                    return;
                case 4:
                    player.SetData("GUNSHOP", biz.ID);
                    OpenGunShopMenu(player);
                    return;
                case 5:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                    {
                        if (item.FastSlots > 0)
                        {
                            if (FastSlots.Carabine.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Shot.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.SMG.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Pistol.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                        }
                    }
                    player.SetData("CLOTHES_SHOP", biz.ID);
                    Trigger.ClientEvent(player, "openClothes", biz.Products[0].Price);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    NAPI.Entity.SetEntityDimension(player, Dimensions.RequestPrivateDimension(player));

                    return;
                case 6:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                    {
                        if (item.FastSlots > 0)
                        {
                            if (FastSlots.Carabine.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Shot.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.SMG.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Pistol.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                        }
                    }

                    player.SetData("BODY_SHOP", biz.ID);
                    Main.Players[player].ExteriorPos = player.Position;
                    var dim = Dimensions.RequestPrivateDimension(player);
                    NAPI.Entity.SetEntityDimension(player, dim);
                    NAPI.Entity.SetEntityPosition(player, new Vector3(324.9798, 180.6418, 103.6665));
                    player.Rotation = new Vector3(0, 0, 101.0228);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    Customization.ClearClothes(player, Main.Players[player].Gender);

                    Trigger.ClientEvent(player, "openBody", false, biz.Products[1].Price);
                    return;
                case 7:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                    {
                        if (item.FastSlots > 0)
                        {
                            if (FastSlots.Carabine.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Shot.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.SMG.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Pistol.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                        }
                    }

                    player.SetData("BODY_SHOP", biz.ID);
                    dim = Dimensions.RequestPrivateDimension(player);
                    NAPI.Entity.SetEntityDimension(player, dim);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    Customization.ClearClothes(player, Main.Players[player].Gender);
                    Trigger.ClientEvent(player, "openBody", true, biz.Products[1].Price);
                    return;
                case 8:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                    {
                        if (item.FastSlots > 0)
                        {
                            if (FastSlots.Carabine.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Shot.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.SMG.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Pistol.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                        }
                    }
                    player.SetData("MASKS_SHOP", biz.ID);
                    Trigger.ClientEvent(player, "openMasks", biz.Products[0].Price);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    Customization.ApplyMaskFace(player);
                    return;
                case 9:
                    var veh = player.Vehicle;
                    if (!veh.HasData("ACCESS") && (veh.GetData<string>("ACCESS") != "PERSONAL" || veh.GetData<string>("ACCESS") != "GARAGE"))
                    {
                        var access = VehicleManager.canAccessByNumber(player, veh.NumberPlate);
                        if (!access)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы можете тюнинговать только личные авто", 3000);
                            return;
                        }
                    }
                    if (player.Vehicle.Class == 13)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Велосипед не может быть затюнингован", 3000);
                        return;
                    }
                    foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                    {
                        if (item.FastSlots > 0)
                        {
                            if (FastSlots.Carabine.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Shot.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.SMG.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                            if (FastSlots.Pistol.Contains(item.Type))
                            {
                                BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                            }
                        }
                    }
                    var occupants = VehicleManager.GetVehicleOccupants(player.Vehicle);
                    foreach (var p in occupants)
                    {
                        if (p != player)
                            VehicleManager.WarpPlayerOutOfVehicle(p);
                    }

                    Trigger.ClientEvent(player, "tuningSeatsCheck");
                    return;
                case 10:
                    if (!player.IsInVehicle)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                        return;
                    }
                    Trigger.ClientEvent(player, "openDialog", "CARWASH_PAY", $"Вы хотите помыть машину за ${biz.Products[0].Price}$?");
                    return;
                case 11:
                    if (player.HasData("FOLLOWER"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Отпустите человека", 3000);
                        return;
                    }
                    player.SetData("PETSHOPID", biz.ID);
                    enterPetShop(player, biz.Products[0].Name);
                    return;
            }
        }

        public static void enterPetShop(Player player, string prodname)
        {
            Main.Players[player].ExteriorPos = player.Position;
            uint mydim = (uint)(player.Value + 500);
            NAPI.Entity.SetEntityDimension(player, mydim);
            NAPI.Entity.SetEntityPosition(player, new Vector3(-758.3929, 319.5044, 175.302));
            player.PlayAnimation("amb@world_human_sunbathe@male@back@base", "base", 39);
            //player.FreezePosition = true;
            player.SetData("INTERACTIONCHECK", 0);
            var prices = new List<int>();
            Business biz = BusinessManager.BizList[player.GetData<int>("PETSHOPID")];
            for (byte i = 0; i != 9; i++)
            {
                prices.Add(biz.Products[0].Price);
            }
            Trigger.ClientEvent(player, "openPetshop", JsonConvert.SerializeObject(PetNames), JsonConvert.SerializeObject(PetHashes), JsonConvert.SerializeObject(prices), mydim);
        }
        [RemoteEvent("fishshop")]
        public static void Event_FishShopCallback(Player client, int index)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (client.GetData<int>("BIZ_ID") == -1) return;
                Business biz = BizList[client.GetData<int>("BIZ_ID")];

                var prod = biz.Products[index];

                var type = GetBuyingItemType(prod.Name);

                nItem item = new nItem((ItemType)type);

                var aItem = nInventory.Find(Main.Players[client].UUID, RodManager.GetSellingItemType(prod.Name));
                if (aItem == null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет {prod.Name}", 3000);
                    return;
                }

                var prices = prod.Price * Main.pluscost;

                nInventory.Remove(client, RodManager.GetSellingItemType(prod.Name), 1);
                Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {prod.Name}", 3000);
                MoneySystem.Wallet.Change(client, +prices);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", prices, $"sellShop");
            }
            catch (Exception e) { Log.Write($"SellShop: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }
        [RemoteEvent("petshopBuy")]
        public static void RemoteEvent_petshopBuy(Player player, string petName)
        {
            try
            {
                player.StopAnimation();
                Business biz = BusinessManager.BizList[player.GetData<int>("PETSHOPID")];
                NAPI.Entity.SetEntityPosition(player, new Vector3(biz.EnterPoint.X, biz.EnterPoint.Y, biz.EnterPoint.Z + 1.5));
                //player.FreezePosition = false;
                NAPI.Entity.SetEntityDimension(player, 0);
                Main.Players[player].ExteriorPos = new Vector3();
                Trigger.ClientEvent(player, "destroyCamera");
                Dimensions.DismissPrivateDimension(player);

                Houses.House house = Houses.HouseManager.GetHouse(player, true);
                if (house == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
                    return;
                }
                if (Houses.HouseManager.HouseTypeList[house.Type].PetPosition == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ваше место проживания не подходит для жизни петомцев", 3000);
                    return;
                }
                if (Main.Players[player].Money < biz.Products[0].Price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                if (!BusinessManager.takeProd(biz.ID, 1, "Корм для животных", biz.Products[0].Price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "К сожалению, петомцев данного рода пока что нет в магазине", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -biz.Products[0].Price);
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", biz.Products[0].Price, $"buyPet({petName})");
                house.PetName = petName;
                Main.Players[player].PetName = petName;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Теперь Вы являетесь счастливым хозяином {petName}!", 3000);
            }
            catch (Exception e) { Log.Write("PetshopBuy: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("petshopCancel")]
        public static void RemoteEvent_petshopCancel(Player player)
        {
            try
            {
                if (!player.HasData("PETSHOPID")) return;
                player.StopAnimation();
                var enterPoint = BusinessManager.BizList[player.GetData<int>("PETSHOPID")].EnterPoint;
                NAPI.Entity.SetEntityDimension(player, 0);
                NAPI.Entity.SetEntityPosition(player, new Vector3(enterPoint.X, enterPoint.Y, enterPoint.Z + 1.5));
                Main.Players[player].ExteriorPos = new Vector3();
                //player.FreezePosition = false;
                Dimensions.DismissPrivateDimension(player);
                player.ResetData("PETSHOPID");
                Trigger.ClientEvent(player, "destroyCamera");
            }
            catch (Exception e) { Log.Write("petshopCancel: " + e.Message, nLog.Type.Error); }
        }

        public static void Carwash_Pay(Player player)
        {
            try
            {
                if (player.GetData<int>("BIZ_ID") == -1) return;
                Business biz = BizList[player.GetData<int>("BIZ_ID")];

                if (player.IsInVehicle)
                {
                    if (player.VehicleSeat == 0)
                    {
                        if (VehicleStreaming.GetVehicleDirt(player.Vehicle) >= 0f)
                        {
                            if (Main.Players[player].Money < biz.Products[0].Price)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                                return;
                            }

                            if (!takeProd(biz.ID, 1, "Средство для мытья", biz.Products[0].Price))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно товара на складе", 3000);
                                return;
                            }
                            GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", biz.Products[0].Price, "carwash");
                            MoneySystem.Wallet.Change(player, -biz.Products[0].Price);

                            VehicleStreaming.SetVehicleDirt(player.Vehicle, 0.0f);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Ваш транспорт был помыт.", 3000);
                        }
                        else
                            Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Ваш транспорт не грязный.", 3000);
                    }
                    else
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Мыть транспорт может только водитель.", 3000);
                }
                return;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return;
            }
        }

            [RemoteEvent("tuningSeatsCheck")]
        public static void RemoteEvent_tuningSeatsCheck(Player player)
        {
            try
            {
                if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в личной машине", 3000);
                    return;
                }
                if (player.Vehicle.Class == 13)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Велосипед не может быть затюнингован", 3000);
                    return;
                }

                if (player.GetData<int>("BIZ_ID") == -1) return;
                if (player.HasData("FOLLOWING"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                    return;
                }
                Business biz = BizList[player.GetData<int>("BIZ_ID")];

                Main.Players[player].TuningShop = biz.ID;

                var veh = player.Vehicle;
                var dim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(veh, dim);
                NAPI.Entity.SetEntityDimension(player, dim);

                player.SetIntoVehicle(veh, 0);

                var positionveh = new Vector3(-337.7784, -136.5316, 38.6032);
                var positionvehrot = new Vector3(0.04308624, 0.07037075, 149);
                var positioncamera = new Vector3(-333.7966, -137.409, 40.58963);
                var typebiz = 1;

                if (player.Position.DistanceTo(new Vector3(-361.602478, -132.873581, 37.560154)) < 10)
                {
                    positionveh = new Vector3(-337.7784, -136.5316, 38.6032);
                    positionvehrot = new Vector3(0.04308624, 0.07037075, 149);
                    positioncamera = new Vector3(-333.7966, -137.409, 40.58963);
                    typebiz = 1;
                }
                if (player.Position.DistanceTo(new Vector3(-205.807541, -1306.96777, 30.16265)) < 10)
                {
                    positionveh = new Vector3(-210.76334, -1323.8368, 30.453913);
                    positionvehrot = new Vector3(0.3804222, -0.05517577, -38.74467);
                    positioncamera = new Vector3(-206.91806, -1324.184, 32.7704);
                    typebiz = 2;
                }
                NAPI.Entity.SetEntityPosition(veh, positionveh);
                NAPI.Entity.SetEntityRotation(veh, positionvehrot);

                var modelPrice = ProductsOrderPrice[VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model];
                var modelPriceMod = (modelPrice < 20000) ? 1 : 2;

                if (typebiz == 1)
                    Trigger.ClientEvent(player, "openTun", biz.Products[0].Price, VehicleHandlers.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model), modelPriceMod, JsonConvert.SerializeObject(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Components));
                if (typebiz == 2)
                    Trigger.ClientEvent(player, "openTun2", biz.Products[0].Price, VehicleHandlers.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model), modelPriceMod, JsonConvert.SerializeObject(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Components));
            }
            catch (Exception e) { Log.Write("tuningSeatsCheck: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("exitTuning")]
        public static void RemoteEvent_exitTuning(Player player)
        {
            try
            {
                int bizID = Main.Players[player].TuningShop;

                var veh = player.Vehicle;
                NAPI.Entity.SetEntityDimension(veh, 0);
                NAPI.Entity.SetEntityDimension(player, 0);

                player.SetIntoVehicle(veh, 0);

                NAPI.Entity.SetEntityPosition(veh, BizList[bizID].EnterPoint + new Vector3(0, 0, 1.0));
                VehicleManager.ApplyCustomization(veh);
                Dimensions.DismissPrivateDimension(player);
                Main.Players[player].TuningShop = -1;
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
            catch (Exception e) { Log.Write("ExitTuning: " + e.Message, nLog.Type.Error); }
        }

        static Dictionary<int, int> ArmorPrice = new Dictionary<int, int>{
            {-1, 120000},
            {0, 220000},
            {1, 320000},
            {2, 420000},
            {3, 520000},
            {4, 620000},
        };

        [RemoteEvent("buyTuning")]
        public static void RemoteEvent_buyTuning(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                int bizID = Main.Players[player].TuningShop;
                Business biz = BizList[bizID];

                var cat = Convert.ToInt32(arguments[0].ToString());
                var id = Convert.ToInt32(arguments[1].ToString());

                var wheelsType = -1;
                var r = 0;
                var g = 0;
                var b = 0;

                if (cat == 19)
                    wheelsType = Convert.ToInt32(arguments[2].ToString());
                else if (cat == 20)
                {
                    r = Convert.ToInt32(arguments[2].ToString());
                    g = Convert.ToInt32(arguments[3].ToString());
                    b = Convert.ToInt32(arguments[4].ToString());
                }

                var vehModel = VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model;

                var modelPrice = ProductsOrderPrice[vehModel];
                var modelPriceMod = (modelPrice < 20000) ? 1 : 2;

                var price = 0;
                if (cat <= 18)
                    price = Convert.ToInt32(TuningPrices[cat][id.ToString()] * modelPriceMod * biz.Products[0].Price / 100.0);
                else if (cat == 19)
                    price = Convert.ToInt32(TuningWheels[wheelsType][id] * biz.Products[0].Price / 100.0);
                else if (cat == 21)
                    price = 1000;
                else if (cat == 26)
                    price = ArmorPrice[id];
                else
                    price = Convert.ToInt32(5000 * biz.Products[0].Price / 100.0);

                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вам не хватает ещё {price - Main.Players[player].Money}$ для покупки этой модификации", 3000);
                    Trigger.ClientEvent(player, "tunBuySuccess", -2);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 2000);
                if (amount <= 0) amount = 1;
                if (!takeProd(biz.ID, amount, "Запчасти", price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В данной автомастерской закончились все запчасти", 3000);
                    Trigger.ClientEvent(player, "tunBuySuccess", -2);
                    return;
                }

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", price, $"buyTuning({player.Vehicle.NumberPlate},{cat},{id})");
                MoneySystem.Wallet.Change(player, -price);
                Trigger.ClientEvent(player, "tunBuySuccess", id);

                var number = player.Vehicle.NumberPlate;

                switch (cat)
                {
                    case 0:
                        VehicleManager.Vehicles[number].Components.Muffler = id;
                        break;
                    case 1:
                        VehicleManager.Vehicles[number].Components.SideSkirt = id;
                        break;
                    case 2:
                        VehicleManager.Vehicles[number].Components.Hood = id;
                        break;
                    case 3:
                        VehicleManager.Vehicles[number].Components.Spoiler = id;
                        break;
                    case 4:
                        VehicleManager.Vehicles[number].Components.Lattice = id;
                        break;
                    case 5:
                        VehicleManager.Vehicles[number].Components.Wings = id;
                        break;
                    case 6:
                        VehicleManager.Vehicles[number].Components.Roof = id;
                        break;
                    case 7:
                        VehicleManager.Vehicles[number].Components.Vinyls = id;
                        break;
                    case 8:
                        VehicleManager.Vehicles[number].Components.FrontBumper = id;
                        break;
                    case 9:
                        VehicleManager.Vehicles[number].Components.RearBumper = id;
                        break;
                    case 10:
                        VehicleManager.Vehicles[number].Components.Engine = id;
                        break;
                    case 11:
                        VehicleManager.Vehicles[number].Components.Turbo = id;
                        break;
                    case 12:
                        VehicleManager.Vehicles[number].Components.Horn = id;
                        break;
                    case 13:
                        VehicleManager.Vehicles[number].Components.Transmission = id;
                        break;
                    case 14:
                        VehicleManager.Vehicles[number].Components.WindowTint = id;
                        break;
                    case 15:
                        VehicleManager.Vehicles[number].Components.Suspension = id;
                        break;
                    case 16:
                        VehicleManager.Vehicles[number].Components.Brakes = id;
                        break;
                    case 17:
                        VehicleManager.Vehicles[number].Components.Headlights = id;
                        player.Vehicle.SetSharedData("hlcolor", id);
                        Trigger.ClientEvent(player, "VehStream_SetVehicleHeadLightColor", player.Vehicle.Handle, id);
                        break;
                    case 18:
                        VehicleManager.Vehicles[number].Components.NumberPlate = id;
                        break;
                    case 19:
                        VehicleManager.Vehicles[number].Components.Wheels = id;
                        VehicleManager.Vehicles[number].Components.WheelsType = wheelsType;
                        break;
                    case 20:
                        if (id == 0)
                            VehicleManager.Vehicles[number].Components.PrimColor = new Color(r, g, b);
                        else if (id == 1)
                            VehicleManager.Vehicles[number].Components.SecColor = new Color(r, g, b);
                        else if (id == 2)
                            VehicleManager.Vehicles[number].Components.NeonColor = new Color(r, g, b);
                        else if (id == 3)
                            VehicleManager.Vehicles[number].Components.NeonColor = new Color(0, 0, 0, 0);
                        break;
                    case 21:
                        VehicleManager.Vehicles[number].Components.PrimModColor = id;
                        VehicleManager.Vehicles[number].Components.SecModColor = id;
                        break;
                    case 26:
                        VehicleManager.Vehicles[number].Components.Armor = id;
                        break;
                }
                VehicleManager.Save(number);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили и установили данную модификацию", 3000);
                var veh = player.Vehicle;
                VehicleManager.ApplyCustomization(veh);
                Trigger.ClientEvent(player, "tuningUpd", JsonConvert.SerializeObject(VehicleManager.Vehicles[number].Components));
            }
            catch (Exception e) { Log.Write("buyTuning: " + e.Message, nLog.Type.Error); }
        }

        public static bool takeProd(int bizid, int amount, string prodname, int addMoney)
        {
            try
            {
                Business biz = BizList[bizid];
                foreach (var p in biz.Products)
                {
                    if (p.Name != prodname) continue;
                    if (p.Lefts - amount < 0)
                        return false;

                    p.Lefts -= amount;

                    if (biz.Owner == "Государство") break;
                    Bank.Data bData = Bank.Get(Main.PlayerBankAccs[biz.Owner]);
                    if (bData.ID == 0)
                    {
                        Log.Write($"TakeProd BankID error: {bizid.ToString()}({biz.Owner}) {amount.ToString()} {prodname} {addMoney.ToString()}", nLog.Type.Error);
                        return false;
                    }
                    if (!Bank.Change(bData.ID, addMoney, false))
                    {
                        Log.Write($"TakeProd error: {bizid.ToString()}({biz.Owner}) {amount.ToString()} {prodname} {addMoney.ToString()}", nLog.Type.Error);
                        return false;
                    }
                    GameLog.Money($"biz({bizid})", $"bank({bData.ID})", addMoney, "bizProfit");
                    Log.Write($"{biz.Owner}'s business get {addMoney}$ for '{prodname}'", nLog.Type.Success);
                    break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int getPriceOfProd(int bizid, string prodname)
        {
            Business biz = BizList[bizid];
            var price = 0;
            foreach (var p in biz.Products)
            {
                if (p.Name == prodname)
                {
                    price = p.Price;
                    break;
                }
            }
            return price;
        }

        public static Vector3 getNearestBiz(Player player, int type)
        {
            Vector3 nearestBiz = new Vector3();
            foreach (var b in BizList)
            {
                Business biz = BizList[b.Key];
                if (biz.Type != type) continue;
                if (nearestBiz == null) nearestBiz = biz.EnterPoint;
                if (player.Position.DistanceTo(biz.EnterPoint) < player.Position.DistanceTo(nearestBiz))
                    nearestBiz = biz.EnterPoint;
            }
            return nearestBiz;
        }

        private static List<int> clothesOutgo = new List<int>()
        {
            1, // Головные уборы
            4, // Верхняя одежда
            3, // Нижняя одежда
            2, // Треники abibas
            1, // Кеды нike
			362,
			363,
			364,
			365,
			366,
			367,
			368,
			369,
			370,
			371,
			372,
			373,
			374,
			396,
			397,
			398,
			399,
			400,
			401,
			402,
			403,
			80,
			21,
			111,
			142,
			72,
			70,
			321,
			381,
			382,
			383,
			384,
			385,
			386,
			387,
			388,
			389,
			390,
			391,
			392,
			393,
			394,
			395,
			396,
			397,
			398,
			133,
			134,
			135,
			136,
			137,
			138,
			146,
			147,
			83,
			26,
			140,
			141,
			142,
			143,
			144,
			145,
			148,
			149,
			150,
			102,
			103,
			104,
			105,
			5,
			6,
			7,
			8,
			9,
            10,
            15,
            22,
            24,
            28,
            31,
            32,
            42,
            46,
            76,
            77,
            0,
            8,
            9,
            11,
            13,
            14,
            15,
            18,
            19,
            20,
            23,
            32,
            33,
            43,
            44,
            47,
            49,
            80,
            81,
            89,
            90,
            102,
            103,
            104,
            105,
            106,
            107,
            108,
            109,
            110,
        };

        [RemoteEvent("cancelMasks")]
        public static void RemoteEvent_cancelMasks(Player player)
        {
            try
            {
                player.StopAnimation();
                Customization.ApplyCharacter(player);
                Customization.SetMask(player, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Texture);
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
            catch (Exception e) { Log.Write("cancelMasks: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("buyMasks")]
        public static void RemoteEvent_buyMasks(Player player, int variation, int texture)
        {
            try
            {
                Business biz = BizList[player.GetData<int>("MASKS_SHOP")];
                var prod = biz.Products[0];

                var tempPrice = Customization.Masks.FirstOrDefault(f => f.Variation == variation).Price;

                var price = Convert.ToInt32((tempPrice / 100.0) * prod.Price);

                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Top));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }

                if (!takeProd(biz.ID, 1, "Маски", price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно товара на складе", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", price, "buyMask");
                MoneySystem.Wallet.Change(player, -price);

                Customization.AddClothes(player, ItemType.Mask, variation, texture);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили новую маску. Она была добавлена в Ваш инвентарь.", 3000);
                return;
            }
            catch (Exception e) { Log.Write("buyMasks: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("cancelClothes")]
        public static void RemoteEvent_cancelClothes(Player player)
        {
            try
            {
                player.StopAnimation();
                Customization.ApplyCharacter(player);
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);
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
            catch (Exception e) { Log.Write("cancelClothes: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("buyClothes")]
        public static void RemoteEvent_buyClothes(Player player, int type, int variation, int texture)
        {
            try
            {
                Business biz = BizList[player.GetData<int>("CLOTHES_SHOP")];
                var prod = biz.Products[0];

                var tempPrice = 0;
                switch (type)
                {
                    case 0:
                        tempPrice = Customization.Hats[Main.Players[player].Gender].FirstOrDefault(h => h.Variation == variation).Price;
                        break;
                    case 1:
                        tempPrice = Customization.Tops[Main.Players[player].Gender].FirstOrDefault(t => t.Variation == variation).Price;
                        break;
                    case 2:
                        tempPrice = Customization.Underwears[Main.Players[player].Gender].FirstOrDefault(h => h.Value.Top == variation).Value.Price;
                        break;
                    case 3:
                        tempPrice = Customization.Legs[Main.Players[player].Gender].FirstOrDefault(l => l.Variation == variation).Price;
                        break;
                    case 4:
                        tempPrice = Customization.Feets[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 5:
                        tempPrice = Customization.Gloves[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 6:
                        tempPrice = Customization.Accessories[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 7:
                        tempPrice = Customization.Glasses[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 8:
                        tempPrice = Customization.Jewerly[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 9:
                        tempPrice = Customization.Bag[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                }
                var price = Convert.ToInt32((tempPrice / 100.0) * prod.Price);

                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Top));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 50);
                if (amount <= 0) amount = 1;
                if (!takeProd(biz.ID, amount, "Одежда", price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно товара на складе", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", price, "buyClothes");
                MoneySystem.Wallet.Change(player, -price);

                switch (type)
                {
                    case 0:
                        Customization.AddClothes(player, ItemType.Hat, variation, texture);
                        break;
                    case 1:
                        Customization.AddClothes(player, ItemType.Top, variation, texture);
                        break;
                    case 2:
                        var id = Customization.Underwears[Main.Players[player].Gender].FirstOrDefault(u => u.Value.Top == variation);
                        Customization.AddClothes(player, ItemType.Undershit, id.Key, texture);
                        break;
                    case 3:
                        Customization.AddClothes(player, ItemType.Leg, variation, texture);
                        break;
                    case 4:
                        Customization.AddClothes(player, ItemType.Feet, variation, texture);
                        break;
                    case 5:
                        Customization.AddClothes(player, ItemType.Gloves, variation, texture);
                        break;
                    case 6:
                        Customization.AddClothes(player, ItemType.Accessories, variation, texture);
                        break;
                    case 7:
                        Customization.AddClothes(player, ItemType.Glasses, variation, texture);
                        break;
                    case 8:
                        Customization.AddClothes(player, ItemType.Jewelry, variation, texture);
                        break;
                    case 9:
                        Customization.AddClothes(player, ItemType.Bag, variation, texture);
                        break;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили новую одежду. Она была добавлена в Ваш инвентарь.", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyClothes: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("cancelBody")]
        public static void RemoteEvent_cancelTattoo(Player player)
        {
            try
            {
                Business biz = BizList[player.GetData<int>("BODY_SHOP")];
                NAPI.Entity.SetEntityDimension(player, 0);
                NAPI.Entity.SetEntityPosition(player, biz.EnterPoint + new Vector3(0, 0, 1.12));
                Main.Players[player].ExteriorPos = new Vector3();
                Customization.ApplyCharacter(player);
            }
            catch (Exception e) { Log.Write("CancelBody: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("buyTattoo")]
        public static void RemoteEvent_buyTattoo(Player player, params object[] arguments)
        {
            try
            {
                var zone = Convert.ToInt32(arguments[0].ToString());
                var tattooID = Convert.ToInt32(arguments[1].ToString());
                var tattoo = BusinessTattoos[zone][tattooID];
                //player.SendChatMessage("zone " + zone + " tattooID " + tattooID + " tattoo" + tattoo);
                Log.Debug($"buyTattoo zone: {zone} | id: {tattooID}");

                Business biz = BizList[player.GetData<int>("BODY_SHOP")];

                var prod = biz.Products.FirstOrDefault(p => p.Name == "Татуировки");
                player.SendChatMessage(" prod" + prod);
                double price = tattoo.Price / 100.0 * prod.Price;
                if (Main.Players[player].Money < Convert.ToInt32(price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 100);
                if (amount <= 0) amount = 1;
                //На время фикса
                //if (!takeProd(biz.ID, amount, "Расходники", Convert.ToInt32(price)))
                //{
                //    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этот тату-салон не может оказать данную услугу", 3000);
                //    return;
                //}
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", Convert.ToInt32(price), "buyTattoo");
                MoneySystem.Wallet.Change(player, -Convert.ToInt32(price));

                var tattooHash = (Main.Players[player].Gender) ? tattoo.MaleHash : tattoo.FemaleHash;
                List<Tattoo> validTattoos = new List<Tattoo>();
                foreach (var t in Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos[zone])
                {
                    var isValid = true;
                    foreach (var slot in tattoo.Slots)
                    {
                        if (t.Slots.Contains(slot))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid) validTattoos.Add(t);
                }

                validTattoos.Add(new Tattoo(tattoo.Dictionary, tattooHash, tattoo.Slots));
                Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos[zone] = validTattoos;

                player.SetSharedData("TATTOOS", JsonConvert.SerializeObject(Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos));

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам набили татуировку {tattoo.Name} за {Convert.ToInt32(price)}$", 3000);
            }
            catch (Exception e) { Log.Write("BuyTattoo: " + e.Message, nLog.Type.Error); }
        }

        public static Dictionary<string, List<int>> BarberPrices = new Dictionary<string, List<int>>()
        {
            { "hair", new List<int>() {
                400,
                350,
                350,
                450,
                450,
                600,
                450,
                1100,
                450,
                600,
                600,
                400,
                350,
                2000,
                750,
                1500,
                450,
                600,
                600,
                400,
                350,
                2000,
                750,
                1500,
                2000,
                3000
              
            }},
            { "beard", new List<int>() {
                120,
                120,
                120,
                120,
                120,
                160,
                160,
                160,
                120,
                120,
                240,
                240,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                180,
                180,
            }},
            { "eyebrows", new List<int>() {
                100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100
            }},
            { "chesthair", new List<int>() {
                100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100
            }},
            { "lenses", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
                400,
                1000,
                1000,
            }},
            { "lipstick", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
                400,
                1000,
                300,
            }},
            { "blush", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
            }},
            { "makeup", new List<int>() {
                120,
                120,
                120,
                120,
                120,
                160,
                160,
                160,
                120,
                120,
                240,
                240,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                180,
                180,
            }},
        };

        [RemoteEvent("buyBarber")]
        public static void RemoteEvent_buyBarber(Player player, string id, int style, int color)
        {
            try
            {
                Log.Debug($"buyBarber: id - {id} | style - {style} | color - {color}");

                Business biz = BizList[player.GetData<int>("BODY_SHOP")];

             
                

                var prod = biz.Products.FirstOrDefault(p => p.Name == "Парики");
                double price;
                if (id == "hair")
                {
                    if (style >= 23) price = BarberPrices[id][23] / 100.0 * prod.Price;
                    else price = (style == 255) ? BarberPrices[id][0] / 100.0 * prod.Price : BarberPrices[id][style] / 100.0 * prod.Price;
                }
                else price = (style == 255) ? BarberPrices[id][0] / 100.0 * prod.Price : BarberPrices[id][style] / 100.0 * prod.Price;
                if (Main.Players[player].Money < Convert.ToInt32(price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                if (!takeProd(biz.ID, 1, "Расходники", Convert.ToInt32(price)))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этот барбер-шоп не может оказать эту услугу в данный момент", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", Convert.ToInt32(price), "buyBarber");
                MoneySystem.Wallet.Change(player, -Convert.ToInt32(price));

                switch (id)
                {
                    case "hair":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Hair = new HairData(style, color, color);
                        break;
                    case "beard":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[1].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].BeardColor = color;
                        break;
                    case "eyebrows":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[2].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].EyebrowColor = color;
                        break;
                    case "chesthair":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[10].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].ChestHairColor = color;
                        break;
                    case "lenses":
                        Customization.CustomPlayerData[Main.Players[player].UUID].EyeColor = style;
                        break;
                    case "lipstick":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[8].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].LipstickColor = color;
                        break;
                    case "blush":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[5].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].BlushColor = color;
                        break;
                    case "makeup":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[4].Value = style;
                        break;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы оплатили услугу Барбер-Шопа ({Convert.ToInt32(price)}$)", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyBarber: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("petrol")]
        public static void fillCar(Player player, int lvl)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                Vehicle vehicle = player.Vehicle;
                if (vehicle == null) return; //check
                if (player.VehicleSeat != 0) return;
                if (lvl <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                    return;
                }
                if (!vehicle.HasSharedData("PETROL"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно заправить эту машину", 3000);
                    return;
                }
                if (Core.VehicleStreaming.GetEngineState(vehicle))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Чтобы начать заправляться - заглушите транспорт.", 3000);
                    return;
                }
                int fuel = vehicle.GetSharedData<int>("PETROL");
                if (fuel >= VehicleManager.VehicleTank[vehicle.Class])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У транспорта полный бак", 3000);
                    return;
                }

                var isGov = false;
                if (lvl == 9999)
                    lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
                else if (lvl == 99999)
                {
                    isGov = true;
                    lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
                }

                if (lvl < 0) return;

                int tfuel = fuel + lvl;
                if (tfuel > VehicleManager.VehicleTank[vehicle.Class])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                    return;
                }
                Business biz = BizList[player.GetData<int>("BIZ_ID")];
                if (isGov)
                {
                    int frac = Main.Players[player].FractionID;
                    if (Fractions.Manager.FractionTypes[frac] != 2)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Чтобы заправить транспорт за гос. счет, Вы должны состоять в гос. организации", 3000);
                        return;
                    }
                    if (!vehicle.HasData("ACCESS") || vehicle.GetData<string>("ACCESS") != "FRACTION" || vehicle.GetData<int>("FRACTION") != frac)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете заправить за государственный счет не государственный транспорт", 3000);
                        return;
                    }
                    if (Fractions.Stocks.fracStocks[frac].FuelLeft < lvl * biz.Products[0].Price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Лимит на заправку гос. транспорта за день исчерпан", 3000);
                        return;
                    }
                }
                else
                {
                    if (Main.Players[player].Money < lvl * biz.Products[0].Price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {lvl * biz.Products[0].Price - Main.Players[player].Money}$)", 3000);
                        return;
                    }
                }
                if (!takeProd(biz.ID, lvl, "Бензин", lvl * biz.Products[0].Price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На заправке осталось {biz.Products[0].Lefts}л", 3000);
                    return;
                }
                if (isGov)
                {
                    GameLog.Money($"frac(6)", $"biz({biz.ID})", lvl * biz.Products[0].Price, "buyPetrol");
                    Fractions.Stocks.fracStocks[6].Money -= lvl * biz.Products[0].Price;
                    Fractions.Stocks.fracStocks[Main.Players[player].FractionID].FuelLeft -= lvl * biz.Products[0].Price;
                }
                else
                {
                    GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", lvl * biz.Products[0].Price, "buyPetrol");
                    MoneySystem.Wallet.Change(player, -lvl * biz.Products[0].Price);
                }

                vehicle.SetSharedData("PETROL", tfuel);

                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "PERSONAL")
                {
                    var number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);
                    VehicleManager.Vehicles[number].Fuel += lvl;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Транспорт заправлен", 3000);
                Commands.RPChat("me", player, $"заправил(а) транспортное средство");
            }
            catch (Exception e) { Log.Write("Petrol: " + e.Message, nLog.Type.Error); }
        }

        public static void bizNewPrice(Player player, int price, int BizID)
        {
            if (!Main.Players[player].BizIDs.Contains(BizID)) return;
            Business biz = BizList[BizID];
            var prodName = player.GetData<string>("SELECTPROD");

            double minPrice = (biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || prodName == "Татуировки" || prodName == "Парики"
                || prodName == "Патроны") ? 80 : (biz.Type == 1) ? 2 : ProductsOrderPrice[player.GetData<string>("SELECTPROD")] * 0.8;
            double maxPrice = (biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || prodName == "Татуировки" || prodName == "Парики"
                || prodName == "Патроны") ? 150 : (biz.Type == 1) ? 7 : ProductsOrderPrice[player.GetData<string>("SELECTPROD")] * 1.2;

            if (price < minPrice || price > maxPrice)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно установить такую цену", 3000);
                OpenBizProductsMenu(player);
                return;
            }
            foreach (var p in biz.Products)
            {
                if (p.Name == prodName)
                {
                    p.Price = price;
                    string ch = (biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || p.Name == "Татуировки" || p.Name == "Парики") ? "%" : "$";

                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Теперь {p.Name} стоит {p.Price}{ch}", 3000);
                    if (p.Name == "Бензин") biz.UpdateLabel();
                    OpenBizProductsMenu(player);
                    return;
                }
            }
        }

        public static void bizOrder(Player player, int amount, int BizID)
        {
            if (!Main.Players[player].BizIDs.Contains(BizID)) return;
            Business biz = BizList[BizID];

            if (amount < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неверное значение", 3000);
                OpenBizProductsMenu(player);
                return;
            }

            foreach (var p in biz.Products)
            {
                if (p.Name == player.GetData<string>("SELECTPROD"))
                {
                    if (p.Ordered)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже заказали этот товар", 3000);
                        OpenBizProductsMenu(player);
                        return;
                    }

                    if (biz.Type >= 2 && biz.Type <= 3)
                    {
                        if (amount > 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Укажите значение от 1 до 3", 3000);
                            OpenBizProductsMenu(player);
                            return;
                        }
                    }
                    else if (biz.Type == 14)
                    {
                        if (amount < 1 || p.Lefts + amount > ProductsCapacity[p.Name])
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Укажите значение от 1 до {ProductsCapacity[p.Name] - p.Lefts}", 3000);
                            OpenBizProductsMenu(player);
                            return;
                        }
                    }
                    else
                    {
                        if (amount < 10 || p.Lefts + amount > ProductsCapacity[p.Name])
                        {
                            var text = "";
                            if (ProductsCapacity[p.Name] - p.Lefts < 10) text = "У Вас достаточно товаров на складе";
                            else text = $"Укажите от 10 до {ProductsCapacity[p.Name] - p.Lefts}";

                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, text, 3000);
                            OpenBizProductsMenu(player);
                            return;
                        }
                    }

                    var price = (p.Name == "Патроны") ? 4 : ProductsOrderPrice[p.Name];
                    if (!Bank.Change(Main.Players[player].Bank, -amount * price))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств на счету", 3000);
                        return;
                    }
                    GameLog.Money($"bank({Main.Players[player].Bank})", $"server", amount * price, "bizOrder");
                    var order = new Order(p.Name, amount);
                    p.Ordered = true;

                    var random = new Random();
                    do
                    {
                        order.UID = random.Next(000000, 999999);
                    } while (BusinessManager.Orders.ContainsKey(order.UID));
                    BusinessManager.Orders.Add(order.UID, biz.ID);

                    biz.Orders.Add(order);
                    SetTimerToAddProducts(player, biz, order);

                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заказали {p.Name} в количестве {amount}. №{order.UID}", 3000);
                    player.SendChatMessage($"Номер Вашего заказа: {order.UID}");
                    return;
                }
            }
        }

        public static void SetTimerToAddProducts(Player player, Business biz, Order order)
        {
            if (!Main.Players[player].BizIDs.Contains(biz.ID)) return;
            Player pl = player;
            try
            {
                Timers.StartOnce($"{Main.Players[pl].UUID}_ordertime", 180000, () => { //3  minutes 180.000
                    if (!BusinessManager.Orders.ContainsKey(order.UID))
                    {
                        Timers.Stop($"{Main.Players[pl].UUID}_ordertime");
                        return;
                    }
                    Business biz = BusinessManager.BizList[BusinessManager.Orders[order.UID]];
                    if (order == null)
                    {
                        Timers.Stop($"{Main.Players[pl].UUID}_ordertime");
                        return;
                    }
                    var ow = NAPI.Player.GetPlayerFromName(biz.Owner);
                    if (ow != null)
                        Notify.Alert(ow, $"Ваш заказ на {order.Name} был выполнен", 3000);

                    foreach (var p in biz.Products)
                    {
                        if (p.Name != order.Name) continue;
                        p.Ordered = false;
                        p.Lefts += order.Amount;
                        break;
                    }
                    biz.Orders.Remove(order);
                    BusinessManager.Orders.Remove(order.UID);

                    Timers.Stop($"{Main.Players[pl].UUID}_ordertime");
                });
            }
            catch (Exception e)
            {
                Log.Write("ERROR (SetTimerToAddProducts): " + e.ToString(), nLog.Type.Error);
            }
        }

        public static void buyBusinessCommand(Player player)
        {
            if (!player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться около бизнеса", 3000);
                return;
            }


            int id = player.GetData<int>("BIZ_ID");
            Business biz = BusinessManager.BizList[id];
            if (Main.Players[player].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете приобрести больше {Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl]} бизнесов", 3000);
                return;
            }
            else
            {
                if (biz.Owner == "Государство")
                {
                    if (MoneySystem.Wallet.Change(player, -biz.SellPrice))
                    {
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", biz.SellPrice, $"buyBiz({biz.ID})");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем! Вы купили {BusinessManager.BusinessTypeNames[biz.Type]}, не забудьте внести налог за него в банкомате", 3000);
                        biz.Owner = player.Name.ToString();
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает средств", 3000);
                        return;
                    }
                }
                else if (biz.Owner == player.Name.ToString())
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Этот бизнес принадлежит Вам", 3000);
                    return;
                }
                else if (biz.Owner != player.Name.ToString())
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Этот бизнес принадлежит другому игроку", 3000);
                    return;
                }

                biz.UpdateLabel();
                foreach (var p in biz.Products)
                    p.Lefts = 100;
                var newOrders = new List<Order>();
                foreach (var o in biz.Orders)
                {
                    if (o.Taked) newOrders.Add(o);
                    else Orders.Remove(o.UID);
                }
                biz.Orders = newOrders;

                Main.Players[player].BizIDs.Add(id);
                var tax = Convert.ToInt32(biz.SellPrice / 10000);
                MoneySystem.Bank.Accounts[biz.BankID].Balance = tax * 2;

                var split = biz.Owner.Split('_');
                MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[player].BizIDs)}' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                MySQL.Query($"UPDATE businesses SET owner='{biz.Owner}' WHERE id='{biz.ID}'");
            }
        }

        public static void createBusinessCommand(Player player, int govPrice, int type)
        {
            if (!Group.CanUseCmd(player, "createbusiness")) return;
            var pos = player.Position;
            pos.Z -= 1.12F;
            string productlist = "";
            List<Product> products_list = BusinessManager.fillProductList(type);
            productlist = JsonConvert.SerializeObject(products_list);
            lastBizID++;

            var bankID = MoneySystem.Bank.Create("", 3, 1000);
            MySQL.Query($"INSERT INTO businesses (id, owner, sellprice, type, products, enterpoint, unloadpoint, money, mafia, orders) " +
                $"VALUES ({lastBizID}, 'Государство', {govPrice}, {type}, '{productlist}', '{JsonConvert.SerializeObject(pos)}', '{JsonConvert.SerializeObject(new Vector3())}', {bankID}, -1, '{JsonConvert.SerializeObject(new List<Order>())}')");

            Business biz = new Business(lastBizID, "Государство", govPrice, type, products_list, pos, new Vector3(), bankID, -1, new List<Order>());
            biz.UpdateLabel();
            BizList.Add(lastBizID, biz);

            if (type == 6)
            {
                MySQL.Query($"INSERT INTO `weapons`(`id`,`lastserial`) VALUES({biz.ID},0)");
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы создали бизнес {BusinessManager.BusinessTypeNames[type]}", 3000);
        }

        public static void createBusinessUnloadpoint(Player player, int bizid)
        {
            if (!Group.CanUseCmd(player, "createunloadpoint")) return;
            var pos = player.Position;
            BizList[bizid].UnloadPoint = pos;
            MySQL.Query($"UPDATE businesses SET unloadpoint='{JsonConvert.SerializeObject(pos)}' WHERE id={bizid}");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Успешно создана точка разгрузки для бизнеса ID: {bizid}", 3000);
        }

        public static void deleteBusinessCommand(Player player, int id)
        {
            if (!Group.CanUseCmd(player, "deletebusiness")) return;
            MySQL.Query($"DELETE FROM businesses WHERE id={id}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы удалили бизнес", 3000);
            Business biz = BusinessManager.BizList.FirstOrDefault(b => b.Value.ID == id).Value;

            if (biz.Type == 6)
            {
                MySQL.Query($"DELETE FROM `weapons` WHERE id={id}");
            }

            if (biz.Owner != "Государство" && biz.Owner != "")
            {
                var owner = NAPI.Player.GetPlayerFromName(biz.Owner);
                if (owner == null)
                {
                    var split = biz.Owner.Split('_');
                    var data = MySQL.QueryRead($"SELECT `biz` FROM `characters` WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    List<int> ownerBizs = new List<int>();
                    foreach (DataRow Row in data.Rows)
                        ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                    ownerBizs.Remove(biz.ID);

                    MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                }
                else
                {
                    Main.Players[owner].BizIDs.Remove(id);
                    MoneySystem.Wallet.Change(owner, biz.SellPrice);
                }
            }
            biz.Destroy();
            BizList.Remove(biz.ID);
        }

        public static void sellBusinessCommand(Player player, Player target, int price)
        {
            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }

            if (Main.Players[player].BizIDs.Count == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет бизнеса", 3000);
                return;
            }

            if (Main.Players[target].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[target].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок купил максимум бизнесов", 3000);
                return;
            }

            var biz = BizList[Main.Players[player].BizIDs[0]];
            if (price < biz.SellPrice / 2 || price > biz.SellPrice * 3)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно продать бизнес за такую цену. Укажите цену от {biz.SellPrice / 2}$ до {biz.SellPrice * 3}$", 3000);
                return;
            }

            if (Main.Players[target].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно денег", 3000);
                return;
            }

            Trigger.ClientEvent(target, "openDialog", "BUSINESS_BUY", $"{player.Name} предложил Вам купить {BusinessTypeNames[biz.Type]} за ${price}");
            target.SetData("SELLER", player);
            target.SetData("SELLPRICE", price);
            target.SetData("SELLBIZID", biz.ID);

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили игроку ({target.Value}) купить Ваш бизнес за {price}$", 3000);
        }

        public static void acceptBuyBusiness(Player player)
        {
            Player seller = player.GetData<Player>("SELLER");
            if (!Main.Players.ContainsKey(seller) || !Main.Players.ContainsKey(player)) return;

            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }

            var price = player.GetData<int>("SELLPRICE");
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег", 3000);
                return;
            }

            Business biz = BizList[player.GetData<int>("SELLBIZID")];
            if (!Main.Players[seller].BizIDs.Contains(biz.ID))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Бизнес больше не принадлежит игроку", 3000);
                return;
            }

            if (Main.Players[player].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во бизнесов", 3000);
                return;
            }

            Main.Players[player].BizIDs.Add(biz.ID);
            Main.Players[seller].BizIDs.Remove(biz.ID);

            biz.Owner = player.Name.ToString();
            var split1 = seller.Name.Split('_');
            var split2 = player.Name.Split('_');
            MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[seller].BizIDs)}' WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
            MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[player].BizIDs)}' WHERE firstname='{split2[0]}' AND lastname='{split2[1]}'");
            MySQL.Query($"UPDATE businesses SET owner='{biz.Owner}' WHERE id='{biz.ID}'");
            biz.UpdateLabel();

            MoneySystem.Wallet.Change(player, -price);
            MoneySystem.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"buyBiz({biz.ID})");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили у {seller.Name.Replace('_', ' ')} {BusinessTypeNames[biz.Type]} за {price}$", 3000);
            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name.Replace('_', ' ')} купил у Вас {BusinessTypeNames[biz.Type]} за {price}$", 3000);
        }

        #region Menus
        #region manage biz
        public static void OpenBizListMenu(Player player)
        {
            if (Main.Players[player].BizIDs.Count == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ни одного бизнеса", 3000);
                return;
            }

            Menu menu = new Menu("bizlist", false, false);
            menu.Callback = callback_bizlist;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Ваши бизнесы";
            menu.Add(menuItem);

            foreach (var id in Main.Players[player].BizIDs)
            {
                menuItem = new Menu.Item(id.ToString(), Menu.MenuItem.Button);
                menuItem.Text = BusinessManager.BusinessTypeNames[BusinessManager.BizList[id].Type];
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizlist(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "close":
                    MenuManager.Close(player);
                    return;
                default:
                    OpenBizManageMenu(player, Convert.ToInt32(item.ID));
                    player.SetData("SELECTEDBIZ", Convert.ToInt32(item.ID));
                    return;
            }
        }

        public static void OpenBizManageMenu(Player player, int id)
        {
            if (!Main.Players[player].BizIDs.Contains(id))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас больше нет этого бизнеса", 3000);
                return;
            }

            Menu menu = new Menu("bizmanage", false, false);
            menu.Callback = callback_bizmanage;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Управление бизнесом";
            menu.Add(menuItem);

            menuItem = new Menu.Item("products", Menu.MenuItem.Button);
            menuItem.Text = "Товары";
            menu.Add(menuItem);

            Business biz = BizList[id];
            menuItem = new Menu.Item("tax", Menu.MenuItem.Card);
            menuItem.Text = $"Налог: {Convert.ToInt32(biz.SellPrice / 100 * 0.013)}$/ч";
            menu.Add(menuItem);

            menuItem = new Menu.Item("money", Menu.MenuItem.Card);
            menuItem.Text = $"Счёт бизнеса: {MoneySystem.Bank.Accounts[biz.BankID].Balance}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = "Продать бизнес";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizmanage(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "products":
                    MenuManager.Close(client);
                    OpenBizProductsMenu(client);
                    return;
                case "sell":
                    MenuManager.Close(client);
                    OpenBizSellMenu(client);
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }

        public static void OpenBizSellMenu(Player player)
        {
            Menu menu = new Menu("bizsell", false, false);
            menu.Callback = callback_bizsell;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Продажа";
            menu.Add(menuItem);

            var bizID = player.GetData<int>("SELECTEDBIZ");
            Business biz = BizList[bizID];
            var price = biz.SellPrice / 100 * 70;
            menuItem = new Menu.Item("govsell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать государству (${price})";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizsell(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (!client.HasData("SELECTEDBIZ") || !Main.Players[client].BizIDs.Contains(client.GetData<int>("SELECTEDBIZ")))
            {
                MenuManager.Close(client);
                return;
            }

            var bizID = client.GetData<int>("SELECTEDBIZ");
            Business biz = BizList[bizID];
            switch (item.ID)
            {
                case "govsell":
                    var price = biz.SellPrice / 100 * 70;
                    MoneySystem.Wallet.Change(client, price);
                    GameLog.Money($"server", $"player({Main.Players[client].UUID})", price, $"sellBiz({biz.ID})");

                    Main.Players[client].BizIDs.Remove(bizID);
                    biz.Owner = "Государство";
                    biz.UpdateLabel();

                    Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали бизнес государству за {price}$", 3000);
                    MenuManager.Close(client);
                    return;
                case "back":
                    MenuManager.Close(client);
                    OpenBizManageMenu(client, bizID);
                    return;
            }
        }

        public static void OpenBizProductsMenu(Player player)
        {
            if (!player.HasData("SELECTEDBIZ") || !Main.Players[player].BizIDs.Contains(player.GetData<int>("SELECTEDBIZ")))
            {
                MenuManager.Close(player);
                return;
            }

            Menu menu = new Menu("bizproducts", false, false);
            menu.Callback = callback_bizprod;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Товары";
            menu.Add(menuItem);

            var bizID = player.GetData<int>("SELECTEDBIZ");

            Business biz = BizList[bizID];
            foreach (var p in biz.Products)
            {
                menuItem = new Menu.Item(p.Name, Menu.MenuItem.Button);
                menuItem.Text = p.Name;
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizprod(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "back":
                    MenuManager.Close(client);
                    OpenBizManageMenu(client, client.GetData<int>("SELECTEDBIZ"));
                    return;
                default:
                    MenuManager.Close(client);
                    OpenBizSettingMenu(client, item.ID);
                    return;
            }
        }

        public static void OpenBizSettingMenu(Player player, string product)
        {
            if (!player.HasData("SELECTEDBIZ") || !Main.Players[player].BizIDs.Contains(player.GetData<int>("SELECTEDBIZ")))
            {
                MenuManager.Close(player);
                return;
            }

            Menu menu = new Menu("bizsetting", false, false);
            menu.Callback = callback_bizsetting;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = product;
            menu.Add(menuItem);

            var bizID = player.GetData<int>("SELECTEDBIZ");
            Business biz = BizList[bizID];

            foreach (var p in biz.Products)
                if (p.Name == product)
                {
                    string ch = (biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || product == "Татуировки" || product == "Парики" || product == "Патроны") ? "%" : "$";
                    menuItem = new Menu.Item("price", Menu.MenuItem.Card);
                    menuItem.Text = $"Текущая цена: {p.Price}{ch}";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("lefts", Menu.MenuItem.Card);
                    menuItem.Text = $"Кол-во на складе: {p.Lefts}";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("capacity", Menu.MenuItem.Card);
                    menuItem.Text = $"Вместимость склада: {ProductsCapacity[p.Name]}";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("setprice", Menu.MenuItem.Button);
                    menuItem.Text = "Установить цену";
                    menu.Add(menuItem);

                    var price = (product == "Патроны") ? 4 : ProductsOrderPrice[product];
                    menuItem = new Menu.Item("order", Menu.MenuItem.Button);
                    menuItem.Text = $"Заказать: {price}$/шт";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("cancel", Menu.MenuItem.Button);
                    menuItem.Text = "Отменить заказ";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("back", Menu.MenuItem.Button);
                    menuItem.Text = "Назад";
                    menu.Add(menuItem);

                    player.SetData("SELECTPROD", product);
                    menu.Open(player);
                    return;
                }
        }
        private static void callback_bizsetting(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {

            var bizID = client.GetData<int>("SELECTEDBIZ");
            switch (item.ID)
            {
                case "setprice":
                    MenuManager.Close(client);
                    if (client.GetData<string>("SELECTPROD") == "Расходники")
                    {
                        Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно установить цену на этот товар", 3000);
                        return;
                    }
                    Main.OpenInputMenu(client, "Введите новую цену:", "biznewprice");
                    return;
                case "order":
                    MenuManager.Close(client);
                    if (client.GetData<string>("SELECTPROD") == "Татуировки" || client.GetData<string>("SELECTPROD") == "Парики")
                    {
                        Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Если хотите возобновить продажу услуг, то закажите расходные материалы", 3000);
                        return;
                    }
                    Main.OpenInputMenu(client, "Введите кол-во:", "bizorder");
                    return;
                case "cancel":
                    Business biz = BizList[bizID];
                    var prodName = client.GetData<string>("SELECTPROD");

                    foreach (var p in biz.Products)
                    {
                        if (p.Name != prodName) continue;
                        if (p.Ordered)
                        {
                            var order = biz.Orders.FirstOrDefault(o => o.Name == prodName);
                            if (order == null)
                            {
                                p.Ordered = false;
                                return;
                            }
                            if (order.Taked)
                            {
                                Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете отменить заказ, пока его доставляют", 3000);
                                return;
                            }
                            biz.Orders.Remove(order);
                            p.Ordered = false;

                            MoneySystem.Wallet.Change(client, order.Amount * ProductsOrderPrice[prodName]);
                            GameLog.Money($"server", $"player({Main.Players[client].UUID})", order.Amount * ProductsOrderPrice[prodName], $"orderCancel");
                            Notify.Send(client, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отменили заказ на {prodName}", 3000);
                        }
                        else Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не заказывали этот товар", 3000);
                        return;
                    }
                    return;
                case "back":
                    MenuManager.Close(client);
                    OpenBizManageMenu(client, bizID);
                    return;
            }
        }
        #endregion

        public static void OpenBizShopMenu(Player player)
        {
            Business biz = BizList[player.GetData<int>("BIZ_ID")];
            List<List<string>> items = new List<List<string>>();

            foreach (var p in biz.Products)
            {
                List<string> item = new List<string>();
                item.Add(p.Name);
                item.Add($"{p.Price}$");
                item.Add(Product.GetKeyItem(p.Name).ToString());
                items.Add(item);
            }
            string json = JsonConvert.SerializeObject(items);
            Trigger.ClientEvent(player, "shop", json);
        }
        [RemoteEvent("shop")]
        public static void Event_ShopCallback(Player client, int index)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (client.GetData<int>("BIZ_ID") == -1) return;
                Business biz = BizList[client.GetData<int>("BIZ_ID")];

                var prod = biz.Products[index];
                if (Main.Players[client].Money < prod.Price)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }

                if (prod.Name == "Сим-карта")
                {
                    if (!takeProd(biz.ID, 1, prod.Name, prod.Price))
                    {
                        Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно товара на складе", 3000);
                        return;
                    }

                    if (Main.Players[client].Sim != -1) Main.SimCards.Remove(Main.Players[client].Sim);
                    Main.Players[client].Sim = Main.GenerateSimcard(Main.Players[client].UUID);
                    Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили сим-карту с номером {Main.Players[client].Sim}", 3000);
                    GUI.Dashboard.sendStats(client);
                }
                else
                {
                    var type = GetBuyingItemType(prod.Name);
                    if (type != -1)
                    {
                        var tryAdd = nInventory.TryAdd(client, new nItem((ItemType)type));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Ваш инвентарь больше не может вместить {prod.Name}", 3000);
                            return;
                        }
                        else
                        {
                            if (!takeProd(biz.ID, 1, prod.Name, prod.Price))
                            {
                                Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно товара на складе", 3000);
                                return;
                            }
                            nItem item = ((ItemType)type == ItemType.KeyRing) ? new nItem(ItemType.KeyRing, 1, "") : new nItem((ItemType)type);
                            nInventory.Add(client, item);
                        }
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {prod.Name}", 3000);
                    }
                }
                MoneySystem.Wallet.Change(client, -prod.Price);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", prod.Price, $"buyShop");
            }
            catch (Exception e) { Log.Write($"BuyShop: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        public static void OpenPetrolMenu(Player player)
        {
            Business biz = BizList[player.GetData<int>("BIZ_ID")];
            Product prod = biz.Products[0];

            Trigger.ClientEvent(player, "openPetrol");
            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Цена за литр: {prod.Price}$", 7000);
        }
        private static void callback_petrol(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "fill":
                    MenuManager.Close(client);
                    Main.OpenInputMenu(client, "Введите кол-во литров:", "fillcar");
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }

        public static void OpenGunShopMenu(Player player)
        {
            List<List<int>> prices = new List<List<int>>();

            Business biz = BizList[player.GetData<int>("GUNSHOP")];
            for (int i = 0; i < 3; i++)
            {
                List<int> p = new List<int>();
                foreach (var g in biz.Products)
                {
                    if (gunsCat[i].Contains(g.Name))
                        p.Add(g.Price);
                }
                prices.Add(p);
            }

            var ammoPrice = biz.Products.FirstOrDefault(p => p.Name == "Патроны").Price;
            prices.Add(new List<int>());
            foreach (var ammo in AmmoPrices)
            {
                //if(Convert.ToInt32(ammo / 100.0 * ammoPrice) != 0)
                //prices[3].Add(Convert.ToInt32(ammo / 100.0 * ammoPrice));
                //else
                //    prices[3].Add(Convert.ToInt32(Math.Ceiling(ammo / 100.0 * ammoPrice)));
                //player.SendChatMessage(ammo + " / 100 * " + ammoPrice + " = " + (ammo / 100.0 * ammoPrice) + " = " + Convert.ToInt32(ammo / 100.0 * ammoPrice));
                prices[3].Add(Convert.ToInt32(ammo));
            }

            string json = JsonConvert.SerializeObject(prices);
            //Log.Write(json, nLog.Type.Debug);
            Log.Debug(json);
            Trigger.ClientEvent(player, "openWShop", biz.ID, json);
        }

        [RemoteEvent("wshopammo")]
        public static void Event_WShopAmmo(Player client, string text1, string text2)
        {
            try
            {
                //client.SendChatMessage("category " + text1 + "needMoney " + text2);
                var category = Convert.ToInt32(text1.Replace("wbuyslider", null));
                var needMoney = Convert.ToInt32(text2.Trim('$'));
                var ammo = 50;
                //client.SendChatMessage("category " + category + " needMoney " + needMoney + " ammo " + ammo);
                var bizid = client.GetData<int>("GUNSHOP");
                if (!Main.Players[client].Licenses[6])
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии на оружие", 3000);
                    return;
                }

                if (ammo == 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не указали количество патрон", 3000);
                    return;
                }

                var tryAdd = nInventory.TryAdd(client, new nItem(AmmoTypes[category], ammo));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }

                Business biz = BizList[bizid];
                var prod = biz.Products.FirstOrDefault(p => p.Name == "Патроны");
                //var cost = 0;
                //if (Convert.ToInt32(AmmoPrices[category] / 100.0 * prod.Price) != 0)
                //    cost = Convert.ToInt32(AmmoPrices[category] / 100.0 * prod.Price);
                //else
                //    cost = Convert.ToInt32(Math.Ceiling(AmmoPrices[category] / 100.0 * prod.Price));
                var totalPrice = ammo * AmmoPrices[category];

                if (Main.Players[client].Money < totalPrice)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                    return;
                }

                if (!takeProd(bizid, Convert.ToInt32(AmmoPrices[category] / 10.0 * ammo), prod.Name, totalPrice))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно товара на складе", 3000);
                    return;
                }

                MoneySystem.Wallet.Change(client, -totalPrice);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", totalPrice, $"buyWShop(ammo)");
                nInventory.Add(client, new nItem(AmmoTypes[category], ammo));
                Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {nInventory.ItemsNames[(int)AmmoTypes[category]]} x{ammo} за {totalPrice}$", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyWeapons: " + e.Message, nLog.Type.Error); }
        }
        private static List<int> AmmoPrices = new List<int>()
        {
            10, // pistol
            15, // smg
            20, // rifles
            50, // sniperrifles
            25, // sniperrifles
        };
        private static List<ItemType> AmmoTypes = new List<ItemType>()
        {
            ItemType.PistolAmmo,
            ItemType.SMGAmmo,
            ItemType.RiflesAmmo,
            ItemType.SniperAmmo,
            ItemType.ShotgunsAmmo,
        };
        [RemoteEvent("wshop")]
        public static void Event_WShop(Player client, int cat, int index)
        {
            try
            {
                var prodName = gunsCat[cat][index];
                var bizid = client.GetData<int>("GUNSHOP");
                if (!Main.Players[client].Licenses[6])
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии на оружие", 3000);
                    return;
                }
                Business biz = BizList[bizid];
                var prod = biz.Products.FirstOrDefault(p => p.Name == prodName);

                if (Main.Players[client].Money < prod.Price)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                    return;
                }

                ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), prod.Name);

                var tryAdd = nInventory.TryAdd(client, new nItem(wType));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }

                if (!takeProd(bizid, 1, prod.Name, prod.Price))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно товара на складе", 3000);
                    return;
                }

                MoneySystem.Wallet.Change(client, -prod.Price);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", prod.Price, $"buyWShop({prod.Name})");
                Weapons.GiveWeapon(client, wType, Weapons.GetSerial(false, 12));

                Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {prod.Name} за {prod.Price}$", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyWeapons: " + e.Message, nLog.Type.Error); }
        }
        private static List<List<string>> gunsCat = new List<List<string>>()
        {
            new List<string>()
            {
            "Pistol",
            "CombatPistol",
            "Revolver",
            "HeavyPistol",
            "BullpupShotgun",
            "CombatPDW",
            "MachinePistol",
            },
            new List<string>()
            {
            },
            new List<string>()
            {
            },
        };
        #endregion

        public static void changeOwner(string oldName, string newName)
        {
            List<int> toChange = new List<int>();
            lock (BizList)
            {
                foreach (KeyValuePair<int, Business> biz in BizList)
                {
                    if (biz.Value.Owner != oldName) continue;
                    Log.Write($"The biz was found! [{biz.Key}]");
                    toChange.Add(biz.Key);
                }
                foreach (int id in toChange)
                {
                    BizList[id].Owner = newName;
                    BizList[id].UpdateLabel();
                    BizList[id].Save();
                }
            }
        }
    }

    public class Order
    {
        public Order(string name, int amount, bool taked = false)
        {
            Name = name;
            Amount = amount;
            Taked = taked;
        }

        public string Name { get; set; }
        public int Amount { get; set; }
        [JsonIgnore]
        public bool Taked { get; set; }
        [JsonIgnore]
        public int UID { get; set; }
    }

    public class Product
    {
        public Product(int price, int left, int autosell, string name, bool ordered)
        {
            Price = price;
            Lefts = left;
            Autosell = autosell;
            Name = name;
            Ordered = ordered;
        }

        public int Price { get; set; }
        public int Lefts { get; set; }
        public int Autosell { get; set; }
        public string Name { get; set; }
        public bool Ordered { get; set; }

        public static int GetKeyItem(string name)
        {
            try
            {
                return nInventory.ItemsNames.FirstOrDefault(x => x.Value == name).Key;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    public class Business
    {
        public int ID { get; set; }
        public string Owner { get; set; }
        public int SellPrice { get; set; }
        public int Type { get; set; }
        public string Address { get; set; }
        public List<Product> Products { get; set; }
        public int BankID { get; set; }
        public Vector3 EnterPoint { get; set; }
        public Vector3 UnloadPoint { get; set; }
        public int Mafia { get; set; }

        public List<Order> Orders { get; set; }

        [JsonIgnore]
        private Blip blip = null;
        [JsonIgnore]
        private Marker marker = null;
        [JsonIgnore]
        private TextLabel label = null;
        [JsonIgnore]
        private TextLabel mafiaLabel = null;
        [JsonIgnore]
        private ColShape shape = null;
        [JsonIgnore]
        private ColShape truckerShape = null;

        public Business(int id, string owner, int sellPrice, int type, List<Product> products, Vector3 enterPoint, Vector3 unloadPoint, int bankID, int mafia, List<Order> orders)
        {
            ID = id;
            Owner = owner;
            SellPrice = sellPrice;
            Type = type;
            Products = products;
            EnterPoint = enterPoint;
            UnloadPoint = unloadPoint;
            BankID = bankID;
            Mafia = mafia;
            Orders = orders;

            var random = new Random();
            foreach (var o in orders)
            {
                do
                {
                    o.UID = random.Next(000000, 999999);
                } while (BusinessManager.Orders.ContainsKey(o.UID));
                BusinessManager.Orders.Add(o.UID, ID);
            }

            truckerShape = NAPI.ColShape.CreateCylinderColShape(UnloadPoint - new Vector3(0, 0, 1), 3, 10, NAPI.GlobalDimension);
            truckerShape.SetData("BIZID", ID);
            truckerShape.OnEntityEnterColShape += Jobs.Truckers.onEntityEnterDropTrailer;

            float range;
            if (Type == 1) range = 10f;
            else if (Type == 12) range = 5f;
            else range = 1f;
            shape = NAPI.ColShape.CreateCylinderColShape(EnterPoint, range, 3, NAPI.GlobalDimension);

            shape.OnEntityEnterColShape += (s, entity) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 30);
                    NAPI.Data.SetEntityData(entity, "BIZ_ID", ID);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            shape.OnEntityExitColShape += (s, entity) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                    NAPI.Data.SetEntityData(entity, "BIZ_ID", -1);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };

            blip = NAPI.Blip.CreateBlip(Convert.ToUInt32(BusinessManager.BlipByType[Type]), EnterPoint, 0.8f, Convert.ToByte(BusinessManager.BlipColorByType[Type]), Main.StringToU16(BusinessManager.BusinessTypeNames[Type]), 255, 0, true);
            var textrange = (Type == 1) ? 5F : 20F;
            UpdateLabel();
            //if (Type != 1) marker = NAPI.Marker.CreateMarker(1, EnterPoint - new Vector3(0, 0, range - 0.3f), new Vector3(), new Vector3(), range, new Color(255, 255, 255, 220), false, 0);
            switch (Type)
            {
                case 0:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                    marker = NAPI.Marker.CreateMarker(1, EnterPoint + new Vector3(0, 0, 0.15), new Vector3(), new Vector3(), 0.5f, new Color(129, 159, 235, 255), false, 0);
                    break;
            }
        }

        public void UpdateLabel()
        {
            
        }

        public void Destroy()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    blip.Delete();
                    if (marker != null) marker.Delete();
                    label.Delete();
                    mafiaLabel.Delete();
                    shape.Delete();
                    truckerShape.Delete();
                }
                catch { }
            });
        }

        public void Save()
        {
            MySQL.Query($"UPDATE businesses SET owner='{this.Owner}',sellprice={this.SellPrice}," +
                    $"products='{JsonConvert.SerializeObject(this.Products)}',money={this.BankID},mafia={this.Mafia},orders='{JsonConvert.SerializeObject(this.Orders)}' WHERE id={this.ID}");
            MoneySystem.Bank.Save(this.BankID);
        }
    }

    public class BusinessTattoo
    {
        public List<int> Slots { get; set; }
        public string Name { get; set; }
        public string Dictionary { get; set; }
        public string MaleHash { get; set; }
        public string FemaleHash { get; set; }
        public int Price { get; set; }

        public BusinessTattoo(List<int> slots, string name, string dictionary, string malehash, string femalehash, int price)
        {
            Slots = slots;
            Name = name;
            Dictionary = dictionary;
            MaleHash = malehash;
            FemaleHash = femalehash;
            Price = price;
        }
    }
}
