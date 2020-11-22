using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonkeyInCSharp;
using System;
using System.Globalization;
using System.Collections.Generic;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyTests {
    [TestClass]
    public class AST_test {

        [TestMethod]
        public void Test_AST_ToString() {
            string inputString = @"let myVar = anotherVar;";

            //construct the AST by hand and then serialize it to spit out the string
            Program program = new Program() {
                Statements = new List<Statement>() {
                                    new LetStatement(){ Token = new Token(TokenHelper.TokenType.LET, "let"),
                                                        Name = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "myVar"), Value="myVar"},
                                                        Value = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "anotherVar"), Value="anotherVar"}
                                                      }
                }
            };

            string AST_ToString = program.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", program.ToString()));
        }

        [TestMethod]
        public void TestStringHashKey() {
            String_AST hello1 = new String_AST("Hello World");
            String_AST hello2 = new String_AST("Hello World");

            String_AST diff1 = new String_AST("My name is johnny");
            String_AST diff2 = new String_AST("My name is johnny");

            //Assert.Equals(hello1.HashKey(), hello2.HashKey());//this assertion works on structs fine

            //if (hello1.HashKey() != hello2.HashKey()) {//outof the box, comparison like this do not work for structs. 
            if (!hello1.HashKey().Equals(hello2.HashKey())) {
                throw new AssertFailedException("strings with same content have different hash keys");
            }

            //if (diff1.HashKey() != diff2.HashKey()) {
            if (!diff1.HashKey().Equals(diff2.HashKey())) {
                throw new AssertFailedException("strings with same content have different hash keys");
            }

            //if (hello1.HashKey() == diff1.HashKey()) {
            if (hello1.HashKey().Equals(diff1.HashKey())) {
                throw new AssertFailedException("strings with different content have same hash keys");
            }
        }

        [TestMethod]
        public void TestBooleanHashKey() {
            Boolean_AST hello1 = new Boolean_AST(true);
            Boolean_AST hello2 = new Boolean_AST(true);

            Boolean_AST diff1 = new Boolean_AST(false);
            Boolean_AST diff2 = new Boolean_AST(false);

            //Assert.Equals(hello1.HashKey(), hello2.HashKey());//this assertion works on structs fine

            //if (hello1.HashKey() != hello2.HashKey()) {//outof the box, comparison like this do not work for structs. 
            if (!hello1.HashKey().Equals(hello2.HashKey())) {
                throw new AssertFailedException("trues do not have same hash key");
            }

            //if (diff1.HashKey() != diff2.HashKey()) {
            if (!diff1.HashKey().Equals(diff2.HashKey())) {
                throw new AssertFailedException("falses do not have same hash key");
            }

            //if (hello1.HashKey() == diff1.HashKey()) {
            if (hello1.HashKey().Equals(diff1.HashKey())) {
                throw new AssertFailedException("true has same hash key as false");
            }
        }

        [TestMethod]
        public void TestIntegerHashKey() {
            Integer_AST hello1 = new Integer_AST(1);
            Integer_AST hello2 = new Integer_AST(1);

            Integer_AST diff1 = new Integer_AST(2);
            Integer_AST diff2 = new Integer_AST(2);

            //Assert.Equals(hello1.HashKey(), hello2.HashKey());//this assertion works on structs fine

            //if (hello1.HashKey() != hello2.HashKey()) {//outof the box, comparison like this do not work for structs. 
            if (!hello1.HashKey().Equals(hello2.HashKey())) {
                throw new AssertFailedException("ints with same content have different hash keys");
            }

            //if (diff1.HashKey() != diff2.HashKey()) {
            if (!diff1.HashKey().Equals(diff2.HashKey())) {
                throw new AssertFailedException("ints with same content have different hash keys");
            }

            //if (hello1.HashKey() == diff1.HashKey()) {
            if (hello1.HashKey().Equals(diff1.HashKey())) {
                throw new AssertFailedException("ints with different content have same hash keys");
            }
        }

    }
}
