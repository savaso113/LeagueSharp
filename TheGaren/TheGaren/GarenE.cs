using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheGaren.ComboSystem;

namespace TheGaren
{
    class GarenE : Skill
    {
        private float _issueTime;

        public GarenE(Spell spell)
            : base(spell)
        {
        }

        public override void Combo(IMainContext context, ComboProvider combo)
        {
            var target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
            if (Spell.Instance.State == SpellState.Ready && target != null && ((!context.GetRootMenu().SubMenu("Misc").Item("Misc.Eafterattack").GetValue<bool>()) || AAHelper.JustFinishedAutoattack || !AAHelper.WillAutoattackSoon) && combo.GrabControl(this))
            {
                SafeCast(() =>
                {
                    Spell.Cast();
                    var orbwalker = context.GetOrbwalker();
                    orbwalker.SetAttack(false);
                    orbwalker.SetMovement(true);
                }, "GarenE");

                if (_issueTime < Game.Time)
                {
                    _issueTime = Game.Time + 0.01f;
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }
        }

        public override void Harass(IMainContext context, ComboProvider combo)
        {
            Combo(context, combo);
            base.Harass(context, combo);
        }

        public override void LaneClear(IMainContext context, ComboProvider combo)
        {
            var minions = MinionManager.GetMinions(400);
            if (Spell.Instance.State == SpellState.Ready && minions.Count > 0 && combo.GrabControl(this))
            {
                SafeCast(() =>
                {
                    Spell.Cast();
                    var orbwalker = context.GetOrbwalker();
                    orbwalker.SetAttack(false);
                    orbwalker.SetMovement(true);
                }, "GarenE");

                if (_issueTime < Game.Time)
                {
                    _issueTime = Game.Time + 0.01f;
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }

            base.LaneClear(context, combo);
        }

        public override bool TryTerminate(IMainContext context)
        {
            var orbwalker = context.GetOrbwalker();
            orbwalker.SetAttack(true);
            orbwalker.SetMovement(true);
            return base.TryTerminate(context);
        }

        public override bool NeedsControl()
        {
            return IsInSafeCast("GarenE") || ObjectManager.Player.Buffs.Any(buff => buff.Name == "GarenE");
        }

        public override int GetPriority()
        {
            return ObjectManager.Player.Buffs.Any(buff => buff.Name == "GarenE") ? 2 : 1; //overrule Q when spinning
        }
    }
}
