﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Resource = CharaSimResource.Resource;
using WzComparerR2.Common;
using WzComparerR2.CharaSim;

namespace WzComparerR2.CharaSimControl
{
    public class SkillTooltipRender2 : TooltipRender
    {
        public SkillTooltipRender2()
        {
        }

        public Skill Skill { get; set; }

        public override object TargetItem
        {
            get { return this.Skill; }
            set { this.Skill = value as Skill; }
        }

        public bool ShowProperties { get; set; } = true;
        public bool ShowDelay { get; set; }
        public bool ShowReqSkill { get; set; } = true;
        public bool DisplayCooltimeMSAsSec { get; set; } = true;
        public bool DisplayPermyriadAsPercent { get; set; } = true;
        public bool IsWideMode { get; set; } = true;

        public override Bitmap Render()
        {
            if (this.Skill == null)
            {
                return null;
            }

            CanvasRegion region = this.IsWideMode ? CanvasRegion.Wide : CanvasRegion.Original;

            int picHeight;
            Bitmap originBmp = RenderSkill(region, out picHeight);
            Bitmap tooltip = new Bitmap(originBmp.Width, picHeight);
            Graphics g = Graphics.FromImage(tooltip);

            //绘制背景区域
            GearGraphics.DrawNewTooltipBack(g, 0, 0, tooltip.Width, tooltip.Height);

            //复制图像
            g.DrawImage(originBmp, 0, 0, new Rectangle(0, 0, originBmp.Width, picHeight), GraphicsUnit.Pixel);

            //左上角
            g.DrawImage(Resource.UIToolTip_img_Item_Frame2_cover, 3, 3);

            if (this.ShowObjectID)
            {
                GearGraphics.DrawGearDetailNumber(g, 3, 3, Skill.SkillID.ToString("d7"), true);
            }

            if (originBmp != null)
                originBmp.Dispose();

            g.Dispose();
            return tooltip;
        }

        private Bitmap RenderSkill(CanvasRegion region, out int picH)
        {
            Bitmap bitmap = new Bitmap(region.Width, DefaultPicHeight);
            Graphics g = Graphics.FromImage(bitmap);
            StringFormat format = (StringFormat)StringFormat.GenericDefault.Clone();
            picH = 0;

            //获取文字
            StringResult sr;
            if (StringLinker == null || !StringLinker.StringSkill.TryGetValue(Skill.SkillID, out sr))
            {
                sr = new StringResultSkill();
                sr.Name = "(null)";
            }

            //绘制技能名称
            format.Alignment = StringAlignment.Center;
            g.DrawString(sr.Name, GearGraphics.ItemNameFont2, Brushes.White, region.TitleCenterX, 10, format);

            //绘制图标
            picH = 33;
            g.FillRectangle(GearGraphics.GearIconBackBrush2, 14, picH, 68, 68);
            if (Skill.Icon.Bitmap != null)
            {
                g.DrawImage(GearGraphics.EnlargeBitmap(Skill.Icon.Bitmap),
                14 + (1 - Skill.Icon.Origin.X) * 2,
                picH + (33 - Skill.Icon.Bitmap.Height) * 2);
            }

            //绘制desc
            picH = 35;
            if (!Skill.PreBBSkill)
                GearGraphics.DrawString(g, "[Master Level：" + Skill.MaxLevel + "]", GearGraphics.ItemDetailFont2, region.SkillDescLeft, region.TextRight, ref picH, 16);
            
            if (sr.Desc != null)
            {
                string hdesc = SummaryParser.GetSkillSummary(sr.Desc, Skill.Level, Skill.Common, SummaryParams.Default);
                //string hStr = SummaryParser.GetSkillSummary(skill, skill.Level, sr, SummaryParams.Default);
                GearGraphics.DrawString(g, hdesc, GearGraphics.ItemDetailFont2, region.SkillDescLeft, region.TextRight, ref picH, 16);
            }
            if (Skill.ReqLevel > 0)
            {
                GearGraphics.DrawString(g, "#c[Required Level：" + Skill.ReqLevel.ToString() + "]#", GearGraphics.ItemDetailFont2, region.SkillDescLeft, region.TextRight, ref picH, 16);
            }
            if (Skill.ReqAmount > 0)
            {
                GearGraphics.DrawString(g, "#c" + ItemStringHelper.GetSkillReqAmount(Skill.SkillID, Skill.ReqAmount) + "#", GearGraphics.ItemDetailFont2, region.SkillDescLeft, region.TextRight, ref picH, 16);
            }

            //分割线
            picH = Math.Max(picH, 114);
            g.DrawLine(Pens.White, region.SplitterX1, picH, region.SplitterX2, picH);
            picH += 9;

            if (Skill.Level > 0)
            {
                string hStr = SummaryParser.GetSkillSummary(Skill, Skill.Level, sr, SummaryParams.Default, new SkillSummaryOptions
                {
                    ConvertCooltimeMS = this.DisplayCooltimeMSAsSec,
                    ConvertPerM = this.DisplayPermyriadAsPercent
                });
                GearGraphics.DrawString(g, "[Current Level " + Skill.Level + "]", GearGraphics.ItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
                if (hStr != null)
                {
                    GearGraphics.DrawString(g, hStr, GearGraphics.ItemDetailFont2, region.LevelDescLeft, region.TextRight, ref picH, 16);
                }
            }

            if (Skill.Level < Skill.MaxLevel)
            {
                string hStr = SummaryParser.GetSkillSummary(Skill, Skill.Level + 1, sr, SummaryParams.Default, new SkillSummaryOptions
                {
                    ConvertCooltimeMS = this.DisplayCooltimeMSAsSec,
                    ConvertPerM = this.DisplayPermyriadAsPercent
                });
                GearGraphics.DrawString(g, "[Next Level " + (Skill.Level + 1) + "]", GearGraphics.ItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
                if (hStr != null)
                {
                    GearGraphics.DrawString(g, hStr, GearGraphics.ItemDetailFont2, region.LevelDescLeft, region.TextRight, ref picH, 16);
                }
            }
            picH += 9;

            List<string> skillDescEx = new List<string>();
            if (ShowProperties)
            {
                List<string> attr = new List<string>();
                if (Skill.Invisible)
                {
                    attr.Add("[Hidden Skill]");
                }
                if (Skill.Hyper != HyperSkillType.None)
                {
                    attr.Add("[Hyper Skill: " + Skill.Hyper + "]");
                }
                if (Skill.CombatOrders)
                {
                    attr.Add("[Can pass Master Level with Combat Orders]");
                }
                if (Skill.NotRemoved)
                {
                    attr.Add("[Cannot be canceled]");
                }
                if (Skill.MasterLevel > 0 && Skill.MasterLevel < Skill.MaxLevel)
                {
                    attr.Add("[Requires Mastery Book to pass Lv. " + Skill.MasterLevel + "]");
                }
                if (Skill.NotIncBuffDuration)
                {
                    attr.Add("[Not affected by Buff Duration increases]");
                }
                if (Skill.NotCooltimeReset)
                {
                    attr.Add("[Not affected by Cooldown reductions/resets]");
                }
                if (attr.Count > 0)
                {
                    skillDescEx.Add("#c" + string.Join("\n", attr.ToArray()) + "#");
                }
            }

            if (ShowDelay && Skill.Action.Count > 0)
            {
                foreach (string action in Skill.Action)
                {
                    skillDescEx.Add("#c[Skill Delay] " + action + ": " + CharaSimLoader.GetActionDelay(action) + " ms#");
                }
            }

            if (ShowReqSkill && Skill.ReqSkill.Count > 0)
            {
                foreach (var kv in Skill.ReqSkill)
                {
                    string skillName;
                    if (this.StringLinker != null && this.StringLinker.StringSkill.TryGetValue(kv.Key, out sr))
                    {
                        skillName = sr.Name;
                    }
                    else
                    {
                        skillName = kv.Key.ToString();
                    }
                    skillDescEx.Add("#c[Required Skill]: " + skillName + ": " + kv.Value + " #");
                }
            }

            if (skillDescEx.Count > 0)
            {
                g.DrawLine(Pens.White, region.SplitterX1, picH, region.SplitterX2, picH);
                picH += 9;
                foreach (var descEx in skillDescEx)
                {
                    GearGraphics.DrawString(g, descEx, GearGraphics.ItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
                }
                picH += 9;
            }

            format.Dispose();
            g.Dispose();
            return bitmap;
        }

        private class CanvasRegion
        {
            public int Width { get; private set; }
            public int TitleCenterX { get; private set; }
            public int SplitterX1 { get; private set; }
            public int SplitterX2 { get; private set; }
            public int SkillDescLeft { get; private set; }
            public int LevelDescLeft { get; private set; }
            public int TextRight { get; private set; }

            public static CanvasRegion Original { get; } = new CanvasRegion()
            {
                Width = 290,
                TitleCenterX = 144,
                SplitterX1 = 6,
                SplitterX2 = 283,
                SkillDescLeft = 90,
                LevelDescLeft = 8,
                TextRight = 272,
            };

            public static CanvasRegion Wide { get; } = new CanvasRegion()
            {
                Width = 430,
                TitleCenterX = 215,
                SplitterX1 = 6,
                SplitterX2 = 423,
                SkillDescLeft = 92,
                LevelDescLeft = 10,
                TextRight = 412,
            };
        }
    }
}
