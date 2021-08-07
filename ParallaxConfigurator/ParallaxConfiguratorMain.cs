using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Parallax;
using ParallaxQualityLibrary;
using PQSModExpansion;
using UniLinq;
using UnityEngine;
using UnityEngine.UI;

namespace ParallaxConfigurator
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ParallaxConfiguratorMain : MonoBehaviour
    {
        public static  ParallaxBody     ParallaxBody;
        public static  PQSMod_Subdivide Subdivision;
        private static Rect             window       = new Rect(100, 100, 450, 200);
        private static bool             showTextures = false;
        private static bool             showPqs      = false;
        private static string           lastBodyName;
        private static bool             anyValueHasChanged;
        private static bool             firstRun = true;

        public static Dictionary<string, ParallaxBody> ParallaxBodiesOriginal;

        private static readonly Dictionary<string, PropertyInfo> VarProperties     = new Dictionary<string, PropertyInfo>();
        private static readonly Dictionary<string, PropertyInfo> TextureProperties = new Dictionary<string, PropertyInfo>();
        public static bool ShowUI { get; set; }

        private static ParallaxInFlightOperations ParallaxInFlight { get; set; }

        private static Dictionary<string, string> Labels => Utils.LabelFromVar;

        public void Start()
        {
            CelestialBody currentBody = FlightGlobals.currentMainBody;
            lastBodyName = currentBody.name;

            ParallaxBody = ParallaxBodies.parallaxBodies.ContainsKey(currentBody.name) ? ParallaxBodies.parallaxBodies[currentBody.name] : null;

            ParallaxInFlight = FindObjectOfType<ParallaxInFlightOperations>();

            foreach (PQSMod mod in currentBody.GetComponentsInChildren<PQSMod>())
            {
                if (mod is PQSMod_Subdivide subdivide)
                {
                    Subdivision = subdivide;
                }
            }

            if (firstRun)
            {
                firstRun = false;
                SaveDefaultVars();
                GetPropertyInfos();
            }
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
            {
                if (ParallaxBody != null)
                    ShowUI = !ShowUI;
                else
                {
                    PopupDialog.SpawnPopupDialog(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        "ParallaxConfiguratorInvalidBody",
                        "Error",
                        $"Body {FlightGlobals.currentMainBody.name} is not parallax configured",
                        "Close",
                        false,
                        HighLogic.UISkin
                        );
                }
            }

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

        private static void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Save current config to ParallaxConfigurator folder"))
            {
                SaveConfigs(ParallaxBody.bodyName + ".txt");
            }
            if (GUILayout.Button("Load config from file"))
            {
                LoadConfigs(ParallaxBody.bodyName + ".txt");
            }

            ParallaxBody._SurfaceTextureScale = TextAreaLabelFloat(Labels[nameof(ParallaxBody._SurfaceTextureScale)], ParallaxBody._SurfaceTextureScale);
            ParallaxBody._displacement_offset = TextAreaLabelFloat(Labels[nameof(ParallaxBody._displacement_offset)], ParallaxBody._displacement_offset);
            ParallaxBody._displacement_scale = TextAreaLabelFloat(Labels[nameof(ParallaxBody._displacement_scale)], ParallaxBody._displacement_scale);
            ParallaxBody._EmissionColor = TextAreaLabelColor(Labels[nameof(ParallaxBody._EmissionColor)], ParallaxBody._EmissionColor);
            ParallaxBody._Gloss = TextAreaLabelFloat(Labels[nameof(ParallaxBody._Gloss)], ParallaxBody._Gloss);
            ParallaxBody._Hapke = TextAreaLabelFloat(Labels[nameof(ParallaxBody._Hapke)], ParallaxBody._Hapke);
            ParallaxBody._Metallic = TextAreaLabelFloat(Labels[nameof(ParallaxBody._Metallic)], ParallaxBody._Metallic);
            ParallaxBody._MetallicTint = TextAreaLabelColor(Labels[nameof(ParallaxBody._MetallicTint)], ParallaxBody._MetallicTint);
            ParallaxBody._NormalSpecularInfluence = TextAreaLabelFloat(Labels[nameof(ParallaxBody._NormalSpecularInfluence)], ParallaxBody._NormalSpecularInfluence);
            ParallaxBody._LowStart = TextAreaLabelFloat(Labels[nameof(ParallaxBody._LowStart)], ParallaxBody._LowStart);
            ParallaxBody._LowEnd = TextAreaLabelFloat(Labels[nameof(ParallaxBody._LowEnd)], ParallaxBody._LowEnd);
            ParallaxBody._HighStart = TextAreaLabelFloat(Labels[nameof(ParallaxBody._HighStart)], ParallaxBody._HighStart);
            ParallaxBody._HighEnd = TextAreaLabelFloat(Labels[nameof(ParallaxBody._HighEnd)], ParallaxBody._HighEnd);
            ParallaxBody._SteepContrast = TextAreaLabelFloat(Labels[nameof(ParallaxBody._SteepContrast)], ParallaxBody._SteepContrast);
            ParallaxBody._SteepMidpoint = TextAreaLabelFloat(Labels[nameof(ParallaxBody._SteepMidpoint)], ParallaxBody._SteepMidpoint);
            ParallaxBody._SteepPower = TextAreaLabelFloat(Labels[nameof(ParallaxBody._SteepPower)], ParallaxBody._SteepPower);

            GUILayout.EndVertical();

            if (showTextures != GUILayout.Toggle(showTextures, "Show Textures"))
            {
                showTextures = !showTextures;
                window = new Rect(window.position.x, window.position.y, 450, 200);
            }

            if (showTextures)
            {
                GUILayout.BeginVertical();

                ParallaxBody._BumpMap = TextAreaLabelTexture(Labels[nameof(ParallaxBody._BumpMap)], ParallaxBody._BumpMap);
                ParallaxBody._BumpMapMid = TextAreaLabelTexture(Labels[nameof(ParallaxBody._BumpMapMid)], ParallaxBody._BumpMapMid);
                ParallaxBody._BumpMapHigh = TextAreaLabelTexture(Labels[nameof(ParallaxBody._BumpMapHigh)], ParallaxBody._BumpMapHigh);
                ParallaxBody._BumpMapSteep = TextAreaLabelTexture(Labels[nameof(ParallaxBody._BumpMapSteep)], ParallaxBody._BumpMapSteep);
                ParallaxBody._SteepTex = TextAreaLabelTexture(Labels[nameof(ParallaxBody._SteepTex)], ParallaxBody._SteepTex);
                ParallaxBody._SurfaceTexture = TextAreaLabelTexture(Labels[nameof(ParallaxBody._SurfaceTexture)], ParallaxBody._SurfaceTexture);
                ParallaxBody._SurfaceTextureHigh = TextAreaLabelTexture(Labels[nameof(ParallaxBody._SurfaceTextureHigh)], ParallaxBody._SurfaceTextureHigh);
                ParallaxBody._SurfaceTextureMid = TextAreaLabelTexture(Labels[nameof(ParallaxBody._SurfaceTextureMid)], ParallaxBody._SurfaceTextureMid);
                ParallaxBody._DispTex = TextAreaLabelTexture(Labels[nameof(ParallaxBody._DispTex)], ParallaxBody._DispTex);
                ParallaxBody._InfluenceMap = TextAreaLabelTexture(Labels[nameof(ParallaxBody._InfluenceMap)], ParallaxBody._InfluenceMap);

                GUILayout.EndVertical();
            }

            if (Subdivision != null)
            {
                if (showPqs != GUILayout.Toggle(showPqs, "Show Subdivision parameters (only for testing, can't be saved or loaded)"))
                {
                    showPqs = !showPqs;
                    window = new Rect(window.position.x, window.position.y, 450, 200);
                }

                if (showPqs)
                {
                    GUILayout.BeginVertical();

                    Subdivision.subdivisionLevel = TextAreaLabelInt("Subdivision level", Subdivision.subdivisionLevel);
                    Subdivision.advancedSubdivisionLevel = TextAreaLabelInt("Advanced Subdivision level", Subdivision.advancedSubdivisionLevel);

                    // TODO: rebuild PQS
                    GUILayout.EndVertical();
                }
            }
            else
            {
                GUILayout.Label("Subdivision PQSMod not found. Make sure you add one.");
            }

            GUI.DragWindow();
        }

        private static string TextAreaLabelTexture(string label, string value)
        {
            GUILayout.BeginHorizontal();
            string newValue = InputFields.TexField(label, value);
            GUILayout.EndHorizontal();

            if (newValue != value)
                anyValueHasChanged = true;

            return newValue;
        }

        private static float TextAreaLabelFloat(string label, float value)
        {
            GUILayout.BeginHorizontal();
            float newValue = InputFields.FloatField(label, value);
            GUILayout.EndHorizontal();

            if (Math.Abs(newValue - value) > 0.001)
                anyValueHasChanged = true;

            return newValue;
        }

        private static int TextAreaLabelInt(string label, int value)
        {
            GUILayout.BeginHorizontal();
            int newValue = InputFields.IntField(label, value);
            GUILayout.EndHorizontal();

            return newValue;
        }

        private static Color TextAreaLabelColor(string label, Color value)
        {
            GUILayout.BeginHorizontal();
            Color newValue = InputFields.ColorField(label, value);
            GUILayout.EndHorizontal();

            if (newValue != value)
                anyValueHasChanged = true;

            return newValue;
        }

        private static void SaveDefaultVars()
        {
            ParallaxBodiesOriginal = new Dictionary<string, ParallaxBody>(ParallaxBodies.parallaxBodies);
            foreach (var body in ParallaxBodies.parallaxBodies)
            {
                ParallaxBodiesOriginal[body.Key] = Utils.Clone(body.Value);
            }
        }

        private static void GetPropertyInfos()
        {
            foreach (PropertyInfo propertyInfo in typeof(ParallaxBody).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!propertyInfo.Name.StartsWith("_"))
                    continue;

                if (propertyInfo.PropertyType == typeof(string))
                {
                    TextureProperties[propertyInfo.Name] = propertyInfo;
                }
                else
                {
                    VarProperties[propertyInfo.Name] = propertyInfo;
                }
            }
        }

        private static void UpdateShaderValues()
        {
            Type bodyType = typeof(ParallaxBody);
            Type parallaxShaderType = typeof(ParallaxQualityLibrary.Parallax);

            Debug.Log("[ParallaxConfigurator] Updating Parallax materials");

            // change all shaders regardless of current quality
            IEnumerable<PropertyInfo> parallaxShaderInfos = bodyType.GetProperties().Where(p => p.PropertyType == parallaxShaderType);

            foreach (PropertyInfo shaderInfo in parallaxShaderInfos)
            {
                // get value object of the examined parallax shader
                var thisParallaxShader = (ParallaxQualityLibrary.Parallax) shaderInfo.GetValue(ParallaxBody);

                foreach (string shaderVar in thisParallaxShader.shaderVars)
                {
                    string valueToReplaceWith = shaderVar;
                    if (thisParallaxShader.specificVars.ContainsKey(shaderVar))
                    {
                        valueToReplaceWith = thisParallaxShader.specificVars[shaderVar];
                    }

                    // get value object for this variable
                    object storedVar = bodyType.GetProperty(valueToReplaceWith)?.GetValue(ParallaxBody);
                    // get material for the examined parallax shader
                    var shaderMat = (Material) parallaxShaderType.GetProperty("parallaxMaterial")?.GetValue(thisParallaxShader);
                    // get type of the property (float, Color or string)
                    Type propertyType = bodyType.GetProperty(shaderVar)?.GetType();

                    // only ask to change textures if a texture has actually changed (prevent excess texture loadings/unloadings)
                    ParallaxInFlight.ConvertAndSetMaterialType(shaderMat, storedVar, propertyType, shaderVar);
                }
            }
        }

        private static void SaveConfigs(string fileName)
        {
            string url = "ParallaxConfigurator/" + fileName;
            string path = KSPUtil.ApplicationRootPath + "GameData/" + url;

            if (File.Exists(path))
            {
                PopupDialog.SpawnPopupDialog(
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new MultiOptionDialog(
                        "ParallaxConfiguratorOverwrite",
                        $"File \"{url}\" already exists!",
                        "Warning",
                        HighLogic.UISkin,
                        new Rect(0.5f, 0.5f, 240f, 60f),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIVerticalLayout(
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Append", () => WriteConfigToFile(path, true), 230.0f, 30.0f, true),
                            new DialogGUIButton("Overwrite", () => WriteConfigToFile(path, false), 230.0f, 30.0f, true)
                            )
                        ),
                    false,
                    HighLogic.UISkin
                    );
            }
            else
            {
                WriteConfigToFile(path, false);
            }
        }

        private static void WriteConfigToFile(string path, bool append)
        {
            Debug.Log("[ParallaxConfigurator] " + (append ? "Appending config to " : "Overwriting file ") + path);

            using (var file = new StreamWriter(path, append))
            {
                file.WriteLine("// CONFIGS FROM " + DateTime.Now);
                file.WriteLine("Textures");
                file.WriteLine("{");

                foreach (PropertyInfo varProperty in VarProperties.Values)
                {
                    string line = "\t" + varProperty.Name + " = ";

                    if (varProperty.PropertyType == typeof(Color))
                    {
                        var col = (Color) varProperty.GetValue(ParallaxBody);

                        line += $"{col.r}, {col.g}, {col.b}, {col.a}";
                    }
                    else
                    {
                        line += varProperty.GetValue(ParallaxBody).ToString();
                    }

                    file.WriteLine(line);
                }
                foreach (PropertyInfo texProperty in TextureProperties.Values)
                {
                    string line = "\t" + texProperty.Name + " = ";

                    line += texProperty.GetValue(ParallaxBody);

                    file.WriteLine(line);
                }

                file.WriteLine("}");
            }

            ScreenMessages.PostScreenMessage($"Configs {(append ? "appended" : "overwritten")} to {path}", 5, ScreenMessageStyle.UPPER_RIGHT, false);
        }

        private static void LoadConfigs(string fileName)
        {
            string url = "ParallaxConfigurator/" + fileName;
            string path = KSPUtil.ApplicationRootPath + "GameData/" + url;

            if (!File.Exists(path))
            {
                PopupDialog.SpawnPopupDialog(
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    "ParallaxConfiguratorFileMissing",
                    "Warning",
                    $"File \"{url}\" doesn't exist!",
                    "Close",
                    false,
                    HighLogic.UISkin
                    );
            }
            else
            {
                ReadFileAndLoadValues(path);
            }
        }

        private static void ReadFileAndLoadValues(string path)
        {
            int configs = 0;
            bool loaded = false;

            string[] lines = File.ReadAllLines(path);
            int bracketCounter = 0;

            // count existing configs
            foreach (var line in lines)
            {
                if (line.StartsWith("{")) // config started
                {
                    bracketCounter++;
                }
                else if (bracketCounter > 0 && line.StartsWith("}")) // config ended
                {
                    bracketCounter--;

                    // prevent rogue brackets
                    if (bracketCounter == 0)
                        configs++;
                }
            }

            if (configs == 1)
            {
                loaded = ReadAndLoadConfigFromFile(lines, 0);
            }
            else if (configs > 1)
            {
                SpawnConfigSelectionPopupDialog(lines, configs - 1);
                // check is done later
                loaded = true;
            }

            if (configs == 0 || !loaded)
            {
                ScreenMessages.PostScreenMessage("Invalid file", 4, ScreenMessageStyle.UPPER_RIGHT, false);
            }
        }

        private static void SpawnConfigSelectionPopupDialog(string[] lines, int maxIndex)
        {
            var dialog = new List<DialogGUIBase>();
            var buttons = new List<DialogGUIHorizontalLayout>();

            // add button for each config found
            for (int i = maxIndex; i >= 0; i--)
            {
                var button = new DialogGUIButton<int>(GetDateFromConfigIndex(lines, i, maxIndex), (int n) => ReadAndLoadConfigFromFile(lines, n), i, true);
                var h = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, button);

                buttons.Add(h);
            }

            // create scroll list from buttons
            var scrollList = new DialogGUIBase[buttons.Count + 1];
            scrollList[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < buttons.Count; i++)
            {
                scrollList[i + 1] = buttons[i];
            }

            // add scroll list to dialog
            dialog.Add(new DialogGUIScrollList(Vector2.one, false, true,
                new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleLeft, scrollList)
                ));

            // add spacing and cancel button
            dialog.Add(new DialogGUISpace(4));
            dialog.Add(new DialogGUIHorizontalLayout(
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIButton("Cancel", delegate {}),
                    new DialogGUIFlexibleSpace()
                    )
                );

            PopupDialog.SpawnPopupDialog(
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new MultiOptionDialog(
                    "ParallaxConfiguratorMultipleConfigs",
                    "Choose a config:",
                    $"Detected {maxIndex + 1} configs",
                    HighLogic.UISkin,
                    new Rect(0.5f, 0.5f, 300f, 200f),
                    dialog.ToArray()
                    ),
                false,
                HighLogic.UISkin
                );
        }

        private static string GetDateFromConfigIndex(string[] lines, int index, int maxValue)
        {
            string date = null;

            string[] dateLines = lines.Where(l => l.StartsWith("// CONFIGS FROM ")).ToArray();

            if (index < dateLines.Length)
            {
                date = dateLines[index]?.Remove(0, 16);

                if (index == maxValue)
                    date += " (latest)";
            }

            return date ?? "invalid";
        }

        private static bool ReadAndLoadConfigFromFile(string[] lines, int index)
        {
            int counter = -1;
            bool valid = false;
            Debug.Log($"[ParallaxConfigurator] Parsing config #{index}");

            foreach (string line in lines)
            {
                if (line.StartsWith("{"))
                    counter++;

                // ignore line if it is not in the right config or if it is a comment
                if (counter != index || line.StartsWith("//"))
                    continue;

                // if config is over, break
                if (line.StartsWith("}"))
                {
                    valid = true;
                    break;
                }

                // remove partial comments
                int commentIndex;
                if ((commentIndex = line.IndexOf("//", StringComparison.Ordinal)) != -1)
                    line.Remove(commentIndex);

                string[] array = line.Split('=');
                if (array.Length != 2)
                    continue;

                SetVariableValueFromName(array[0].Trim(), array[1].Trim());
            }

            if (!valid)
            {
                ScreenMessages.PostScreenMessage("Unable to read file", 4, ScreenMessageStyle.UPPER_RIGHT, false);
                Debug.Log($"[ParallaxConfigurator] unable to read config #{index}");
                return false;
            }

            UpdateShaderValues();
            return true;
        }

        private static void SetVariableValueFromName(string varName, string value)
        {
            if (VarProperties.ContainsKey(varName))
            {
                PropertyInfo propInfo = VarProperties[varName];

                if (propInfo.PropertyType == typeof(Color))
                {
                    propInfo.SetValue(ParallaxBody, value.ForceParseColor(new Color()));
                }
                else
                {
                    propInfo.SetValue(ParallaxBody, value.ForceParseFloat());
                }
            }
            else if (TextureProperties.ContainsKey(varName))
            {
                PropertyInfo propInfo = TextureProperties[varName];

                propInfo.SetValue(ParallaxBody, value);
            }
        }
    }
}