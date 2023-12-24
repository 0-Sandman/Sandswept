using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace Sandswept.Elites
{
    internal class Osmium : EliteEquipmentBase<Osmium>
    {
        public override string EliteEquipmentName => "Artificial Void";

        public override string EliteAffixToken => "OSMIUM";

        public override string EliteEquipmentPickupDesc => "Become an aspect of singularity.";

        public override string EliteEquipmentFullDescription => "Become an aspect of singularity.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Osmium";

        public override GameObject EliteEquipmentModel => CreateAffixModel(new Color32(110, 64, 255, 255));

        public override Sprite EliteEquipmentIcon => Main.hifuSandswept.LoadAsset<Sprite>("texOsmiumAffix.png");

        public override Sprite EliteBuffIcon => Main.hifuSandswept.LoadAsset<Sprite>("texOsmiumBuff.png");

        public override Texture2D EliteRampTexture => Main.hifuSandswept.LoadAsset<Texture2D>("texRampOsmium.png");

        public override float DamageMultiplier => 6f;
        public override float HealthMultiplier => 18f;

        public static GameObject warbanner;

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/ElitePoison/edPoison.asset").WaitForCompletion())).ToArray();

        public override Color EliteBuffColor => new Color32(110, 64, 255, 255);

        public static GameObject aura;
        public static BuffDef outsideAura;
        public static BuffDef insideAura;
        public static BuffDef noJump;
        public static GameObject groundVFX;
        public static GameObject distortionVFX;

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
            distortionVFX = PrefabAPI.InstantiateClone(Assets.GameObject.TreebotShockwavePullEffect, "Osmium Pull Down Distortion VFX", false);

            var transform = distortionVFX.transform;
            var pollenSingle = transform.GetChild(1);
            var pollenDust = transform.GetChild(2);
            var pollenRadial = transform.GetChild(3);
            var pollenSingle2 = transform.GetChild(4);
            var distortionWave2 = transform.GetChild(7).GetComponent<ParticleSystem>().main.startColor;
            pollenSingle.gameObject.SetActive(false);
            pollenDust.gameObject.SetActive(false);
            pollenRadial.gameObject.SetActive(false);
            pollenSingle2.gameObject.SetActive(false);
            distortionWave2.color = new Color32(110, 64, 255, 255);

            ContentAddition.AddEffect(distortionVFX);

            groundVFX = PrefabAPI.InstantiateClone(Assets.GameObject.PurchaseLockVoid, "Osmium Pull Down VFX", false);

            groundVFX.RemoveComponent<NetworkIdentity>();

            var sphere2 = groundVFX.transform.GetChild(0).GetComponent<MeshRenderer>();

            var newMat = Object.Instantiate(Assets.Material.matVoidCampLock);
            newMat.SetColor("_TintColor", new Color32(62, 24, 211, 196));

            sphere2.material = newMat;

            var effectComponent = groundVFX.AddComponent<EffectComponent>();
            effectComponent.applyScale = true;
            effectComponent.positionAtReferencedTransform = true;
            effectComponent.parentToReferencedTransform = true;

            var VFXAttributes = effectComponent.AddComponent<VFXAttributes>();

            var destroyOnTimer = groundVFX.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 0.5f;

            ContentAddition.AddEffect(groundVFX);

            noJump = ScriptableObject.CreateInstance<BuffDef>();
            noJump.isDebuff = false;
            noJump.isCooldown = false;
            noJump.canStack = false;
            noJump.isHidden = false;
            noJump.iconSprite = Main.hifuSandswept.LoadAsset<Sprite>("texBuffOsmiumGravity.png");
            noJump.buffColor = new Color32(110, 64, 255, 255);
            noJump.name = "Osmium - Jump Disabled";

            outsideAura = ScriptableObject.CreateInstance<BuffDef>();
            outsideAura.isDebuff = false;
            outsideAura.isCooldown = false;
            outsideAura.canStack = false;
            outsideAura.isHidden = true;
            outsideAura.buffColor = Color.blue;
            outsideAura.iconSprite = Assets.BuffDef.bdArmorBoost.iconSprite;
            outsideAura.name = "Outside Osmium Aura";

            insideAura = ScriptableObject.CreateInstance<BuffDef>();
            insideAura.isDebuff = false;
            insideAura.isCooldown = false;
            insideAura.canStack = false;
            insideAura.isHidden = true;
            insideAura.buffColor = Color.red;
            insideAura.iconSprite = Assets.BuffDef.bdAttackSpeedOnCrit.iconSprite;
            insideAura.name = "Inside Osmium Aura";

            ContentAddition.AddBuffDef(noJump);
            ContentAddition.AddBuffDef(outsideAura);
            ContentAddition.AddBuffDef(insideAura);

            aura = PrefabAPI.InstantiateClone(Assets.GameObject.RailgunnerMineAltDetonated, "Osmium Aura");
            aura.RemoveComponent<SlowDownProjectiles>();

            aura.transform.localPosition = Vector3.zero;
            aura.transform.localEulerAngles = Vector3.zero;

            var areaIndicator = aura.transform.Find("AreaIndicator");
            var softGlow = areaIndicator.Find("SoftGlow");
            var sphere = areaIndicator.Find("Sphere");
            var light = areaIndicator.Find("Point Light").GetComponent<Light>();
            var core = areaIndicator.Find("Core");

            softGlow.gameObject.SetActive(false);
            core.gameObject.SetActive(false);

            light.intensity = 60f;
            light.range = 24f;
            light.color = new Color32(204, 0, 255, 255);

            var chargeIn = areaIndicator.Find("ChargeIn");
            var psr = chargeIn.GetComponent<ParticleSystemRenderer>();
            var ps = chargeIn.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.45f;

            var emission = ps.emission;
            emission.rateOverTime = 60f;

            var newMat2 = Object.Instantiate(Assets.Material.matRailgunTracerHead1);
            newMat2.SetColor("_TintColor", Color.black);

            psr.material = newMat2;

            var newRadiusMat = Object.Instantiate(Assets.Material.matMoonbatteryCrippleRadius);
            newRadiusMat.SetTexture("_RemapTex", Assets.Texture2D.texRampBeetleBreath);
            newRadiusMat.SetColor("_TintColor", new Color32(60, 0, 255, 255));
            newRadiusMat.SetFloat("_SoftFactor", 0.6503434f);
            newRadiusMat.SetFloat("_SoftPower", 0.5117764f);
            newRadiusMat.SetFloat("_Boost", 5f);
            newRadiusMat.SetFloat("_RimPower", 3.624829f);
            newRadiusMat.SetFloat("_RimStrength", 0.4449388f);
            newRadiusMat.SetFloat("_AlphaBoost", 2.27f);
            newRadiusMat.SetFloat("_IntersectionStrength", 1.38f);

            var newIndicatorMat = Object.Instantiate(Assets.Material.matCrippleSphereIndicator);
            newIndicatorMat.SetTexture("_RemapTex", Assets.Texture2D.texRampFogDebug);
            newIndicatorMat.SetColor("_TintColor", new Color32(6, 0, 255, 7));
            newIndicatorMat.SetFloat("_SoftFactor", 3.3f);
            newIndicatorMat.SetFloat("_SoftPower", 1f);
            newIndicatorMat.SetFloat("_Boost", 0.34f);
            newIndicatorMat.SetFloat("_RimPower", 3.830029f);
            newIndicatorMat.SetFloat("_RimStrength", 0.9263985f);
            newIndicatorMat.SetFloat("_AlphaBoost", 2.27f);
            newIndicatorMat.SetFloat("_IntersectionStrength", 20f);

            var meshRenderer = sphere.GetComponent<MeshRenderer>();
            Material[] sharedMaterials = meshRenderer.sharedMaterials;
            sharedMaterials[0] = newRadiusMat;
            sharedMaterials[1] = newIndicatorMat;
            meshRenderer.SetSharedMaterials(sharedMaterials, 2);

            var buffWard = aura.GetComponent<BuffWard>();
            buffWard.buffDef = insideAura;
            buffWard.expires = false;
            buffWard.invertTeamFilter = true;
            buffWard.buffDuration = 0.2f;
            buffWard.radius = 20f;
            buffWard.interval = 0.1f;

            var teamFilter = aura.GetComponent<TeamFilter>();
            teamFilter.defaultTeam = TeamIndex.None;

            PrefabAPI.RegisterNetworkPrefab(aura);

            // On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.TakeDamageForce_Vector3_bool_bool += HealthComponent_TakeDamageForce_Vector3_bool_bool;
            On.RoR2.HealthComponent.TakeDamageForce_DamageInfo_bool_bool += HealthComponent_TakeDamageForce_DamageInfo_bool_bool;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            // IL.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
        }

        private void GenericCharacterMain_ProcessJump(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<GenericCharacterMain>("jumpInputReceived")))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, GenericCharacterMain, bool>>((orig, self) =>
                {
                    var body = self.characterBody;
                    if (body && body.HasBuff(noJump))
                    {
                        return false;
                    }

                    return orig;
                });
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Jump Hook");
            }

            c.Index = 0;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<EntityState>("get_characterBody"),
                x => x.MatchLdloc(out _)))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, GenericCharacterMain, float>>((orig, self) =>
                {
                    var body = self.characterBody;
                    if (body && body.HasBuff(noJump))
                    {
                        return 0f;
                    }
                    return orig;
                });
            }
            else
            {
                Main.ModLogger.LogError("Failed to apply Jump Speed Hook");
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var attackerBody = attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var victimBody = self.body;
                    if (victimBody)
                    {
                        if (victimBody.HasBuff(insideAura) || victimBody.HasBuff(Instance.EliteBuffDef))
                        {
                            if (attackerBody.HasBuff(outsideAura))
                            {
                                damageInfo.damage *= 0.25f;
                                damageInfo.procCoefficient *= 0.5f;
                            }
                            else if (attackerBody.HasBuff(insideAura))
                            {
                                damageInfo.damage *= 1.33f;
                            }
                        }
                    }
                }
            }

            orig(self, damageInfo);
        }

        private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == insideAura)
            {
                self.AddBuff(outsideAura);
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody characterBody)
        {
            var sfp = characterBody.GetComponent<OsmiumController>();
            if (sfp == null && !characterBody.HasBuff(insideAura) && !characterBody.HasBuff(outsideAura))
            {
                characterBody.AddBuff(outsideAura);
            }
            if (characterBody.HasBuff(Instance.EliteBuffDef))
            {
                if (sfp == null)
                {
                    characterBody.gameObject.AddComponent<OsmiumController>();
                    AkSoundEngine.PostEvent(Events.Play_artifactBoss_spawn, characterBody.gameObject);
                }
            }
            else if (sfp != null)
            {
                characterBody.gameObject.RemoveComponent<OsmiumController>();
            }
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == insideAura)
            {
                self.RemoveBuff(outsideAura);
            }
        }

        private void HealthComponent_TakeDamageForce_DamageInfo_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_DamageInfo_bool_bool orig, HealthComponent self, DamageInfo damageInfo, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                damageInfo.force = Vector3.zero;
            }
            orig(self, damageInfo, alwaysApply, disableAirControlUntilCollision);
        }

        private void HealthComponent_TakeDamageForce_Vector3_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_Vector3_bool_bool orig, HealthComponent self, Vector3 force, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(EliteBuffDef))
            {
                force = Vector3.zero;
            }
            orig(self, force, alwaysApply, disableAirControlUntilCollision);
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }

    public class OsmiumController : MonoBehaviour
    {
        public CharacterBody body;
        public HealthComponent healthComponent;
        public GameObject wardInstance;
        public Vector3 pullDownStrength = new(0f, -30f, 0f);
        public float timer;
        public float pullDownInterval = 0.7f;
        public Vector3 myPosition;
        public float radius;
        public Transform model;

        public void Start()
        {
            healthComponent = GetComponent<HealthComponent>();
            body = healthComponent.body;
            model = body.modelLocator?.modelTransform;
            radius = Util.Remap(body.baseMaxHealth, 0f, 1125, 13f, 40f);
            wardInstance = Instantiate(Osmium.aura, model);
            wardInstance.GetComponent<BuffWard>().Networkradius = radius;
            wardInstance.GetComponent<TeamFilter>().teamIndex = TeamIndex.None;
            NetworkServer.Spawn(wardInstance);
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (!healthComponent.alive && NetworkServer.active)
            {
                Destroy(this);
            }

            if (wardInstance)
            {
                wardInstance.transform.localPosition = Vector3.zero;
                wardInstance.transform.localEulerAngles = Vector3.zero;
                wardInstance.transform.position = model.position;
                wardInstance.transform.eulerAngles = Vector3.zero;
                // I HATE THIS WHY DOES IT GET OFFSET OVER TIME
            }

            if (timer >= pullDownInterval)
            {
                // Main.ModLogger.LogWarning("checking distance");
                StartCoroutine(CheckDistance());
                timer = 0f;
            }
        }

        public IEnumerator CheckDistance()
        {
            yield return new WaitForSeconds(0.1f);

            bool anyEnemies = false;

            for (int i = 0; i < CharacterBody.instancesList.Count; i++)
            {
                var enemyBody = CharacterBody.instancesList[i];
                if (!enemyBody)
                {
                    continue;
                }

                var enemyMotor = enemyBody.characterMotor;
                if (!enemyMotor)
                {
                    continue;
                }

                if (enemyMotor.isGrounded)
                {
                    continue;
                }

                if (enemyBody.teamComponent.teamIndex == body.teamComponent.teamIndex)
                {
                    continue;
                }

                myPosition = body.corePosition;
                var enemyPosition = enemyBody.corePosition;
                if (Vector3.Distance(enemyPosition, myPosition) < radius)
                {
                    anyEnemies = true;
                    // Main.ModLogger.LogError("enemy within radius, trying to pull down");
                    PullDown(enemyBody, enemyMotor);
                }
            }

            if (anyEnemies)
            {
                AkSoundEngine.PostEvent(Events.Play_artifactBoss_attack1_explode, gameObject);
            }

            yield return null;
        }

        public void PullDown(CharacterBody characterBody, CharacterMotor characterMotor)
        {
            var effectData = new EffectData { origin = characterBody.corePosition, rotation = Quaternion.identity, scale = characterBody.radius * 1.5f };
            effectData.SetNetworkedObjectReference(characterBody.gameObject);
            EffectManager.SpawnEffect(Osmium.groundVFX, effectData, true);

            if (!NetworkServer.active)
            {
                return;
            }

            characterBody.AddTimedBuff(Osmium.noJump, 0.5f);

            var damageInfo = new DamageInfo()
            {
                attacker = gameObject,
                canRejectForce = false,
                crit = false,
                damage = 0,
                force = pullDownStrength * characterMotor.mass,
                inflictor = gameObject,
                position = characterBody.corePosition,
                procCoefficient = 0,
                damageType = DamageType.BypassBlock
            };

            characterBody.healthComponent.TakeDamageForce(damageInfo);
        }

        public void OnDestroy()
        {
            if (NetworkServer.active)
            {
                Destroy(wardInstance);
            }
        }
    }
}