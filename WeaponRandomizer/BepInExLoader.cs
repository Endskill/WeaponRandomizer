using BepInEx;
using BepInEx.IL2CPP;
using CellMenu;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace WeaponRandomizer
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class BepInExLoader : BasePlugin
    {
        public const string
         MODNAME = "WeaponRandomizer",
         AUTHOR = "Endskill",
         GUID = AUTHOR + "." + MODNAME,
         VERSION = "1.1.1";

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<Entry>();
            //GameObject gameObject = new GameObject("CheatMenu");
            //gameObject.AddComponent<Entry>();
            //UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)gameObject);

            Console.WriteLine("Patching All ...");
            var harmony = new Harmony(GUID);
            harmony.PatchAll();
            Console.WriteLine("Patched");
        }
        [HarmonyPatch(typeof(CM_PageIntro), "Setup")]
        public class PrepareInjection
        {
            public static GameObject _obj;
            [HarmonyPostfix]
            public static void PostFix()
            {
                var _obj = new GameObject();
                _obj.AddComponent<Entry>();
                UnityEngine.Object.DontDestroyOnLoad(_obj);
            }
        }
    }
}
