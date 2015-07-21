using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using TheTwitch.Commons.ComboSystem;

namespace TheTwitch
{
    class TwitchCombo : ComboProvider
    {
        public TwitchCombo(float targetSelectorRange, IEnumerable<Skill> skills, Orbwalking.Orbwalker orbwalker) : base(targetSelectorRange, skills, orbwalker)
        {
        }

        public TwitchCombo(float targetSelectorRange, Orbwalking.Orbwalker orbwalker, params Skill[] skills) : base(targetSelectorRange, orbwalker, skills)
        {
        }


        public override bool ShouldBeDead(LeagueSharp.Obj_AI_Base target, float additionalSpellDamage = 0f)
        {
            return base.ShouldBeDead(target, TwitchE.GetRemainingPoisonDamageMinusRegeneration(target));
        }
    }
}
