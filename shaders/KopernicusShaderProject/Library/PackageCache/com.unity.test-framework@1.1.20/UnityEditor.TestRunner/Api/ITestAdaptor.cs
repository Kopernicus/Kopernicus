using System.Collections.Generic;
using NUnit.Framework.Interfaces;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// ```ITestAdaptor``` is a representation of a node in the test tree implemented as a wrapper around the [NUnit](http://www.nunit.org/) [ITest](https://github.com/nunit/nunit/blob/master/src/NUnitFramework/framework/Interfaces/ITest.cs)  interface.
    /// </summary>
    public interface ITestAdaptor
    {
        /// <returns> The ID of the test tree node. The ID can change if you add new tests to the suite. Use UniqueName, if you want to have a more permanent point of reference. </returns>
        string Id { get; }
        /// <returns> The name of the test. E.g.,```MyTest```. </returns>
        string Name { get; }
        /// <returns> The full name of the test. E.g., ```MyNamespace.MyTestClass.MyTest```.</returns>
        string FullName { get; }
        /// <returns> The total number of test cases in the node and all sub-nodes.</returns>
        int TestCaseCount { get; }
        /// <returns> Whether the node has any children.</returns>
        bool HasChildren { get; }
        /// <returns>True if the node is a test suite/fixture, false otherwise.</returns>
        bool IsSuite { get; }
        /// <returns>The child nodes.</returns>
        IEnumerable<ITestAdaptor> Children { get; }
        /// <returns> The parent node, if any.</returns>
        ITestAdaptor Parent { get; }
        /// <returns>The test case timeout in milliseconds. Note that this value is only available on TestFinished.</returns>
        int TestCaseTimeout { get; }
        /// <returns>The type of test class as an ```NUnit``` <see cref="ITypeInfo"/>. If the node is not a test class, then the value is null.</returns>
        ITypeInfo TypeInfo { get; }
        /// <returns>The Nunit <see cref="IMethodInfo"/> of the test method. If the node is not a test method, then the value is null.</returns>
        IMethodInfo Method { get; }
        /// <returns>An array of the categories applied to the test or fixture.</returns>
        string[] Categories { get; }
        /// <returns>Returns true if the node represents a test assembly, false otherwise.</returns>
        bool IsTestAssembly { get; }
        /// <returns>The run state of the test node. Either ```NotRunnable```, ```Runnable```, ```Explicit```, ```Skipped```, or ```Ignored```.</returns>
        RunState RunState { get; }
        /// <returns>The description of the test.</returns>
        string Description { get; }
        /// <returns>The skip reason. E.g., if ignoring the test.</returns>
        string SkipReason { get; }
        /// <returns>The ID of the parent node.</returns>
        string ParentId { get; }
        /// <returns>The full name of the parent node.</returns>
        string ParentFullName { get; }
        /// <returns>A unique generated name for the test node. E.g., ```Tests.dll/MyNamespace/MyTestClass/[Tests][MyNamespace.MyTestClass.MyTest]```.</returns>
        string UniqueName { get; }
        /// <returns>A unique name of the parent node. E.g., ```Tests.dll/MyNamespace/[Tests][MyNamespace.MyTestClass][suite]```.</returns>
        string ParentUniqueName { get; }
        /// <returns>The child index of the node in its parent.</returns>
        int ChildIndex { get; }
        /// <returns>The mode of the test. Either **Edit Mode** or **Play Mode**.</returns>
        TestMode TestMode { get; }
    }
}
