//#define VisibleInternals

using System;
using System.Collections.Generic;
using System.Text;

using S    = System.Runtime.Serialization;
using N    = System.Globalization.NumberStyles;
using Cult = System.Globalization.CultureInfo;
using P    = W3b.Sine.Precedence;

namespace W3b.Sine {
	
	/// <summary>A C# implementation of Tom Niemann's Operator Precedence Parsing system ( http://epaperpress.com/oper/index.html ).</summary>
	public partial class Expression {
		
		// The precedence table; beautiful, isn't it?
/*
		S = Shift. The input takes precedence over what's at the top of the stack
		R = Reduce. The stack should be evaluated before the input is processed.
		      |                                   input                                                |
		      | +   -   *   /   ^   M   ,     (   )     ==  !=  <   <=  >   >=    &&  ||  !   ^^    $  |
		   ---| --  --  --  --  --  --  --    --  --    --  --  --  --  --  --    --  --  --  --    -- |
		   +  | R   R   S   S   S   S   R     S   R     R   R   R   R   R   R     R   R   S   R     R  |
		   -  | R   R   S   S   S   S   R     S   R     R   R   R   R   R   R     R   R   S   R     R  |
		   *  | R   R   R   R   S   S   R     S   R     R   R   R   R   R   R     R   R   S   R     R  |
		   /  | R   R   R   R   S   S   R     S   R     R   R   R   R   R   R     R   R   S   R     R  |
		   ^  | R   R   R   R   S   S   R     S   R     R   R   R   R   R   R     R   R   S   R     R  |
		   M  | R   R   R   R   R   S   R     S   R     R   R   R   R   R   R     R   R   S   R     R  |
		   ,  | R   R   R   R   R   R   E4    R   R     R   R   R   R   R   R     R   R   R   R     E4 |
		      |                                                                                        |
		   (  | S   S   S   S   S   S   S     S   S     S   S   S   S   S   S     S   S   S   S     E1 |
		s  )  | R   R   R   R   R   R   E4    E2  R     R   R   R   R   R   R     R   R   S   R     R  |
		t                                                                                              |
		a  == | R   R   R   R   R   R   R     S   R     R   R   R   R   R   R     R   R   R   R     R  |
		c  != | R   R   R   R   R   R   R     S   R     R   R   R   R   R   R     R   R   R   R     R  |
		k  <  | R   R   R   R   R   R   R     S   R     R   R   R   R   R   R     R   R   R   R     R  |
		   <= | R   R   R   R   R   R   R     S   R     R   R   R   R   R   R     R   R   R   R     R  |
		   >  | R   R   R   R   R   R   R     S   R     R   R   R   R   R   R     R   R   R   R     R  |
		   >= | R   R   R   R   R   R   R     S   R     R   R   R   R   R   R     R   R   R   R     R  |
		                                                                                               |
		   && | R   R   R   R   R   R   R     S   R     S   S   S   S   S   S     R   R   S   R     R  |
		   || | R   R   R   R   R   R   R     S   R     S   S   S   S   S   S     R   R   S   R     R  |
		   !  | R   R   R   R   R   R   R     S   R     S   S   S   S   S   S     R   R   S   R     R  |
		   ^^ | R   R   R   R   R   R   R     S   R     S   S   S   S   S   S     R   R   S   R     R  |
		      |                                                                                        |
		   $  | S   S   S   S   S   S   E4    S   E3    S   S   S   S   S   S     S   S   S   S     A  | */
		   
		   // comparison operators sit near the bottom of the precdence table, only bitwise operations are lower
		   // but we're not doing bitwise
		   // see: http://en.wikipedia.org/wiki/Order_of_operations
		
		// C# really needs C-style #defines at times...
		private static readonly P[,] _precedence = {
			//  +          -          *          /          ^         M           ,          (          )          ==         !=         <          <=         >          >=         &&         ||         !          ^^        $
	/*	+	*/{ P.Reduce,  P.Reduce,  P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce, P.Reduce  },
	/*	-	*/{ P.Reduce,  P.Reduce,  P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce, P.Reduce  },
	/*	*	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Shift,   P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce, P.Reduce  },
	/*	/	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Shift,   P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce, P.Reduce  },
	/*	^	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Shift,   P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce, P.Reduce  },
	/*	M	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce, P.Reduce  },
	/*	,	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Error4,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce, P.Error4  },
	/*	(	*/{ P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,  P.Error1  },
	/*	)	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Error4,  P.Error2,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce, P.Reduce  },
	
	/*	==	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,  P.Reduce  },
	/*	!=	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,  P.Reduce  },
	/*	<	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,  P.Reduce  },
	/*	<=	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,  P.Reduce  },
	/*	>	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,  P.Reduce  },
	/*	>=	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,  P.Reduce  },
	
	/*	&&	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce, P.Reduce   },
	/*	||	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce, P.Reduce  },
	/*	!	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce, P.Reduce  },
	/*	^^	*/{ P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce,  P.Shift,   P.Reduce,  P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Reduce,  P.Reduce,  P.Reduce,  P.Reduce, P.Reduce  },
	
	/*	$	*/{ P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Error4,  P.Shift,   P.Error3,  P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,   P.Shift,  P.Accept  }
		};
		
		private Object _lock = new Object();
		
		private BigNumFactory _factory;
		
		private Stack<BigNum>   _valueStack    = new Stack<BigNum>();
		private Stack<Operator> _operatorStack = new Stack<Operator>();
		
		private readonly String _expression;
		private Dictionary<String,BigNum> _symbols;
		
		private Operator _token; // current token
		private BigNum   _value; // current value
		private Operator _ptoken; // previous token
		
		private Boolean  _isFirstToken;
		private Int32    _toki;
		
		public Expression(String expression, BigNumFactory factory) {
			
			_factory    = factory;
			_expression = expression;
		}
		
		public BigNum Evaluate(Dictionary<String,BigNum> symbols) {
			
			lock( _lock ) {
			
			////////////////////////////
			// Reinit
			
			_valueStack.Clear();
			_operatorStack.Clear();
			_operatorStack.Push(Operator.Eof );
			
			_token  = Operator.Eof;
			_ptoken = Operator.Eof;
			_value  = _factory.Zero;
			_isFirstToken = true;
			_toki         = 0;
			
			if( symbols == null ) symbols = new Dictionary<String,BigNum>();
			
			_symbols = symbols;
			
			Advance();
			
			while(true) {
				
				OnStackChanged();
				
				if( _token == Operator.Val ) {
					// if the current token is a value
					// shift it to value stack
					
					Shift();
					
				} else {
					
					// get precedence for the last operator and the current operator
					Operator lastOp = _operatorStack.Peek();
					Precedence p = _precedence[ (int)lastOp, (int)_token ];
					switch(p) {
						case Precedence.Reduce:
							
							Reduce();
							
							break;
						case Precedence.Shift:
							
							Shift();
							
							break;
						case Precedence.Accept:
							
							EnsureVal(1);
							
							return _value = _valueStack.Pop();
							
						default:
							throw new ExpressionException( p.ToString() + " at position " + _toki );
					}
					
				}
				
				
			}//while
		
			}//lock
		}
		
		partial void OnStackChanged();
		
#if VisibleInternals
		
		partial void OnStackChanged() {
			if( StackChanged != null ) StackChanged(this, EventArgs.Empty);
		}
		
		public event EventHandler StackChanged;
		
		public BigNum[] CurrentValueStack {
			get {
				return _valueStack.ToArray();
			}
		}
		
		public Operator[] CurrentOperatorStack {
			get {
				return _operatorStack.ToArray();
			}
		}
		
		public BigNum CurrentValue {
			get {
				return _value;
			}
		}
		
		private static readonly Dictionary<Operator,String> OperatorSymbols = new Dictionary<Operator,String>() {
			{ Operator.Add, "+" },
			{ Operator.Sub, "-" },
			{ Operator.Mul, "*" },
			{ Operator.Div, "/" },
			{ Operator.Neg, "--" },
			
			{ Operator.Cmm, "," },
			{ Operator.PaL, "(" },
			{ Operator.PaR, ")" },
			
			{ Operator.CoE, "==" },
			{ Operator.CoN, "!=" },
			{ Operator.CoL, "<" },
			{ Operator.CLE, "<=" },
			{ Operator.CoG, ">" },
			{ Operator.CGE, ">=" },
			
			{ Operator.And, "&&" },
			{ Operator.Or,  "||" },
			{ Operator.Not, "!" },
			{ Operator.Xor, "^^" },
			
			{ Operator.Eof, "$" },
			{ Operator.Max, "Max" },
			{ Operator.Val, "Val" },
		};
		
#endif
		
		public String ExpressionString {
			get { return _expression; }
		}
		
		public String[] Tokenize() {
			
			lock(_lock) {
				
				List<String> tokens = new List<String>();
				
				_toki = 0;
				
				String t = Strtok();
				while(t != null) {
					
					tokens.Add( t );
					
					t = Strtok();
				}
				
				return tokens.ToArray();
			}
		}
		
		private void Advance() {
			
			if( _isFirstToken ) {
				
				_isFirstToken = false;
				_ptoken = Operator.Eof;
				
			}
			
			String s = Strtok();
			if( s == null ) {
				
				_token = Operator.Eof;
				
			} else {
				
				switch(s) {
// Arithmetic
					case "+" : _token = Operator.Add; break;
					case "-" : _token = Operator.Sub; break;
					case "*" : _token = Operator.Mul; break;
					case "/" : _token = Operator.Div; break;
					case "^" : _token = Operator.Pow; break;
// Punctuation
					case "," : _token = Operator.Cmm; break;
					case "(" : _token = Operator.PaL; break;
					case ")" : _token = Operator.PaR; break;
					
// Comparison
					case "==": _token = Operator.CoE; break;
					case "!=": _token = Operator.CoN; break;
					case "<" : _token = Operator.CoL; break;
					case "<=": _token = Operator.CLE; break;
					case ">" : _token = Operator.CoG; break;
					case ">=": _token = Operator.CGE; break;
// Logic
					case "&&": _token = Operator.And; break;
					case "||": _token = Operator.Or;  break;
					case "^^": _token = Operator.Xor; break;
					case "!" : _token = Operator.Not; break;
					default:
						// either a number, a name, or a function
						// if it's a name, resolve it
						
						// TODO: Make BigNum respect Culture's number format settings
						if( _factory.TryParse( s, out _value ) ) {
							
							_token = Operator.Val;
							
						} else {
							
//							if( IsFunction(s) ) {
//								
//								_token = Operator.Fun;
//								
//							} else
							if( _symbols.TryGetValue( s, out _value ) ) {
								
								_token = Operator.Val;
								
							} else {
								
								throw new ExpressionException("Undefined symbol: \"" + s + "\" at position " + _toki );
							}
							
						}
						
						break;
				}
				
			}
			
			// check for unary minus
			if( _token == Operator.Sub ) {
				
				if( _ptoken != Operator.Val && _ptoken != Operator.PaR ) {
					
					_token = Operator.Neg;
				}
				
			}
			
			_ptoken = _token;
			
		}
		
		private String Strtok() {
			
			if( _toki >= _expression.Length ) return null;
			
			String ret = _expression.Tok(ref _toki);
			
			// special case for != (which is not a contiguous category) OtherPunctuation+MathSymbol
			
			if( ret == "!" ) {
				
				Int32 origIdx = _toki;
				String next = _expression.Tok(ref _toki);
				if( next == "=" )
					ret = "!=";
				else
					_toki = origIdx;
			
			}
			
			return ret;
			
		}
		
		private void Shift() {
			
			if( _token == Operator.Val ) {
				
				_valueStack.Push( _value );
				
			} else {
				
				_operatorStack.Push( _token );
				
			}
			
			Advance();
		}
		
		private void Reduce() {
			
			BigNum one = _factory.Unity;
			BigNum zer = _factory.Zero;
			
			Operator op = _operatorStack.Peek();
			switch(op) {
				
				case Operator.Add:
					
					// Apply E := E + E
					EnsureVal(2);
					BigNum aa = _valueStack.Pop();
					BigNum ab = _valueStack.Pop();
					_valueStack.Push( aa + ab );
					
					break;
				
				case Operator.Sub:
					
					// Apply E := E - E
					EnsureVal(2);
					BigNum sa = _valueStack.Pop();
					BigNum sb = _valueStack.Pop();
					_valueStack.Push( sb - sa );
					
					break;
				
				case Operator.Mul:
					
					EnsureVal(2);
					BigNum ma = _valueStack.Pop();
					BigNum mb = _valueStack.Pop();
					_valueStack.Push( ma * mb );
					
					break;
				
				case Operator.Div:
					
					EnsureVal(2);
					BigNum da = _valueStack.Pop();
					BigNum db = _valueStack.Pop();
					_valueStack.Push( db / da );
					
					break;
				
				case Operator.Neg:
					
					EnsureVal(1);
					BigNum na = _valueStack.Pop();
					_valueStack.Push( -na );
					
					break;
				
				case Operator.Pow:
					
					EnsureVal(2);
					BigNum pa = _valueStack.Pop();
					BigNum pb = _valueStack.Pop();
					
					_valueStack.Push( BigMath.Pow( pb, pa ) );
					
					//Int32 exponent = Int32.Parse( pa.ToString(), N.Integer | N.AllowExponent, Cult.InvariantCulture );
					// _valueStack.Push( pb.Power( exponent ) );
					
					break;
					
				case Operator.PaR:
					
					_operatorStack.Pop();
					break;
				
				case Operator.CoE:
				case Operator.CoN:
				case Operator.CoL:
				case Operator.CLE:
				case Operator.CoG:
				case Operator.CGE:
					
					EnsureVal(2);
					BigNum ea = _valueStack.Pop();
					BigNum eb = _valueStack.Pop();
					
					Boolean eq = ea == eb;
					Boolean lt = eb <  ea;
					Boolean gt = eb >  ea;
					
					if     ( op == Operator.CoE ) _valueStack.Push( eq       ? one : zer );
					else if( op == Operator.CoN ) _valueStack.Push( eq       ? zer : one );
					else if( op == Operator.CoL ) _valueStack.Push( lt       ? one : zer );
					else if( op == Operator.CLE ) _valueStack.Push( lt || eq ? one : zer );
					else if( op == Operator.CoG ) _valueStack.Push( gt       ? one : zer );
					else if( op == Operator.CGE ) _valueStack.Push( gt || eq ? one : zer );
					
					break;
					
				case Operator.And:
				case Operator.Or:
				case Operator.Xor:
					
					EnsureVal(2);
					BigNum binA = _valueStack.Pop();
					BigNum binB = _valueStack.Pop();
					
					switch(op) {
						case Operator.And:
							
							Boolean and = binA == one && binB == one;
							_valueStack.Push( and ? one : zer );
							break;
							
						case Operator.Or:
							
							Boolean or  = binA == one || binB == one;
							_valueStack.Push( or  ? one : zer );
							break;
							
						case Operator.Xor:
							
							Boolean xor = (binA == one && binB != one) || (binA != one && binB == one);
							_valueStack.Push( xor ? one : zer );
							break;
							
					}
					
					break;
					
				case Operator.Not:
					
					EnsureVal(1);
					BigNum notA = _valueStack.Pop();
					if(notA == one) notA = zer;
					else            notA = one;
					
					_valueStack.Push( notA );
					
					break;
				
//				Else, ignore it. Do not throw an exception
				
			}
			
			_operatorStack.Pop();
		}
		
		private void EnsureVal(Int32 depth) {
			
			if( _valueStack.Count < depth ) throw new ExpressionException("Syntax error (EnsureVal) at position " + _toki );
			
		}
		
		public override String ToString() {
			
			return ExpressionString;
		}
		
	}
	
	internal enum Operator { // numbering is importance because it's used as a lookup in the precedence table
// Arithmetic	
		Add = 0,
		Sub = 1,
		Mul,
		Div,
		Pow,
		Neg, // unary negation
// Punctuation
		Cmm, // comma
		PaL, // left parens
		PaR, // right parens
// Comparison
		CoE, // Comparison: Equals
		CoN, // Comparison: Not Equals
		CoL, // Comparison: Less Than
		CLE, // Comparison: Less Than or Equal To
		CoG, // Comparison: Greater Than
		CGE, // Comparison: Greater Than or Equal To
// Logic
		And,
		Or,
		Not,
		Xor,
// Special
		Eof, // end of
		Max, // maximum number of operators
		Val, // value
		Fun, // function call
	}
	
	internal enum Precedence {
		Shift  = 0,
		Reduce = 1,
		Accept = 2,
		/// <summary>Missing right parenthesis</summary>
		Error1 = 6,
		/// <summary>Missing operator</summary>
		Error2 = 7,
		/// <summary>Unbalanced right parenthesis</summary>
		Error3 = 8,
		/// <summary>Invalid function argument</summary>
		Error4 = 9
	}
	
	[Serializable]
	public class ExpressionException : Exception {
		
		public ExpressionException() {
		}
		
		public ExpressionException(string message) : base(message) {
		}
		
		public ExpressionException(string message, Exception inner) : base(message, inner) {
		}
		
		protected ExpressionException( S.SerializationInfo info, S.StreamingContext context) : base(info, context) {
		}
		
	}
	
}
