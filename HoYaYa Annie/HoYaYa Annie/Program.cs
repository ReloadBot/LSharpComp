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
            _menu.SubMenu("combo").AddItem(new MenuItem("rcombo", "Use (R) in Combo").SetValue(false));
            _menu.SubMenu("combo").AddItem(new MenuItem("flashCombo", "Targets needed to Flash -> R(stun)")).SetValue(new Slider(4, 5, 1));


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

            _menu.AddSubMenu(new Menu("Passive in Base", "load.fast.stun.base"));
            _menu.SubMenu("load.fast.stun.base").AddItem(new MenuItem("load.fast.enabled", "Load Enabled").SetValue(true));
            _menu.SubMenu("load.fast.stun.base").AddItem(new MenuItem("load.fast.cast.w", "Cast W").SetValue(true));
            _menu.SubMenu("load.fast.stun.base").AddItem(new MenuItem("load.fast.cast.e", "Cast E").SetValue(true));

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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var flashRtarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    _orbwalker.SetAttack(false);
                    Combo(target, flashRtarget);
                    _orbwalker.SetAttack(true);
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


       
    private static void Combo(Obj_AI_Base target, Obj_AI_Base flashRtarget)

            {
        if ((target == null && flashRtarget == null) || Environment.TickCount < DoingCombo ||
            (!Q.IsReady() && !W.IsReady() && !R.IsReady()))
        {
            return;
        }
                
        var useQ = _menu.Item("qcombo").GetValue<bool>();
        var useW = _menu.Item("wcombo").GetValue<bool>();
        var useR = _menu.Item("rcombo").GetValue<bool>();
       
        switch (StunCount)
              
        {
            case 3:
                if (target == null)
                {
                    return;
                }
                if (Q.IsReady() && useQ)
                {
                    DoingCombo = Environment.TickCount;
                    Q.Cast(target, _menu.Item("PCast").GetValue<bool>());
                    Utility.DelayAction.Add(
                        (int)(ObjectManager.Player.Distance(target, false) / Q.Speed * 1000 - Game.Ping / 2.0) +
                        250, () =>
                        {
                            if (R.IsReady() &&
                                !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health))
                            {
                                
                                R.Cast(target, false, true);
                            }
                        });
                }
                else if (W.IsReady() && useW)
                {
                    W.Cast(target);
                    DoingCombo = Environment.TickCount + 250f;
                }

                break;

            case 4:
                if (ObjectManager.Player.Spellbook.CanUseSpell(Flash) == SpellState.Ready && R.IsReady() &&
                    target == null)
                {
                    var position = R1.GetPrediction(flashRtarget, true).CastPosition;

                    if (ObjectManager.Player.Distance(position) > 600 &&
                        GetEnemiesInRange(flashRtarget.ServerPosition, 250) >=
                        _menu.Item("flashCombo").GetValue<Slider>().Value)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Flash, position);
                    }

                    Items.UseItem(3128, flashRtarget);
                    R.Cast(flashRtarget,false,true);

                    if (W.IsReady() && useW)
                    {
                        W.Cast(flashRtarget, false, true);
                    }
                    else if (Q.IsReady() && useQ)
                    {
                        Q.Cast(flashRtarget, _menu.Item("PCast").GetValue<bool>());
                    }
                }
                else if (target != null)
                {
                    if (R.IsReady() && useR &&
                        !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.6 > target.Health))
                    {
                        R.Cast(target, false, true);
                    }

                    if (W.IsReady() && useW)
                    {
                        W.Cast(target, false, true);
                    }

                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(target, _menu.Item("PCast").GetValue<bool>());
                    }
                }
                break;

            default:
                if (Q.IsReady() && useQ)
                {
                    Q.Cast(target, _menu.Item("PCast").GetValue<bool>());
                }

                if (W.IsReady() && useW)
                {
                    W.Cast(target, false, true);
                }

                break;


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

        
            
            
    }
}

