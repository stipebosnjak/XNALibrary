#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Xna.Helpers._3D
{
    public class Material
    {
        public Effect MaterialEffect;
        public static GraphicsDevice GraphicsDevice { get; set; }


        public virtual void SetEffectParameters(Effect effect)
        {
        }

        public virtual void SetEffectParameters()
        {
        }
    }

    public class LightingMaterial : Material
    {
        public Vector3 AmbientColor { get; set; }
        public Vector3 LightDirection { get; set; }
        public Vector3 LightColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        public LightingMaterial()
        {
            AmbientColor = new Vector3(.1f, .1f, .1f);
            LightDirection = new Vector3(1, 1, 1);
            LightColor = new Vector3(.9f, .9f, .9f);
            SpecularColor = new Vector3(1, 1, 1);
            MaterialEffect = EffectManager.Content.Load<Effect>("LightingEffect");
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["AmbientColor"] != null)
                effect.Parameters["AmbientColor"].SetValue(AmbientColor);

            if (effect.Parameters["LightDirection"] != null)
                effect.Parameters["LightDirection"].SetValue(LightDirection);

            if (effect.Parameters["LightColor"] != null)
                effect.Parameters["LightColor"].SetValue(LightColor);

            if (effect.Parameters["SpecularColor"] != null)
                effect.Parameters["SpecularColor"].SetValue(SpecularColor);
        }

        public override void SetEffectParameters()
        {
            if (MaterialEffect.Parameters["AmbientColor"] != null)
                MaterialEffect.Parameters["AmbientColor"].SetValue(AmbientColor);

            if (MaterialEffect.Parameters["LightDirection"] != null)
                MaterialEffect.Parameters["LightDirection"].SetValue(LightDirection);

            if (MaterialEffect.Parameters["LightColor"] != null)
                MaterialEffect.Parameters["LightColor"].SetValue(LightColor);

            if (MaterialEffect.Parameters["SpecularColor"] != null)
                MaterialEffect.Parameters["SpecularColor"].SetValue(SpecularColor);
        }
    }

    public class NormalMapMaterial : LightingMaterial
    {
        public Texture2D NormalMap { get; set; }

        public NormalMapMaterial(Texture2D NormalMap)
        {
            this.NormalMap = NormalMap;
        }

        public override void SetEffectParameters(Effect effect)
        {
            base.SetEffectParameters(effect);

            if (effect.Parameters["NormalMap"] != null)
                effect.Parameters["NormalMap"].SetValue(NormalMap);
        }

        public override void SetEffectParameters()
        {
            base.SetEffectParameters(MaterialEffect);

            if (MaterialEffect.Parameters["NormalMap"] != null)
                MaterialEffect.Parameters["NormalMap"].SetValue(NormalMap);
        }
    }
    public class CubeMapReflectMaterial : Material
    {
        public TextureCube CubeMap { get; set; }

        public CubeMapReflectMaterial(TextureCube CubeMap)
        {
            this.CubeMap = CubeMap;
            MaterialEffect = EffectManager.Content.Load<Effect>("CubeMapReflect");
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["CubeMap"] != null)
                effect.Parameters["CubeMap"].SetValue(CubeMap);
        }
    }
}