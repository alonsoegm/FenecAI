namespace FenecAI.API.Models
{
	public class ContentSafetyAnalysis
	{
		public int HateSeverity { get; set; }
		public int SelfHarmSeverity { get; set; }
		public int SexualSeverity { get; set; }
		public int ViolenceSeverity { get; set; }
		public bool Blocked { get; set; }
		public Dictionary<string, int> Raw { get; set; } = new();
	}
}
