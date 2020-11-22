using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyInCSharp {
    public class BuiltInFuncDefs {
        public static readonly Dictionary<string, BuiltinFunc_AST> builtinFuncs = new Dictionary<string, BuiltinFunc_AST>() {
            {"len", new BuiltinFunc_AST{fn = (Object_AST[] args)=>
                                                                {
                                                                    if(args.Length!=1)
                                                                        return new Error_AST(string.Format("wrong number of arguments. got={0}, want=1", args.Length));

                                                                    switch (args[0]){
                                                                        case String_AST s:
                                                                            return new Integer_AST(s.Value.Length);//why not wrap the return value in ReturnValue_AST
                                                                        case Array_AST arr:
                                                                            return new Integer_AST(arr.Elements.Count);//why not wrap the return value in ReturnValue_AST
                                                                        default:
                                                                            return new Error_AST(string.Format(@"argument to ""len"" not supported, got {0}",args[0].Type));
                                                                    }
                                                                }
                                        }
            },

            {"first", new BuiltinFunc_AST{fn = (Object_AST[] args)=>
                                                                {
                                                                    if(args.Length!=1)
                                                                        return new Error_AST(string.Format("wrong number of arguments. got={0}, want=1", args.Length));

                                                                    switch (args[0]){
                                                                        case Array_AST arr:
                                                                            if(arr.Elements.Count>0)
                                                                                return arr.Elements[0];//why not wrap the return value in ReturnValue_AST
                                                                            else
                                                                                return Evaluator.null_AST;
                                                                        default:
                                                                            return new Error_AST(string.Format(@"argument to ""first"" must be an array, got {0}",args[0].Type));
                                                                    }
                                                                }
                                        }
            },

            {"last", new BuiltinFunc_AST{fn = (Object_AST[] args)=>
                                                                {
                                                                    if(args.Length!=1)
                                                                        return new Error_AST(string.Format("wrong number of arguments. got={0}, want=1", args.Length));

                                                                    switch (args[0]){
                                                                        case Array_AST arr:
                                                                            if(arr.Elements.Count>0)
                                                                                return arr.Elements[arr.Elements.Count-1];//why not wrap the return value in ReturnValue_AST
                                                                            else
                                                                                return Evaluator.null_AST;
                                                                        default:
                                                                            return new Error_AST(string.Format(@"argument to ""last"" must be an array, got {0}",args[0].Type));
                                                                    }
                                                                }
                                        }
            },

            //could be called tail as well. We want to return a new array which contains array elements except the head(we are not modifying the existing array)
            //arrays in monkey are immutable
            {"rest", new BuiltinFunc_AST{fn = (Object_AST[] args)=>
                                                                {
                                                                    if(args.Length!=1)
                                                                        return new Error_AST(string.Format("wrong number of arguments. got={0}, want=1", args.Length));

                                                                    switch (args[0]){
                                                                        case Array_AST arr:
                                                                            if(arr.Elements.Count>0){
                                                                                List<Object_AST> rest = new List<Object_AST>(arr.Elements.GetRange(1, arr.Elements.Count-1));
                                                                                return new Array_AST(){ Elements = rest};//why not wrap the return value in ReturnValue_AST
                                                                            } else
                                                                                return Evaluator.null_AST;
                                                                        default:
                                                                            return new Error_AST(string.Format(@"argument to ""rest"" must be an array, got {0}",args[0].Type));
                                                                    }
                                                                }
                                        }
            },

            //arrays in monkey are immutable. So create a new collection to return with the added element
            {"push", new BuiltinFunc_AST{fn = (Object_AST[] args)=>
                                                                {
                                                                    if(args.Length!=2)
                                                                        return new Error_AST(string.Format("wrong number of arguments. got={0}, want=1", args.Length));

                                                                    switch (args[0], args[1]){
                                                                        case (Array_AST arr, Object_AST obj):
                                                                                List<Object_AST> newArr = new List<Object_AST>(arr.Elements);
                                                                                newArr.Add(obj);

                                                                                return new Array_AST(){ Elements = newArr};//why not wrap the return value in ReturnValue_AST
                                                                        default:
                                                                            return new Error_AST(string.Format(@"argument to ""push"" must be an array, got {0}",args[0].Type));
                                                                    }
                                                                }
                                        }
            },

            {"puts", new BuiltinFunc_AST{fn = (Object_AST[] args)=>
                                                                {
                                                                    foreach (var arg in args){
                                                                        Console.WriteLine(arg.Inspect());//
                                                                    }

                                                                    return Evaluator.null_AST;
                                                                }
                                        }
            },
        };
    }
}
