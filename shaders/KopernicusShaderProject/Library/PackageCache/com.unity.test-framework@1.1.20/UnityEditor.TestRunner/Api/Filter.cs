using System;
using UnityEngine;
using UnityEngine.TestTools.TestRunner.GUI;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// The filter class provides the <see cref="TestRunnerApi"/> with a specification of what tests to run when [running tests programmatically](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/extension-run-tests.html).
    /// </summary>
    [Serializable]
    public class Filter
    {
        /// <returns>
        /// An enum flag that specifies if Edit Mode or Play Mode tests should run.
        ///</returns>
        [SerializeField]
        public TestMode testMode;
        /// <returns>
        /// The full name of the tests to match the filter. This is usually in the format FixtureName.TestName. If the test has test arguments, then include them in parenthesis. E.g. MyTestClass2.MyTestWithMultipleValues(1).
        /// </returns>
        [SerializeField]
        public string[] testNames;
        /// <returns>
        /// The same as testNames, except that it allows for Regex. This is useful for running specific fixtures or namespaces. E.g. "^MyNamespace\\." Runs any tests where the top namespace is MyNamespace.
        /// </returns>
        [SerializeField]
        public string[] groupNames;
        /// <returns>
        /// The name of a [Category](https://nunit.org/docs/2.2.7/category.html) to include in the run. Any test or fixtures runs that have a Category matching the string.
        /// </returns>
        [SerializeField]
        public string[] categoryNames;
        /// <returns>
        /// The name of assemblies included in the run. That is the assembly name, without the .dll file extension. E.g., MyTestAssembly
        /// </returns>
        [SerializeField]
        public string[] assemblyNames;
        /// <returns>
        /// The <see cref="BuildTarget"/> platform to run the test on. If set to null, then the Editor is the target for the tests.
        /// </returns>
        [SerializeField]
        public BuildTarget? targetPlatform;

        internal RuntimeTestRunnerFilter ToRuntimeTestRunnerFilter(bool synchronousOnly)
        {
            return new RuntimeTestRunnerFilter()
            {
                testNames = testNames,
                categoryNames = categoryNames,
                groupNames = groupNames,
                assemblyNames = assemblyNames,
                synchronousOnly = synchronousOnly
            };
        }
    }
}
