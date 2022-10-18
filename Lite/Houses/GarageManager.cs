using GTANetworkAPI;
using Newtonsoft.Json;
using Lite.Core;
using LiteSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Lite.Houses
{
    #region GarageType Class
    class GarageType
    {
        public Vector3 Position { get; }
        public List<Vector3> VehiclesPositions { get; }
        public List<Vector3> VehiclesRotations { get; }
        public int MaxCars { get; }

        public GarageType(Vector3 position, List<Vector3> vehiclesPositions, List<Vector3> vehiclesRotations, int maxCars)
        {
            Position = position;
            VehiclesPositions = vehiclesPositions;
            VehiclesRotations = vehiclesRotations;
            MaxCars = maxCars;
        }
    }
    #endregion

    #region Garage Class
    class Garage
    {
        public int ID { get; }
        public int Type { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        [JsonIgnore] public int Dimension { get; set; }

        [JsonIgnore]
        private ColShape shape;

        [JsonIgnore]
        private ColShape intShape;
        [JsonIgnore]
        private Marker intMarker;

        [JsonIgnore]
        private Dictionary<string, Tuple<int, Entity>> entityVehicles = new Dictionary<string, Tuple<int, Entity>>();
        [JsonIgnore]
        private Dictionary<string, Entity> vehiclesOut = new Dictionary<string, Entity>();
        private nLog Log = new nLog("Garage");

        public Garage(int id, int type, Vector3 position, Vector3 rotation)
        {
            ID = id;
            Type = type;
            Position = position;
            Rotation = rotation;

            shape = NAPI.ColShape.CreateCylinderColShape(position - new Vector3(0, 0, 1), 1, 3, 0);
            shape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "GARAGEID", id);
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 40);
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
            };
            shape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    if (NAPI.Entity.GetEntityType(ent) != EntityType.Player) return;
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    NAPI.Data.ResetEntityData(ent, "GARAGEID");
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
            };
        }
        public bool CheckCar(bool checkin, string number)
        {
            if (checkin)
            {
                if (entityVehicles.ContainsKey(number)) return true;
                else return false;
            }
            else
            {
                if (vehiclesOut.ContainsKey(number)) return true;
                else return false;
            }
        }
        public Vehicle GetOutsideCar(string number)
        {
            if (!vehiclesOut.ContainsKey(number)) return null;
            return NAPI.Entity.GetEntityFromHandle<Vehicle>(vehiclesOut[number]);
        }
        public void DeleteCar(string number, bool resetPosition = true)
        {
            if (entityVehicles.ContainsKey(number))
            {
                NAPI.Task.Run(() => {
                    try
                    {
                        if (VehicleManager.Vehicles.ContainsKey(number))
                        {
                            VehicleManager.Vehicles[number].Items = NAPI.Data.GetEntityData(entityVehicles[number].Item2, "ITEMS");
                            var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[number].Model));
                            VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntitySharedData(entityVehicles[number].Item2, "PETROL")) ? VehicleManager.VehicleTank[vclass] : entityVehicles[number].Item2.GetSharedData<int>("PETROL");
                        }
                        NAPI.Entity.DeleteEntity(entityVehicles[number].Item2);
                        entityVehicles.Remove(number);
                    }
                    catch { }
                });
            }

            if (vehiclesOut.ContainsKey(number))
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (VehicleManager.Vehicles.ContainsKey(number))
                        {
                            VehicleManager.Vehicles[number].Items = NAPI.Data.GetEntityData(vehiclesOut[number], "ITEMS");
                            var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[number].Model));
                            VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntitySharedData(vehiclesOut[number], "PETROL")) ? VehicleManager.VehicleTank[vclass] : vehiclesOut[number].GetSharedData<int>("PETROL");
                        }
                        NAPI.Entity.DeleteEntity(vehiclesOut[number]);
                        vehiclesOut.Remove(number);
                    }
                    catch { }
                });
                if (resetPosition) VehicleManager.Vehicles[number].Position = null;
            }
        }
        public void Create()
        {
            MySQL.Query($"INSERT INTO `garages`(`id`,`type`,`position`,`rotation`) VALUES ({ID},{Type},'{JsonConvert.SerializeObject(Position)}','{JsonConvert.SerializeObject(Rotation)}')");
        }
        public void Save()
        {
            //MySQL.Query($"UPDATE `garages` SET `data`='{JsonConvert.SerializeObject(this)}' WHERE `id`='{ID}'");
        }
        public void Destroy()
        {
            shape.Delete();
            intShape.Delete();
            intMarker.Delete();
        }
        public void SpawnCar(string number)
        {
            if (entityVehicles.ContainsKey(number)) return;
            int i = 0;
            for (i = 0; i < 10; i++)
            {
                if (entityVehicles.Values.FirstOrDefault(t => t.Item1 == i) == null)
                    break;
            }

            if (i >= GarageManager.GarageTypes[Type].VehiclesPositions.Count) return;

            var vehData = VehicleManager.Vehicles[number];
            if (vehData.Health < 1) return;
            //            var car = NAPI.Util.GetHashKey(vehData.Model);
            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), GarageManager.GarageTypes[Type].VehiclesPositions[i] + new Vector3(0, 0, 0.25), GarageManager.GarageTypes[Type].VehiclesRotations[i], 0, 0);
            veh.NumberPlate = number;
            NAPI.Entity.SetEntityDimension(veh, (uint)Dimension);
            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            veh.SetData("ACCESS", "GARAGE");
            veh.SetData("ITEMS", vehData.Items);
            veh.SetSharedData("PETROL", vehData.Fuel);
            VehicleManager.ApplyCustomization(veh);
            entityVehicles.Add(number, new Tuple<int, Entity>(i, veh));
        }
        public void SpawnCars(List<string> vehicles)
        {
            int i = 0;
            foreach (var number in vehicles)
            {
                if (i >= GarageManager.GarageTypes[Type].VehiclesPositions.Count) continue;
                var vehData = VehicleManager.Vehicles[number];
                if (vehData.Health < 1) continue;
                var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), GarageManager.GarageTypes[Type].VehiclesPositions[i] + new Vector3(0, 0, 0.25), GarageManager.GarageTypes[Type].VehiclesRotations[i], 0, 0);
                veh.NumberPlate = number;
                NAPI.Entity.SetEntityDimension(veh, (uint)Dimension);
                VehicleStreaming.SetEngineState(veh, false);
                VehicleStreaming.SetLockStatus(veh, true);
                veh.SetData("ACCESS", "GARAGE");
                veh.SetData("ITEMS", vehData.Items);
                veh.SetSharedData("PETROL", vehData.Fuel);
                VehicleManager.ApplyCustomization(veh);
                entityVehicles.Add(number, new Tuple<int, Entity>(i, veh));
                i++;
            }
        }
        public void DestroyCars()
        {
            Log.Write("Destroy cars");
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (var veh in entityVehicles)
                    {
                        VehicleManager.Vehicles[veh.Key].Items = NAPI.Data.GetEntityData(veh.Value.Item2, "ITEMS");
                        NAPI.Entity.DeleteEntity(veh.Value.Item2);
                    }
                    entityVehicles = new Dictionary<string, Tuple<int, Entity>>();

                    foreach (var veh in vehiclesOut)
                    {
                        VehicleManager.Vehicles[veh.Key].Items = NAPI.Data.GetEntityData(veh.Value, "ITEMS");
                        NAPI.Entity.DeleteEntity(veh.Value);
                        VehicleManager.Vehicles[veh.Key].Position = null;
                    }
                    vehiclesOut = new Dictionary<string, Entity>();
                }
                catch { }
            });
        }
        public void RespawnCars()
        {
            try
            {
                List<string> vehicles = entityVehicles.Keys.ToList();

                /*foreach (var veh in entityVehicles)
                    NAPI.Entity.DeleteEntity(veh.Value.Item2);
                entityVehicles = new Dictionary<string, Tuple<int, NetHandle>>();*/

                foreach (var v in NAPI.Pools.GetAllVehicles())
                {
                    if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "GARAGE" && vehicles.Contains(v.NumberPlate))
                    {
                        if (VehicleManager.Vehicles.ContainsKey(v.NumberPlate) && v.HasData("ITEMS"))
                            VehicleManager.Vehicles[v.NumberPlate].Items = v.GetData<List<nItem>>("ITEMS");
                        v.Delete();
                    }
                }
                entityVehicles.Clear();

                SpawnCars(vehicles);
            }
            catch { }
        }
        public void SpawnCarAtPosition(Player player, string number, Vector3 position, Vector3 rotation)
        {
            if (vehiclesOut.ContainsKey(number))
            {
                Main.Players[player].LastVeh = "";
                return;
            }

            var vData = VehicleManager.Vehicles[number];
            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vData.Model), position, rotation, 0, 0, number);
            vehiclesOut.Add(number, veh);

            veh.SetSharedData("PETROL", vData.Fuel);
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("OWNER", player);
            veh.SetData("ITEMS", vData.Items);

            //VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(Position + new Vector3(0, 0, 0.3));
            //VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(Rotation);
            //Main.Players[player].LastVeh = number;

            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);

            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);

            VehicleManager.ApplyCustomization(veh);
        }
        public void GetVehicleFromGarage(Player player, string number)
        {
            var vData = VehicleManager.Vehicles[number];
            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vData.Model), player.Position + new Vector3(0, 0, 0.3), Rotation, 0, 0, number);
            player.SetIntoVehicle(veh, 0);
            vehiclesOut.Add(number, veh);
            veh.SetSharedData("PETROL", vData.Fuel);
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("OWNER", player);
            veh.SetData("ITEMS", vData.Items);
            veh.Position = Position;
            

            //VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(Position + new Vector3(0, 0, 0.3));
            //VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(Rotation);
            //Main.Players[player].LastVeh = number;

            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);

            if (Type == -1)
            {
                VehicleStreaming.SetEngineState(veh, false);
                VehicleStreaming.SetLockStatus(veh, true);
            }
            else
            {
                if (vData.Fuel > 0)
                    VehicleStreaming.SetEngineState(veh, true);
                else
                    VehicleStreaming.SetEngineState(veh, false);
            }

            if (Type != -1)
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        NAPI.Entity.DeleteEntity(entityVehicles[number].Item2);
                        entityVehicles.Remove(number);
                    }
                    catch { }
                });
            }

            VehicleManager.ApplyCustomization(veh);
        }
        public void SendVehicleIntoGarage(string number)
        {
            vehiclesOut.Remove(number);
            VehicleManager.Vehicles[number].Position = null;
            if (Type != -1) SpawnCar(number);
        }
        public void SendPlayer(Player player)
        {
            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(Dimension));
            NAPI.Entity.SetEntityPosition(player, GarageManager.GarageTypes[Type].Position);
            Main.Players[player].InsideGarageID = ID;
            //Костыль
            RespawnCars();
        }
        public void RemovePlayer(Player player)
        {
            NAPI.Entity.SetEntityDimension(player, 0);
            NAPI.Entity.SetEntityPosition(player, Position);
            Main.Players[player].InsideGarageID = -1;
        }
        public void SendAllVehiclesToGarage()
        {
            try
            {
                var toSend = new List<string>();
                foreach (var v in vehiclesOut)
                {
                    toSend.Add(v.Key);
                    VehicleManager.Vehicles[v.Key].Items = NAPI.Data.GetEntityData(v.Value, "ITEMS");
                    var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[v.Key].Model));
                    VehicleManager.Vehicles[v.Key].Fuel = (!NAPI.Data.HasEntitySharedData(v.Value, "PETROL")) ? VehicleManager.VehicleTank[vclass] : v.Value.GetSharedData<int>("PETROL");
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(v.Value);
                        }
                        catch { }
                    });
                }
                foreach (var v in toSend)
                {
                    SendVehicleIntoGarage(v);
                }
            }
            catch { }
        }
        public string SendVehiclesInsteadNearest(List<Player> Roommates, Player player)
        {
            var number = "";
            var nearPlayerVehicles = new List<Vehicle>();
            var toSend = new List<string>();
            foreach (var v in vehiclesOut)
            {
                var veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(v.Value);
                var someNear = false;
                foreach (var p in Roommates)
                {
                    if (p.Position.DistanceTo(veh.Position) < 100)
                    {
                        someNear = true;
                        break;
                    }
                }

                if (!someNear)
                {
                    if (player.Position.DistanceTo(veh.Position) < 300) nearPlayerVehicles.Add(veh);
                    toSend.Add(v.Key);
                }
            }

            Vehicle nearestVehicle = null;
            foreach (var v in nearPlayerVehicles)
            {
                if (nearestVehicle == null)
                {
                    nearestVehicle = v;
                    continue;
                }
                if (player.Position.DistanceTo(v.Position) < nearestVehicle.Position.DistanceTo(v.Position)) nearestVehicle = v;
            }

            if (nearestVehicle != null)
            {
                toSend.Remove(nearestVehicle.NumberPlate);
                number = nearestVehicle.NumberPlate;
                VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(nearestVehicle.Position);
                VehicleManager.Vehicles[number].Rotation = JsonConvert.SerializeObject(nearestVehicle.Rotation);
                VehicleManager.Save(number);
                NAPI.Util.ConsoleOutput("delete " + number);
                DeleteCar(number, false);
            }

            try
            {
                foreach (var v in toSend)
                {
                    if (vehiclesOut.ContainsKey(v))
                    {
                        VehicleManager.Vehicles[v].Items = NAPI.Data.GetEntityData(vehiclesOut[v], "ITEMS");
                        var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[v].Model));
                        VehicleManager.Vehicles[v].Fuel = (!NAPI.Data.HasEntitySharedData(vehiclesOut[v], "PETROL")) ? VehicleManager.VehicleTank[vclass] : vehiclesOut[v].GetSharedData<int>("PETROL");
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                NAPI.Entity.DeleteEntity(vehiclesOut[v]);
                            }
                            catch { };
                            SendVehicleIntoGarage(v);
                        });
                    }
                }
            }
            catch (Exception e) { Log.Write($"SendVehiclesInsteadNearest: " + e.Message, nLog.Type.Error); }

            return number;
        }
        public void CreateInterior()
        {
            #region Creating Interior ColShape
            intMarker = NAPI.Marker.CreateMarker(1, GarageManager.GarageTypes[Type].Position - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220), false, (uint)Dimension);

            intShape = NAPI.ColShape.CreateCylinderColShape(GarageManager.GarageTypes[Type].Position - new Vector3(0, 0, 1.12), 1f, 4f, (uint)Dimension);
            intShape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 41);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityEnterColShape: " + ex.Message); }
            };
            intShape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityExitColShape: " + ex.Message); }
            };
            #endregion
        }
    }
    #endregion

    class GarageManager : Script
    {
        private static nLog Log = new nLog("Garages");
        public static Dictionary<int, Garage> Garages = new Dictionary<int, Garage>();
        public static Dictionary<int, GarageType> GarageTypes = new Dictionary<int, GarageType>()
        {
            { -1, new GarageType(new Vector3(), new List<Vector3>(), new List<Vector3>(), 1) },
            { 0, new GarageType(new Vector3(178.9925, -1005.661, -98.9995),
                new List<Vector3>(){
                    new Vector3(170.6935, -1004.269, -99.41191),
                    new Vector3(174.3777, -1003.795, -99.41129),
                },
                new List<Vector3>(){
                    new Vector3(-0.1147747, 0.02747092, 183.3471),
                    new Vector3(-0.1562817, 0.01328733, 175.7529),
                }, 2)},
            { 1, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                },
                new List<Vector3>(){
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                }, 3)},
            { 2, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                    new Vector3(201.9032, -1004.244, -99.41065),
                },
                new List<Vector3>(){
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                    new Vector3(-0.1150091, -0.03728109, 163.4917),
                }, 4)},
            { 3, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(204.1544, -997.7147, -99.41058),
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                    new Vector3(201.9032, -1004.244, -99.41065),
                },
                new List<Vector3>(){
                    new Vector3(-0.115809, -0.04190827, 166.4086),
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                    new Vector3(-0.1150091, -0.03728109, 163.4917),
                }, 5)},
            { 4, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(204.1544, -997.7147, -99.41058),
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                    new Vector3(201.9032, -1004.244, -99.41065),
                    new Vector3(196.0699, -1003.287, -99.41054),
                },
                new List<Vector3>(){
                    new Vector3(-0.115809, -0.04190827, 166.4086),
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                    new Vector3(-0.1150091, -0.03728109, 163.4917),
                    new Vector3(-0.1143998, -0.02649088, 161.4624),
                }, 6)},
            { 5, new GarageType(new Vector3(240.411, -1004.753, -100),
                new List<Vector3>(){
                    new Vector3(223.2661, -978.6877, -99.41358),
                    new Vector3(223.1918, -982.4593, -99.41795),
                    new Vector3(222.8921, -985.879, -99.41821),
                    new Vector3(222.8588, -989.4495, -99.41826),
                    new Vector3(223.0551, -993.4521, -99.41066),
                    new Vector3(233.6587, -983.3923, -99.41045),
                    new Vector3(234.0298, -987.5615, -99.41094),
                    new Vector3(234.0298, -991.406, -99.4104),
                    new Vector3(234.2386, -995.7032, -99.41273),
                    new Vector3(234.3856, -999.8402, -99.41091),
                },
                new List<Vector3>(){
                    new Vector3(-0.03247262, -0.08614436, 251.3986),
                    new Vector3(-0.8253403, 0.03646085, 246.0103),
                    new Vector3(-0.8608215, 0.004363943, 251.0875),
                    new Vector3(-0.8236036, 0.02502611, 248.026),
                    new Vector3(-0.1083736, -0.1425103, 240.252),
                    new Vector3(-0.1053052, 0.02684846, 130.5622),
                    new Vector3(-0.09362753, 0.1056001, 130.4442),
                    new Vector3(-0.09778301, 0.03327406, 129.4973),
                    new Vector3(-0.05343597, 0.06972831, 129.157),
                    new Vector3(-0.08984898, 0.1096697, 128.8663),
                }, 10)},
            { 6, new GarageType(new Vector3(1295.355, 260.6555, -49.46),
                new List<Vector3>(){
                    new Vector3(1280.995, 259.496, -48.97149),
                    new Vector3(1281.109, 255.9245, -48.97091),
                    new Vector3(1281.4, 251.907, -48.97081),
                    new Vector3(1281.126, 247.6722, -48.97099),
                    new Vector3(1281.33, 243.5541, -48.97166),
                    new Vector3(1281.638, 239.8396, -48.9708),
                    new Vector3(1294.898, 229.459, -48.9704),
                    new Vector3(1294.515, 233.4026, -48.97046),
                    new Vector3(1294.626, 239.1256, -48.97104),
                    new Vector3(1294.728, 243.0791, -48.97101),
                    new Vector3(1294.863, 247.5295, -48.97121),
                    new Vector3(1294.693, 251.6595, -48.97055),
                    new Vector3(1309.211, 259.4242, -48.97141),
                    new Vector3(1309.496, 256.3828, -48.97001),
                    new Vector3(1309.558, 252.1761, -48.97118),
                    new Vector3(1309.356, 247.8976, -48.97121),
                    new Vector3(1308.805, 243.1659, -48.97168),
                    new Vector3(1309.158, 239.0858, -48.97101),
                    new Vector3(1308.872, 234.6179, -48.97129),
                    new Vector3(1309.587, 229.2085, -48.97048),
                },
                new List<Vector3>(){
                    new Vector3(-0.03491997, -0.07273021, 271.92),
                    new Vector3(-0.01059541, -0.1097583, 271.202),
                    new Vector3(-0.01303124, -0.03560259, 271.1667),
                    new Vector3(0.006514099, -0.1265539, 269.6285),
                    new Vector3(0.007811933, -0.02255676, 273.0717),
                    new Vector3(-0.007202253, 0.003094224, 271.3499),
                    new Vector3(-0.02166085, 0.02071949, 267.1729),
                    new Vector3(0.004852382, -0.009041164, 269.9042),
                    new Vector3(-0.005163306, -0.04093655, 272.9355),
                    new Vector3(-0.0155124, -0.01274915, 273.4317),
                    new Vector3(0.01824737, -0.05399249, 272.2822),
                    new Vector3(-0.1675545, -0.09296297, 270.5526),
                    new Vector3(0.01114236, 0.02137799, 88.72433),
                    new Vector3(0.0760905, -0.08089431, 92.3721),
                    new Vector3(-0.0176492, -0.08804065, 90.03134),
                    new Vector3(0.03663738, 0.07466815, 91.80698),
                    new Vector3(-0.005276262, 0.02197214, 93.12665),
                    new Vector3(-0.01455327, 0.00984211, 91.07117),
                    new Vector3(0.01212086, 0.1076892, 92.03104),
                    new Vector3(0.04914879, 0.1466752, 90.88959),
                }, 20)},
            { 7, new GarageType(new Vector3(-198.54628, -580.65814, 135.5042),
                new List<Vector3>(){
                    new Vector3(-173.19012, -586.52423, 136.29448),
                    new Vector3(-171.78719, -582.7581, 136.2919),
                    new Vector3(-172.0465, -578.8194, 136.29202),
                    new Vector3(-173.76663, -575.13947, 136.29185),
                    new Vector3(-177.16019, -572.1809, 136.29193),
                    new Vector3(-181.69995, -571.1095, 136.29414),
                    new Vector3(-185.49144, -572.10034, 136.29204),
                    new Vector3(-189.89793, -573.48645, 136.29207),
                    new Vector3(-193.61362, -574.27716, 136.29193),
                    new Vector3(-194.50594, -578.3168, 136.29442),

                    new Vector3(-173.19012, -586.52423, 141.63705),
                    new Vector3(-171.78719, -582.7581, 141.63705),
                    new Vector3(-172.0465, -578.8194, 141.63705),
                    new Vector3(-173.76663, -575.13947, 141.63705),
                    new Vector3(-177.16019, -572.1809, 141.63705),
                    new Vector3(-181.69995, -571.1095, 141.63705),
                    new Vector3(-185.49144, -572.10034, 141.63705),
                    new Vector3(-189.89793, -573.48645, 141.63705),
                    new Vector3(-193.61362, -574.27716, 141.63705),
                    new Vector3(-194.50594, -578.3168, 141.63705),

                    new Vector3(-173.19012, -586.52423, 146.98476),
                    new Vector3(-171.78719, -582.7581, 146.98476),
                    new Vector3(-172.0465, -578.8194, 146.98476),
                    new Vector3(-173.76663, -575.13947, 146.98476),
                    new Vector3(-177.16019, -572.1809, 146.98476),
                    new Vector3(-181.69995, -571.1095, 146.98476),
                    new Vector3(-185.49144, -572.10034, 146.98476),
                    new Vector3(-189.89793, -573.48645, 146.98476),
                    new Vector3(-193.61362, -574.27716, 146.98476),
                    new Vector3(-194.50594, -578.3168, 146.98476),
                },
                new List<Vector3>(){
                    new Vector3(-0.23124333, 0.019113284, 54.84348),
                    new Vector3(-0.26069018, -0.009328686, 82.95082),
                    new Vector3(-0.2604094, -0.015201714, 105.11927),
                    new Vector3(-0.2608416, -0.013833907, 128.66927),
                    new Vector3(-0.2610992, -0.01196395, 153.99701),
                    new Vector3(-0.20135225, 0.16230443, -171.52605),
                    new Vector3(-0.26052204, -0.012673302, -158.91222),
                    new Vector3(-0.2606374, -0.013312892, -126.2141),
                    new Vector3(-0.2604114, -0.011865197, -123.397415),
                    new Vector3(-0.23228909, 0.02026361, -105.81482),

                    new Vector3(-0.23124333, 0.019113284, 54.84348),
                    new Vector3(-0.26069018, -0.009328686, 82.95082),
                    new Vector3(-0.2604094, -0.015201714, 105.11927),
                    new Vector3(-0.2608416, -0.013833907, 128.66927),
                    new Vector3(-0.2610992, -0.01196395, 153.99701),
                    new Vector3(-0.20135225, 0.16230443, -171.52605),
                    new Vector3(-0.26052204, -0.012673302, -158.91222),
                    new Vector3(-0.2606374, -0.013312892, -126.2141),
                    new Vector3(-0.2604114, -0.011865197, -123.397415),
                    new Vector3(-0.23228909, 0.02026361, -105.81482),

                    new Vector3(-0.23124333, 0.019113284, 54.84348),
                    new Vector3(-0.26069018, -0.009328686, 82.95082),
                    new Vector3(-0.2604094, -0.015201714, 105.11927),
                    new Vector3(-0.2608416, -0.013833907, 128.66927),
                    new Vector3(-0.2610992, -0.01196395, 153.99701),
                    new Vector3(-0.20135225, 0.16230443, -171.52605),
                    new Vector3(-0.26052204, -0.012673302, -158.91222),
                    new Vector3(-0.2606374, -0.013312892, -126.2141),
                    new Vector3(-0.2604114, -0.011865197, -123.397415),
                    new Vector3(-0.23228909, 0.02026361, -105.81482),
                }, 30)},
        };
        public static int DimensionID = 1000;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM `garages`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    /*Garage garage = JsonConvert.DeserializeObject<Garage>(Row["data"].ToString());
                    garage.Dimension = DimensionID;
                    if (garage.Type != -1) garage.CreateInterior();

                    Garages.Add(Convert.ToInt32(Row["id"]), garage);
                    DimensionID++;

                    MySQL.Query($"UPDATE `garages` SET type={garage.Type}, position='{JsonConvert.SerializeObject(garage.Position)}', rotation='{JsonConvert.SerializeObject(garage.Rotation)}' WHERE id={garage.ID}");*/

                    var id = Convert.ToInt32(Row["id"]);
                    var type = Convert.ToInt32(Row["type"]);
                    var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                    var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());

                    var garage = new Garage(id, type, position, rotation);
                    garage.Dimension = DimensionID;
                    if (garage.Type != -1) garage.CreateInterior();

                    Garages.Add(id, garage);
                    DimensionID++;
                }
                Log.Write($"Loaded {Garages.Count} garages.", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write($"ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void spawnCarsInGarage()
        {
            Log.Write($"Loading garage cars.", nLog.Type.Info);
            var count = 0;
            lock (Garages)
            {
                foreach (var garage in Garages)
                {
                    try
                    {
                        if (garage.Value.Type == -1) continue;
                        var house = HouseManager.Houses.Find(h => h.GarageID == garage.Key);
                        if (house == null) continue;
                        if (string.IsNullOrEmpty(house.Owner)) continue;

                        var vehicles = VehicleManager.getAllPlayerVehicles(house.Owner);
                        vehicles.RemoveAll(v => !string.IsNullOrEmpty(VehicleManager.Vehicles[v].Position));
                        garage.Value.SpawnCars(vehicles);

                        count += vehicles.Count;
                    }
                    catch (Exception e) { Log.Write($"garage load vehicles {e.ToString()}", nLog.Type.Error); }
                }
            }
            Log.Write($"{count} vehicles were spawned in garages.", nLog.Type.Success);
        }

        public static void interactionPressed(Player player, int id)
        {
            try
            {
                switch (id)
                {
                    case 40:
                        if (!player.HasData("GARAGEID") || Houses.HouseManager.GetHouse(player) == null) return;
                        Garage garage = Garages[player.GetData<int>("GARAGEID")];
                        if (garage == null) return;
                        var house = HouseManager.GetHouse(player);
                        if (house == null || house.GarageID != garage.ID) return;

                        var vehicles = VehicleManager.getAllPlayerVehicles(house.Owner);
                        if (player.IsInVehicle && !vehicles.Contains(player.Vehicle.NumberPlate))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Du kannst mit diesem Auto nicht in die Garage fahren", 3000);
                            return;
                        }
                        else if (player.IsInVehicle && vehicles.Contains(player.Vehicle.NumberPlate))
                        {
                            var vehicle = player.Vehicle;
                            var number = vehicle.NumberPlate;
                            VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntitySharedData(player.Vehicle, "PETROL")) ? VehicleManager.VehicleTank[player.Vehicle.Class] : player.Vehicle.GetSharedData<int>("PETROL");
                            VehicleManager.Vehicles[number].Items = player.Vehicle.GetData<List<nItem>>("ITEMS");
                            VehicleManager.Vehicles[number].Position = null;
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(vehicle); } catch { } });

                            garage.SendVehicleIntoGarage(number);
                        }

                        if (garage.Type == -1)
                        {
                            if (vehicles.Count == 0) return;
                            if (garage.CheckCar(false, vehicles[0]))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ihr Auto ist jetzt irgendwo im Staat, Sie können evakuieren", 3000);
                                return;
                            }
                            if (player.IsInVehicle) return;

                            if (VehicleManager.Vehicles[vehicles[0]].Health < 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Sie müssen das Auto wiederherstellen", 3000);
                                return;
                            }
                            garage.GetVehicleFromGarage(player, vehicles[0]);
                        }
                        else
                        {
                            garage.SendPlayer(player);
                        }
                        return;
                    case 41:
                        if (Main.Players[player].InsideGarageID == -1) return;
                        garage = Garages[Main.Players[player].InsideGarageID];
                        garage.RemovePlayer(player);
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"GARAGE_INTERACTION\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void Event_PlayerDisconnected(Player player)
        {

        }

        #region Commands
        [Command("setgarage")]
        public static void CMD_SetGarage(Player player, int ID)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Sie müssen auf der Hausmarkierung stehen", 3000);
                return;
            }

            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            if (!Garages.ContainsKey(ID)) return;
            house.GarageID = ID;
            house.Save();
        }

        [Command("creategarage")]
        public static void CMD_CreateGarage(Player player, int type)
        {
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны находится в автомобиле!", 3000);
                return;
            }

            if (!Group.CanUseCmd(player, "allspawncar")) return;
            if (!GarageTypes.ContainsKey(type)) return;
            int id = 0;
            do
            {
                id++;

            } while (Garages.ContainsKey(id));

            Garage garage = new Garage(id, type, player.Vehicle.Position, player.Vehicle.Rotation);
            garage.Dimension = DimensionID;
            garage.Create();
            if (type != -1) garage.CreateInterior();

            Garages.Add(garage.ID, garage);
            NAPI.Chat.SendChatMessageToPlayer(player, garage.ID.ToString());
        }

        [Command("removegarage")]
        public static void CMD_RemoveGarage(Player player)
        {
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            if (!player.HasData("GARAGEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находится на метке гаража!", 3000);
                return;
            }
            if (!GarageManager.Garages.ContainsKey(player.GetData<int>("GARAGEID"))) return;
            Garage garage = GarageManager.Garages[player.GetData<int>("GARAGEID")];

            garage.Destroy();
            Garages.Remove(player.GetData<int>("GARAGEID"));
            MySQL.Query($"DELETE FROM `garages` WHERE `id`='{garage.ID}'");
        }

        #endregion
    }
}
