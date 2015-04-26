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
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot Ignite;
        public static SpellSlot Flash;
        public static Menu _menu;
        private static Obj_AI_Hero Player;
        

        #endregion



        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }



        private static void Game_OnGameLoad(EventArgs args)
        {

            #region Create Spells

            Q = new Spell(SpellSlot.Q, 650);
            Q.SetTargetted(0.25f, 1400);

            W = new Spell(SpellSlot.W, 625);
            W.SetSkillshot(0.6f, (float)(50 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 600);
            R.SetSkillshot(0.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            #endregion

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Player = ObjectManager.Player;

            if (Player.ChampionName != ChampionName)
            {

                Game.PrintChat(string.Format("<font color='#736AFF'>HoYaYa Annie</font> <font color='#00FF00'>Loaded</font> \n Created by: Silva & iPobre"));
                return;                           
            }
             

            


           
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
        }
            
        private static void LaneClear()
        {

            {

            }
        }
       
        private static void Combo()
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                
                {
                    if (_menu.Item("combofull").GetValue<KeyBind>().Active)
                    {
                        Q.Cast(target,false,false);
                        W.Cast(target, false, false);
                        
                    }

                    var minenemy = _menu.Item("rcombo").GetValue<Slider>().Value;
                    var rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                    if (rtarget != null && ObjectManager.Player.Distance(rtarget, false) <= R.Range && (minenemy <= minenemy) && R.IsReady())
                    {
                        R.Cast(target, false,false);
                    }
                }

            }
        private static void FlashCombo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            {
                if (_menu.Item("FlashCombo").GetValue<bool>())              
                {
                    
                }
            }
        }
        

            public static int GetPassiveStacks()
            {
                var buffs = Player.Buffs.Where(buff => (buff.Name.ToLower() == "pyromania" || buff.Name.ToLower() == "pyromania_particle"));
                if (buffs.Any())
                {
                    var buff = buffs.First();
                    if (buff.Name.ToLower() == "pyromania_particle")
                        return 4;
                    else
                        return buff.Count;
                }
                return 0;
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

