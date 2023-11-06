namespace Sandswept.Skills.Ranger.VFX {
    public static class HeatSprayVFX {
        public static GameObject SprayTracer;

        [AutoRun]
        public static void Init() {
            SprayTracer = PrefabAPI.InstantiateClone(Assets.GameObject.TracerCommandoShotgun, "HeatSprayTracer");

            LineRenderer renderer = SprayTracer.GetComponent<LineRenderer>();
            renderer.startColor = new Color32(255, 72, 0, 255);
            renderer.endColor = new Color32(255, 125, 0, 255);
            renderer.material = Assets.Material.matWispFire;

            Tracer tracer = SprayTracer.GetComponent<Tracer>();
            tracer.speed = 120f;
            tracer.beamDensity = 5f;

            ContentAddition.AddEffect(SprayTracer);
        }
    }
}