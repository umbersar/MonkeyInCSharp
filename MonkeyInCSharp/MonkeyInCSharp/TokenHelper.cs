using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyInCSharp {
    public static class TokenHelper {

        //i could have used default int enum but specifying the 'char' values (not string) help in readability. That char value 
        //is internally converted to int. And for that reason can't use strings enum values. 
        public enum TokenType {
            ILLEGAL,
            EOF,

            IDENT,
            INT,
            STRING,

            ASSIGN,
            PLUS,
            MINUS,
            BANG,//!
            ASTERISK,
            SLASH,// /

            LT,
            GT,

            EQ,//can't use string "=="
            NOT_EQ,//can't use string "!="

            COMMA,
            SEMICOLON,
            COLON,//used for dictionary/hashmap implementation

            LPAREN,//(
            RPAREN,//)
            LBRACE,//{
            RBRACE,//}
            LBRACKET,//[
            RBRACKET,//]

            FUNCTION,
            LET,
            TRUE,
            FALSE,
            IF,
            ELSE,
            RETURN
        }


        //keywords is a mapping dictionary which maps string literals to TokenTypes.
        static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> {
                                                        {"fn", TokenType.FUNCTION},
                                                        {"let", TokenType.LET},
                                                        {"true", TokenType.TRUE},
                                                        {"false", TokenType.FALSE},
                                                        {"if", TokenType.IF},
                                                        {"else", TokenType.ELSE},
                                                        {"return", TokenType.RETURN}
                                                };

        public static TokenType LookupIdent(string ident) {
            //use ident.ToLower() to handle case-insensitive match
            ident = ident.ToLower();
            if (keywords.ContainsKey(ident))//is it a keyword like LET
            {
                return keywords[ident];
            } else
                return TokenType.IDENT;//else it is a variable or function name literal
        }
    }
}
