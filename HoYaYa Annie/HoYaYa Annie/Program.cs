﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;



namespace Aprendendo
{
    class Program
    {

        public const string ChampionName = "Annie";
        public static Orbwalking.Orbwalker _orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Menu _menu;
        private static Obj_AI_Hero Player;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 600f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 625f);
            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.50f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.20f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            _menu = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            _menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));



        }
    }
}
