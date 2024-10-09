namespace Sandswept.Survivors.Ranger.VFX
{
    public static class HeatSinkVFX
    {
        public static Material explodeMat1Default;
        public static Material explodeMat2Default;

        public static Material explodeMat1Major;
        public static Material explodeMat2Major;

        public static Material explodeMat1Renegade;
        public static Material explodeMat2Renegade;

        public static Material explodeMat1MileZero;
        public static Material explodeMat2MileZero;

        public static Material explodeMat1Sandswept;
        public static Material explodeMat2Sandswept;

        public static GameObject explosion1Default;
        public static GameObject explosion2Default;

        public static GameObject explosion1Major;
        public static GameObject explosion2Major;

        public static GameObject explosion1Renegade;
        public static GameObject explosion2Renegade;

        public static GameObject explosion1MileZero;
        public static GameObject explosion2MileZero;

        public static GameObject explosion1Sandswept;
        public static GameObject explosion2Sandswept;

        public static void Init()
        {
            explodeMat1Default = CreateMat1Recolor(new Color32(191, 49, 3, 255));

            explodeMat2Default = CreateMat2Recolor(new Color32(148, 46, 0, 255));

            //

            explodeMat1Major = CreateMat1Recolor(new Color32(56, 128, 204, 255));

            explodeMat2Major = CreateMat2Recolor(new Color32(61, 121, 170, 255));

            //

            explodeMat1Renegade = CreateMat1Recolor(new Color32(132, 56, 204, 255));

            explodeMat2Renegade = CreateMat2Recolor(new Color32(112, 54, 170, 255));

            //

            explodeMat1MileZero = CreateMat1Recolor(Color.black);

            explodeMat2MileZero = CreateMat2Recolor(Color.black);

            //

            explodeMat1Sandswept = CreateMat1Recolor(new Color32(150, 150, 150, 255));
            explodeMat2Sandswept = CreateMat2Recolor(new Color32(249, 197, 143, 255));

            //

            explosion1Default = CreateExplosion1Recolor("Default", new Color32(224, 164, 52, 255), new Color32(206, 114, 15, 255), new Color32(255, 247, 158, 255), new Color32(233, 92, 0, 255), new Color32(146, 51, 0, 255), new Color32(216, 123, 40, 255));
            explosion1Major = CreateExplosion1Recolor("Major", new Color32(52, 152, 224, 255), new Color32(15, 152, 206, 255), new Color32(158, 189, 255, 255), new Color32(0, 195, 233, 255), new Color32(0, 129, 146, 255), new Color32(40, 174, 216, 255));
            explosion1Renegade = CreateExplosion1Recolor("Renegade", new Color32(164, 52, 224, 255), new Color32(114, 15, 206, 255), new Color32(247, 158, 255, 255), new Color32(92, 0, 233, 255), new Color32(51, 0, 146, 255), new Color32(123, 40, 216, 255));
            explosion1MileZero = CreateExplosion1Recolor("Mile Zero", new Color32(224, 58, 52, 255), new Color32(255, 0, 0, 255), new Color32(0, 0, 0, 255), new Color32(233, 0, 5, 255), new Color32(146, 0, 5, 255), new Color32(177, 33, 37, 255));
            explosion1Sandswept = CreateExplosion1Recolor("Sandswept", new Color32(249, 197, 143, 255), new Color32(214, 159, 79, 255), new Color32(150, 150, 150, 255), new Color32(87, 87, 87, 255), new Color32(150, 150, 150, 255), new Color32(249, 197, 143, 255));

            explosion2Default = CreateExplosion2Recolor("Default", new Color32(255, 221, 23, 255), new Color32(207, 153, 0, 255), new Color32(255, 20, 255, 255), 20f, 1.266667f);
            explosion2Major = CreateExplosion2Recolor("Major", new Color32(23, 173, 255, 255), new Color32(0, 157, 207, 255), new Color32(1, 0, 255, 255));
            explosion2Renegade = CreateExplosion2Recolor("Renegade", new Color32(147, 23, 255, 255), new Color32(87, 0, 207, 255), new Color32(157, 0, 214, 255));
            explosion2MileZero = CreateExplosion2Recolor("Mile Zero", new Color32(255, 23, 30, 255), new Color32(207, 0, 2, 255), new Color32(255, 0, 0, 255));
            explosion2Sandswept = CreateExplosion2Recolor("Sandswept", new Color32(150, 150, 150, 255), new Color32(87, 87, 87, 255), new Color32(214, 159, 79, 255));
        }

        public static GameObject CreateExplosion2Recolor(string name, Color32 yellowEquivalent, Color32 yellowEquivalent2, Color32 tintColor, float brightnessBoost = 4.87f, float alphaBoost = 6.6f, float alphaBias = 0f)
        {
            // acidGreenEquivalent = new Color32(114, 255, 0, 255);
            // yellowEquivalent = new Color32(255,221,23,255);
            // yellowEquivalent2 = new Color32(207,153,0,255);
            var trimmedName = name.Replace(" ", "");
            var explosion = PrefabAPI.InstantiateClone(Paths.GameObject.FireMeatBallExplosion, "Heat Sink Explosion 2 " + name, false);

            var trans = explosion.transform.GetChild(0);

            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                child.transform.localScale = Vector3.one * 3f;
            }

            var flameBurst = trans.GetChild(0).GetComponent<ParticleSystemRenderer>();
            var flameBurstSpeed = flameBurst.GetComponent<ParticleSystem>().main.startSpeed;
            flameBurstSpeed.constantMin = 3f;
            flameBurstSpeed.constantMax = 4f;

            var newMat = Object.Instantiate(Paths.Material.matMageFlamethrower);
            newMat.SetColor("_TintColor", Color.white);
            newMat.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("texRampHeatSink" + trimmedName + ".png"));
            newMat.SetFloat("_AlphaBias", 0.38f);
            newMat.SetFloat("_AlphaBoost", 0.5693459f);

            flameBurst.material = newMat;

            var sparks = trans.GetChild(1);
            var sparksPS = sparks.GetComponent<ParticleSystem>().main.startColor;
            sparksPS.color = yellowEquivalent;
            var sparksPSR = sparks.GetComponent<ParticleSystemRenderer>();

            var newMat2 = Object.Instantiate(Paths.Material.matTracerBright);
            newMat2.SetTexture("_RemapTex", Paths.Texture2D.texRampShield);
            newMat2.SetFloat("_Boost", brightnessBoost);
            newMat2.SetFloat("_AlphaBoost", alphaBoost);
            newMat2.SetFloat("_AlphaBias", alphaBias);
            newMat2.SetColor("_TintColor", tintColor);

            sparksPSR.material = newMat2;

            var fireTrailStreak = trans.GetChild(2).GetComponent<ParticleSystemRenderer>();

            fireTrailStreak.material = newMat;

            // var flash = trans.GetChild(3).GetComponent<ParticleSystem>().main.startColor;
            // flash.color = yellowEquivalent2;
            var flash = trans.GetChild(3);
            flash.gameObject.SetActive(false);

            ContentAddition.AddEffect(explosion);

            return explosion;
        }

        public static GameObject CreateExplosion1Recolor(string name, Color32 yellowEquivalent, Color32 orangeEquivalent, Color32 lightYellowEquivalent, Color32 orangeEquivalent2, Color32 orangeEquivalent3, Color32 orangeEquivalent4)
        {
            var trimmedName = name.Replace(" ", "");
            // yellowEquivalent = new Color32(224, 164, 52, 255);
            // orangeEquivalent = new Color32(206, 114, 15, 255);
            // lightYellowEquivalent = new Color32(255, 247, 158, 255);
            // orangeEquivalent2 = new Color32(233, 92, 0, 255);
            // orangeEquivalent3 = new Color32(146,51,0,255);
            // orangeEquivalent4 = new Color32(216, 123, 40, 255);
            var explosion = PrefabAPI.InstantiateClone(Paths.GameObject.IgniteDirectionalExplosionVFX, "Heat Sink Explosion 1 " + name, false);

            var explosionPSR = explosion.GetComponent<ParticleSystemRenderer>();

            var newMat2 = Object.Instantiate(Paths.Material.matCryoCanisterSphere);
            newMat2.SetColor("_TintColor", orangeEquivalent);
            newMat2.SetFloat("_InvFade", 0.1851155f);
            newMat2.SetFloat("_SoftPower", 0.480717f);
            newMat2.SetFloat("_Boost", 0.2f);
            newMat2.SetFloat("_RimStrength", 2.063599f);
            newMat2.SetFloat("_IntersectionStrength", 0.56f);

            explosionPSR.material = newMat2;

            var trans = explosion.transform;

            var startColor = explosion.GetComponent<ParticleSystem>().main.startColor;
            startColor.color = yellowEquivalent;

            var omniDirectionals = trans.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var newMat = Object.Instantiate(Paths.Material.matOmniHitspark3Gasoline);
            newMat.SetTexture("_RemapTex", Main.hifuSandswept.LoadAsset<Texture2D>("texRampHeatSink" + trimmedName + ".png"));
            newMat.SetFloat("_AlphaBoost", 0.9377567f);
            newMat.SetFloat("_AlphaBias", 0.2189533f);
            newMat.SetFloat("_Boost", 4f);

            omniDirectionals.material = newMat;
            omniDirectionals.gameObject.SetActive(false);

            var light = trans.GetChild(1).GetComponent<Light>();
            light.color = orangeEquivalent;
            light.intensity = 50f;
            light.range = 16f;

            var guh = trans.GetChild(2);
            guh.transform.localScale = Vector3.one * 7f;
            var flames = guh.GetComponent<ParticleSystem>().colorOverLifetime;

            var gradient = new Gradient();

            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(lightYellowEquivalent, 0f);
            colors[1] = new GradientColorKey(orangeEquivalent2, 0.424f);

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(0f, 1f);

            gradient.SetKeys(colors, alphas);

            flames.color = gradient;

            var flash = trans.GetChild(3).GetComponent<ParticleSystem>().colorOverLifetime;

            var gradient2 = new Gradient();

            var colors2 = new GradientColorKey[2];
            colors2[0] = new GradientColorKey(orangeEquivalent3, 0f);
            colors2[1] = new GradientColorKey(orangeEquivalent4, 1f);

            var alphas2 = new GradientAlphaKey[2];
            alphas2[0] = new GradientAlphaKey(0.27058823529f, 0f);
            alphas2[1] = new GradientAlphaKey(0f, 1f);

            gradient2.SetKeys(colors, alphas);

            flash.color = gradient2;

            var flash2 = trans.GetChild(6).GetComponent<ParticleSystemRenderer>();

            flash2.material = newMat;

            ContentAddition.AddEffect(explosion);

            return explosion;
        }

        public static Material CreateMat1Recolor(Color32 tintColor)
        {
            var mat = Object.Instantiate(Paths.Material.matHuntressFlashBright);

            mat.SetColor("_TintColor", tintColor);

            return mat;
        }

        public static Material CreateMat2Recolor(Color32 tintColor)
        {
            var mat = Object.Instantiate(Paths.Material.matHuntressFlashExpanded);

            mat.SetColor("_TintColor", tintColor);

            return mat;
        }
    }
}