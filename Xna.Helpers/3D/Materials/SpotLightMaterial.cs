using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers._3D
{
    public class SpotLightMaterial : Material
    {
        public Vector3 AmbientLightColor { get; set; }
        public Vector3 LightPosition { get; set; }
        public Vector3 LightColor { get; set; }
        public Vector3 LightDirection { get; set; }
        public float ConeAngle { get; set; }
        public float LightFalloff { get; set; }

        public SpotLightMaterial()
        {
            AmbientLightColor = new Vector3(.15f, .15f, .15f);
            LightPosition = new Vector3(0, 3000, 0);
            LightColor = new Vector3(.85f, .85f, .85f);
            ConeAngle = 30;
            LightDirection = new Vector3(0, -1, 0);
            LightFalloff = 20;
            MaterialEffect = EffectManager.Content.Load<Effect>("SpotLightEffect");
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["AmbientLightColor"] != null)
                effect.Parameters["AmbientLightColor"].SetValue(
                    AmbientLightColor);

            if (effect.Parameters["LightPosition"] != null)
                effect.Parameters["LightPosition"].SetValue(LightPosition);

            if (effect.Parameters["LightColor"] != null)
                effect.Parameters["LightColor"].SetValue(LightColor);

            if (effect.Parameters["LightDirection"] != null)
                effect.Parameters["LightDirection"].SetValue(LightDirection);

            if (effect.Parameters["ConeAngle"] != null)
                effect.Parameters["ConeAngle"].SetValue(
                    MathHelper.ToRadians(ConeAngle / 2));

            if (effect.Parameters["LightFalloff"] != null)
                effect.Parameters["LightFalloff"].SetValue(LightFalloff);
        }
        public override void SetEffectParameters()
        {
            if (MaterialEffect.Parameters["AmbientLightColor"] != null)
                MaterialEffect.Parameters["AmbientLightColor"].SetValue(
                    AmbientLightColor);

            if (MaterialEffect.Parameters["LightPosition"] != null)
                MaterialEffect.Parameters["LightPosition"].SetValue(LightPosition);

            if (MaterialEffect.Parameters["LightColor"] != null)
                MaterialEffect.Parameters["LightColor"].SetValue(LightColor);

            if (MaterialEffect.Parameters["LightDirection"] != null)
                MaterialEffect.Parameters["LightDirection"].SetValue(LightDirection);

            if (MaterialEffect.Parameters["ConeAngle"] != null)
                MaterialEffect.Parameters["ConeAngle"].SetValue(
                    MathHelper.ToRadians(ConeAngle / 2));

            if (MaterialEffect.Parameters["LightFalloff"] != null)
                MaterialEffect.Parameters["LightFalloff"].SetValue(LightFalloff);
        }
    }
}
