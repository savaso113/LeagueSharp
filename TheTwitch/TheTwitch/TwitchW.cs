using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheTwitch.Commons.ComboSystem;

namespace TheTwitch
{
    class TwitchW : Skill
    {
        public int MinFarmMinions;
        public Circle DrawRange;
        private bool _afterAttack;

        public TwitchW(Spell spell)
            : base(spell)
        {
            Spell.SetSkillshot(0.25f, 275f, 1400f, false, SkillshotType.SkillshotCircle);
            IsAreaOfEffect = true;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                _afterAttack = true;
            }
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            base.Update(mode, combo, target);
            if (_afterAttack) _afterAttack = false;
        }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            if (_afterAttack || force)
            {
                SafeCast(target);
            }
        }

        public override void Draw()
        {
            if (DrawRange.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 950, DrawRange.Color);
        }

        public override void LaneClear(ComboProvider combo, Obj_AI_Hero target)
        {
            var location = Spell.GetCircularFarmLocation(MinionManager.GetMinions(950, MinionTypes.All, MinionTeam.NotAlly));
            if (location.MinionsHit >= MinFarmMinions)
                SafeCast(location.Position);
        }

        public override void Gapcloser(ComboProvider combo, ActiveGapcloser gapcloser)
        {
            Cast(gapcloser.Sender, true);
        }

        public override int GetPriority()
        {
            return 3;
        }
    }
}
