using HarmonyLib;
using Player;
using System;

namespace WeaponRandomizer.WeaponRandomizer
{
    public static class ModIdentifier
    {
        public static string ModdingName { get; set; } = "";
    }

    /// <summary>
    /// Patch to apply custom nickname locally
    /// </summary>
    [HarmonyPatch(typeof(PlayerReplicationManager), "OnSpawn",
        new Type[] { typeof(pPlayerSpawnData), typeof(PlayerReplicator) })]
    public class PlayerManager_OnPlayerSpawned
    {
        [HarmonyPrefix]
        public static void Prefix(pPlayerSpawnData spawnData, PlayerReplicator replicator)
        {
            if (replicator.OwningPlayer.IsLocal && ModIdentifier.ModdingName.StartsWith("[Modded]"))
            {
                replicator.OwningPlayer.NickName = ModIdentifier.ModdingName;
            }
        }
    }
}
