using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using LiteSDK;
using System.Linq;

namespace Lite.Core
{
    class BasicSync : Script
    {
        private static nLog Log = new nLog("BasicSync");

        public static void AttachLabelToObject(string text, Vector3 posOffset, NetHandle obj)
        {
            var attachedLabel = new AttachedLabel(text, posOffset);
            switch (obj.Type)
            {
                case EntityType.Player:
                    var player = NAPI.Entity.GetEntityFromHandle<Player>(obj);
                    player.SetSharedData("attachedLabel", JsonConvert.SerializeObject(attachedLabel));
                    Trigger.ClientEventInRange(player.Position, 550, "attachLabel", player);
                    break;
                case EntityType.Vehicle:
                    var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(obj);
                    vehicle.SetSharedData("attachedLabel", JsonConvert.SerializeObject(attachedLabel));
                    Trigger.ClientEventInRange(vehicle.Position, 550, "attachLabel", vehicle);
                    break;
            }
        }

        public static void DetachLabel(NetHandle obj)
        {
            switch (obj.Type)
            {
                case EntityType.Player:
                    var player = NAPI.Entity.GetEntityFromHandle<Player>(obj);
                    player.ResetSharedData("attachedLabel");
                    Trigger.ClientEventInRange(player.Position, 550, "detachLabel");
                    break;
                case EntityType.Vehicle:
                    var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(obj);
                    vehicle.ResetSharedData("attachedLabel");
                    Trigger.ClientEventInRange(vehicle.Position, 550, "detachLabel");
                    break;
            }
        }

        public static void AttachObjectToPlayer(Player player, uint model, int bone, Vector3 posOffset, Vector3 rotOffset)
        {
            var attObj = new AttachedObject(model, bone, posOffset, rotOffset);
            player.SetSharedData("attachedObject", JsonConvert.SerializeObject(attObj));
            Trigger.ClientEventInRange(player.Position, 550, "attachObject", player);
        }

        public static void DetachObject(Player player)
        {
            player.ResetSharedData("attachedObject");
            Trigger.ClientEventInRange(player.Position, 550, "detachObject", player);
        }

        private static string SerializeAttachments(List<uint> attachments)
        {
            return string.Join('|', attachments.Select(hash => hash.ToString("X")));
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            player.SetData("ATTACHMENTS", new List<uint>());
        }
        [RemoteEvent("staticAttachments.Add")]
        public static void StaticAttachmentsAdd(Player player, uint model, int bone, Vector3 posOffset, Vector3 rotOffset, nItem item)
        {
            if (FastSlots.Carabine.Contains(item.Type))
            {
                var attObj = new AttachedObject(model, bone, posOffset, rotOffset);
                player.SetSharedData("assut", JsonConvert.SerializeObject(attObj));
                player.SetData("assuts", true);
                Trigger.ClientEventInRange(player.Position, 550, "attachWeaponCarab", player);
            }
            if (FastSlots.Shot.Contains(item.Type))
            {
                var attObj = new AttachedObject(model, bone, posOffset, rotOffset);
                player.SetSharedData("shot", JsonConvert.SerializeObject(attObj));
                player.SetData("shots", true);
                Trigger.ClientEventInRange(player.Position, 550, "attachWeaponShot", player);
            }
            if (FastSlots.SMG.Contains(item.Type))
            {
                var attObj = new AttachedObject(model, bone, posOffset, rotOffset);
                player.SetSharedData("smg", JsonConvert.SerializeObject(attObj));
                player.SetData("smgs", true);
                Trigger.ClientEventInRange(player.Position, 550, "attachWeaponSMG", player);
            }
            if (FastSlots.Pistol.Contains(item.Type))
            {
                var attObj = new AttachedObject(model, bone, posOffset, rotOffset);
                player.SetSharedData("pistol", JsonConvert.SerializeObject(attObj));
                player.SetData("pistols", true);
                Trigger.ClientEventInRange(player.Position, 550, "attachWeaponPistol", player);
            }

        }

        [RemoteEvent("staticAttachments.Remove")]
        public static void StaticAttachmentsRemove(Player player, uint model, nItem item)
        {
            if (FastSlots.Carabine.Contains(item.Type))
            {
                player.ResetSharedData("assut");
                player.SetData("assuts", false);
                Trigger.ClientEventInRange(player.Position, 550, "detachWeapCarabin", player);
            }
            if (FastSlots.Shot.Contains(item.Type))
            {
                player.ResetSharedData("shot");
                player.SetData("shots", false);
                Trigger.ClientEventInRange(player.Position, 550, "detachWeapShot", player);
            }
            if (FastSlots.SMG.Contains(item.Type))
            {
                player.ResetSharedData("smg");
                player.SetData("smgs", false);
                Trigger.ClientEventInRange(player.Position, 550, "detachWeapSMG", player);
            }
            if (FastSlots.Pistol.Contains(item.Type))
            {
                player.ResetSharedData("pistol");
                player.SetData("pistols", false);
                Trigger.ClientEventInRange(player.Position, 550, "detachWeapPistol", player);
            }

        }
        [RemoteEvent("invisible")]
        public static void SetInvisible(Player player, bool toggle)
        {
            try
            {
                if (Main.Players[player].AdminLVL == 0) return;
                player.SetSharedData("INVISIBLE", toggle);
                Trigger.ClientEventInRange(player.Position, 550, "toggleInvisible", player, toggle);
            }
            catch (Exception e) { Log.Write("InvisibleEvent: " + e.Message, nLog.Type.Error); }
        }

        public static bool GetInvisible(Player player)
        {
            if (!player.HasSharedData("INVISIBLE") || !player.GetSharedData<bool>("INVISIBLE"))
                return false;
            else
                return true;
        }

        internal class PlayAnimData
        {
            public string Dict { get; set; }
            public string Name { get; set; }
            public int Flag { get; set; }

            public PlayAnimData(string dict, string name, int flag)
            {
                Dict = dict;
                Name = name;
                Flag = flag;
            }
        }

        internal class AttachedObject
        {
            public uint Model { get; set; }
            public int Bone { get; set; }
            public Vector3 PosOffset { get; set; }
            public Vector3 RotOffset { get; set; }

            public AttachedObject(uint model, int bone, Vector3 pos, Vector3 rot)
            {
                Model = model;
                Bone = bone;
                PosOffset = pos;
                RotOffset = rot;
            }
        }

        internal class AttachedLabel
        {
            public string Text { get; set; }
            public Vector3 PosOffset { get; set; }

            public AttachedLabel(string text, Vector3 pos)
            {
                Text = text;
                PosOffset = pos;
            }
        }


    }
}
