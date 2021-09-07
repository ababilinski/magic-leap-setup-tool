using MagicLeapSetupTool.Editor.ScriptableObjects;

namespace MagicLeapSetupTool.Editor.Interfaces
{
	public interface ISetupStep
	{
		bool Draw(MagicLeapSetupDataScriptableObject data);


		void Execute(MagicLeapSetupDataScriptableObject data);
	}
}