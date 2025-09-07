using System;

namespace Sandswept.Enemies
{
    public class SpawnAndDeath
    {
        public static GameObject deltaConstructSpawnAndDeathVFX;
        public static GameObject gammaConstructSpawnAndDeathVFX;
        public static GameObject thetaConstructSpawnAndDeathVFX;
        public static GameObject cannonballJellyfishSpawnVFX;

        public static void Init()
        {
            deltaConstructSpawnAndDeathVFX = CreateSpawnAndDeathRecolor("Delta", new Color32(255, 0, 32, 255));
            gammaConstructSpawnAndDeathVFX = CreateSpawnAndDeathRecolor("Gamma", new Color32(255, 66, 0, 255));
            thetaConstructSpawnAndDeathVFX = CreateSpawnAndDeathRecolor("Theta", new Color32(14, 255, 0, 255));

            cannonballJellyfishSpawnVFX = PrefabAPI.InstantiateClone(Paths.GameObject.ClayGrenadierBarrelExplosion, "Cannonball Jellyfish Spawn VFX", false);
            cannonballJellyfishSpawnVFX.transform.Find("Decal").gameObject.SetActive(false);

            cannonballJellyfishSpawnVFX.GetComponent<EffectComponent>().soundName = "";

            ContentAddition.AddEffect(cannonballJellyfishSpawnVFX);

            ContentAddition.AddEntityState(typeof(CannonballJellyfish.SpawnState), out _);

            ContentAddition.AddEntityState(typeof(DeltaConstruct.SpawnState), out _);
            ContentAddition.AddEntityState(typeof(DeltaConstruct.DeathState), out _);

            ContentAddition.AddEntityState(typeof(GammaConstruct.SpawnState), out _);
            ContentAddition.AddEntityState(typeof(GammaConstruct.DeathState), out _);

            ContentAddition.AddEntityState(typeof(ThetaConstruct.SpawnState), out _);
            ContentAddition.AddEntityState(typeof(ThetaConstruct.DeathState), out _);

        }

        public static GameObject CreateSpawnAndDeathRecolor(string name, Color32 color)
        {
            var prefab = PrefabAPI.InstantiateClone(Paths.GameObject.ExplosionMinorConstruct, name + " Construct Spawn and Death VFX", false);

            VFXUtils.RecolorMaterialsAndLights(prefab, color, color, true);

            ContentAddition.AddEffect(prefab);

            return prefab;
        }
    }
}