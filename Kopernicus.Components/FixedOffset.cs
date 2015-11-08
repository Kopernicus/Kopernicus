

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Fixed version of PQSMod_VertexHeightOffset
        /// </summary>
        public class PQSMod_FixedOffset : PQSMod_VertexHeightOffset
        {
            // Re-arrange the Vertex Build order
            public override void OnVertexBuild(PQS.VertexBuildData data)
            {
                // Do nothing
                // base.OnVertexBuild(data);
            }

            // Build the height in the correct function
            public override void OnVertexBuildHeight(PQS.VertexBuildData data)
            {
                // Tricky...
                base.OnVertexBuild(data);
            }
        }
    }
}
