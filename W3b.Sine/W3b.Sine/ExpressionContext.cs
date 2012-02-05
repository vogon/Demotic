using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

// The code in this class is unfinished
// TODO:
// * Modify the Expression class so it can participate in a context (rather than having a symbol table defined)
//		* So if it encounters a symbol it looks it up in the context and evaluates it to get the value to resume execution
//		* I'll need a way to spot for stack-overflow
// * Actually, keep the symbol table as a simpler alternative to setting up an expression context

namespace W3b.Sine {
	
	public class ExpressionContext {
		
		private static BuiltInFunction[] _builtIn = {
			new BuiltInFunction("abs"    , new BigFunction ( BigMath.Abs     ), "x" ),
			new BuiltInFunction("floor"  , new BigFunction ( BigMath.Floor   ), "x" ),
			new BuiltInFunction("ceiling", new BigFunction ( BigMath.Ceiling ), "x" ),
			new BuiltInFunction("max"    , new BigFunction2( BigMath.Max     ), "x", "y" ),
			new BuiltInFunction("min"    , new BigFunction2( BigMath.Min     ), "x", "y" ),
			
			new BuiltInFunction("gcd"    , new BigIntFunction2( BigMath.Gcd       ), "x", "y" ),
			new BuiltInFunction("fact"   , new BigIntFunction ( BigMath.Factorial ), "z" ),
			
			new BuiltInFunction("pow"    , new BigFunction2( BigMath.Pow   ), "x", "exponent" ),
			new BuiltInFunction("exp"    , new BigFunction ( BigMath.Exp   ), "exponent" ),
			new BuiltInFunction("ln"     , new BigFunction ( BigMath.Log   ), "x" ),
			new BuiltInFunction("logb"   , new BigFunction2( BigMath.Log   ), "x", "base" ),
			new BuiltInFunction("gam"    , new BigFunction ( BigMath.Gamma ), "z" ),
			
			new BuiltInFunction("sin"    , new BigFunction( BigMath.Sin ), "theta" ),
			new BuiltInFunction("cos"    , new BigFunction( BigMath.Cos ), "theta" ),
			new BuiltInFunction("tan"    , new BigFunction( BigMath.Tan ), "theta" ),
			
			new BuiltInFunction("csc"    , new BigFunction( BigMath.Csc ), "theta" ),
			new BuiltInFunction("sec"    , new BigFunction( BigMath.Sec ), "theta" ),
			new BuiltInFunction("cot"    , new BigFunction( BigMath.Cot ), "theta" ),
		};
		
		public ReadOnlyCollection<Symbol> Symbols { get; private set; }
		
		
		
	}
	
	public abstract class Symbol {
		
		public String Name { get; protected set; }
		
		public override abstract String ToString();
		
	}
	
	public class Variable : Symbol {
		
		public Variable(String name, BigNum value) {
			
			Name  = name;
			Value = value;
		}
		
		public BigNum Value { get; private set; }
		
		public override String ToString() {
			
			return Name + " := " + Value.ToString();
		}
		
	}
	
	public abstract class Function : Symbol {
		
		public FunctionType               FunctionType   { get; protected set; }
		public ReadOnlyCollection<String> ParameterNames { get; protected set; }
		
		public abstract BigNum Evaluate(params Variable[] arguments);
		
		public override String ToString() {
			
			return "Function " +  Name + "(" + ParameterNames.ToStringList() + ")";
		}
		
	}
	
	public class BuiltInFunction : Function {
		
		private Delegate _impl;
		
		internal BuiltInFunction(String name, Delegate implementation, params String[] parameterNames) {
			
			Name           = name;
			FunctionType   = FunctionType.BuiltIn;
			ParameterNames = new ReadOnlyCollection<String>(parameterNames);
			
			_impl = implementation;
		}
		
		public override BigNum Evaluate(params Variable[] arguments) {
			
			Object ret = _impl.DynamicInvoke( arguments );
			
			return ret as BigNum;
		}
		
	}
	
	public class UserFunction : Function {
		
		private Expression _expr;
		
		public UserFunction(String name, String expression, params String[] parameterNames) {
			
			Name           = name;
			FunctionType   = FunctionType.User;
			ParameterNames = new ReadOnlyCollection<String>(parameterNames);
			
			_expr = new Expression( expression, null );
		}
		
		public override BigNum Evaluate(params Variable[] arguments) {
			
			// TODO
			return _expr.Evaluate( null );
		}
		
		public override String ToString() {
			String signature = base.ToString();
			return signature + " := " + _expr.ToString();
		}
		
	}
	
	public enum FunctionType {
		BuiltIn,
		Predefined,
		User
	}
	
	public delegate BigNum BigFunction(BigNum x);
	public delegate BigNum BigFunction2(BigNum x, BigNum y);
	public delegate BigNum BigFunction3(BigNum x, BigNum y, BigNum z);
	
	public delegate BigInt BigIntFunction(BigInt x);
	public delegate BigInt BigIntFunction2(BigInt x, BigInt y);
	
}
