using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace KingdomTemplate
{
    public class Main : MBSubModuleBase
    {
        private static Dictionary<string, string> hunterLocations = new Dictionary<string, string>();
        
        private static string xDocPath = BasePath.Name + "Modules\\KingdomTemplate\\ModuleData\\headhunter_locations.xml";
        private KingdomTemplateBehavior _kingdomTemplateBehavior;

        protected override void OnBeforeInitialModuleScreenSetAsRoot() // Main Menu.
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            // Clear hunterLocations, just in case, and then initialize it
            hunterLocations.Clear();
            Initialize();
        }
        
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (!(game.GameType is Campaign))
                return;
            // Here we create a new custom campaign behavior, KingdomTemplateBehavior, and add it to the CampaignGameStarter.
            CampaignGameStarter campaignGameStarter = (CampaignGameStarter) gameStarterObject;
            _kingdomTemplateBehavior = new KingdomTemplateBehavior(new DataSynchronizer());
            campaignGameStarter.AddBehavior(_kingdomTemplateBehavior);
        }

        private void Initialize()
        {
            // Load the xml information for where to spawn the new faction members
            if (File.Exists(xDocPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(xDocPath);
                    if (doc.Root != null)
                    {
                        IEnumerable<XElement> headhunters = doc.Root.Elements("Headhunters").Elements("Headhunter");
                        IEnumerable<XElement> elements = headhunters.ToList();

                        if (elements.Any())
                        {
                            IEnumerator<XElement> elementrator = elements.GetEnumerator();
                            while (elementrator.MoveNext())
                            {
                                XElement current = elementrator.Current;
                                if (current?.Element("Name") != null &&
                                    current.Element("Settlement") != null)
                                {
                                    string heroName = current.Element("Name")?.Value;
                                    string heroLocation = current.Element("Settlement")?.Value;
                                    hunterLocations.Add(heroName, heroLocation);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Failed to load Hunter Location Data: " + e.Message, Color.FromUint(4278255360U)));
                }
            }
            InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Initialized.", Color.FromUint(4282569842U)));
        }
        
        public static void SetupHeadhunterParties(MBReadOnlyList<MobileParty> mobileParties)
        {
            if(mobileParties == null)
            {
                return;
            }
            
            Dictionary<string, MobileParty> existingParties = new Dictionary<string, MobileParty>();

            List<MobileParty> tempList = mobileParties.ToList();
            tempList.ForEach(p =>
            {
                // Campaign.Current.MobileParties can have some invalid entries, make sure only valid MobileParties get added for processing
                if (p.Leader?.Name != null && !existingParties.ContainsKey(p.Leader.Name.ToString()))
                {
                    existingParties.Add(p.Leader.Name.ToString(), p);
                }
            });

            if(existingParties.Count > 0)
            {
                if (!hunterLocations.IsEmpty() && hunterLocations.Count > 0)
                {
                    string[] names = new string[hunterLocations.Count];
                    hunterLocations.Keys.CopyTo(names, 0);
                    List<string> headhuntersToAdd = names.ToList();

                    headhuntersToAdd.ForEach(hunter =>
                    {
                        if (existingParties.ContainsKey(hunter))
                        {
                            MobileParty p = existingParties[hunter];
                            if (p?.Leader?.Name != null)
                            {
                                Settlement targetSettlement = Settlement.Find(hunterLocations[hunter]);
                                float offset = 5.0f;

                                if (targetSettlement.StringId != null && targetSettlement.StringId.Equals("town_A3"))
                                {
                                    // This is done for a specific city so the party doesn't spawn somewhere they can't navigate out of
                                    p.Position2D = new Vec2(targetSettlement.Position2D.X - offset, targetSettlement.Position2D.Y + offset);
                                    p.SetMovePatrolAroundSettlement(targetSettlement);
                                }
                                else
                                {
                                    // Here we assign ownership of the targetSettlement to "Suuhl", in this case.
                                    // If I were to put more time into this, I'd get the Kingdom from the mod and assign that settlement to it's leader.
                                    if (p.Leader.Name.Equals("Suuhl"))
                                    {
                                        TaleWorlds.CampaignSystem.Actions.ChangeOwnerOfSettlementAction.ApplyByDefault(p.LeaderHero, targetSettlement);
                                    }
                                    // Move the party's position
                                    p.Position2D = new Vec2(targetSettlement.Position2D.X, targetSettlement.Position2D.Y - offset);
                                    // Make them patrol around their location
                                    p.SetMovePatrolAroundSettlement(targetSettlement);
                                }
                            }
                        }
                    });
                    InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Processed Headhunter parties. " + hunterLocations.Count + " parties found.", Color.FromUint(4282569842U)));
                }
            }
        }
    }
}