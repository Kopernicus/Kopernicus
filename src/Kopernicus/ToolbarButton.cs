using System;
using System.Collections;
using UnityEngine;
using KSP.UI.Screens;
using KSP;
using Kopernicus.RuntimeUtility;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ToolbarButton : MonoBehaviour
    {
        private static ToolbarButton instance;
        bool addedButton = false;
        bool draw = false;
        public ApplicationLauncherButton button;
        public static Rect windowRect;
        public int windowId;
        public bool loaded = false;
        private void Awake()
        {
            windowId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                UnityEngine.Object.Destroy(this);
            }
        }
        public static ToolbarButton Instance
        {
            get
            {
                return instance;
            }
        }
        void Start()
        {
            if (!loaded)
            {
                windowRect = new Rect(RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowXcoord, RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowYcoord, 400, 50);
                loaded = true;
            }
            try
            {
                if (!addedButton)
                {
                    if (HighLogic.LoadedScene.Equals(GameScenes.SPACECENTER))
                    {
                        Texture buttonTexture = GameDatabase.Instance.GetTexture("Kopernicus/Graphics/KopernicusIcon", false);
                        button = ApplicationLauncher.Instance.AddModApplication(ShowToolbarGUI, HideToolbarGUI, DummyFunction, DummyFunction, DummyFunction, DummyFunction, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
                        addedButton = true;
                    }
                    else
                    {
                        addedButton = true;
                    }
                }
            }
            catch
            {
                addedButton = true;
                //No button. :(
            }
        }
        public void ShowToolbarGUI()
        {
            draw = true;
        }
        public void HideToolbarGUI()
        {
            draw = false;
        }
        void OnGUI()
        {
            if (draw)
            {
                windowRect = GUILayout.Window(windowId, windowRect, DrawKopernicusWindow, "Kopernicus " + Kopernicus.Constants.Version.VersionNumber);
            }
        }
        public void DrawKopernicusWindow(int windowId)
        {
            GUILayout.Label("Kopernicus_Config.cfg Editor");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforceShaders = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforceShaders, "EnforceShaders: Whether or not to force the user into EnforcedShaderLevel, not allowing them to change settings.");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.WarnShaders = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.WarnShaders, "WarnShaders: Whether or not to warn the user with a message if not using EnforcedShaderLevel.");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableKopernicusShadowManager = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableKopernicusShadowManager, "EnableKopernicusShadowManager: Whether or not to run the Internal Kopernicus Shadow System. True by default.");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableMainMenuMunScene = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableMainMenuMunScene, " DisableMainMenuMunScene: Whether or not to disable the Mun main menu scene. Only set to false if you actually have a Mun, and want that scene back.");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.HandleHomeworldAtmosphericUnitDisplay = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.HandleHomeworldAtmosphericUnitDisplay, "HandleHomeworldAtmosphericUnitDisplay: This is for calculating 1atm unit at home world.  Normally should be true, but mods like PlanetaryInfoPlus may want to set this false.");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.UseIncorrectScatterDensityLogic = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.UseIncorrectScatterDensityLogic, "UseIncorrectScatterDensityLogic: This is a compatability option for old modpacks that were built with the old (wrong) density logic in mind.  Turn on if scatters seem too dense.");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableFarAwayColliders = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableFarAwayColliders, "DisableFarAwayColliders: Disables distant colliders farther away than stock eeloo. This fixes the distant body sinking bug, but has a slight performance penalty. Advised to use only in larger than stock systems.");
            RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableAtmosphericExtinction = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableAtmosphericExtinction, "EnableAtmosphericExtinction: Whether to use built-in atmospheric extinction effect of lens flares. This is somewhat expensive - O(nlog(n)) on average.");
            GUILayout.Label("EnforcedShaderLevel: A number defining the enforced shader level for the above parameters. 0 = Low, 1 = Medium, 2 = High, 3 = Ultra.");
            try
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel.ToString()));
            }
            catch
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel = 2;
                RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel.ToString()));
            }
            GUILayout.Label("UseKopernicusAsteroidSystem: Three valid values, True, False, and Stock. True means use the old customizable Kopernicus asteroid generator with no comet support,  False means don't do anything/wait for an external generator. Stock means use the internal games generator.");
            GUILayout.BeginHorizontal();
            RuntimeUtility.RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem = GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem.ToString());
            GUILayout.Label("RESTART REQUIRED WHEN CHANGING ASTEROID SPAWNER");
            GUILayout.EndHorizontal();

            GUILayout.Label("SolarRefreshRate: A number defining the number of seconds between EC calculations when using the multistar cfg file. Can be used to finetune performance.");
            try
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate.ToString()));
            }
            catch
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate = 1;
                RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate.ToString()));
            }
            GUILayout.Label("A number defining the maximum distance at which shadows may be cast. Lower numbers yield less shadow cascading artifacts.");
            try
            {
                GUILayout.BeginHorizontal();
                RuntimeUtility.RuntimeUtility.KopernicusConfig.ShadowDistanceLimit = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.ShadowDistanceLimit.ToString()));
                GUILayout.Label("SCENE SWITCH REQUIRED WHEN CHANGING THIS SETTING");
                GUILayout.EndHorizontal();
            }
            catch
            {
                GUILayout.BeginHorizontal();
                RuntimeUtility.RuntimeUtility.KopernicusConfig.ShadowDistanceLimit = 25000;
                GUILayout.Label("SCENE SWITCH REQUIRED WHEN CHANGING THIS SETTING");
                GUILayout.EndHorizontal();
            }
            GUI.DragWindow();
            RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowXcoord = windowRect.x;
            RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowYcoord = windowRect.y;
        }
        void OnDestroy()
        {
            RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowXcoord = windowRect.x;
            RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowYcoord = windowRect.y;
            if (button)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
        }
        void DummyFunction()
        {

        }
    }
}
