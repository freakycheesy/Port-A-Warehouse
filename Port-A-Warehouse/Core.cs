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

        public static Page AllCratesPages;

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
            WarehouseData.OnPalletGenerated += (_)=>CreatePalletPage(_, PalletPage);
            BoneMenuCreator();
        }

        private void BoneMenuCreator() {
            Page = Page.Root.CreatePage("Asset Warehouse", Color.red);
            Page.CreateFunction("Refresh", Color.green, Refresh);
            Page.CreateString("Search Query", Color.green, SearchQuery, Search);
            FitlerOptions();
            QueryOptions();

            PalletPage = Page.CreatePage("Pallets", Color.green);
            AllCratesPages = Page.CreatePage("All Crates", Color.green);
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
            WarehouseData.GeneratePalletData();
        }

        public void Search(string query) {
            SearchQuery = query;
            Refresh();
        }

        public void CreatePalletPage(Pallet pallet, Page parentPage) {
            var page = parentPage.CreatePage($"{pallet._title}\n({pallet._barcode._id})", Color.green);
            CreateCratesPage(pallet, page);
        }

        private void CreateCratesPage(Pallet pallet, Page parentPage) {
            var crates = GetCleanList(pallet.Crates);
            crates.RemoveAll(x => x.Redacted && !ShowRedacted);
            crates.RemoveAll(x => x.Pallet.Internal && !ShowInternal);
            crates.RemoveAll(x => x.Unlockable && !ShowUnlockable);
            if (!SearchQuery.IsNullOrEmpty()) {
                var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Predicate<Crate> match = x => x._barcode._id.Contains(SearchQuery, comparison) && IncludeBarcodes || x._tags.Contains(SearchQuery) && IncludeTags || x._title.Contains(SearchQuery, comparison) && IncludeTitles || x._pallet.Author.Contains(SearchQuery, comparison) && IncludeAuthors;

                crates = crates.FindAll(match);
            }
            foreach (var crate in crates) {
                var typePage = parentPage.CreatePage(crate.GetType().Name, Color.white);
                var cratePage = typePage.CreatePage($"{crate._title}\n({crate._barcode._id})", Color.white);
                AllCratesPages.CreatePageLink(cratePage);
                cratePage.CreateFunction("Load Asset", Color.white, () => OnCrateClick(crate));
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
            PalletPage.RemoveAll();
        }
    }
}