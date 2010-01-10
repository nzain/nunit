// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Xml;
using NUnit.Framework.Internal;
using NUnit.TestUtilities;

namespace NUnit.Framework.Api
{
	/// <summary>
	/// Summary description for TestResultTests.
	/// </summary>
	[TestFixture]
	public abstract class TestResultTests
	{
		protected TestResult testResult;
        protected TestResult suiteResult;
        protected XmlNode testNode;
        protected XmlNode suiteNode;

		[SetUp]
		public void SetUp()
		{
			testResult = new TestResult( new TestMethod(Reflect.GetNamedMethod( typeof(DummySuite), "DummyMethod" ) ) );
            suiteResult = new TestResult(new TestSuite(typeof(DummySuite)));

            SetResultState();
            suiteResult.AddResult(testResult);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(testResult.ToXml());
            testNode = doc.FirstChild;
            doc.LoadXml(suiteResult.ToXml());
            suiteNode = doc.FirstChild;
        }

        [Test]
        public void TestResultBasicInfo()
        {
            Assert.AreEqual("DummyMethod", testResult.Name);
            Assert.AreEqual("NUnit.Framework.Api.TestResultTests+DummySuite.DummyMethod", testResult.FullName);
            Assert.AreEqual(0.0, testResult.Time);
        }

        [Test]
        public void SuiteResultBasicInfo()
        {
            Assert.AreEqual("TestResultTests+DummySuite", suiteResult.Name);
            Assert.AreEqual("NUnit.Framework.Api.TestResultTests+DummySuite", suiteResult.FullName);
            Assert.AreEqual(0.0, suiteResult.Time);
        }

        [Test]
        public void TestResultXmlNodeBasicInfo()
        {
            Assert.True(testNode is XmlElement);
            Assert.AreEqual("test-case", testNode.Name);
            Assert.AreEqual("DummyMethod", testNode.Attributes["name"].Value);
            Assert.AreEqual("NUnit.Framework.Api.TestResultTests+DummySuite.DummyMethod", testNode.Attributes["fullname"].Value);
            Assert.AreEqual("0.000", testNode.Attributes["time"].Value);
            Assert.AreEqual(0, testNode.SelectNodes("test-case").Count);
        }

        [Test]
        public void SuiteResultXmlNodeBasicInfo()
        {
            Assert.True(suiteNode is XmlElement);
            Assert.AreEqual("test-suite", suiteNode.Name);
            Assert.AreEqual("TestResultTests+DummySuite", suiteNode.Attributes["name"].Value);
            Assert.AreEqual("NUnit.Framework.Api.TestResultTests+DummySuite", suiteNode.Attributes["fullname"].Value);
            Assert.AreEqual("0.000", suiteNode.Attributes["time"].Value);
        }

        protected abstract void SetResultState();

        public class DummySuite
        {
            public void DummyMethod() { }
        }
    }

    public class DefaultResultTests : TestResultTests
    {
        protected override void SetResultState()
        {
        }

        [Test]
        public void TestResultIsInconclusive()
        {
            Assert.AreEqual(ResultState.Inconclusive, testResult.ResultState);
        }

        [Test]
        public void SuiteResultIsInconclusive()
        {
            Assert.AreEqual(ResultState.Inconclusive, suiteResult.ResultState);
        }

        [Test]
        public void TestResultXmlNodeIsInconclusive()
        {
            Assert.AreEqual("Inconclusive", testNode.Attributes["result"].Value);
        }

        [Test]
        public void SuiteResultXmlNodeIsInconclusive()
        {
            Assert.AreEqual("Inconclusive", suiteNode.Attributes["result"].Value);
        }

        [Test]
        public void SuiteResultXmlNodeHasOneChildTest()
        {
            Assert.AreEqual(1, suiteNode.SelectNodes("test-case").Count);
        }
    }

    public class SuccessResultTests : TestResultTests
    {
        protected override void SetResultState()
        {
            testResult.Success();
        }

        [Test]
        public void TestResultIsSuccess()
        {
            Assert.True(testResult.IsSuccess);
        }

        [Test]
        public void SuiteResultIsSuccess()
        {
            Assert.True(suiteResult.IsSuccess);
        }

        [Test]
        public void TestResultXmlNodeIsSuccess()
        {
            Assert.AreEqual("Success", testNode.Attributes["result"].Value);
        }

        [Test]
        public void SuiteResultXmlNodeIsSuccess()
        {
            Assert.AreEqual("Success", suiteNode.Attributes["result"].Value);
        }

        [Test]
        public void SuiteResultXmlNodeHasOneChildTest()
        {
            Assert.AreEqual(1, suiteNode.SelectNodes("test-case").Count);
        }
    }

    public class IgnoredResultTests : TestResultTests
    {
        protected override void SetResultState()
        {
            testResult.Ignore("because");
        }

        [Test]
		public void TestResultIsIgnored()
		{
            Assert.AreEqual(ResultState.Ignored, testResult.ResultState);
            Assert.AreEqual("because", testResult.Message);
        }

        [Test]
        public void SuiteResultIsUnchanged()
        {
            Assert.AreEqual(ResultState.Inconclusive, suiteResult.ResultState);
            Assert.Null(suiteResult.Message);
        }

        [Test]
        public void TestResultXmlNodeIsIgnored()
        {
            Assert.AreEqual("Ignored", testNode.Attributes["result"].Value);
            XmlNode reason = testNode.SelectSingleNode("reason");
            Assert.NotNull(reason);
            Assert.NotNull(reason.SelectSingleNode("message"));
            Assert.AreEqual("because", reason.SelectSingleNode("message").InnerText);
            Assert.Null(reason.SelectSingleNode("stacktrace"));
        }

        [Test]
        public void SuiteResultXmlNodeIsUnchanged()
        {
            Assert.AreEqual("Inconclusive", suiteNode.Attributes["result"].Value);
        }

        [Test]
        public void SuiteResultXmlNodeHasOneChildTest()
        {
            Assert.AreEqual(1, suiteNode.SelectNodes("test-case").Count);
        }
    }

    public class FailedResultTests : TestResultTests
    {
        protected override void SetResultState()
        {
            testResult.Failure("message", "stack trace");
        }

        [Test]
        public void TestResultIsFailure()
        {
            Assert.AreEqual(ResultState.Failure, testResult.ResultState);
            Assert.AreEqual("message", testResult.Message);
            Assert.AreEqual("stack trace", testResult.StackTrace);
        }

        [Test]
        public void SuiteResultIsFailure()
        {
            Assert.AreEqual(ResultState.Failure, suiteResult.ResultState);
            Assert.AreEqual("Child test failed", suiteResult.Message);
            Assert.Null(suiteResult.StackTrace);
        }

        [Test]
        public void TestResultXmlNodeIsFailure()
        {
            Assert.AreEqual("Failure", testNode.Attributes["result"].Value);
            XmlNode failureNode = testNode.SelectSingleNode("failure");
            Assert.NotNull(failureNode, "No failure element found");

            XmlNode messageNode = failureNode.SelectSingleNode("message");
            Assert.NotNull(messageNode, "No message element found");
            Assert.AreEqual("message", messageNode.InnerText);

            XmlNode stacktraceNode = failureNode.SelectSingleNode("stacktrace");
            Assert.NotNull(stacktraceNode, "No stacktrace element found");
            Assert.AreEqual("stack trace", stacktraceNode.InnerText);
        }

        [Test]
        public void SuiteResultXmlNodeIsFailure()
        {
            Assert.AreEqual("Failure", suiteNode.Attributes["result"].Value);
            XmlNode failureNode = suiteNode.SelectSingleNode("failure");
            Assert.NotNull(failureNode, "No failure element found");

            XmlNode messageNode = failureNode.SelectSingleNode("message");
            Assert.NotNull(messageNode, "No message element found");
            Assert.AreEqual("Child test failed", messageNode.InnerText);

            XmlNode stacktraceNode = failureNode.SelectSingleNode("stacktrace");
            Assert.Null(stacktraceNode, "There should be no stacktrace");
        }

        [Test]
        public void SuiteResultXmlNodeHasOneChildTest()
        {
            Assert.AreEqual(1, suiteNode.SelectNodes("test-case").Count);
        }
    }

    public class MixedResultTests : TestResultTests
    {
        protected override void SetResultState()
        {
            testResult.Success();
            suiteResult.AddResult(testResult);
            testResult.Failure("message", "stack trace");
            suiteResult.AddResult(testResult);
            testResult.Success();
            suiteResult.AddResult(testResult);
            testResult.SetResult(ResultState.Inconclusive, "inconclusive reason", "stacktrace");
            suiteResult.AddResult(testResult);
        }

        [Test]
        public void SuiteResultIsFailure()
        {
            Assert.AreEqual(ResultState.Failure, suiteResult.ResultState);
            Assert.AreEqual("Child test failed", suiteResult.Message);
            Assert.Null(suiteResult.StackTrace);
        }

        [Test]
        public void SuiteResultXmlNodeIsFailure()
        {
            Assert.AreEqual("Failure", suiteNode.Attributes["result"].Value);
            XmlNode failureNode = suiteNode.SelectSingleNode("failure");
            Assert.NotNull(failureNode, "No failure element found");

            XmlNode messageNode = failureNode.SelectSingleNode("message");
            Assert.NotNull(messageNode, "No message element found");
            Assert.AreEqual("Child test failed", messageNode.InnerText);

            XmlNode stacktraceNode = failureNode.SelectSingleNode("stacktrace");
            Assert.Null(stacktraceNode, "There should be no stacktrace");
        }

        [Test]
        public void SuiteResultXmlNodeHasFiveChildTests()
        {
            Assert.AreEqual(5, suiteNode.SelectNodes("test-case").Count);
        }
    }
}
