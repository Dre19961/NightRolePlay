using LiteSDK;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Lite.MoneySystem
{
    class Deposit : Script
    {
        private static nLog Log = new nLog("Deposit");
        [ServerEvent(Event.ResourceStart)]
        public static void EnterDepositShape()
        {
            try
            {
                #region #Creating Marker && Blip && Colshape
                foreach (Vector3 pos in BankForDep)
                {
                    NAPI.Blip.CreateBlip(431, pos, 0.9f, 63, Main.StringToU16("Банк"), 255, 0, true, 0, 0);
                    NAPI.Marker.CreateMarker(1, pos + new Vector3(0, 0, 0.1), new Vector3(), new Vector3(), 0.7f, new Color(255, 225, 64), false, 0);
                    var shape = NAPI.ColShape.CreateCylinderColShape(pos, 1, 2, 0);
                    shape.OnEntityEnterColShape += (s, ent) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 1541);
                        }
                        catch (Exception er) { Log.Write("shape.OnEntityEnterColShape.Deposit: " + er.ToString()); }
                    };
                    shape.OnEntityExitColShape += (s, ent) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                        }
                        catch (Exception er) { Log.Write("shape.OnEntityExitColShape.Deposit: " + er.ToString()); }
                    };
                }
                #endregion
            }
            catch (Exception er) { Log.Write(er.ToString(), nLog.Type.Error); }
        }
        private static List<Vector3> BankForDep = new List<Vector3>
        {
            new Vector3(149.44368, -1040.215, 28.254076),
            new Vector3(-1212.9001, -330.6785, 36.666637),
            new Vector3(-350.98334, -49.713284, 47.9225),
        };
        [RemoteEvent("StartDepServ")]
        public static void Event_StartDepServ(Player player, int Money, int FullMoney, int payday, int Hours)
        {
            try
            {
                Deposits.StartDepServ(player, Money, FullMoney, payday, Hours);
            }
            catch (Exception e) { Log.Write("StartDepServ_Event: " + e.ToString(), nLog.Type.Error); }
        }
    }
    #region nice
    public class DepData
    {
        public int UUID;
        public int Hours;
        public int DepPayDay;
        public int DepCount;
    }
    class Deposits : DepData
    {
        private static List<Deposits> DepositCount = new List<Deposits>();
        private static nLog Log = new nLog("Deposits");
        public static void DepSyncInDb()
        {
            DepositCount.Clear();
            DataTable result = MySQL.QueryRead($"SELECT `uuid`,`deposithour`,`depositmon`,`depositpd` FROM `deposit`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow row in result.Rows)
            {
                DepositCount.Add(new Deposits()
                {
                    UUID = Convert.ToInt32(row["uuid"]),
                    Hours = Convert.ToInt32(row["deposithour"]),
                    DepCount = Convert.ToInt32(row["depositmon"]),
                    DepPayDay = Convert.ToInt32(row["depositpd"])
                });
            }
        }
        public static void Open(Player player)
        {
            if (player == null || !Main.Players.ContainsKey(player)) return;
            Deposits dep = DepositCount.FirstOrDefault(b => b.UUID == Main.Players[player].UUID);
            if (dep != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас уже есть открытый вклад.", 3000);
                return;
            }
            Trigger.ClientEvent(player, "OpenDeposit", true);
        }
        public static void StartDepServ(Player player, int Money, int FullMoney, int payday, int Hours)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (Bank.Accounts[Main.Players[player].Bank].Balance < Money)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств на банковском счету.", 3000);
                    return;
                }
                Deposits dep = new Deposits()
                {
                    UUID = Convert.ToInt32(Main.Players[player].UUID),
                    Hours = Convert.ToInt32(Hours),
                    DepCount = Convert.ToInt32(FullMoney),
                    DepPayDay = Convert.ToInt32(payday),
                };
                MySQL.Query("INSERT INTO `deposit`(`uuid`,`deposithour`,`depositpd`,`depositmon`)" + $"VALUES ({dep.UUID},'{dep.Hours}','{dep.DepPayDay}','{dep.DepCount}')");
                DepositCount.Add(dep);
                Bank.Change(Main.Players[player].Bank, -Money, false);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно открыли вклад на {Hours} часов.", 3000);
                player.SendChatMessage($"Вы открыли вклад на {Hours} часов под ставку 0.15%. На сумму: {Money}$. \n По окончанию вклада вы получите {FullMoney}$. В час вы будете получать: {payday}$");
                Trigger.ClientEvent(player, "CloseDeposit", true);
                return;
            }
            catch (Exception e) { Log.Write("StartDepServ: " + e.ToString(), nLog.Type.Error); }
        }
        public static void DepositPayDay(Player player)
        {
            if (player == null || !Main.Players.ContainsKey(player)) return;
            Deposits dep = DepositCount.FirstOrDefault(b => b.UUID == Main.Players[player].UUID);
            if (dep != null)
            {
                var money = dep.DepPayDay;
                if (dep.Hours == 1)
                {
                    Bank.Accounts[Main.Players[player].Bank].Balance += money;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили {money}$. Ваш вклад был успешно закрыт. Итого: {dep.DepCount}$", 3000);
                    DepositCount.Remove(dep);
                    MySQL.Query($"DELETE FROM `deposit` WHERE `uuid`={Main.Players[player].UUID}");
                }
                else
                {
                    dep.Hours -= 1;
                    Bank.Accounts[Main.Players[player].Bank].Balance += money;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили {money}$. Еще осталось {dep.Hours}", 3000);
                    MySQL.Query($"UPDATE `deposit` SET `deposithour`='{dep.Hours}' WHERE `uuid`={Main.Players[player].UUID}");
                }
            }
        }
    }
    #endregion
}