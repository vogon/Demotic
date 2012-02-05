using System;
using System.Collections.Generic;
using System.Text;

using Cult = System.Globalization.CultureInfo;

namespace W3b.Sine {
	
	/// <summary>The base class for all immutable, non-limited (arbitrary or infinite) prescision number implementations. It defines a common interface and provides various utility methods.</summary>
	public abstract class BigNum : IComparable, IComparable<BigNum>, IEquatable<BigNum>, ICloneable {
		
#region Equality and Comparisons
		
		public abstract BigNum Clone();
		
		Object ICloneable.Clone() {
			return Clone();
		}
		
		public virtual Int32 CompareTo(Object obj) {
			BigNum num = obj as BigNum;
			return num == null ? -1 : CompareTo( num );
		}
		
		/// <summary>Performs a numeric value comparison.</summary>
		/// <remarks>If the BigNum type passed in is not the same type as 'this' the comparison must still be carried out.</remarks>
		public abstract          Int32  CompareTo(BigNum other);
		
		public Boolean Equals(Int16  x) { return Equals( Factory.Create( x ) ); }
		public Boolean Equals(Int32  x) { return Equals( Factory.Create( x ) ); }
		public Boolean Equals(Int64  x) { return Equals( Factory.Create( x ) ); }
		public Boolean Equals(Single x) { return Equals( Factory.Create( x ) ); }
		public Boolean Equals(Double x) { return Equals( Factory.Create( x ) ); }
		
		public override Boolean Equals(Object obj) {
			BigNum num = obj as BigNum;
			if( num == null ) num = Factory.Create( obj.ToString() );
			return Equals( num );
		}
		
		/// <summary>Performs a numeric value equality comparison.</summary>
		/// <remarks>If the BigNum type passed in is not the same type as 'this' the equality comparison must still be carried out.</remarks>
		public abstract          Boolean Equals(BigNum other);
		
		public abstract override Int32   GetHashCode();
		public abstract override String  ToString();
		
#endregion
		
#region Operations
	
	#region Must Implement
		
		public abstract Boolean IsZero { get; }
		/// <summary>Returns true if this number is positive. This library respects the notion of signed-zero. If this is undefined in the number type then zero is always assumed to be positive.</summary>
		public abstract Boolean Sign   { get; }
		
		protected abstract BigNum Add(BigNum other);
		
		protected abstract BigNum Multiply(BigNum multiplicand);
		
		/// <summary>Divides this instance by the divisor, returning the quotient.</summary>
		protected abstract BigNum Divide(BigNum divisor);
		
		/// <summary>Inverts this instance's sign</summary>
		protected abstract BigNum Negate();
		
		/// <summary>Returns the norm of the number</summary>
		protected internal abstract BigNum Absolute();
		
		protected internal abstract BigNum Floor();
		protected internal abstract BigNum Ceiling();
		
		/// <summary>Removes digit places beyond the specified figure of significance</summary>
		protected internal abstract void Truncate(Int32 significance);
		
	#endregion
	
	#region Overridable
		
		/// <summary>Divides this instance by the divisor, returning the remainder.</summary>
		protected virtual BigNum Modulo(BigNum divisor) {
			
			return Modulo(this, divisor);
		}
		
		private static BigNum Modulo(BigNum x, BigNum y) {
			
			// a % b == a  - ( b * Floor[a / b] )
			
			if(y == 0) throw new DivideByZeroException("Divisor y cannot be zero");
			if(x.IsZero) return new BigFloat(0);
			
			BigNum floored = BigMath.Floor( x / y );
			BigNum multbyb = y * floored;
			BigNum retval  = x - multbyb;
			
			return retval;
		}
		
		protected virtual BigNum Subtract(BigNum other) {
			return Add( other.Negate() );
		}
		
		protected internal virtual BigNum Power(Int32 exponent) {
			
			Int32 pow = exponent < 0 ? -exponent : exponent; // abs(exponent);
			
			BigNum unity = this.Factory.Unity;
			
			BigNum retVal = unity;
			for(int i=0;i<pow;i++) {
				retVal = retVal.Multiply( this );
			}
			
			if(exponent < 0) { // reciprocal
				retVal = unity / retVal;
			}
			
			return retVal;
			
		}
		
	#endregion
		
#endregion
		
#region Creation
		
		public abstract BigNumFactory Factory {
			get;
		}
		
		public static implicit operator String(BigNum n) {
			return n.ToString();
		}
		
#endregion
		
#region Operators
		
		public static BigNum operator +(BigNum a, BigNum b) {
			if(a == null) return null;
			if(b == null) return null;
			return a.Add(b);
		}
		
		public static BigNum operator -(BigNum a) {
			if(a == null) return null;
			return a.Negate();
		}
		
		public static BigNum operator -(BigNum a, BigNum b) {
			if(a == null) return null;
			if(b == null) return null;
			return a.Add( b.Negate() );
		}
		
		public static BigNum operator *(BigNum a, BigNum b) {
			if(a == null) return null;
			if(b == null) return null;
			return a.Multiply(b);
		}
		
		public static BigNum operator /(BigNum a, BigNum b) {
			if(a == null) return null;
			if(b == null) return null;
			return a.Divide(b);
		}
		
		public static BigNum operator %(BigNum a, BigNum b) {
			if(a == null) return null;
			if(b == null) return null;
			return a.Modulo(b);
		}
		
		public static BigNum operator ^(BigNum a, Int32 b) {
			if(a == null) return null;
			return a.Power( b );
		}
		
		public static Boolean operator ==(BigNum a, BigNum b) {
			return Equals(a, b);
		}
		
		public static Boolean operator !=(BigNum a, BigNum b) {
			return !Equals(a, b);
		}
		
		public static Boolean operator <(BigNum a, BigNum b) {
			if(a == null && b == null) return false;
			if(a == null) return true;
			if(b == null) return false;
			return a.CompareTo(b) == -1;
		}
		
		public static Boolean operator <=(BigNum a, BigNum b) {
			if(a == null && b == null) return true;
			if(a == null) return true;
			if(b == null) return false;
			Int32 result = a.CompareTo(b);
			return result == -1 || result == 0;
		}
		
		public static Boolean operator >(BigNum a, BigNum b) {
			if(a == null && b == null) return false;
			if(a == null) return true;
			if(b == null) return false;
			return b.CompareTo(a) == -1;
		}
		
		public static Boolean operator >=(BigNum a, BigNum b) {
			if(a == null && b == null) return true;
			if(a == null) return true;
			if(b == null) return false;
			Int32 result = b.CompareTo(a);
			return result == -1 || result == 0;
		}
		
		public static Boolean operator ==(BigNum a, Int64 b) {
			return Equals(a, b);
		}
		public static Boolean operator !=(BigNum a, Int64 b) {
			return !Equals(a, b);
		}
		
#endregion
		
	}
}
