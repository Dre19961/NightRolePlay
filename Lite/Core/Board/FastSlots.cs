using GTANetworkAPI;
using LiteSDK;
using System;
using System.Collections.Generic;

namespace Lite.Core
{
    public class FastSlots : Script
    {
        public static List<ItemType> IgnoreItems = new List<ItemType>()
        {
            
        };
        public static List<ItemType> Carabine = new List<ItemType>()
        {
            ItemType.AssaultRifle,
            ItemType.CarbineRifle,
            ItemType.AdvancedRifle,
            ItemType.SpecialCarbine,
            ItemType.BullpupRifle,
            ItemType.CompactRifle,
            ItemType.AssaultRifleMk2,
            ItemType.CarbineRifleMk2,
            ItemType.SpecialCarbineMk2,
            ItemType.BullpupRifleMk2,

            ItemType.SniperRifle,
            ItemType.HeavySniper,
            ItemType.MarksmanRifle,
            ItemType.HeavySniperMk2,
            ItemType.MarksmanRifleMk2,
        };
        public static List<ItemType> SMG = new List<ItemType>()
        {
            ItemType.MicroSMG,
            ItemType.MachinePistol,
            ItemType.SMG,
            ItemType.AssaultSMG,
            ItemType.CombatPDW,
            ItemType.MG,
            ItemType.CombatMG,
            ItemType.Gusenberg,
            ItemType.MiniSMG,
            ItemType.SMGMk2,
            ItemType.CombatMGMk2,
        };
        public static List<ItemType> Pistol = new List<ItemType>()
        {
            ItemType.Pistol,
            ItemType.CombatPistol,
            ItemType.Pistol50,
            ItemType.SNSPistol,
            ItemType.HeavyPistol,
            ItemType.VintagePistol,
            ItemType.MarksmanPistol,
            ItemType.Revolver,
            ItemType.APPistol,
            ItemType.FlareGun,
            ItemType.DoubleAction,
            ItemType.PistolMk2,
            ItemType.SNSPistolMk2,
            ItemType.RevolverMk2,
        };
        public static List<ItemType> Shot = new List<ItemType>()
        {
            ItemType.PumpShotgun,
            ItemType.SawnOffShotgun,
            ItemType.BullpupShotgun,
            ItemType.AssaultShotgun,
            ItemType.Musket,
            ItemType.HeavyShotgun,
            ItemType.DoubleBarrelShotgun,
            ItemType.SweeperShotgun,
            ItemType.PumpShotgunMk2,
        };
        [RemoteEvent("server::inventory:setslot")]
        public static void SetItemToSlot(Player player, int index, int slot)
        {
            try
            {
                if (nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot) != null)
                {
                    Notify.Error(player, "Неудалось поставить предмет в быстрый слот, занят");
                    return;
                }
                if (nInventory.Items[Main.Players[player].UUID][index] == null)
                {
                    Notify.Error(player, "Неудалось поставить предмет в быстрый слот");
                    return;
                }
                nItem item = nInventory.Items[Main.Players[player].UUID][index];
                if (item.IsActive)
                {
                    Notify.Info(player, "Сначала уберите предмет из рук");
                    return;
                }

                if (Carabine.Contains(item.Type) && player.GetData<bool>("assuts") == true || Shot.Contains(item.Type) && player.GetData<bool>("shots") == true || SMG.Contains(item.Type) && player.GetData<bool>("smgs") == true || Pistol.Contains(item.Type) && player.GetData<bool>("pistols") == true)
                {
                    Notify.Info(player, "Данный тип оружия уже храниться в фаст слоте");
                    return;
                }
                if (nInventory.ClothesItems.Contains(item.Type) || nInventory.AmmoItems.Contains(item.Type) || IgnoreItems.Contains(item.Type)) return;
                if (Carabine.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[item.Type], 24818, new Vector3(-0.1, -0.15, -0.13), new Vector3(0.0, 0.0, 3.5), item);
                }
                if (Shot.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[item.Type], 24818, new Vector3(-0.1, -0.15, 0.11), new Vector3(-180.0, 0.0, 0.0), item);
                }
                if (SMG.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[item.Type], 58271, new Vector3(0.08, 0.03, -0.1), new Vector3(-80.77, 0.0, 0.0), item);
                }
                if (Pistol.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsAdd(player, nInventory.ItemModels[item.Type], 51826, new Vector3(0.02, 0.06, 0.1), new Vector3(-100.0, 0.0, 0.0), item);
                }
                item.FastSlots = slot;
                GUI.Dashboard.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("RemoveItemFromSlot: " + e.ToString()); }
        }
        [RemoteEvent("server::inventory:removeslot")]
        public static void RemoveItemFromSlot(Player player, int index)
        {
            try
            {
                if (nInventory.Items[Main.Players[player].UUID][index] == null) return;
                nItem item = nInventory.Items[Main.Players[player].UUID][index];
                item.FastSlots = -1;
                if (Carabine.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                }
                if (Shot.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                }
                if (SMG.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                }
                if (Pistol.Contains(item.Type))
                {
                    BasicSync.StaticAttachmentsRemove(player, nInventory.ItemModels[item.Type], item);
                }
                GUI.Dashboard.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("RemoveItemFromSlot: " + e.ToString()); }
        }
        public static void RemoveAllFastSlots(Player player)
        {
            try
            {
                var items = nInventory.Items[Main.Players[player].UUID];
                List<nItem> ItemsRemove = new List<nItem>();
                foreach (nItem item in items)
                {
                    if (item.FastSlots > 0)
                    {
                        ItemsRemove.Add(item);
                        Items.onDrop(player, item, null);
                    }
                }
                foreach(nItem item in ItemsRemove)
                {
                    nInventory.Remove(player, item);
                }
            }
            catch (Exception e) { Console.WriteLine("RemoveAllFastSlots: " + e.ToString()); }
        }
        public static int GetIndex(Player player, nItem item, int slot)
        {
            int i = 0;
            foreach(nItem it in nInventory.Items[Main.Players[player].UUID])
            {
                if (it == nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot))
                    return i;
                i++;
            }
            return -1;
        } 
        [RemoteEvent("server::inventory:useitemslot")]
        public static void UseItemFromSlot(Player player, int slot)
        {
            try
            {
                if (nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot) == null) return;
                nItem item = nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot);
                if (nInventory.ClothesItems.Contains(item.Type) || nInventory.AmmoItems.Contains(item.Type) || IgnoreItems.Contains(item.Type)) return;
                if (item.IsActive)
                {
                    Notify.Info(player, "У вас в руках предмет");
                    return;
                }
                if (player.GetData<bool>("AttachWeapon") == true)
                {
                    Notify.Info(player, "У вас в руках есть активный предмет");
                    return;
                }
                Items.onUse(player, item, GetIndex(player, item, slot));
                GUI.Dashboard.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("UseItemFromSlot: " + e.ToString()); }
        } 
        [RemoteEvent("server::inventory:swap")]
        public static void SwapItemFromSlot(Player player)
        {
            try
            {
                foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                {
                    if (item.IsActive && item.FastSlots > 0)
                    {
                        Items.onUse(player, item, GetIndex(player, item, item.FastSlots));
                    } 
                }
                GUI.Dashboard.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("SwapItemFromSlot: " + e.ToString()); }
        }
    }
}
