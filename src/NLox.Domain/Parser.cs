namespace NLox.Domain;

public class Parser(IReadOnlyList<Token> tokens, IErrorReporter errorReporter)
{
    private int current = 0;

    public IReadOnlyList<Stmt> Parse()
    {
        var result = new List<Stmt>();

        while (!IsAtEnd())
        {
            var d = Declaration();
            if (d != null)
                result.Add(d);
        }
        
        return result;
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.Var))
                return VarDeclaration();

            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect variable name.");
        Expr? initializer = null;
        if (Match(TokenType.Equal))
            initializer = Expression();
        
        Consume(TokenType.Semicolon,  "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt WhileStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after while.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after while.");
        var body = Statement();
        
        return new Stmt.While(condition, body);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.For))
            return ForStatement();
        
        if (Match(TokenType.If))
            return IfStatement();
        
        if (Match(TokenType.Print))
            return PrintStatement();
        
        if (Match(TokenType.While))
            return WhileStatement();

        if (Match(TokenType.LeftBrace))
            return new Stmt.Block(BlockStatement());

        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after for.");

        Stmt? initializer;
        if (Match(TokenType.Semicolon))
            initializer = null;
        else if (Match(TokenType.Var))
            initializer = VarDeclaration();
        else
            initializer = ExpressionStatement();

        Expr condition;
        if (Check(TokenType.Semicolon))
            condition = new Expr.Literal(true);
        else
            condition = Expression();
        
        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");
        
        Expr? increment = null;
        if (!Check(TokenType.RightParen))
            increment = Expression();
        
        Consume(TokenType.RightParen, "Expect ')' after for clauses.");
        
        var body = Statement();

        if (increment != null)
            body = new Stmt.Block([ body, new Stmt.Expression(increment) ]);
        
        body = new Stmt.While(condition, body);
        
        if (initializer != null)
            body = new Stmt.Block([ initializer, body ]);
        
        return body;
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after if.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after if.");

        var thenBranch = Statement();
        Stmt? elseBranch = null;
        
        if (Match(TokenType.Else))
            elseBranch = Statement();
        
        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new Stmt.Print(value);
    }
    
    private IReadOnlyList<Stmt?> BlockStatement()
    {
        var statements = new List<Stmt?>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
            statements.Add(Declaration());

        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }
    
    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Expr Expression() => Assignment();
    
    private Expr Assignment()
    {
        var expr = Or();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                return new Expr.Assign(variable.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }
        
        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(TokenType.Or))
        {
            var @operator = Previous();
            var right = And();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var @operator = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

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

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
                expr = FinishCall(expr);
            else
                break;
        }
        
        return  expr;
    }

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();
        if (!Check(TokenType.RightParen))
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        
        var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
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
        if (Match(TokenType.Identifier))
            return new Expr.Variable(Previous());
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