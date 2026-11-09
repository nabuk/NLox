namespace NLox.Domain;

public class Parser(List<Token> tokens, IErrorReporter errorReporter)
{
    private int current = 0;

    public Expr? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }

    private Expr Expression() => Equality();

    private Expr Equality() =>
        LeftAssociativeBinary(Comparison, TokenType.BangEqual, TokenType.EqualEqual);

    private Expr Comparison() =>
        LeftAssociativeBinary(Term, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual);

    private Expr Term() =>
        LeftAssociativeBinary(Factor, TokenType.Minus, TokenType.Plus);
    
    private Expr Factor() =>
        LeftAssociativeBinary(Unary, TokenType.Slash, TokenType.Star);

    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            var @operator = Previous();
            var right = Unary();
            return new Expr.Unary(@operator, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(TokenType.False))
            return new Expr.Literal(false);
        if (Match(TokenType.True))
            return new Expr.Literal(true);
        if (Match(TokenType.Nil))
            return new Expr.Literal(null);
        if (Match(TokenType.Number, TokenType.String))
            return new Expr.Literal(Previous().Literal);
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        
        throw Error(Peek(), "Expect expression.");
    }
    
    private Expr LeftAssociativeBinary(Func<Expr> exprFactory, params TokenType[] types)
    {
        var expr = exprFactory();
        
        while (Match(types))
        {
            var @operator = Previous();
            var right = exprFactory();
            expr = new Expr.Binary(expr, @operator, right);
        }
        
        return expr;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
            return Advance();
        
        throw Error(Peek(), message);
    }
    
    private ParseError Error(Token token, string message) {
        errorReporter.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon)
                return;

            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }

    private bool Match(params TokenType[] types)
    {
        if (types.All(t => !Check(t)))
            return false;

        Advance();
        return true;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
            return false;

        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
            current++;

        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.Eof;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }
}