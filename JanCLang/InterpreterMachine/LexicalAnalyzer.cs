using JanCLang.Misc;
using System.Collections.Generic;

namespace JanCLang.InterpreterMachine
{
    // Performs lexical analysis and creates a sequence of lexical tokens.
    internal class LexicalAnalyzer
    {
        SymbolTable SyT;



        // ENTRY POINT
        internal Queue<Token> RunLexicalAnalyzer(Queue<string> lexemes, string sourceCode, SymbolTable syt)
        {
            SyT = syt;

            string buffer = "";
            bool isComment = false;
            bool notString = true;

            foreach (char c in sourceCode)
            {
                if (c == '#' && !isComment && notString)
                {
                    isComment = true;
                    continue;
                }

                if (isComment && c != '\n')
                {
                    continue;
                }
                else
                {
                    isComment = false;
                }

                if (c == '\"')
                {
                    notString = false;

                    if (buffer.Length > 0)
                    {
                        buffer += c.ToString();
                        lexemes.Enqueue(buffer);
                        buffer = "";
                        notString = true;
                        continue;
                    }
                }

                if (notString)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        if (buffer.Length > 0)
                        {
                            lexemes.Enqueue(buffer);
                        }

                        buffer = "";

                        if (c == '\n')
                        {
                            buffer = c.ToString();
                            lexemes.Enqueue(buffer);
                            buffer = "";
                        }

                        continue;
                    }

                    if (c == '+' || c == '-' || c == '*' || c == '/' || c == ';' || c == '(' || c == ')' || c == '{' || c == '}' || c == ',')
                    {
                        if (buffer.Length > 0)
                        {
                            lexemes.Enqueue(buffer);
                        }

                        buffer = c.ToString();
                        lexemes.Enqueue(buffer);
                        buffer = "";
                        continue;
                    }

                    buffer += c.ToString();
                }
                else
                {
                    buffer += c.ToString();
                }
            }

            if (buffer.Length > 0)
            {
                lexemes.Enqueue(buffer);
            }

            return Tokenization(lexemes);
        }

        private Queue<Token> Tokenization(Queue<string> lexemes)
        {
            var tokens = new Queue<Token>();

            TokenType tokenType;
            int tokenLine = 1;

            foreach (string lexeme in lexemes)
            {
                int operatorTokenPrecedence = -1;

                if (lexeme == "\n")
                {
                    tokenLine++;
                    continue;
                }

                if (SyT.ReservedSymbols.ContainsKey(lexeme))
                {
                    tokenType = SyT.ReservedSymbols[lexeme].type;
                    operatorTokenPrecedence = SyT.ReservedSymbols[lexeme].operatorPrecedence;
                }
                else
                {
                    // Literals and identifiers
                    if (int.TryParse(lexeme, out int i))
                    {
                        if (i == 0 && lexeme.Length > 1) throw new NumberZeroFormatException(lexeme, tokenLine);
                        if (i != 0 && lexeme.StartsWith("0")) throw new IntegerNumberFormatException(lexeme, tokenLine);

                        tokenType = TokenType.Literal;
                    }
                    else if (double.TryParse(lexeme, out double d))
                    {
                        if (d == 0 && lexeme.Length > 1) throw new NumberZeroFormatException(lexeme, tokenLine);
                        if (d > 0 && d < 1 && lexeme.StartsWith("00")) throw new DecimalNumberZeroFormatException(lexeme, tokenLine);
                        if (d >= 1 && lexeme.StartsWith("0")) throw new DecimalNumberFormatException(lexeme, tokenLine);

                        tokenType = TokenType.Literal;
                    }
                    else if (lexeme.StartsWith("\"") && lexeme.EndsWith("\""))
                    {
                        tokenType = TokenType.Literal;
                    }
                    else if (char.IsDigit(lexeme[0]))
                    {
                        throw new IdentifierStartsWithNumberException(lexeme, tokenLine);
                    }
                    else
                    {
                        if (lexeme.Length > 200)
                        {
                            throw new IdentifierTooLongException(lexeme, tokenLine);
                        }

                        foreach (char c in lexeme)
                        {
                            if (!SyT.allowedChar.Contains(char.ToLower(c)))
                            {
                                throw new IdentifierUnallowedCharacterException(lexeme, c, tokenLine);
                            }
                        }

                        tokenType = TokenType.Identifier;
                    }
                }

                tokens.Enqueue(new Token(tokenType, lexeme, tokenLine, operatorTokenPrecedence));
            }

            // End of file token
            tokens.Enqueue(new Token(tokenLine));

            return tokens;
        }
    }
}
