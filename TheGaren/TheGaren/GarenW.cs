using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheGaren.ComboSystem;

namespace TheGaren
{
    class GarenW : Skill
    {
        private const float CheckTime = 0.5f;
        private const float TriggerRate = 0.01f; // 1%
        private float _checkTime;
        private float _lastHealthRate;

        public GarenW(Spell spell)
            : base(spell)
        {
        }

        public override void SetEnabled(Orbwalking.OrbwalkingMode mode, bool enabled)
        {
            if(mode == Orbwalking.OrbwalkingMode.Combo && enabled == true)
                _lastHealthRate = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth;
            base.SetEnabled(mode, enabled);
        }

        public override void Combo(IMainContext context, ComboProvider combo)
        {
            if (context.GetRootMenu().SubMenu("Misc").Item("Misc.WMode").GetValue<StringList>().SelectedValue == "Always")
            {
                if (Spell.Instance.State == SpellState.Ready)
                    SafeCast(() => Spell.Cast());
            }
            else if (_checkTime < Game.Time)
            {
                _checkTime = Game.Time + CheckTime;

                if (_lastHealthRate - TriggerRate > (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) && Spell.Instance.State == SpellState.Ready)
                    SafeCast(() => Spell.Cast());
                _lastHealthRate = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth;
            }
        }

        public override bool NeedsControl()
        {
            return false;
        }

        public override int GetPriority()
        {
            return 3;
        }
    }
}
