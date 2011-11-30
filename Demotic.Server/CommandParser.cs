using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Demotic.Server
{
    static class CommandParser
    {
        public class ParseErrorException : Exception
        {
            public ParseErrorException(string message) : base(message) {}
        }

        #region Lexer
        private enum TokenId
        {
            NONE = 0,
            STRING,
            NUMBER,
            GET,
            PUT,
            DO,
            WS
        }

        // note: \G anchors matches at exactly the start position passed to .Match();
        // ^ anchors them at character 0)
        private static Regex _StringLiteral = new Regex(@"\G""(?<val>([^""]|\"")*)""");
        private static Regex _NumberLiteral = new Regex(@"\G(?<val>(\+|-)?\d+)");
        private static Regex _Get = new Regex(@"\Gget", RegexOptions.IgnoreCase);
        private static Regex _Put = new Regex(@"\Gput", RegexOptions.IgnoreCase);
        private static Regex _Do = new Regex(@"\Gdo", RegexOptions.IgnoreCase);
        private static Regex _Whitespace = new Regex(@"\G\s+");

        private static Token Next(ParserState state, out ParserState newState)
        {
            var matchers = new[] 
            { 
                new { Id = TokenId.STRING, Regex = _StringLiteral, Skip = false }, 
                new { Id = TokenId.NUMBER, Regex = _NumberLiteral, Skip = false },
                new { Id = TokenId.GET,    Regex = _Get,           Skip = false },
                new { Id = TokenId.PUT,    Regex = _Put,           Skip = false },
                new { Id = TokenId.DO,     Regex = _Do,            Skip = false },
                new { Id = TokenId.WS,     Regex = _Whitespace,    Skip = true },
            };
            ParserState curState = state;

            while (true)
            {
                // it's possible for us to evaluate multiple tokens iff one or more are skipped
                // in the output token stream.
                bool validTokenSkipped = false;

                foreach (var matcher in matchers)
                {
                    Match m = matcher.Regex.Match(curState.Input, curState.Cursor);

                    if (m.Success)
                    {
                        curState =
                            new ParserState { Input = curState.Input, Cursor = curState.Cursor + m.Length };

                        if (matcher.Skip)
                        {
                            // break out of the foreach so we evaluate the next token.
                            validTokenSkipped = true;
                            break;
                        }
                        else
                        {
                            Token next = new Token { Id = matcher.Id, Match = m };

                            // we found a non-skip token.  return it.
                            newState = curState;
                            return next;
                        }
                    }
                }

                if (!validTokenSkipped)
                {
                    // we fell out of the foreach without hitting a skip token.  next token
                    // can't be lexed.  bummer.
                    newState = state;
                    return new Token();
                }
            }
        }

        private static Token Expect(ParserState state, TokenId id, out ParserState newState)
        {
            Token t = Next(state, out newState);

            if (t.Id != id)
            {
                throw new ParseErrorException(
                    message: string.Format("expectation failed; expected {0}, got {1}",
                                           id, t.Id)
                );
            }

            return t;
        }
        #endregion

        private struct Token
        {
            public TokenId Id { get; set; }
            public Match Match { get; set; }
        }

        #region Parser and generator
        /// <summary>
        ///   Remove escape sequences from a user-provided string literal (e.g., the sequence
        ///   \" is replaced with a double quote character.)
        /// </summary>
        /// <param name="s">the escaped string</param>
        /// <returns>an unescaped version of s</returns>
        private static string Unescape(string s)
        {
            return s.Replace(@"\""", @"""")        // \" -> "
                ;
        }

        private static UserAction ParseGetArguments(IPresentationClient client, ParserState state)
        {
            ParserState leftovers;

            Token path = Expect(state, TokenId.STRING, out leftovers);

            return new GetObjectAction(client, Unescape(path.Match.Groups["val"].Value));
        }

        private static UserAction ParsePutArguments(IPresentationClient client, ParserState state)
        {
            ParserState leftovers;

            Token path = Expect(state, TokenId.STRING, out leftovers);
            Token value = Expect(leftovers, TokenId.NUMBER, out leftovers);

            int i = int.Parse(value.Match.Groups["val"].Value);

            return new PutNumberAction(client, Unescape(path.Match.Groups["val"].Value), i);
        }

        private static UserAction ParseDoArguments(IPresentationClient client, ParserState state)
        {
            ParserState leftovers;

            Token trigger = Expect(state, TokenId.STRING, out leftovers);
            Token body = Expect(state, TokenId.STRING, out leftovers);

            return null;
        }

        public static UserAction ParseCommandLine(IPresentationClient client, string line)
        {
            ParserState state = new ParserState { Input = line, Cursor = 0 };

            Token cmd = Next(state, out state);

            switch (cmd.Id)
            {
                case TokenId.GET: return ParseGetArguments(client, state);
                case TokenId.PUT: return ParsePutArguments(client, state);
                case TokenId.DO: return ParseDoArguments(client, state);
                default:
                    throw new ParseErrorException(
                        message: string.Format("expected command, got {0}", cmd.Id)
                    );
            }
        }
        #endregion

        private struct ParserState
        {
            public string Input { get; set; }
            public int Cursor { get; set; }
        }
    }
}
