# KingdomTemplate
A template for adding new Kingdoms and Factions to Mount &amp; Blade: Bannerlord

This repository contains an assortment of configuration files(Build/KingdomTemplate/ModuleData) that will populate the game with a new Culture, two new Kingdoms, four sub-Clans, 11 leaders/heroes, and two party templates.

Relevant Files:
- Mod Configuration: config.xml
- Culture: spcultures.xml
- Kingdom: spkingdoms.xml
- Clans: spclans.xml
- Lords: lords.xml
- Heroes: heroes.xml
- Party Template: party_templates.xml
- Custom Spawn Locations(one each for the new Kingdoms): locations_headhunter.xml & locations_vaal.xml
- Meeting location: spmeetings.xml

These files(and dll) are defined in the SubModule.xml, load order is important (modify at your own risk).

The codebase is pretty straight forward.

**Main:**

- `OnBeforeInitialModuleScreenSetAsRoot`: Initializes the Mod Configuration, and based on that initializes the custom spawn locations for each of the Custom Spawn Location files
- `OnGameStart`: Ensures the player has started a Campaign, and then adds a custom `KingdomTemplateBehavior` to the CampaignGameStarter.
- `SetupHeadhunterParties`: Retrieves all spawned parties within the current Campaign, move the new Heroes/Leaders to the positions specified in Custom Spawn Location(also applies any location offset, also specified in that file, SHOULD NOT BE EMPTY), and assigns two fiefs, one to each new Kingdom leader.

**KingdomTemplateBehavior:**

This class allows us to hook into some of the game's more closed-off lifecycle hooks.  Specifically, the `OnQuestStartedEvent`.

- `RegisterEvents`:  Adds a listener for `CampaignEvents.OnQuestStartedEvent`.  When the event is received, it will call the `KingdomBehvaiorTemplate`'s private `OnQuestStarted` method.
- `OnQuestStarted`:  Ensures the Campaign is valid, a new Campaign, and that the Quest "rebuild_player_clan_storymode_quest" is the Quest that's starting.  It then calls the static method `SetupHeadhunterParties` in the `Main` class and passes the current Campaign's MobileParties list.

**Config.xml**

- This is where you can define the XML tags to look for when loading your own Kingdom.
- KingdomId is the ID of the Kingdom defined in the spkingdoms.xml
- LocationsFileName is the name of the locations file, eg locations_headhunters.  Do not add the file extension, eg locations_headhunters.xml
- KingdomLordCollection is the tag for the Lords collection for the Kingdom, should match the main tag in the lords.xml file
- KingdomLord is the tag of the subobject entries for KingdomLordCollection
- KingdomLeaderId is the ID, defined in the lords.xml, for the Lord that rules the Kingdom

There shouldn't be anymore hardcoded entries, everythings that's relevant is contained in the config.xml.  I added a second, much smaller Kingdom, this could be easily duplicated to add your own.
