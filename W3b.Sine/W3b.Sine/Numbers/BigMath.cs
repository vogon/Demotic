#define DoubleTrig
//#define EnableNotYetImplemented

using System;
using System.Collections.Generic;
using System.Text;

using Cult = System.Globalization.CultureInfo;

namespace W3b.Sine {
	
	public static class BigMath {
		
#region Numbers
		
		public static BigNum Abs(BigNum num) {
			
			return num.Absolute();
		}
		
		public static BigNum Floor(BigNum num) {
			
			return num.Floor();
		}
		
		public static BigNum Ceiling(BigNum num) {
			
			return num.Ceiling();
		}
		
		public static BigNum Max(BigNum x, BigNum y) {
			
			return x < y ? y : x;
		}
		
		public static BigNum Min(BigNum x, BigNum y) {
			
			return x > y ? y : x;
		}
		
#endregion
		
#region Misc
		
		private static int cnt = 0;
		
		public static BigInt Gcd(BigInt x, BigInt y) {
			
			cnt++;
			
			if( y == 0 ) return x;
			if( y == 1 ) return y;
			
			return Gcd( y, (BigInt)(x % y) );
			
		}
		
		public static Boolean IsInteger(BigNum x) {
			
			return x.Floor() == x;
		}
		
#endregion
		
#region Exponentiation and Factorial
		
		public static BigNum Pow(BigNum num, Int32 exponent) {
			
			return num.Power( exponent );
		}
		
		public static BigNum Pow(BigNum num, BigNum exponent) {
			
			// http://en.wikipedia.org/wiki/Exponentiation
			
			// a^x == E^( x * ln(a) )
			
			return Exp( exponent * Log( num ) );
		}
		
		/// <summary>Computes Euler's number raised to the specified exponent</summary>
		public static BigNum Exp(BigNum exponent) {
			
			const int iterations = 25;
			
			// E^x ~= sum(int i = 0 to inf, x^i/i!)
			//     ~= 1 + x + x^2/2! + x^3/3! + etc
			
			BigNum ret = exponent.Factory.Unity;
			ret += exponent;
			
			for(int i=2;i<iterations;i++) {
				
				BigNum numerator = exponent.Power( i );
				BigNum denominat = exponent.Factory.Create( Factorial( i ) );
				
				BigNum addThis = numerator / denominat;
				
				ret += addThis;
			}
			
			return ret;
		}
		
		/// <summary>Computes the Natural Logarithm (Log to base E, or Ln(x)) of the specified argument</summary>
		public static BigNum Log(BigNum x) {
			
			BigNumFactory f = x.Factory;
			
			if( x <= f.Zero ) throw new ArgumentOutOfRangeException("x", "Must be a positive real number");
			
			const int iterations = 25;
			
			BigNum one = f.Unity;
			BigNum two = f.Create(2);
			
			// ln(z) == 2 * Sum(n=0;n<inf;n++) { (1 / (2n+1) ) * Pow( (z-1)/(z+1), 2n+1) }
			
			BigNum ret = f.Zero;
			
			for(int n=0;n<iterations;n++) {
				
				int denom = 2*n + 1;
				
				Double coefficient = 1 / denom;
				BigNum otherHalf   = Pow( ( x - one ) / ( x + one ), denom ); // hurrah for integer powers
				
				BigNum sumThis = f.Create( coefficient ) * otherHalf;
				ret += sumThis;
			}
			
			return ret;
		}
		
		/// <summary>Computes the Logarithm of x for base b. So log10(100) is Log(10,100)</summary>
		public static BigNum Log(BigNum x, BigNum b) {
			
			return Log( x ) / Log( b );
		}
		
		/////////////////////////////////////////
		
		public static BigNum Gamma(BigNum z) {
			
			// http://www.rskey.org/gamma.htm
			// n! = nn√2πn exp(1/[12n + 2/(5n + 53/42n)] – n)(1 + O(n–8))
			// which requires the exponential function, and some function O (which the webpage fails to define, hmmm)
			
			// so here's the infinite product series from Wikipedia instead
			
			// Gamma(z) == (1/z) * Prod(n=1;n<inf;n++) {  (1+1/n)^z / (1+z/n) }
			
			const Int32 iterations = 25;
			
			BigNumFactory f = z.Factory;
			
			BigNum ret = z.Power(-1);
			
			for(int n=1;n<iterations;n++) {
				
				Double numerator1 = 1 + (1/n);
				BigNum numerator2 = Pow( f.Create( numerator1 ), z );
				
				BigNum denominato = f.Unity + z / f.Create(n);
				
				BigNum prodThis = numerator2 / denominato;
				
				ret *= prodThis;
			}
			
			return ret;
		}
		
		public static BigInt Factorial(BigInt num) {
			// HACK: Is there a more efficient implementation?
			// I know there is a way to cache and use earlier results, but not much more
			
			// also, note this function fails if num is non-integer. This should be the Gamma function instead
			
			BigInt one = (BigInt)BigIntFactory.Instance.Unity;
			
			if(num.IsZero) return one;
			if(num < num.Factory.Zero) throw new ArgumentException("Argument must be greater than or equal to zero", "num");
			
			return (BigInt)( num * Factorial( (BigInt)(num - one) ) );
		}
		
		internal static Int32 Factorial(Int32 number) {
			if(number == 0) return 1;
			return number * Factorial( number - 1 );
		}
		
		internal static Double Factorial(Double number) {
			if( number == 0 ) return 1;
			return number * Factorial( number - 1 );
		}
		
#endregion
		
#region Trig
		
		public static BigNum Sin(BigNum theta) {
			
			BigNumFactory f = theta.Factory;
			
			// calculate sine using the taylor series, the infinite sum of x^r/r! but to n iterations
			BigNum retVal = f.Zero;
			
			// first, reduce this to between 0 and 2Pi
			if( theta > f.TwoPi || theta < f.Zero )
				theta = theta % f.TwoPi;
			
			Boolean subtract = false;
			
			// using bignums for sine computation is too heavy. It's faster (and just as accurate) to use Doubles
	#if DoubleTrig
			
			Double thetaDbl = Double.Parse( theta.ToString(), Cult.InvariantCulture );
			for(Int32 r=0;r<20;r++) { // 20 iterations is enough, any more just yields inaccurate less-significant digits
				
				Double xPowerR = Math.Pow( thetaDbl, 2*r + 1 );
				Double factori = BigMath.Factorial( (double)( 2*r + 1 ) );
				
				Double element = xPowerR / factori;
				
				Double addThis = subtract ? -element : element;
				
				BigNum addThisBig = f.Create( addThis );
				
				retVal += addThisBig;
				
				subtract = !subtract;
			}
			
			
	#else
			
			for(Int32 r=0;r<_iterations;r++) {
				
				BigNum xPowerR = theta.Power(2*r +1);
				BigNum factori = Factorial( 2*r + 1);
				
				BigNum element = xPowerR / factori;
				
				retVal += subtract ? -element : element;
				
				subtract = !subtract;
			}
			
	#endif
			
			// TODO: This calculation generates useless and inaccurate trailing digits that must be truncated
			// so truncate them, when I figure out how many digits can be removed
			
			retVal.Truncate( 10 );
			
			return retVal;
			
		}
		
		public static BigNum Cos(BigNum theta) {
			
			// using Cos(x) == Sin(x + 90deg)
			
			theta += theta.Factory.HalfPi;
			
			return Sin( theta );
		}
		
		public static BigNum Tan(BigNum theta) {
			
			// using Tan(x) == Sin(x) / Cos(x)
			
			BigNum sine = Sin(theta);
			BigNum cosi = Cos(theta);
			
			return sine / cosi;
		}
		
		public static BigNum Csc(BigNum theta) {
			return Sin(theta).Power(-1);
		}
		
		public static BigNum Sec(BigNum theta) {
			return Cos(theta).Power(-1);
		}
		
		public static BigNum Cot(BigNum theta) {
			return Tan(theta).Power(-1);
		}
		
		// TODO: Arctan/Arcsin/Arccos
		
#endregion
		
	}
}
