using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Weapons;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Library.Utils;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using Digi.Utils;

namespace dondelium.OreRemapping{
  [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
  public class OreRemapping : MySessionComponentBase{
    MyRandom rand = new MyRandom();

    List<string> oreTypes = new List<string>(){"Iron","Nickel","Cobalt","Silicon","Magnesium","Silver","Gold","Uranium","Platinum"};

    new Dictionary<string, Dictionary<string, List<string>>> planetDispositions = new Dictionary<string, Dictionary<string, List<string>>>(){
      {"EarthLike", new Dictionary<string, List<string>>(){
        {"rich", new List<string>(){"Iron","Silver"}},
        {"impure", new List<string>(){"Gold", "Uranium", "Platinum"}}
      }},
      {"Moon", new Dictionary<string, List<string>>(){
        {"rich", new List<string>(){"Silicon","Magnesium"}},
        {"impure", new List<string>(){"Nickel", "Cobalt", "Silver"}}
      }},
      {"Mars", new Dictionary<string, List<string>>(){
        {"rich", new List<string>(){"Cobalt","Nickel"}},
        {"impure", new List<string>(){"Iron", "Silicon", "Uranium"}}
      }},
      {"Europa", new Dictionary<string, List<string>>(){
        {"rich", new List<string>(){"Silicon","Gold"}},
        {"impure", new List<string>(){"Iron", "Silver", "Platinum"}}
      }},
      {"Alien", new Dictionary<string, List<string>>(){
        {"rich", new List<string>(){"Iron","Uranium"}},
        {"impure", new List<string>(){"Nickel", "Cobalt", "Magnesium"}}
      }},
      {"Titan", new Dictionary<string, List<string>>(){
        {"rich", new List<string>(){"Nickel","Platinum"}},
        {"impure", new List<string>(){"Silicon", "Magnesium", "Gold"}}
      }}
    };

    new Dictionary<byte, Dictionary<string, string>> oreMapValueToType = new Dictionary<byte, Dictionary<string, string>>(){
      {200, new Dictionary<string, string>(){{"ore", "Iron"},{"impure", "IronI_01"},{"default", "IronI_01"},{"rich", "Iron_02"}}},
      {220, new Dictionary<string, string>(){{"ore", "Nickel"},{"impure", "NickelI_01"},{"default", "NickelI_01"},{"rich", "Nickel_01"}}},
      {240, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "SiliconI_01"},{"default", "SiliconI_01"},{"rich", "Silicon_01"}}},
      {50, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "IceEuropa2"},{"default", "IceEuropa2"},{"rich", "IceEuropa2"}}},

      {1, new Dictionary<string, string>(){{"ore", "Iron"},{"impure", "IronI_01"},{"default", "IronI_01"},{"rich", "Iron_02"}}},
      {4, new Dictionary<string, string>(){{"ore", "Iron"},{"impure", "IronI_01"},{"default", "IronI_01"},{"rich", "Iron_02"}}},
      {8, new Dictionary<string, string>(){{"ore", "Iron"},{"impure", "IronI_01"},{"default", "IronI_01"},{"rich", "Iron_02"}}},

      {12, new Dictionary<string, string>(){{"ore", "Iron"},{"impure", "IronI_01"},{"default", "Iron_02"},{"rich", "IronP_01"}}},
      {16, new Dictionary<string, string>(){{"ore", "Iron"},{"impure", "IronI_01"},{"default", "Iron_02"},{"rich", "IronP_01"}}},
      {20, new Dictionary<string, string>(){{"ore", "Iron"},{"impure", "IronI_01"},{"default", "Iron_02"},{"rich", "IronP_01"}}},

      {24, new Dictionary<string, string>(){{"ore", "Nickel"},{"impure", "NickelI_01"},{"default", "NickelI_01"},{"rich", "Nickel_01"}}},
      {28, new Dictionary<string, string>(){{"ore", "Nickel"},{"impure", "NickelI_01"},{"default", "NickelI_01"},{"rich", "Nickel_01"}}},
      {32, new Dictionary<string, string>(){{"ore", "Nickel"},{"impure", "NickelI_01"},{"default", "NickelI_01"},{"rich", "Nickel_01"}}},

      {36, new Dictionary<string, string>(){{"ore", "Nickel"},{"impure", "NickelI_01"},{"default", "Nickel_01"},{"rich", "NickelP_01"}}},
      {40, new Dictionary<string, string>(){{"ore", "Nickel"},{"impure", "NickelI_01"},{"default", "Nickel_01"},{"rich", "NickelP_01"}}},
      {44, new Dictionary<string, string>(){{"ore", "Nickel"},{"impure", "NickelI_01"},{"default", "Nickel_01"},{"rich", "NickelP_01"}}},

      {48, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "SiliconI_01"},{"default", "SiliconI_01"},{"rich", "Silicon_01"}}},
      {52, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "SiliconI_01"},{"default", "SiliconI_01"},{"rich", "Silicon_01"}}},
      {56, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "SiliconI_01"},{"default", "SiliconI_01"},{"rich", "Silicon_01"}}},

      {60, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "SiliconI_01"},{"default", "Silicon_01"},{"rich", "SiliconP_01"}}},
      {64, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "SiliconI_01"},{"default", "Silicon_01"},{"rich", "SiliconP_01"}}},
      {68, new Dictionary<string, string>(){{"ore", "Silicon"},{"impure", "SiliconI_01"},{"default", "Silicon_01"},{"rich", "SiliconP_01"}}},

      {72, new Dictionary<string, string>(){{"ore", "Cobalt"},{"impure", "CobaltI_01"},{"default", "CobaltI_01"},{"rich", "Cobalt_01"}}},
      {76, new Dictionary<string, string>(){{"ore", "Cobalt"},{"impure", "CobaltI_01"},{"default", "CobaltI_01"},{"rich", "Cobalt_01"}}},
      {80, new Dictionary<string, string>(){{"ore", "Cobalt"},{"impure", "CobaltI_01"},{"default", "CobaltI_01"},{"rich", "Cobalt_01"}}},

      {84, new Dictionary<string, string>(){{"ore", "Cobalt"},{"impure", "CobaltI_01"},{"default", "Cobalt_01"},{"rich", "CobaltP_01"}}},
      {88, new Dictionary<string, string>(){{"ore", "Cobalt"},{"impure", "CobaltI_01"},{"default", "Cobalt_01"},{"rich", "CobaltP_01"}}},
      {92, new Dictionary<string, string>(){{"ore", "Cobalt"},{"impure", "CobaltI_01"},{"default", "Cobalt_01"},{"rich", "CobaltP_01"}}},

      {96, new Dictionary<string, string>(){{"ore", "Silver"},{"impure", "SilverI_01"},{"default", "SilverI_01"},{"rich", "Silver_01"}}},
      {100, new Dictionary<string, string>(){{"ore", "Silver"},{"impure", "SilverI_01"},{"default", "SilverI_01"},{"rich", "Silver_01"}}},
      {104, new Dictionary<string, string>(){{"ore", "Silver"},{"impure", "SilverI_01"},{"default", "SilverI_01"},{"rich", "Silver_01"}}},

      {108, new Dictionary<string, string>(){{"ore", "Silver"},{"impure", "SilverI_01"},{"default", "Silver_01"},{"rich", "SilverP_01"}}},
      {112, new Dictionary<string, string>(){{"ore", "Silver"},{"impure", "SilverI_01"},{"default", "Silver_01"},{"rich", "SilverP_01"}}},
      {116, new Dictionary<string, string>(){{"ore", "Silver"},{"impure", "SilverI_01"},{"default", "Silver_01"},{"rich", "SilverP_01"}}},

      {120, new Dictionary<string, string>(){{"ore", "Magnesium"},{"impure", "MagnesiumI_01"},{"default", "MagnesiumI_01"},{"rich", "Magnesium_01"}}},
      {124, new Dictionary<string, string>(){{"ore", "Magnesium"},{"impure", "MagnesiumI_01"},{"default", "MagnesiumI_01"},{"rich", "Magnesium_01"}}},
      {128, new Dictionary<string, string>(){{"ore", "Magnesium"},{"impure", "MagnesiumI_01"},{"default", "MagnesiumI_01"},{"rich", "Magnesium_01"}}},

      {132, new Dictionary<string, string>(){{"ore", "Magnesium"},{"impure", "MagnesiumI_01"},{"default", "Magnesium_01"},{"rich", "MagnesiumP_01"}}},
      {136, new Dictionary<string, string>(){{"ore", "Magnesium"},{"impure", "MagnesiumI_01"},{"default", "Magnesium_01"},{"rich", "MagnesiumP_01"}}},
      {140, new Dictionary<string, string>(){{"ore", "Magnesium"},{"impure", "MagnesiumI_01"},{"default", "Magnesium_01"},{"rich", "MagnesiumP_01"}}},

      {144, new Dictionary<string, string>(){{"ore", "Platinum"},{"impure", "PlatinumI_01"},{"default", "Platinum_01"},{"rich", "PlatinumP_01"}}},
      {148, new Dictionary<string, string>(){{"ore", "Platinum"},{"impure", "PlatinumI_01"},{"default", "Platinum_01"},{"rich", "PlatinumP_01"}}},
      {152, new Dictionary<string, string>(){{"ore", "Platinum"},{"impure", "PlatinumI_01"},{"default", "Platinum_01"},{"rich", "PlatinumP_01"}}},

      {156, new Dictionary<string, string>(){{"ore", "Uranium"},{"impure", "UraniniteI_01"},{"default", "Uraninite_01"},{"rich", "UraniniteP_01"}}},
      {160, new Dictionary<string, string>(){{"ore", "Uranium"},{"impure", "UraniniteI_01"},{"default", "Uraninite_01"},{"rich", "UraniniteP_01"}}},
      {164, new Dictionary<string, string>(){{"ore", "Uranium"},{"impure", "UraniniteI_01"},{"default", "Uraninite_01"},{"rich", "UraniniteP_01"}}},

      {168, new Dictionary<string, string>(){{"ore", "Gold"},{"impure", "GoldI_01"},{"default", "GoldI_01"},{"rich", "Gold_01"}}},
      {172, new Dictionary<string, string>(){{"ore", "Gold"},{"impure", "GoldI_01"},{"default", "GoldI_01"},{"rich", "Gold_01"}}},
      {176, new Dictionary<string, string>(){{"ore", "Gold"},{"impure", "GoldI_01"},{"default", "GoldI_01"},{"rich", "Gold_01"}}},

      {180, new Dictionary<string, string>(){{"ore", "Gold"},{"impure", "GoldI_01"},{"default", "Gold_01"},{"rich", "GoldP_01"}}},
      {184, new Dictionary<string, string>(){{"ore", "Gold"},{"impure", "GoldI_01"},{"default", "Gold_01"},{"rich", "GoldP_01"}}},
      {188, new Dictionary<string, string>(){{"ore", "Gold"},{"impure", "GoldI_01"},{"default", "Gold_01"},{"rich", "GoldP_01"}}},

      {192, new Dictionary<string, string>(){{"ore", "Uranium"},{"impure", "UraniniteI_01"},{"default", "UraniniteI_01"},{"rich", "Uraninite_01"}}},
      {196, new Dictionary<string, string>(){{"ore", "Uranium"},{"impure", "UraniniteI_01"},{"default", "UraniniteI_01"},{"rich", "Uraninite_01"}}},
      {208, new Dictionary<string, string>(){{"ore", "Uranium"},{"impure", "UraniniteI_01"},{"default", "UraniniteI_01"},{"rich", "Uraninite_01"}}},

      {212, new Dictionary<string, string>(){{"ore", "Platinum"},{"impure", "PlatinumI_01"},{"default", "PlatinumI_01"},{"rich", "Platinum_01"}}},
      {217, new Dictionary<string, string>(){{"ore", "Platinum"},{"impure", "PlatinumI_01"},{"default", "PlatinumI_01"},{"rich", "Platinum_01"}}},
      {222, new Dictionary<string, string>(){{"ore", "Platinum"},{"impure", "PlatinumI_01"},{"default", "PlatinumI_01"},{"rich", "Platinum_01"}}}
    };

    public override void LoadData(){
      var allPlanets = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions();

      foreach (var def in allPlanets){
        var planet = def as MyPlanetGeneratorDefinition;
        var oreList = new List<MyPlanetOreMapping>(planet.OreMappings.ToList());

        if (!planetDispositions.ContainsKey(planet.Id.SubtypeName))
          generatePlanetDisposition(planet.Id.SubtypeName);
        Dictionary<string, List<string>> disposition = planetDispositions[planet.Id.SubtypeName];

        //Update ore veins to appropriate purity/ore
        for (int i = 0; i < oreList.Count; i++){
          var oreMap = planet.OreMappings[i];
          if (!oreMapValueToType.ContainsKey(oreMap.Value)){
            Log.Info("Missing value for planet: "+planet.Id.SubtypeName+"\n      Value: "+Convert.ToString(oreMap.Value, 10));
            continue;
          }
          List<string> impures = disposition["impure"];
          List<string> riches = disposition["rich"];

          Dictionary<string, string> oreMapType = oreMapValueToType[oreMap.Value];
          if(impures.Contains(oreMapType["ore"]) == true)
            oreMap.Type = oreMapType["impure"];
          else if(riches.Contains(oreMapType["ore"]) == true)
            oreMap.Type = oreMapType["rich"];
          else
            oreMap.Type = oreMapType["default"];
        }
        planet.OreMappings = oreList.ToArray();
      }

      //No rich ores on asteroids, and only impure of anything important on a planet.
      var allVoxelMaterials = MyDefinitionManager.Static.GetVoxelMaterialDefinitions();
      foreach (var def in allVoxelMaterials){
        var voxelMaterial = def as MyVoxelMaterialDefinition;
        if (voxelMaterial.Id.SubtypeName == "Nickel_01") { voxelMaterial.SpawnsInAsteroids = false; }
        if (voxelMaterial.Id.SubtypeName == "Silver_01") { voxelMaterial.SpawnsInAsteroids = false; }
        if (voxelMaterial.Id.SubtypeName == "Magnesium_01") { voxelMaterial.SpawnsInAsteroids = false; }
        if (voxelMaterial.Id.SubtypeName == "Uraninite_01") { voxelMaterial.SpawnsInAsteroids = false; }
        if (voxelMaterial.Id.SubtypeName == "Gold_01") { voxelMaterial.SpawnsInAsteroids = false; }
        if (voxelMaterial.Id.SubtypeName == "Platinum_01") { voxelMaterial.SpawnsInAsteroids = false; }
      }
    }

    private void generatePlanetDisposition(string SubtypeName){
      //Default modded planets to this for now.
      planetDispositions.Add(SubtypeName, new Dictionary<string, List<string>>(){
        {"rich", new List<string>(){"Cobalt","Magnesium"}},
        {"impure", new List<string>(){"Silver", "Gold", "Iron"}}
      });
    }
    protected override void UnloadData(){
      Log.Info("Closing OreRemapping.");
      Log.Close();
    }
  }
}