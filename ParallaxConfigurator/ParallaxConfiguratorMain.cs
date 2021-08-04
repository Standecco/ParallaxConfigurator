using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Parallax;
using ParallaxQualityLibrary;
using UniLinq;
using UnityEngine;

namespace ParallaxConfigurator
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ParallaxConfiguratorMain : MonoBehaviour
    {
        public bool ShowUI { get; set; }
        
        private ParallaxInFlightOperations ParallaxInFlight { get; set; }
        private static ParallaxBody parallaxBody;
        private static Rect         window       = new Rect(100, 100, 450, 200);
        private static bool         showTextures = false;
        private static string       lastBodyName;
        private static bool         anyValueHasChanged;


        public void Start()
        {
            CelestialBody currentBody = FlightGlobals.currentMainBody;
            lastBodyName = currentBody.name;
            parallaxBody = ParallaxBodies.parallaxBodies[currentBody.name];
            ParallaxInFlight = FindObjectOfType<ParallaxInFlightOperations>();
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
                ShowUI = !ShowUI;

            if (lastBodyName != FlightGlobals.currentMainBody?.name)
                this.Start();

            if (anyValueHasChanged)
            {
                anyValueHasChanged = false;
                UpdateShaderValues();
            }
        }

        public void OnGUI()
        {
            if (!ShowUI)
                return;

            window = GUILayout.Window(GetInstanceID(), window, DrawWindow, "Parallax Configurator");
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Save current config to ParallaxConfigurator folder"))
            {
                SaveConfigs(parallaxBody.bodyName + ".txt");
            }

            parallaxBody._SurfaceTextureScale = TextAreaLabelFloat("Surface texture scale", parallaxBody._SurfaceTextureScale);
            parallaxBody._displacement_offset = TextAreaLabelFloat("Displacement offset", parallaxBody._displacement_offset);
            parallaxBody._displacement_scale = TextAreaLabelFloat("Displacement scale", parallaxBody._displacement_scale);
            parallaxBody._EmissionColor = TextAreaLabelColor("Emission color", parallaxBody._EmissionColor);
            parallaxBody._Gloss = TextAreaLabelFloat("Gloss", parallaxBody._Gloss);
            parallaxBody._Hapke = TextAreaLabelFloat("Hapke", parallaxBody._Hapke);
            parallaxBody._Metallic = TextAreaLabelFloat("Metallic", parallaxBody._Metallic);
            parallaxBody._MetallicTint = TextAreaLabelColor("Metallic tint", parallaxBody._MetallicTint);
            parallaxBody._NormalSpecularInfluence = TextAreaLabelFloat("Normal specular influence", parallaxBody._NormalSpecularInfluence);
            parallaxBody._LowStart = TextAreaLabelFloat("Low start", parallaxBody._LowStart);
            parallaxBody._LowEnd = TextAreaLabelFloat("Low end", parallaxBody._LowEnd);
            parallaxBody._HighStart = TextAreaLabelFloat("High start", parallaxBody._HighEnd);
            parallaxBody._HighEnd = TextAreaLabelFloat("High end", parallaxBody._HighEnd);
            parallaxBody._SteepContrast = TextAreaLabelFloat("Steep contrast", parallaxBody._SteepContrast);
            parallaxBody._SteepMidpoint = TextAreaLabelFloat("Steep midpoint", parallaxBody._SteepMidpoint);
            parallaxBody._SteepPower = TextAreaLabelFloat("Steep power", parallaxBody._SteepPower);

            GUILayout.EndVertical();

            if (showTextures != GUILayout.Toggle(showTextures, "Show Textures"))
            {
                showTextures = !showTextures;
                window = new Rect(window.position.x, window.position.y, 450, 200);
            }

            if (showTextures)
            {
                GUILayout.BeginVertical();

                parallaxBody._BumpMap = TextAreaLabelTexture("Bump map", parallaxBody._BumpMap);
                parallaxBody._BumpMapMid = TextAreaLabelTexture("Bump map mid", parallaxBody._BumpMapMid);
                parallaxBody._BumpMapHigh = TextAreaLabelTexture("Bump map high", parallaxBody._BumpMapSteep);
                parallaxBody._BumpMapSteep = TextAreaLabelTexture("Bump map steep", parallaxBody._BumpMapSteep);
                parallaxBody._SteepTex = TextAreaLabelTexture("Steep tex", parallaxBody._SteepTex);
                parallaxBody._SurfaceTexture = TextAreaLabelTexture("Surface texture", parallaxBody._SurfaceTexture);
                parallaxBody._SurfaceTextureHigh = TextAreaLabelTexture("Surface texture high", parallaxBody._SurfaceTextureHigh);
                parallaxBody._SurfaceTextureMid = TextAreaLabelTexture("Surface texture mid", parallaxBody._SurfaceTextureMid);
                parallaxBody._DispTex = TextAreaLabelTexture("Disp tex", parallaxBody._DispTex);
                parallaxBody._InfluenceMap = TextAreaLabelTexture("Influence map", parallaxBody._InfluenceMap);

                GUILayout.EndVertical();
            }

            GUI.DragWindow();
        }

        private static string TextAreaLabelTexture(string label, string value)
        {
            GUILayout.BeginHorizontal();
            string newValue = InputFields.TexField(new GUIContent(label + " [Texture] "), value);
            GUILayout.EndHorizontal();

            if (newValue != value)
                anyValueHasChanged = true;

            return newValue;
        }

        private static float TextAreaLabelFloat(string label, float value)
        {
            GUILayout.BeginHorizontal();
            float newValue = InputFields.FloatField(new GUIContent(label + " [float] "), value);
            GUILayout.EndHorizontal();

            if (Math.Abs(newValue - value) > 0.001)
                anyValueHasChanged = true;

            return newValue;
        }

        private static Color TextAreaLabelColor(string label, Color value)
        {
            GUILayout.BeginHorizontal();
            Color newValue = InputFields.ColorField(new GUIContent(label + " [Color] "), value);
            GUILayout.EndHorizontal();

            if (newValue != value)
                anyValueHasChanged = true;

            return newValue;
        }

        private void UpdateShaderValues()
        {
            Debug.Log("[ParallaxConfigurator] Updating Parallax materials");
            IEnumerable<PropertyInfo> parallaxShaders = parallaxBody.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(ParallaxQualityLibrary.Parallax));

            foreach (PropertyInfo shader in parallaxShaders)
            {
                ParallaxQualityLibrary.Parallax physicalShader =
                    (ParallaxQualityLibrary.Parallax)shader.GetValue(parallaxBody);
                foreach (string shaderVar in physicalShader.shaderVars)
                {
                    string valueToReplaceWith = shaderVar;
                    if (physicalShader.specificVars.ContainsKey(shaderVar))
                    {
                        valueToReplaceWith = physicalShader.specificVars[shaderVar];
                    }

                    object storedVar = parallaxBody.GetType().GetProperty(valueToReplaceWith).GetValue(parallaxBody);
                    var thisShaderInstance =
                        (ParallaxQualityLibrary.Parallax)parallaxBody.GetType().GetProperty(shader.Name)
                            .GetValue(parallaxBody);
                    var shaderMat = (Material)thisShaderInstance.GetType().GetProperty("parallaxMaterial")
                        .GetValue(thisShaderInstance);
                    Type propertyType = parallaxBody.GetType().GetProperty(shaderVar).GetType();
                    ParallaxInFlight.ConvertAndSetMaterialType(shaderMat, storedVar, propertyType, shaderVar);
                }
            }
        }

        private void SaveConfigs(string fileName)
        {
            string url = "ParallaxConfigurator/" + fileName;
            string path = KSPUtil.ApplicationRootPath + "GameData/" + url;

            if (System.IO.File.Exists(path))
            {
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new MultiOptionDialog("ParallaxConfiguratorOverwrite",
                        $"File \"{url}\" already exists!",
                        "Warning",
                        HighLogic.UISkin,
                        new Rect(0.5f, 0.5f, 240f, 60f),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIVerticalLayout(
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Append", () => WriteConfigToFile(path, true), 230.0f, 30.0f, true),
                            new DialogGUIButton("Override", () => WriteConfigToFile(path, false), 230.0f, 30.0f, true)
                        )),
                    false,
                    HighLogic.UISkin);
            }
            else
            {
                WriteConfigToFile(path, false);
            }
        }

        private void WriteConfigToFile(string path, bool append)
        {
            Debug.Log("[ParallaxConfigurator] " + (append ? "Appending config to " : "Overwriting file ") + path);
            using (var file = new StreamWriter(path, append))
            {
                file.WriteLine("// CONFIGS FROM " + DateTime.Now);
                file.WriteLine("Textures");
                file.WriteLine("{");
                foreach (var propertyInfo in parallaxBody.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!propertyInfo.Name.StartsWith("_"))
                        continue;

                    string line = "\t" + propertyInfo.Name + " = ";

                    if (propertyInfo.PropertyType == typeof(Color))
                    {
                        var col = (Color)propertyInfo.GetValue(parallaxBody);

                        line += $"{col.r}, {col.g}, {col.b}, {col.a}";
                    }
                    else
                    {
                        line += propertyInfo.GetValue(parallaxBody).ToString();
                    }

                    file.WriteLine(line);
                }

                file.WriteLine("}");
            }

            ScreenMessages.PostScreenMessage($"Configs {(append ? "appended" : "overwritten")} to {path}", 5, ScreenMessageStyle.UPPER_RIGHT, false);
        }
    }
}