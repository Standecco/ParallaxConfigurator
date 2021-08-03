using System;
using System.ComponentModel;
using System.Globalization;
using KSP.UI.Screens.Settings;
using Parallax;
using ParallaxQualityLibrary;
using UnityEngine;

namespace ParallaxConfigurator
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ParallaxConfiguratorMain : MonoBehaviour
    {
        private ParallaxBody _parallaxBody;
        public bool ShowUI { get; set; }
        private static Rect window       = new Rect(100, 100, 450, 200);
        private static bool showTextures = false;
        
        public void Start()
        {
            CelestialBody currentBody = FlightGlobals.currentMainBody;
            _parallaxBody = ParallaxBodies.parallaxBodies[currentBody.name];
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
                ShowUI = !ShowUI;
        }

        public void OnGUI()
        {
            if(!ShowUI)
                return;
            
            window = GUILayout.Window(GetInstanceID(), window, Window, "Parallax Configurator");
        }

        private void Window(int windowID)
        {
            GUILayout.BeginVertical();

            _parallaxBody._displacement_offset = TextAreaLabelFloat("Displacement offset", _parallaxBody._displacement_offset);
            _parallaxBody._displacement_scale = TextAreaLabelFloat("Displacement scale", _parallaxBody._displacement_scale);
            _parallaxBody._EmissionColor = TextAreaLabelColor("Emission color", _parallaxBody._EmissionColor);
            _parallaxBody._Gloss = TextAreaLabelFloat("Gloss", _parallaxBody._Gloss);
            _parallaxBody._Hapke = TextAreaLabelFloat("Hapke", _parallaxBody._Hapke);
                _parallaxBody._SurfaceTextureScale = TextAreaLabelFloat("Surface texture scale", _parallaxBody._SurfaceTextureScale);
            _parallaxBody._MetallicTint = TextAreaLabelColor("Metallic tint", _parallaxBody._MetallicTint);
            _parallaxBody._NormalSpecularInfluence = TextAreaLabelFloat("Normal specular influence", _parallaxBody._NormalSpecularInfluence);
            _parallaxBody._HighEnd = TextAreaLabelFloat("High end", _parallaxBody._HighEnd);
            _parallaxBody._HighStart = TextAreaLabelFloat("High start", _parallaxBody._HighEnd);
            _parallaxBody._LowEnd = TextAreaLabelFloat("Low end", _parallaxBody._LowEnd);
            _parallaxBody._LowStart = TextAreaLabelFloat("Low start", _parallaxBody._LowStart);
            _parallaxBody._Metallic = TextAreaLabelFloat("Metallic", _parallaxBody._Metallic);
            _parallaxBody._SteepContrast = TextAreaLabelFloat("Steep contrast", _parallaxBody._SteepContrast);
            _parallaxBody._SteepMidpoint = TextAreaLabelFloat("Steep midpoint", _parallaxBody._SteepMidpoint);
            _parallaxBody._SteepPower = TextAreaLabelFloat("Steep power", _parallaxBody._SteepPower);
            
            if (showTextures = GUILayout.Toggle(showTextures, showTextures ? "Hide Textures" : "Show Textures"))
            {
                _parallaxBody._BumpMap = TextAreaLabelTexture("Bump map", _parallaxBody._BumpMap);
                _parallaxBody._BumpMapHigh = TextAreaLabelTexture("Bump map high", _parallaxBody._BumpMapSteep);
                _parallaxBody._BumpMapMid = TextAreaLabelTexture("Bump map mid", _parallaxBody._BumpMapMid);
                _parallaxBody._BumpMapSteep = TextAreaLabelTexture("Bump map steep", _parallaxBody._BumpMapSteep);
                _parallaxBody._DispTex = TextAreaLabelTexture("Disp tex", _parallaxBody._DispTex);
                _parallaxBody._InfluenceMap = TextAreaLabelTexture("Influence map", _parallaxBody._InfluenceMap);
                _parallaxBody._SteepTex = TextAreaLabelTexture("Steep tex", _parallaxBody._SteepTex);
                _parallaxBody._SurfaceTexture = TextAreaLabelTexture("Surface texture", _parallaxBody._SurfaceTexture);
                _parallaxBody._SurfaceTextureHigh = TextAreaLabelTexture("Surface texture high", _parallaxBody._SurfaceTextureHigh);
                _parallaxBody._SurfaceTextureMid = TextAreaLabelTexture("Surface texture mid", _parallaxBody._SurfaceTextureMid);
            }
            
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private static string TextAreaLabelTexture(string label, string value)
        {
            GUILayout.BeginHorizontal();
            string newValue = InputFields.TexField(new GUIContent(label + " [Texture] "), value);
            GUILayout.EndHorizontal();
            
            return newValue;
        }

        private static float TextAreaLabelFloat(string label, float value)
        {
            GUILayout.BeginHorizontal();
            float newValue = InputFields.FloatField(new GUIContent(label + " [float] "), value);
            GUILayout.EndHorizontal();
            
            return newValue;
        }

        private static Color TextAreaLabelColor(string label, Color value)
        {
            GUILayout.BeginHorizontal();
            Color newValue = InputFields.ColorField(new GUIContent(label + " [Color] "), value);
            GUILayout.EndHorizontal();
            
            return newValue;
        }
    }
}