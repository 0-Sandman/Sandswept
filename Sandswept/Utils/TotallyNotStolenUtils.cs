using RoR2;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace Sandswept.Utils
{
    public static class TotallyNotStolenUtils
    {
        public enum ItemIconBackgroundType
        {
            Tier1,
            Tier2,
            Tier3,
            Boss,
            Equipment,
            Lunar,
            Survivor
        }

        private static Dictionary<ItemTier, ItemIconBackgroundType> itemTierToIconBackgroundType = new Dictionary<ItemTier, ItemIconBackgroundType>()
        {
            { ItemTier.Tier1, ItemIconBackgroundType.Tier1 },
            { ItemTier.Tier2, ItemIconBackgroundType.Tier2 },
            { ItemTier.Tier3, ItemIconBackgroundType.Tier3 },
            { ItemTier.Boss, ItemIconBackgroundType.Boss },
            { ItemTier.Lunar, ItemIconBackgroundType.Lunar }
        };

        public static ItemIconBackgroundType GetItemIconBackgroundTypeFromTier(ItemTier tier)
        {
            if (itemTierToIconBackgroundType.ContainsKey(tier)) return itemTierToIconBackgroundType[tier];
            return ItemIconBackgroundType.Tier1;
        }

        public static Sprite AddItemIconBackgroundToSprite(Sprite originalSprite, ItemIconBackgroundType bgType)
        {
            Texture2D loadedOriginalTexture = originalSprite.texture;

            Texture2D originalTexture = new Texture2D(loadedOriginalTexture.width, loadedOriginalTexture.height, TextureFormat.ARGB32, false);
            Graphics.ConvertTexture(loadedOriginalTexture, originalTexture);
            RenderTexture renderTexture = RenderTexture.GetTemporary(originalTexture.width, originalTexture.height, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            RenderTexture.active = renderTexture;
            Graphics.Blit(originalTexture, renderTexture);
            originalTexture.ReadPixels(new Rect(0, 0, originalTexture.width, originalTexture.height), 0, 0);
            originalTexture.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            Sprite loadedBackground = null;
            switch (bgType)
            {
                case ItemIconBackgroundType.Tier1:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier1BGIcon.png").WaitForCompletion();
                    break;

                case ItemIconBackgroundType.Tier2:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier2BGIcon.png").WaitForCompletion();
                    break;

                case ItemIconBackgroundType.Tier3:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier3BGIcon.png").WaitForCompletion();
                    break;

                case ItemIconBackgroundType.Boss:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBossBGIcon.png").WaitForCompletion();
                    break;

                case ItemIconBackgroundType.Equipment:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texEquipmentBGIcon.png").WaitForCompletion();
                    break;

                case ItemIconBackgroundType.Lunar:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texLunarBGIcon.png").WaitForCompletion();
                    break;

                case ItemIconBackgroundType.Survivor:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texSurvivorBGIcon.png").WaitForCompletion();
                    break;

                default:
                    loadedBackground = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier1BGIcon.png").WaitForCompletion();
                    break;
            }

            Texture2D backgroundTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.ARGB32, false);
            Graphics.ConvertTexture(loadedBackground.texture, backgroundTexture);
            renderTexture = RenderTexture.GetTemporary(backgroundTexture.width, backgroundTexture.height, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            RenderTexture.active = renderTexture;
            Graphics.Blit(backgroundTexture, renderTexture);
            backgroundTexture.ReadPixels(new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), 0, 0);
            backgroundTexture.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, false);
            newTexture.wrapMode = originalTexture.wrapMode;
            newTexture.filterMode = originalTexture.filterMode;
            for (var x = 0; x < newTexture.width; x++)
                for (var y = 0; y < newTexture.height; y++)
                {
                    Color backgroundPixel = backgroundTexture.GetPixel(x, y);
                    Color originalPixel = originalTexture.GetPixel(x, y);
                    newTexture.SetPixel(x, y, new Color(
                        backgroundPixel.r * (1 - originalPixel.a) + originalPixel.r * originalPixel.a,
                        backgroundPixel.g * (1 - originalPixel.a) + originalPixel.g * originalPixel.a,
                        backgroundPixel.b * (1 - originalPixel.a) + originalPixel.b * originalPixel.a,
                        Mathf.Clamp01(backgroundPixel.a + originalPixel.a)
                    ));
                }
            newTexture.Apply();

            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f), 25f, 1u, SpriteMeshType.Tight);
            return newSprite;
        }

        public static Sprite CreateItemIconWithBackgroundFromItem(ItemDef itemDef)
        {
            if (itemDef && itemDef.pickupIconSprite)
            {
                return AddItemIconBackgroundToSprite(itemDef.pickupIconSprite, GetItemIconBackgroundTypeFromTier(itemDef.tier));
            }
            return null;
        }

        public static Sprite CreateItemIconWithBackgroundFromEquipment(EquipmentDef equipmentDef)
        {
            if (equipmentDef && equipmentDef.pickupIconSprite)
            {
                return AddItemIconBackgroundToSprite(equipmentDef.pickupIconSprite, ItemIconBackgroundType.Equipment);
            }
            return null;
        }

        public static Sprite CreateItemIconWithBackgroundFromLunarEquipment(EquipmentDef equipmentDef)
        {
            if (equipmentDef && equipmentDef.pickupIconSprite)
            {
                return AddItemIconBackgroundToSprite(equipmentDef.pickupIconSprite, ItemIconBackgroundType.Lunar);
            }
            return null;
        }
    }
}