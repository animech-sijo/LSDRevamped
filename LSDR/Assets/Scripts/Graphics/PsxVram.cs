using IO;
using libLSD.Formats;
using UnityEngine;

namespace Graphics
{
    public static class PsxVram
    {
        public const int VRAM_WIDTH = 2056;
        public const int VRAM_HEIGHT = 512;

        public static Material VramMaterial;
        public static Material VramAlphaBlendMaterial;
        public static Texture VramTexture;
        private static readonly int _mainTex = Shader.PropertyToID("_MainTex");

        public static Material[] Materials => new[] {VramMaterial, VramAlphaBlendMaterial};

        public static void Initialize()
        {
            VramMaterial = new Material(Shader.Find("LSDR/RevampedDiffuse"));
            VramAlphaBlendMaterial = new Material(Shader.Find("LSDR/RevampedDiffuseAlphaBlend"));
        }

        public static void LoadVramTix(TIX tix)
        {
            VramTexture = LibLSDUnity.GetTextureFromTIX(tix);
            VramMaterial.SetTexture(_mainTex, VramTexture);
            VramAlphaBlendMaterial.SetTexture(_mainTex, VramTexture);
        }
    }
}
