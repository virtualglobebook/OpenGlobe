// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Reflection;

namespace NUnit.Core.Extensions
{
	/// <summary>
	/// MaxTimeTestCase is a special form of test case that measures
	/// the elapsed time to run a test, failing the test if it
	/// exceeds a certain amount.
	/// 
	/// Implementation Notes
	/// 
	/// In implementing a Decorator, you need to honor any propeties
	/// taht were set on the original test before you decorated it
	/// and also be ready to deal correctly with any other decorators
	/// that are applied before or after yours.
	/// 
	/// There are three basic approaches to doing this:
	/// 
	/// 1) Modify or set properties of the original test case 
	///    and return it. This avoids all issues of ordering
	///    but won't work if new properties are needed.
	/// 2) Replace the test case with a new one implementing
	///    additional properties. Copy all properties from
	///    the original case. This is the approach used in
	///    this example.
	/// 3) Wrap the test case with a special test case that
	///    delegates all properties and methods to the
	///    origial test case. This is quite difficult to
	///    get right, but is necessary in some cases.
	/// </summary>
	public class MaxTimeTestCase : NUnitTestMethod
	{
		private int maxTime = 0;
		bool expectFailure;

		public MaxTimeTestCase( NUnitTestMethod testCase, int maxTime, bool expectFailure )
			: base( testCase.Method )
		{
			this.maxTime = maxTime;
			this.expectFailure = expectFailure;

			// Copy all the attributes of the original test
			this.Description = testCase.Description;
			this.Fixture = testCase.Fixture;
			this.Parent = testCase.Parent;
			this.RunState = testCase.RunState;
			this.IgnoreReason = testCase.IgnoreReason;
			this.ExceptionExpected = testCase.ExceptionExpected;
			this.ExpectedExceptionName = testCase.ExpectedExceptionName;
			this.ExpectedExceptionType = testCase.ExpectedExceptionType;
			this.ExpectedMessage = testCase.ExpectedMessage;
			this.Properties = testCase.Properties;
			this.Categories = testCase.Categories;
		}

		public override void Run(TestCaseResult result)
		{
			base.Run( result );
			if ( result.IsSuccess && !ExceptionExpected )
			{
				int elapsedTime = (int)(result.Time * 1000);
				if ( elapsedTime > maxTime && !expectFailure)
					result.Failure( string.Format( "Elapsed time of {0}ms exceeds maximum of {1}ms", elapsedTime, maxTime ), null );
				else if ( elapsedTime <= maxTime && expectFailure )
					result.Failure( "Expected a timeout failure, but none occured", null );
			}
		}
	}
}
