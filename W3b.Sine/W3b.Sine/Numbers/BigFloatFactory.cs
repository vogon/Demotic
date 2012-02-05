using System;
using System.Collections.Generic;
using System.Text;

namespace W3b.Sine {
	
	public class BigFloatFactory : BigNumFactory {
		
#region Singleton Pattern
		
		private static BigFloatFactory _this;
		
		private BigFloatFactory() {
		}
		
		public static BigFloatFactory Instance {
			get {
				if( _this == null ) _this = new BigFloatFactory();
				return _this;
			}
		}
		
#endregion
		
		public override BigNum Create(long value) {
			
			return new BigFloat( value );
		}
		
		public override BigNum Create(decimal value) {
			
			return new BigFloat( (double)value ); // TODO: Change BigFloat to support Decimal
		}
		
		public override BigNum Create(String value) {
			
			return new BigFloat( value );
		}
		
		public override BigNum Create(BigNum value) {
			throw new NotImplementedException();
		}
		
	}
}
