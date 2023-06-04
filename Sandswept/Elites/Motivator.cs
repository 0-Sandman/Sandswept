using HG;
using System.Linq;

namespace Sandswept.Elites
{
    internal class Motivator : EliteEquipmentBase<Motivator>
    {
        public override string EliteEquipmentName => "He is Kill Yourself";

        public override string EliteAffixToken => "MOTIVATOR";

        public override string EliteEquipmentPickupDesc => "Become an aspect of low tier god.";

        public override string EliteEquipmentFullDescription => "Become an aspect of low tier god.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Motivator";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(0, 0, 0, 255));

        public override Sprite EliteEquipmentIcon => null;

        public override Sprite EliteBuffIcon => null;

        public override Texture2D EliteRampTexture => null;

        public override float DamageMultiplier => 2f;
        public override float HealthMultiplier => 4f;

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/EliteFire/edFire.asset").WaitForCompletion())).ToArray();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        private void CreateEliteTiers()
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            var info = report.damageInfo;
            if (!NetworkServer.active)
            {
                return;
            }

            if (!info.attacker)
            {
                return;
            }

            var motivatorController = info.attacker.GetComponent<MotivatorController>();
            if (!motivatorController)
            {
                return;
            }
            if (Util.CheckRoll(100f * info.procCoefficient))
            {
                if (Util.HasEffectiveAuthority(info.attacker))
                {
                    motivatorController.Proc();
                }
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body)
        {
            var sfp = body.GetComponent<MotivatorController>();
            if (body.HasBuff(Instance.EliteBuffDef))
            {
                if (sfp == null)
                {
                    body.gameObject.AddComponent<MotivatorController>();
                }
            }
            else if (sfp != null)
            {
                body.gameObject.RemoveComponent<MotivatorController>();
            }
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }

    public class MotivatorController : MonoBehaviour
    {
        public GameObject warbannerPrefab = Utils.Assets.GameObject.WarbannerWard;
        public GameObject warbannerInstance;
        public float warbannerRadius = 20f;
        public float onHitRadius = 13f;
        public CharacterBody body;
        public HealthComponent healthComponent;
        public TeamIndex team;
        public static readonly SphereSearch sphereSearch = new();
        public static readonly List<HurtBox> hurtBoxBuffer = new();
        public static List<MotivatorController> motivatorControllers = new();

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            healthComponent = body?.healthComponent;
            if (body)
            {
                warbannerRadius += body.radius;
                onHitRadius += body.radius;
            }

            warbannerInstance = Instantiate(warbannerPrefab, body.transform.position, Quaternion.identity);
            warbannerInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
            warbannerInstance.GetComponent<BuffWard>().Networkradius = warbannerRadius;
            NetworkServer.Spawn(warbannerInstance);

            motivatorControllers.Add(this);
        }

        public void Proc()
        {
            if (!body)
            {
                return;
            }

            if (!healthComponent)
            {
                return;
            }

            if (!healthComponent.alive)
            {
                return;
            }

            Vector3 corePosition = body.corePosition;
            sphereSearch.origin = corePosition;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.radius = onHitRadius;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.GetHurtBoxes(hurtBoxBuffer);
            sphereSearch.ClearCandidates();

            for (int i = 0; i < hurtBoxBuffer.Count; i++)
            {
                var hurtBox = hurtBoxBuffer[i];
                if (hurtBox.healthComponent)
                {
                    var body = hurtBox.healthComponent.body;
                    if (body && !body.HasBuff(Motivator.Instance.EliteBuffDef) && body.teamComponent.teamIndex == body.teamComponent.teamIndex)
                    {
                        body.AddTimedBuff(RoR2Content.Buffs.TeamWarCry, 3f);
                    }
                }
            }

            hurtBoxBuffer.Clear();

            // Someone Else make vfx later and uncomment
            /*
            EffectManager.SpawnEffect(buffVFX, new EffectData
            {
                origin = corePosition,
                scale = radius
            }, true);
            */
        }

        public void OnDestroy()
        {
            NetworkServer.Destroy(warbannerInstance);
            motivatorControllers.Remove(this);
        }
    }
}