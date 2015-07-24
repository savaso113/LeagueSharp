using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;
using Color = System.Drawing.Color;

namespace TheCassiopeia
{
    class CassR : Skill
    {
        public bool UltOnKillable;
        public int MinTargetsFacing;
        public int MinTargetsNotFacing;
        public int GapcloserUltHp;
        public int MinHealth;
        public int PanicModeHealth;
        public MenuItem BurstMode;

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

        public Commons.Prediction.PredictionOutput GetPrediction(Obj_AI_Hero target)
        {
            return Commons.Prediction.Prediction.GetPrediction(new Commons.Prediction.PredictionInput() { Aoe = true, Collision = false, Delay = Delay, From = ObjectManager.Player.ServerPosition, Radius = (float)(80 * Math.PI / 180), Range = Range, Type = Commons.Prediction.SkillshotType.SkillshotCone, Unit = target });
        }

        public override void Execute(Obj_AI_Hero target)
        {
            var pred = GetPrediction(target);
            if (pred.Hitchance < Commons.Prediction.HitChance.Low) return;

            var targets = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Range) && WillHit(enemy.Position, pred.CastPosition));
            var looking = targets.Count(trgt => trgt.IsFacingMe());

            if (looking >= MinTargetsFacing || targets.Count() >= MinTargetsNotFacing || UltOnKillable && Provider.GetComboDamage(target) > target.Health && target.IsFacingMe() && target.HealthPercent > MinHealth || PanicModeHealth > ObjectManager.Player.HealthPercent || BurstMode.IsActive())
            {
                Cast(pred.CastPosition);
            }
        }

        public override void Draw()
        {
            if (!Provider.Target.IsValidTarget(Range))
            {
                Drawing.DrawText(200, 200, Color.Red, 0 + " / " + 0);
                return;
            }

            var pred = GetPrediction(Provider.Target);
            if (pred.Hitchance < Commons.Prediction.HitChance.Low) return;

            var targets = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Range) && WillHit(enemy.Position, pred.CastPosition));
            var looking = targets.Count(trgt => trgt.IsFacingMe());

            Drawing.DrawText(200, 200, Color.Red, targets.Count() + " / " + looking);
        }

        public override void Gapcloser(ComboProvider combo, ActiveGapcloser gapcloser)
        {
            if (ObjectManager.Player.HealthPercent < GapcloserUltHp)
            {
                var pred = GetPrediction(gapcloser.Sender);
                if (pred.Hitchance < Commons.Prediction.HitChance.Low) return;

                Cast(pred.CastPosition);
            }
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
