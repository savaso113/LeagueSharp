using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace TheGaren
{
    public static class Commons
    {
        private static readonly Dictionary<MenuItem, EventHandler<OnValueChangeEventArgs>> HandlerMapper = new Dictionary<MenuItem, EventHandler<OnValueChangeEventArgs>>();
        private static readonly Dictionary<MenuItem, Menu> MenuMapper = new Dictionary<MenuItem, Menu>();

        public static MenuItem AddMItem<T>(this Menu menu, string name, T value)
        {
            return menu.AddItem(new MenuItem(menu.Name + "." + name.Replace(" ", ""), name).SetValue(value));
        }

        public static MenuItem AddMItem<T>(this Menu menu, string name, T value, EventHandler<OnValueChangeEventArgs> handler)
        {
            var menuItem = new MenuItem(menu.Name + "." + name.Replace(" ", ""), name).SetValue(value);
            menuItem.ValueChanged += handler;
            HandlerMapper.Add(menuItem, handler);
            MenuMapper.Add(menuItem, menu);
            return menu.AddItem(menuItem);
        }

        public static void ProcStoredValueChanged<T>(this Menu menu)
        {
            foreach (var eventHandler in HandlerMapper.Where(item => MenuMapper[item.Key] == menu))
                eventHandler.Value(eventHandler.Key, new OnValueChangeEventArgs(eventHandler.Key.GetValue<T>(), eventHandler.Key.GetValue<T>()));
        }
    }
}
