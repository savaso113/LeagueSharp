using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using TheGaren.Commons.ComboSystem;
using TheGaren.Commons.Items;

namespace TheGaren.Commons
{
    public static class ItemManager
    {
        private static Dictionary<IActivateableItem, bool> _items;

        public static void Initialize(Menu menu, ComboProvider combo)
        {
            _items = new Dictionary<IActivateableItem, bool>();
            var items = new IActivateableItem[] { new BilgewaterCutlass(), new Botrk(), new YoumusBlade(), new RavenousHydra() };

            foreach (var activateableItem in items)
            {
                IActivateableItem item = activateableItem;

                var itemMenu = new Menu(item.GetDisplayName(), item.GetDisplayName());
                item.Initialize(itemMenu);
                _items.Add(item, true);
                itemMenu.AddMItem("Enabled", true, (sender, agrs) => _items[item] = agrs.GetNewValue<bool>()).ProcStoredValueChanged<bool>();
                menu.AddSubMenu(itemMenu);
            }
            Game.OnUpdate += _ => Update(combo);
        }

        private static void Update(ComboProvider combo)
        {
            var target = combo.GetTarget();
            if (!target.IsValidTarget() || combo.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo) return;
            foreach (var item in _items)
                if (item.Value)
                    item.Key.Update(target);
        }

        public static T GetItem<T>() where T : IActivateableItem
        {
            foreach (var item in _items.Keys)
                if (item is T)
                    return (T)item;
            return default(T);
        }
    }
}
