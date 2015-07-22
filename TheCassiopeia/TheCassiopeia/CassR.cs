using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheCassiopeia.Commons.ComboSystem;
using Color = System.Drawing.Color;

namespace TheCassiopeia
{
    class CassR : Skill
    {
        public bool UltOnKillable;
        public int MinTargets;

        public CassR(SpellSlot slot)
            : base(slot)
        {
            
        }

        public override void Initialize(ComboProvider combo)
        {
            Range = 825f;
            SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            base.Initialize(combo);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            var pred = GetPrediction(target, true);
            if (!WillHit(target, pred.CastPosition)) return;
            var targetsLooking = pred.AoeTargetsHit.Count(enemy => enemy.IsFacing(ObjectManager.Player));

            if (targetsLooking >= MinTargets || Provider.GetComboDamage(target) > target.Health && target.IsFacing(ObjectManager.Player))
            {
                Console.WriteLine(pred.CastPosition+" "+ObjectManager.Player.Position.Distance(pred.CastPosition));
                Cast(pred.CastPosition);
            }

                
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
