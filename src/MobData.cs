﻿#region License

/**
 * Copyright (C) 2018 Chirstopher Torres (Raven)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#endregion

//#define OUTPUT_STB_DATA_COMMENT
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Revise.Files.STB;
using Revise.Files.STL;
using SharpDX;

namespace convert_tool
{
  public class MobData
  {
    public void Load(string stbPath = null, string stlPath = null)
    {
      if (stbPath == null || stlPath == null)
        return;

      var stringFile = new StringTableFile();
      var dataFile = new DataFile();

      Console.Write("Attempting to load \"" + stbPath + "\" and \"" + stlPath + "\" - ");

      try
      {
        dataFile.Load(stbPath);
        stringFile.Load(stlPath);
      }
      catch (FileNotFoundException e)
      {
        Console.Write("Failed!\n");
        Console.WriteLine(e);
        return;
      }

      Console.Write("Successful!\n");

      var luaList = new List<string>();
      Dictionary<string,string> luaCode = new Dictionary<string, string>();
      for (var i = 0; i < dataFile.RowCount; i++)
      {
        StringTableRow strTableRow;
        var curRow = dataFile[i];
        try
        {
          strTableRow = stringFile[curRow[41]];
        }
        catch (ArgumentException)
        {
          continue;
        }

        var npcName = strTableRow.GetText();
        var npcDesc = curRow[41];

        if (npcName.Length == 0) continue;

        int npcHeight,
          npcWalkSpeed,
          npcRunSpeed,
          npcScale,
          npcRWeapon,
          npcLWeapon,
          npcLevel,
          npcHp,
          npcAttack,
          npcHit,
          npcDef,
          npcRes,
          npcAvoid,
          npcAttackSpd,
          npcIsMagicDamage,
          npcAiType,
          npcGiveExp,
          npcDropType,
          npcMarkNumber,
          npcDropMoney,
          npcDropItem,
          npcUnionNumber,
          npcNeedSummonCount,
          npcSellTab0,
          npcSellTab1,
          npcSellTab2,
          npcSellTab3,
          npcCanTarget,
          npcAttackRange,
          npcType,
          npcHitMaterialType,
          npcFaceIcon,
          npcSummonMobType,
          npcNormalEffectSound,
          npcAttackSound,
          npcHitSound,
          npcHandHitEffect,
          npcDeadEffect,
          npcDieSound,
          npcQuestType,
          npcGlowColor,
          npcCreateEffect,
          npcCreateSound;

        npcHeight = npcWalkSpeed = npcRunSpeed = npcScale = npcRWeapon = npcLWeapon = npcLevel = npcHp = npcAttack =
          npcHit = npcDef = npcRes = npcAvoid = npcAttackSpd = npcIsMagicDamage =
            npcAiType = npcGiveExp = npcDropType = npcMarkNumber = npcDropMoney = npcDropItem = npcUnionNumber =
              npcNeedSummonCount =
                npcSellTab0 = npcSellTab1 = npcSellTab2 = npcSellTab3 = npcCanTarget = npcAttackRange = npcType =
                  npcHitMaterialType = npcFaceIcon = npcSummonMobType = npcNormalEffectSound = npcAttackSound =
                    npcHitSound = npcHandHitEffect = npcDeadEffect =
                      npcDieSound = npcQuestType = npcGlowColor = npcCreateEffect = npcCreateSound = 0;


        int.TryParse(curRow[02], out npcWalkSpeed); //lua?
        int.TryParse(curRow[03], out npcRunSpeed); //lua?
        int.TryParse(curRow[04], out npcScale); //lua
        int.TryParse(curRow[05], out npcRWeapon); //lua?
        int.TryParse(curRow[06], out npcLWeapon); //lua?
        int.TryParse(curRow[07], out npcLevel); //lua?
        int.TryParse(curRow[08], out npcHp); //sql? lua?
        int.TryParse(curRow[09], out npcAttack); //lua?
        int.TryParse(curRow[10], out npcHit); //lua?
        int.TryParse(curRow[11], out npcDef); //lua?
        int.TryParse(curRow[12], out npcRes); //lua?
        int.TryParse(curRow[13], out npcAvoid); //lua?
        int.TryParse(curRow[14], out npcAttackSpd); //lua?
        int.TryParse(curRow[15], out npcIsMagicDamage); //lua?
        int.TryParse(curRow[16], out npcAiType); //lua
        int.TryParse(curRow[17], out npcGiveExp); //lua
        int.TryParse(curRow[18], out npcDropType); //lua
        int.TryParse(curRow[18], out npcMarkNumber); // NPC icon (only used with EVO style npc boxes)
        int.TryParse(curRow[19], out npcDropMoney); //lua

        int.TryParse(curRow[20], out npcDropItem); //lua
        int.TryParse(curRow[20], out npcUnionNumber); //sql
        int.TryParse(curRow[21], out npcNeedSummonCount); //lua, used for unknown...
        int.TryParse(curRow[21], out npcSellTab0); //lua
        int.TryParse(curRow[22], out npcSellTab1); //lua
        int.TryParse(curRow[23], out npcSellTab2); //lua 
        int.TryParse(curRow[24], out npcSellTab3); //lua
        int.TryParse(curRow[25], out npcCanTarget);
        int.TryParse(curRow[26], out npcAttackRange); //lua?
        int.TryParse(curRow[27], out npcType); //sql?
        int.TryParse(curRow[28], out npcHitMaterialType);
        int.TryParse(curRow[29], out npcFaceIcon); // NPC face icon (only used with EVO style npc boxes)
        int.TryParse(curRow[29], out npcSummonMobType);

        int.TryParse(curRow[30], out npcNormalEffectSound);
        int.TryParse(curRow[31], out npcAttackSound);
        int.TryParse(curRow[32], out npcHitSound);
        int.TryParse(curRow[33], out npcHandHitEffect);
        int.TryParse(curRow[34], out npcDeadEffect);
        int.TryParse(curRow[35], out npcDieSound);
        int.TryParse(curRow[38], out npcQuestType); //lua
        int.TryParse(curRow[39], out npcGlowColor);

        int.TryParse(curRow[42], out npcHeight);
        int.TryParse(curRow[43], out npcCreateEffect);
        int.TryParse(curRow[44], out npcCreateSound);

        string script = "";

        script += "npc[" + i + "].walk_speed = " + npcWalkSpeed + "\n";
        script += "npc[" + i + "].run_speed = " + npcRunSpeed + "\n";
        script += "npc[" + i + "].scale = " + npcScale + "\n";
        script += "npc[" + i + "].r_weapon = " + npcRWeapon + "\n";
        script += "npc[" + i + "].l_weapon = " + npcLWeapon + "\n";
        script += "npc[" + i + "].level = " + npcLevel + "\n";
        script += "npc[" + i + "].hp = " + npcHp + "\n";
        script += "npc[" + i + "].attack = " + npcAttack + "\n";
        script += "npc[" + i + "].hit = " + npcHit + "\n";
        script += "npc[" + i + "].def = " + npcDef + "\n";
        script += "npc[" + i + "].res = " + npcRes + "\n";
        script += "npc[" + i + "].avoid = " + npcAvoid + "\n";
        script += "npc[" + i + "].attack_spd = " + npcAttackSpd + "\n";
        script += "npc[" + i + "].is_magic_damage = " + npcIsMagicDamage + "\n";
        script += "npc[" + i + "].ai_type = " + npcAiType + "\n";
        script += "npc[" + i + "].give_exp = " + npcGiveExp + "\n";
        script += "npc[" + i + "].drop_type = " + npcDropType + "\n";
        script += "npc[" + i + "].drop_money = " + npcDropMoney + "\n";
        script += "npc[" + i + "].drop_item = " + npcDropItem + "\n";
        script += "npc[" + i + "].union_number = " + npcUnionNumber + "\n";
        script += "npc[" + i + "].need_summon_count = " + npcNeedSummonCount + "\n";
        script += "npc[" + i + "].sell_tab0 = " + npcSellTab0 + "\n";
        script += "npc[" + i + "].sell_tab1 = " + npcSellTab1 + "\n";
        script += "npc[" + i + "].sell_tab2 = " + npcSellTab2 + "\n";
        script += "npc[" + i + "].sell_tab3 = " + npcSellTab3 + "\n";
        script += "npc[" + i + "].can_target = " + npcCanTarget + "\n";
        script += "npc[" + i + "].attack_range = " + npcAttackRange + "\n";
        script += "npc[" + i + "].npc_type = " + npcType + "\n";
        script += "npc[" + i + "].hit_material_type = " + npcHitMaterialType + "\n";
        script += "npc[" + i + "].face_icon = " + npcFaceIcon + "\n";
        script += "npc[" + i + "].summon_mob_type = " + npcSummonMobType + "\n";
        script += "npc[" + i + "].quest_type = " + npcQuestType + "\n";
        script += "npc[" + i + "].height = " + npcHeight + "\n";


        var luaObjFixed = npcName.ToLower().Replace(" ", "_");
        if (!luaCode.ContainsKey(luaObjFixed))
        {
          luaCode.Add(luaObjFixed, script);
        }
        else
        {
          string oldScript = luaCode[luaObjFixed];
          luaCode.Remove(luaObjFixed);

          luaCode.Add(luaObjFixed, oldScript + "\n" + script);
        }
      }

      foreach (var luaOut in luaCode)
      {
        var catagoryName = "mobs";
        if (luaOut.Key.Contains("["))
        {
          catagoryName = "npcs";
        }
        var outFilePath = "srv_data\\scripts\\" + catagoryName + "\\ai\\" + luaOut.Key + ".lua";
        (new FileInfo(outFilePath)).Directory.Create();
        var luaFile = new System.IO.StreamWriter(outFilePath, false);
        luaList.Add("include(\"" + outFilePath + "\");\n");
        using (luaFile)
        {
          var valueOut = luaOut.Value + @"
function OnInit()
  return true
end

function OnCreate()
  return true
end

function OnDelete()
  return true
end

function OnDead(entity)
end

function OnDamaged(entity)
end"; ;
          luaFile.Write(valueOut);
        }
      }

      if (luaList.Count > 0)
      {
        var outFilePath = "srv_data\\scripts\\npc_scripts.lua";
        (new FileInfo(outFilePath)).Directory.Create();
        var luaFile = new System.IO.StreamWriter(outFilePath, true);
        using (luaFile)
        {
          foreach (var luaObj in luaList)
          {
            var luaObjFixed = luaObj.Replace("srv_data\\scripts\\", "");
            luaObjFixed = luaObjFixed.Replace("\\", "/");
            luaFile.Write(luaObjFixed);
          }
        }
      }
    }
  }
}