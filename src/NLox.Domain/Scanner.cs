using NLox.Domain.Contracts;

namespace NLox.Domain;

public class Scanner(string source, IErrorReporter errorReporter)
{
    private readonly List<Token> tokens = new List<Token>();
    private int start = 0;
    private int current = 0;
    private int line = 0;

    private readonly Dictionary<string, TokenType> keywords = new()
    {
        { "and", TokenType.And },
        { "class", TokenType.Class },
        { "else", TokenType.Else },
        { "false", TokenType.False },
        { "for", TokenType.For },
        { "fun", TokenType.Fun },
        { "if", TokenType.If },
        { "nil", TokenType.Nil },
        { "or", TokenType.Or },
        { "print", TokenType.Print },
        { "return", TokenType.Return },
        { "super", TokenType.Super },
        { "this", TokenType.This },
        { "true", TokenType.True },
        { "var", TokenType.Var },
        { "while", TokenType.While }
    };
    
    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            // We are at the beginning of the next lexeme.
            start = current;
            ScanToken();
        }
        
        tokens.Add(new Token(TokenType.Eof, "", null, line));
        return tokens;
    }

    private bool IsAtEnd()
    {
        return current >= source.Length;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c) {
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case '-': AddToken(TokenType.Minus); break;
            case '+': AddToken(TokenType.Plus); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '*': AddToken(TokenType.Star); break; 
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                
                break;
            
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            
            case '\n':
                line++;
                break;
            
            case '"':
                AddString();
                break;
                    
            default:
                if (IsDigit(c))
                {
                    AddNumber();
                }
                else if (IsAlpha(c))
                {
                    AddIdentifier();
                }
                else
                {
                    errorReporter.Error(line, "Unexpected character.");
                }
                
                break;
        }
    }

    private char Peek()
    {
        if (IsAtEnd())
            return '\0';
        
        return source[current];
    }
    
    private char PeekNext()
    {
        if (current + 1 >= source.Length)
            return '\0';
        
        return source[current + 1];
    }

    private char Advance()
    {
        return source[current++];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd())
            return false;
        
        if (source[current] != expected)
            return false;
        
        current++;
        return true;
    }

    private bool IsDigit(char c)
    {
        return  c is >= '0' and <= '9';
    }

    private bool IsAlpha(char c)
    {
        return c
            is >= 'a' and <= 'z'
            or >= 'A' and <= 'Z'
            or '_';
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) ||  IsDigit(c);
    }

    private void AddString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
                line++;
            
            Advance();
        }

        if (IsAtEnd())
        {
            errorReporter.Error(line, "Unterminated string.");
            return;
        }

        // The closing double quote.
        Advance();
        
        // Trim the surrounding quotes.
        var value = source.Substring(start + 1, current - start - 1);
        AddToken(TokenType.String, value);
    }

    private void AddNumber()
    {
        while (IsDigit(Peek()))
            Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            
            while (IsDigit(Peek()))
                Advance();
        }

        var strValue = source.Substring(start, current - start);
        var value = double.Parse(strValue);
        AddToken(TokenType.Number, value);
    }

    private void AddIdentifier()
    {
        while (IsAlphaNumeric(Peek()))
            Advance();
        
        var text = source.Substring(start, current - start);
        var type = keywords.GetValueOrDefault(text, TokenType.Identifier);
        AddToken(type);
    }

    private void AddToken(TokenType type, object? literal = null)
    {
        var text  = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}