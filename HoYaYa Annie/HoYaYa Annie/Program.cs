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
        public static float DoingCombo;
        public static Menu _menu;
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Obj_AI_Base _Tibbers;
        

        #endregion

        #region BuffStun
        
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
            if (Player.BaseSkinName != "Annie") { return; }

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
            Game.PrintChat("<font color='#ab82ff'>HoYaYa Annie</font color> <font color='#6dc066'>Loaded!</font> \n Made by: Silva & iPobre");
           


            #region Menu
            _menu = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            _menu.AddSubMenu(new Menu("Orbwalker", "orbwalker"));
            _orbwalker = new Orbwalking.Orbwalker(_menu.SubMenu("orbwalker"));

            _menu.AddSubMenu(new Menu("Combo Settings", "combo"));
            _menu.SubMenu("combo").AddItem(new MenuItem("qcombo", "Use (Q) in Combo").SetValue(false));
            _menu.SubMenu("combo").AddItem(new MenuItem("wcombo", "Use (W) in Combo").SetValue(false));
            _menu.SubMenu("combo").AddItem(new MenuItem("Combo", "Targets needed to R(stun)")).SetValue(new Slider(4, 5, 1));


            _menu.AddSubMenu(new Menu("Farming", "Farm.mode"));
            _menu.SubMenu("Farm.mode").AddItem(new MenuItem("farmq", "Use Q Last Hit").SetValue(false));
            _menu.SubMenu("Farm.mode").AddItem(new MenuItem("farmw", "Use W Lane Clear").SetValue(false));
            _menu.SubMenu("Farm.mode").AddItem(new MenuItem("notfarmstun", "Not Spell WHEN Stun").SetValue(true));

            _menu.AddSubMenu(new Menu("Anti GapCloser", "gapcloser"));
            _menu.SubMenu("gapcloser").AddItem(new MenuItem("qgap", "Evite Gap with (Q)").SetValue(true));
            _menu.SubMenu("gapcloser").AddItem(new MenuItem("egap", "Use (E) When Gapclosed").SetValue(true));


            _menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            _menu.SubMenu("Drawings").AddItem(new MenuItem("ComboDamage", "Drawings on HPBar").SetValue(true));

            _menu.AddSubMenu(new Menu("Stun Charger", "load.fast.stun"));
            _menu.SubMenu("load.fast.stun").AddItem(new MenuItem("load.fast.enabled", "Load Enabled").SetValue(true));
            _menu.SubMenu("load.fast.stun").AddItem(new MenuItem("load.fast.base", "Charger in Fountain").SetValue(true));
            _menu.SubMenu("load.fast.stun.").AddItem(new MenuItem("load.fast.lane", "Charger in Lane").SetValue(true));
            _menu.SubMenu("load.fast.stun.").AddItem(new MenuItem("LanePassivePercent", "Min Mana % to Charge").SetValue(new Slider(60)));

            _menu.AddSubMenu(new Menu("misc", "misc"));
            _menu.SubMenu("misc").AddItem(new MenuItem("Pcast", "Package Cast (dont work)").SetValue(false));


            _menu.AddToMainMenu(); 


        }

            #endregion

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            // use Q against gap closer
            var target = gapcloser.Sender;
            if (Q.IsReady() && StunCount == 4 && _menu.Item("qgap").GetValue<bool>())
            {
                Q.Cast(target);
            }
            if (E.IsReady() && _menu.Item("egap").GetValue<bool>())
            {
                E.Cast();
            }
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

        private static void ChargeStun()
        {
            if (StunCount == 4 || ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            if (_menu.Item("load.fast.base").GetValue<bool>() && ObjectManager.Player.InFountain())
            {
                if (E.IsReady())
                {
                    E.Cast();
                    return;
                }

                if (W.IsReady())
                {
                    W.Cast(Game.CursorPos);
                }
                return;
            }

            if (_menu.Item("load.fast.lane").GetValue<bool>() && E.IsReady() &&
                ObjectManager.Player.ManaPercentage() >= _menu.Item("LanePassivePercent").GetValue<Slider>().Value)
            {
                E.Cast();
            }
        }

        #region Lane Clear Area
        public static void LaneClear()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var jungleMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);
            minions.AddRange(jungleMinions);
            if (StunCount == 4 && _menu.Item("notfarmstun").GetValue<bool>()) { return; }

            if (_menu.Item("farmw").GetValue<bool>() && W.IsReady() && minions.Count != 0)
            {
                W.Cast(W.GetLineFarmLocation(minions).Position);
            }                     
            else if (_menu.Item("farmq").GetValue<bool>() && Q.IsReady() && minions.Count >= 0)
            {
                foreach (var minion in
                from minion in
                    minions.OrderByDescending(Minions => Minions.MaxHealth)
                        .Where(minion => minion.IsValidTarget(Q.Range))
                let predictedHealth = Q.GetHealthPrediction(minion)
                where
                    predictedHealth < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) * 0.85 &&
                    predictedHealth > 0
                select minion)
                {
                    Q.CastOnUnit(minion, _menu.Item("farmq").GetValue<bool>());
                }
            }

        }
        #endregion

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = Environment.TickCount > DoingCombo;
        }
  
  
            HaveStun = StunCount();
        
        public static void Combo()
        var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (!HaveTibers && R.IsReady())
                {
                    if (Combo && HaveStun && target.CountEnemiesInRange(400) > 1)
                        R.Cast(target, true, true);
                    else if (_menu.Item("Combo").GetValue<Slider>().Value > 0 && _menu.Item("Combo").GetValue<Slider>().Value <= target.CountEnemiesInRange(300))
                        R.Cast(target, true, true);
                    else if (Combo && !W.IsReady() && !Q.IsReady()
                        && Q.GetDamage(target) < target.Health
                        && (target.CountEnemiesInRange(400) > 1 || R.GetDamage(target) + Q.GetDamage(target) > target.Health))
                        R.Cast(target, true, true);
                    else if (Combo && Q.GetDamage(target) < target.Health)
                        if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) ||
                                     target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) ||target.HasBuffOfType(BuffType.Taunt))
                        {
                            R.Cast(target, true, true);
                        }
                }
                if (W.IsReady() && (Farm || Combo))
                {
                    if (Combo && HaveStun && target.CountEnemiesInRange(250) > 1)
                        W.Cast(target, true, true);
                    else if (!Q.IsReady())
                        W.Cast(target, true, true);
                    else if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Charm) || 
                    target.HasBuffOfType(BuffType.Fear) ||target.HasBuffOfType(BuffType.Taunt))
                    {
                        W.Cast(target, true, true);
                    }
                }
                if (Q.IsReady() && (Farm || Combo))
                {
                    if (HaveStun && Combo && target.CountEnemiesInRange(400) > 1 && (W.IsReady() || R.IsReady()))
                    {
                        return;
                    }
                    else
                        Q.Cast(target, true);
                }
            }

        public static bool CastIncendiar(Obj_AI_Base target)
        {
            if (target == null) return false;
            int _dmg_Incediar_Base = 50 + (Player.Level * 20);

            if (target.Health <= _dmg_Incediar_Base)
            {
                return true;
            }
            else if (target.Health == _dmg_Incediar_Base)
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

            private static int GetEnemiesInRange(Vector3 pos, float range)
            {
                //var Pos = pos;
                return
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.Team != ObjectManager.Player.Team)
                        .Count(hero => Vector3.Distance(pos, hero.ServerPosition) <= range);
            }

        private static bool HaveTibers
        {
            get { return ObjectManager.Player.HasBuff("infernalguardiantimer"); }
        }
            
            
    }
}

