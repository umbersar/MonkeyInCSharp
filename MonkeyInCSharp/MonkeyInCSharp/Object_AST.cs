using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

//when we evaluate the nodes walking the AST tree, we need to handle the return types of the the recursive walk. So we need a new type/interface from which the Monkey types derive that could be
//returned in the tree walk. Dynamic languages like python do not have to deal with this 'issue' as we do not define a return type of a pthon method and it can return any type it wants to.
//The object hierarchy we are building here (base type and then classes for all types to be handled) looks like a mirror image of type hierarchy we build for tokenizer phase (where we have
//base class Node and then types we are parsing are derived from it). Why could we not have used hierarchy here with changes as required??
namespace MonkeyInCSharp {
    public class Environment_AST {
        private Dictionary<string, Object_AST> _store;
        private Environment_AST _outer;//used to keep track of nested scopes

        private Environment_AST() {
            this._store = new Dictionary<string, Object_AST>();
            this._outer = null;
        }

        public static Environment_AST NewEnvironment_AST() {
            return new Environment_AST();
        }

        public static Environment_AST NewEnclosedEnvironment_AST(Environment_AST outer) {
            //Environment_AST env = new Environment_AST() { _outer = outer};
            //env._outer = outer;

            return new Environment_AST() { _outer = outer };
        }

        public (Object_AST, bool) Get(string name) {
            bool found = _store.TryGetValue(name, out Object_AST value);
            if (!found && this._outer != null) {
                return this._outer.Get(name);
            }

            return (value, found);
        }

        public Object_AST Set(string name, Object_AST value) {
            _store[name] = value;
            return value;
        }
    }

    //Integer_AST, Boolean_AST and String_AST implement the Hashable interface so that they can be used as a hash key
    public interface IHashable {
        HashKey HashKey();
    }

    //if we declare it as a class, then we can't compare as shown below
    //hello1.HashKey() == hello2.HashKey() if both hell1 and hello2 are the same strings, we expect this condition to succeed. But it wont as the we are comparing ref types(if HashKey is defined as class)
    //declaring it as a struct (value type) allows us to do comparisons
    public struct HashKey {
        public ObjectType_AST Type { get; set; }
        public ulong Value;
    }

    public class HashPair {
        public Object_AST Key { get; set; }
        public Object_AST Value { get; set; }
    }

    public class Hash_AST : Object_AST {
        public Dictionary<HashKey, HashPair> Pairs = new Dictionary<HashKey, HashPair>();

        public ObjectType_AST Type => ObjectType_AST.Hash_AST;

        public string Inspect() {
            //string.Join(", ", Pairs). It will output in the form [A, 1], [B, 2], [C, 3], [D, 4] 
            return string.Join(",", Pairs.Select(x => x.Value.Key.Inspect() + ":" + x.Value.Value.Inspect()).ToArray());
        }
    }


    public interface Object_AST {
        public ObjectType_AST Type { get; }
        public string Inspect();
    }

    public enum ObjectType_AST {//Evaluation of our language by interpretor would result in one of these types. Some are used internally for walking the AST like (ReturnValue_AST) and never returned end user
        Integer_AST,
        Boolean_AST,
        ReturnValue_AST,
        Error_AST,
        Function_AST,
        String_AST,
        BuiltinFunc_AST,
        Array_AST,
        Hash_AST,

        Null_AST //Why do we have null if we do not have support for it in tokenizer?
    }

    public class Array_AST : Object_AST {
        public List<Object_AST> Elements;

        public ObjectType_AST Type => ObjectType_AST.Array_AST;

        public string Inspect() {

            List<string> serializedElements = new List<string>();
            foreach (var e in this.Elements) {
                serializedElements.Add(e.Inspect());
            }

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("[");
            stringBuilder.Append(String.Join(",", serializedElements));
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }

    //a function is a type that we can pass around (for e.g. to higher order funcs), bind them to IDENTs, use in expressions, return from functions, etc.
    //That means a function object will be passed around while we traverse the AST. So just like Integer_AST, Boolean_AST, etc., we need a Function object derived from Object_AST so
    //that the Eval func can handle it.
    public class Function_AST : Object_AST {
        //we don't hold a reference Token in this class used in walking AST. We could have but we just hardcode the "fn" value where we need it. for comparison, we also don't hold a reference to 
        //Token in Integer_AST but IntegerLiteral does hold it. It is because when we match against different nodes in Eval, we already know what Token the node corresponds to, so we can just hardcode
        //it if we need to. The other reason is that we don't want to leak our internal implmentation (or helper objects) to the end user. For example, when we return Integer_AST to end user, we
        //just want to return the value associated with it and not the internal helper Tokens we are using in lexing(Tokenization phase).
        //public Token Token { get; set; }//TokenType.FUNCTION 'fn' token. 
        public List<Identifier> Parameters { get; set; }
        public BlockStatement Body;
        public Environment_AST Env;//since we allow for closures, so function has to hold the reference to variables (defined outside the function body but being used in it)
                                   //the function object has to track the enviroment so that the variable can be used. Normally the outside variable would have been defined on the stack 
                                   // and would have gone out of scope and freed. Remember that this is referring to outer scope/environment of a function. The arguments that have been
                                   //passed to this func would be bound to the parameters in a inner environment(scope). Environment_AST class allows nested environs/scopes.

        public Function_AST(List<Identifier> parameters, BlockStatement body, Environment_AST env) {
            this.Parameters = parameters;
            this.Body = body;
            this.Env = env;
        }

        public ObjectType_AST Type => ObjectType_AST.Function_AST;

        public string Inspect() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("fn");//(this.TokenLiteral());//we are not storing the Token in classes used by AST
            stringBuilder.Append("(");
            stringBuilder.Append(String.Join(",", this.Parameters));
            stringBuilder.Append("){");
            stringBuilder.Append(this.Body.ToString());//todo:BlockStatement should have handled the concatenation of curly braces to statement block and 
            stringBuilder.Append("}");//we should not be doing it here.

            return stringBuilder.ToString();

        }
    }

    public class Error_AST : Object_AST {
        public string Message { get; }

        public Error_AST(string message) {
            Message = message;
        }

        public ObjectType_AST Type {
            get { return ObjectType_AST.Error_AST; }
        }

        public string Inspect() {
            return string.Format("Error: {0}", this.Message.ToString());
        }
    }

    //we have to not only evaluate the expression in the ReturnStatement but also 'return' it. So we need to track the value being returned so that the value can be used later.
    //this class is the wrapper class that handles all the different return type (int_ast,bool_ast). 
    //Used internally for walking the AST like (ReturnValue_AST) and never returned end user. 
    public class ReturnValue_AST : Object_AST {
        public Object_AST Value { get; }

        public ReturnValue_AST(Object_AST value) {
            Value = value;
        }

        public ObjectType_AST Type {
            get { return ObjectType_AST.ReturnValue_AST; }
        }

        public string Inspect() {
            return this.Value.ToString();
        }
    }

    public delegate Object_AST BuiltinFunction(params Object_AST[] args);
    //using BuiltinFunction = Func<params Object_AST[], Object_AST>; //instead of definning my own delegates here, i could have used readymade Action and Func delegate types
    //using BuiltinFunction = Func<List<Object_AST>, Object_AST>;//but when we make custom delegate types, we give them names and that makes the intention clearer

    public class BuiltinFunc_AST : Object_AST {
        public BuiltinFunction fn;
        public ObjectType_AST Type {
            get { return ObjectType_AST.BuiltinFunc_AST; }
        }
        public string Inspect() {
            return "builtin function";
        }
    }
    public class Integer_AST : Object_AST, IHashable {
        public Int64 Value { get; }

        public Integer_AST(long value) {
            Value = value;
        }

        public ObjectType_AST Type {
            get { return ObjectType_AST.Integer_AST; }
        }

        public string Inspect() {
            return this.Value.ToString();
        }

        public HashKey HashKey() {
            return new HashKey { Type = this.Type, Value = (ulong)this.Value };
        }
    }

    public class String_AST : Object_AST, IHashable {
        public string Value { get; }

        public String_AST(string value) {
            Value = value;
        }

        public ObjectType_AST Type {
            get { return ObjectType_AST.String_AST; }
        }

        public string Inspect() {
            return this.Value;
        }

        public HashKey HashKey() {
            ulong strHashcode = (ulong)this.Value.GetHashCode();//todo: find a better way to implement this

            return new HashKey { Type = this.Type, Value = strHashcode };
        }
    }

    public class Boolean_AST : Object_AST, IHashable {
        public bool Value { get; }

        public Boolean_AST(bool value) {
            Value = value;
        }

        public ObjectType_AST Type {
            get { return ObjectType_AST.Boolean_AST; }
        }

        public string Inspect() {
            return this.Value.ToString().ToLower();//Calling ToString on true changes it to "True". The leading char becomes uppercase.
                                                   //calling ToLower() is not the right fix but better than not doing anything
        }

        public HashKey HashKey() {
            return new HashKey { Type = this.Type, Value = (ulong)(this.Value ? 1 : 0) };
        }
    }

    public class Null_AST : Object_AST {//todo:since we do not have a equivalent Null object in the tokenizer, why do we have it here? 
        public ObjectType_AST Type {
            get { return ObjectType_AST.Null_AST; }
        }

        public string Inspect() {
            return "null";
        }
    }
}
