using System;
using System.Collections.Generic;
using System.Text;

namespace W3b.Sine {
	
	public class BigIntFactory : BigNumFactory {
		
#region Singleton Pattern
		
		private static BigIntFactory _this;
		
		private BigIntFactory() {
		}
		
		public static BigIntFactory Instance {
			get {
				if( _this == null ) _this = new BigIntFactory();
				return _this;
			}
		}
		
#endregion
		
		public override BigNum Create(long value) {
			
			return new BigInt( value );
		}
		
		public override BigNum Create(decimal value) {
			throw new NotSupportedException();
		}
		
		public override BigNum Create(string value) {
			
			return new BigInt( value );
		}
		
		public override BigNum Create(BigNum value) {
			throw new NotImplementedException();
		}
		
	}
}
