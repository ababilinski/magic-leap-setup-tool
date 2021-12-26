
namespace MagicLeapSetupTool.Editor.Interfaces
{
	/// <summary>
	///     Interface for each setup step
	/// </summary>
	public interface ISetupStep
	{
		/// <summary>
		///     How to draw the Step
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		bool Draw();

		/// <summary>
		///     Action during step execution
		/// </summary>
		/// <param name="data"></param>
		void Execute();

		void Refresh();
	}
}
