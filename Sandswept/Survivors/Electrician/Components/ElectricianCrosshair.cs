using System;
using Rewired.ComponentControls.Effects;
using RoR2.UI;

namespace Sandswept.Survivors.Electrician
{
    public class ElectricianCrosshair : MonoBehaviour
    {
        public Animator anim;
        public HudElement hud;
        public float rotSpeedInner = 60f;
        public float rotSpeedOuter = -20f;
        public RectTransform outer;
        public RectTransform inner;
        public bool sigma = false;

        public void Start()
        {
            hud = GetComponent<HudElement>();

            CrosshairController cm = GetComponent<CrosshairController>();
            cm.maxSpreadAngle = 2f;

            outer.localScale = Vector3.one * 0.85f;
            inner.localScale = Vector3.one * 0.85f;

            var dot = outer.parent.Find("texCrosshairDot").GetComponent<RectTransform>();
            dot.localScale *= 0.85f;

            /*for (int i = 0; i < cm.spriteSpreadPositions.Length; i++) {
                var sprite = cm.spriteSpreadPositions[i];
                float mult = 3f;
                sprite.onePosition.x *= mult;
                sprite.onePosition.y *= mult;
                sprite.onePosition.z *= mult;
            }*/
        }

        public void Update()
        {
            Vector3 rot1 = inner.localRotation.eulerAngles;
            Vector3 rot2 = outer.localRotation.eulerAngles;

            if (sigma || rot1.z != 0f)
            {
                rot1.z += rotSpeedInner * Time.deltaTime;

                if (!sigma)
                {
                    rot1.z = 0f;
                }
            }

            if (sigma || rot2.z != 0f)
            {
                rot2.z += rotSpeedOuter * Time.deltaTime;

                if (!sigma)
                {
                    rot2.z = 0f;
                }
            }

            outer.localRotation = Quaternion.Euler(rot2);
            inner.localRotation = Quaternion.Euler(rot1);
        }

        public void FixedUpdate()
        {
            if (!anim)
            {
                GameObject bodyObject = hud._targetBodyObject;

                if (bodyObject)
                {
                    ModelLocator loc = bodyObject.GetComponent<ModelLocator>();

                    if (loc && loc._modelTransform)
                    {
                        anim = loc._modelTransform.GetComponent<Animator>();
                    }
                }

                return;
            }

            sigma = anim.GetBool("discharging");
        }
    }
}