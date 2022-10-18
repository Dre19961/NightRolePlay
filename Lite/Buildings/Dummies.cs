using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using LiteSDK;

namespace Lite.Buildings
{
    public static class Dummies
    {
        private static nLog Log = new nLog("Dummies");

        private static Dictionary<string, Tuple<int, int, Vector3, Vector3>> _vehicleDummies = new Dictionary<string, Tuple<int, int, Vector3, Vector3>>();
        private static void VehicleDummy(string vehicleName, Vector3 position, Vector3 rotation, int a = 0, int b = 0)
        {
            if(vehicleName != null && position != null && rotation != null)
            {
                _vehicleDummies.Add(vehicleName, new Tuple<int, int, Vector3, Vector3>(a, b, position, rotation));
            }
        }

        #region Init VehicleList
        public static void OnResourceStart()
        {                //model    //position                  //rotation                        color color



            foreach (var item in _vehicleDummies)
            {
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(item.Key);
                var vehicle = NAPI.Vehicle.CreateVehicle(vh, item.Value.Item3, item.Value.Item4, item.Value.Item1, item.Value.Item2, "LITE", 255, true, false, 0);
                vehicle.SetSharedData("ACCESS", "DUMMY");
            }

            Log.Write("Заспавнено " + _vehicleDummies.Count + " выставочного транспорта", nLog.Type.Info);
        }
        #endregion
    }
}
