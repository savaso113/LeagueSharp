using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheTwitch.Commons
{
    interface IActivateableItem
    {
        void Initialize(Menu menu);
        string GetDisplayName();
        void Update(Obj_AI_Hero target);
    }
}
