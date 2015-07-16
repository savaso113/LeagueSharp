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
    class CassW: Skill
    {
        private CassQ _q;

        public CassW(SpellSlot slot)
            : base(slot)
        {
            SetSkillshot(0.5f, Instance.SData.CastRadius, Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);
        }

        public override void Initialize(ComboProvider combo)
        {
            _q = combo.GetSkill<CassQ>();
            base.Initialize(combo);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            if (_q.Instance.State == SpellState.Cooldown && (!target.IsPoisoned() && !Provider.IsMarked(target)))
            {
                Cast(target);
            }
        }

        public override int GetPriority()
        {
            return 1;
        }
    }
}
