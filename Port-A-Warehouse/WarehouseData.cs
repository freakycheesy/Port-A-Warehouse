using BoneLib.BoneMenu;
using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using MelonLoader;
namespace Port_A_Warehouse {
    public static class WarehouseData {
        public static List<Pallet> Pallets = new();
        public static Action<Pallet> OnPalletGenerated;

        public static async void GeneratePalletData() {
            var task = Task.Factory.StartNew(GeneratePalletDataTask);
        }

        private static void GeneratePalletDataTask() {
            Pallets.Clear();

            Pallets = Core.GetCleanList(AssetWarehouse.Instance.GetPallets());
            Pallets.RemoveAll(x => x.Redacted && !Core.ShowRedacted);
            Pallets.RemoveAll(x => x.Internal && !Core.ShowInternal);
            Pallets.RemoveAll(x => x.Unlockable && !Core.ShowUnlockable);
            if (!Core.SearchQuery.IsNullOrEmpty()) {
                var comparison = Core.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Predicate<Pallet> match = x => x._barcode._id.Contains(Core.SearchQuery, comparison) && Core.IncludeBarcodes || x._tags.Contains(Core.SearchQuery) && Core.IncludeTags || x._title.Contains(Core.SearchQuery, comparison) && Core.IncludeTitles || x.Author.Contains(Core.SearchQuery, comparison) && Core.IncludeAuthors;
                Pallets = Pallets.ToList().FindAll(match);
            }
            foreach (var pallet in Pallets) {
                OnPalletGenerated.Invoke(pallet);
            }
            MelonLogger.Msg("Done Loading");
        }
    }
}
