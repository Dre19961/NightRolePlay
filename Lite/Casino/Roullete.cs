using GTANetworkAPI;
using Newtonsoft.Json;
using Lite.Core;
using Lite.Fractions;
using LiteSDK;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lite.Casino
{
    class Roullete : Script
    {

        private static nLog Log = new nLog("Roullete");
            #region Casino Roulette

        public static void onplayerdisconnect(Player player)
        {
            try
            {
                if (player.HasData("key")) { player.ResetData("key"); Trigger.ClientEvent(player, "deleteObjectsd"); }
                if (player.HasData("key2")) { player.ResetData("key2"); Trigger.ClientEvent(player, "deleteObjectsd2"); }
                if (player.HasData("key3")) { player.ResetData("key3"); Trigger.ClientEvent(player, "deleteObjectsd3"); }
                if (player.HasData("key4")) { player.ResetData("key4"); Trigger.ClientEvent(player, "deleteObjectsd4"); }
                if (player.HasData("key5")) { player.ResetData("key5"); Trigger.ClientEvent(player, "deleteObjectsd5"); }
                if (player.HasData("seat"))
                {
                    int table = player.GetData<int>("tablekey");
                    int seat = player.GetData<int>("seat");
                    tablelib.Casino[table].Seats[seat] = true;
                    player.ResetData("seat");
                    player.ResetData("tablekey");
                }
            }
            catch (Exception e) { Log.Write("playerPressCuffBut: " + e.Message, nLog.Type.Error); }
        }
        #region LeaveSeat
        [RemoteEvent("leaveCasinoSeat")]
        public static void leaveCasinoSeat(Player player)
        {
            try
            {
                if (player.GetData<bool>("ingames") == true)
                {
                    if (player.HasData("onseat"))
                    {
                        player.ResetData("onseat");
                        Trigger.ClientEvent(player, "freeze", false);
                        int table = player.GetData<int>("tablekey");
                        int seat = player.GetData<int>("seat");
                        tablelib.Casino[table].Seats[seat] = true;
                        Main.OffAntiAnim(player);
                        if (seat != 1)
                        {
                            player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "sit_exit_left", 33);
                        }
                        else
                        {
                            player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "sit_exit_right", 33);
                        }
                        player.ResetData("seat");
                        NAPI.Task.Run(() =>
                        {
                            player.StopAnimation();
                            player.SetData("ingames", false);
                            Trigger.ClientEvent(player, "freeze", false);
                        }, 4000);
                        Trigger.ClientEvent(player, "setBlockControl", false);
                        player.SetSharedData("afkgames", false);
                        player.SetSharedData("seats", false);
                        Trigger.ClientEvent(player, "seatsdisable");
                    }
                }
            }
            catch (Exception e) { Log.Write("playerPressCuffBut: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Seat
        [RemoteEvent("occupyCasinoSeat")]
        public static void occupyCasinoSeat(Player player, int table, int seat)
        {
            try
            {
                if (tablelib.Casino[table].Seats[seat] == false) { Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Это место занято!", 3000); return; }
                if (player.GetData<bool>("ingames") == false)
                {
                    player.Position = tablelib.Casino[table].SeatsPositions[seat];
                    playergoanimtoall(player);
                    if (player.HasData("key"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас есть активная ставка за другим столом", 3000);
                        return;
                    }
                    player.SetData("onseat", true);
                    if (seat != 1)
                    {
                        player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "sit_enter_left_side", 33);
                        NAPI.Task.Run(() =>
                        {
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "sit_enter_left_side", 34);
                            NAPI.Task.Run(() =>
                            {
                                Trigger.ClientEvent(player, "freeze", true);
                            }, 500);
                        }, 3500);
                    }
                    else
                    {
                        player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "sit_enter_right_side", 33);
                        NAPI.Task.Run(() =>
                        {
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "sit_enter_right_side", 34);
                            NAPI.Task.Run(() =>
                            {
                                Trigger.ClientEvent(player, "freeze", true);
                            }, 500);
                        }, 3500);
                    }
                    Trigger.ClientEvent(player, "setBlockControl", true);
                    // player.FreezePosition = true;
                    player.SetData("tablekey", table);
                    tablelib.Casino[table].Seats[seat] = false;
                    player.SetData("ingames", true);
                    player.SetData("seat", seat);
                    player.SetSharedData("seats", true);
                    Trigger.ClientEvent(player, "playerSitAtCasinoTable", player, table);
                    Trigger.ClientEvent(player, "seatsactive");
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"подождите немного...", 3000);
            }
            catch (Exception e) { Log.Write("playerPressCuffBut: " + e.Message, nLog.Type.Error); }
        }
        #endregion


        #region Tables
        public static void TableCheck()
        {
            foreach (var table in tablelib.Casino.Keys)
            {
                var p = Main.GetPlayersInRadiusOfPosition(tablelib.Casino[table].TablePosition, 5);
                foreach (var player in p)
                {
                 //   Console.WriteLine($"StartTimer {tablelib.Casino[table].Tablestartgame}: FistBet {tablelib.Casino[table].Firstbet} Стол ¹{table}");
                    if (tablelib.Casino[table].Tablestartgame == true)
                    {
                        Trigger.ClientEvent(player, "rouletteAllowBets", true);
                        //Console.WriteLine($"StartTimer {tablelib.Casino[table].StartTimer}: FistBet {tablelib.Casino[table].Firstbet} Ñòîë ¹{table}");
                    }
                    else
                    {
                        Trigger.ClientEvent(player, "rouletteAllowBets", false);
                    }
                    //Console.WriteLine($"StartTimer {tablelib.Casino[table].StartTimer}: FistBet {tablelib.Casino[table].Firstbet} Ñòîë ¹{table}");
                    if (tablelib.Casino[table].StartTimer == true && tablelib.Casino[table].Firstbet == false)
                    {
                        startnewbet(table);
                    }
                }
            }
        }
        #endregion
        #region timerstart
        public static void newtimer(int table)
        {
            var rndt = new Random();
            tablelib.Casino[table].Ballends = rndt.Next(1, 36);
        }
        #endregion
        public static void startnewbet(int table)
        {
            try
            {
                tablelib.Casino[table].StartTimer = false;
                newtimer(table);
                NAPI.Task.Run(() =>
                {
                    tablelib.Casino[table].Tablestartgame = false;
                  //  Console.WriteLine($"Âûêëþ÷àåì ñòàâêè çà ñòîëîì {table}: {tablelib.Casino[table].Tablestartgame}");
                }, 20000);
                var p = Main.GetPlayersInRadiusOfPosition(new Vector3(1140.802, 259.2148, -52.44087), 100);
                NAPI.Task.Run(() =>
                {
                    foreach (var player in p)
                    {
                        Trigger.ClientEvent(player, "entityStreamIn", "S_M_Y_Casino_01");
                        Trigger.ClientEvent(player, "spinRouletteWheel", table, 2, $"exit_{tablelib.Casino[table].Ballends}_wheel", $"exit_{tablelib.Casino[table].Ballends}_ball");
                    }
                }, 23000);
                NAPI.Task.Run(() =>
                {
                    foreach (var player in p)
                    {
                        tablelib.Casino[table].StartTimer = true;
                        tablelib.Casino[table].Firstbet = true;
                        tablelib.Casino[table].Tablestartgame = true;
                        Trigger.ClientEvent(player, "clearRouletteTable", table);
                        getprize(player, table);
                    }
                }, 43000);
            }
            catch (Exception e) { Log.Write("Çàïóñê ñòîëîâ: " + e.Message, nLog.Type.Error); }
        }
        #region RemoveBet
        [RemoteEvent("removeRouletteBet")]
        public static void removeRouletteBet(Player player, int closestChipSpot)
        {
            int money = 0;
            if (player.HasData("key"))
            {
                if (player.GetData<int>("key") == closestChipSpot)
                {
                    money = player.GetData<int>("keymoney");
                    MoneySystem.Wallet.Change(player, +money);
                    player.ResetData("key");
                    Trigger.ClientEvent(player, "deleteObjectsd");
                    return;
                }
            }
            if (player.HasData("key2"))
            {
                if (player.GetData<int>("key2") == closestChipSpot)
                {
                    money = player.GetData<int>("keymoney2");
                    player.ResetData("key2");
                    MoneySystem.Wallet.Change(player, +money);
                    Trigger.ClientEvent(player, "deleteObjectsd2");
                    return;
                }
            }
            if (player.HasData("key3"))
            {
                if (player.GetData<int>("key3") == closestChipSpot)
                {
                    money = player.GetData<int>("keymoney3");
                    player.ResetData("key3");
                    MoneySystem.Wallet.Change(player, +money);
                    Trigger.ClientEvent(player, "deleteObjectsd3");
                    return;
                }
            }
            if (player.HasData("key4"))
            {
                if (player.GetData<int>("key4") == closestChipSpot)
                {
                    money = player.GetData<int>("keymoney4");
                    player.ResetData("key4");
                    MoneySystem.Wallet.Change(player, +money);
                    Trigger.ClientEvent(player, "deleteObjectsd4");
                    return;
                }
            }
            if (player.HasData("key5"))
            {
                if (player.GetData<int>("key5") == closestChipSpot)
                {
                    money = player.GetData<int>("keymoney5");
                    player.ResetData("key5");
                    MoneySystem.Wallet.Change(player, +money);
                    Trigger.ClientEvent(player, "deleteObjectsd5");
                    return;
                }
            }
        }
        #endregion
        [RemoteEvent("makeRouletteBet")]
        public static void makeRouletteBet(Player player, int closestChipSpot, int money)
        {
            try
            {
                int table = player.GetData<int>("tablekey");
                if(money < 0 && money > 999999999)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Некоректная ставка", 3000);
                    return;
                }
                if (Main.Players[player].Money < money)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно фишек", 3000);
                    return;
                }
                if (tablelib.Casino[table].Firstbet == true)
                {
                    tablelib.Casino[table].Firstbet = false;
                }  
                player.SetSharedData("afkgames", true);
               // Log.Write($"playerPressCuffBut: {money}", nLog.Type.Error);
                if (!player.HasData("key"))
                {
                    player.SetData("keymoney", money);
                    player.SetData("key", closestChipSpot);
                    MoneySystem.Wallet.Change(player, -money);
                    Trigger.ClientEvent(player, "chipsserver", closestChipSpot, money);
                    return;
                }
                if (!player.HasData("key2"))
                {
                    if (player.GetData<int>("key") != closestChipSpot)
                    {
                        player.SetData("keymoney2", money);
                        player.SetData("key2", closestChipSpot);
                        MoneySystem.Wallet.Change(player, -money);
                        Trigger.ClientEvent(player, "chipsserver2", closestChipSpot, money);
                        return;
                    }
                    else { Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже ставили на это число", 3000); return; };
                }
                if (!player.HasData("key3"))
                {
                    if (player.GetData<int>("key") != closestChipSpot && player.GetData<int>("key2") != closestChipSpot)
                    {
                        player.SetData("keymoney3", money);
                        player.SetData("key3", closestChipSpot);
                        MoneySystem.Wallet.Change(player, -money);
                        Trigger.ClientEvent(player, "chipsserver3", closestChipSpot, money);
                        return;
                    }
                    else { Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже ставили на это число", 3000); return; }
                }
                if (!player.HasData("key4"))
                {
                    if (player.GetData<int>("key") != closestChipSpot && player.GetData<int>("key3") != closestChipSpot)
                    {
                        player.SetData("keymoney4", money);
                        player.SetData("key4", closestChipSpot);
                        MoneySystem.Wallet.Change(player, -money);
                        Trigger.ClientEvent(player, "chipsserver4", closestChipSpot, money);
                        return;
                    }
                    else { Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже ставили на это число", 3000); return; }
                }
                if (!player.HasData("key5"))
                {
                    if (player.GetData<int>("key") != closestChipSpot && player.GetData<int>("key4") != closestChipSpot)
                    {
                        player.SetData("keymoney5", money);
                        player.SetData("key5", closestChipSpot);
                        MoneySystem.Wallet.Change(player, -money);
                        Trigger.ClientEvent(player, "chipsserver5", closestChipSpot, money);
                        return;
                    }
                    else { Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже ставили на это число", 3000); return; }
                }
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы поставили максимальные 3 ставки", 3000);
            }
            catch (Exception e) { Log.Write("playerPressCuffBut: " + e.Message, nLog.Type.Error); }
        }
        #region getprize
        public static void getprize(Player player, int table)
        {
            try
            {
                if (player.HasData("tablekey"))
                {
                    if (player.GetData<int>("tablekey") == table)
                    {
                        int money = 0;
                        int money2 = 0;
                        int money3 = 0;
                        int key3 = -5;
                        int key2 = -5;
                        int key = -5;
                        if (player.HasData("key")) key = player.GetData<int>("key");
                        if (player.HasData("key2")) key2 = player.GetData<int>("key2");
                        if (player.HasData("key3")) key3 = player.GetData<int>("key3");
                        player.SetData("winmoney", 0);
                        if (player.HasData("keymoney")) money = player.GetData<int>("keymoney");
                        if (player.HasData("keymoney2")) money2 = player.GetData<int>("keymoney2");
                        if (player.HasData("keymoney3")) money3 = player.GetData<int>("keymoney3");
                        player.SetData("win", false);
                        checkprize(player, table, key, money);
                        checkprize(player, table, key2, money2);
                        checkprize(player, table, key3, money3);
                        int monis = 0;
                        int i = betlib.Bets[tablelib.Casino[table].Ballends].Number;
                        string s = betlib.Bets[tablelib.Casino[table].Ballends].Color;
                        if (player.HasData("winmoney"))
                        {
                            monis = player.GetData<int>("winmoney");
                        }
                        winnotify(player, i, s, monis);
                        if (player.HasData("key")) { player.ResetData("key"); Trigger.ClientEvent(player, "deleteObjectsd"); }
                        if (player.HasData("key2")) { player.ResetData("key2"); Trigger.ClientEvent(player, "deleteObjectsd2"); }
                        if (player.HasData("key3")) { player.ResetData("key3"); Trigger.ClientEvent(player, "deleteObjectsd3"); }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
        }
        #endregion
        public static void winnotify(Player player, int i, string s, int money)
        {
            if (player.GetData<bool>("win") == true)
            {
                MoneySystem.Wallet.Change(player, +money);
                if(i == 1000 && s == "Зеленок")
                {
                    i = 0;
                    s = "дабл зеленое";
                }
                if (player.HasData("ingames"))
                {
                    if (player.GetData<bool>("ingames") == true)
                    {
                        if (player.HasData("key") || player.HasData("key2") || player.HasData("key3"))
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Выпало число {i} {s}, вы выйграли {money}$", 3000);
                            player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "reaction_great_var_01", 33);
                            NAPI.Task.Run(() =>
                            {
                                player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "idle_var_01", 33);
                            }, 3000);
                        }
                    }
                }
            }
            else
            {
                if (player.HasData("ingames"))
                {
                    if (player.GetData<bool>("ingames") == true)
                    {
                        if (player.HasData("key") || player.HasData("key2") || player.HasData("key3") || player.HasData("key4") || player.HasData("key5"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Выпало число  {i} {s}", 3000);
                            player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "reaction_terrible_var_04", 33);
                            NAPI.Task.Run(() =>
                            {
                                player.PlayAnimation("anim_casino_b@amb@casino@games@shared@player@", "idle_var_01", 33);
                            }, 3000);
                        }
                    }
                }
            }
        }
        private static Dictionary<string, object> AnimList = new Dictionary<string, object>
        {
            {"tableLib", "anim_casino_b@amb@casino@games@roulette@table" },
            {"dealerLib", "anim_casino_b@amb@casino@games@roulette@dealer" },
            {"tableStart", "intro_wheel" },
            {"tableMain", "loop_wheel" },
            {"ballStart", "intro_ball" },
            {"ballMain", "loop_ball" },
            {"ballRot", 32.6 },
            {"speed", 136704 },
        };
        public static void playergoanimtoall(Player player)
        {
            Trigger.ClientEvent(player, "initRoulette", JsonConvert.SerializeObject(AnimList));
        }
        #endregion
        public static void checkprize(Player player, int table, int key, int money)
        {
            int i = betlib.Bets[tablelib.Casino[table].Ballends].Number;
            string s = betlib.Bets[tablelib.Casino[table].Ballends].Color;
            int xkey = 0;
            if (key >= 50 && key <= 107) xkey = 18;
            if (key >= 108 && key <= 119) xkey = 12;
            if (key >= 41 && key <= 43) xkey = 3;
            if (key >= 44 && key <= 49) xkey = 2;
            if (key >= 120 && key <= 141) xkey = 9;
            if (key == 142) xkey = 7;
            if (key >= 143 && key <= 153) xkey = 6;
            if (s != "Зеленое" && key == i + 1 )
            {
                player.SetData("winmoney", player.GetData<int>("winmoney") + money * 36);
                player.SetData("win", true);
            }
            if (s == "Зеленое" && key == i && i == 0)
            {
                player.SetData("winmoney", player.GetData<int>("winmoney") + money * 38);
                player.SetData("win", true);
            }
            if (i == 1000 && key == 1)
            {
                player.SetData("winmoney", player.GetData<int>("winmoney") + money * 40);
                player.SetData("win", true);
            }
            switch (key)
            {
                case 38:
                    if (s != "Зеленое" && i >= 1 && i <= 12)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * 3);
                        player.SetData("win", true);
                    }
                    break;
                case 39:
                    if (s != "Зеленое" && i >= 13 && i <= 24)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * 3);
                        player.SetData("win", true);
                    }
                    break;
                case 40:
                    if (s != "Зеленое" && i >= 25 && i <= 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * 3);
                        player.SetData("win", true);
                    }
                    break;
                case 41:
                    if(i == 1 || i == 4 || i == 7 || i == 10 || i == 13 || i == 16 || i == 19 || i == 22 || i == 25 || i == 28 || i == 31 || i == 34)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break; 
                case 42:
                    if(i == 2 || i == 5 || i == 8 || i == 11 || i == 14 || i == 17 || i == 20 || i == 23 || i == 26 || i == 29 || i == 32 || i == 35)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 43:
                    if(i == 3 || i == 6 || i == 9 || i == 12 || i == 15 || i == 18 || i == 21 || i == 24 || i == 27 || i == 30 || i == 33 || i == 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 44:
                    if (i >= 1 && i <= 18)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 45:
                    if (i % 2 == 0 && s != "Зеленое")
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 46:
                    if (s == "Красное")
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 47:
                    if (s == "Черное")
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 48:
                    if (i % 2 != 0 && s != "Зеленое")
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 49:
                    if (i >= 19 && i <= 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 50:
                    if (i == 1 || i == 2)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 51:
                    if (i == 1 || i == 2)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 52:
                    if (i == 2 || i == 3)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 53:
                    if (i == 4 || i == 5)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 54:
                    if (i == 5 || i == 6)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 55:
                    if (i == 7 || i == 8)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 56:
                    if (i == 9 || i == 8)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 57:
                    if (i == 10 || i == 11)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 58:
                    if (i == 12 || i == 11)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 59:
                    if (i == 13 || i == 14)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 60:
                    if (i == 14 || i == 15)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 61:
                    if (i == 16 || i == 17)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 62:
                    if (i == 17 || i == 18)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 63:
                    if (i == 19 || i == 20)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 64:
                    if (i == 20 || i == 21)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 65:
                    if (i == 22 || i == 23)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 66:
                    if (i == 23 || i == 24)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 67:
                    if (i == 25 || i == 26)
                    {

                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 68:
                    if (i == 26 || i == 27)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 69:
                    if (i == 28 || i == 29)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 70:
                    if (i == 29 || i == 30)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 71:
                    if (i == 31 || i == 32)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 72:
                    if (i == 32 || i == 33)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 73:
                    if (i == 34 || i == 35)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 74:
                    if (i == 35 || i == 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 75:
                    if (i == 1 || i == 4)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 76:
                    if (i == 2 || i == 5)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 77:
                    if (i == 3 || i == 6)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 78:
                    if (i == 4 || i == 7)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 79:
                    if (i == 5 || i == 8)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 80:
                    if (i == 6 || i == 9)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 81:
                    if (i == 7 || i == 10)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 82:
                    if (i == 8 || i == 11)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 83:
                    if (i == 9 || i == 12)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 84:
                    if (i == 10 || i == 13)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 85:
                    if (i == 11 || i == 14)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 86:
                    if (i == 12 || i == 15)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 87:
                    if (i == 13 || i == 16)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 88:
                    if (i == 14 || i == 17)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 89:
                    if (i == 15 || i == 18)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 90:
                    if (i == 16 || i == 19)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 91:
                    if (i == 17 || i == 20)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 92:
                    if (i == 18 || i == 21)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 93:
                    if (i == 19 || i == 22)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 94:
                    if (i == 20 || i == 23)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 95:
                    if (i == 21 || i == 24)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 96:
                    if (i == 22 || i == 25)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 97:
                    if (i == 23 || i == 26)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 98:
                    if (i == 24 || i == 27)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 99:
                    if (i == 25 || i == 28)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 100:
                    if (i == 26 || i == 29)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 101:
                    if (i == 27 || i == 30)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 102:
                    if (i == 28 || i == 31)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 103:
                    if (i == 29 || i == 32)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 104:
                    if (i == 30 || i == 33)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 105:
                    if (i == 31 || i == 34)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 106:
                    if (i == 32 || i == 35)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 107:
                    if (i == 33 || i == 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 108:
                    if (i == 1 || i == 2 || i == 3)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 109:
                    if (i == 4 || i == 5 || i == 6)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 110:
                    if (i == 7 || i == 8 || i == 9)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 111:
                    if (i == 10 || i == 11 || i == 12)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 112:
                    if (i == 13 || i == 14 || i == 15)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 113:
                    if (i == 16 || i == 17 || i == 18)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 114:
                    if (i == 19 || i == 20 || i == 21)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 115:
                    if (i == 22 || i == 23 || i == 24)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 116:
                    if (i == 25 || i == 26 || i == 27)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 117:
                    if (i == 28 || i == 29 || i == 30)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 118:
                    if (i == 31 || i == 32 || i == 33)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 119:
                    if (i == 34 || i == 35 || i == 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 120:
                    if (i == 1 || i == 2 || i == 4 || i == 5)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 121:
                    if (i == 2 || i == 5 || i == 3 || i == 6)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 122:
                    if (i == 4 || i == 7 || i == 5 || i == 8)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 123:
                    if (i == 5 || i == 6 || i == 9 || i == 8)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 124:
                    if (i == 7 || i == 8 || i == 10 || i == 11)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 125:
                    if (i == 8 || i == 11 || i == 9 || i == 12)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 126:
                    if (i == 10 || i == 13 || i == 11 || i == 14)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 127:
                    if (i == 12 || i == 15 || i == 11 || i == 14)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 128:
                    if (i == 16 || i == 13 || i == 17 || i == 14)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 129:
                    if (i == 15 || i == 18 || i == 17 || i == 14)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 130:
                    if (i == 16 || i == 17 || i == 19 || i == 20)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 131:
                    if (i == 18 || i == 17 || i == 21 || i == 20)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 132:
                    if (i == 19 || i == 22 || i == 20 || i == 23)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 133:
                    if (i == 20 || i == 23 || i == 21 || i == 24)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 134:
                    if (i == 22 || i == 23 || i == 25 || i == 26)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 135:
                    if (i == 24 || i == 23 || i == 27 || i == 26)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 136:
                    if (i == 25 || i == 28 || i == 26 || i == 29)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 137:
                    if (i == 27 || i == 30 || i == 26 || i == 29)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 138:
                    if (i == 28 || i == 29 || i == 31 || i == 32)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 139:
                    if (i == 30 || i == 29 || i == 33 || i == 32)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 140:
                    if (i == 31 || i == 32 || i == 34 || i == 35)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 141:
                    if (i == 32 || i == 35 || i == 33 || i == 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 142:
                    if (i == 0 || i == 1000 )
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);

                    }
                    break;
                case 143:
                    if (i == 1 || i == 2 || i == 3 || i == 4 || i == 5 || i == 6)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 144:
                    if (i == 7 || i == 8 || i == 9 || i == 4 || i == 5 || i == 6)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 145:
                    if (i == 7 || i == 8 || i == 9 || i == 10 || i == 11 || i == 12)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 146:
                    if (i == 13 || i == 14 || i == 15 || i == 10 || i == 11 || i == 12)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 147:
                    if (i == 13 || i == 14 || i == 15 || i == 16 || i == 17 || i == 18)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 148:
                    if (i == 19 || i == 20 || i == 21 || i == 16 || i == 17 || i == 18)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 149:
                    if (i == 19 || i == 20 || i == 21 || i == 22 || i == 23 || i == 24)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 150:
                    if (i == 25 || i == 26 || i == 27 || i == 22 || i == 23 || i == 24)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 151:
                    if (i == 25 || i == 26 || i == 27 || i == 28 || i == 29 || i == 30)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 152:
                    if (i == 31 || i == 32 || i == 33 || i == 28 || i == 29 || i == 30)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
                case 153:
                    if (i == 31 || i == 32 || i == 33 || i == 34 || i == 35 || i == 36)
                    {
                        player.SetData("winmoney", player.GetData<int>("winmoney") + money * xkey);
                        player.SetData("win", true);
                    }
                    break;
            }
        }
    }
}