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

            base.Initialize();
        }


        public override void Update()
        {

            if (!(AutoInCombo && (Orbwalking.CanAttack() || ObjectManager.Player.IsWindingUp)))
            {
                base.Update();
            }
          

            var target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
            if (target.IsValidTarget())
                IgniteManager.Update(this, GetRemainingCassDamage(target));
            else
                IgniteManager.Update(this); // maybe should use GetTarget!?
        }

        public override bool ShouldBeDead(Obj_AI_Base target, float additionalSpellDamage = 0f)
        {
            return base.ShouldBeDead(target, additionalSpellDamage: GetRemainingCassDamage(target));
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
    }
}
