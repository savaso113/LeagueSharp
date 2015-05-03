using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheGaren.ComboSystem
{
    class ComboProvider
    {
        protected List<Skill> Skills;
        protected Skill TotalControl { get; private set; }
        private bool _totalControl;
        private bool _cancelUpdate;
        private Orbwalking.Orbwalker _orbwalker;
        private IMainContext _context;

        /// <summary>
        /// Represents a "combo" and it's logic. Manages skill logic.
        /// </summary>
        public ComboProvider(List<Skill> skills)
        {
            Skills = skills;
        }


        /// <summary>
        /// call to init all stuffs. Menu has to exist at that time
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(IMainContext context)
        {
            Skills.ForEach(skill => skill.Initialize(context, this));
        }

        public float GetComboDamage(Obj_AI_Hero enemy)
        {
            return Skills.Sum(skill => skill.ComboEnabled ? skill.GetDamage(enemy) : 0);
        }

        /// <summary>
        /// override in sub class to add champion combo logic. for example Garen has a fixed combo, but wants to do W not in order, but when he gets damage.
        /// In this example you would override Update and have a seperate logic for W instead of adding it to the skill routines.
        /// </summary>
        /// <param name="context"></param>
        public virtual void Update(IMainContext context)
        {

            _context = context;
            _orbwalker = context.GetOrbwalker();
            if (_totalControl)
            {
                TotalControl.Update(_orbwalker.ActiveMode, context, this);

                if (!TotalControl.NeedsControl())
                {
                    TotalControl.TryTerminate(_context);
                    _totalControl = false;
                    Update(context);
                    return;
                }

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var skill in Skills.Where(item => item.GetPriority() > TotalControl.GetPriority()))
                {
                    skill.Update(_orbwalker.ActiveMode, context, this);
                }
            }
            else
            {
                foreach (var item in Skills)
                {
                    if (_cancelUpdate)
                    {
                        _cancelUpdate = false;
                        return;
                    }
                    item.Update(_orbwalker.ActiveMode, context, this);
                }
            }

        }

        public bool GrabControl(Skill skill)
        {
            if (_totalControl && TotalControl == skill)
                return true;
            if (_totalControl && TotalControl.GetPriority() < skill.GetPriority())
            {
                TotalControl.TryTerminate(_context);
                TotalControl = skill;
                _cancelUpdate = true;
                return true;
            }
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var currentSkill in Skills)
            {
                if (skill != currentSkill && currentSkill.NeedsControl() && currentSkill.GetEnabled(_context.GetOrbwalker().ActiveMode) && currentSkill.GetPriority() >= skill.GetPriority())
                {
                    return false;
                }
            }
            _totalControl = true;
            TotalControl = skill;
            _cancelUpdate = true;
            return true;
        }

        public void SetEnabled<T>(Orbwalking.OrbwalkingMode mode, bool enabled) where T : Skill
        {
            foreach (var skill in Skills.Where(skill => skill.GetType() == typeof(T)))
            {
                skill.SetEnabled(mode, enabled);
            }
        }

        public T GetSkill<T>() where T : Skill
        {
            return (T)Skills.FirstOrDefault(skill => skill is T);
        }
    }
}
