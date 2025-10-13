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
        public static Page PalletPage;
        public static Page CratePage;

        public static Page AllCratesPages;
        public static Page SpawnablesCratesPages;
        public static Page LevelsCratesPages;
        public static Page AvatarsCratesPages;

        public static bool ShowRedacted { get; set; } = true;
        public static bool ShowInternal { get; set; } = true;
        public static bool ShowUnlockable { get; set; } = true;

        public static bool IncludeBarcodes { get; set; } = true;
        public static bool IncludeTags { get; set; } = true;
        public static bool IncludeAuthors { get; set; } = true;
        public static bool IncludeTitles { get; set; } = true;
        public static bool CaseSensitive { get; set; } = false;

        public static string searchquery;
        public override void OnInitializeMelon() {
            LoggerInstance.Msg("Initialized.");
            BoneMenuCreator();
        }

        private void BoneMenuCreator() {
            Page = Page.Root.CreatePage("Asset Warehouse", Color.red);
            Page.CreateFunction("Refresh", Color.green, Refresh);
            Page.CreateString("Search Query", Color.green, searchquery, Search);
            FitlerOptions();
            QueryOptions();

            PalletPage = Page.CreatePage("Pallets", Color.green);
            CratePage = Page.CreatePage("Crates", Color.cyan);
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
        public static Task RefreshTask;
        public void Refresh() {
            _RefreshThread();
        }
        private async void _RefreshThread() {
            ClearPages();
            AddCrateElementsToPage(CratePage);
            CreatePalletPages(PalletPage);
            MelonLogger.Msg("Done Loading");
        }

        private void AddCrateElementsToPage(Page page) {
            CreateCratePages(typeof(Crate).Name, "action", AssetWarehouse.Instance.GetCrates(), page, out AllCratesPages);
            CreateCratePages(typeof(SpawnableCrate).Name, "Select Spawnable", AssetWarehouse.Instance.GetCrates<SpawnableCrate>(), page, out SpawnablesCratesPages);
            CreateCratePages(typeof(LevelCrate).Name, "Load Level", AssetWarehouse.Instance.GetCrates<LevelCrate>(), page, out LevelsCratesPages);
            CreateCratePages(typeof(AvatarCrate).Name, "Swap Avatar", AssetWarehouse.Instance.GetCrates<AvatarCrate>(), page, out AvatarsCratesPages);
        }

        public void Search(string query) {
            searchquery = query;
            Refresh();
        }

        public void CreatePalletPages(Page parentPage) {
            var pallets = GetCleanList(AssetWarehouse.Instance.GetPallets());
            List<Pallet> selectedPallets = pallets;
            selectedPallets.RemoveAll(x => x.Redacted && !ShowRedacted);
            selectedPallets.RemoveAll(x => x.Internal && !ShowInternal);
            selectedPallets.RemoveAll(x => x.Unlockable && !ShowUnlockable);
            if (!searchquery.IsNullOrEmpty()) {
                var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Predicate<Pallet> match = x => x._barcode._id.Contains(searchquery, comparison) && IncludeBarcodes || x._tags.Contains(searchquery) && IncludeTags || x._title.Contains(searchquery, comparison) && IncludeTitles || x.Author.Contains(searchquery, comparison) && IncludeAuthors;

                var foundPallets = pallets.FindAll(match);
                selectedPallets = foundPallets;
            }
            foreach (var p in selectedPallets) {
                var palletPage = parentPage.CreatePage($"{p._title}\n({p._barcode._id})", Color.green);
                AddCrateElementsToPage(palletPage);
            }
        }

        public void CreateCratePages<T>(string label, string buttonName, Il2CppSystem.Collections.Generic.List<T> dirtyList, Page parentPage, out Page page) where T : Crate {
            page = parentPage.CreatePage(label, Color.white);
            var crates = GetCleanList(dirtyList);
            List<T> selectedCrates = crates;
            selectedCrates.RemoveAll(x => x.Redacted && !ShowRedacted);
            selectedCrates.RemoveAll(x => x.Pallet.Internal && !ShowInternal);
            selectedCrates.RemoveAll(x => x.Unlockable && !ShowUnlockable);
            if (!searchquery.IsNullOrEmpty()) {
                var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Predicate<T> match = x => x._barcode._id.Contains(searchquery, comparison) && IncludeBarcodes || x._tags.Contains(searchquery) && IncludeTags || x._title.Contains(searchquery, comparison) && IncludeTitles || x._pallet.Author.Contains(searchquery, comparison) && IncludeAuthors;

                var foundCrates = selectedCrates.FindAll(match);
                selectedCrates = foundCrates;
            }
            foreach (var c in selectedCrates) {
                var cratePage = page.CreatePage($"{c._title}\n({c._barcode._id})", Color.white);
                cratePage.CreateFunction(buttonName, Color.white, () => OnCrateClick(c));
            }
        }

        private void OnCrateClick<T>(T c) where T : Crate {
            if (c is SpawnableCrate) {
                if (c is AvatarCrate) {
                    Player.RigManager.SwapAvatarCrate(c.Barcode);
                    return;
                }
                SpawnGunPatch.SwapGlobalCrate(c as SpawnableCrate);
                return;
            }
            if (c is LevelCrate) {
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
            CratePage.RemoveAll();
        }
    }
}