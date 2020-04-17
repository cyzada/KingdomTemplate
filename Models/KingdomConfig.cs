namespace KingdomTemplate.Models
{
    public class KingdomConfig
    {
        public string kingdomId;
        public string locationFilename;
        public string kingdomLordCollectionTag;
        public string kingdomLordTag;
        public string kingdomLeaderId;

        public KingdomConfig(string id, string filename, string lordCollectionTag, string lordTag, string leaderId)
        {
            kingdomId = id;
            locationFilename = filename;
            kingdomLordCollectionTag = lordCollectionTag;
            kingdomLordTag = lordTag;
            kingdomLeaderId = leaderId;
        }
    }
}