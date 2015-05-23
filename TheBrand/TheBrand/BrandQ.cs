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
        private BrandE _brandE;
        private Skill[] _brandQWE;
        private MenuItem _harassHitchance, _comboHitchance;

        public BrandQ(Spell spell)
            : base(spell)
        {
            spell.SetSkillshot(0.625f, 50f, 1600f, true, SkillshotType.SkillshotLine);
        }

        public override void Initialize(IMainContext context, ComboProvider combo)
        {
            _brandE = combo.GetSkill<BrandE>();
            var skills = combo.GetSkills().ToList();
            skills.Remove(skills.First(skill => skill is BrandQ));
            _brandQWE = skills.ToArray();
            _harassHitchance = context.GetRootMenu().GetMenuItem("Harass.Hitchance");
            _comboHitchance = context.GetRootMenu().GetMenuItem("Combo.MinHitchance");
            base.Initialize(context, combo);
        }


        public override void Cast(Obj_AI_Hero target, bool force = false, HitChance minChance = HitChance.Low)
        {
            if (target == null ||
                HasBeenSafeCast(Spell.Instance.Name) ||
                (!target.HasBuff("brandablaze") && (!(ObjectManager.Player.GetSpellDamage(target, Spell.Instance.Slot) + ObjectManager.Player.GetAutoAttackDamage(target, true) > target.Health))) && !force && _brandQWE.Any(spell => spell.Spell.Instance.State == SpellState.Ready || spell.Spell.Instance.CooldownExpires > Game.Time && spell.Spell.Instance.CooldownExpires - Game.Time < spell.Spell.Instance.Cooldown / 2f)) return;
            // wenn any skill ready || half cooldown 

            var prediction = Spell.GetPrediction(target);
            if (prediction.Hitchance < minChance) return;

            SafeCast(() => Spell.Cast(prediction.CastPosition));
        }

        public override void Combo(IMainContext context, ComboProvider combo, Obj_AI_Hero target)
        {
            Cast(target, false, (HitChance)Enum.Parse(typeof(HitChance), _comboHitchance.GetValue<StringList>().SelectedValue, true));
        }

        public override void Harass(IMainContext context, ComboProvider combo, Obj_AI_Hero target)
        {
            Cast(target, false, (HitChance)Enum.Parse(typeof(HitChance), _harassHitchance.GetValue<StringList>().SelectedValue, true));
        }

        public override void Interruptable(IMainContext context, ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell)
        {
            if (sender.Distance(ObjectManager.Player) < 1050 && sender.HasBuff("brandablaze") && !HasBeenSafeCast())
                Cast(sender);
            base.Interruptable(context, combo, sender, interruptableSpell);
        }


        public override int GetPriority()
        {
            return 3;
        }
    }
}
