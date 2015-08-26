using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheKalista.Commons.ComboSystem;

namespace TheKalista
{
    class KalistaCombo : ComboProvider
    {
        public KalistaCombo(float targetSelectorRange, IEnumerable<Skill> skills, Orbwalking.Orbwalker orbwalker) : base(targetSelectorRange, skills, orbwalker)
        {
        }

        public KalistaCombo(float targetSelectorRange, Orbwalking.Orbwalker orbwalker, params Skill[] skills) : base(targetSelectorRange, orbwalker, skills)
        {
        }

        protected override LeagueSharp.Obj_AI_Hero SelectTarget()
        {
            var target = Orbwalker.GetTarget();
            if (target != null && target.Type == LeagueSharp.GameObjectType.obj_AI_Hero)
                return (Obj_AI_Hero)target;
            return base.SelectTarget();
        }
    }
}
