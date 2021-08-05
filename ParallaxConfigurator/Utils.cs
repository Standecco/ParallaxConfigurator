using System.Collections.Generic;
using System.Reflection;
using ParallaxQualityLibrary;
using UnityEngine;

namespace ParallaxConfigurator
{
    public class Utils
    {
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

        private static ParallaxBody parallaxBody => ParallaxConfiguratorMain.ParallaxBody;

        public static object GetVariableOriginalValue(string varName)
        {
            Debug.Log($"[ParallaxConfigurator] Resetting {varName} to original value");

            ParallaxBody originalBody = ParallaxConfiguratorMain.ParallaxBodiesOriginal[FlightGlobals.currentMainBody.name];
            PropertyInfo originalProperty = typeof(ParallaxBody).GetProperty(varName, BindingFlags.Instance | BindingFlags.Public);

            return originalProperty?.GetValue(originalBody);
        }

        public static ParallaxBody Clone(ParallaxBody orig)
        {
            var clone = new ParallaxBody("", 0);

            clone.bodyName = orig.bodyName;
            clone.hasEmission = orig.hasEmission;
            clone.full = orig.full;
            clone.singleLow = orig.singleLow;
            clone.singleMid = orig.singleMid;
            clone.singleHigh = orig.singleHigh;
            clone.singleSteepLow = orig.singleSteepLow;
            clone.singleSteepMid = orig.singleSteepMid;
            clone.singleSteepHigh = orig.singleSteepHigh;
            clone.doubleLow = orig.doubleLow;
            clone.doubleHigh = orig.doubleHigh;
            clone._SurfaceTexture = orig._SurfaceTexture;
            clone._SurfaceTextureMid = orig._SurfaceTextureMid;
            clone._SurfaceTextureHigh = orig._SurfaceTextureHigh;
            clone._SteepTex = orig._SteepTex;
            clone._BumpMap = orig._BumpMap;
            clone._BumpMapMid = orig._BumpMapMid;
            clone._BumpMapHigh = orig._BumpMapHigh;
            clone._BumpMapSteep = orig._BumpMapSteep;
            clone._InfluenceMap = orig._InfluenceMap;
            clone._DispTex = orig._DispTex;
            clone._SurfaceTextureScale = orig._SurfaceTextureScale;
            clone._LowStart = orig._LowStart;
            clone._LowEnd = orig._LowEnd;
            clone._HighStart = orig._HighStart;
            clone._HighEnd = orig._HighEnd;
            clone._displacement_scale = orig._displacement_scale;
            clone._displacement_offset = orig._displacement_offset;
            clone._Metallic = orig._Metallic;
            clone._Gloss = orig._Gloss;
            clone._MetallicTint = orig._MetallicTint;
            clone._NormalSpecularInfluence = orig._NormalSpecularInfluence;
            clone._SteepPower = orig._SteepPower;
            clone._SteepContrast = orig._SteepContrast;
            clone._SteepMidpoint = orig._SteepMidpoint;
            clone._Hapke = orig._Hapke;
            clone._EmissionColor = orig._EmissionColor;

            return clone;
        }
    }
}