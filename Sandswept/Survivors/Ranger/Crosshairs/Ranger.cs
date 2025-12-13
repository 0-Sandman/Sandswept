namespace Sandswept.Survivors.Ranger.Crosshairs
{
    public static class Ranger
    {
        public static GameObject hitscanCrosshairPrefab;

        public static void Init()
        {
            
            hitscanCrosshairPrefab = Paths.GameObject.StandardCrosshairSmall.InstantiateClone("Ranger Hitscan Crosshair", false);
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