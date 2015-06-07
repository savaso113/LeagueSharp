using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheTwitch.Commons;
using TheTwitch.Commons.ComboSystem;

namespace TheTwitch
{
    class TwitchQ : Skill
    {
        public Circle DrawRange;

        public TwitchQ(Spell spell)
            : base(spell)
        {
            OnlyUpdateIfTargetValid = false;
            HarassEnabled = false;
        }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            SafeCast();
        }

        public override void Draw()
        {
            if (!DrawRange.Active) return;
            var stealthTime = GetRemainingTime();
            if (stealthTime > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GetRemainingTime() * ObjectManager.Player.MoveSpeed, DrawRange.Color);
            }
        }

        private float GetRemainingTime()
        {
            var buff = ObjectManager.Player.GetBuff("TwitchHideInShadows");
            if (buff == null && Spell.GetState() == SpellState.Ready) return Spell.Level + 3 + 1.5f;
            if (buff == null) return 0;
            return buff.EndTime - Game.Time;
        }

        public override int GetPriority()
        {
            return 2;
        }
    }
}
