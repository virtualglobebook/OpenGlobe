// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Reflection;
using NUnit.Core.Extensibility;

namespace NUnit.Core.Extensions
{
	/// <summary>
	/// MaxTimeDecorator is a test decorator which permits specifying a maximum
	/// time that the test should take. If it takes longer, the test fails.
	/// 
	/// The decorator works by replacing the original test with a special
	/// test case that overrides the run method to measure the time taken.
	/// </summary>
	[NUnitAddin(Description="Fails a test if its elapsed time is longer than a given maximum")]
	public class MaxTimeDecorator : IAddin, ITestDecorator
	{
		#region IAddin Members

		public bool Install(IExtensionHost host)
		{
			System.Diagnostics.Trace.WriteLine( "MaxTimeDecorator: Install called" );
			IExtensionPoint decorators = host.GetExtensionPoint( "TestDecorators" );
			if ( decorators == null ) return false;

			decorators.Install( this );
			return true;
		}

		#endregion

		#region ITestDecorator Members

		public Test Decorate(Test test, System.Reflection.MemberInfo member)
		{
			if ( test is NUnitTestMethod )
			{
				Attribute attr = Reflect.GetAttribute( 
					member, "NUnit.Framework.Extensions.MaxTimeAttribute", false );
				if ( attr != null )
				{
					int maxTime = (int)Reflect.GetPropertyValue( attr, "MaxTime", BindingFlags.Public | BindingFlags.Instance );
					bool expectFailure = (bool)Reflect.GetPropertyValue( attr, "ExpectFailure", BindingFlags.Public | BindingFlags.Instance );
					test = new MaxTimeTestCase( (NUnitTestMethod)test, maxTime, expectFailure );
				}
			}

			return test;
		}

		#endregion
	}
}
