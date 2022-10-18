using System;
using GTANetworkAPI;
using LiteSDK;
using Lite.MoneySystem;
using Lite.Core;
using System.Linq;

namespace Lite.Casino
{
    class CasinoManager : Script
    {
        private static nLog Log = new nLog("Casino");
        private static int _priceForAdmission = 1;
        private static Vector3 _entrancePosition = new Vector3(924.49506, 46.83829, 80.98636);
        private static Vector3 _exitPosition = new Vector3(1089.695, 206.015, -49);
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Timers.StartTask("newcasino", 1000, () => Roullete.TableCheck());
                NAPI.TextLabel.CreateTextLabel($"~r~Вход", _entrancePosition, 5f, 0.3f, 4, new Color(255, 255, 255), true, 0);
                NAPI.TextLabel.CreateTextLabel($"~r~Выход", _exitPosition, 5f, 0.3f, 4, new Color(255, 255, 255), true, 0);

                var colShapeEnter = NAPI.ColShape.CreateCylinderColShape(_entrancePosition, 1f, 2, 0);
                var colShapeExit = NAPI.ColShape.CreateCylinderColShape(_exitPosition, 1f, 2, 0);

                NAPI.Marker.CreateMarker(1, _entrancePosition - new Vector3(0, 0, 1), new Vector3(), new Vector3(), 1, new Color(129, 159, 235), false, 0);
                NAPI.Marker.CreateMarker(1, _exitPosition - new Vector3(0, 0, 1), new Vector3(), new Vector3(), 1, new Color(129, 159, 235), false, 0);

                NAPI.Blip.CreateBlip(617, _entrancePosition, 1, 4, "Казино", 255, 0, true);

                colShapeEnter.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 805);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "ENTER");
                        }
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeEnter.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                colShapeExit.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 805);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "EXIT");
                        }
                    }
                    catch (Exception ex) { Log.Write("ExitCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeExit.OnEntityExitColShape += OnEntityExitCasinoMainShape;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OnEntityExitCasinoMainShape(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
            NAPI.Data.ResetEntityData(player, "CASINO_MAIN_SHAPE");
        }
        public static void CallBackShape(Player player)
        {
            if (!player.HasData("CASINO_MAIN_SHAPE")) return;
            string data = player.GetData<string>("CASINO_MAIN_SHAPE");
            if (data == "ENTER")
            {
                Trigger.ClientEvent(player, "openDialog", "ENTER_CASINO", $"Стоимость входа: {_priceForAdmission}$. Зайти?");
                return;
            }
            if(data == "EXIT")
            {
                NAPI.Entity.SetEntityPosition(player, _entrancePosition);
                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 113.5));
            }
        }
        public static void EnterCasino(Player player)
        {
            if (!MoneySystem.Wallet.Change(player, -_priceForAdmission))
            {
                Notify.Error(player, "У вас недостаточно средств");
                return;
            }

            Business biz = BusinessManager.BizList.First((t) => t.Value.Type == 14).Value;

            Bank.Data bData = Bank.Get(Main.PlayerBankAccs[biz.Owner]);
            if (bData.ID == 0)
            {
                Log.Write($"TakeProd BankID error: {biz.ID.ToString()}({biz.Owner}) casino enter {_priceForAdmission.ToString()}", nLog.Type.Error);
                return;

            }
            if (!Bank.Change(bData.ID, _priceForAdmission, false))
            {
                Log.Write($"TakeProd error: {biz.ID.ToString()}({biz.Owner}) casino enter {_priceForAdmission.ToString()}", nLog.Type.Error);
                return;
            }
            GameLog.Money($"biz({biz.ID})", $"bank({bData.ID})", _priceForAdmission, "bizProfit");
            Log.Write($"{biz.Owner}'s business get {_priceForAdmission}$ for 'enter casino'", nLog.Type.Success);

            NAPI.Entity.SetEntityPosition(player, _exitPosition);
            NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
        }
    }
}
