/*
using System;
using Sandswept.Artifacts;
using Sandswept.Buffs;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Sandswept.Mechanics {
    public class NewtReflection {
        public static List<string> rootObjectsList = new() {
            "HOLDER: Starting Cave", "HOLDER: Store"
        };
        public static List<string> storeDisableTargets = new()
        {
            "LunarShop", "SeerShop", "CauldronShop", "DroneShop", "HOLDER: Pillars", "HOLDER: Store Platforms", "HOLDER: Backroom", "HOLDER: Water", "HOLDER: Lights", "HOLDER: Flavor Props", "HOLDER: Wood Columns"
        };

        public static List<string> boundsOffsets = new()
        {
            "HOLDER: Starting Cave/Static/GPRuinedRing1",
            "HOLDER: Starting Cave/Static/GPRuinedRing1 (1)",
            "HOLDER: Starting Cave/Static/GPRuinedRing1 (2)",
            "HOLDER: Starting Cave/Static/GPRuinedRing1 (3)",
            "HOLDER: Starting Cave/Static/GPRuinedRing1 (4)",
        };

        public static Vector3 BounceTargetNormal = new Vector3(9.8f, -5.3f, -2.4f);
        public static Vector3 BounceTargetFlipped = new Vector3(-187f, -5.1f, -200f) + new Vector3(-300f, 0f, -300f);
        public static Vector3 ArtifactPoint = new Vector3(-213f, -438f, -622f);
        public static PostProcessProfile ppGrayscale;

        public static void Initialize() {
            SceneManager.activeSceneChanged += OnSceneChange;
            On.RoR2.MapZone.TeleportBody += HotPooGames;

            PostProcessProfile ppLBB = Utils.Assets.PostProcessProfile.ppLocalBottomOfBazaar;
            RampFog fog = ppLBB.GetSetting<RampFog>();
            fog.fogIntensity.value = 1f;
            fog.fogOne.value = 0;
            fog.fogColorStart.value = new Color32(0, 0, 0, 0);
            fog.fogColorMid.value = new Color32(0, 0, 0, 87);
            fog.fogColorEnd.value = new Color32(99, 99, 99, 255);

            ppGrayscale = ScriptableObject.CreateInstance<PostProcessProfile>();

            ColorGrading cg = ppGrayscale.AddSettings<ColorGrading>();
            cg.saturation.overrideState = true;
            cg.saturation.value = -100;
            cg.contrast.overrideState = true;
            cg.contrast.value = 0;
            cg.hueShift.overrideState = true;
            cg.hueShift.value = -50;
        }

        private static void HotPooGames(On.RoR2.MapZone.orig_TeleportBody orig, MapZone self, CharacterBody characterBody)
        {
            Transform targetDestination = self.explicitDestination;

            if (targetDestination && (targetDestination.name == "SS Normal Bazaar Dest" || targetDestination.name == "SS Mirrored Bazaar Dest")) {
                TeleportHelper.TeleportBody(characterBody, targetDestination.position, true);
                EventRaiser.RaiseEvent(self, nameof(MapZone.onBodyTeleport), characterBody);
            }
            else {
                orig(self, characterBody);
            }
        }

        private static void OnSceneChange(Scene arg0, Scene arg1)
        {
            if (SceneManager.GetActiveScene().name == "bazaar") {
                CreateMirroredShop();
            }
        }

        public static void CreateMirroredShop()
        {
            GameObject caveRoot = GameObject.Find("HOLDER: Starting Cave");
            MapZone m1 = caveRoot.GetComponentInChildren<MapZone>();
            m1.transform.position = new Vector3(-16f, -477f, -1.5f);
            m1.transform.localScale = new Vector3(70f, 3f, 70f);
            m1.triggerType = MapZone.TriggerType.TriggerEnter;

            GameObject.Find("HOLDER: Starting Cave/Static/GPRuinedRing1 (3)").GetComponent<MeshCollider>().enabled = false;
            GameObject.Find("HOLDER: Starting Cave/Static/GPRuinedRing1 (4)").GetComponent<MeshCollider>().enabled = false;

            GameObject newtShop = new("Mirrored Shop Holder");

            foreach (string copy in rootObjectsList)
            {
                GameObject copied = GameObject.Instantiate(GameObject.Find(copy), newtShop.transform);
                copied.name = copied.name.Replace("(Clone)", "");
            }

            foreach (string disable in storeDisableTargets) {
                newtShop.transform.Find("HOLDER: Store").Find(disable).gameObject.SetActive(false);
            }

            foreach (string offset in boundsOffsets) {
                MeshRenderer renderer = newtShop.transform.Find(offset).GetComponent<MeshRenderer>();
                Vector3 center = renderer.bounds.center + new Vector3(-500, 0, -500);
                Vector3 extents = renderer.bounds.extents;
                renderer.bounds = new Bounds(center, extents);
                renderer.gameObject.SetActive(false);
            }

            newtShop.transform.position = new Vector3(-500, 0, -500);

            MapZone m2 = newtShop.transform.Find("HOLDER: Starting Cave").GetComponentInChildren<MapZone>();

            GameObject dest1 = new("SS Normal Bazaar Dest");
            dest1.transform.parent = m1.transform;
            dest1.transform.position = new Vector3(-0.7f, -338f, -30.1f);
            GameObject dest2 = new("SS Mirrored Bazaar Dest");
            dest2.transform.parent = m2.transform;
            dest2.transform.position = new Vector3(-512, -463f, -522);

            m1.explicitDestination = dest2.transform;
            m2.explicitDestination = dest1.transform;

            m1.onBodyTeleport += (cb) =>
            {
                ApplyLeap(cb, m1, BounceTargetFlipped);
            };

            m2.onBodyTeleport += (cb) =>
            {
                ApplyLeap(cb, m2, BounceTargetNormal);
            };

            void ApplyLeap(CharacterBody cb, MapZone m, Vector3 bounceTarget) {
                float dur = 5f;
                Vector3 start = m.explicitDestination.position;
                Vector3 end = bounceTarget + (Vector3.up * 10f);

                float y = Trajectory.CalculateInitialYSpeed(dur, end.y - start.y);
                float x = end.x - start.x;
                float z = end.z - start.z;

                cb.characterMotor.velocity = new Vector3(x / dur, y, z / dur);
                cb.characterMotor.disableAirControlUntilCollision = true;
                cb.characterMotor.Motor.ForceUnground();

                cb.AddTimedBuff(Falling.instance.BuffDef, 4f + (Falling.CameraMirror.offset), 1);
            }

            PostProcessVolume vol = newtShop.FindComponent<PostProcessVolume>("PP, BottomOfArenaHole");
            vol.profile = ppGrayscale;
            vol.sharedProfile = ppGrayscale;
            vol.blendDistance = 50;
            vol.RemoveComponent<SphereCollider>();
            vol.AddComponent<BoxCollider>(x =>
            {
                x.size = new Vector3(100, 200, 100);
                x.isTrigger = true;
                x.transform.localScale = Vector3.one;
                x.transform.localPosition = Vector3.zero;
            });

            GameObject fog = GameObject.Find("SceneInfo/PP, BottomOfBazaar");
            fog.GetComponent<PostProcessVolume>().blendDistance = 140f;
            fog.RemoveComponent<SphereCollider>();
            fog.AddComponent<BoxCollider>(x =>
            {
                x.size = new Vector3(50, 30, 50);
                x.isTrigger = true;
            });

            GameObject.Instantiate(fog, fog.transform.position + new Vector3(-500f, 0f, -500f), Quaternion.identity);

            GameObject koos = GameObject.Find("SceneInfo").transform.Find("KickOutOfShop").gameObject;

            GameObject realmBlocker = GameObject.Instantiate(koos);
            realmBlocker.gameObject.SetActive(true);
            realmBlocker.transform.position = koos.transform.position + new Vector3(-500f, 0f, -500f);

            newtShop.FindComponent<MeshFilter>("Bazaar_CaveMain").mesh = Main.assets.LoadAsset<GameObject>("CaveMeshMain.prefab").GetComponent<MeshFilter>().mesh;

            if (NetworkServer.active) {
                GameObject artifactPickup = GameObject.Instantiate(Paths.GameObject.GenericPickup, ArtifactPoint, Quaternion.identity);
                // Debug.Log(PickupCatalog.FindPickupIndex(ArtifactRewind.instance.ArtifactDef.artifactIndex));
                Main.ModLogger.LogError(PickupCatalog.FindPickupIndex(RoR2Content.Artifacts.Bomb.artifactIndex));
                // artifactPickup.GetComponent<GenericPickupController>().NetworkpickupIndex = PickupCatalog.FindPickupIndex(ArtifactRewind.instance.ArtifactDef.artifactIndex);
                NetworkServer.Spawn(artifactPickup);
            }
        }
    }
}
*/