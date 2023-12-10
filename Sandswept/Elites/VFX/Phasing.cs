using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Elites.VFX
{
    public static class Phasing
    {
        public static GameObject prefab;

        public static void Init()
        {
            prefab = PrefabAPI.InstantiateClone(Assets.GameObject.TreebotShockwavePullEffect, "Phasing Pull VFX", false);

            var transform = prefab.transform;
            var pollenSingle = transform.GetChild(1);
            var pollenDust = transform.GetChild(2);
            var pollenRadial = transform.GetChild(3);
            var pollenSingle2 = transform.GetChild(4);
            var distortionWave2 = transform.GetChild(7).GetComponent<ParticleSystem>().main.startColor;
            pollenSingle.gameObject.SetActive(false);
            pollenDust.gameObject.SetActive(false);
            pollenRadial.gameObject.SetActive(false);
            pollenSingle2.gameObject.SetActive(false);
            distortionWave2.color = new Color32(79, 0, 175, 255);

            ContentAddition.AddEffect(prefab);
        }
    }
}