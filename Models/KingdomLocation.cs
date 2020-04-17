namespace KingdomTemplate.Models
{
    public class KingdomLocation
    {
        public string SettlementName;
        public int XOffset;
        public int YOffset;

        public KingdomLocation(string settlementName, int xOffset, int yOffset)
        {
            SettlementName = settlementName;
            XOffset = xOffset;
            YOffset = yOffset;
        }
    }
}