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
        private Vector3 _ultPos;
        private float _ultPosTime;

        public CassR(SpellSlot slot)
            : base(slot)
        {
            SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            var pred = GetPrediction(target, true, Range);
            var targetsLooking = pred.AoeTargetsHit.Count(enemy => enemy.IsFacing(ObjectManager.Player));

            if (targetsLooking >= MinTargets)
            {
                _ultPos = pred.CastPosition;
                _ultPosTime = Game.Time;
            }
            else if (Provider.GetComboDamage(target) > target.Health && target.IsFacing(ObjectManager.Player))
            {
                _ultPos = pred.CastPosition;
                _ultPosTime = Game.Time;
            }

            if (Game.Time - _ultPosTime < 0.5f)
                Cast(_ultPos);
        }

        public override void Draw()
        {
            if (Game.Time - _ultPosTime < 10)
                Render.Circle.DrawCircle(_ultPos, 200, Color.Pink);

            Drawing.DrawText(500, 500, Color.Red, (_ultPos.X - Game.CursorPos.X) + " / " + (_ultPos.Y - Game.CursorPos.Y) + " / " + (_ultPos.Z - Game.CursorPos.Z) + " / ");
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
