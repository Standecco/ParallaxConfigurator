using System.Security.Cryptography.X509Certificates;
using ParallaxQualityLibrary;

namespace ParallaxConfigurator
{
    public class Utils
    {
        public static ParallaxBody Clone(ParallaxBody orig)
        {
            var clone = new ParallaxBody("", 0);
            
            clone.bodyName = orig.bodyName;
            clone.hasEmission = orig.hasEmission ;
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