using RoR2.Achievements;

namespace Sandswept.Unlocks
{
    [RegisterAchievement("ITEM_SANDSWEPT_HALLOWED_ICHOR", "ITEM_SANDSWEPT_HALLOWED_ICHOR", null, 5, null)]
    public class HallowedIchorUnlock : BaseAchievement
    {
        public const int requirement = 0;
        public int itemsPickedUpThisStage = 0;
        public PlayerCharacterMasterController currentMasterController;
        public Inventory currentInventory;

        public int OnGiveItem { get; private set; }

        public override void OnInstall()
        {
            base.OnInstall();
            // Main.ModLogger.LogError("ichor on install, subscrbigingfdnb");
            localUser.onMasterChanged += OnMasterChanged;
            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterCharged;
        }

        public void OnMasterChanged()
        {
            // Main.ModLogger.LogError("ichor on master changed");
            SetMasterController(localUser.cachedMasterController);
        }

        public void SetMasterController(PlayerCharacterMasterController newMasterController)
        {
            // Main.ModLogger.LogError("ichor set master controller");
            if (currentMasterController != newMasterController)
            {
                // Main.ModLogger.LogError("current master controller does not equal new master controller (duh)");
                if (currentInventory != null)
                {
                    // Main.ModLogger.LogError("current inventory PRE is not null, unsubscribing");
                    currentInventory.onItemAddedClient -= OnItemAddedClient;
                    RoR2.Stage.onStageStartGlobal -= OnStageStart;
                }

                currentMasterController = newMasterController;
                currentInventory = currentMasterController?.master?.inventory;
                if (currentInventory != null)
                {
                    // Main.ModLogger.LogError("CURRENT INVENTORY POST IS NOT NULL , SUBSCRIBING ! ! ");
                    currentInventory.onItemAddedClient += OnItemAddedClient;
                    RoR2.Stage.onStageStartGlobal += OnStageStart;
                }
            }
        }

        public void OnStageStart(RoR2.Stage stage)
        {
            // Main.ModLogger.LogError("ichor on stage start called");
            itemsPickedUpThisStage = 0;
        }

        public void OnItemAddedClient(ItemIndex index)
        {
            var itemDef = ItemCatalog.GetItemDef(index);
            if (!itemDef.hidden && itemDef.tier != ItemTier.NoTier && itemDef.deprecatedTier != ItemTier.NoTier)
            {
                // Main.ModLogger.LogError("ichor incremented pickup count");
                itemsPickedUpThisStage++;
            }
        }

        public void OnTeleporterCharged(TeleporterInteraction teleporterInteraction)
        {
            // Main.ModLogger.LogError("ichor on teleporter charged");
            if (IsPrimordialTeleporter(teleporterInteraction) && itemsPickedUpThisStage <= requirement)
            {
                Grant();
            }
        }

        public bool IsPrimordialTeleporter(TeleporterInteraction teleporterInteraction)
        {
            return teleporterInteraction.beginContextString == "LUNAR_TELEPORTER_BEGIN_CONTEXT"; // wanted to add snowtime stages compat but unsure about the functionality of their teleporter, also no token changes suck here
        }

        public override void OnUninstall()
        {
            // Main.ModLogger.LogError("iwhore onuninstall, unsubscribing");
            localUser.onMasterChanged -= OnMasterChanged;
            TeleporterInteraction.onTeleporterChargedGlobal -= OnTeleporterCharged;
            base.OnUninstall();
        }
    }
}