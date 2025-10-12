using BoneLib;
using BoneLib.BoneMenu;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Port_A_Warehouse.Core), "Port-A-Warehouse", "1.0.0", "freakycheesy", null)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace Port_A_Warehouse
{
    public class Core : MelonMod
    {
        public static Page Page { get; set; }
        public static Page CratePage;
        public static Page AllCratesPages;
        public static Page SpawnablesCratesPages;
        public static Page LevelsCratesPages;
        public static Page AvatarsCratesPages;

        public static string searchquery;
        public override void OnInitializeMelon()
        {
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
            CratePage = Page.CreatePage("Crates", Color.cyan);
            Refresh();
        }

        private void Refresh() {
            ClearPages();
            AddItems();
        }

        private void AddItems() {
            CratePage.CreateString("Search Query", Color.green, searchquery, (a) => { searchquery = a; Refresh(); });
            CreatePages<Crate>(typeof(Crate).Name, out AllCratesPages);
            CreatePages<SpawnableCrate>(typeof(SpawnableCrate).Name, out SpawnablesCratesPages);
            CreatePages<LevelCrate>(typeof(LevelCrate).Name, out LevelsCratesPages);
            CreatePages<AvatarCrate>(typeof(AvatarCrate).Name, out AvatarsCratesPages);
            
        }

        public void CreatePages<T>(string label, out Page page) where T : Crate {
            page = CratePage.CreatePage(label, Color.white);
            var crates = GetCleanList(AssetWarehouse.Instance.GetCrates<T>());
            List<T> selectedCrates = crates;
            if (!searchquery.IsNullOrEmpty()) {
                var comparison = StringComparison.OrdinalIgnoreCase;
                var foundCrates = crates.FindAll(x => x._barcode._id.Contains(searchquery, comparison) || x._tags.Contains(searchquery) || x._title.Contains(searchquery, comparison));
                selectedCrates = foundCrates;
            }
            foreach (var c in selectedCrates) {
                var cratePage = page.CreatePage($"{c._title}\n({c._barcode._id})", Color.white);
                cratePage.CreateFunction(label, Color.white, ()=>OnCrateClickClick(c));
            }
        }

        private void OnCrateClickClick<T>(T c) where T : Crate {
            if (typeof(T) == typeof(SpawnableCrate)) {
                Behaviour.FindObjectsOfType<Il2CppSLZ.Bonelab.SpawnGun>().ToList().ForEach((a) => {
                    a._selectedCrate = c as SpawnableCrate;
                    a._lastSelectedCrate = c as SpawnableCrate;
                    a._selectedCrate = c as SpawnableCrate;
                });
            }
            else if (typeof(T) == typeof(LevelCrate)) {
                SceneStreamer.Load(c.Barcode);
            }
            else if (typeof(T) == typeof(AvatarCrate)) {
                Player.RigManager.SwapAvatarCrate(c.Barcode);
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