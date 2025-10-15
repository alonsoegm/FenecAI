using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IConversationService
	{
		Task<ConversationalAnalysisResponse> AnalyzeConversationAsync(ConversationalAnalysisRequest request);
	}
}
