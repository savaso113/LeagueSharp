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
    class TwitchR : Skill
    {
        public bool OnlyInTeamfight;
        public Circle DrawRange;

        public TwitchR(Spell spell)
            : base(spell)
        {
            HarassEnabled = false;
        }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            if (OnlyInTeamfight && !(HeroManager.AllHeroes.Count(hero => hero.Distance(ObjectManager.Player) < 2500) > HeroManager.AllHeroes.Count / 2f) && !force) return;
            SafeCast();
        }

        public override int GetPriority()
        {
            return 1;
        }

        public override void Draw()
        {
            if (DrawRange.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 975, DrawRange.Color);
        }
    }
}
