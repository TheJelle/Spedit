using System;
using System.Collections.Generic;
using System.Text;
using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
	    private int ConsumeSMFunction()
	    {
	        var kind = SMFunctionKind.Unknown;
	        var startPosition = _position;
	        var iteratePosition = startPosition + 1;

	        switch (_t[startPosition].Value)
	        {
	            case "stock":
	            {
	                if (startPosition + 1 < _length)
	                    if (_t[startPosition + 1].Kind == TokenKind.FunctionIndicator)
	                        if (_t[startPosition + 1].Value == "static")
	                        {
	                            kind = SMFunctionKind.StockStatic;
	                            ++iteratePosition;
	                            break;
	                        }
	                kind = SMFunctionKind.Stock;
	                break;
	            }
	            case "native":
	            {
	                kind = SMFunctionKind.Native;
	                break;
	            }
	            case "forward":
	            {
	                kind = SMFunctionKind.Forward;
	                break;
	            }
	            case "public":
	            {
	                if (startPosition + 1 < _length)
	                    if (_t[startPosition + 1].Kind == TokenKind.FunctionIndicator)
	                        if (_t[startPosition + 1].Value == "native")
	                        {
	                            kind = SMFunctionKind.PublicNative;
	                            ++iteratePosition;
	                            break;
	                        }
	                kind = SMFunctionKind.Public;
	                break;
	            }
	            case "static":
	            {
	                kind = SMFunctionKind.Static;
	                break;
	            }
	            case "normal":
	            {
	                kind = SMFunctionKind.Normal;
	                break;
	            }
	        }

	        var functionCommentString = string.Empty;
	        var commentTokenIndex = BacktraceTestForToken(startPosition - 1, TokenKind.MultiLineComment, true, false);

	        if (commentTokenIndex == -1)
	        {
	            commentTokenIndex = BacktraceTestForToken(startPosition - 1, TokenKind.SingleLineComment, true, false);
	            if (commentTokenIndex != -1)
	            {
	                var strBuilder = new StringBuilder(_t[commentTokenIndex].Value);

	                while (
	                (commentTokenIndex =
	                    BacktraceTestForToken(commentTokenIndex - 1, TokenKind.SingleLineComment, true, false)) != -1)
	                {
	                    strBuilder.Insert(0, Environment.NewLine);
	                    strBuilder.Insert(0, _t[commentTokenIndex].Value);
	                }

	                functionCommentString = strBuilder.ToString();
	            }
	        }
	        else
	        {
	            functionCommentString = _t[commentTokenIndex].Value;
	        }

	        string functionReturnType = string.Empty, functionName = string.Empty;

	        for (; iteratePosition < startPosition + 5; ++iteratePosition)
	        {
	            if (_t.Length <= iteratePosition + 1)
	                return -1;

	            if (_t[iteratePosition].Kind == TokenKind.Identifier)
	            {
	                if (_t[iteratePosition + 1].Kind == TokenKind.ParenthesisOpen)
	                {
	                    functionName = _t[iteratePosition].Value;
	                    break;
	                }

	                functionReturnType = _t[iteratePosition].Value;
	                continue;
	            }

	            if (_t[iteratePosition].Kind != TokenKind.Character)
	                return -1;

	            if (_t[iteratePosition].Value.Length <= 0)
	                return -1;

	            var testChar = _t[iteratePosition].Value[0];

	            if (testChar == ':' || testChar == '[' || testChar == ']')
	                continue;
	            return -1;

	            return -1;
	        }
	        if (string.IsNullOrEmpty(functionName))
	            return -1;

	        ++iteratePosition;

	        var functionParameters = new List<string>();
	        var parameterDeclIndexStart = _t[iteratePosition].Index;
	        var parameterDeclIndexEnd = -1;
	        var lastParameterIndex = parameterDeclIndexStart;
	        var parenthesisCounter = 0;
	        var gotCommaBreak = false;
	        var outTokenIndex = -1;
	        var braceState = 0;

	        for (; iteratePosition < _length; ++iteratePosition)
	        {
	            if (_t[iteratePosition].Kind == TokenKind.ParenthesisOpen)
	            {
	                ++parenthesisCounter;
	                continue;
	            }

	            if (_t[iteratePosition].Kind == TokenKind.ParenthesisClose)
	            {
	                --parenthesisCounter;
	                if (parenthesisCounter == 0)
	                {
	                    outTokenIndex = iteratePosition;
	                    parameterDeclIndexEnd = _t[iteratePosition].Index;

	                    var length = _t[iteratePosition].Index - 1 - (lastParameterIndex + 1);

	                    if (gotCommaBreak)
	                    {
	                        functionParameters.Add(length == 0
	                            ? string.Empty
	                            : _source.Substring(lastParameterIndex + 1, length + 1).Trim());
	                    }
	                    else if (length > 0)
	                    {
	                        var singleParameterString = _source.Substring(lastParameterIndex + 1, length + 1);
	                        if (!string.IsNullOrWhiteSpace(singleParameterString))
	                            functionParameters.Add(singleParameterString);
	                    }
	                    break;
	                }
	                continue;
	            }
	            if (_t[iteratePosition].Kind == TokenKind.BraceOpen)
	                ++braceState;
	            if (_t[iteratePosition].Kind == TokenKind.BraceClose)
	                --braceState;

	            if (_t[iteratePosition].Kind != TokenKind.Comma || braceState != 0)
	                continue;
	            {
	                gotCommaBreak = true;
	                var length = _t[iteratePosition].Index - 1 - (lastParameterIndex + 1);
	                functionParameters.Add(length == 0
	                    ? string.Empty
	                    : _source.Substring(lastParameterIndex + 1, length + 1).Trim());
	                lastParameterIndex = _t[iteratePosition].Index;
	            }
	        }
	        if (parameterDeclIndexEnd == -1)
	            return -1;
	        _def.Functions.Add(new SMFunction
	        {
	            FunctionKind = kind,
	            Index = _t[startPosition].Index,
	            File = _fileName,
	            Length = parameterDeclIndexEnd - _t[startPosition].Index + 1,
	            Name = functionName,
	            FullName =
	                TrimFullname(_source.Substring(_t[startPosition].Index,
	                    parameterDeclIndexEnd - _t[startPosition].Index + 1)),
	            ReturnType = functionReturnType,
	            CommentString = TrimComments(functionCommentString),
	            Parameters = functionParameters.ToArray()
	        });

	        if (outTokenIndex + 1 >= _length)
	            return outTokenIndex;

	        if (_t[outTokenIndex + 1].Kind == TokenKind.Semicolon)
	            return outTokenIndex + 1;

	        var nextOpenBraceTokenIndex = FortraceTestForToken(outTokenIndex + 1, TokenKind.BraceOpen, true, false);

	        if (nextOpenBraceTokenIndex == -1)
	            return outTokenIndex;

	        braceState = 0;

	        for (var i = nextOpenBraceTokenIndex; i < _length; ++i)
	            switch (_t[i].Kind)
	            {
	                case TokenKind.BraceOpen:
	                    ++braceState;
	                    break;
	                case TokenKind.BraceClose:
	                    --braceState;
	                    if (braceState == 0)
	                        return i;
	                    break;
	                default:
	                    // ignored
	                    break;
	            }
	        return outTokenIndex;
	    }
	}
}
