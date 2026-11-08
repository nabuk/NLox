namespace NLox.Domain;

public class Token(TokenType type, string lexeme, object? literal, int line)
{
    public TokenType Type => type;
    public string Lexeme => lexeme;
    public object? Literal => literal;
    public int Line => line;
    
    public override string ToString()
    {
        return $"{type}  {lexeme} {literal}";
    }
}