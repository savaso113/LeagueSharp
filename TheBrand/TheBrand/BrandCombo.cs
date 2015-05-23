using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheBrand.ComboSystem;
using TheBrand.Commons;

namespace TheBrand
{
    class BrandCombo : ComboProvider
    {
        private MenuItem _forceAA;

        public BrandCombo(List<Skill> skills, float range)
            : base(skills, range)
        {
        }

        public override void Initialize(IMainContext context)
        {
            _forceAA = context.GetRootMenu().GetMenuItem("Misc.ForceAAincombo");
            base.Initialize(context);
        }

        public override void Update(IMainContext context)
        {
            if (!(_forceAA.GetValue<bool>() && ObjectManager.Player.IsWindingUp))
                base.Update(context);


            var passiveBuff = ObjectManager.Player.GetBuff("brandablaze");
            var target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);

            if (passiveBuff != null)
                IgniteManager.Update(context, target, GetRemainingPassiveDamage(target, passiveBuff), (int)(passiveBuff.EndTime - Game.Time) + 1); // maybe should use GetTarget!?
            else
                IgniteManager.Update(context, target); // maybe should use GetTarget!?

        }


        private float GetRemainingPassiveDamage(Obj_AI_Base target, BuffInstance passive)
        {
            return (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, ((int)(passive.EndTime - Game.Time) + 1) * target.MaxHealth * 0.02f);
        }
    }
}
