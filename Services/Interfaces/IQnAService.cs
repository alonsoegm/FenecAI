using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IQnAService
	{
		/// <summary>
		/// Queries the Azure Question Answering Knowledge Base (QnA) to retrieve the best answer.
		/// </summary>
		Task<QnAResponse> GetAnswerAsync(QnARequest request);
	}
}
