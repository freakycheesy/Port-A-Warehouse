using BoneLib.BoneMenu;
using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using MelonLoader;
namespace Port_A_Warehouse {
    public static class WarehouseData {
        public static List<Crate> Crates = new();
        public static Action OnCratesGenerated;
        public static Task Task { get; private set; }
        public static async void GenerateCratesData() {
            Task = Task.Factory.StartNew(GenerateCrateDataTask);
            await Task;
            if (!Task.IsCompletedSuccessfully)
                return;
            OnCratesGenerated?.Invoke();
            MelonLogger.Msg("Generated Crates");
        }

        private static void GenerateCrateDataTask() {
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
        }
    }
}
