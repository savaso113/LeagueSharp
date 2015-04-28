using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace TheGaren.ComboSystem
{
    /// <summary>
    /// The context needed by the ComboProvider, to access common data
    /// </summary>
    interface IMainContext
    {
        Menu GetRootMenu();
        Orbwalking.Orbwalker GetOrbwalker();
    }
}
