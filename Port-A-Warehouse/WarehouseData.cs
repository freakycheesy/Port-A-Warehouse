using BoneLib.BoneMenu;
using BoneLib.Notifications;
using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using MelonLoader;
namespace Port_A_Warehouse {
    public static class WarehouseData {
        public static List<Crate> Crates = new();
        public static Action OnCratesGenerated;
        public static Task Task {
            get; private set;
        }
        public static async void GenerateCratesData() {
            if(Task != null) if (Task.Status == TaskStatus.Running) return;
            try {              
                Task = Task.Factory.StartNew(GenerateCrateDataTask);
                await Task;
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

                Crates.Clear();

                Crates = Core.GetCleanList(AssetWarehouse.Instance.GetCrates());
                Crates.RemoveAll(x => x.Redacted && !Core.ShowRedacted);
                Crates.RemoveAll(x => x.Pallet.Internal && !Core.ShowInternal);
                Crates.RemoveAll(x => x.Unlockable && !Core.ShowUnlockable);
                if (!Core.SearchQuery.IsNullOrEmpty()) {
                    var comparison = Core.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                    Predicate<Crate> match = x => x._barcode._id.Contains(Core.SearchQuery, comparison) && Core.IncludeBarcodes || x._tags.Contains(Core.SearchQuery) && Core.IncludeTags || x._title.Contains(Core.SearchQuery, comparison) && Core.IncludeTitles || x.Pallet.Author.Contains(Core.SearchQuery, comparison) && Core.IncludeAuthors;
                    Crates = Crates.ToList().FindAll(match);
                }
                MelonLogger.Msg("Done Loading");
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
