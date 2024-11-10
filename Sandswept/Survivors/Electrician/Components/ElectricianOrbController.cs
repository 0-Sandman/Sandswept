using System;
using System.Linq;

namespace Sandswept.Survivors.Electrician
{
    public class ElectricianOrbController : MonoBehaviour
    {
        private MeshRenderer target;
        public ModelSkinController MSC;
        private Pair[] MaterialMap;
        public SkinnedMeshRenderer VisibilityTarget;
        private int lastSkinIndex = -1;

        public void Start()
        {
            target = GetComponent<MeshRenderer>();

            MaterialMap = new Pair[] {
                new Pair {
                    TargetSkin = Electrician.sdElecDefault,
                    Materials = new Material[] {
                        Electrician.matElecOrbOuter,
                        Electrician.matElecOrbInner
                    }
                },

                new Pair {
                    TargetSkin = Electrician.sdElecMastery,
                    Materials = new Material[] {
                        Electrician.matMasteryElecOrbOuter,
                        Electrician.matMasteryElecOrbInner
                    }
                }
            };
        }

        public void Update()
        {
            if (MSC.currentSkinIndex != lastSkinIndex)
            {
                RefreshOrb();
            }

            target.enabled = VisibilityTarget.enabled;
        }

        public void RefreshOrb()
        {
            lastSkinIndex = MSC.currentSkinIndex;
            SkinDef targetSkin = MSC.skins[MSC.currentSkinIndex];
            Pair pair = MaterialMap.FirstOrDefault(x => x.TargetSkin == targetSkin);
            target.sharedMaterials = pair.Materials;
        }
    }

    public class Pair
    {
        public SkinDef TargetSkin;
        public Material[] Materials;
    }
}