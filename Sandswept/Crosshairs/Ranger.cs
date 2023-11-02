using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace Sandswept.Crosshairs
{
    public static class Ranger
    {
        public static GameObject projectileCrosshairPrefab;
        public static GameObject hitscanCrosshairPrefab;

        public static void Init()
        {
            projectileCrosshairPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.TiltedBracketCrosshair, "Ranger Projectile Crosshair", false);

            var rectTransform = projectileCrosshairPrefab.GetComponent<RectTransform>();
            rectTransform.position = Vector3.zero;
            rectTransform.localScale = Vector3.one * 1.5f;

            /*
            var rawImage = projectileCrosshairPrefab.GetComponent<RawImage>();
            rawImage.enabled = false;
            */

            var holder = projectileCrosshairPrefab.transform.GetChild(0);
            holder.gameObject.SetActive(false);

            var center = projectileCrosshairPrefab.transform.GetChild(1).GetComponent<Image>();
            center.sprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texProjectileCrosshair.png");
            center.color = Color.white;

            var centerRect = center.GetComponent<RectTransform>();
            centerRect.localPosition = Vector3.zero;
            centerRect.localScale = Vector3.one * 0.5f;

            hitscanCrosshairPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.StandardCrosshairSmall, "Ranger Hitscan Crosshair", false);
            var upArrow = hitscanCrosshairPrefab.transform.GetChild(2);
            upArrow.gameObject.SetActive(false);

            var downArrow = hitscanCrosshairPrefab.transform.GetChild(3);
            downArrow.gameObject.SetActive(false);

            var leftArrow = hitscanCrosshairPrefab.transform.GetChild(0).GetComponent<RectTransform>();
            leftArrow.position = new Vector3(80, 0, 0);
            leftArrow.localScale = Vector3.one * 1.2f;

            var rightArrow = hitscanCrosshairPrefab.transform.GetChild(1).GetComponent<RectTransform>();
            rightArrow.position = new Vector3(-80, 0, 0);
            rightArrow.localScale = Vector3.one * 1.2f;
        }
    }
}