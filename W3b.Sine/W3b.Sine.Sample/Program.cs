using System;
using Cult = System.Globalization.CultureInfo;
using System.Collections.Generic;

namespace W3b.Sine {
	
// TODO Ideas:
// a) Change the symbols dictionary from <String,BigNum> to <String,Expression>, which means it'll still work for constant values
// b) Have two entry modes: evaluation or command
//     i) command mode would do stuff like alter or view the symbol/function dictionary, and give it a blue background?
// c) Create a FractionBigNum class which stores numbers as a numerator/denominator pair
// d) Add in support for native functions
	
	public static class Program {
		
		private static BigNumFactory Factory { get { return BigRationalFactory.Instance; } }
		
		public static Int32 Main(String[] args) {
			
			if(args.Length > 0) {
				
				String one = args[0];
				switch(one.ToUpperInvariant()) {
					case "?":
					case "/?":
					case "-?":
					case "HELP":
					case "/HELP":
					case "-HELP":
						
						PrintHelp();
						break;
						
					case "TEST":
					case "/TEST":
					case "-TEST":
						
//						BigNumTests.Test();
						Console.WriteLine("Tests temporarily disabled");
						break;
						
					default:
						
						// HACK: Remove the path from the command line, using quote-finding isn't doing it right
						String expr = Environment.CommandLine;
						expr = expr.Substring( expr.LastIndexOf('"') + 2 ); // +1 to clear the " and another to clear the space
						
						EvaluateExpression( expr );
						break;
				}
			
			} else {
				
				PrintBanner();
				
				while(PromptUser()) {
				}
				
			}
			
			return 0;
		}
		
		private static void PrintBanner() {
			
			Console.WriteLine("W3b.Sine Sample Program. http://sine.codeplex.com");
			Console.WriteLine("Copyright 2008-2009 David Rees. New BSD License.");
			Console.WriteLine("Type 'help' for help, 'test' to run tests, 'q' to quit.");
		}
		
		private static void PrintHelp() {
			
			String help = 
@"Commands:
	
	q
		Quits the program
	add <symbolName> = <expr>
		Add an expression's value to the dictionary (expressions are not reevaluated, use functions instead)
	rem <symbolName>
		Remove a symbol to the dictionary
	help
		Displays this message
	test
		Runs tests
		
	Any other input is interpreted as a function call or evaluated as an expression.
	For this reason, don't use command names as symbols
	
Command-line options:
	
	Sample.exe /help
		Prints this message
	Sample.exe /test
		Runs BigNum tests
	Samplele.exe <expr>
		Evaluates <expr> and quits

Implemented Functions:
	Trigonometric:
		sin, cos, tan
		(Sine, Cosine, Tangent)
	Reciprocal Trig:
		sec, csc, cot
		(Secant, Cosec, Cotangent)
	Number:
		abs, floor, ceil, fact
		(Absolute, Floor, Ceiling, Factorial)
		
	Functions must be on their own line and cannot be a part of an expression";
			
			Console.WriteLine(help);
			
		}
		
		private static Int32 _count;
		private static Dictionary<String,BigNum> _symbols = new Dictionary<String,BigNum>();
		
		private static Boolean PromptUser() {
			
			Console.Write( (++_count).ToString() );
			Console.Write(">");
			
			String s = Console.ReadLine();
			if( s == "q" ) return false;
			
			if( s.StartsWith("add ", StringComparison.OrdinalIgnoreCase) ) {
				
				String name = s.Substring(4, s.IndexOf('=') - 5 ).Trim();
				String expr = s.Substring( s.IndexOf('=') + 1 ).Trim();
				
				try {
					
					Expression xp = new Expression( expr, Factory );
					BigNum ret = xp.Evaluate( _symbols );
					
					if( _symbols.ContainsKey( name ) ) _symbols[name] = ret;
					else                               _symbols.Add( name, ret );
					
					Console.WriteLine("Added: " + name + " = " + ret.ToString() );
					
				} catch(Exception ex) {
					
					PrintException(ex);
				}
				
			} else if( s.StartsWith("rem ", StringComparison.OrdinalIgnoreCase) ) {
				
				String name = s.Substring(4);
				
				_symbols.Remove( name );
				
				Console.WriteLine("Removed: " + name);
				
			} else if( String.Equals(s, "help", StringComparison.OrdinalIgnoreCase) ) {
				
				PrintHelp();
				
			} else if( String.Equals(s, "test", StringComparison.OrdinalIgnoreCase) ) {
				
//				BigNumTests.Test();
				
			} else {
				
				Int32 startIndex;
				Function func = IsFunction( s, out startIndex );
				if( func != Function.None ) {
					
					EvaluateFunction( func, s.Substring( startIndex ) );
					
				} else {
					
					EvaluateExpression( s );
					
				}
			}
			
			return true;
			
		}
		
		private static void EvaluateExpression(String expression) {
			
			try {
				
				Expression expr = new Expression( expression, Factory );
				BigNum ret = expr.Evaluate( _symbols );
				
				Console.WriteLine("Result: " + ret.ToString() );
				
			} catch(Exception ex) {
				
				PrintException(ex);
			}
			
		}
		
		private static void EvaluateFunction(Function function, String expression) {
			
			try {
				
				Expression expr = new Expression( expression, Factory );
				BigNum num = expr.Evaluate( _symbols );
				
				BigNum result = EvaluateFunction(function, num);
				
				Console.WriteLine("Result: " + result.ToString() );
				
			} catch(Exception ex) {
				
				PrintException(ex);
			}
			
		}
		
		private static BigNum EvaluateFunction(Function function, BigNum num) {
			
			switch(function) {
				
				case Function.Sin: return BigMath.Sin( num );
				case Function.Cos: return BigMath.Cos( num );
				case Function.Tan: return BigMath.Tan( num );
				
				case Function.Csc: return BigMath.Csc( num );
				case Function.Sec: return BigMath.Sec( num );
				case Function.Cot: return BigMath.Cot( num );
				
				case Function.Abs:   return BigMath.Abs( num );
				case Function.Ceil:  return BigMath.Ceiling( num );
				case Function.Floor: return BigMath.Floor( num );
				case Function.Fact:  return BigMath.Gamma( num );
				
				default:
					throw new ExpressionException("Unknown or invalid function");				
				
			}
			
		}
		
		private static void PrintException(Exception ex) {
			
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("\tException during evaluation");
			
			while(ex != null) {
				
				Console.Write('\t');
				Console.Write(ex.GetType().FullName);
				Console.Write(" - ");
				Console.WriteLine(ex.Message);
				
				String[] lines = ex.StackTrace.Split('\n');
				foreach(String line in lines) {
					
					Console.Write('\t');
					Console.WriteLine(line);
				}
				
				Console.WriteLine();
				
				ex = ex.InnerException;
			}
			
			Console.ResetColor();
			
		}
		
		private static Function IsFunction(String input, out Int32 spaceIdx) {
			
			spaceIdx = input.IndexOf(" ");
			if( spaceIdx <= 2 ) return Function.None;
			
			String funcName = input.Substring(0, spaceIdx );
			
			switch(funcName) {
				case "sin": return Function.Sin;
				case "cos": return Function.Cos;
				case "tan": return Function.Tan;
				
				case "csc": return Function.Csc;
				case "sec": return Function.Sec;
				case "cot": return Function.Cot;
				
				case "abs"  : return Function.Abs;
				case "floor": return Function.Floor;
				case "ceil" : return Function.Ceil;
				case "fact" : return Function.Fact;
				
				default : return Function.None;
			}
			
		}
		
		private enum Function {
			None,
			
			Sin,
			Cos,
			Tan,
			
			Csc,
			Sec,
			Cot,
			
			Abs,
			Floor,
			Ceil,
			Fact
		}
		
	}
	
}
