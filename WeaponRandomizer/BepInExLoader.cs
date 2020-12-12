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
using WeaponRandomizer.Overlay;

namespace WeaponRandomizer
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class BepInExLoader : BasePlugin
    {
        public const string
         MODNAME = "WeaponRandomizer",
         AUTHOR = "Endskill",
         GUID = AUTHOR + "." + MODNAME,
         VERSION = "1.1";

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<InputHandler>();

            var harmony = new Harmony("WeaponRandomizer");

            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(CM_PageIntro), "Setup")]
    public class PrepareInjection
    {
        public static GameObject _obj;
        [HarmonyPostfix]
        public static void PostFix()
        {
            var _obj = new GameObject();
            _obj.AddComponent<InputHandler>();
            UnityEngine.Object.DontDestroyOnLoad(_obj);
        }
    }
}
