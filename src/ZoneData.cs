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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Revise.Files.CON;
using Revise.Files.IFO;
using Revise.Files.STB;
using Revise.Files.ZON;
using SharpDX;

namespace convert_tool
{
  public class ZoneData
  {
    private const int PostionModifier = 100;

    public void Load(string filePath = null, int mapId = 0)
    {
      if (filePath == null)
        return;

      var directoryPath = Path.GetDirectoryName(filePath);
      var fileName = Path.GetFileNameWithoutExtension(filePath);
      var zoneFile = new ZoneFile();
      try
      {
        zoneFile.Load(filePath);
      }
      catch (FileNotFoundException)
      {
        Console.Write("Failed!\n");
        return;
      }

      Console.Write("Successful!\n");

      // Do the data conversion here
      var mapDataFile = new MapDataFile[65,65];
      for (var x = 0; x < zoneFile.Width; x++)
      {
        for (var y = 0; y < zoneFile.Height; y++)
        {
          if (!zoneFile.Positions[x, y].IsUsed) continue;

          mapDataFile[x, y] = new MapDataFile();
          try
          {
            mapDataFile[x, y].Load(directoryPath + "\\" + y + "_" + x + ".ifo");
          }
          catch (FileNotFoundException)
          {
            continue;
          }
        }
      }

      var mobList = new List<string>();
      var npcList = new List<string>();
      var spawnList = new List<string>();
      var warpList = new List<string>();

      foreach (var spawnPoint in zoneFile.SpawnPoints)
      {
        if (spawnPoint.Name.Contains("WARP")) continue;

        var destCoords = new Vector3(((spawnPoint.Position.X + 520000.00f) / 100.0f), ((spawnPoint.Position.Z + 520000.00f) / 100.0f), ((spawnPoint.Position.Y) / 100.0f));

        if (spawnPoint.Name.Contains("start"))
        {
          spawnList.Add("login_point(" + mapId + ", " + destCoords.X + ", " + destCoords.Y + ");\n");
        }
        else
        {
          spawnList.Add("respawn_point(" + mapId + ", " + destCoords.X + ", " + destCoords.Y + ");\n");
        }
      }

      foreach (var ifo in mapDataFile)
      {
        if (ifo == null) continue;

        var blockX = ifo.ZonePosition.X * PostionModifier;
        var blockY = ifo.ZonePosition.Y * PostionModifier;

        ExtractNpcs(npcList, mapId, ifo);
        ExtractMobs(mobList, mapId, ifo);
        ExtractWarpGates(warpList, mapId, ifo);
      }

      var catagoryName = "field";
      Regex townRgx = new Regex(@"^[a-zA-Z][a-zA-Z]T\d{2}$"); // matches '<2 chars>T<2 numbers>'
      if(fileName.Contains("PVP")) 
      {
        catagoryName = "pvp";
      }
      else if(townRgx.IsMatch(fileName))
      {
        catagoryName = "cities";
      }

      var luaList = new List<string>();
      if (spawnList.Count > 0)
      {
        var outFilePath = "scripts\\spawns\\" + catagoryName + "\\" + fileName + ".lua";
        (new FileInfo(outFilePath)).Directory.Create();
        var luaFile = new System.IO.StreamWriter(outFilePath, false);
        luaList.Add("include(\""+ outFilePath + "\");\n");
        using (luaFile)
        {
          luaFile.Write("--[[ SPAWN LIST\n");
          luaFile.Write(
            "login_point(<warp_alias>, <dest_map_id>, <dest_x_pos>, <dest_y_pos>, <dest_z_pos>, <map_id>, <x_pos>, <y_pos>, <z_pos>, <angle>, <x_scale>, <y_scale>, <z_scale>);\n");
          luaFile.Write(
            "respawn_point(<warp_alias>, <dest_map_id>, <dest_x_pos>, <dest_y_pos>, <dest_z_pos>, <map_id>, <x_pos>, <y_pos>, <z_pos>, <angle>, <x_scale>, <y_scale>, <z_scale>);\n");
          luaFile.Write("--]]\n");
          foreach (var mapObj in spawnList)
            luaFile.Write(mapObj);
        }
      }

      if (warpList.Count > 0)
      {
        var outFilePath = "scripts\\warps\\" + catagoryName + "\\" + fileName + ".lua";
        (new FileInfo(outFilePath)).Directory.Create();
        var luaFile = new System.IO.StreamWriter(outFilePath, false);
        luaList.Add("include(\""+ outFilePath + "\");\n");
        using (luaFile)
        {
          luaFile.Write("--[[ WARP LIST\n");
          luaFile.Write(
            "warp_gate(<warp_alias>, <dest_map_id>, <dest_x_pos>, <dest_y_pos>, <dest_z_pos>, <map_id>, <x_pos>, <y_pos>, <z_pos>, <angle>, <x_scale>, <y_scale>, <z_scale>);\n");
          luaFile.Write("--]]\n");
          foreach (var mapObj in warpList)
            luaFile.Write(mapObj);
        }
      }

      if(npcList.Count > 0)
      {
        var outFilePath = "scripts\\npcs\\" + catagoryName + "\\" + fileName + ".lua";
        (new FileInfo(outFilePath)).Directory.Create();
        var luaFile = new System.IO.StreamWriter(outFilePath, false);
        luaList.Add("include(\""+ outFilePath + "\");\n");
        using (luaFile)
        {
          luaFile.WriteLine();
          luaFile.WriteLine();
          luaFile.Write("--[[ NPC SPAWN LIST\n");
          luaFile.Write("npc(<npc_lua_file>, <npc_id>, <map_id>, <x_pos>, <y_pos>, <z_pos>, <angle>);\n");
          luaFile.Write("--]]\n");
          foreach (var mapObj in npcList)
            luaFile.Write(mapObj);
        }
      }

      if(mobList.Count > 0)
      {
        var outFilePath = "scripts\\mobs\\" + catagoryName + "\\" + fileName + ".lua";
        (new FileInfo(outFilePath)).Directory.Create();
        var luaFile = new System.IO.StreamWriter(outFilePath, false);
        luaList.Add("include(\""+ outFilePath + "\");\n");
        using (luaFile)
        {
          luaFile.WriteLine();
          luaFile.WriteLine();
          luaFile.Write("--[[ MOB SPAWN LIST\n");
          luaFile.Write(
            "mob(<mob_spawner_alias>, <mob_id>, <mob_count>, <spawner_limit>, <spawn_interval>, <spawner_range>, <map_id>, <x_pos>, <y_pos>, <z_pos>,);\n");
          luaFile.Write("--]]\n");
          foreach (var mapObj in mobList)
            luaFile.Write(mapObj);
        }
      }

      Console.Write("\n\n");
    }

    private static void ExtractMobs(List<string> mobList, int mapId, MapDataFile ifo)
    {
      foreach (var mobSpawns in ifo.MonsterSpawns)
      {
        var adjPosCoords = new Vector3(((mobSpawns.Position.X + 520000.00f) / 100.0f), ((mobSpawns.Position.Y + 520000.00f) / 100.0f), ((mobSpawns.Position.Z) / 100.0f));
        foreach (var normalMobs in mobSpawns.NormalSpawnPoints)
        {

          mobList.Add("mob(\"\", "
                        + normalMobs.Monster.ToString() + ", "
                        + normalMobs.Count.ToString() + ", "
                        + mobSpawns.Limit.ToString() + ", "
                        + mobSpawns.Interval.ToString() + ", "
                        + mobSpawns.Range.ToString() + ", "
                        + mapId.ToString() + ", "
                        + (adjPosCoords.X).ToString() + ", "
                        + (adjPosCoords.Y).ToString() + ", "
                        + (adjPosCoords.Z).ToString() + ");\n");
        }

        foreach (var tacticalMobs in mobSpawns.TacticalSpawnPoints)
        {
          mobList.Add("mob(\"\", "
                        + tacticalMobs.Monster.ToString() + ", "
                        + tacticalMobs.Count.ToString() + ", "
                        + mobSpawns.Limit.ToString() + ", "
                        + mobSpawns.Interval.ToString() + ", "
                        + mobSpawns.Range.ToString() + ", "
                        + mapId.ToString() + ", "
                        + (adjPosCoords.X).ToString() + ", "
                        + (adjPosCoords.Y).ToString() + ", "
                        + (adjPosCoords.Z).ToString() + ");\n");
        }
      }
    }

    private static void ExtractNpcs(List<string> npcList, int mapId, MapDataFile ifo)
    {
      foreach (var npc in ifo.NPCs)
      {
        var eventDataFile = new DataFile();
        eventDataFile.Load("./3DDATA/stb/list_event.stb");
        int dialogId = 0;
        for (int i = 0; i < eventDataFile.RowCount; i++)
        {
          if (eventDataFile[i][0] == npc.ConversationFile)
          {
            dialogId = i;
            break;
          }
        }

        var adjPosCoords = new Vector3(((npc.Position.X + 520000.00f) / 100.0f), ((npc.Position.Y + 520000.00f) / 100.0f), ((npc.Position.Z) / 100.0f));
        npcList.Add("npc(\"" + dialogId + "\", "
                      + npc.ObjectID.ToString() + ", "
                      + mapId.ToString() + ", "
                      + (adjPosCoords.X).ToString() + ", "
                      + (adjPosCoords.Y).ToString() + ", "
                      + (adjPosCoords.Z).ToString() + ", "
                      + (npc.Rotation.Angle * (180.0 / Math.PI)) + ");\n");
      }
    }

    private static void ExtractWarpGates(List<string> warpList, int mapId, MapDataFile ifo)
    {
      const string warpStb = "./3DDATA/stb/warp.stb";
      const string zoneStb = "./3DDATA/stb/list_zone.stb";

      var zoneDataFile = new DataFile();
      zoneDataFile.Load(zoneStb);

      var warpDataFile = new DataFile();
      warpDataFile.Load(warpStb);
      var destCoords = Vector3.Zero;

      foreach (var warpGate in ifo.WarpPoints)
      {
        var destMapId = int.Parse(warpDataFile[warpGate.WarpID][2]);
        if (zoneDataFile[destMapId][2].ToString().Contains(".zon"))
        {
          ZoneFile zoneFile = new ZoneFile();
          zoneFile.Load(zoneDataFile[destMapId][2].ToString()); // Load the zon file

          foreach (var spawnPoint in zoneFile.SpawnPoints)
          {
            if (spawnPoint.Name != warpDataFile[warpGate.WarpID][3].ToString()) continue;

            // rose is stupid and we need to do this to get the right coords
            destCoords = new Vector3(((spawnPoint.Position.X + 520000.00f) / 100.0f), ((spawnPoint.Position.Z + 520000.00f) / 100.0f), ((spawnPoint.Position.Y) / 100.0f));
            break;
          }
        }

        var gateCoords = new Vector3(((warpGate.Position.X + 520000.00f) / 100.0f), ((warpGate.Position.Y + 520000.00f) / 100.0f), ((warpGate.Position.Z) / 100.0f));

        warpList.Add("warp_gate(\"\", " 
                      + warpDataFile[warpGate.WarpID][2].ToString() + ", "
                      + (destCoords.X) + ", "
                      + (destCoords.Y) + ", "
                      + (destCoords.Z) + ", "
                      + mapId.ToString() + ", "
                      + (gateCoords.X) + ", "
                      + (gateCoords.Y) + ", "
                      + (gateCoords.Z) + ", "
                      + warpGate.Rotation.Angle + ", "
                      + warpGate.Scale.X + ", "
                      + warpGate.Scale.Y + ", "
                      + warpGate.Scale.Z + ");\n");
      }
    }
  }
}