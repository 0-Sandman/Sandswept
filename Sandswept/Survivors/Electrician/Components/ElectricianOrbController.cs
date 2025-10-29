using System;
using System.Linq;

namespace Sandswept.Survivors.Electrician
{
    public class ElectricianOrbController : MonoBehaviour
    {
        public MeshRenderer target;
        public ModelSkinController MSC;
        public Pair[] MaterialMap;
        public SkinnedMeshRenderer VisibilityTarget;
        public int lastSkinIndex = -1;

        public LineRenderer tether1;
        public LineRenderer tether2;

        public bool ran = false;

        public void Start()
        {
            target = GetComponent<MeshRenderer>();

            MaterialMap = new Pair[]
            {
                new Pair
                {
                    TargetSkin = Electrician.sdElecDefault,
                    Materials = new Material[]
                    {
                        Electrician.matElecOrbOuter,
                        Electrician.matElecOrbInner
                    }
                },

                new Pair
                {
                    TargetSkin = Electrician.sdElecMastery,
                    Materials = new Material[]
                    {
                        Electrician.matMasteryElecOrbOuter,
                        Electrician.matMasteryElecOrbInner
                    }
                },
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

            if (ran)
            {
                return;
            }

            var skinNameToken = MSC.skins[MSC.currentSkinIndex].nameToken;
            var tetherColors = skinNameToken switch
            {
                "VOLT_SKIN_COVENANT_NAME" => new Color32[2] { new Color32(115, 0, 255, 255), new Color32(113, 143, 255, 255) },
                _ => new Color32[2] { new Color32(0, 157, 255, 255), new Color32(0, 216, 255, 255) }
            };

            var tethers = target.transform.Find("Tethers");

            if (!tethers)
            {
                return;
            }

            tether1 = tethers.Find("Tether 1").GetComponent<LineRenderer>();
            tether1.material = Main.lineRendererBase;
            tether1.startWidth = 0.4f;
            tether1.endWidth = 0.4f;
            tether1.startColor = tetherColors[0];
            tether1.endColor = tetherColors[1];
            tether1.textureMode = LineTextureMode.Tile;

            tether2 = tethers.Find("Tether 2").GetComponent<LineRenderer>();
            tether2.material = Main.lineRendererBase;
            tether2.startWidth = 0.4f;
            tether2.endWidth = 0.4f;
            tether2.startColor = tetherColors[1];
            tether2.endColor = tetherColors[0];
            tether2.textureMode = LineTextureMode.Tile;

            ran = true;
        }
    }

    public class Pair
    {
        public SkinDef TargetSkin;
        public Material[] Materials;
    }
}