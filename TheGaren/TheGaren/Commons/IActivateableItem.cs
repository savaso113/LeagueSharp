using LeagueSharp;
using LeagueSharp.Common;

namespace TheGaren.Commons
{
    public interface IActivateableItem
    {
        void Initialize(Menu menu);
        string GetDisplayName();
        void Update(Obj_AI_Hero target);
        void Use(Obj_AI_Base target);
    }
}
