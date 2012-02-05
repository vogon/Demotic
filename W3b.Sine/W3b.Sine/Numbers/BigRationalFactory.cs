using System;
using System.Collections.Generic;
using System.Text;

namespace W3b.Sine {
	
	public class BigRationalFactory : BigNumFactory {
		
#region Singleton Pattern
		
		private static BigRationalFactory _this;
		
		private BigRationalFactory() {
		}
		
		public static BigRationalFactory Instance {
			get {
				if( _this == null ) _this = new BigRationalFactory();
				return _this;
			}
		}
		
#endregion
		
		public override BigNum Create(long value) {
			
			return new BigRational( value.ToString() );
		}
		
		public override BigNum Create(decimal value) {
			
			return new BigRational( value.ToString() );
		}
		
		public override BigNum Create(string value) {
			
			return new BigRational( value );
		}
		
		public override BigNum Create(BigNum value) {
			throw new NotImplementedException();
		}
		
	}
}
