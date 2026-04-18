using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using Stunlock.Core;

namespace ModCore.Data;
public static class ServantData
{
	public static List<string> ServantTypes = new List<string>()
	{
		"Cleric",
		"Lightweaver",
		"Nun",
		"Rifleman",
		"Priest",
		"Knight Shield",
		"Knight 2H",
		"Paladin",
		"Longbowman",
		"Devoted",
		"Brawler",
		"Pyro",
		"TractorBeamer",
		"Railgunner",
		"Sentry Officer",
		"Thief",
		"Slave Master Sentry",
		"Slave Master Enforcer",
		"Lesser Succubus",
		"Succubus",
		"Exsanguinator",
		"Bellringer",
		"Stalker",
		"Torchbearer",
		"Bandit Bomber",
		"Militia Bomber",
		"Deadeye",
		"Mugger",
		"Militia Guard",
		"Church of Light Archer"
	};
	

	public static List<KeyValuePair<PrefabGUID, PrefabGUID>> UnitToServantList = new List<KeyValuePair<PrefabGUID, PrefabGUID>>()
	{
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Cleric, Prefabs.CHAR_ChurchOfLight_Cleric_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Lightweaver, Prefabs.CHAR_ChurchOfLight_Lightweaver_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Nun, Prefabs.CHAR_Farmlands_Nun_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Rifleman, Prefabs.CHAR_ChurchOfLight_Rifleman_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Priest, Prefabs.CHAR_ChurchOfLight_Priest_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Knight_Shield, Prefabs.CHAR_ChurchOfLight_Knight_Shield_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Knight_2H, Prefabs.CHAR_ChurchOfLight_Knight_2H_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Paladin, Prefabs.CHAR_ChurchOfLight_Paladin_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Longbowman, Prefabs.CHAR_Militia_Longbowman_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Devoted, Prefabs.CHAR_Militia_Devoted_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Heavy, Prefabs.CHAR_Militia_Heavy_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Gloomrot_Pyro, Prefabs.CHAR_Gloomrot_Pyro_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Gloomrot_TractorBeamer, Prefabs.CHAR_Gloomrot_TractorBeamer_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Gloomrot_Railgunner, Prefabs.CHAR_Gloomrot_Railgunner_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Gloomrot_SentryOfficer, Prefabs.CHAR_Gloomrot_SentryOfficer_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Bandit_Thief, Prefabs.CHAR_Bandit_Thief_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_SlaveMaster_Sentry, Prefabs.CHAR_ChurchOfLight_SlaveMaster_Sentry_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_SlaveMaster_Enforcer, Prefabs.CHAR_ChurchOfLight_SlaveMaster_Enforcer_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Legion_NightMaiden_Lesser, Prefabs.CHAR_Legion_NightMaiden_Lesser_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Legion_NightMaiden, Prefabs.CHAR_Legion_NightMaiden_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Legion_Assassin, Prefabs.CHAR_Legion_Assassin_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_BellRinger, Prefabs.CHAR_Militia_BellRinger_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Bandit_Stalker, Prefabs.CHAR_Bandit_Stalker_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Torchbearer, Prefabs.CHAR_Militia_Torchbearer_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Bandit_Bomber, Prefabs.CHAR_Bandit_Bomber_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Bomber, Prefabs.CHAR_Militia_Bomber_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Bandit_Deadeye, Prefabs.CHAR_Bandit_Deadeye_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Bandit_Mugger, Prefabs.CHAR_Bandit_Mugger_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_Militia_Guard, Prefabs.CHAR_Militia_Guard_Servant),
		new KeyValuePair<PrefabGUID, PrefabGUID>(Prefabs.CHAR_ChurchOfLight_Archer, Prefabs.CHAR_ChurchOfLight_Archer_Servant),
	};
}
