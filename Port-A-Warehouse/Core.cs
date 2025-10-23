using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.Notifications;
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
        public const int MaxElements = 10;
        public static string SearchQuery;
        public override void OnInitializeMelon() {
            LoggerInstance.Msg("Initialized.");
            WarehouseData.OnCratesGenerated += CreateCratesPage;
            Hooking.OnLevelLoaded += (_) => RemovePallets();
            Hooking.OnLevelUnloaded += RemovePallets;
            Hooking.OnUIRigCreated += RemovePallets;
            BoneMenuCreator();
        }

        private void CreateCratesPage() {
            foreach (var crate in WarehouseData.AvatarCrates) {
                CreateCratePage(crate);
            }
            foreach (var crate in WarehouseData.LevelCrates) {
                CreateCratePage(crate);
            }
            foreach (var crate in WarehouseData.SpawnableCrates) {
                CreateCratePage(crate);
            }
        }

        private void BoneMenuCreator() {
            Page = Page.Root.CreatePage("Asset Warehouse", Color.red);
            Page.CreateFunction("Refresh", Color.green, Refresh);
            Page.CreateString("Search Query", Color.green, SearchQuery, Search);
            FitlerOptions();
            QueryOptions();
            PalletsPage = Page.CreatePage("Pallets", Color.green, MaxElements);
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
            RemovePallets();
            WarehouseData.GenerateCratesData();
        }

        private static void RemovePallets() {
            PalletsPage.RemoveAll();
        }

        public void Search(string query) {
            SearchQuery = query;
            Refresh();
        }

        private void CreateCratePage<T>(T crate) where T : Crate {
            var pallet = crate.Pallet;
            var palletPage = PalletsPage.CreatePage($"{pallet._title}\n({pallet._barcode._id})", Color.green, MaxElements);
            Page typePage = palletPage.CreatePage(crate.GetType().Name, Color.cyan, MaxElements);
            var cratePage = typePage.CreatePage($"{crate._title}\n({crate._barcode._id})", Color.cyan, MaxElements);
            cratePage.CreateFunction(GetCrateActionName(crate), Color.white, () => UseCrate(crate));
        }

        public string GetCrateActionName<T>(T crate) where T : Crate {
            string name = string.Empty;
            if (crate is LevelCrate)
                name = "Load Level";
            if (crate is AvatarCrate)
                name = "Swap Avatar";
            else if (crate is SpawnableCrate)
                name = "Swap Spawn Gun Crate";
            return name;
        }

        private void UseCrate<T>(T crate) where T : Crate {
            if (crate is LevelCrate)
                LoadLevel(crate);
            if (crate is AvatarCrate)
                SwapAvatar(crate);
            else if (crate is SpawnableCrate)
                SelectSpawnable(crate);
        }

        public void LoadLevel(Crate value) {
            MelonLogger.Msg("Trying to Load Scene");
            SceneStreamer.Load(value.Barcode);
        }

        public void SelectSpawnable(Crate value) {
            MelonLogger.Msg("Trying to Select Spawnable");
            SpawnGunPatch.SwapGlobalCrate(value as SpawnableCrate);
        }

        public void SwapAvatar(Crate value) {
            MelonLogger.Msg("Trying to Swap Avatar");
            Player.RigManager.SwapAvatarCrate(value.Barcode);
        }
    }
}