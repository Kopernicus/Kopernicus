namespace UnityEngine.TestTools
{
    /// <summary>
    /// An interface implemented by a MonoBehaviour test.
    /// </summary>
    public interface IMonoBehaviourTest
    {
        /// <returns> true when the test is considered finished.</returns>
        bool IsTestFinished {get; }
    }
}
