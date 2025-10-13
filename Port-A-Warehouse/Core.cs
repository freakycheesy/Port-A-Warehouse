using BoneLib;
using BoneLib.BoneMenu;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using MelonLoader;
using System.Threading.Tasks;
using UnityEngine;

[assembly: MelonInfo(typeof(Port_A_Warehouse.Core), "Port-A-Warehouse", "1.0.0", "freakycheesy", null)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace Port_A_Warehouse {
    public class Core : MelonMod {
        public static Page Page;
        public static Page PalletsPage;

        public static bool ShowRedacted { get; set; } = true;
        public static bool ShowInternal { get; set; } = true;
        public static bool ShowUnlockable { get; set; } = true;

        public static bool IncludeBarcodes { get; set; } = true;
        public static bool IncludeTags { get; set; } = true;
        public static bool IncludeAuthors { get; set; } = true;
        public static bool IncludeTitles { get; set; } = true;
        public static bool CaseSensitive { get; set; } = false;

        public static string SearchQuery;
        public override void OnInitializeMelon() {
            LoggerInstance.Msg("Initialized.");
            WarehouseData.OnCratesGenerated += CreateCratesPage;
            BoneMenuCreator();
        }

        private void CreateCratesPage() {
            foreach (var crate in WarehouseData.Crates) {
                CreateCratePage(crate);
            }
        }

        private void BoneMenuCreator() {
            Page = Page.Root.CreatePage("Asset Warehouse", Color.red);
            Page.CreateFunction("Refresh", Color.green, Refresh);
            Page.CreateString("Search Query", Color.green, SearchQuery, Search);
            FitlerOptions();
            QueryOptions();

            PalletsPage = Page.CreatePage("Pallets", Color.green);
        }

        private static void QueryOptions() {
            var queryOptions = Page.CreatePage("Query Options", Color.white);
            queryOptions.CreateBool("Include Barcodes", Color.white, IncludeBarcodes, (a) => IncludeBarcodes = a);
            queryOptions.CreateBool("Include Tags", Color.white, IncludeTags, (a) => IncludeTags = a);
            queryOptions.CreateBool("Include Authors", Color.white, IncludeAuthors, (a) => IncludeAuthors = a);
            queryOptions.CreateBool("Include Titles", Color.white, IncludeTitles, (a) => IncludeTitles = a);
            queryOptions.CreateBool("Case Sensitive", Color.white, CaseSensitive, (a) => CaseSensitive = a);
        }

        private static void FitlerOptions() {
            var filters = Page.CreatePage("Filters", Color.white);
            filters.CreateBool("Show Redacted", Color.white, ShowRedacted, (a) => ShowRedacted = a);
            filters.CreateBool("Show Internal", Color.white, ShowInternal, (a) => ShowInternal = a);
            filters.CreateBool("Show Unlockable", Color.white, ShowUnlockable, (a) => ShowUnlockable = a);
        }
        public void Refresh() {
            _RefreshThread();
        }
        private async void _RefreshThread() {
            ClearPages();
            WarehouseData.GenerateCratesData();
        }

        public void Search(string query) {
            SearchQuery = query;
            Refresh();
        }

        private void CreateCratePage<T>(T crate) where T : Crate {
            var pallet = crate.Pallet;
            var palletPage = PalletsPage.CreatePage($"{pallet._title}\n({pallet._barcode._id})", Color.green);
            var typePage = palletPage.CreatePage(typeof(T).Name, Color.white);
            var cratePage = typePage.CreatePage($"{crate._title}\n({crate._barcode._id})", Color.white);
            cratePage.CreateFunction("Load Asset", Color.white, () => OnCrateClick(crate));
        }

        private void OnCrateClick<T>(T c) where T : Crate {
            Type crateType = typeof(T);
            if (crateType == typeof(SpawnableCrate)) {
                if (crateType == typeof(AvatarCrate)) {
                    Player.RigManager.SwapAvatarCrate(c.Barcode);
                    return;
                }
                SpawnGunPatch.SwapGlobalCrate(c as SpawnableCrate);
                return;
            }
            if (crateType == typeof(LevelCrate)) {
                SceneStreamer.Load(c.Barcode);
                return;
            }
        }

        public static List<T> GetCleanList<T>(Il2CppSystem.Collections.Generic.List<T> dirtyList) {
            // idk what this does, vs just gave me this
            List<T> list = [.. dirtyList];
            dirtyList.Clear();
            return list;
        }

        private static void ClearPages() {
            PalletsPage.RemoveAll();
        }
    }
}