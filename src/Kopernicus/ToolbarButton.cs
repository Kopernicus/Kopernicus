using System;
using System.Collections;
using UnityEngine;
using KSP.UI.Screens;
using KSP;
using Kopernicus.RuntimeUtility;
using Expansions.Missions;
using static Targeting;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Security.AccessControl;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ToolbarButton : MonoBehaviour
    {
        private static ToolbarButton instance;
        bool addedButton = false;
        bool draw = false;
        private ApplicationLauncherButton button;
        private static Rect windowRect;
        private int windowId;
        private bool loaded = false;
        private GUIStyle labelStyle;
        private GUIStyle toggleStyle;
        private GUIStyle boxStyle;
        private int fontSize = 12;
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
                windowRect = new Rect(RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowXcoord, RuntimeUtility.RuntimeUtility.KopernicusConfig.SettingsWindowYcoord, 400 * GameSettings.UI_SCALE, 50 * GameSettings.UI_SCALE);
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
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.UpperLeft;
            labelStyle.wordWrap = false;
            labelStyle.fontSize = (int)Math.Round((double)((float)fontSize * GameSettings.UI_SCALE));
            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.alignment = TextAnchor.UpperLeft;
            toggleStyle.wordWrap = false;
            toggleStyle.fontSize = (int)Math.Round((double)((float)fontSize * GameSettings.UI_SCALE));
            boxStyle = new GUIStyle(GUI.skin.textField);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.wordWrap = false;
            boxStyle.fontSize = (int)Math.Round((double)((float)fontSize * GameSettings.UI_SCALE));
            GUILayout.Label("Kopernicus_Config.cfg Editor", labelStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforceShaders = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforceShaders, "EnforceShaders: Whether or not to force the user into EnforcedShaderLevel, not allowing them to change settings.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.WarnShaders = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.WarnShaders, "WarnShaders: Whether or not to warn the user with a message if not using EnforcedShaderLevel.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableKopernicusShadowManager = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableKopernicusShadowManager, "EnableKopernicusShadowManager: Whether or not to run the Internal Kopernicus Shadow System. True by default.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableMainMenuMunScene = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableMainMenuMunScene, " DisableMainMenuMunScene: Whether or not to disable the Mun main menu scene. Only set to false if you actually have a Mun, and want that scene back.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.HandleHomeworldAtmosphericUnitDisplay = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.HandleHomeworldAtmosphericUnitDisplay, "HandleHomeworldAtmosphericUnitDisplay: This is for calculating 1atm unit at home world.  Normally should be true, but mods like PlanetaryInfoPlus may want to set this false.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.UseIncorrectScatterDensityLogic = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.UseIncorrectScatterDensityLogic, "UseIncorrectScatterDensityLogic: This is a compatability option for old modpacks that were built with the old (wrong) density logic in mind.  Turn on if scatters seem too dense.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableFarAwayColliders = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.DisableFarAwayColliders, "DisableFarAwayColliders: Disables distant colliders farther away than stock eeloo. This fixes the distant body sinking bug, but has a slight performance penalty. Advised to disable only in stock or smaller radius systems.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableAtmosphericExtinction = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableAtmosphericExtinction, "EnableAtmosphericExtinction: Whether to use built-in atmospheric extinction effect of lens flares. This is somewhat expensive - O(nlog(n)) on average.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.UseStockMohoTemplate = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.UseStockMohoTemplate, "UseStockMohoTemplate: This uses the stock Moho template with the Mohole bug / feature.Planet packs may customize this as desired.Be aware disabling this disables the Mohole.", toggleStyle);
            RuntimeUtility.RuntimeUtility.KopernicusConfig.ResetFloatingOriginOnKSCReturn = GUILayout.Toggle(RuntimeUtility.RuntimeUtility.KopernicusConfig.ResetFloatingOriginOnKSCReturn, "ResetFloatingOriginOnKSCReturn: Enable this for interstaller (LY+) range planet packs to prevent corruption on return to KSC.", toggleStyle);
            GUILayout.Label("EnforcedShaderLevel: A number defining the enforced shader level for the above parameters. 0 = Low, 1 = Medium, 2 = High, 3 = Ultra.", labelStyle);
            try
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel.ToString()));
            }
            catch
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel = 2;
                RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.EnforcedShaderLevel.ToString()));
            }
            GUILayout.Label("UseKopernicusAsteroidSystem: Three valid values, True, False, and Stock. True means use the old customizable Kopernicus asteroid generator with no comet support,  False means don't do anything/wait for an external generator. Stock means use the internal games generator.", labelStyle);
            GUILayout.BeginHorizontal();
            RuntimeUtility.RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem = GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.UseKopernicusAsteroidSystem.ToString());
            GUILayout.Label("RESTART REQUIRED WHEN CHANGING ASTEROID SPAWNER", labelStyle);
            GUILayout.EndHorizontal();

            GUILayout.Label("SolarRefreshRate: A number defining the number of seconds between EC calculations when using the multistar cfg file. Can be used to finetune performance.", labelStyle);
            try
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate.ToString()));
            }
            catch
            {
                RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate = 1;
                RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate.ToString()));
            }
            GUILayout.Label("A number defining the maximum distance at which shadows may be cast. Lower numbers yield less shadow cascading artifacts.", labelStyle);
            try
            {
                GUILayout.BeginHorizontal();
                RuntimeUtility.RuntimeUtility.KopernicusConfig.ShadowRangeLimit = (int)Convert.ToInt32(GUILayout.TextField(RuntimeUtility.RuntimeUtility.KopernicusConfig.ShadowRangeLimit.ToString()));
                GUILayout.Label("SCENE SWITCH REQUIRED WHEN CHANGING THIS SETTING", labelStyle);
                GUILayout.EndHorizontal();
            }
            catch
            {
                GUILayout.BeginHorizontal();
                RuntimeUtility.RuntimeUtility.KopernicusConfig.ShadowRangeLimit = 10000;
                GUILayout.Label("SCENE SWITCH REQUIRED WHEN CHANGING THIS SETTING", labelStyle);
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
