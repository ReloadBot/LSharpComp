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
        public static SummonerSpellManager summonerSpellManager;
        public static ItemManager itemManager;
        public static Menu _menu;
        private static Obj_AI_Hero Player;
        #endregion


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }



        private static void Game_OnGameLoad(EventArgs args)
        {

            Game.OnUpdate += Game_OnGameUpdate;
            Player = ObjectManager.Player;

            if (Player.ChampionName != ChampionName)
            {

                InitializeSpells();
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
            comboMenu.AddItem(new MenuItem("FlashCombo", "Flash To Combo !!").SetValue(new KeyBind()));
            comboMenu.AddItem(new MenuItem("combofull", "Combo !!").SetValue(true));
            comboMenu.AddItem(new MenuItem("qcombo", "(Q) Combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("rcombo", "(R) When ").SetValue(new Slider(3,0,5)));
            _menu.AddSubMenu(comboMenu);

            _menu.AddToMainMenu();

            

        }



        private static void Game_OnGameUpdate(EventArgs args)

        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
        }

        
        public static void FlashCombo()

        {
            var UseFlashCombo = _menu.Item("FlashCombo").GetValue<KeyBind>().Active;
            var FlashComboMinEnemies = _menu.Item("rcombo").GetValue<Slider>();


            if (!UseFlashCombo)
                return;

            int qtPassiveStacks = GetPassiveStacks();


            if (((qtPassiveStacks == 3 && E.IsReady()) || qtPassiveStacks == 4) && summonerSpellManager.IsReadyFlash() && R.IsReady())
            {


                
                if (enemies.Any())
                {


                    var enemy = enemies.First();
                    if (DevHelper.CountEnemyInPositionRange(enemy.ServerPosition, 250) >= FlashComboMinEnemies)
                    {
                        var predict = R.GetPrediction(enemy, true).CastPosition;

                        if (qtPassiveStacks == 3)
                        {
                            E.Cast(true);
                        }

                        summonerSpellManager.CastFlash(predict);


                        if (R.IsReady())
                            R.Cast(predict, true);

                        if (W.IsReady())
                            W.Cast(predict, true);

                        if (E.IsReady())
                            E.Cast();

                    }
                }
            }
        }

        

        private static void Combo()
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                
                {
                    if (_menu.Item("combofull").GetValue<bool>());
                    {
                        Q.Cast(true);
                        W.Cast(true);

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

            private static void InitializeSpells()
            {
                #region Create Spells
                summonerSpellManager = new SummonerSpellManager();
                itemManager = new ItemManager();

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
            }
            #region Summuner Spell by InjectionDev
            public class SummonerSpellManager
            {
                public SpellSlot IgniteSlot = SpellSlot.Unknown;
                public SpellSlot FlashSlot = SpellSlot.Unknown;
                public SpellSlot BarrierSlot = SpellSlot.Unknown;
                public SpellSlot HealSlot = SpellSlot.Unknown;
                public SpellSlot ExhaustSlot = SpellSlot.Unknown;


                public SummonerSpellManager()
                {
                    IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
                    FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
                    BarrierSlot = ObjectManager.Player.GetSpellSlot("SummonerBarrier");
                    HealSlot = ObjectManager.Player.GetSpellSlot("SummonerHeal");
                    ExhaustSlot = ObjectManager.Player.GetSpellSlot("SummonerExhaust");
                }

                public bool CastIgnite(Obj_AI_Hero target)
                {
                    if (IsReadyIgnite())
                        return ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
                    else
                        return false;
                }

                public bool CastFlash(Vector3 position)
                {
                    if (IsReadyFlash())
                        return ObjectManager.Player.Spellbook.CastSpell(FlashSlot, position);
                    else
                        return false;
                }

                public bool CastBarrier()
                {
                    if (IsReadyBarrier())
                        return ObjectManager.Player.Spellbook.CastSpell(BarrierSlot);
                    else
                        return false;
                }

                public bool CastHeal()
                {
                    if (IsReadyHeal())
                        return ObjectManager.Player.Spellbook.CastSpell(HealSlot);
                    else
                        return false;
                }

                public bool CastExhaust(Obj_AI_Hero target)
                {
                    if (IsReadyExhaust())
                        return ObjectManager.Player.Spellbook.CastSpell(ExhaustSlot, target);
                    else
                        return false;
                }

                public bool IsReadyIgnite()
                {
                    return (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready);
                }

                public bool IsReadyFlash()
                {
                    return (FlashSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready);
                }

                public bool IsReadyBarrier()
                {
                    return (BarrierSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(BarrierSlot) == SpellState.Ready);
                }

                public bool IsReadyHeal()
                {
                    return (HealSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(HealSlot) == SpellState.Ready);
                }

                public bool IsReadyExhaust()
                {
                    return (ExhaustSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(ExhaustSlot) == SpellState.Ready);
                }

                public bool CanKillIgnite(Obj_AI_Hero target)
                {
                    return IsReadyIgnite() && target.Health < ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                }

                public double GetIgniteDamage(Obj_AI_Hero target)
                {
                    if (IsReadyIgnite())
                        return ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                    else
                        return 0;
                }
            }
            #endregion
            #region Dev Helper
            public static class DevHelper
    {

        public static List<Obj_AI_Hero> GetEnemyList()
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid)
                .OrderBy(x => ObjectManager.Player.ServerPosition.Distance(x.ServerPosition))
                .ToList();
        }

        public static List<Obj_AI_Hero> GetAllyList()
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsAlly && x.IsValid)
                .OrderBy(x => ObjectManager.Player.ServerPosition.Distance(x.ServerPosition))
                .ToList();
        }

        public static Obj_AI_Hero GetNearestEnemy(this Obj_AI_Base unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid && x.NetworkId != unit.NetworkId)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        public static Obj_AI_Hero GetNearestAlly(this Obj_AI_Base unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsAlly && x.IsValid && x.NetworkId != unit.NetworkId)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        public static Obj_AI_Hero GetNearestEnemyFromUnit(this Obj_AI_Base unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        public static float GetHealthPerc(this Obj_AI_Base unit)
        {
            return (unit.Health / unit.MaxHealth) * 100;
        }

        public static float GetManaPerc(this Obj_AI_Base unit)
        {
            return (unit.Mana / unit.MaxMana) * 100;
        }

        public static void SendMovePacket(this Obj_AI_Base v, Vector2 point)
        {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(point.X, point.Y)).Send();
        }

        public static bool IsUnderEnemyTurret(this Obj_AI_Base unit)
        {
            IEnumerable<Obj_AI_Turret> query;

            if (unit.IsEnemy)
            {
                query = ObjectManager.Get<Obj_AI_Turret>()
                    .Where(x => x.IsAlly && x.IsValid && !x.IsDead && unit.ServerPosition.Distance(x.ServerPosition) < 950);
            }
            else
            {
                query = ObjectManager.Get<Obj_AI_Turret>()
                    .Where(x => x.IsEnemy && x.IsValid && !x.IsDead && unit.ServerPosition.Distance(x.ServerPosition) < 950);
            }

            return query.Any();
        }

        public static void Ping(Vector3 pos)
        {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos.X, pos.Y, 0, 0, Packet.PingType.Normal)).Process();
        }

        public static float GetDistanceSqr(Obj_AI_Base source, Obj_AI_Base target)
        {
            return Vector2.DistanceSquared(source.ServerPosition.To2D(), target.ServerPosition.To2D());
        }

        //public static bool IsFacing(this Obj_AI_Base source, Obj_AI_Base target)
        //{
        //    if (!source.IsValid || !target.IsValid)
        //        return false;

        //    if (source.Path.Count() > 0 && source.Path[0].Distance(target.ServerPosition) < target.Distance(source))
        //        return true;
        //    else
        //        return false;
        //}

        public static bool IsKillable(this Obj_AI_Hero source, Obj_AI_Base target, IEnumerable<SpellSlot> spellCombo)
        {
            return Damage.GetComboDamage(source, target, spellCombo) * 0.9 > target.Health;
        }

        public static int CountEnemyInPositionRange(Vector3 position, float range)
        {
            return GetEnemyList().Where(x => x.ServerPosition.Distance(position) <= range).Count();
        }

        private static readonly string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianq", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };
        private static readonly string[] NoAttacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        private static readonly string[] Attacks = { "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };

        public static bool IsAutoAttack(string spellName)
        {
            return (spellName.ToLower().Contains("attack") && !NoAttacks.Contains(spellName.ToLower())) || Attacks.Contains(spellName.ToLower());
        }

        public static bool IsMinion(AttackableUnit unit, bool includeWards = false)
        {
            if (unit is Obj_AI_Minion)
            {
                var minion = unit as Obj_AI_Minion;
                var name = minion.BaseSkinName.ToLower();
                return name.Contains("minion") || (includeWards && (name.Contains("ward") || name.Contains("trinket")));
            }
            else
                return false;
        }

        public static float GetRealDistance(GameObject unit, GameObject target)
        {
            return unit.Position.Distance(target.Position) + unit.BoundingRadius + target.BoundingRadius;
        }
    }

#endregion
            #region Item Manager by InjectionDev
            public class ItemManager
    {
        private List<ItemDTO> ItemDTOList;


        public ItemManager()
        {
            this.ItemDTOList = new List<ItemDTO>();
            this.InitiliazeItemList();
        }

        public bool IsItemReady(ItemName pItemName)
        {
            return this.ItemDTOList.Where(x => x.ItemName == pItemName).First().Item.IsReady();
        }

        public void UseItem(ItemName pItemName, Obj_AI_Hero target = null)
        {
            var item = this.ItemDTOList.Where(x => x.ItemName == pItemName).First().Item;

            if (!item.IsReady())
                return;

            if (target == null)
                item.Cast();
            else
                item.Cast(target);
        }

        private void InitiliazeItemList()
        {
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3144, 450),
                ItemName = ItemName.BilgewaterCutlass
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3188, 750),
                ItemName = ItemName.BlackfireTorch
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3153, 450),
                ItemName = ItemName.BladeOfTheRuineKing
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3128, 750),
                ItemName = ItemName.DeathfireGrasp
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3146, 700),
                ItemName = ItemName.HextechGunblade
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3042, int.MaxValue),
                ItemName = ItemName.Muramana
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3074, 400),
                ItemName = ItemName.RavenousHydra
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3077, 400),
                ItemName = ItemName.Tiamat
            });
            this.ItemDTOList.Add(new ItemDTO
            {
                Item = new Items.Item(3142, (int)(ObjectManager.Player.AttackRange * 2)),
                ItemName = ItemName.YoumuusGhostblade
            });
        }

    }

    public class ItemDTO
    {
        public Items.Item Item { get; set; }
        public ItemName ItemName { get; set; }
    }

    public enum ItemName
    {
        BilgewaterCutlass,
        BlackfireTorch,
        BladeOfTheRuineKing,
        DeathfireGrasp,
        HextechGunblade,
        Muramana,
        RavenousHydra,
        Tiamat,
        YoumuusGhostblade,

    }



        #endregion
    }
}

