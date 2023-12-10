namespace Sandswept.Survivors.Ranger.ItemDisplays
{
    internal class Funny
    {
        private static Dictionary<string, GameObject> itemDisplayPrefabs = new();

        public static void Populate()
        {
            PopulateFromBody("MageBody");
        }

        private static void PopulateFromBody(string bodyName)
        {
            ItemDisplayRuleSet itemDisplayRuleSet = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/" + bodyName).GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

            ItemDisplayRuleSet.KeyAssetRuleGroup[] item = itemDisplayRuleSet.keyAssetRuleGroups;

            for (int i = 0; i < item.Length; i++)
            {
                ItemDisplayRule[] rules = item[i].displayRuleGroup.rules;

                for (int j = 0; j < rules.Length; j++)
                {
                    GameObject followerPrefab = rules[j].followerPrefab;
                    if (followerPrefab)
                    {
                        string key = followerPrefab.name?.ToLowerInvariant();
                        if (!itemDisplayPrefabs.ContainsKey(key))
                        {
                            itemDisplayPrefabs[key] = followerPrefab;
                        }
                    }
                }
            }
        }

        public static GameObject LoadDisplay(string name)
        {
            if (itemDisplayPrefabs.ContainsKey(name.ToLowerInvariant()))
            {
                if (itemDisplayPrefabs[name.ToLowerInvariant()])
                {
                    GameObject display = itemDisplayPrefabs[name.ToLowerInvariant()];

                    return display;
                }
            }
            Main.ModLogger.LogFatal("bruh you typed the idrs name wrong: " + name);
            return null;
        }

        public static void SetItemDisplayRules(List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules)
        {
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Jetpack,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBugWings"),
                            childName = "Chest",
                            localPos = new Vector3(0.0009F, 0.2767F, -0.1F),
                            localAngles = new Vector3(21.4993F, 358.6616F, 358.3334F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.BFG,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBugWings"),
                            childName = "Chest",
                            localPos = new Vector3(0.0009F, 0.2767F, -0.1F),
                            localAngles = new Vector3(21.4993F, 358.6616F, 358.3334F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Jetpack,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBugWings"),
                            childName = "Chest",
                            localPos = new Vector3(0.0009F, 0.2767F, -0.1F),
                            localAngles = new Vector3(21.4993F, 358.6616F, 358.3334F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
        }
    }
}