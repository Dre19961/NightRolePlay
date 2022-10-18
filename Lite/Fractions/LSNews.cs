using System;
using System.Collections.Generic;
using GTANetworkAPI;
using Lite.Core;
using LiteSDK;
using System.Data;
using System.Linq;
using MySqlConnector;

namespace Lite.Fractions
{
    class LSNews : Script
    {
        private static nLog Log = new nLog("Weazel News");

        private class Advert
        {
            public int ID { get; set; }
            public string Author { get; set; }
            public int AuthorSIM { get; set; }
            public string AD { get; set; }
            public string EditedAD { get; set; }
            public string BlockedBy { get; set; }

            public DateTime OpenedDate { get; set; }
            public DateTime ClosedDate { get; set; }

            public bool Status { get; set; }

            public void Send(Player someone = null)
            {
                if (someone == null)
                {
                    foreach (Player target in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(target)) continue;
                        if (Main.Players[target].FractionID != 15) continue;

                        Trigger.ClientEvent(target, "addadvert", ID, Author, AD);
                    }
                }
                else
                {
                    if (!Main.Players.ContainsKey(someone)) return;
                    if (Main.Players[someone].FractionID != 15) return;

                    Trigger.ClientEvent(someone, "addadvert", ID, Author, AD);
                }
            }
        }
        private static Dictionary<int, Advert> Adverts;

        public static List<string> AdvertNames;
        public static void Init()
        {
            try
            {
                AdvertNames = new List<string>();
                Adverts = new Dictionary<int, Advert>();

                string cmd = @"
                CREATE TABLE IF NOT EXISTS `advertised` (
                    `ID` int(12) unsigned NOT NULL AUTO_INCREMENT,
                    `Author` varchar(40) NOT NULL,
                    `AuthorSIM` int(11) NOT NULL,
                    `AD` varchar(150) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
                    `Editor` varchar(40) DEFAULT NULL,
                    `EditedAD` varchar(150) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
                    `Opened` DateTime NOT NULL,
                    `Closed` DateTime DEFAULT NULL,
                    `Status` tinyint(4) DEFAULT 0,
                    PRIMARY KEY (`ID`)
                ) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
                SELECT * FROM advertised;";

                DataTable result = MySQL.QueryRead(cmd);
                if (result is null) return;
                foreach (DataRow row in result.Rows)
                {
                    if (Convert.ToBoolean((sbyte)row[8]) != false) continue;

                    Advert ad = new Advert
                    {
                        ID = (int)row[0],
                        Author = row[1].ToString(),
                        AuthorSIM = (int)row[2],
                        AD = Main.BlockSymbols(row[3].ToString()),
                        BlockedBy = row[4].ToString(),
                        EditedAD = Main.BlockSymbols(row[5].ToString()),
                        OpenedDate = (DateTime)row[6],
                        ClosedDate = (DateTime)row[7],
                        Status = Convert.ToBoolean((sbyte)row[8])
                    };
                    AdvertNames.Add(ad.Author);
                    Adverts.Add((int)row[0], ad);
                }

            }
            catch (Exception e)
            {
                Log.Write("Init: " + e.ToString(), nLog.Type.Error);
            }
        }
        public static void onLSNPlayerLoad(Player client)
        {
            try
            {
                foreach (Advert ad in Adverts.Values)
                {
                    ad.Send(client);
                }

            }
            catch (Exception e)
            {
                Log.Write("onLSNPlayerLoad: " + e.ToString(), nLog.Type.Error);
            }
        }

        #region Remote Events
        //Админ взял репорт на себя
        [RemoteEvent("takeadvert")]
        public void AdvertTake(Player client, int id, bool retrn = false)
        {
            try
            {
                if (Main.Players[client].FractionID != 15) return;
                if (!Adverts.ContainsKey(id))
                {
                    Remove(id, client);
                    return;
                }

                if (Adverts[id].Status)
                {
                    Remove(id, client);
                    return;
                }

                foreach (Player target in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(target)) continue;
                    if (Main.Players[target].FractionID != 15) continue;

                    if (retrn) Trigger.ClientEvent(target, "setadvert", id, "");
                    else Trigger.ClientEvent(target, "setadvert", id, client.Name);
                }
            }
            catch
            {
            }
        }
        [RemoteEvent("sendadvert")]
        public void AdvertSend(Player player, int ID, string answer)
        {
            try
            {
                if (Main.Players[player].FractionID != 15) return;
                if (!Adverts.ContainsKey(ID)) return;
                if (!Adverts[ID].Status)
                {
                    AddAnswer(player, ID, answer);
                }
                else
                {
                    player.SendChatMessage("Это объявление более недоступно для изменения.");
                    Remove(ID, player);
                }
            }
            catch
            {
            }
        }
        #endregion

        public static void AddAdvert(Player player, string question, int price)
        {
            try
            {
                question = Main.BlockSymbols(question);

                GameLog.Money($"bank({Main.Players[player].Bank})", $"server", price, "ad");
                player.SetData("NEXT_AD", DateTime.Now.AddMinutes(45));
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы подали объявление. Ожидайте модерации", 3000);

                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "INSERT INTO `advertised` (`Author`,`AuthorSIM`,`AD`,`Opened`,`Closed`) VALUES (@pn,@sim,@ques,@time,@ntime); SELECT LAST_INSERT_ID();";
                cmd.Parameters.AddWithValue("@pn", player.Name);
                cmd.Parameters.AddWithValue("@sim", Main.Players[player].Sim);
                cmd.Parameters.AddWithValue("@ques", question);
                cmd.Parameters.AddWithValue("@time", MySQL.ConvertTime(DateTime.Now));
                cmd.Parameters.AddWithValue("@ntime", MySQL.ConvertTime(DateTime.MinValue));

                DataTable dt = MySQL.QueryRead(cmd);

                int id = Convert.ToInt32(dt.Rows[0][0]);
                Advert advert = new Advert
                {
                    ID = id,
                    Author = player.Name,
                    AuthorSIM = Main.Players[player].Sim,
                    AD = question,
                    BlockedBy = "",
                    EditedAD = "",
                    Status = false,
                    OpenedDate = DateTime.Now,
                    ClosedDate = DateTime.MinValue
                };
                AdvertNames.Add(advert.Author);
                advert.Send();
                Adverts.Add(id, advert);
                foreach (Player p in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1) p.SendChatMessage($"~r~Объявление от {player.Name.Replace('_', ' ')} (ID {player.Value} | Объявление №{id}): {question}");
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void AddAnswer(Player player, int repID, string response, bool deleted = false)
        {
            try
            {
                response = Main.BlockSymbols(response);

                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL == 0)
                {
                    if (Main.Players[player].FractionID != 15) return;
                }
                if (!Adverts.ContainsKey(repID))
                {
                    if (deleted) player.SendChatMessage("Объявления с подобным номером не было найдено.");
                    return;
                }
                DateTime now = DateTime.Now;

                if (!deleted)
                {
                    try
                    {
                        int moneyad = Adverts[repID].AD.Length / 15 * 6;
                        MoneySystem.Bank.Change(Main.Players[player].Bank, (moneyad * 60 / 100), false);
                        Stocks.fracStocks[6].Money += moneyad * 40 / 100;
                        if (Adverts[repID].AuthorSIM >= 1) NAPI.Chat.SendChatMessageToAll("!{#00BCD4}" + $"Объявление от {Adverts[repID].Author.Replace('_', ' ')}: {response} | Тел: {Adverts[repID].AuthorSIM}");
                        else NAPI.Chat.SendChatMessageToAll("!{#00BCD4}" + $"Объявление от {Adverts[repID].Author.Replace('_', ' ')}: {response}");
                        NAPI.Chat.SendChatMessageToAll("!{#00BCD4}" + $"Редактор объявления: {player.Name.Replace('_', ' ')}.");
                    }
                    catch
                    {
                    }
                }
                else
                {
                    if (Main.Players[player].AdminLVL != 0) GameLog.Admin($"{player.Name}", $"delAd", $"{Adverts[repID].Author}");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы удалили объявление игрока {Adverts[repID].Author}", 3000);
                    Player target = NAPI.Player.GetPlayerFromName(Adverts[repID].Author);
                    if (target != null) Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"{player.Name} удалил Ваше объявление по причине: {response}.", 3000);
                    response += " | Удалено";
                }

                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "UPDATE advertised SET Editor=@resp,EditedAD=@res,Status=@st,Closed=@time WHERE ID=@repid LIMIT 1";
                cmd.Parameters.AddWithValue("@resp", player.Name);
                cmd.Parameters.AddWithValue("@res", response);
                cmd.Parameters.AddWithValue("@st", true);
                cmd.Parameters.AddWithValue("@time", MySQL.ConvertTime(now));
                cmd.Parameters.AddWithValue("@repid", repID);
                MySQL.Query(cmd);

                AdvertNames.Remove(Adverts[repID].Author);

                Adverts[repID].Author = player.Name;
                Adverts[repID].EditedAD = response;
                Adverts[repID].ClosedDate = now;
                Adverts[repID].Status = true;
                Remove(repID);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        private static void Remove(int ID_, Player someone = null)
        {
            try
            {
                Log.Debug($"Remove {ID_}");
                if (someone == null)
                {
                    foreach (Player target in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(target)) continue;
                        if (Main.Players[target].FractionID != 15) continue;

                        Trigger.ClientEvent(target, "deladvert", ID_);
                    }
                }
                else
                {
                    if (!Main.Players.ContainsKey(someone)) return;
                    if (Main.Players[someone].FractionID != 15) return;

                    Trigger.ClientEvent(someone, "deladvert", ID_);
                }
                Adverts.Remove(ID_);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
    }
}
