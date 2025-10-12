using BoneLib;
using BoneLib.BoneMenu;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Port_A_Warehouse.Core), "Port-A-Warehouse", "1.0.0", "freakycheesy", null)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace Port_A_Warehouse {
    public class Core : MelonMod {
        public static Page Page;
        public static Page CratePage;
        public static Page AllCratesPages;
        public static Page SpawnablesCratesPages;
        public static Page LevelsCratesPages;
        public static Page AvatarsCratesPages;

        public static bool ShowRedacted { get; set; } = true;
        public static bool ShowInternal { get; set; } = true;
        public static bool CaseSensitive { get; set; } = false;

        public static string searchquery;
        public override void OnInitializeMelon() {
            LoggerInstance.Msg("Initialized.");
            BoneMenuCreator();

            Hooking.OnWarehouseReady += Hooking_OnWarehouseReady;
        }

        private void Hooking_OnWarehouseReady() {
            Refresh();
        }

        private void BoneMenuCreator() {
            Page = Page.Root.CreatePage("Asset Warehouse", Color.red);
            Page.CreateFunction("Refresh", Color.green, Refresh);
            Page.CreateBool("Show Redacted", Color.white, ShowRedacted, (a) => ShowRedacted = a);
            Page.CreateBool("Show Internal", Color.white, ShowInternal, (a) => ShowInternal = a);
            Page.CreateBool("Case Sensitive", Color.white, CaseSensitive, (a) => CaseSensitive = a);
            CratePage = Page.CreatePage("Crates", Color.cyan);
            Refresh();
        }

        private void Refresh() {
            ClearPages();
            AddItems();
        }

        private void AddItems() {
            CratePage.CreateString("Search Query", Color.green, searchquery, Search).ElementTooltip = "Can lag alot if the search query has tons of results,\nor if you don't search anything at all to see everything";
            CreatePages<Crate>(typeof(Crate).Name, "Preview Only (No Function)", out AllCratesPages, "i just said preview bud");
            CreatePages<SpawnableCrate>(typeof(SpawnableCrate).Name, "Select Spawnable", out SpawnablesCratesPages, "Selects the spawnable on all spawn guns");
            CreatePages<LevelCrate>(typeof(LevelCrate).Name, "Load Level", out LevelsCratesPages, "Loads the level directly");
            CreatePages<AvatarCrate>(typeof(AvatarCrate).Name, "Swap Avatar", out AvatarsCratesPages, "Swaps your avatar directly");
        }

        public void Search(string query) {
            searchquery = query;
            Refresh();
        }

        public void CreatePages<T>(string label, string buttonName, out Page page, string buttonTooltip = "") where T : Crate {
            page = CratePage.CreatePage(label, Color.white);
            var crates = GetCleanList(AssetWarehouse.Instance.GetCrates<T>());
            List<T> selectedCrates = crates;
            selectedCrates.RemoveAll(x => x.Redacted && !ShowRedacted);
            selectedCrates.RemoveAll(x => x.Pallet.Internal && !ShowInternal);
            if (!searchquery.IsNullOrEmpty()) {
                var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Predicate<T> match = x => x._barcode._id.Contains(searchquery, comparison) || x._tags.Contains(searchquery) || x._title.Contains(searchquery, comparison) || x._pallet.Author.Contains(searchquery, comparison);
                var foundCrates = crates.FindAll(match);
                selectedCrates = foundCrates;
            }
            foreach (var c in selectedCrates) {
                var cratePage = page.CreatePage($"{c._title}\n({c._barcode._id})", Color.white);
                cratePage.CreateFunction(buttonName, Color.white, () => OnCrateClickClick(c)).ElementTooltip = buttonTooltip;
            }
        }

        private void OnCrateClickClick<T>(T c) where T : Crate {
            if (c is SpawnableCrate) {
                if (c is AvatarCrate) {
                    Player.RigManager.SwapAvatarCrate(c.Barcode);
                    return;
                }
                Behaviour.FindObjectsOfType<Il2CppSLZ.Bonelab.SpawnGun>().ToList().ForEach((a) => {
                    a._selectedCrate = c as SpawnableCrate;
                    a._lastSelectedCrate = c as SpawnableCrate;
                    a._selectedCrate = c as SpawnableCrate;
                });
            }
            if (c is LevelCrate) {
                SceneStreamer.Load(c.Barcode);
            }
        }

        public static List<T> GetCleanList<T>(Il2CppSystem.Collections.Generic.List<T> dirtyList) {
            List<T> list = new List<T>();
            foreach (var dirtyItem in dirtyList) {
                list.Add(dirtyItem);
            }
            return list;
        }

        private static void ClearPages() {
            CratePage.RemoveAll();
        }
    }
}