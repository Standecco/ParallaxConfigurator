using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LibNoise.Models;
using Parallax;
using ParallaxQualityLibrary;
using UniLinq;
using UnityEngine;

namespace ParallaxConfigurator
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ParallaxConfiguratorMain : MonoBehaviour
    {
        public static bool ShowUI { get; set; }

        private static ParallaxInFlightOperations ParallaxInFlight { get; set; }
        private static ParallaxBody  parallaxBody;
        private static Rect          window       = new Rect(100, 100, 450, 200);
        private static bool          showTextures = false;
        private static string        lastBodyName;
        private static bool          anyValueHasChanged;
        private static bool          firstRun  = true;
        
        private static string loadConfigPopupDialogText = "";

        public static Dictionary<string, ParallaxBody> ParallaxBodiesOriginal;

        private static readonly Dictionary<string, PropertyInfo> VarProperties     = new Dictionary<string, PropertyInfo>();
        private static readonly Dictionary<string, PropertyInfo> TextureProperties = new Dictionary<string, PropertyInfo>();

        public static readonly Dictionary<string, string>
            VarFromLabels = new Dictionary<string, string>
            {
                { "Surface texture scale", nameof(parallaxBody._SurfaceTextureScale) },
                { "Displacement offset", nameof(parallaxBody._displacement_offset) },
                { "Displacement scale", nameof(parallaxBody._displacement_scale) },
                { "Emission color", nameof(parallaxBody._EmissionColor) },
                { "Gloss", nameof(parallaxBody._Gloss) },
                { "Hapke", nameof(parallaxBody._Hapke) },
                { "Metallic", nameof(parallaxBody._Metallic) },
                { "Metallic tint", nameof(parallaxBody._MetallicTint) },
                { "Normal specular influence", nameof(parallaxBody._NormalSpecularInfluence) },
                { "Low start", nameof(parallaxBody._LowStart) },
                { "Low end", nameof(parallaxBody._LowEnd) },
                { "High start", nameof(parallaxBody._HighStart) },
                { "High end", nameof(parallaxBody._HighEnd) },
                { "Steep contrast", nameof(parallaxBody._SteepContrast) },
                { "Steep midpoint", nameof(parallaxBody._SteepMidpoint) },
                { "Steep power", nameof(parallaxBody._SteepPower) },
                { "Bump map", nameof(parallaxBody._BumpMap) },
                { "Bump map mid", nameof(parallaxBody._BumpMapMid) },
                { "Bump map high", nameof(parallaxBody._BumpMapHigh) },
                { "Bump map steep", nameof(parallaxBody._BumpMapSteep) },
                { "Steep tex", nameof(parallaxBody._SteepTex) },
                { "Surface texture", nameof(parallaxBody._SurfaceTexture) },
                { "Surface texture high", nameof(parallaxBody._SurfaceTextureHigh) },
                { "Surface texture mid", nameof(parallaxBody._SurfaceTextureMid) },
                { "Disp tex", nameof(parallaxBody._DispTex) },
                { "Influence map", nameof(parallaxBody._InfluenceMap) }
            };

        public static readonly Dictionary<string, string>
            LabelFromVar = new Dictionary<string, string>
            {
                { nameof(parallaxBody._SurfaceTextureScale), "Surface texture scale" },
                { nameof(parallaxBody._displacement_offset), "Displacement offset" },
                { nameof(parallaxBody._displacement_scale), "Displacement scale" },
                { nameof(parallaxBody._EmissionColor), "Emission color" },
                { nameof(parallaxBody._Gloss), "Gloss" },
                { nameof(parallaxBody._Hapke), "Hapke" },
                { nameof(parallaxBody._Metallic), "Metallic" },
                { nameof(parallaxBody._MetallicTint), "Metallic tint" },
                { nameof(parallaxBody._NormalSpecularInfluence), "Normal specular influence" },
                { nameof(parallaxBody._LowStart), "Low start" },
                { nameof(parallaxBody._LowEnd), "Low end" },
                { nameof(parallaxBody._HighStart), "High start" },
                { nameof(parallaxBody._HighEnd), "High end" },
                { nameof(parallaxBody._SteepContrast), "Steep contrast" },
                { nameof(parallaxBody._SteepMidpoint), "Steep midpoint" },
                { nameof(parallaxBody._SteepPower), "Steep power" },
                { nameof(parallaxBody._BumpMap), "Bump map" },
                { nameof(parallaxBody._BumpMapMid), "Bump map mid" },
                { nameof(parallaxBody._BumpMapHigh), "Bump map high" },
                { nameof(parallaxBody._BumpMapSteep), "Bump map steep" },
                { nameof(parallaxBody._SteepTex), "Steep tex" },
                { nameof(parallaxBody._SurfaceTexture), "Surface texture" },
                { nameof(parallaxBody._SurfaceTextureHigh), "Surface texture high" },
                { nameof(parallaxBody._SurfaceTextureMid), "Surface texture mid" },
                { nameof(parallaxBody._DispTex), "Disp tex" },
                { nameof(parallaxBody._InfluenceMap), "Influence map" },
            };

        public void Start()
        {
            CelestialBody currentBody = FlightGlobals.currentMainBody;
            lastBodyName = currentBody.name;
            parallaxBody = ParallaxBodies.parallaxBodies[currentBody.name];
            ParallaxInFlight = FindObjectOfType<ParallaxInFlightOperations>();

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

        private static void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Save current config to ParallaxConfigurator folder"))
            {
                SaveConfigs(parallaxBody.bodyName + ".txt");
            }
            if (GUILayout.Button("Load config from file"))
            {
                LoadConfigs(parallaxBody.bodyName + ".txt");
            }

            parallaxBody._SurfaceTextureScale = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._SurfaceTextureScale)], parallaxBody._SurfaceTextureScale);
            parallaxBody._displacement_offset = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._displacement_offset)], parallaxBody._displacement_offset);
            parallaxBody._displacement_scale = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._displacement_scale)], parallaxBody._displacement_scale);
            parallaxBody._EmissionColor = TextAreaLabelColor(LabelFromVar[nameof(parallaxBody._EmissionColor)], parallaxBody._EmissionColor);
            parallaxBody._Gloss = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._Gloss)], parallaxBody._Gloss);
            parallaxBody._Hapke = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._Hapke)], parallaxBody._Hapke);
            parallaxBody._Metallic = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._Metallic)], parallaxBody._Metallic);
            parallaxBody._MetallicTint = TextAreaLabelColor(LabelFromVar[nameof(parallaxBody._MetallicTint)], parallaxBody._MetallicTint);
            parallaxBody._NormalSpecularInfluence = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._NormalSpecularInfluence)], parallaxBody._NormalSpecularInfluence);
            parallaxBody._LowStart = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._LowStart)], parallaxBody._LowStart);
            parallaxBody._LowEnd = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._LowEnd)], parallaxBody._LowEnd);
            parallaxBody._HighStart = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._HighStart)], parallaxBody._HighStart);
            parallaxBody._HighEnd = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._HighEnd)], parallaxBody._HighEnd);
            parallaxBody._SteepContrast = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._SteepContrast)], parallaxBody._SteepContrast);
            parallaxBody._SteepMidpoint = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._SteepMidpoint)], parallaxBody._SteepMidpoint);
            parallaxBody._SteepPower = TextAreaLabelFloat(LabelFromVar[nameof(parallaxBody._SteepPower)], parallaxBody._SteepPower);

            GUILayout.EndVertical();

            if (showTextures != GUILayout.Toggle(showTextures, "Show Textures"))
            {
                showTextures = !showTextures;
                window = new Rect(window.position.x, window.position.y, 450, 200);
            }

            if (showTextures)
            {
                GUILayout.BeginVertical();

                parallaxBody._BumpMap = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._BumpMap)], parallaxBody._BumpMap);
                parallaxBody._BumpMapMid = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._BumpMapMid)], parallaxBody._BumpMapMid);
                parallaxBody._BumpMapHigh = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._BumpMapHigh)], parallaxBody._BumpMapHigh);
                parallaxBody._BumpMapSteep = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._BumpMapSteep)], parallaxBody._BumpMapSteep);
                parallaxBody._SteepTex = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._SteepTex)], parallaxBody._SteepTex);
                parallaxBody._SurfaceTexture = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._SurfaceTexture)], parallaxBody._SurfaceTexture);
                parallaxBody._SurfaceTextureHigh = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._SurfaceTextureHigh)], parallaxBody._SurfaceTextureHigh);
                parallaxBody._SurfaceTextureMid = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._SurfaceTextureMid)], parallaxBody._SurfaceTextureMid);
                parallaxBody._DispTex = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._DispTex)], parallaxBody._DispTex);
                parallaxBody._InfluenceMap = TextAreaLabelTexture(LabelFromVar[nameof(parallaxBody._InfluenceMap)], parallaxBody._InfluenceMap);

                GUILayout.EndVertical();
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
                var thisParallaxShader = (ParallaxQualityLibrary.Parallax) shaderInfo.GetValue(parallaxBody);

                foreach (string shaderVar in thisParallaxShader.shaderVars)
                {
                    string valueToReplaceWith = shaderVar;
                    if (thisParallaxShader.specificVars.ContainsKey(shaderVar))
                    {
                        valueToReplaceWith = thisParallaxShader.specificVars[shaderVar];
                    }

                    // get value object for this variable
                    object storedVar = bodyType.GetProperty(valueToReplaceWith)?.GetValue(parallaxBody);
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
                        var col = (Color) varProperty.GetValue(parallaxBody);

                        line += $"{col.r}, {col.g}, {col.b}, {col.a}";
                    }
                    else
                    {
                        line += varProperty.GetValue(parallaxBody).ToString();
                    }

                    file.WriteLine(line);
                }
                foreach (PropertyInfo texProperty in TextureProperties.Values)
                {
                    string line = "\t" + texProperty.Name + " = ";

                    line += texProperty.GetValue(parallaxBody);

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
            foreach(var line in lines)
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
                loaded = ReadConfigFromFileAtIndex(lines, 0);
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
            PopupDialog.SpawnPopupDialog(
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new MultiOptionDialog(
                    "ParallaxConfiguratorMultipleConfigs",
                    $"Found {maxIndex + 1} configs. Write an index to select a config. A larger index corresponds to an older config",
                    "Attention",
                    HighLogic.UISkin,
                    new Rect(0.5f, 0.5f, 300f, 150f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIVerticalLayout(
                        new DialogGUIHorizontalLayout(
                            new DialogGUITextInput(
                                loadConfigPopupDialogText,
                                false,
                                10,
                                str =>
                                {
                                    return loadConfigPopupDialogText = str;
                                },
                                200f,
                                25f
                            ),
                            new DialogGUILabel(
                                () => GetDateFromConfigIndex(lines, ParseConfigInt(loadConfigPopupDialogText, maxIndex), maxIndex)
                            )
                        ),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton("Load", delegate
                        {
                            ReadConfigFromFileAtIndex(lines, ParseConfigInt(loadConfigPopupDialogText, maxIndex));
                        }, 290.0f, 30.0f, true)
                    )
                ),
                false,
                HighLogic.UISkin
            );
        }

        private static string GetDateFromConfigIndex(string[] lines, int index, int maxValue)
        {
            if (index == maxValue)
                return "latest";
            
            string[] dateLines = lines.Where(l => l.StartsWith("// CONFIGS FROM ")).ToArray();

            string date = null;
            
            if (index < dateLines.Length)
            {
                date = dateLines[index]?.Remove(0, 16);
            }
            
            return date ?? "invalid";
        }

        private static int ParseConfigInt(string str, int maxValue)
        {
            // if invalid return latest
            if (!int.TryParse(str, out int index))
                return maxValue;
            
            // clamp and invert index
            if (index < 0)
                return maxValue;
            if (index > maxValue)
                return 0;

            return maxValue - index;
        }

        private static bool ReadConfigFromFileAtIndex(string[] lines, int index)
        {
            int counter = -1;
            bool valid = false;
            Debug.Log($"[ParallaxConfigurator] Parsing config #{index}");

            foreach (string line in lines)
            {
                if (line.StartsWith("{"))
                    counter++;
                
                // ignore line if it is not in the right config or if it is a comment
                if(counter != index || line.StartsWith("//"))
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
                if(array.Length != 2)
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
            if(VarProperties.ContainsKey(varName))
            {
                PropertyInfo propInfo = VarProperties[varName];

                if (propInfo.PropertyType == typeof(Color))
                {
                    propInfo.SetValue(parallaxBody, value.ForceParseColor(new Color()));
                }
                else
                {
                    propInfo.SetValue(parallaxBody, value.ForceParseFloat());
                }
            }
            else if (TextureProperties.ContainsKey(varName))
            {
                PropertyInfo propInfo = TextureProperties[varName];
                
                propInfo.SetValue(parallaxBody, value);
            }
        }
    }
}