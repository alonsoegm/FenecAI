namespace FenecAI.API.Models
{
	public class QnAResponse
	{
		public string Answer { get; set; } = string.Empty;
		public double Confidence { get; set; }
		public IList<string> AlternateAnswers { get; set; } = new List<string>();
	}
}
