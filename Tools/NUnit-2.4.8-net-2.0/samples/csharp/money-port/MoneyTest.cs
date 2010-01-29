// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Samples.Money.Port
{
	using System;
	using NUnit.Framework;
	/// <summary>
	/// 
	/// </summary>
	public class MoneyTest: TestCase 
	{
		private Money f12CHF;
		private Money f14CHF;
		private Money f7USD;
		private Money f21USD;
        
		private MoneyBag fMB1;
		private MoneyBag fMB2;

		/// <summary>
		/// 
		/// </summary>
		protected override void SetUp() 
		{
			f12CHF= new Money(12, "CHF");
			f14CHF= new Money(14, "CHF");
			f7USD= new Money( 7, "USD");
			f21USD= new Money(21, "USD");

			fMB1= new MoneyBag(f12CHF, f7USD);
			fMB2= new MoneyBag(f14CHF, f21USD);
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestBagMultiply() 
		{
			// {[12 CHF][7 USD]} *2 == {[24 CHF][14 USD]}
			Money[] bag = { new Money(24, "CHF"), new Money(14, "USD") };
			MoneyBag expected= new MoneyBag(bag);
			Assertion.AssertEquals(expected, fMB1.Multiply(2));
			Assertion.AssertEquals(fMB1, fMB1.Multiply(1));
			Assertion.Assert(fMB1.Multiply(0).IsZero);
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestBagNegate() 
		{
			// {[12 CHF][7 USD]} negate == {[-12 CHF][-7 USD]}
			Money[] bag= { new Money(-12, "CHF"), new Money(-7, "USD") };
			MoneyBag expected= new MoneyBag(bag);
			Assertion.AssertEquals(expected, fMB1.Negate());
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestBagSimpleAdd() 
		{
			// {[12 CHF][7 USD]} + [14 CHF] == {[26 CHF][7 USD]}
			Money[] bag= { new Money(26, "CHF"), new Money(7, "USD") };
			MoneyBag expected= new MoneyBag(bag);
			Assertion.AssertEquals(expected, fMB1.Add(f14CHF));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestBagSubtract() 
		{
			// {[12 CHF][7 USD]} - {[14 CHF][21 USD] == {[-2 CHF][-14 USD]}
			Money[] bag= { new Money(-2, "CHF"), new Money(-14, "USD") };
			MoneyBag expected= new MoneyBag(bag);
			Assertion.AssertEquals(expected, fMB1.Subtract(fMB2));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestBagSumAdd() 
		{
			// {[12 CHF][7 USD]} + {[14 CHF][21 USD]} == {[26 CHF][28 USD]}
			Money[] bag= { new Money(26, "CHF"), new Money(28, "USD") };
			MoneyBag expected= new MoneyBag(bag);
			Assertion.AssertEquals(expected, fMB1.Add(fMB2));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestIsZero() 
		{
			Assertion.Assert(fMB1.Subtract(fMB1).IsZero);

			Money[] bag = { new Money(0, "CHF"), new Money(0, "USD") };
			Assertion.Assert(new MoneyBag(bag).IsZero);
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestMixedSimpleAdd() 
		{
			// [12 CHF] + [7 USD] == {[12 CHF][7 USD]}
			Money[] bag= { f12CHF, f7USD };
			MoneyBag expected= new MoneyBag(bag);
			Assertion.AssertEquals(expected, f12CHF.Add(f7USD));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestMoneyBagEquals() 
		{
			Assertion.Assert(!fMB1.Equals(null)); 

			Assertion.AssertEquals(fMB1, fMB1);
			MoneyBag equal= new MoneyBag(new Money(12, "CHF"), new Money(7, "USD"));
			Assertion.Assert(fMB1.Equals(equal));
			Assertion.Assert(!fMB1.Equals(f12CHF));
			Assertion.Assert(!f12CHF.Equals(fMB1));
			Assertion.Assert(!fMB1.Equals(fMB2));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestMoneyBagHash() 
		{
			MoneyBag equal= new MoneyBag(new Money(12, "CHF"), new Money(7, "USD"));
			Assertion.AssertEquals(fMB1.GetHashCode(), equal.GetHashCode());
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestMoneyEquals() 
		{
			Assertion.Assert(!f12CHF.Equals(null)); 
			Money equalMoney= new Money(12, "CHF");
			Assertion.AssertEquals(f12CHF, f12CHF);
			Assertion.AssertEquals(f12CHF, equalMoney);
			Assertion.AssertEquals(f12CHF.GetHashCode(), equalMoney.GetHashCode());
			Assertion.Assert(!f12CHF.Equals(f14CHF));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestMoneyHash() 
		{
			Assertion.Assert(!f12CHF.Equals(null)); 
			Money equal= new Money(12, "CHF");
			Assertion.AssertEquals(f12CHF.GetHashCode(), equal.GetHashCode());
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestNormalize() 
		{
			Money[] bag= { new Money(26, "CHF"), new Money(28, "CHF"), new Money(6, "CHF") };
			MoneyBag moneyBag= new MoneyBag(bag);
			Money[] expected = { new Money(60, "CHF") };
			// note: expected is still a MoneyBag
			MoneyBag expectedBag= new MoneyBag(expected);
			Assertion.AssertEquals(expectedBag, moneyBag);
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestNormalize2() 
		{
			// {[12 CHF][7 USD]} - [12 CHF] == [7 USD]
			Money expected= new Money(7, "USD");
			Assertion.AssertEquals(expected, fMB1.Subtract(f12CHF));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestNormalize3() 
		{
			// {[12 CHF][7 USD]} - {[12 CHF][3 USD]} == [4 USD]
			Money[] s1 = { new Money(12, "CHF"), new Money(3, "USD") };
			MoneyBag ms1= new MoneyBag(s1);
			Money expected= new Money(4, "USD");
			Assertion.AssertEquals(expected, fMB1.Subtract(ms1));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestNormalize4() 
		{
			// [12 CHF] - {[12 CHF][3 USD]} == [-3 USD]
			Money[] s1 = { new Money(12, "CHF"), new Money(3, "USD") };
			MoneyBag ms1= new MoneyBag(s1);
			Money expected= new Money(-3, "USD");
			Assertion.AssertEquals(expected, f12CHF.Subtract(ms1));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestPrint() 
		{
			Assertion.AssertEquals("[12 CHF]", f12CHF.ToString());
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestSimpleAdd() 
		{
			// [12 CHF] + [14 CHF] == [26 CHF]
			Money expected= new Money(26, "CHF");
			Assertion.AssertEquals(expected, f12CHF.Add(f14CHF));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestSimpleBagAdd() 
		{
			// [14 CHF] + {[12 CHF][7 USD]} == {[26 CHF][7 USD]}
			Money[] bag= { new Money(26, "CHF"), new Money(7, "USD") };
			MoneyBag expected= new MoneyBag(bag);
			Assertion.AssertEquals(expected, f14CHF.Add(fMB1));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestSimpleMultiply() 
		{
			// [14 CHF] *2 == [28 CHF]
			Money expected= new Money(28, "CHF");
			Assertion.AssertEquals(expected, f14CHF.Multiply(2));
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestSimpleNegate() 
		{
			// [14 CHF] negate == [-14 CHF]
			Money expected= new Money(-14, "CHF");
			Assertion.AssertEquals(expected, f14CHF.Negate());
		}
		/// <summary>
		/// 
		/// </summary>
		public void TestSimpleSubtract() 
		{
			// [14 CHF] - [12 CHF] == [2 CHF]
			Money expected= new Money(2, "CHF");
			Assertion.AssertEquals(expected, f14CHF.Subtract(f12CHF));
		}
	}
}