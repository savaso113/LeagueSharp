using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheGaren
{
    // ReSharper disable once InconsistentNaming
    static class AAHelper
    {
        public static bool JustFinishedAutoattack;
        public static bool WillAutoattackSoon;
        private static float _lastAATime;
        private static bool _windingUp;
        private static readonly Obj_AI_Hero _player;

        static AAHelper()
        {
            _player = ObjectManager.Player;
            Game.OnUpdate += Update;
        }

        private static void Update(EventArgs args)
        {
            JustFinishedAutoattack = _windingUp && !_player.IsWindingUp;
            if (JustFinishedAutoattack)
                _lastAATime = Game.Time;
            WillAutoattackSoon = (_windingUp = _player.IsWindingUp) || Game.Time - _lastAATime > _player.AttackDelay / 2f;
        }
    }
}
