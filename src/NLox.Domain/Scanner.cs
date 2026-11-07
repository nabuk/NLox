using NLox.Domain.Contracts;

namespace NLox.Domain;

public class Scanner(string source, IErrorReporter errorReporter)
{
    private readonly List<Token> tokens = new List<Token>();
    private int start = 0;
    private int current = 0;
    private int line = 0;
    
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
                    
            default:
                errorReporter.Error(line, "Unexpected character.");
                break;
        }
    }

    private char Peek()
    {
        if (IsAtEnd())
            return '\0';
        
        return source[current];
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

    private void AddToken(TokenType type, object? literal = null)
    {
        var text  = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}