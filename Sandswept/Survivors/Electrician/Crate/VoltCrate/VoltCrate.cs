
using System.Collections;

namespace Sandswept.Survivors.Electrician.Crate
{
    public static class VoltCrate
    {
        public static GameObject crateDefault;
        public static GameObject crateCovenant;
        public static void Init()
        {
            crateDefault = CreateCrateRecolor("Default", new Color32(195, 208, 255, 255), new Color32(255, 140, 0, 255));
            crateCovenant = CreateCrateRecolor("Covenant", new Color32(255, 132, 234, 255), new Color32(223, 93, 255, 255));
            On.RoR2.Run.HandlePlayerFirstEntryAnimation += Run_HandlePlayerFirstEntryAnimation;
        }

        private static void Run_HandlePlayerFirstEntryAnimation(On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig, Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (body && body.baseNameToken == "SANDSWEPT_ELECTR_NAME")
            {
                // Main.ModLogger.LogError("is ranger");
                var modelLocator = body.modelLocator;
                var master = body.master;
                if (modelLocator && master)
                {
                    var modelTransform = modelLocator.modelTransform;

                    if (modelTransform)
                    {
                        var loadoutSkinIndex = body.master.loadout.bodyLoadoutManager.GetSkinIndex(body.bodyIndex);

                        body.preferredPodPrefab = loadoutSkinIndex switch
                        {
                            1 => crateCovenant,
                            _ => crateDefault
                        };
                    }
                }
            }

            orig(self, body, spawnPosition, spawnRotation);
        }

        public static GameObject CreateCrateRecolor(string name, Color32 crateColor, Color32 lightningColor) // lol
        {
            var crate = PrefabAPI.InstantiateClone(Paths.GameObject.RoboCratePod, "VOL-T Crate " + name);

            crate.AddComponent<PlaySoundOnCrateBreak>();

            var transform = crate.transform;
            var crateBase = transform.Find("Base/mdlRoboCrate/Base");

            var newCrateMaterial = new Material(Paths.Material.matRoboCrate);
            newCrateMaterial.SetColor("_TintColor", crateColor);
            newCrateMaterial.SetTexture("_MainTex", Paths.Texture2D.texTrimSheetRailGunnerAltColossus);
            newCrateMaterial.SetFloat("_NormalStrength", 0.4532577f);
            newCrateMaterial.SetTexture("_NormalTex", Paths.Texture2D.texTrimSheetMetalNormal);
            newCrateMaterial.SetColor("_EmColor", Color.white);
            newCrateMaterial.SetFloat("_EmPower", 0.01f);
            newCrateMaterial.SetFloat("_Smoothness", 1f);

            var newFireMaterial = new Material(Paths.Material.matEntryBurn);
            newFireMaterial.SetColor("_TintColor", lightningColor);
            newFireMaterial.SetTexture("_RemapTex", Paths.Texture2D.texRampTritone);
            newFireMaterial.SetFloat("_InvFade", 0f); // try 2 (more) and 0.1 (less) if not
            newFireMaterial.SetFloat("_Boost", 2f);
            newFireMaterial.SetFloat("_AlphaBoost", 0.4f);
            newFireMaterial.SetFloat("_AlphaBias", 0f);
            newFireMaterial.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudLightning1);
            newFireMaterial.SetTexture("_Cloud2Tex", null);

            var newCrateMaterials = new Material[2] { newCrateMaterial, newFireMaterial };

            var crateMesh = crateBase.Find("RobotCrateMesh").GetComponent<MeshRenderer>();
            crateMesh.sharedMaterials = newCrateMaterials;

            var flames = crateBase.Find("Flames").GetComponent<ParticleSystemRenderer>();
            flames.material = newFireMaterial;

            PrefabAPI.RegisterNetworkPrefab(crate);

            return crate;
        }
    }

    public class PlaySoundOnCrateBreak : MonoBehaviour
    {
        public VehicleSeat vehicleSeat;

        public void Start()
        {
            // Main.ModLogger.LogError("play sound on crate break start");
            vehicleSeat = GetComponent<VehicleSeat>();
            vehicleSeat.onPassengerExit += OnPassengerExit;
        }

        private void OnPassengerExit(GameObject passenger)
        {
            // Main.ModLogger.LogError("crate: on passenger exit called, trying to play sounds");
            // Main.ModLogger.LogError("passenger is: " + passenger);
            Util.PlaySound("Play_captain_R_impact", passenger);
            Util.PlaySound("Play_item_proc_chain_lightning", passenger);
            Util.PlaySound("Play_item_proc_chain_lightning", passenger);
            Util.PlaySound("Play_item_proc_chain_lightning", passenger);
            StartCoroutine(PlayActivationSound(passenger));
        }

        public IEnumerator PlayActivationSound(GameObject passenger)
        {
            // Main.ModLogger.LogError("crate: couroutine called, trying to play sounds");
            // Main.ModLogger.LogError("coroutine: passenger is: " + passenger);
            yield return new WaitForSeconds(0.8f);
            Util.PlaySound("Play_item_proc_chain_lightning", passenger);
            Util.PlaySound("Play_item_proc_chain_lightning", passenger);
            Util.PlaySound("Play_item_proc_chain_lightning", passenger);
            yield return new WaitForSeconds(0.2f);
            Util.PlaySound("Play_elec_spin", passenger);
        }

        public void OnDestroy()
        {
            // Main.ModLogger.LogError("on destroy called, unsubscribing from on passenger exit");
            vehicleSeat.onPassengerExit -= OnPassengerExit;
        }
    }
}