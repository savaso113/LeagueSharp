using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using TheBrand.ComboSystem;

namespace TheBrand
{
    class BrandQ : Skill
    {
        // ReSharper disable once InconsistentNaming
        private Skill[] _brandQWE;

        public BrandQ(Spell spell)
            : base(spell)
        {
            spell.SetSkillshot(0.625f, 50f, 1600f, true, SkillshotType.SkillshotLine);
        }

        public override void Initialize(ComboProvider combo)
        {
            var skills = combo.GetSkills().ToList();
            skills.Remove(skills.First(skill => skill is BrandQ));
            _brandQWE = skills.ToArray();
            base.Initialize(combo);
        }


        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            if ((!target.HasBuff("brandablaze") && (!(ObjectManager.Player.GetSpellDamage(target, Spell.Instance.Slot) + ObjectManager.Player.GetAutoAttackDamage(target, true) > target.Health))) && !force && _brandQWE.Any(spell => spell.Spell.Instance.State == SpellState.Ready || spell.Spell.Instance.CooldownExpires > Game.Time && spell.Spell.Instance.CooldownExpires - Game.Time < spell.Spell.Instance.Cooldown / 2f)) return;
            // wenn any skill ready || half cooldown 
            var targetBurn = target.GetBuff("brandablaze");
            if (targetBurn != null && !force && targetBurn.EndTime - Game.Time < 0.75f) return;

            Console.WriteLine("q Cast 123 "+force+" "+target.HasBuff("brandablaze"));
            SafeCast(target);
        }



        public override void Interruptable(ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell)
        {
            if (sender.Distance(ObjectManager.Player) < 1050 && sender.HasBuff("brandablaze"))
                Cast(sender, true);
            base.Interruptable(combo, sender, interruptableSpell);
        }


        public override int GetPriority()
        {
            return 3;
        }
    }
}
