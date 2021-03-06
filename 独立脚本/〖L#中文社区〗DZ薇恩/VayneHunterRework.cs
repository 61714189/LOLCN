﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace VayneHunterRework
{
    class VayneHunterRework
    {
        public static LXOrbwalker Orbwalker = new LXOrbwalker();
        public static Orbwalking.Orbwalker COrbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static String charName = "Vayne";
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static Vector3 AfterCond = Vector3.Zero;
        public static Obj_AI_Base current; // for tower farming
        public static Obj_AI_Base last; // for tower farming

        public VayneHunterRework()
        {
            CustomEvents.Game.OnGameLoad +=Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != charName) return;
            Cleanser.CreateQSSSpellList();
            Menu = new Menu("〖L#中文社区〗|薇恩之狩猎|", "VHRework", true);
            var lxMenu = new Menu("走砍", "LXOrb");
            //LXOrbwalker.AddToMenu(lxMenu);
            COrbwalker = new Orbwalking.Orbwalker(lxMenu);
            Menu.AddSubMenu(lxMenu);
            var tsMenu = new Menu("目标选择", "TargetSel");
            SimpleTs.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);
            Menu.AddSubMenu(new Menu("[VH] 连招", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "连招 Q 连招")).SetValue(true);
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "连招 E 连招").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "连招 R 连招").SetValue(false));
            Menu.SubMenu("Combo").AddItem(new MenuItem("QManaC", "使用Q最低蓝量").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("EManaC", "使用E最低蓝量").SetValue(new Slider(20, 1, 100)));

            Menu.AddSubMenu(new Menu("[VH] 骚扰", "Harrass"));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "连招 Q 骚扰")).SetValue(true);
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "连招 E 骚扰").SetValue(true));
           // Menu.SubMenu("Harrass").AddItem(new MenuItem("3RdE", "Try to 3rd Proc E").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("QManaH", "使用Q最低蓝量").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("EManaH", "使用E最低蓝量").SetValue(new Slider(20, 1, 100)));
            Menu.AddSubMenu(new Menu("[VH] 清线", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLH", "连招 Q 补刀")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLC", "连招 Q 清线")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLH", "补刀最低蓝量").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLC", "清线最低蓝量").SetValue(new Slider(35, 1, 100)));
            Menu.AddSubMenu(new Menu("[VH] 杂项", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Packets", "使用 封包").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AntiGP", "防止 突进")).SetValue(true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "中断 法术").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("SmartQ", "优先 使用 QE").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("ENext", "使用 E").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("PushDistance", "使用 E 距离").SetValue(new Slider(425, 400, 500)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("CondemnTurret", "尝试 E 进塔").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AutoE", "自动 E").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("NoEEnT", "禁止敌方塔下E").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("WallTumble", "突袭 E墙").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("ThreshLantern", "抓起 锤石 灯笼").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
            Menu.AddSubMenu(new Menu("[VH] BushRevealer", "BushReveal"));
            //Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushReveal", "Bush Revealer").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushRevealer", "连招 使用 物品").SetValue(true));
            Menu.AddSubMenu(new Menu("[VH] 物品", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkC", "破败 连招").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkH", "破败 骚扰").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuC", "幽梦 连招").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuH", "幽梦 骚扰").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeC", "小弯刀 连招").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeH", "小弯刀 骚扰").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("OwnHPercBotrk", "破败|自己血量").SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("Items").AddItem(new MenuItem("EnHPercBotrk", "破败|敌人血量").SetValue(new Slider(20, 1, 100)));

            Menu.AddSubMenu(new Menu("[VH] 水银弯刀", "QSSMenu"));
           Menu.SubMenu("QSSMenu").AddItem(new MenuItem("UseQSS", "使用 水银弯刀").SetValue(true));
            Menu.AddSubMenu(new Menu("[VH] 水银 解除状态", "QSST"));
            Cleanser.CreateTypeQSSMenu();
            Menu.AddSubMenu(new Menu("[VH] 水银 解除法术", "QSSSpell"));
            Cleanser.CreateQSSSpellMenu();
            Menu.AddSubMenu(new Menu("[VH] 连招 禁用", "NoCondemn"));
            CreateNoCondemnMenu();
            /**
            Menu.AddSubMenu(new Menu("[VH] AutoLeveler", "AutoLevel"));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALSeq", "AutoLevel Seq").SetValue(new StringList(new []{"Q,W,E Max W,Q,E","Q,E,W Max Q,W,E"},0)));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALAct", "AutoLevel Active").SetValue(false));
             * */
            Menu.AddSubMenu(new Menu("[VH] 范围", "Draw"));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawE", "显示 E 范围").SetValue(new Circle(true,Color.MediumPurple)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawCond", "显示 E墙 位置").SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawDrake", "显示 越墙 位置1").SetValue(new Circle(true, Color.WhiteSmoke)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawMid", "显示 越墙 位置2").SetValue(new Circle(true, Color.WhiteSmoke)));
            Menu.AddToMainMenu();
            Game.PrintChat("<font color = \"#FF0020\">鑴氭湰</font><font color = \"#22FF10\">鍔犺級鎴愬姛锛佹洿澶歸ww.loll35.com  </font>");
            Game.PrintChat("By <font color='#FF0000'>DZ</font><font color='#FFFFFF'>191</font>. Special Thanks to: Kurisuu");
            Game.PrintChat("If you like my assemblies feel free to donate me (link on the forum :) )");
           //Cleanser.cleanUselessSpells();
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E,550f);
            R = new Spell(SpellSlot.R);
            E.SetTargetted(0.25f,1600f);
            Orbwalking.AfterAttack += Orbwalker_AfterAttack;
            Game.OnGameUpdate += Game_OnGameUpdate;
           // Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
           Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
           AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
           Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += Cleanser.OnCreateObj;
            GameObject.OnDelete += Cleanser.OnDeleteObj;
        }

       
        private void Orbwalker_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe && target.IsValidTarget())
            {
                 AfterAA(target);
            }
        }
        
        void AfterAA(Obj_AI_Base target)
        {
            if (!(target is Obj_AI_Hero)) return;
            var tar = (Obj_AI_Hero)target;


            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:

                    if (isMenuEnabled("UseQC")) SmartQCheck(tar);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (isMenuEnabled("UseQH")) SmartQCheck(tar);
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:

                default:
                    break;
            }

            ENextAuto(tar);
            UseItems(tar);
        }
        private void ENextAuto(Obj_AI_Hero tar)
        {
            if (!E.IsReady() || !tar.IsValid || !Menu.Item("ENext").GetValue<KeyBind>().Active) return;
                CastE(tar,true);
            Menu.Item("ENext").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle,false));
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            //Cleanser.enableCheck();
            if (Player.IsDead) return;
            Obj_AI_Hero tar;

            if (isMenuEnabled("AutoE") && CondemnCheck(Player.Position, out tar)) { CastE(tar,true);}
            if (Menu.Item("WallTumble").GetValue<KeyBind>().Active) WallTumble();
            if (Menu.Item("ThreshLantern").GetValue<KeyBind>().Active) takeLantern();
            QFarmCheck();
            //Cleanser
            Cleanser.cleanserBySpell();
            Cleanser.cleanserByBuffType();

            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Obj_AI_Hero tar2;
                    if (isMenuEnabled("UseEC") && CondemnCheck(Player.ServerPosition, out tar2)) { CastE(tar2);}
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Obj_AI_Hero tar3;
                    if (isMenuEnabled("UseEH") && CondemnCheck(Player.ServerPosition, out tar3)) { CastE(tar3); }
                    break;
                default:
                    break;
            }  
        }
        

        void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var DrawE = Menu.Item("DrawE").GetValue<Circle>();
            var DrawCond = Menu.Item("DrawCond").GetValue<Circle>();
            var DrawDrake = Menu.Item("DrawDrake").GetValue<Circle>();
            var DrawMid = Menu.Item("DrawMid").GetValue<Circle>();
            Vector2 MidWallQPos = new Vector2(6010.5869140625f, 8508.8740234375f);
            Vector2 DrakeWallQPos = new Vector2(11334.74f, 4517.47f);
            if (DrawDrake.Active && Player.Distance(DrakeWallQPos) < 1500f) Utility.DrawCircle(new Vector3(11590.95f, 4656.26f, 0f), 75f, DrawDrake.Color);
            if (DrawMid.Active && Player.Distance(MidWallQPos) < 1500f) Utility.DrawCircle(new Vector3(6623, 8649, 0f), 75f, DrawMid.Color);
            if (DrawE.Active)Utility.DrawCircle(Player.Position,E.Range,DrawE.Color);
            if (DrawCond.Active) DrawPostCondemn();
            
        }

        void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var GPSender = (Obj_AI_Hero)gapcloser.Sender;
            if (!isMenuEnabled("AntiGP") || !E.IsReady() || !GPSender.IsValidTarget()) return;
            CastE(GPSender, true);

        }

        void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var Sender = (Obj_AI_Hero)unit;
            if (!isMenuEnabled("Interrupt") || !E.IsReady() || !Sender.IsValidTarget()) return;
            CastE(Sender,true);
        }

        bool CondemnCheck(Vector3 Position, out Obj_AI_Hero target)
        {
            if (isUnderEnTurret(Player.Position) && isMenuEnabled("NoEEnT"))
            {
                target = null;
                return false;
            }
            foreach (var En in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget() && !isMenuEnabled("nC"+hero.ChampionName) && hero.Distance(Player.Position)<=E.Range))
            {
                var EPred = E.GetPrediction(En);
                int pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                for (int i = 0; i < pushDist; i += (int)En.BoundingRadius)
                {
                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Position.To2D(), -i).To3D();
                    var OrTurret = isMenuEnabled("CondemnTurret") && isUnderTurret(loc3);
                    AfterCond = loc3;
                    if (isWall(loc3) || OrTurret)
                    {
                        if(isMenuEnabled("BushRevealer"))CheckAndWard(Position,loc3,En);
                        target = En;
                        return true; 
                    }
                }
            }
            target = null;
            return false;
            
        }

        void QFarmCheck()
        {
           // if (COrbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit ||
           //     COrbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear ||
           //     COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) return; //Tempfix
            if (!Q.IsReady()) return;
            var PosAfterQ = Player.Position.To2D().Extend(Game.CursorPos.To2D(), 300);
            var minList =
                MinionManager.GetMinions(Player.Position, 550f).Where(min =>
                    HealthPrediction.GetHealthPrediction(min,(int)(Q.Delay + min.Distance(PosAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000)+Game.Ping <= (Q.GetDamage(min)+Player.GetAutoAttackDamage(min))
                    && HealthPrediction.GetHealthPrediction(min, (int)(Q.Delay + min.Distance(PosAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000)+Game.Ping > 0);
            if (!minList.First().IsValidTarget()) return;
            CastQ(Vector3.Zero,minList.First());
        }
        
        void SmartQCheck(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !target.IsValidTarget()) return;
            if (!isMenuEnabled("SmartQ") || !E.IsReady())
            {
                CastQ(Game.CursorPos,target);
            }
            else
            {
                for (int I = 0; I <= 360; I += 65)
                {
                    var F1 = new Vector2(Player.Position.X + (float)(300 * Math.Cos(I * (Math.PI / 180))), Player.Position.Y + (float)(300 * Math.Sin(I * (Math.PI / 180)))).To3D();
                   // var FinalPos = Player.Position.To2D().Extend(F1, 300).To3D();
                    Obj_AI_Hero targ;
                    if (CondemnCheck(F1, out targ))
                    {
                        CastTumble(F1,target);
                        CastE(target);
                        return;
                    }
                }
                CastQ(Game.CursorPos, target);
            }
        }

        void CastQ(Vector3 Pos,Obj_AI_Base target,bool customPos=false)
        {
           if (!Q.IsReady() || !target.IsValidTarget()) return;
           
            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var ManaC = Menu.Item("QManaC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaC && isMenuEnabled("UseQC"))
                    {
                        if(isMenuEnabled("UseRC") && R.IsReady())R.CastOnUnit(Player);
                        if(!customPos){CastTumble(target);}else{CastTumble(Pos,target);}
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var ManaH = Menu.Item("QManaH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaH && isMenuEnabled("UseQH")){ if (!customPos){ CastTumble(target);} else{ CastTumble(Pos, target);}}
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    var ManaLH = Menu.Item("QManaLH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaLH && isMenuEnabled("UseQLH")) { if (!customPos) { CastTumble(target); } else { CastTumble(Pos, target); } }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    var ManaLC = Menu.Item("QManaLC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaLC && isMenuEnabled("UseQLC")){ if (!customPos){CastTumble(target); }else{ CastTumble(Pos, target);}}
                    break;
                default:
                    break;
            }
        }

        void CastTumble(Obj_AI_Base target)
        {
            
            //Q.Cast(Game.CursorPos, isMenuEnabled("Packets"));
         //  return;
          
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100)Q.Cast(Game.CursorPos, isMenuEnabled("Packets"));
        }
        void CastTumble(Vector3 Pos,Obj_AI_Base target)
        {
           //Q.Cast(Pos, isMenuEnabled("Packets"));
          //  return;
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Pos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100) Q.Cast(Pos, isMenuEnabled("Packets"));
        }
        void CastE(Obj_AI_Hero target, bool isForGapcloser = false)
        {
            if (!E.IsReady() || !target.IsValidTarget()) return;
            if (isForGapcloser)
            {
                E.Cast(target, isMenuEnabled("Packets"));
                AfterCond = Vector3.Zero;
                return;
            }
            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var ManaC = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if (isMenuEnabled("UseEC") && getPerValue(true) >= ManaC)
                    {
                        E.Cast(target, isMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var ManaH = Menu.Item("EManaH").GetValue<Slider>().Value;
                    if (isMenuEnabled("UseEH") && getPerValue(true) >= ManaH)
                    {
                        E.Cast(target, isMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
                default:
                    break;
            }
        }
        private static void CreateNoCondemnMenu()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                Menu.SubMenu("NoCondemn").AddItem(new MenuItem("nC"+hero.ChampionName, hero.ChampionName).SetValue(false));
            }
        }
        
        #region Items & Tumble
        void UseItems(Obj_AI_Hero tar)
        {
            var ownH = getPerValue(false);
            if ((Menu.Item("BotrkC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar,false))))
            {
                UseItem(3153, tar);
            }
            if ((Menu.Item("BotrkH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
               ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }
            if (Menu.Item("YoumuuC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3142);
            }
            if (Menu.Item("YoumuuH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                UseItem(3142);
            }
            if (Menu.Item("BilgeC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3144,tar);
            }
            if (Menu.Item("BilgeH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                UseItem(3144, tar);
            }
        }
        void WallTumble()
        {
            //Credits to Chogart
            Vector2 MidWallQPos = new Vector2(6010.5869140625f, 8508.8740234375f);
            Vector2 DrakeWallQPos = new Vector2(11334.74f, 4517.47f);
            if (Player.Distance(MidWallQPos) >= Player.Distance(DrakeWallQPos))
            {

                if (Player.Position.X < 11540 || Player.Position.X > 11600 || Player.Position.Y < 4638 ||
                    Player.Position.Y > 4712)
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(11590.95f, 4656.26f)).Send();
                }
                else
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(11590.95f, 4656.26f)).Send();
                    Q.Cast(DrakeWallQPos, true);
                }
            }
            else
            {
                if (Player.Position.X < 6600 || Player.Position.X > 6660 || Player.Position.Y < 8630 || Player.Position.Y > 8680)
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(6623, 8649)).Send();
                }
                else
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(6623, 8649)).Send();
                    Q.Cast(MidWallQPos, true);
                }
            }
        }
        
        void takeLantern()
        {
            foreach (GameObject obj in ObjectManager.Get<GameObject>())
            {
                if (obj.Name.Contains("ThreshLantern") &&obj.Position.Distance(ObjectManager.Player.ServerPosition) <= 500 && obj.IsAlly)
                {
                    GamePacket pckt =Packet.C2S.InteractObject.Encoded(new Packet.C2S.InteractObject.Struct(ObjectManager.Player.NetworkId,obj.NetworkId));
                    pckt.Send();
                    return;
                }
            }
        }
        #endregion
        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }

        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;
            SpellDataInst sdi = GetItemSpell(slot);
            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)return slot;
            return null;
        }
        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        bool isWall(Vector3 Pos)
        {
            CollisionFlags cFlags = NavMesh.GetCollisionFlags(Pos);
            return (cFlags == CollisionFlags.Wall);
        }
        bool isUnderTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsAlly && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }
        bool isUnderEnTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsEnemy && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }
        public static bool isMenuEnabled(String val)
        {
            return Menu.Item(val).GetValue<bool>();
        }
        float getPerValue(bool mana)
        {
            if (mana) return (Player.Mana / Player.MaxMana) * 100;
            return (Player.Health / Player.MaxHealth) * 100;
        }
        float getPerValueTarget(Obj_AI_Hero target, bool mana)
        {
            if (mana) return (target.Mana / target.MaxMana) * 100;
            return (target.Health / target.MaxHealth) * 100;
        }
        bool isGrass(Vector3 Pos)
        {
            return NavMesh.IsWallOfGrass(Pos,65);
        //return false; 
        }

        void CheckAndWard(Vector3 sPos, Vector3 EndPosition, Obj_AI_Hero target)
        {
            if (isGrass(EndPosition))
            {
                var WardSlot = FindBestWardItem();
                if (WardSlot == null) return;
                for (int i = 0; i < Vector3.Distance(sPos, EndPosition); i += (int)target.BoundingRadius)
                {
                    var v = sPos.To2D().Extend(EndPosition.To2D(), -i).To3D();
                    if (isGrass(v))
                    {
                        WardSlot.UseItem(v);
                    }
                }
            }
        }

        void DrawPostCondemn()
        {
            var DrawCond = Menu.Item("DrawCond").GetValue<Circle>();
            foreach (var En in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget() && !isMenuEnabled("nC" + hero.ChampionName) && hero.Distance(Player.Position) <= E.Range))
            {
                var EPred = E.GetPrediction(En);
                int pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                for (int i = 0; i < pushDist; i += (int)En.BoundingRadius)
                {
                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.Position.To2D(), -i).To3D();
                    if (isWall(loc3)) Utility.DrawCircle(loc3, 100f, DrawCond.Color);

                }
            }
        }
        
    }
    
}
