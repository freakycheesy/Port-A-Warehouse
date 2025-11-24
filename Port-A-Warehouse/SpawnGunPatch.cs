using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;

namespace Port_A_Warehouse {
    public static class SpawnGunPatch {

        public static void SwapSpawnGunCrate(SpawnableCrate crate) {
            foreach (var instance in UnityEngine.Object.FindObjectsOfType<SpawnGun>()) {
                instance._lastSelectedCrate = crate;
                instance._selectedCrate = crate;
            }
        }
    }
}
