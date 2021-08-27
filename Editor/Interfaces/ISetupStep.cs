using MagicLeapSetupTool.Editor.ScriptableObjects;

namespace MagicLeapSetupTool.Editor.Interfaces
{
	public interface ISetupStep
	{
		void Draw(MagicLeapSetupDataScriptableObject data);

		
		void Execute(MagicLeapSetupDataScriptableObject data);
	}
}