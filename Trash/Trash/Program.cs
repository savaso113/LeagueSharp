using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Trash
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                var trash = new[] { "Trash", "Just Trash", "trash" };

                var menu = new Menu("Trash", "Trash", true);
                var item = new MenuItem("tk", "Trash Key");
                item.SetValue(new KeyBind('I', KeyBindType.Press));
                item.ValueChanged += (sender, changeEventArgs) =>
                {
                    if (changeEventArgs.GetNewValue<KeyBind>().Active)
                        Game.Say("/ALL " + trash[new Random().Next(0, trash.Length)]);
                };
                menu.AddItem(item);
                menu.AddToMainMenu();

            };
        }
    }
}
