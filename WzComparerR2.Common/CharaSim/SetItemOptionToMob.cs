﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WzComparerR2.CharaSim
{
    public class SetItemOptionToMob
    {
        public SetItemOptionToMob()
        {
            this.Mobs = new List<int>();
            this.Props = new Dictionary<GearPropType, int>();
        }

        public List<int> Mobs { get; private set; }
        public string MobName { get; set; }
        public Dictionary<GearPropType, int> Props { get; private set; }

        public string ConvertSummary()
        {
            StringBuilder sb = new StringBuilder();

            string mobStr = null;
            if (MobName != null)
            {
                mobStr = MobName;
            }
            else if (Mobs.Count > 0)
            {
                mobStr = Mobs[0].ToString();
            }

            foreach (var kv in this.Props)
            {
                if (kv.Key == GearPropType.damR)
                {
                    sb.AppendFormat("+{0}% damage", kv.Value);
                    sb.AppendFormat(" when attacking {0}.", mobStr);
                }
                else
                {
                    sb.Append(ItemStringHelper.GetGearPropString(kv.Key, kv.Value));
                    sb.AppendFormat(" when attacking {0}.", mobStr);
                }
            }

            return sb.ToString(0, sb.Length);
        }
    }
}