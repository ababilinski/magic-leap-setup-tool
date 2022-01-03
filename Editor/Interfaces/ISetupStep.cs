namespace MagicLeapSetupTool.Editor.Interfaces
{
    /// <summary>
    /// Interface for each setup step
    /// </summary>
    public interface ISetupStep
    {
        /// <summary>
        /// How to draw the Step
        /// </summary>
        /// <returns>Whether or not the item drew correctly.</returns>
        bool Draw();

        /// <summary>
        /// Action during step execution
        /// </summary>
        void Execute();

        void Refresh();
    }
}