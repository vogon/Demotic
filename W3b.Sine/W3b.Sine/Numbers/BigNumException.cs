using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;

namespace W3b.Sine {
	
	[Serializable]
	public class BigNumException : Exception {
		
		public BigNumException() {
		}
		
		public BigNumException(String message) : base(message) {
		}
		
		public BigNumException(String message, Exception inner) : base(message, inner) {
		}
		
		protected BigNumException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
		
	}
	
}
