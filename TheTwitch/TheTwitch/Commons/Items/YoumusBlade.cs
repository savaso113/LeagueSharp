using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheTwitch.Commons.Items
{
    class YoumusBlade : IActivateableItem
    {
        private int _minEnemyHealth;
        private bool _onlyTwitchUlt;

        public void Initialize(Menu menu)
        {
            menu.AddMItem("Enemy min HP %", new Slider(20), (sender, args) => _minEnemyHealth = args.GetNewValue<Slider>().Value).ProcStoredValueChanged<Slider>();
            if (ObjectManager.Player.ChampionName == "Twitch")
            {
                menu.AddMItem("Only in Twitch ult", true, (sender, args) => _onlyTwitchUlt = args.GetNewValue<bool>()).ProcStoredValueChanged<bool>();
            }
        }

        public string GetDisplayName()
        {
            return "Youmus Ghostblade";
        }

        public void Update(Obj_AI_Hero target)
        {
            if (target.HealthPercent >= _minEnemyHealth && (!_onlyTwitchUlt || ObjectManager.Player.HasBuff("TwitchFullAutomatic")))
            {
                var itemSpell = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name == "YoumusBlade");
                if (itemSpell != null && itemSpell.GetState() == SpellState.Ready) ObjectManager.Player.Spellbook.CastSpell(itemSpell.Slot, target);
            }
        }
    }
}
