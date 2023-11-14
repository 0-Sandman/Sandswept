using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace Sandswept.Survivors.Ranger.Crosshairs
{
    public static class Ranger
    {
        public static GameObject projectileCrosshairPrefab;
        public static GameObject hitscanCrosshairPrefab;

        public static void Init()
        {
            projectileCrosshairPrefab = Assets.GameObject.TiltedBracketCrosshair.InstantiateClone("Ranger Projectile Crosshair", false);

            var rectTransform = projectileCrosshairPrefab.GetComponent<RectTransform>();
            rectTransform.position = Vector3.zero;
            rectTransform.localScale = Vector3.one;

            var rawImage = projectileCrosshairPrefab.GetComponent<RawImage>();
            rawImage.enabled = false;

            var holder = projectileCrosshairPrefab.transform.GetChild(0);
            holder.GetComponent<RectTransform>().eulerAngles = Vector3.zero;
            holder.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
            holder.localPosition = new Vector3(-8f, 0f, 0f);

            var sayGex = holder.GetChild(0);
            var gaySexRect = sayGex.GetComponent<RectTransform>();
            gaySexRect.localPosition = Vector3.zero;
            gaySexRect.position = Vector3.zero;
            gaySexRect.anchoredPosition = Vector3.zeroVector;
            gaySexRect.localScale = Vector3.one * 0.9f;

            var gaySexImage = sayGex.GetComponent<Image>();
            gaySexImage.sprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texProjectileCrosshair.png");

            holder.GetChild(1).gameObject.SetActive(false);
            holder.GetChild(2).gameObject.SetActive(false);
            holder.GetChild(3).gameObject.SetActive(false);

            var center = projectileCrosshairPrefab.transform.GetChild(1).GetComponent<Image>();
            center.sprite = Main.hifuSandswept.LoadAsset<Sprite>("Assets/Sandswept/texProjectileCrosshair3.png");
            center.color = Color.white;

            var centerRect = center.GetComponent<RectTransform>();
            centerRect.localPosition = new Vector3(0f, -32.5f, 0f);
            centerRect.localScale = Vector3.one * 0.75f;

            hitscanCrosshairPrefab = Assets.GameObject.StandardCrosshairSmall.InstantiateClone("Ranger Hitscan Crosshair", false);
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