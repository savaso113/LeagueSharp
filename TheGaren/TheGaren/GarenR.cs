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
    class GarenR : Skill
    {
        private readonly int[] _ultDamage = { 170, 340, 500 };   
        private readonly float[] _ultDamagePerHp = { 3.5f, 3f, 2.5f };

        public GarenR(Spell spell)
            : base(spell)
        {
        }

        public override void Combo(IMainContext context, ComboProvider combo)
        {
            if (Spell.Instance.State != SpellState.Ready) return;

            IEnumerable<Obj_AI_Hero> targets;
            if (context.GetRootMenu().SubMenu("Combo").Item("Combo.OnlyUlttarget").GetValue<bool>())
            {
                var selected = TargetSelector.GetTarget(375, TargetSelector.DamageType.Magical);
                if (selected == null)
                    return;
                targets = new[] { selected };
            }
            else
                targets = HeroManager.Enemies.ToArray();

            foreach (var objAiHero in targets)
            {
                if ((!objAiHero.IsVisible) || objAiHero.IsDead || objAiHero.Distance(ObjectManager.Player) > 375)
                    continue;

                var level = Spell.Level;
                var damage = ObjectManager.Player.CalcDamage(objAiHero, Damage.DamageType.Magical, (_ultDamage[level - 1] + (objAiHero.MaxHealth - objAiHero.Health) / _ultDamagePerHp[level - 1]));

                if (damage > objAiHero.Health + objAiHero.MagicShield  && objAiHero.Health > ObjectManager.Player.GetAutoAttackDamage(objAiHero) && combo.GrabControl(this)) //Todo: refine calculation
                {
                    Obj_AI_Hero hero = objAiHero;
                    SafeCast(() => Spell.Cast(hero));
                }
            }
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            var level = Spell.Level;
            if (level == 0 || Spell.Instance.State != SpellState.Ready)
                return 0f;
            var damage = ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical, (_ultDamage[level - 1] + (enemy.MaxHealth - enemy.Health) / _ultDamagePerHp[level - 1]));
            return (float)damage;
        }

        public override bool NeedsControl()
        {
            return IsInSafeCast("GarenR", Spell.Instance.SData.SpellCastTime + 0.5f);
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
