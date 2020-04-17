using TaleWorlds.CampaignSystem;

namespace KingdomTemplate
{
    public class KingdomTemplateBehavior : CampaignBehaviorBase
    {
        private readonly DataSynchronizer _dataSynchroniser;

        public KingdomTemplateBehavior(DataSynchronizer dataSynchroniser)
        {
            _dataSynchroniser = dataSynchroniser;
        }

        public override void RegisterEvents()
        {
            // Creating a lister for the OnQuestStartedEvent during the Campaign
            CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener(this,OnQuestStarted);
        }

        private void OnQuestStarted(QuestBase obj)
        {
            // We only want to execute this mod when a new Campaign has been started and the "Rebuild Clan" quest is started, typically after completing the Tutorial
            if (Campaign.Current != null && Campaign.Current.CampaignGameLoadingType == Campaign.GameLoadingType.NewCampaign)
            {
                if (obj.StringId.Equals("rebuild_player_clan_storymode_quest"))
                {
                    Main.SetupKingdomParties(Campaign.Current.MobileParties);
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}