using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForumSurfer.Collections
{
    public static class SortableObservableCollectionExtension
    {
        public static void AddRange<T>(this SortableObservableCollection<T> list, IEnumerable<T> items)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (items == null) throw new ArgumentNullException("items");

            if (list is List<T>)
            {
                ((SortableObservableCollection<T>)list).AddRange(items);
            }
            else
            {
                foreach (var item in items)
                {
                    list.Add(item);
                }
            }
        }

        public static void AddMissing<T>(this SortableObservableCollection<T> list, IEnumerable<T> items, IEqualityComparer<T> comp)
        {
            AddMissing(list, items, comp, null);
        }

        public static void AddMissing<T>(this SortableObservableCollection<T> list, IEnumerable<T> items, IEqualityComparer<T> comp, SynchronizationContext context)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (items == null) throw new ArgumentNullException("items");


            foreach (var item in items)
            {
                var sought = list.FirstOrDefault(el => comp.Equals(el, item));
                if (sought == null)
                {
                    context.Send(x => list.Add(item), null);
                }
            }
        }
    }
}
