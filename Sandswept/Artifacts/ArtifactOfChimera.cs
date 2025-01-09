using BepInEx.Configuration;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using RoR2;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.AddressableAssets;
using R2API;
using System.Collections.Generic;
using Sandswept.Artifacts;
using Sandswept;
using RoR2.ContentManagement;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Sandswept.Artifacts
{
    [ConfigSection("Artifacts :: Chimera")]
    internal class ArtifactOfChimera : ArtifactBase<ArtifactOfChimera>
    {
        public override string ArtifactName => "Artifact of Chimera";

        public override string ArtifactLangTokenName => "CHIMERA";

        // sound chimera :>
        public override string ArtifactDescription => "Your vision and hearing may mislead you.";

        public override Sprite ArtifactEnabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessEnabled.png");

        public override Sprite ArtifactDisabledIcon => Main.hifuSandswept.LoadAsset<Sprite>("texArtifactOfBlindnessDisabled.png");

        public static List<string> randomSounds = new()
        {
            "Play_hermitCrab_spawn", "Play_imp_overlord_spawn", "Play_acid_larva_spawn", "Play_voidJailer_spawn", "Play_nullifier_spawn", "Play_bison_spawn",
            "Play_affix_void_bug_spawn", "Play_gravekeeper_spawn_01", "Play_lemurian_spawn",
            "Play_scav_spawn", "Play_gup_spawn", "Play_minorConstruct_spawn", "Play_voidRaid_spawn", "Play_clayGrenadier_spawn",
            "Play_vagrant_spawn", "Play_engi_R_turret_spawn", "Play_beetle_guard_spawn", "Play_titanboss_spawn",
            "Play_elite_antiHeal_spawn","Play_elite_antiHeal_urchin_spawn", "Play_clayboss_spawn", "Play_moonBrother_spawn",
            "Play_item_use_gummyClone_spawn", "Play_voidBarnacle_spawn", "Play_roboBall_spawn", "Play_grandParent_spawn",
            "Play_beetle_worker_spawn", "Play_majorConstruct_spawn_surface", "Play_minimushroom_spawn",
            "Play_affix_void_spawn", "Play_beetle_queen_spawn", "Play_parent_spawn", "Play_lunar_exploder_spawn", "Play_elite_haunt_spawn", "Play_clayBruiser_spawn",
            "Play_flyingVermin_spawn", "Play_blindVermin_spawn", "Play_vulture_spawn", "Play_artifactBoss_spawn", "Play_golem_spawn", "Play_jellyfish_spawn",
            "Play_bellBody_spawn", "Play_imp_spawn", "Play_elite_antiHeal_turret_spawn", "Play_lunar_wisp_spawn", "Play_lemurianBruiser_spawn", "Play_wisp_spawn",
            "Play_lunar_golem_spawn",

            "Play_grandParent_attack3_sun_spawn", "Play_vagrant_attack1_spawn", "Play_vagrant_attack1_shoot", "Play_clayBruiser_attack1_windUp", "Play_clayBruiser_attack2_shoot",
            "Play_lemurianBruiser_m1_charge", "Play_lemurianBruiser_m1_charge", "Play_minorConstruct_attack_shoot", "Play_gravekeeper_attack2_shoot_singleChain",
            "Play_gravekeeper_attack1_close", "Play_gravekeeper_attack2_shoot", "Play_scav_attack2_chargeup", "Play_vagrant_attack2_charge", "Play_wisp_attack_fire",
            "Play_lunar_wisp_attack2_explode", "Play_hermitCrab_attack_explo", "Play_beetle_guard_attack1", "Play_bellBody_attackLand", "Play_lunar_golem_attack1_explode", "Play_minorConstruct_attack_bodyClose",
            "Play_roboBall_attack1_explode", "Play_flyingVermin_attack1_explo", "Play_clayGrenadier_attack2_explode", "Play_beetle_guard_attack2_initial", "Play_vagrant_attack1_pop",
            "Play_beetle_worker_attack", "Play_wisp_attack_chargeup", "Play_gup_attack1_charge", "Play_bison_headbutt_attack_swing", "Play_lemurian_bite_attack",
            "Play_greater_wisp_attack", "Play_gup_attack1_shoot", "Play_scav_attack1_chargeup", "Play_nullifier_attack1_summon", "Play_clayGrenadier_attack1_launch",
            "Play_imp_overlord_attack1_pop", "Play_imp_overlord_attack1_throw", "Play_bellBody_attackShoot", "Play_grandParent_attack2_spawn", "Play_nullifier_attack1_explode",
            "Play_lunar_wisp_attack2_launch", "Play_hermitCrab_attack", "Play_clayGrenadier_attack2_throw", "Play_minorConstruct_attack_bodyOpen", "Play_parent_attack1_slam",
            "Play_imp_overlord_attack2_smash", "Play_clayBruiser_attack2_shoot", "Play_clayBruiser_attack1_shoot_flyby", "Play_imp_attack", "Play_scav_attack1_explode",
            "Play_scav_attack2_explode", "Play_acid_larva_attack1_explo", "Play_vulture_attack1_shoot", "Play_beetle_queen_attack1", "Play_acid_larva_attack1_start",
            "Play_beetle_queen_attack2_impact", "Play_bison_charge_attack_start", "Play_roboBall_attack1_shoot", "Play_roboBall_attack3_gravityBump_charge",
            "Play_imp_overlord_attack1_land", "Play_lunar_golem_attack1_launch", "Play_elite_antiHeal_turret_shot", "Play_voidJailer_m1_shoot", "Play_clayboss_m2_shoot",
            "Play_titanboss_shift_shoot", "Play_voidJailer_m2_shoot", "Play_voidRaid_m1_shoot", "Play_minimushroom_spore_shoot", "Play_titanboss_R_laser_preshoot",
            "Play_lemurianBruiser_m1_shoot", "Play_voidDevastator_m1_shoot", "Play_lemurianBruiser_m2_shoot", "Play_voidRaid_snipe_shoot_final", "Play_voidBarnacle_m1_shoot",
            "Play_moonBrother_phaseJump_jumpAway", "Play_moonBrother_phaseJump_land_impact",

            "Play_UI_item_spawn_tier1", "Play_UI_item_land_tier1", "Play_UI_item_spawn_tier2", "Play_UI_item_land_tier2", "Play_UI_item_spawn_tier3", "Play_UI_item_land_tier3",
            "Play_UI_conversion_item", "Play_UI_conversion_void", "Play_UI_achievementUnlock_enhanced", "Play_ui_teleporter_activate", "Play_UI_barrel_open",
            "Play_UI_takeDamage", "Play_UI_cooldownRefresh", "Play_UI_coin", "Play_UI_item_pickup", "Play_UI_menuHover", "Play_UI_skill_unlock", "Play_UI_monsterLogDrop",
            "Play_UI_levelUp_player", "Play_UI_equipment_activate", "Play_UI_achievementUnlock", "Play_ui_obj_voidCradle_open", "Play_UI_shrineActivate", "Play_ui_obj_lunarPool_activate",
            "Play_UI_charTeleport", "Play_UI_teleport_off_map", "Play_UI_chest_unlock", "Play_UI_monsterLogLand", "Play_UI_hit", "Play_UI_item_land_command", "Play_UI_arenaMode_voidCollapse_select",
            "Play_ui_player_death", "Play_UI_crit", "Play_UI_obj_casinoChest_expire", "Play_ui_lunar_coin_drop", "Play_UI_chatMessage", "Play_UI_levelUp_enemy", "Play_UI_insufficient_funds",
            "Play_UI_xp_gain", "Play_wCrit", /* <3 */

            "Play_wMushroom", /* <3 */ "Play_item_proc_vagrantNova_charge", "Play_item_proc_slug_emerge", "Play_item_proc_slug_hide", "Play_item_proc_TPhealingNova",
            "Play_item_proc_squidTurret_spawn", "Play_item_proc_iceRingSpear", "Play_item_proc_TPhealingNova_hitPlayer", "Play_item_proc_feather", "Play_item_proc_bear",
            "Play_item_proc_warhorn", "Play_item_proc_phasing", "Play_item_proc_moveSpeedOnKill", "Play_item_proc_clover", "Play_item_proc_roseBuckler", "Play_item_proc_equipMag",
            "Play_item_use_gainArmor", "Play_item_void_bear", "Play_item_proc_guillotine", "Play_item_proc_personal_shield_recharge", "Play_item_use_tonic_debuff",
            "Play_item_proc_whip", "Play_Item_proc_medkit", "Play_item_use_meteor_impact", "Play_item_use_passive_healing", "Play_item_proc_crit_heal",
            "Play_item_proc_goldOnHurt", "Play_item_use_lighningArm", "Play_item_proc_mushroom_start", "Play_item_void_bleedOnHit_buildup", "Play_item_proc_crit_attack_speed3",
            "Play_item_use_recycler", "Play_item_proc_moneyOnKill_loot",
        };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
            On.RoR2.DirectorCore.TrySpawnObject += DirectorCore_TrySpawnObject;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.UI.CombatHealthBarViewer.Start += CombatHealthBarViewer_Start;
            On.RoR2.UI.HealthBar.Start += HealthBar_Start;
        }

        private void HealthBar_Start(On.RoR2.UI.HealthBar.orig_Start orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            self.updateDelay = 4f;
        }

        private void CombatHealthBarViewer_Start(On.RoR2.UI.CombatHealthBarViewer.orig_Start orig, RoR2.UI.CombatHealthBarViewer self)
        {
            orig(self);
            if (ArtifactEnabled)
            {
                self.healthBarDuration = -1;
            }
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (!ArtifactEnabled)
            {
                return;
            }

            if (!body.isPlayerControlled)
            {
                return;
            }

            if (body.GetComponent<RandomSoundController>() != null)
            {
                return;
            }

            var monsterTeam = TeamCatalog.GetTeamDef(TeamIndex.Monster);
            monsterTeam.softCharacterLimit = 100;
            var voidTeam = TeamCatalog.GetTeamDef(TeamIndex.Void);
            voidTeam.softCharacterLimit = 100;
            var lunarTeam = TeamCatalog.GetTeamDef(TeamIndex.Lunar);
            lunarTeam.softCharacterLimit = 100;
            var neutralTeam = TeamCatalog.GetTeamDef(TeamIndex.Neutral);
            neutralTeam.softCharacterLimit = 100;

            body.gameObject.AddComponent<RandomSoundController>();
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            DisableChimeraDamage(damageInfo);

            orig(self, damageInfo);
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            DisableChimeraDamage(damageInfo);

            orig(self, damageInfo, victim);
        }

        private void DisableChimeraDamage(DamageInfo damageInfo)
        {
            if (ArtifactEnabled)
            {
                var attacker = damageInfo.attacker;
                if (attacker)
                {
                    var attackerBody = attacker.GetComponent<CharacterBody>();
                    if (attackerBody)
                    {
                        if (attackerBody.GetComponent<ChimeraIdentifier>())
                        {
                            damageInfo.damage = 0;
                            damageInfo.damage *= 0;
                            damageInfo.force = Vector3.zero;
                            damageInfo.dotIndex = DotController.DotIndex.None;
                            damageInfo.procCoefficient = 0;
                            damageInfo.crit = false;
                        }
                    }
                }
            }
        }

        private GameObject DirectorCore_TrySpawnObject(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            if (ArtifactEnabled)
            {
                directorSpawnRequest.onSpawnedServer += OnMasterSpawned;
            }

            return orig(self, directorSpawnRequest);
        }

        private void OnMasterSpawned(SpawnCard.SpawnResult spawnResult)
        {
            if (Run.instance && Run.instance.spawnRng.RangeInt(0, 100) > 50)
            {
                return;
            }

            var spawnedInstance = spawnResult.spawnedInstance;
            if (!spawnedInstance)
            {
                return;
            }

            var master = spawnedInstance.GetComponent<CharacterMaster>();
            if (master)
            {
                var body = master.GetBody();
                if (body)
                {
                    if (body.isBoss)
                    {
                        // makes tp bosses not spawn?????????
                        return;
                    }

                    master.ToggleGod();

                    body.AddComponent<ChimeraIdentifier>();

                    var healthComponent = body.healthComponent;
                    if (healthComponent)
                    {
                        healthComponent.globalDeathEventChanceCoefficient = 0;
                    }

                    if (body.TryGetComponent<DeathRewards>(out var deathRewards))
                    {
                        deathRewards.spawnValue = 0;
                        deathRewards.expReward = 0U;
                        // breaks deathRewards for everyone???????????
                        deathRewards.logUnlockableDef = null;
                        deathRewards.bossDropTable = null;
                    }
                }

                var inventory = master.inventory;
                if (inventory)
                {
                    inventory.GiveItem(RoR2Content.Items.HealthDecay, 20);
                }
            }
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            orig(self);
            if (ArtifactEnabled)
            {
                self.creditMultiplier += 2;
                self.maxRerollSpawnInterval *= 0.5f;
                self.minRerollSpawnInterval *= 0.5f;
            }
        }
    }

    public class ChimeraIdentifier : MonoBehaviour
    {
    }

    public class RandomSoundController : MonoBehaviour
    {
        public float timeBetweenSounds = 15f;
        public float maxVariation = 7f;
        public float timer;
        public float rngTimer;
        public float rngRollInterval = 2f;

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            rngTimer += Time.fixedDeltaTime;

            if (rngTimer >= rngRollInterval)
            {
                maxVariation = Run.instance.spawnRng.RangeFloat(-5f, 5f);
                rngTimer = 0f;
            }

            if (timer >= timeBetweenSounds + maxVariation)
            {
                if (Run.instance)
                {
                    var randomSound = ArtifactOfChimera.randomSounds[Run.instance.spawnRng.RangeInt(0, ArtifactOfChimera.randomSounds.Count)];
                    Util.PlaySound(randomSound, gameObject);
                }

                timer = 0f;
            }
        }
    }
}