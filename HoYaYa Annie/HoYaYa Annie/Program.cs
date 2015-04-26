using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;




namespace YaYaAnnie //By Silva & iPobre
{
    class Program
    {
        #region

        public const string ChampionName = "Annie";
        public static Orbwalking.Orbwalker _orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static readonly List<SpellSlot> _SumList = new List<SpellSlot>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;
        public static SpellSlot Ignite;
        public static SpellSlot Flash;
        public static Menu _menu;
        private static Obj_AI_Hero Player;
        private static Obj_AI_Base _Tibbers;
        

        #endregion

        #region Buff
        public static int StunCount
        {
            get
            {
                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => buff.Name == "pyromania" || buff.Name == "pyromania_particle"))
                {
                    switch (buff.Name)
                    {
                        case "pyromania":
                            return buff.Count;
                        case "pyromania_particle":
                            return 4;
                    }
                }

                return 0;
            }
        }
        #endregion



        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }



        private static void Game_OnGameLoad(EventArgs args)
        {

            if (Player.ChampionName != ChampionName)
            {

                Game.PrintChat(string.Format("<font color='#736AFF'>HoYaYa Annie</font> <font color='#00FF00'>Loaded</font> Created by: Silva & iPobre"));

            }

            #region Create Spells

            Q = new Spell(SpellSlot.Q, 625f);
            Q.SetTargetted(0.15f, 1500f);

            W = new Spell(SpellSlot.W, 610f);
            W.SetSkillshot(0.15f, 75f, 1500f, false, SkillshotType.SkillshotCone);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 625);
            R.SetSkillshot(0.15f, 75f, 1500f, false, SkillshotType.SkillshotCircle);

            R1 = new Spell(SpellSlot.Unknown, 400f);
            R1.SetSkillshot(0.15f, 75f, 1500f, false, SkillshotType.SkillshotCircle);

            Ignite = Player.GetSpellSlot("SummonerDot");
            Flash = Player.GetSpellSlot("SummonerFlash");

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            SpellList.Add(R1);
            _SumList.Add(Ignite);
            _SumList.Add(Flash);
            #endregion

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Player = ObjectManager.Player;

            
            

            


           
            _menu = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            _menu.AddSubMenu(new Menu("Orbwalker", "orbwalker"));
            _orbwalker = new Orbwalking.Orbwalker(_menu.SubMenu("orbwalker"));

            var comboMenu = new Menu("Combo", "combo_menu");
            comboMenu.AddItem(new MenuItem("FlashCombo", "Flash To Combo !!").SetValue(false));
            comboMenu.AddItem(new MenuItem("combofull", "Combo !!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Toggle)));
           
           comboMenu.AddItem(new MenuItem("rcombo", "(R) When ").SetValue(new Slider(3,0,5)));
            _menu.AddSubMenu(comboMenu);

            var FarmMenu = new Menu("Farming", "farming_menu");
            FarmMenu.AddItem(new MenuItem("qfarm", "Farm With (Q)").SetValue(true));

            _menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("ComboDamage", "Drawings on HPBar").SetValue(true));

            _menu.AddSubMenu(new Menu("Passive in Base", "load.fast.stun.base"));
            _menu.SubMenu("load.fast.stun.base").AddItem(new MenuItem("load.fast.enabled", "Load Enabled").SetValue(true));
            _menu.SubMenu("load.fast.stun.base").AddItem(new MenuItem("load.fast.cast.w", "Cast W").SetValue(true));
            _menu.SubMenu("load.fast.stun.base").AddItem(new MenuItem("load.fast.cast.e", "Cast E").SetValue(true));


            _menu.AddToMainMenu();

            

        }



        private static void Game_OnGameUpdate(EventArgs args)

        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }

            // Passive in Base 
            if (_menu.Item("load.fast.enabled").GetValue<bool>() && ObjectManager.Player.InFountain() && StunCount != 4)
            {
                if (W.IsReady() || E.IsReady())
                {
                    if (_menu.Item("load.fast.cast.w").GetValue<bool>()) { W.Cast(Player.Position, false); }
                    if (_menu.Item("load.fast.cast.e").GetValue<bool>()) { E.Cast(); }
                }
            }

        }

          
        private static void LaneClear()
        {

            {

            }
        }


       
        private static void Combo()
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                var minenemy = _menu.Item("rcombo").GetValue<Slider>().Value;
                var rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                
                {
                    if (_menu.Item("combofull").GetValue<KeyBind>().Active)
                    {
                        Q.Cast(target,false,false);
                        W.Cast(target, false, false);
                        
                    }
                    if (rtarget != null && ObjectManager.Player.Distance(rtarget, false) <= R.Range && (minenemy <= minenemy) && R.IsReady())
                    {
                        R.Cast(rtarget, false,false);
                    }
                }

            }
        private static void FlashCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (StunCount == 4 && R.IsReady() && Q.IsReady() && W.IsReady() && target != null)
            {

                if (_menu.Item("FlashCombo").GetValue<bool>() && StunCount == 4 && R.IsReady() && _Tibbers == null)
                {
                    ObjectManager.Player.Spellbook.CastSpell(Flash, R1.GetPrediction(target, true).UnitPosition);
                }
                R.CastOnUnit(target, false);
                Q.CastOnUnit(target, false);
                W.CastOnUnit(target, false);
                _orbwalker.SetAttack(true);
                Player.IssueOrder(GameObjectOrder.AutoAttackPet, target);


            }
        }

        public static bool CastIncendiar(Obj_AI_Base _target)
        {
            if (_target == null) return false;
            int _dmg_Incediar_Base = 50 + (Player.Level * 20);

            if (_target.Health <= _dmg_Incediar_Base)
            {
                return true;
            }
            else if (_target.Health == _dmg_Incediar_Base)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        

           


            #region Drawing
            static void Drawing_OnDraw(EventArgs args)
            {
                foreach (var spell in SpellList)
                {
                    var menuItem = _menu.Item(spell.Slot + "Range").GetValue<Circle>();
                    if (menuItem.Active && spell.IsReady())
                    {
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, spell.IsReady() ? System.Drawing.Color.Green : System.Drawing.Color.Red);
                    }
                }

            }
            #endregion
            
            
    }
}

