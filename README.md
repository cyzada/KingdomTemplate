# KingdomTemplate
A template for adding new Kingdoms and Factions to Mount &amp; Blade: Bannerlord

This repository contains an assortment of configuration files(Build/KingdomTemplate/ModuleData) that will populate the game with a new Culture, Kingdom, three sub-Clans, 10 leaders/heroes, and party templates.

Relevant Files:
- Culture: spcultures.xml
- Kingdom: spkingdoms.xml
- Clans: spclans.xml
- Lords: lords.xml
- Heroes: heroes.xml
- Party Template: party_templates.xml
- Custom spawn locations: headhunter_locations.xml
- Meeting location: spmeetings.xml

These files(and dll) are defined in the SubModule.xml, load order is important (modify at your own risk).

The codebase is pretty straight forward.

**Main:**

- `OnBeforeInitialModuleScreenSetAsRoot`: Initializes the custom spawn locations specified in headhunter_locations.xml
- `OnGameStart`: Ensures the player has started a Campaign, and then adds a custom `KingdomTemplateBehavior` to the CampaignGameStarter.
- `SetupHeadhunterParties`: Retrieves all spawned parties within the current Campaign, move the new Heroes/Leaders to the positions specified in headhunter_locations.xml, and assign Varagos Castle as a Fief to Suuhl, the new Kindom's leader.

**KingdomTemplateBehavior:**

This class allows us to hook into some of the game's more closed-off lifecycle hooks.  Specifically, the `OnQuestStartedEvent`.

- `RegisterEvents`:  Adds a listener for `CampaignEvents.OnQuestStartedEvent`.  When the event is received, it will call the `KingdomBehvaiorTemplate`'s private `OnQuestStarted` method.
- `OnQuestStarted`:  Ensures the Campaign is valid, a new Campaign, and that the Quest "rebuild_player_clan_storymode_quest" is the Quest that's starting.  It then calls the static method `SetupHeadhunterParties` in the `Main` class and passes the current Campaign's MobileParties list.
