using LiteSDK;
using GTANetworkAPI;
using System;
using Lite.Core;
using System.Collections.Generic;
using System.Text;

namespace Lite.Fractions.Activity
{
    class CodeRing : Script
    {

        #region Инициализация
        private static nLog RLog = new nLog("fmark");
        private static Random rnd = new Random();
        //
        private static GTANetworkAPI.Blip _markBlip;
        //
        public static bool _isStart = false;
        //
        #endregion

        #region Methods
        public static void SpawnAnCode(Player player, int code)
        {
            if (Main.Players[player].FractionID == 7 || Main.Players[player].FractionID == 9 || Main.Players[player].FractionID == 6 || Main.Players[player].FractionID == 14 || Main.Players[player].FractionID == 18)
            {
                _markBlip = NAPI.Blip.CreateBlip(767, player.Position, 1, 1, Main.StringToU16($"Сигнал Код {code}"), 255, 0, true, 0, 0);
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отправили сигнал бедствия", 9000);
            _isStart = true;
        }
        public static void DestroyCode(Player player)
        {
            NAPI.Task.Run(() =>
            {
                _markBlip.Delete();
            });
            _isStart = false;
        }
        #endregion

        [Command("code")]
        public static void CMD_SpawnBox(Player player, int code)
        {
            try
            {
                if (Main.Players[player].FractionID == 0)
                {
                    Notify.Error(player, "Вы не состоите во фракции!");
                    return;
                }
                if (Main.Players[player].FractionID >= 1 && Main.Players[player].FractionID <= 5)
                {
                    Notify.Error(player, "Команда для гос сотрудников");
                    return;
                }
                if (Main.Players[player].FractionID >= 10 && Main.Players[player].FractionID <= 13)
                {
                    Notify.Error(player, "Команда для гос сотрудников");
                    return;
                }
                if (_isStart)
                {
                    Notify.Error(player, "Сигнал уже создан!");
                    return;
                }
                SpawnAnCode(player, code);
                Commands.RPChat("me", player, $"Достал планшет и отправил сигнал бедствия");
                Manager.sendFractionMessage(6, $"~b~[Департамент]{player.Name} отправил сигнал Код {code}!!!");
                Manager.sendFractionMessage(7, $"~b~[Департамент]{player.Name} отправил сигнал Код {code}!!!");
                Manager.sendFractionMessage(9, $"~b~[Департамент]{player.Name} отправил сигнал Код {code}!!!");
                Manager.sendFractionMessage(14, $"~b~[Департамент]{player.Name} отправил сигнал Код {code}!!!");
                Manager.sendFractionMessage(18, $"~b~[Департамент]{player.Name} отправил сигнал! Код {code}!!");
            }
            catch (Exception e) { RLog.Write("code: " + e.Message, nLog.Type.Error); }
        }
        [Command("destroycode")]
        public static void CMD_DelSpawnBox(Player player)
        {
            try
            {
                if (!Core.Group.CanUseCmd(player, "delfmark")) return;
                if (Main.Players[player].FractionID >= 1 && Main.Players[player].FractionID <= 5)
                {
                    Notify.Error(player, "Команда для гос сотрудников");
                    return;
                }
                if (Main.Players[player].FractionID >= 10 && Main.Players[player].FractionID <= 13)
                {
                    Notify.Error(player, "Команда для гос сотрудников");
                    return;
                }
                DestroyCode(player);
                Commands.RPChat("me", player, $"Достал планшет и отключил сигнал бедствия");
            }
            catch (Exception e) { RLog.Write("destroycode: " + e.Message, nLog.Type.Error); }
        }
    }
}
