using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheCassiopeia.Commons.ComboSystem;

namespace TheCassiopeia
{
    class CassQ : Skill
    {
        private Vector3 _castPosition;
        public bool FastCombo;
        public bool RiskyCombo;

        public CassQ(SpellSlot slot)
            : base(slot)
        {
            SetSkillshot(0.75f, Instance.SData.CastRadius, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void Initialize(ComboProvider combo)
        {
            GameObject.OnCreate += OnCreateGameObject;
            Obj_AI_Base.OnProcessSpellCast += OnSpellCast;
            base.Initialize(combo);
        }

        private void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "CassiopeiaNoxiousBlast")
                _castPosition = args.End;
        }

        
        private void OnCreateGameObject(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Cassiopeia_Base_Q_Hit_Green.troy" && sender.Position.Distance(_castPosition) < 10 && FastCombo)
            {
                Console.WriteLine("hit");
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (enemy.ServerPosition.Distance(sender.Position) < Instance.SData.CastRadius + enemy.BoundingRadius - (RiskyCombo ? 0 : (enemy.MoveSpeed * 0.5f)))
                    {
                        Console.WriteLine("marked");
                        Provider.SetMarked(enemy, 0.5f);
                    }
                }
            }


        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            return base.GetDamage(enemy);
        }

        public override void Draw()
        {

        }

        public override void LaneClear(ComboProvider combo, Obj_AI_Hero target)
        {
            var farmLocation = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(900, MinionTypes.All, MinionTeam.NotAlly).Select(minion => minion.Position.To2D()).ToList(), Instance.SData.CastRadius, 850);
            if (farmLocation.MinionsHit > 0)
            {
                Cast(farmLocation.Position);
            }
            base.LaneClear(combo, target);
        }


        public override void Execute(Obj_AI_Hero target)
        {
            Cast(target);
        }

        public override int GetPriority()
        {
            return 2;
        }
    }
}
