using System;
using System.Collections.Generic;
using System.Text;

namespace W3b.Sine {
	
	public static class Math {
		
		private const Int32 iterations = 100;
		
		private const Double Pi = 3.1415926535897932385;
		private const Double Pi2 = 2*Pi;
		private const Double PiH = 0.5*Pi;
		
		public static Double Sine(Double theta) {
			
			// calculate sine using the taylor series, the infinite sum of x^r/r! but to n iterations
			Double retVal = 0;
			
			Boolean subtract = false;
			for(Int32 r=0;r<iterations;r++) {
				
				Double xPowerR = Power(theta, 2*r + 1);
				Double factori = Factorial( 2*r + 1);
				
				Double element = xPowerR / factori;
				
				retVal += subtract ? -element : element;
				
				subtract = !subtract;
			}
			
			return retVal;
			
		}
		
		public static Double Cosine(Double theta) {
			return Sine( theta + PiH );
		}
		
		public static Double Tangent(Double theta) {
			return Sine(theta) / Cosine(theta);
		}
		
		public static Double Cosec(Double theta) {
			return 1 / Sine(theta);
		}
		
		public static Double Secant(Double theta) {
			return 1 / Cosine(theta);
		}
		
		public static Double Cotangent(Double theta) {
			return 1 / Tangent(theta);
		}
		
		public static Double Power(Double x, Int32 exponent) {
			
			Int32 pow = exponent < 0 ? -exponent : exponent; // abs(exponent)
			
			Double retVal = 1;
			for(int i=0;i<pow;i++) {
				retVal = retVal * x;
			}
			
			if(exponent < 0) { // reciprocal minus powers
				retVal = 1 / retVal;
			}
			
			return retVal;
		}
		
		public static Double Factorial(Int32 x) {
			if(x == 0) return 1;
			return x * Factorial(x - 1);
		}
		
		public static BigNum CalculatePi() {
			return CalculatePi(50); // 50 iterations for enough accuracy
		}
		
		/// <summary>Calculates Pi, but is inaccuate beyond 4 digits for some reason. Not recommended.</summary>
		public static BigNum CalculatePi(Int32 iterations) {
			
			BigNum retVal = new BigNumDec();
			
			for(int i=0;i<iterations;i++) {
				
				// calculate inner polynomial
				// numerator
				BigNum innerPolynomialNumerator = new BigNumDec(
					120 * new BigNumDec(i).Power(2) +
					151 * new BigNumDec(i) +
					 47
				);
				
				BigNum innerPolynomrialDenominator = new BigNumDec(
					 512 * new BigNumDec(i).Power(4) +
					1024 * new BigNumDec(i).Power(3) +
					 712 * new BigNumDec(i).Power(2) +
					 192 * new BigNumDec(i) +
					  15
				);
				
				BigNum innerPolynomial = innerPolynomialNumerator / innerPolynomrialDenominator;
				
				BigNum outerReciprocal = new BigNumDec( 1 ) / new BigNumDec( 16 ).Power(i);
				
				BigNum result = outerReciprocal * innerPolynomial;
				
				retVal += result;
				
			}
			
			return retVal;
			
		}
		
	}
	
}
