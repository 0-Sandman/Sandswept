using R2API.Networking;
using RoR2.ContentManagement;
using Sandswept.Utils.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.PlayerLoop;

namespace Sandswept.Equipment
{
    [ConfigSection("Equipment :: Twinblade")]
    public class Twinblade : EquipmentBase
    {
        public override string EquipmentName => "Twinblade";

        public override string EquipmentLangTokenName => "TWINBLADE";

        public override string EquipmentPickupDesc => "";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";
   
        public override GameObject EquipmentModel => Utils.Assets.GameObject.GenericPickup;
        public override float Cooldown => 20f;
        public override Sprite EquipmentIcon => Utils.Assets.Sprite.texEquipmentBGIcon;
        [ConfigField("Activation Length", "", 0.2f)]
        public static float activationTime;
        [ConfigField("Base Damage", "Decimal", 20f)]
        public static float baseDamage;
        [ConfigField("Hit Damage", "Decimal", 2f)]
        public static float hitDamage;
        [ConfigField("Blast Field Radius","",15f)]
        public static float radius;
        [ConfigField("Projectile Graze Radius","",2.5f)]
        public static float grazeRadius;

        public static GameObject vfx;
        public static BlastAttack blastAttack;
        
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();

            CreateEquipment();
            Hooks();
        }
  
        public override void Hooks()
        {
            vfx = PrefabAPI.InstantiateClone(Utils.Assets.GameObject.EngiShield, "Parry VFX");
            Object.Destroy(vfx.GetComponent<TemporaryVisualEffect>());
            var component = vfx.AddComponent<EffectComponent>();
            //component.applyScale = true;
            
            vfx.RemoveComponents<ObjectScaleCurve>();
            var scale = vfx.AddComponent<ObjectScaleCurve>();
            scale.overallCurve = Main.dgoslingAssets.LoadAsset<AnimationCurveAsset>("ACAparryVFXScale").value;
            scale.useOverallCurveOnly = true;
            scale.timeMax = 1;
            vfx.GetComponent<DestroyOnTimer>().enabled = true;
            Main.EffectPrefabs.Add(vfx);
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody obj)
        {
            obj.AddItemBehavior<TwinbladeItemBehavior>((obj.inventory.GetEquipment(obj.inventory.activeEquipmentSlot).equipmentDef == this.EquipmentDef)?1:0);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if(NetworkServer.active && self.body.HasBuff(Buffs.ParryBuff.instance.BuffDef) && damageInfo.damage > 0f)
            {
                HandleParryBuffsServer(self.body,damageInfo);
                return;
            }
            orig(self, damageInfo);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if(slot.characterBody == null) return false;
            TwinbladeItemBehavior twinbladeItemBehavior = slot.characterBody.GetComponentInChildren<TwinbladeItemBehavior>();
            if (twinbladeItemBehavior)
            {
                twinbladeItemBehavior.activated = true;
            }
            return true;
        }
        public static void HandleParryBuffsServer(CharacterBody body, DamageInfo damageInfo)
        {
            if(body.HasBuff(Buffs.ParryBuff.instance.BuffDef)) body.RemoveBuff(Buffs.ParryBuff.instance.BuffDef);
            if (!body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef)) body.AddBuff(Buffs.ParryActivatedBuff.instance.BuffDef);

            body.AddTimedBuff(Utils.Assets.BuffDef.bdImmune, 0.5f);
            body.GetComponentInChildren<TwinbladeItemBehavior>().damageInfo = damageInfo;
            return;
        }
    }
    public class TwinbladeItemBehavior : CharacterBody.ItemBehavior
    {

        GameObject effectPrefab;
        float timer = 0;
        public DamageInfo damageInfo;
        public bool activated = false;
        bool hasFired = false;
        float projdelradius;
        public void Start()
        {
          //  if (NetworkServer.active)
         //   {
        //        CleanBuffsServer();
        //        if (!body.HasBuff(Buffs.ParryBuff.instance.BuffDef)) body.AddBuff(Buffs.ParryBuff.instance.BuffDef);
        //    }
            projdelradius = Twinblade.grazeRadius + body.radius;
        }
        void CleanBuffsServer()
        {
            if(!NetworkServer.active) return;
            if (body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef)) body.RemoveBuff(Buffs.ParryActivatedBuff.instance.BuffDef);
            if (body.HasBuff(Buffs.ParryBuff.instance.BuffDef)) body.RemoveBuff(Buffs.ParryBuff.instance.BuffDef);
        }
        void FixedUpdate()
        {
            if (activated)
            {
                
                if (!effectPrefab)
                {
                    effectPrefab = Twinblade.vfx;
                    effectPrefab.GetComponent<ObjectScaleCurve>().baseScale = new Vector3(Twinblade.radius, Twinblade.radius, Twinblade.radius);
                    CleanBuffsServer();
                    if (!body.HasBuff(Buffs.ParryBuff.instance.BuffDef)) body.AddBuff(Buffs.ParryBuff.instance.BuffDef);
                }
                if (body.HasBuff(Buffs.ParryBuff.instance.BuffDef)||body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef))
                {
                    timer += Time.fixedDeltaTime;
                    if (damageInfo != null && !hasFired)
                    {
                        DoAttack();
                    }
                }
                
                if (timer >= Twinblade.activationTime)
                {
                    reset();
                }
                
            }
        }
        void reset()
        {
            damageInfo = null;
            activated = false;
            hasFired = false;
            timer = 0f;
            effectPrefab = null;
            CleanBuffsServer();
        }
        void DoAttack()
        {
            if (!NetworkServer.active || !body) return;
            
            bool parry = body.HasBuff(Buffs.ParryActivatedBuff.instance.BuffDef);
            if(!parry) return;
            DamageType damageType = DamageType.Shock5s;

            float damageCoef = (Twinblade.baseDamage * body.damage) + (damageInfo.damage * Twinblade.hitDamage);
            float radius = Twinblade.radius;
            if (parry)
            {
                hasFired = true;

                DeleteProjectilesServer(projdelradius);
                EffectData effectData = new() { 
                    origin = body.corePosition,
                    scale = radius,
                    rootObject = body.gameObject
                   
                   
                };
                EffectManager.SpawnEffect(effectPrefab, effectData,true);
                BlastAttack.Result result;
                result = new BlastAttack
                {
                    //impactEffect = EffectCatalog.FindEffectIndexFromPrefab()
                    attacker = body.gameObject,
                    inflictor = body.gameObject,
                    teamIndex = TeamComponent.GetObjectTeam(body.gameObject),
                    baseDamage = 0f,
                    baseForce = 0f,
                    position = body.corePosition,
                    radius = radius,
                    falloffModel = BlastAttack.FalloffModel.None,
                    damageType = damageType,
                    attackerFiltering = AttackerFiltering.NeverHitSelf
                }.Fire();
                damageInfo.attacker.GetComponent<CharacterBody>().healthComponent.TakeDamage(new DamageInfo
                {
                    attacker = body.gameObject,
                    damage = damageCoef,
                    damageType = damageType,
                    crit = Util.CheckRoll(body.master.luck, body.master),
                    position = body.corePosition,
                    force = Vector3.zero,
                    damageColorIndex = DamageColorIndex.Default,
                    procCoefficient = 0f,
                    procChainMask = default(ProcChainMask)


                });
                reset();
            }
        }

        void DeleteProjectilesServer(float rad)
        {
            List<ProjectileController> projectileControllers = new List<ProjectileController>();

            Collider[] array = Physics.OverlapSphere(body.corePosition, rad, LayerIndex.projectile.mask);
            for (int i = 0; i < array.Length; i++)
            {
                ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                if (pc && !pc.cannotBeDeleted && pc.owner != body.gameObject && !(pc.teamFilter && pc.teamFilter.teamIndex == TeamComponent.GetObjectTeam(body.gameObject)))
                {
                    bool cannotDelete = false;
                    ProjectileSimple ps = pc.gameObject.GetComponent<ProjectileSimple>();
                    ProjectileCharacterController pcc = pc.gameObject.GetComponent<ProjectileCharacterController>();

                    if ((!ps || (ps.desiredForwardSpeed == 0)) && !pcc)
                    {
                        cannotDelete = true;
                    }

                    if (!cannotDelete && !projectileControllers.Contains(pc))
                    {
                        projectileControllers.Add(pc);
                    }
                }
            }

            int projectilesDeleted = projectileControllers.Count;
            for (int i = 0; i < projectilesDeleted; i++)
            {
                GameObject toDelete = projectileControllers[i].gameObject;
                if (toDelete)
                    Object.Destroy(toDelete);
            }
        }
    }
}
