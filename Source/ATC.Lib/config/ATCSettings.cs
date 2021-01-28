
namespace ATC.Lib.config
{
	public class ATCSettings : SettingsModule
	{
		public AWCSettings awc = new AWCSettings();
		public DIPSSettings dips = new DIPSSettings();
		public TVCSettings tvc = new TVCSettings();
		public CSESettings cse = new CSESettings();
	}
}
