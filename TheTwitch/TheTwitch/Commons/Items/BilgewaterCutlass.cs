using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheTwitch.Commons.Items
{
    class BilgewaterCutlass : IActivateableItem
    {
        private int _minEnemyHealth;


        public void Initialize(Menu menu)
        {
            menu.AddMItem("Enemy min HP %", new Slider(20), (sender, args) => _minEnemyHealth = args.GetNewValue<Slider>().Value).ProcStoredValueChanged<Slider>();
        }

        public string GetDisplayName()
        {
            return "Bigewater Cutlass";
        }

        public void Update(Obj_AI_Hero target)
        {
            if (target.HealthPercent >= _minEnemyHealth && target.Distance(ObjectManager.Player) < 550)
            {
                var itemSpell = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name == "BilgewaterCutlass");
                if (itemSpell != null && itemSpell.GetState() == SpellState.Ready) ObjectManager.Player.Spellbook.CastSpell(itemSpell.Slot, target);
            }
        }
    }
}
