using AIGraph;
using GameData;
using Gear;
using HarmonyLib;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaponRandomizer.WeaponRandomizer
{
    /// <summary>
    /// Handles everything what is considered an executer
    /// </summary>
    public class WeaponRandomizer
    {
        private List<int> _exploredZones = new List<int>();
        private int _lastZone = -1;
        private Random _random = new Random();

        public WeaponRandomizer()
        {
        }

        /// <summary>
        /// The Local Inventory you have
        /// </summary>
        public static PlayerInventoryLocal LocalInventory { get; set; }

        /// <summary>
        /// Declaration if the Weapon Randomizer is active
        /// </summary>
        public static bool WeaponRandomizerActive
        {
            get; set;
        }

        /// <summary>
        /// Checks if you enter any new Zone or Re-enter any Zone you already were in
        /// </summary>
        /// <returns>The Action you have to do now</returns>
        public ActionRequired ZoneSwitched()
        {
            var currentZoneId = PlayerManager.GetLocalPlayerAgent()?.CourseNode.m_zone.ID;

            if (currentZoneId != null)
            {
                if (!_exploredZones.Contains((int)currentZoneId))
                {
                    _exploredZones.Add((int)currentZoneId);
                    _lastZone = (int)currentZoneId;
                    return ActionRequired.NewZoneEntered;
                }
                if(currentZoneId != _lastZone )
                {
                    _lastZone = (int)currentZoneId;
                    return ActionRequired.ExploredZoneEntered;
                }
            }
            return ActionRequired.None;
        }

        /// <summary>
        /// The Code which will Randomize your Weapon Layout
        /// </summary>
        public void RandomizeCurrentWeapons()
        {
            GearIDRange[] standardWeapons = GearManager.GetAllGearForSlot(InventorySlot.GearStandard);
            GearIDRange[] specialWeapons = GearManager.GetAllGearForSlot(InventorySlot.GearSpecial);
            PlayerDataBlock playerDataBlock = GameDataBlockBase<PlayerDataBlock>.GetBlock(1U);

            int standardArrayIndex = _random.Next(0, standardWeapons.Length);
            int specialArrayIndex = _random.Next(0, specialWeapons.Length);

            var currentAmmoInStandard = PlayerBackpackManager.LocalBackpack.AmmoStorage.StandardAmmo.AmmoInPack;
            var currentAmmoInSpecial = PlayerBackpackManager.LocalBackpack.AmmoStorage.SpecialAmmo.AmmoInPack;

            PlayerBackpackManager.LocalBackpack.AmmoStorage.SetAmmo(AmmoType.Standard, 0f);
            PlayerBackpackManager.LocalBackpack.AmmoStorage.SetAmmo(AmmoType.Special, 0f);

            PlayerBackpackManager.EquipLocalGear(standardWeapons[standardArrayIndex]);
            LocalInventory.WieldedItem.SetCurrentClip(PlayerBackpackManager.LocalBackpack.AmmoStorage.GetClipBulletsFromPack(LocalInventory.m_wieldedItem.WeaponComp.GetCurrentClip(), LocalInventory.m_wieldedItem.AmmoType));

            PlayerBackpackManager.EquipLocalGear(specialWeapons[specialArrayIndex]);
            LocalInventory.WieldedItem.SetCurrentClip(PlayerBackpackManager.LocalBackpack.AmmoStorage.GetClipBulletsFromPack(LocalInventory.m_wieldedItem.WeaponComp.GetCurrentClip(), LocalInventory.m_wieldedItem.AmmoType));
            
            PlayerBackpackManager.LocalBackpack.AmmoStorage.m_hasMappedCMPageMapInventory = false;
            PlayerBackpackManager.LocalBackpack.AmmoStorage.m_cmPageMapInventory = (PUI_Inventory) null;

            //Hardcoded +10 to recompensate the Fact, that the Magazine Ammunition is somehow Bugged 
            //PlayerBackpackManager.LocalBackpack.AmmoStorage.StandardAmmo.AddAmmo(currentAmmoInStandard + 10);
            //PlayerBackpackManager.LocalBackpack.AmmoStorage.SpecialAmmo.AddAmmo(currentAmmoInSpecial + 10);

            PlayerManager.GetLocalPlayerAgent().GiveAmmoRel(1f, 1f, 0f);

            PlayerBackpackManager.LocalBackpack.AmmoStorage.UpdateAllAmmoUI();
            PlayerBackpackManager.LocalBackpack.AmmoStorage.NeedsSync = true;

            //PlayerBackpackManager.LocalBackpack.AmmoStorage.PickupAmmo(AmmoType.Standard, 40);
            //PlayerBackpackManager.LocalBackpack.AmmoStorage.SetClipAmmoInSlot(InventorySlot.GearStandard);

            //PlayerBackpackManager.LocalBackpack.AmmoStorage.UpdateAmmoInPack(AmmoType.Special, 20);
            //PlayerBackpackManager.LocalBackpack.AmmoStorage.UpdateBulletsInPack(AmmoType.Special, 15);
            //PlayerBackpackManager.LocalBackpack.AmmoStorage.PickupAmmo(AmmoType.Special, 40);
            //PlayerBackpackManager.LocalBackpack.AmmoStorage.SetClipAmmoInSlot(InventorySlot.GearSpecial);

            PlayerBackpackManager.LocalBackpack.AmmoStorage.FillAllClips();
        }

        /// <summary>
        /// Lowers the current Ammo to the absolute minimum with an exception of the ResourcePack
        /// </summary>
        public void NewZoneEntered()
        {
            Console.WriteLine("re-entering Zone");

            //if (PlayerBackpackManager.LocalBackpack.AmmoStorage.StandardAmmo.AmmoInPack > _playerDataBlock.AmmoStandardMaxCap / 5)
            //{
            //    PlayerBackpackManager.LocalBackpack.AmmoStorage.StandardAmmo.AmmoInPack = _playerDataBlock.AmmoStandardMaxCap / 5;
            //    PlayerBackpackManager.LocalBackpack.AmmoStorage.StandardAmmo.BulletClipSize = _standardBulletClipSize;
            //}

            //if(PlayerBackpackManager.LocalBackpack.AmmoStorage.SpecialAmmo.AmmoInPack > _playerDataBlock.AmmoSpecialMaxCap / 5)
            //{
            //    PlayerBackpackManager.LocalBackpack.AmmoStorage.SpecialAmmo.AmmoInPack = _playerDataBlock.AmmoSpecialMaxCap / 5;
            //    PlayerBackpackManager.LocalBackpack.AmmoStorage.SpecialAmmo.BulletClipSize = _specialBulletClipSize;
            //}

            if(PlayerBackpackManager.LocalBackpack.AmmoStorage.ResourcePackAmmo.AmmoInPack > 0.0f)
            {
                PlayerBackpackManager.LocalBackpack.AmmoStorage.ResourcePackAmmo.AmmoInPack = 0.25f;
            }
        }

        public void NewExpeditionStarted()
        {
            _exploredZones = new List<int>();
        }
    }

    /// <summary>
    /// Patch to apply custom nickname locally
    /// </summary>
    [HarmonyPatch(typeof(PlayerInventoryLocal), "GetNextSlot",
        new Type[] { typeof(InventorySlot), typeof(int) })]
    public class PlayerInventoryLocal_GetNextSlot
    {
        [HarmonyPrefix]
        private static void Prefix(InventorySlot current, int dir)
        {
            if(current == InventorySlot.GearStandard && dir == -1)
            {
                dir = 1;
                current = InventorySlot.GearMelee;
            }


            if (current == InventorySlot.HackingTool && dir == 1)
            {
                dir = -1;
                current = InventorySlot.HackingTool;
            }

        }
    }
}
