using System;
using System.Collections.Generic;
using System.Text;

namespace W3b.Sine {
	
	public abstract class BigNumFactory {
		
		public abstract BigNum Create(Int64 value);
		
		[CLSCompliant(false)]
		public virtual BigNum Create(UInt64 value) { return Create( (long)value ); }
		
		[CLSCompliant(false)]
		public virtual BigNum Create(UInt32 value) { return Create( (long)value ); }
		public virtual BigNum Create( Int32 value) { return Create( (long)value ); }
		
		[CLSCompliant(false)]
		public virtual BigNum Create(UInt16 value) { return Create( (long)value ); }
		public virtual BigNum Create( Int16 value) { return Create( (long)value ); }
		
		[CLSCompliant(false)]
		public virtual BigNum Create(SByte value)  { return Create( (long)value ); }
		public virtual BigNum Create( Byte value)  { return Create( (long)value ); }
		
		/////////////////////////////////////////
		
		public abstract BigNum Create(Decimal value);
		
		public virtual BigNum Create(Double value) { return Create( (decimal)value ); }
		public virtual BigNum Create(Single value) { return Create( (decimal)value ); }
		
		/////////////////////////////////////////
		
		public abstract BigNum Create(String value);
		
		/// <summary>Converts a BigNum from one internal representation to another. If the type is the same then it returns a clone.</summary>
		public abstract BigNum Create(BigNum value);
		
		/////////////////////////////////////////
		
		public virtual Boolean TryParse(String value, out BigNum number) {
			
			try {
				
				number = Create( value );
				
			} catch(FormatException) {
				
				number = null;
				return false;
			}
			
			return true;
		}
		
#region BigNum Mathematical Constants
		
		private BigNum _cUnity;
		private BigNum _cZero;
		private BigNum _cHalfPi;
		private BigNum _cPi;
		private BigNum _cTwoPi;
		private BigNum _cEulerMascheroni;
		private BigNum _cEuler;
		private BigNum _cSqrtTwo;
		private BigNum _cGoldenRatio;
		private BigNum _cDegree;
		
		public BigNum Unity           { get { if( _cUnity           == null ) _cUnity           = this.Create(UnityStr);           return _cUnity; } }
		public BigNum Zero            { get { if( _cZero            == null ) _cZero            = this.Create(ZeroStr);            return _cZero; } }
		public BigNum HalfPi          { get { if( _cHalfPi          == null ) _cHalfPi          = this.Create(HalfPiStr);          return _cHalfPi; } }
		public BigNum Pi              { get { if( _cPi              == null ) _cPi              = this.Create(PiStr);              return _cPi; } }
		public BigNum TwoPi           { get { if( _cTwoPi           == null ) _cTwoPi           = this.Create(TwoPiStr);           return _cTwoPi; } }
		public BigNum EulerMascheroni { get { if( _cEulerMascheroni == null ) _cEulerMascheroni = this.Create(EulerMascheroniStr); return _cEulerMascheroni; } }
		public BigNum Euler           { get { if( _cEuler           == null ) _cEuler           = this.Create(EulerStr);           return _cEuler; } }
		public BigNum SqrtTwo         { get { if( _cSqrtTwo         == null ) _cSqrtTwo         = this.Create(SqrtTwoStr);         return _cSqrtTwo; } }
		public BigNum GoldenRatio     { get { if( _cGoldenRatio     == null ) _cGoldenRatio     = this.Create(GoldenRatioStr);     return _cGoldenRatio; } }
		public BigNum Degree          { get { if( _cDegree          == null ) _cDegree          = this.Create(DegreeStr);          return _cDegree; } }
		
		// I'll resist the temptation to put down a bunch of Physical constants, arguably that's the concern of any user of this library
		
		// Various mathematical consts computed by Mathematica 7.0, using N[const,66]
		// It's often said that not even Wolfram Mathematica is powerful enough to compute the size of Stephen Wolfram's ego
		
		protected const String UnityStr           = "1";
		protected const String ZeroStr            = "0";
		protected const String HalfPiStr          = "1.57079632679489661923132169163975144209858469968755291048747229615";
		protected const String PiStr              = "3.14159265358979323846264338327950288419716939937510582097494459231";
		protected const String TwoPiStr           = "6.28318530717958647692528676655900576839433879875021164194988918462";
		protected const String EulerMascheroniStr = "0.57721566490153286060651209008240243104215933593992359880576723488";
		protected const String EulerStr           = "2.71828182845904523536028747135266249775724709369995957496696762772";
		protected const String SqrtTwoStr         = "1.41421356237309504880168872420969807856967187537694807317667973799";
		protected const String GoldenRatioStr     = "1.61803398874989484820458683436563811772030917980576286213544862271";
		protected const String DegreeStr          = "0.01745329251994329576923690768488612713442871888541725456097191440";
		
#endregion
		
	}
	
	
	
}
