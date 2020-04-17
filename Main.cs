using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using KingdomTemplate.Models;
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
        private static List<KingdomConfig> KingdomConfigs = new List<KingdomConfig>();
        private static string baseXDocPath = BasePath.Name + "Modules\\KingdomTemplate\\ModuleData\\";
        private static string configXDocPath = baseXDocPath + "config.xml";
        private KingdomTemplateBehavior _kingdomTemplateBehavior;

        protected override void OnBeforeInitialModuleScreenSetAsRoot() // Main Menu.
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            // Clear hunterLocations, just in case, and then initialize it
            // hunterLocations.Clear();
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
            KingdomConfigs.Clear();
            // Load mod's config information, this defines what to look for when loading the mods other xmls
            if (File.Exists(configXDocPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(configXDocPath);
                    if (doc.Root != null)
                    {
                        IEnumerable<XElement> kingdomConfigs = doc.Root.Elements();

                        if (kingdomConfigs.Any())
                        {
                            IEnumerator<XElement> elementrator = kingdomConfigs.GetEnumerator();
                            while (elementrator.MoveNext())
                            {
                                XElement current = elementrator.Current;
                                if (current?.Element("KingdomId") != null &&
                                    current.Element("LocationsFileName") != null &&
                                    current.Element("KingdomLordCollection") != null &&
                                    current.Element("KingdomLord") != null &&
                                    current.Element("kingdomLeaderId") != null)
                                {
                                    KingdomConfig kingdomConfig = new KingdomConfig(
                                        current.Element("KingdomId")?.Value, 
                                        current.Element("LocationsFileName")?.Value,
                                        current.Element("KingdomLordCollection")?.Value, 
                                        current.Element("KingdomLord")?.Value,
                                        current.Element("kingdomLeaderId")?.Value);
                                    KingdomConfigs.Add(kingdomConfig);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Config.xml Data: " + e.Message, Color.FromUint(4278255360U)));
                }
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Config.xml not found", Color.FromUint(4278255360U)));
            }

            if (KingdomConfigs.Any())
            {
                KingdomConfigs.ForEach(kC =>
                {
                    // if (File.Exists(locationsxDocPath))
                    // {
                    //     try
                    //     {
                    //         XDocument doc = XDocument.Load(locationsxDocPath);
                    //         if (doc.Root != null)
                    //         {
                    //             IEnumerable<XElement> headhunters = doc.Root.Elements("Headhunters").Elements("Headhunter");
                    //             IEnumerable<XElement> elements = headhunters.ToList();
                    //
                    //             if (elements.Any())
                    //             {
                    //                 IEnumerator<XElement> elementrator = elements.GetEnumerator();
                    //                 while (elementrator.MoveNext())
                    //                 {
                    //                     XElement current = elementrator.Current;
                    //                     if (current?.Element("Name") != null &&
                    //                         current.Element("Settlement") != null)
                    //                     {
                    //                         string heroName = current.Element("Name")?.Value;
                    //                         string heroLocation = current.Element("Settlement")?.Value;
                    //                         hunterLocations.Add(heroName, heroLocation);
                    //                     }
                    //                 }
                    //             }
                    //         }
                    //     }
                    //     catch (Exception e)
                    //     {
                    //         InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Failed to load Hunter Location Data: " + e.Message, Color.FromUint(4278255360U)));
                    //     }
                    // }
                    // InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Initialized.", Color.FromUint(4282569842U)));
                });
            }
        }
        
        public static void SetupKingdomParties(MBReadOnlyList<MobileParty> mobileParties)
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

            if(existingParties.Count > 0 && KingdomConfigs.Any())
            {
                KingdomConfigs.ForEach(kC =>
                {
                    Dictionary<string,string> lordLocations = LoadLocationsForKingdomConfiguration(kC);
                    if (!lordLocations.IsEmpty() && lordLocations.Count > 0)
                    {
                        string[] names = new string[lordLocations.Count];
                        lordLocations.Keys.CopyTo(names, 0);
                        List<string> lordsToAdd = names.ToList();

                        lordsToAdd.ForEach(lord =>
                        {
                            if (existingParties.ContainsKey(lord))
                            {
                                MobileParty p = existingParties[lord];
                                if (p?.Leader?.Name != null)
                                {
                                    Settlement targetSettlement = Settlement.Find(lordLocations[lord]);
                                    float offset = 5.0f;

                                    if (targetSettlement.StringId != null && targetSettlement.StringId.Equals("town_A3"))
                                    {
                                        // This is done for a specific city so the party doesn't spawn somewhere they can't navigate out of
                                        p.Position2D = new Vec2(targetSettlement.Position2D.X - offset, targetSettlement.Position2D.Y + offset);
                                        p.SetMovePatrolAroundSettlement(targetSettlement);
                                    }
                                    else
                                    {
                                        // Here we assign ownership of the targetSettlement to the KingdomLeaderId defined in config.xml
                                        if (p.Leader.StringId.Equals(kC.kingdomLeaderId))
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
                    }
                    InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Processed Lord parties.", Color.FromUint(4282569842U)));
                });
            }
        }

        private static Dictionary<string, string> LoadLocationsForKingdomConfiguration(KingdomConfig config)
        {
            string fileLocation = baseXDocPath + config.locationFilename + ".xml";
            Dictionary<string, string> lordLocations = new Dictionary<string, string>();
            if (File.Exists(fileLocation))
            {
                try
                {
                    XDocument doc = XDocument.Load(fileLocation);
                    if (doc.Root != null)
                    {
                        IEnumerable<XElement> lords = doc.Root.Elements(config.kingdomLordCollectionTag).Elements(config.kingdomLordTag);
                        IEnumerable<XElement> elements = lords.ToList();

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
                                    lordLocations.Add(heroName, heroLocation);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage("KingdomTemplate: Failed to load Lord Location Data: " + e.Message, Color.FromUint(4278255360U)));
                }
            }

            return lordLocations;
        }
    }
}