using RoR2;

namespace Sandswept.Survivors.Ranger.Pod
{
    public static class RangerPod
    {
        public static GameObject prefabDefault;
        public static GameObject prefabMajor;
        public static GameObject prefabRenegade;
        public static GameObject prefabMileZero;
        public static GameObject prefabSandswept;
        public static Material rangerPodMat;

        public static void Init()
        {
            prefabDefault = CreateRecolor("Default");
            prefabMajor = CreateRecolor("Major");
            prefabRenegade = CreateRecolor("Renegade");
            prefabMileZero = CreateRecolor("MileZero");
            prefabSandswept = CreateRecolor("Sandswept");

            On.RoR2.InstantiatePrefabBehavior.Start += InstantiatePrefabBehavior_Start;
            On.RoR2.Run.HandlePlayerFirstEntryAnimation += Run_HandlePlayerFirstEntryAnimation;
        }

        private static void Run_HandlePlayerFirstEntryAnimation(On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig, Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (body && body.baseNameToken == "SS_RANGER_BODY_NAME")
            {
                // Main.ModLogger.LogError("is ranger");
                var modelLocator = body.modelLocator;
                var master = body.master;
                if (modelLocator && master)
                {
                    // Main.ModLogger.LogError("has model locator");
                    var modelTransform = modelLocator.modelTransform;

                    if (modelTransform)
                    {
                        // Main.ModLogger.LogError("has model transform");

                        var loadoutSkinIndex = body.master.loadout.bodyLoadoutManager.GetSkinIndex(body.bodyIndex);
                        /*
                        var modelSkinController = modelTransform.GetComponent<ModelSkinController>();

                        var overallSkinIndex = (int)modelSkinController.skins[loadoutSkinIndex].skinIndex;
                        */
                        // Main.ModLogger.LogError(loadoutSkinIndex);
                        // Main.ModLogger.LogError(overallSkinIndex);

                        // gives 0-3 and 46-49 respectively

                        body.preferredPodPrefab = loadoutSkinIndex switch
                        {
                            1 => prefabSandswept,
                            2 => prefabMajor,
                            3 => prefabRenegade,
                            4 => prefabMileZero,
                            _ => prefabDefault
                        };

                        // Main.ModLogger.LogError("preferred body prefab is " + body.preferredPodPrefab);
                    }
                }
            }

            orig(self, body, spawnPosition, spawnRotation);
        }

        public static GameObject CreateRecolor(string name)
        {
            var trimmedName = name.Replace(" ", "");
            var rangerPodMat = Object.Instantiate(Paths.Material.matEscapePod);
            rangerPodMat.SetTexture("_MainTex", Main.hifuSandswept.LoadAsset<Texture2D>("texRangerPod" + trimmedName + ".png"));
            rangerPodMat.SetColor("_Color", Color.white);
            rangerPodMat.SetFloat("_Smoothness", 0.5f);
            rangerPodMat.SetFloat("_SpecularExponent", 1.082716f);
            rangerPodMat.SetFloat("_NormalStrength", 2f);

            var prefab = PrefabAPI.InstantiateClone(Paths.GameObject.SurvivorPod, "Ranger Pod " + name);

            var modelLocator = prefab.GetComponent<ModelLocator>();

            var modelTransform = modelLocator._modelTransform;
            var childLocator = modelTransform.GetComponent<ChildLocator>();
            var door = childLocator.transformPairs[4];
            var escapePodDoorMesh = door.transform.GetChild(1);
            escapePodDoorMesh.GetComponent<MeshRenderer>().material = rangerPodMat;

            var baseObject = door.transform.parent;
            var escapePodMesh = baseObject.GetChild(1);
            escapePodMesh.GetComponent<MeshRenderer>().material = rangerPodMat;

            var releaseVFX = baseObject.GetChild(9);
            var door2 = releaseVFX.GetChild(2);
            door2.GetComponent<MeshRenderer>().material = rangerPodMat;

            PrefabAPI.RegisterNetworkPrefab(prefab);

            return prefab;
        }

        private static void InstantiatePrefabBehavior_Start(On.RoR2.InstantiatePrefabBehavior.orig_Start orig, InstantiatePrefabBehavior self)
        {
            orig(self);
            if (self.prefab == Paths.GameObject.SurvivorPodBatteryPanel)
            {
                var trans = self.transform;
                var a = trans.GetChild(0);
                if (a)
                {
                    var b = a.GetChild(0);
                    if (b)
                    {
                        var c = b.GetChild(0);
                        if (c)
                        {
                            var d = c.GetChild(0);
                            if (d)
                            {
                                var e = d.GetChild(10);
                                if (e)
                                {
                                    var f = e.GetChild(2);
                                    if (f)
                                    {
                                        var g = f.GetChild(1);
                                        if (g)
                                        {
                                            g.gameObject.SetActive(false);
                                            // lmao
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}