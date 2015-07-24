using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;

namespace TheCassiopeia
{
    class CassioCombo : ComboProvider
    {
        public bool AutoInCombo;
        public MenuItem AssistedUltMenu;
        private CassR _r;
        public bool BlockBadUlts;
        public bool EnablePoisonTargetSelection;

        public CassioCombo(float targetSelectorRange, IEnumerable<Skill> skills, Orbwalking.Orbwalker orbwalker)
            : base(targetSelectorRange, skills, orbwalker)
        {
        }

        public CassioCombo(float targetSelectorRange, Orbwalking.Orbwalker orbwalker, params Skill[] skills)
            : base(targetSelectorRange, orbwalker, skills)
        {
        }

        public override void Initialize()
        {
            _r = GetSkill<CassR>();
            Spellbook.OnCastSpell += OnCastSpell;
            Orbwalking.BeforeAttack += OrbwalkerBeforeAutoAttack;
            base.Initialize();
        }

        private void OrbwalkerBeforeAutoAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Target.IsValidTarget() && Target.IsPoisoned())
                args.Process = AutoInCombo;
        }

        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R && HeroManager.Enemies.All(enemy => !enemy.IsValidTarget(_r.Range) || !_r.WillHit(enemy, args.StartPosition)))
            {
                args.Process = !BlockBadUlts;
            }
        }

        private void CastAssistedUlt()
        {
            if (!Target.IsValidTarget(_r.Range)) return;
            var pred = _r.GetPrediction(Target);
            if (pred.Hitchance >= Commons.Prediction.HitChance.Low)
                _r.Cast(pred.CastPosition);
        }

        public override void Update()
        {
            if (AssistedUltMenu != null && AssistedUltMenu.GetValue<KeyBind>().Active)
                CastAssistedUlt();

            base.Update();
        }

        public override bool ShouldBeDead(Obj_AI_Base target, float additionalSpellDamage = 0f)
        {
            return base.ShouldBeDead(target, GetRemainingCassDamage(target));
        }

        public float GetRemainingCassDamage(Obj_AI_Base target)
        {
            var buff = target.GetBuff("cassiopeianoxiousblastpoison");
            float damage = 0;
            if (buff != null)
                damage += (float)(((int)(buff.EndTime - Game.Time)) * (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) / 3));

            buff = target.GetBuff("cassiopeiamiasmapoison");
            if (buff != null)
            {
                damage += (float)(((int)(buff.EndTime - Game.Time)) * (ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)));
            }

            return damage;
        }

        protected override Obj_AI_Hero SelectTarget()
        {
            var target = base.SelectTarget();
            if (EnablePoisonTargetSelection && target.IsValidTarget(TargetRange) && !target.IsPoisoned())
            {
                var newTarget = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(TargetRange) && enemy.IsPoisoned()).MaxOrDefault(TargetSelector.GetPriority);
                if (newTarget != null && TargetSelector.GetPriority(target) - TargetSelector.GetPriority(newTarget) < 0.5f)
                    return newTarget;
            }
            return target;
        }
    }
}
