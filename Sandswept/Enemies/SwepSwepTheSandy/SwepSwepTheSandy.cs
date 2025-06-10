using EntityStates.QuestVolatileBattery;
using static RoR2.GivePickupsOnStart;

namespace Sandswept.Enemies.SwepSwepTheSandy
{
    public class SwepSwepTheSandy
    {
        public static void Init()
        {
            var anyItemDisabled = Main.AllItems.Count < Main.EnabledItems.Count;
            var anyEquipmentDisabled = Main.AllEquipment.Count < Main.EnabledEquipment.Count;
            if (anyItemDisabled || anyEquipmentDisabled)
            {
                Main.ModLogger.LogError("Swepswep the Sandy will not load because you disabled an item or equipment. Shame on you.");
                return;
            }

            var master = PrefabAPI.InstantiateClone(Paths.GameObject.ScavLunar1Master, "SwepSwepMaster", true);
            var body = PrefabAPI.InstantiateClone(Paths.GameObject.ScavLunar1Body, "SwepSwepBody", true);
            var modelLocator = body.GetComponent<ModelLocator>();
            var modelTransform = modelLocator._modelTransform;
            var scavMesh = modelTransform.Find("ScavMesh").GetComponent<SkinnedMeshRenderer>();

            var newScavMeshMat = new Material(Paths.Material.matScavLunar);
            newScavMeshMat.SetColor("_TintColor", new Color32(162, 137, 69, 255));
            newScavMeshMat.SetColor("_EmColor", new Color32(203, 99, 0, 255));
            newScavMeshMat.SetFloat("_EmPower", 0.2f);
            newScavMeshMat.SetFloat("_GreenChannelBias", 0.38f);
            scavMesh.material = newScavMeshMat;

            var scavEyeMesh = modelTransform.Find("ScavEyeMesh").GetComponent<SkinnedMeshRenderer>();

            var newScavEyeMeshMat = new Material(Paths.Material.matScavLunarEyes);
            newScavEyeMeshMat.SetColor("_Color", new Color32(187, 5, 0, 255));
            newScavEyeMeshMat.SetFloat("_EmPower", 0.25f);

            scavEyeMesh.material = newScavEyeMeshMat;

            var scavBackpackMesh = modelTransform.Find("ScavBackpackMesh").GetComponent<SkinnedMeshRenderer>();

            var newScavBackpackMeshMat = new Material(Paths.Material.matScavLunarBackpack);
            newScavBackpackMeshMat.SetColor("_TintColor", Color.white);
            newScavBackpackMeshMat.SetColor("_EmColor", new Color32(123, 62, 0, 255));
            newScavBackpackMeshMat.SetFloat("_EmPower", 0.1f);

            scavBackpackMesh.material = newScavBackpackMeshMat;

            var rootBase = modelTransform.Find("ScavArmature/ROOT/base");
            var scavWeaponMesh = rootBase.Find("WeaponParent/ScavWeaponMesh").GetComponent<MeshRenderer>();

            var newScavWeaponMeshMat = new Material(Paths.Material.matTrimSheetConstructionBlueEmission);
            newScavWeaponMeshMat.SetColor("_TintColor", Color.white);
            newScavWeaponMeshMat.SetColor("_EmColor", new Color32(213, 107, 0, 255));
            newScavWeaponMeshMat.SetFloat("_EmPower", 0.1f);

            scavWeaponMesh.material = newScavWeaponMeshMat;

            var characterMaster = master.GetComponent<CharacterMaster>();
            characterMaster.bodyPrefab = body;

            var characterBody = body.GetComponent<CharacterBody>();
            characterBody.baseNameToken = "SANDSWEPT_SWEPSWEP_NAME";

            LanguageAPI.Add("SANDSWEPT_SWEPSWEP_NAME", "Swepswep the Sandy");

            var twistedScavengerSpawnCard = Paths.MultiCharacterSpawnCard.cscScavLunar;
            var count = twistedScavengerSpawnCard.masterPrefabs.Length;
            HG.ArrayUtils.ArrayAppend<GameObject>(ref twistedScavengerSpawnCard.masterPrefabs, ref count, in master);

            foreach (GivePickupsOnStart givePickupsOnStart in master.GetComponents<GivePickupsOnStart>())
            {
                givePickupsOnStart.enabled = false;
            }

            var itemDefInfos = new List<ItemDefInfo>
            {
                new() { itemDef = Items.Whites.FracturedTimepiece.instance.ItemDef, count = 2 }, // seems fine if only 2
                new() { itemDef = Items.Whites.RedSpringWater.instance.ItemDef, count = 2 }, // ditto
                new() { itemDef = Items.Whites.AmberKnife.instance.ItemDef, count = 1 }, // ditto
                new() { itemDef = Items.Greens.CrownsDiamond.instance.ItemDef, count = 1 }, // should be fine
                new() { itemDef = Items.Greens.DriftingPerception.instance.ItemDef, count = 1 }, // ditto
                new() { itemDef = RoR2Content.Items.Phasing, count = 1 }, // synergy & ditto
                new() { itemDef = RoR2Content.Items.BarrierOnOverHeal, count = 2 }, // synergy & ditto
                new() { itemDef = RoR2Content.Items.AutoCastEquipment, count = 100 }, // funny equip :pray:
                new() { itemDef = RoR2Content.Items.BarrierOnKill, count = 40 }, // synergy, should be fine
                new() { itemDef = DLC2Content.Items.AttackSpeedPerNearbyAllyOrEnemy, count = 1 }, // extra funny factor if npc allies get close
                new() { itemDef = DLC1Content.Items.OutOfCombatArmor, count = 10 }, // just because
                new() { itemDef = RoR2Content.Items.BonusGoldPackOnKill, count = 3 }, // funny
                new() { itemDef = RoR2Content.Items.Seed, count = 50 }, // ditto
                new() { itemDef = RoR2Content.Items.Behemoth, count = 1 }, // more funnn and interesting
                new() { itemDef = DLC2Content.Items.BoostAllStats, count = 1 }, // ditto
                new() { itemDef = DLC2Content.Items.MeteorAttackOnHighDamage, count = 10 }, // ditto
                new() { itemDef = RoR2Content.Items.ArmorReductionOnHit, count = 2 }, // ditto
                new() { itemDef = DLC1Content.Items.PermanentDebuffOnHit, count = 2 }, // ditto
                new() { itemDef = RoR2Content.Items.RandomDamageZone, count = 1 } // ditto
            };

            var newGivePickupsOnStart = master.AddComponent<GivePickupsOnStart>();
            newGivePickupsOnStart.itemDefInfos = itemDefInfos.ToArray();
            newGivePickupsOnStart.itemInfos = Array.Empty<ItemInfo>();

            newGivePickupsOnStart.equipmentDef = Equipment.Lunar.CorruptedCatalyst.instance.EquipmentDef;

            ContentAddition.AddMaster(master);
            ContentAddition.AddBody(body);
        }
    }
}