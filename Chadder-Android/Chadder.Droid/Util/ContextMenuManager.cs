using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chadder.Droid.Views;

namespace Chadder.Droid.Util
{
    internal class ContextMenuIdManager
    {
        private static int nextGroupId = 0;
        public static int GetId()
        {
            return nextGroupId++;
        }
    }
    public class ContextMenuManager<T>
    {
        public ContextMenuManager(BaseFragment fragment, View view)
            : this()
        {
            fragment.RegisterForContextMenu(view);
        }

        public ContextMenuManager(BaseActionBarActivity activity, View view)
            : this()
        {
            activity.RegisterForContextMenu(view);
        }

        private ContextMenuManager()
        {
            GroupID = ContextMenuIdManager.GetId();
        }

        private int GroupID { get; set; }
        public string Title { get; set; }

        public delegate bool BoolDelegate(T item);
        public delegate string StringDelegate(T item);
        public delegate void VoidDelegate(T item);
        private List<Tuple<StringDelegate, VoidDelegate, BoolDelegate>> _items = new List<Tuple<StringDelegate, VoidDelegate, BoolDelegate>>();

        public void InsertItem(StringDelegate txt, VoidDelegate e, BoolDelegate visible = null)
        {
            _items.Add(new Tuple<StringDelegate, VoidDelegate, BoolDelegate>(txt, e, visible));
        }
        public void InsertItem(string txt, VoidDelegate e, BoolDelegate visible = null)
        {
            _items.Add(new Tuple<StringDelegate, VoidDelegate, BoolDelegate>(a => txt, e, visible));
        }

        private T _menuItem;
        public void CreateMenu(IContextMenu menu, T item)
        {
            _menuItem = item;
            if (Title != null)
                menu.SetHeaderTitle(Title);

            for (int i = 0; i < _items.Count; ++i)
                if (_items[i].Item3 == null || _items[i].Item3(item) == true)
                    menu.Add(GroupID, i, i, _items[i].Item1(item));
        }

        public bool Selected(IMenuItem item)
        {
            if (item.GroupId != GroupID)
                return false;

            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var menuId = item.ItemId;

            _items[menuId].Item2(_menuItem);
            _menuItem = default(T);

            return true;
        }
    }
}