using Il2CppSLZ.Marrow.Warehouse;
using Il2CppWebSocketSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Port_A_Warehouse {
    public static class Extensions {
        public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.List<T> dirtyList) {
            // idk what this does, vs just gave me this
            List<T> list = [.. dirtyList];
            dirtyList.Clear();
            return list;
        }

        public static List<T> FilterAndCleanCrates<T>(this List<T> Crates) where T : Crate {
            Crates.RemoveAll(x => x.Redacted && !Core.ShowRedacted);
            Crates.RemoveAll(x => x.Pallet.Internal && !Core.ShowInternal);
            Crates.RemoveAll(x => x.Unlockable && !Core.ShowUnlockable);
            if (!Core.SearchQuery.IsNullOrEmpty()) {
                var comparison = Core.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Predicate<Crate> match = x => x._barcode._id.Contains(Core.SearchQuery, comparison) && Core.IncludeBarcodes || x._tags.Contains(Core.SearchQuery) && Core.IncludeTags || x._title.Contains(Core.SearchQuery, comparison) && Core.IncludeTitles || x.Pallet.Author.Contains(Core.SearchQuery, comparison) && Core.IncludeAuthors;
                Crates = Crates.ToList().FindAll(match);
            }
            return Crates;
        }
    }
}
