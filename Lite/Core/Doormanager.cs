using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using LiteSDK;

namespace Lite.Core
{
    class Doormanager : Script
    {
        private static nLog Log = new nLog("Doormanager");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                RegisterDoor(961976194, new Vector3(255.2283, 223.976, 102.3932)); // safe door 
                SetDoorLocked(0, true, 0);

                RegisterDoor(110411286, new Vector3(232.6054, 214.1584, 106.4049)); // pacific standart main door 1
                SetDoorLocked(1, true, 0);

                RegisterDoor(110411286, new Vector3(231.5123, 216.5177, 106.4049)); // pacific standart main door 2
                SetDoorLocked(2, true, 0);

                RegisterDoor(631614199, new Vector3(461.8065, -997.6583, 25.06443)); // police prison door
                SetDoorLocked(3, true, 0);

                RegisterDoor(-1663022887, new Vector3(150.8389, -1008.352, -98.85)); // hotel
                SetDoorLocked(4, true, 0);

                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("tr_prop_tr_gate_r_01a"), new Vector3(-2148.653, 1110.646, -23.5492), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("tr_prop_tr_gate_l_01a"), new Vector3(-2148.653, 1101.464, -23.5492), 30f);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static List<Door> allDoors = new List<Door>();
        public static int RegisterDoor(int model, Vector3 Position)
        {
            allDoors.Add(new Door(model, Position));
            var col = NAPI.ColShape.CreateCylinderColShape(Position, 5, 5, 0);
            col.SetData("DoorID", allDoors.Count - 1);
            col.OnEntityEnterColShape += Door_onEntityEnterColShape;
            return allDoors.Count - 1;
        }

        private static void Door_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) != EntityType.Player) return;
                var door = allDoors[shape.GetData<int>("DoorID")];
                Trigger.ClientEvent(entity, "setDoorLocked", door.Model, door.Position.X, door.Position.Y, door.Position.Z, door.Locked, door.Angle);
            }
            catch (Exception e) { Log.Write("Door_onEntityEnterColshape: " + e.ToString(), nLog.Type.Error); }
        }

        public static void SetDoorLocked(int id, bool locked, float angle)
        {
            if (allDoors.Count < id + 1) return;
            allDoors[id].Locked = locked;
            allDoors[id].Angle = angle;
            Main.ClientEventToAll("setDoorLocked", allDoors[id].Model, allDoors[id].Position.X, allDoors[id].Position.Y, allDoors[id].Position.Z, allDoors[id].Locked, allDoors[id].Angle);
        }

        public static bool GetDoorLocked(int id)
        {
            if (allDoors.Count < id + 1) return false;
            return allDoors[id].Locked;
        }

        internal class Door
        {
            public Door(int model, Vector3 position)
            {
                Model = model;
                Position = position;
                Locked = false;
                Angle = 50.0f;
            }

            public int Model { get; set; }
            public Vector3 Position { get; set; }
            public bool Locked { get; set; }
            public float Angle { get; set; }
        }
    }
}
