using System;
using System.Collections.Generic;
using System.Text;

using Cult = System.Globalization.CultureInfo;

namespace W3b.Sine {
	
	/// <summary>Implements arbitrary-precsision floating-point arithmetic with a decimal byte array.</summary>
	public class BigFloat : BigNum {
		
		private List<SByte> _data;
		private Boolean     _sign;
		/// <summary>The exponential part.</summary>
		/// <remarks>Auto-initialised to 0.</remarks>
		private Int32       _exp;
		
#region Options
		
		/// <summary>Whether to print + in ToString if the value is positive.</summary>
		/// <remarks>Auto-initialised to 0.</remarks>
		private static Boolean _printPositiveSign;
		/// <summary>Number of digits to get when dividing.</summary>
		private static Int32   _divisionDigits     = 100;
//		/// <summary>Number of radix digits to display when calling ToString().</summary>
//		private static Int32   _toStringRadixLimit = 20;
		
#endregion
		
		private readonly static Char[] _digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
		
		/// <summary>Creates a new BigFloat with an empty data list. Not identical to zero.</summary>
		public BigFloat() {
			_data = new List<SByte>();
			_sign = true;
		}
		
		public BigFloat(Int64 value) : this() {
			Load( value );
		}
		
		public BigFloat(Double value) : this() {
			Load( value );
		}
		
		public BigFloat(String value) : this() {
			Load(value);
		}
		
		public override BigNumFactory Factory {
			get { return BigFloatFactory.Instance; }
		}
		
		protected void Load(String textRepresentation) {
			_data.Clear();
			
			// assuming the string has already been tokenized, expression parsing happens elsewhere
			textRepresentation = textRepresentation.Replace(" ", "").ToUpper(Cult.InvariantCulture);
			
			if(textRepresentation.Length == 0) {
				_data.Add( 0 );
				_exp = 0;
				_sign = true;
				return;
			}
			
			// get the exponent part
			String[] splitAtE = textRepresentation.Split('E');
			if( splitAtE.Length > 1 ) {
				textRepresentation = splitAtE[0];
				String exponentPart = splitAtE[1];
				_exp = Convert.ToInt32( exponentPart, Cult.InvariantCulture );
			}
			
			Boolean hasDot = false;
			for(int i=textRepresentation.Length-1;i>=0;i--) {
				Char c = textRepresentation[i];
				
				if( c >= '0' && c <= '9' ) {
					
					_data.Add( Convert.ToSByte( CharToSByte(c) ) );
					
				} else if(c == '+' || c == '-') {
					
					_sign = c == '+';
					
				} else if(c == '.') {
					
					if(hasDot) throw new FormatException("Too many '.' characters.");
					if(!hasDot) hasDot = true;
					
					_data.Add( -1 ); // special marker -1
					
					
				} else {
					throw new FormatException("Invalid character detected.");
				}
				
			}
			
			// correct exponent, look for the marker -1
			Int32 idx = _data.IndexOf( -1 );
			if( idx != -1 ) {
				_data.RemoveAt( idx );
				_exp -= idx;
			}
			
		}
		
		protected void Load(Int64 value) {
			
			// cheat by getting .NET to convert to string
			Load( value.ToString(System.Globalization.CultureInfo.InvariantCulture) );
			
		}
		protected void Load(Double value) {
			
			// cheat by getting .NET to convert to string
			Load( value.ToString(System.Globalization.CultureInfo.InvariantCulture) );
		}
		
		public static Boolean PrintPositiveSign {
			get { return _printPositiveSign; }
			set { _printPositiveSign = value; }
		}
		
#region Overriden Methods
		
		protected override BigNum Add(BigNum other) {
			return BigFloat.Add( this, (BigFloat)other );
		}
			
		protected override BigNum Multiply(BigNum multiplicand) {
			return BigFloat.Multiply(this, (BigFloat)multiplicand);
		}
		
		protected override BigNum Divide(BigNum divisor) {
			return BigFloat.Divide(this, (BigFloat)divisor);
		}
		
		public override Int32 CompareTo(BigNum other) {
			
			BigFloat o = other as BigFloat;
			
			if( o == null ) {
				
				o = (BigFloat)Factory.Create( other );
			}
			
			return CompareTo( o );
		}
		
		public Int32 CompareTo(BigFloat o) {
			
			if( Equals(o) ) return 0;
			
			if(Sign && !o.Sign) return 1;
			else if(!Sign && !o.Sign) return -1;
			
			AlignExponent(o);
			o.AlignExponent(this);
			
			Int32 expDifference = ( _exp + _data.Count ) - ( o._exp + o._data.Count );
			if(expDifference > 0) return 1;
			else if(expDifference < 0) return -1;
			
			// by now both will have the same length
			for(int i=Length-1;i>=0;i--) {
				if( this[i] > o[i] ) return 1;
				if( this[i] < o[i] ) return -1;
			}
			
			return 0;
			
		}
		
		protected override BigNum Negate() {
			BigFloat result = (BigFloat)Clone();
			result._sign = !result.Sign;
			return result;
		}
		
		protected internal override BigNum Absolute() {
			BigFloat dolly = (BigFloat)Clone();
			dolly._sign = true;
			return dolly;
		}
		
		protected internal override BigNum Floor() {
			return BigFloat.Subtract( this, GetFractionalPart() );
		}
		
		protected internal override BigNum Ceiling() {
			// using the identity ceil(x) == -floor(-x)
			BigFloat floored = (BigFloat)Negate().Floor();
			return floored.Negate();
		}
		
		public override Boolean IsZero {
			get {
				return Length == 0;
			}
		}
		
		public override BigNum Clone() {
			BigFloat dolly = new BigFloat();
			dolly._sign = Sign;
			dolly._exp = _exp;
			dolly._data.AddRange( _data );
			return dolly;
		}
		
		public override Boolean Equals(Object obj) {
			// remember, this is value equality and two BigNums (even if they're of different subclass) can have an equal value
			if( obj == null ) return false;
			
			if( obj is BigNum ) {
				BigFloat bn = obj as BigFloat;
				return bn == null ? false : Equals( bn );
			}
			
			String s = obj.ToString();
			BigFloat b = new BigFloat( s );
			
			return Equals( b );
			
		}
		public Boolean Equals(BigFloat other) {
			
			if( other == null ) return false;
			if(Object.ReferenceEquals(this, other)) return true;
			
			if(Sign != other.Sign) return false;
			if(_exp != other._exp) return false;
			if(Length != other.Length) return false;
			
			for(int i=0;i<Length;i++) {
				if(this[i] != other[i]) return false;
			}
			
			return true;
			
		}
		public override bool Equals(BigNum other) {
			return Equals( other as BigFloat );
		}
		
		public override Int32 GetHashCode() {
			return _data.GetHashCode() ^ _exp.GetHashCode() ^ _sign.GetHashCode();
		}
		
		public override String ToString() {
			
			StringBuilder sb = new StringBuilder(_data.Count);
			if(_printPositiveSign) {
				sb.Append( Sign ? '+' : '-' );
			} else {
				if(!Sign) sb.Append( '-' );
			}

// commented out the ToString limit because it interferes with some calculations (such as operations that (mistakenly) use the string representation			
//			Int32 nofRadixDigits = 0;
			for(int i=Length-1;i>=0;i--) {
				
				if( i + _exp + 1 == 0 ) { // if reached radix point
					sb.Append('.');
				}
				sb.Append( _digits[ _data[i] ] );
				
//				nofDigits++;
//				if(nofDigits > _toStringRadixLimit) break;
			}
			
			if(Length == 0) sb.Append("0");
			
			if( _exp > 0 ) {
				sb.Append('E');
				sb.Append( _exp > 0 ? '+' : '-');
				sb.Append(_exp);
			}
			
			return sb.ToString();
			
		}
		
		private enum StringParserState {
			Sign,
			IntegerPart,
			FractionalPart,
			Exponent
		}
		
		private static SByte CharToSByte(Char c) {
			for(sbyte i=0;i<=_digits.Length;i++) {
				if(c == _digits[i]) return i;
			}
			return -2;
		}
		
		public override Boolean Sign {
			get { return _sign; }
		}
		
#endregion
		
#region Operation Implementation
		// there really needs to be a new feature in OOP where interfaces can define static methods without having to use factory classes or the singleton pattern
		
		public static BigFloat Add(BigFloat a, BigFloat b) {
			return Add(a, b, true);
		}
		
		private static BigFloat Add(BigFloat a, BigFloat b, Boolean normalise) {
			
			a.Normalise();
			b.Normalise();
			
			a = (BigFloat)a.Clone();
			b = (BigFloat)b.Clone();
			
			if(a.IsZero && b.IsZero) return a;
			if(a.Sign && !b.Sign) {
				b._sign = true;
				return Subtract(a, b, normalise);
			}
			if(!a.Sign && b.Sign) {
				b._sign = true;
				return Subtract(b, a, normalise);
			}
			
			BigFloat result = new BigFloat();
			
			a.AlignExponent( b );
			b.AlignExponent( a );
			
			result._exp = a._exp;
			
			if(b.Length > a.Length) { // then switch them around
				BigFloat temp = a;
				a = b;
				b = temp;
			}
			
			// the work:
			// 
			SByte digit = 0;
			SByte carry = 0;
			for(int i=0;i<b.Length;i++) {
				digit = (SByte)( ( a[i] + b[i] + carry ) % 10 );
				carry = (SByte)( ( a[i] + b[i] + carry ) / 10 );
				result._data.Add( digit );
			}
			for(int i=b.Length;i<a.Length;i++) {
				digit = (SByte)( ( a[i] + carry ) % 10 );
				carry = (SByte)( ( a[i] + carry ) / 10 );
				result._data.Add( digit );
			}
			if( carry > 0 ) result._data.Add( carry );
			
			result._sign = a.Sign && b.Sign;
			
/*			if(normalise) // normalising shouldn't be necessary, but what the heck
				result.Normalise(); */
			
			return result;
			
		}
		
		public static BigFloat Subtract(BigFloat a, BigFloat b) {
			return Subtract(a, b, true);
		}
		
		private static BigFloat Subtract(BigFloat a, BigFloat b, Boolean normalise) {
			
			a.Normalise();
			b.Normalise();
			
			a = (BigFloat)a.Clone();
			b = (BigFloat)b.Clone();
			
			if( a.IsZero && b.IsZero ) return a;
			if( a.Sign && !b.Sign ) {
				b._sign = true;
				return Add(a, b, normalise);
			}
			if( !a.Sign && b.Sign ) {
				a._sign = true;
				BigFloat added = Add(a, b, normalise);
				return (BigFloat)added.Negate();
			}
			
			
			BigFloat result = new BigFloat();
			
			a.AlignExponent( b );
			b.AlignExponent( a );
			
			result._exp = a._exp;
			
			Boolean wasSwapped = false;
			if(b.Length > a.Length) { // then switch them around
				BigFloat temp = a;
				a = b;
				b = temp;
				wasSwapped = true;
			} else {
				// if same length, check magnitude
				BigFloat a1 = (BigFloat)a.Absolute();
				BigFloat b1 = (BigFloat)b.Absolute();
				if(a1 < b1) {
					BigFloat temp = a;
					a = b;
					b = temp;
					wasSwapped = true;
				} else if( !(a1 > b1) ) { // i.e. equal
					return new BigFloat(); // return zero
				}
			}
			
			// Do work
			// it's a sad day when the preparation for an operation is just as long as the operation itself
			
			
			SByte digit = 0;
			SByte take = 0;
			
			for(int i=0;i<b.Length;i++ ) {
				digit = (SByte)( a[i] - b[i] - take );
				if( digit < 0 ) {
					take = 1;
					digit = (SByte)( 10 + digit );
				} else {
					take = 0;
				}
				result._data.Add( digit );
			}
			
			for(int i=b.Length;i<a.Length;i++ ) {
				digit = (SByte)( a[i] - take );
				if( digit < 0 ) {
					take = 1;
					digit = (SByte)( 10 + digit );
				} else {
					take = 0;
				}
				result._data.Add( digit );
			}
			
			result._sign = a.Sign && b.Sign ? !wasSwapped : wasSwapped;
			
/*			if(normalise)
				result.Normalise(); */
			
			return result;
		}
		
		public static BigFloat Multiply(BigFloat a, BigFloat b) {
			
			a = (BigFloat)a.Clone();
			b = (BigFloat)b.Clone();
			
			if(b.Length > a.Length) { // then switch them around
				BigFloat temp = a;
				a = b;
				b = temp;
			}
			
			BigFloat retval = new BigFloat();
			List<BigFloat> rows = new List<BigFloat>();
			
			retval._sign = a.Sign == b.Sign;
			retval._exp  = a._exp + b._exp;
			
			// for each digit in b
			for(int i=0;i<b.Length;i++) {
				
				BigFloat row = new BigFloat();
				row._exp = retval._exp;
				
				Int32 digit = 0, carry = 0;
				for(int exp=0;exp<i;exp++) row._data.Add( 0 ); // pad with zeros to the right
				
				for(int j=0;j<a.Length;j++) { // perform per-digit multiplication of a against b
					digit = a[j] * b[i] + carry;
					carry = digit / 10;
					digit = digit % 10;
					row._data.Add( (SByte)digit );
				}
				
				// reduce the carry
				while(carry > 0) {
					digit = carry % 10;
					carry = carry / 10;
					row._data.Add( (SByte)digit );
				}
				
				rows.Add( row );
				
			}
			
			// sum the rows to give the result
			foreach(BigFloat row in rows) {
				retval  = (BigFloat)retval.Add(row);
			}
			
			retval.Normalise();
			
			return retval;
			
		}
		
		public static BigFloat Divide(BigFloat dividend, BigFloat divisor) {
			Boolean wasExact;
			return Divide(dividend, divisor, out wasExact);
		}
		
		public static BigFloat Divide(BigFloat dividend, BigFloat divisor, out Boolean isExact) {
			
			if(divisor.IsZero) throw new DivideByZeroException();
			
			isExact = true;
			if(dividend.IsZero) return (BigFloat)dividend.Clone();
			
			///////////////////////////////
			
			BigFloat quotient = new BigFloat();
			quotient._sign = dividend.Sign == divisor.Sign;
			
			dividend = (BigFloat)dividend.Absolute();
			divisor  = (BigFloat)divisor.Absolute();
			
			BigFloat aPortion;
			BigFloat bDigit = null;
			
			//////////////////////////////
			
			while( divisor[0] == 0 ) { // remove least-significant zeros and up the exponent to compensate
				divisor._exp++;
				divisor._data.RemoveAt(0);
			}
			quotient._exp = dividend.Length + dividend._exp - divisor.Length - divisor._exp + 1;
			dividend._exp = 0;
			divisor._exp  = 0;
			
			aPortion = new BigFloat();
			
			Int32 bump = 0, c = -1;
			Boolean hump = false;
			
			isExact = false;
			
			while( quotient.Length < _divisionDigits ) { // abandon hope all ye who enter here
				
				aPortion = dividend.Msd( divisor.Length + ( hump ? 1 : 0 ) );
				
				if( aPortion < divisor ) {
					int i = 1;
					while( aPortion < divisor ) {
						aPortion = dividend.Msd( divisor.Length + i + ( hump ? 1 : 0 ) );
						quotient._exp--;
						quotient._data.Add( 0 );
						i++;
					}
					hump = true;
				}
				
				bDigit = 0; //tt = 0;
				c = 0;
				while( bDigit < aPortion ) {
					bDigit = (BigFloat)bDigit.Add(divisor); //tt += b;
					c++;
				}
				if( bDigit != aPortion ) {
					c--;
					bDigit = new BigFloat( c );
					bDigit = (BigFloat)bDigit.Multiply(divisor); //tt *= b;
					isExact = false;
				} else {
					isExact = true;
				}
				
				quotient._data.Add( (sbyte)c );
				quotient._exp--;
				
				aPortion.Normalise();
				dividend.Normalise();
				
				bump = aPortion.Length - bDigit.Length;
				if( aPortion.Length > dividend.Length ) dividend = aPortion;
				bDigit = bDigit.Msd( dividend.Length - bump );
				
				aPortion = BigFloat.Add(dividend, (BigFloat)bDigit.Negate(), false );
				
				dividend = (BigFloat)aPortion.Clone();
				
				if( aPortion.IsZero ) break; // no more work necessary
				
				if( hump ) {
					if( dividend[dividend.Length - 1] == 0 ) {
						dividend._data.RemoveAt( dividend.Length - 1 );
					}
				}
				if( dividend[dividend.Length - 1] == 0 ) {
					dividend._data.RemoveAt( dividend.Length - 1 );
					
					while( dividend[dividend.Length - 1] == 0 ) {
						quotient._exp--;
						quotient._data.Add( 0 );
						dividend._data.RemoveAt( dividend.Length - 1 );
						if( dividend.Length == 0 ) break;
					}
					if( dividend.Length == 0 ) break;
					hump = false;
				} else {
					hump = true;
				}
				
				if( quotient.Length == 82 ) {
					c = 0;
				}
			}
			
			quotient._data.Reverse();
			quotient.Normalise();
			
			return quotient;
			
		}
		
		protected internal override void Truncate(Int32 significance) {
			
			Normalise();
			
			if( _data.Count > significance ) {
				
				_data.RemoveRange( 0, _data.Count - significance );
				
				this._exp = -significance;
			}
			
		}
		
#endregion
		
#region Utility Methods and Private Properties
		
		/// <summary>Nabs the most significant digits of this number. If <paramref name="count" /> is greater than the length of the number it pads zeros.</summary>
		/// <param name="count">Number of digits to return</param>
		private BigFloat Msd(Int32 count) {
			BigFloat retVal = new BigFloat();
			if(count > Length) {
/*				Int32 i = 0;
				while( i < count - Length) {
					retVal._data.Add(0);
					i++;
				}*/
				for(int i=0;i<count-Length;i++) {
					retVal._data.Add( 0 );
				}
				count = Length;
			}
			for(int i=0;i<count;i++) {
				retVal._data.Add( this[Length - count + i] );
			}
			return retVal;
		}
		
		/// <summary>Makes the exponent values of both numbers equal by padding zeros.</summary>
		private void AlignExponent(BigFloat withThis) {
			while( _exp > withThis._exp ) {
				_exp--;
				_data.Insert( 0, 0 );
			}
		}
		
		/// <summary>Removes chaff elements from the internal list.</summary>
		private void Normalise() {
			for(int i=Length-1;i>=0;i--) {
				if( _data[i] != 0 ) break;
				_data.RemoveAt( i );
			}
			if( Length == 0 ) _sign = true; // can't have "negative zero"
			/*else {
				while( _data[0] == 0 ) { // removes least-significant zeros
					_data.RemoveAt(0);
					_exp++;
				}
			}*/
			
		}
		
		private Int32 Length { get { return _data.Count; } }
		
		private SByte this[Int32 index] {
			get { return _data[index]; }
		}
		
		private BigFloat GetFractionalPart() {
			
			if(_exp == 0) return 0;
			
			Normalise();
			
			BigFloat retVal = new BigFloat();
			for(int i=0;i<Length && i<-_exp;i++) { // get all the digits to the right of the radix point
				retVal._data.Add( this[i] );
			}
			retVal._exp = _exp;
			
			return retVal;
			
		}
		
		public static implicit operator BigFloat(String s) {
			if(s == null) return null;
			return new BigFloat(s);
		}
		
		public static implicit operator BigFloat(Int64 i) {
			return new BigFloat(i);
		}
		
#endregion
		
	}
	
}
