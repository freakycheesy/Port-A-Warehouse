using BoneLib.BoneMenu;
using BoneLib.Notifications;
using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using MelonLoader;
namespace Port_A_Warehouse {
    public static class WarehouseData {
        public static List<Crate> Crates = new();
        public static List<LevelCrate> LevelCrates = new();
        public static List<SpawnableCrate> SpawnableCrates = new();
        public static List<AvatarCrate> AvatarCrates = new();


        public static Action OnCratesGenerated;
        public static async void GenerateCratesData() {
            try {
                GenerateCrateDataTask();
            }
            catch (Exception ex) {
                MelonLogger.Error("ERROR", ex);
            }
        }

        private static void GenerateCrateDataTask() {
            try {
                Notification notification = new Notification();
                notification.ShowTitleOnPopup = true;
                notification.PopupLength = 0.3f;
                notification.Title = "Port-A-Warehouse";
                notification.Message = "Generating Crates (DON'T ENTER PALLETS, GAME WILL CRASH)";
                Notifier.Send(notification);
                SpawnableCrates = AssetWarehouse.Instance.GetCrates<SpawnableCrate>().ToList().FilterAndCleanCrates();
                AvatarCrates = AssetWarehouse.Instance.GetCrates<AvatarCrate>().ToList().FilterAndCleanCrates();
                LevelCrates = AssetWarehouse.Instance.GetCrates<LevelCrate>().ToList().FilterAndCleanCrates();

                OnCratesGenerated?.Invoke();
                MelonLogger.Msg("Generated Crates");
                notification.Message = "Generated Crates";
                Notifier.Send(notification);
            }
            catch (Exception ex) {
                MelonLogger.Error("ERROR", ex);
            }
        } 
    }
}
