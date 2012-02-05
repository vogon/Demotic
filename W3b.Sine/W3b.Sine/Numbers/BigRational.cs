using System;
using System.Collections.Generic;
using System.Text;

namespace W3b.Sine {
	
	/// <summary>Represents a rational number; that is, a real number that can be represented by a numerator and denominator integer pair.</summary>
	/// <remarks>For simple mathematical operations BigRational will be faster and more accurate than BigFloat, however the converse is also true: for more complicated floating-point operations use BigFloat.</remarks>
	public class BigRational : BigNum {
		
		private readonly BigInt _num;
		private readonly BigInt _den;
		
		public BigRational(BigInt num) {
			// an integer is represented by a fraction integer/1
			_num = num;
			_den = new BigInt(1);
		}
		
		public BigRational(BigFloat num) : this(num.ToString()) {
			
			// rather than use any implementation-specific method of getting the integer and non-int part of the number just use the string representation
			
		}
		
		public BigRational(String num) : this(num, 10) {
		}
		
		public BigRational(String num, int nBase) {
			
			// the algo to convert from a float into a rational isn't that complicated:
			
			// 1. Get the non-integer part of the number
			// 2. Assuming it's base 10, then:
			// 2.1 ...the numerator is the non-integer part converted to an integer, and the
			// 2.2 ...the denominator is an integer equal to 10^(length of non-integer part)
			// 2.3 ...then just normalise it
			// 3. Then you need to add the integer part back
			
			
			// HACK: String processing to get numbers: baaaaad
			
			int radixIdx = num.IndexOf('.');
			
			if( radixIdx == -1 ) {
				
				_num = new BigInt( num );
				_den = new BigInt(1);
				return;
				
			}
			
			String intPart = num.Substring(0, radixIdx);
			String fltPart = num.Substring( radixIdx );
			
			BigInt tempNumerator   = new BigInt( fltPart );
			BigInt tempDenominator = new BigInt( (int)Math.Pow( nBase, fltPart.Length ) );
			
			Normalise( tempNumerator, tempDenominator, out tempNumerator, out tempDenominator );
			
			// then add back the integer part
			
			BigRational intPortion = new BigRational( intPart );
			BigRational fltPortion = new BigRational( tempNumerator, tempDenominator );
			
			BigRational result = (BigRational)(intPortion + fltPortion);
			
			this._num = result._num;
			this._den = result._den;
			
		}
		
		private BigRational(BigInt num, BigInt den) {
			
			_num = num;
			_den = den;
		}
		
		public override BigNumFactory Factory {
			get { return BigRationalFactory.Instance; }
		}
		
		public override BigNum Clone() {
			
			BigInt newNumerator = (BigInt)this._num.Clone();
			BigInt newDenominia = (BigInt)this._den.Clone();
			
			return new BigRational( newNumerator, newDenominia );
		}
		
		public override int CompareTo(BigNum other) {
			
			BigRational rat = Ensure( other );
			
			// I think the best way to do this is just to evaluate the Rational...
			
			return Evaluate().CompareTo( rat.Evaluate() );
		}
		
		public override bool Equals(BigNum other) {
			// assuming both rationals are normalised, just check for equality of num and dec
			
			BigRational rat = Ensure( other );
			
			return _num.Equals( rat._num ) && _den.Equals( rat._den );
		}
		
		public override int GetHashCode() {
			return _num.GetHashCode() ^ _den.GetHashCode();
		}
		
		public BigFloat Evaluate() {
			
			BigFloat num = new BigFloat( _num.ToString() );
			BigFloat den = new BigFloat( _den.ToString() );
			
			return (BigFloat)(num / den);
		}
		
		public override string ToString() {
			
			return Evaluate().ToString();
		}
		
		public override bool IsZero {
			get { return _num.IsZero; }
		}
		
		public override bool Sign {
			get {
				return _num.Sign == _den.Sign;
			}
		}
		
		private static BigRational Ensure(BigNum number) {
			
			BigRational rational = number as BigRational;
			if( rational != null ) return rational;
			
			return (BigRational)BigRationalFactory.Instance.Create( number );
		}
		
		protected override BigNum Add(BigNum other) {
			
			return Add( this, Ensure( other ) );
		}
		
		protected override BigNum Multiply(BigNum multiplicand) {
			
			return Multiply( this, Ensure( multiplicand ) );
		}
		
		protected override BigNum Divide(BigNum divisor) {
			
			return Divide( this, Ensure( divisor ) );
		}
		
		protected override BigNum Negate() {
			
			BigInt num = (BigInt)(-_num);
			
			return new BigRational( num, (BigInt)_den.Clone() );
		}
		
		protected internal override BigNum Absolute() {
			
			if( _num.Sign == _den.Sign ) return Clone();
			
			// hold on, do I need to clone the BigInt instances since everything's immutable anyway?
			
			if( !_num.Sign ) {
				
				BigInt newNum = (BigInt)(-_num);
				BigInt newDen = (BigInt)_den.Clone();
				return new BigRational( newNum, newDen );
			} else { // then _den must be negative
				
				BigInt newNum = (BigInt)_num.Clone();
				BigInt newDen = (BigInt)(-_den);
				return new BigRational( newNum, newDen );
			}
			
		}
		
		protected internal override BigNum Floor() {
			throw new NotImplementedException();
		}
		
		protected internal override BigNum Ceiling() {
			throw new NotImplementedException();
		}
		
		protected internal override void Truncate(int significance) {
			throw new NotImplementedException();
		}
		
#region Operations
		
		private static void Normalise(BigInt oldNum, BigInt oldDen, out BigInt newNum, out BigInt newDen) {
			
			// TODO: Maybe also ensure that the denominator is positive, only the numerator can be negative
			
			// find the GCD of oldNum and oldDen, then apply it to find and return newNum and newDen
			
			BigInt gcd = (BigInt)BigMath.Gcd( oldNum, oldDen );
			
			newNum = (BigInt)(oldNum / gcd);
			newDen = (BigInt)(oldDen / gcd);
			
		}
		
		private static BigRational Add(BigRational x, BigRational y) {
			
			// the easiest way to do operations is by getting the simple denominator product
			// ...then normalising later
			
			// I think computing the LCM/GCD at this stage is premature and might hinder performance
			
			// a/b + c/d == (a*d)/(b*d) + (c*b)/(d*b) == (a*d)+(c*d) / (b*d)
			
			BigInt a = x._num;
			BigInt b = x._den;
			BigInt c = y._num;
			BigInt d = y._den;
			
			BigInt newNum = (BigInt)(a * d + c * d);
			BigInt newDen = (BigInt)(b * d);
			
			// then normalise
			
			Normalise(newNum, newDen, out newNum, out newDen);
			
			return new BigRational(newNum, newDen);
			
		}
		
		private static BigRational Multiply(BigRational x, BigRational y) {
			
			// a/b * c/d == (ac/bd)
			
			BigInt a = x._num;
			BigInt b = x._den;
			BigInt c = y._num;
			BigInt d = y._den;
			
			BigInt newNum = (BigInt)( a * c );
			BigInt newDen = (BigInt)( b * d );
			
			Normalise(newNum, newDen, out newNum, out newDen);
			
			return new BigRational(newNum, newDen);
			
		}
		
		private static BigRational Divide(BigRational x, BigRational y) {
			
			// a/b / c/d == a/b * d/c == (ad/bc)
			
			BigInt a = x._num;
			BigInt b = x._den;
			BigInt c = y._num;
			BigInt d = y._den;
			
			BigInt newNum = (BigInt)( a * b );
			BigInt newDen = (BigInt)( b * c );
			
			Normalise(newNum, newDen, out newNum, out newDen);
			
			return new BigRational(newNum, newDen);
			
		}
		
#endregion
		
	}
}
