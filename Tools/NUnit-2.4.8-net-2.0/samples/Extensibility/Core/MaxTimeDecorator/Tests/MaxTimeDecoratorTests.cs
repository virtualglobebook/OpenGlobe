// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using NUnit.Framework;
using NUnit.Framework.Extensions;

namespace Tests
{
	/// <summary>
	/// Tests for MaxTime decoration. Some of these tests are
	/// actually expected to fail, so the results must be
	/// examined visually. It would be possible to test these
	/// automatically by running a second copy of NUnit, but
	/// this is better handled through an acceptance test
	/// suite such as FIT.
	/// </summary>
	[TestFixture]
	public class MaxTimeDecoratorTests
	{
		[Test,MaxTime(1000)]
		public void MaxTimeNotExceeded()
		{
		}

		[Test,MaxTime(1000), ExpectedException(typeof(AssertionException),ExpectedMessage="Intentional failure")]
		public void MaxTimeNotExceededButFailed()
		{
			Assert.Fail("Intentional failure");
		}

		[Test,MaxTime(1, ExpectFailure=true), Description("This should fail due to time exceeded")]
		public void MaxTimeWasExceeded()
		{
			System.Threading.Thread.Sleep(100);
		}

		[Test,MaxTime(1),ExpectedException(typeof(AssertionException),ExpectedMessage="Intentional failure")]
		public void MaxTimeWasExceededButFailed()
		{
			System.Threading.Thread.Sleep(100);
			Assert.Fail("Intentional failure");
		}
	}
}
