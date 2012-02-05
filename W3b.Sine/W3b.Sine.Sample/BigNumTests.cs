using System;
using System.Collections.Generic;
using System.Text;

namespace W3b.Sine {
	
	internal class BigNumTests {
		
		private static Random rnd = new Random();
		
		private const Int32 nofTests = 25;
		
		public static Double GetRnd() {
			Double num = rnd.NextDouble();
			return 1 / num;
		}
		
		public static void Test() {
			
			///////////////////////////////////
			// Step 1: Simple representation and ToDecimalString tests
			///////////////////////////////////
			
			Console.WriteLine("Representation");
			
			Random rnd = new Random();
			
			for(int i=0;i<nofTests;i++) {
				
				TestStore( GetRnd() );
			}
			
			TestStore( 1.7E+3 );
			TestStore( 1111.222E+3 );
			TestStore( 1111.222E+20 );
			TestStore( 1111.222E-20 );
			
			///////////////////////////////////
			// Step 2: Comparison
			///////////////////////////////////
			Console.WriteLine("Comparison");
			for(int i=0;i<nofTests;i++) {
				
				TestCompare( GetRnd(), GetRnd() );
			}
			
			///////////////////////////////////
			// Step 3: Addition
			///////////////////////////////////
			Console.WriteLine("Addition");
			for(int i=0;i<nofTests;i++) {
				
				TestAdd( GetRnd(), GetRnd() );
			}
			
			///////////////////////////////////
			// Step 4: Multiplication
			///////////////////////////////////
			Console.WriteLine("Multiplication");
			for(int i=0;i<nofTests;i++) {
				
				TestMul( GetRnd(), GetRnd() );
			}
			
			///////////////////////////////////
			// Step 5: Division
			///////////////////////////////////
			Console.WriteLine("Division");
			for(int i=0;i<nofTests;i++) {
				
				TestDiv( GetRnd(), GetRnd() );
			}
			
			///////////////////////////////////
			// Step 6: Power
			///////////////////////////////////
			Console.WriteLine("Power");
			for(int i=0;i<nofTests;i++) {
				
				TestPow( GetRnd(), rnd.NextDouble() );
			}
			
			///////////////////////////////////
			// Step 7: Sine
			///////////////////////////////////
			Console.WriteLine("Sine");
			for(int i=0;i<nofTests;i++) {
				
				TestSin( GetRnd() );
			}
			
		}
		
		private static void TestStore(Double value) {
			
			Double   ai = value;
			BigNum   ab = ai; // implicit conversion
			
			String  bi = ai.ToString();
			String  bn = ab.ToString();
			
			Console.WriteLine( "{0,18} : {1,18} -> {2}", ai, bn,  bi == bn ? "Pass" : "Fail" );
			
		}
		
		private static void TestAdd(Double a, Double b) {
			
			String di = (a+b).ToString();
			String ni  = ( BigNum.Create(a) + BigNum.Create(b) ).ToString();
			
			Console.WriteLine("{0,18} + {1,18} = {2,18} : {3,18} -> {4}", a, b, di, ni, di == ni ? "Pass" : "Fail" );
			
		}
		
		private static void TestCompare(Double a, Double b) {
			
			String di = a.CompareTo(b).ToString() + ' ' + b.CompareTo(a).ToString();
			
			BigNum na = a;
			BigNum nb = b;
			
			String ni = na.CompareTo(nb).ToString() + ' ' + nb.CompareTo(na).ToString();
			
			Console.WriteLine("{0,18} comp {1,18} = {2,18} : {3,18} -> {4}", a, b, di, ni, di == ni ? "Pass" : "Fail" );
			
			
		}
		
		private static void TestMul(Double a, Double b) {
			
			String di = (a*b).ToString();
			String ni = ( BigNum.Create(a) * BigNum.Create(b) ).ToString();
			
			Console.WriteLine("{0,6} * {1,6} = {2,6} : {3,6} -> {4,6}", a, b, di, ni, di == ni ? "Pass" : "Fail" );
		}
		
		private static void TestDiv(Double a, Double b) {
			
			String di = (a/b).ToString();
			String ni  = ( BigNum.Create(a) / BigNum.Create(b) ).ToString();
			
			Console.WriteLine("{0,6} / {1,6} = {2,6} : {3,6} -> {4,6}", a, b, di, ni, di == ni ? "Pass" : "Fail" );
		}
		
		private static void TestPow(Double a, Double b) {
			
			Int32 power = (Int32)System.Math.Round(100 * b);
			
			String di = System.Math.Pow(a, power).ToString();
			String ni = BigMath.Pow( a, power ).ToString();
			
			Console.WriteLine("{0,6} ^ {1,6} = {2,6} : {3,6} -> {4,6}", a, power, di, ni, di == ni ? "Pass" : "Fail" );
		}
		
		private static void TestSin(Double a) {
			
			String di = System.Math.Sin(a).ToString();
			String ni = BigMath.Sin( BigNum.Create(a) ).ToString();
			
			Console.WriteLine("Sin({0,6}) = {1,6} : {2,6} -> {3,6}", a, di, ni, di == ni ? "Pass" : "Fail" );
		}
		
	}
}
