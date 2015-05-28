using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheBrand.ComboSystem;

namespace TheEkko
{
    class EkkoQ : Skill
    {
        public EkkoQ(Spell spell)
            : base(spell)
        {
            spell.SetSkillshot(0.5f, 60, 1200, false, SkillshotType.SkillshotLine);
            Console.WriteLine(spell.Instance.SData.LineWidth + " lw");
            Console.WriteLine(spell.Instance.SData.MissileSpeed + " cast");
        }


        public override void Cast(Obj_AI_Hero target, bool force = false, HitChance minChance = HitChance.Low)
        {
            if (HasBeenSafeCast() || target == null) return;
            var prediction = Spell.GetPrediction(target);
            if (prediction.Hitchance < minChance) return;
            SafeCast(() => Spell.Cast(prediction.CastPosition));

        }

        public override int GetPriority()
        {
            return 1;
        }
    }
}
