using HarmonyLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;

namespace Port_A_Warehouse {
    [HarmonyPatch(typeof(SpawnGun))]
    public class SpawnGunPatch {
        public static List<SpawnGun> Instances = new List<SpawnGun>();

        [HarmonyPatch(nameof(SpawnGun.OnPoolSpawn))]
        [HarmonyPrefix]
        public static void OnSpawnPrefix(SpawnGun __instance) {
            if (Instances.Contains(__instance))
                return;
            Instances.Add(__instance);
        }

        [HarmonyPatch(nameof(SpawnGun.OnPoolDeInitialize))]
        [HarmonyPrefix]
        public static void OnDespawnPrefix(SpawnGun __instance) {
            if (Instances.Contains(__instance))
                return;
            Instances.Remove(__instance);
        }

        public static void SwapSpawnGunCrate(SpawnGun instance, SpawnableCrate crate) {
            instance._lastSelectedCrate = crate;
            instance._selectedCrate = crate;
        }

        public static void SwapGlobalCrate(SpawnableCrate crate) {
            foreach (var instance in Instances) SwapSpawnGunCrate(instance, crate);
        }
    }
}
