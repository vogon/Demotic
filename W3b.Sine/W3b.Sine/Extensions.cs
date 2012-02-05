using System;
using System.Text;
using System.Globalization;
using System.Collections;

// Extension methods seem to require System.Core.dll, which is not in .NET2.0
// so here's an ersatz Extension attribute class

namespace System.Runtime.CompilerServices {
	
	[AttributeUsage(AttributeTargets.Method)]
	internal sealed class ExtensionAttribute : Attribute {
		
		public ExtensionAttribute() {
		}
		
	}
	
}

namespace W3b.Sine {
	
	internal static class Extensions {
		
		/// <summary>Returns the next token in <paramref name="str"/> from position <paramref name="start"/>. Barring whitespace, it returns the sequence of characters from <paramref name="start"/> that share the same unicode category.</summary>
		public static String Tok(this String str, ref Int32 start) {
			
			if(start >= str.Length) throw new ArgumentOutOfRangeException("start");
			
			Int32 i = start;
			
			StringBuilder sb = new StringBuilder();
			
			Char c = str[i];
			
			// skip whitespace
			while( i < str.Length ) {
				
				c = str[ i ];
				if( !Char.IsWhiteSpace( c ) ) break;
				i++;
			}
			if( Char.IsWhiteSpace( c ) ) {
				start = str.Length;
				return null; // then it's got some trailing whitespace left, and it's at the end
			}
			
			Boolean doneRadixPoint = false;
			UnicodeCategory currentCat, initialCat;
			Char initialChar = c;
			initialCat = currentCat = Char.GetUnicodeCategory( c );
			
			// special case for ( and )
			if( initialChar == '(' || initialChar == ')' ) {
				start = i + 1;
				return initialChar.ToString();
			}
			
			while( i < str.Length && CategoryEquals(initialCat, currentCat) ) {
				
				c = str[i];
				sb.Append( str[i++] );
				
				if( i >= str.Length ) break;
				
				currentCat = Char.GetUnicodeCategory(str, i);
				
				// special case exception for radix points, there can only be 1
				if( initialCat == UnicodeCategory.DecimalDigitNumber && str[i] == '.' && !doneRadixPoint) {
					currentCat = UnicodeCategory.DecimalDigitNumber;
					doneRadixPoint = true;
				}
				
			}
			
			start = i;
			
			return sb.ToString();
			
		}
		
		private static Boolean CategoryEquals(UnicodeCategory x, UnicodeCategory y) {
			
			if( (int)x <= 2 && (int)y <= 2 ) return true; // both x and y are letters of any case
			
			return x == y;
			
		}
		
		public static String ToStringList(this IEnumerable enumerable) {
			
			StringBuilder sb = new StringBuilder();
			bool first = true;
			
			foreach(Object o in enumerable) {
				
				if( !first ) sb.Append(", ");
				first = false;
				
				sb.Append( o.ToString() );
			}
			
			return sb.ToString();
		}
	}
}
