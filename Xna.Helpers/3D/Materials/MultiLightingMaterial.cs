using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers._3D
{
    public class MultiLightingMaterial : Material
    {
        public Vector3 AmbientColor { get; set; }
        public Vector3[] LightDirection { get; set; }
        public Vector3[] LightColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        public MultiLightingMaterial()
        {
            AmbientColor = new Vector3(.1f, .1f, .1f);
            LightDirection = new Vector3[3];
            LightColor = new Vector3[] { Vector3.One, Vector3.One, 
                Vector3.One };
            SpecularColor = new Vector3(1, 1, 1);
            MaterialEffect = EffectManager.Content.Load<Effect>("MultiLightingEffect");
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
}
