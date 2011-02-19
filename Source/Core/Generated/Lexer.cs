namespace PHP.Core.Parsers
{
	#region User Code
	
	/*
 Copyright (c) 2004-2006 Tomas Matousek. Based on PHP5 and PHP6 grammar tokens definition. 
 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 You must not remove this notice from this software.
*/
using System;
using PHP.Core;
using System.Collections.Generic;
#endregion
	
	
	public partial class Lexer
	{
		public enum LexicalStates
		{
			INITIAL = 0,
			ST_IN_SCRIPTING = 1,
			ST_DOUBLE_QUOTES = 2,
			ST_SINGLE_QUOTES = 3,
			ST_BACKQUOTE = 4,
			ST_HEREDOC = 5,
			ST_LOOKING_FOR_PROPERTY = 6,
			ST_LOOKING_FOR_VARNAME = 7,
			ST_DOC_COMMENT = 8,
			ST_COMMENT = 9,
			ST_ONE_LINE_COMMENT = 10,
		}
		
		[Flags]
		private enum AcceptConditions : byte
		{
			NotAccept = 0,
			AcceptOnStart = 1,
			AcceptOnEnd = 2,
			Accept = 4
		}
		
		public struct Position
		{
			public int Char;
			public int Line;
			public int Column;
			public Position(int ch, int line, int column)
			{
				this.Char = ch;
				this.Line = line;
				this.Column = column;
			}
		}
		private const int NoState = -1;
		private const char BOL = (char)128;
		private const char EOF = (char)129;
		
		private Tokens yyreturn;
		
		private System.IO.TextReader reader;
		private char[] buffer = new char[512];
		
		// whether the currently parsed token is being expanded (yymore has been called):
		private bool expanding_token;
		
		// offset in buffer where the currently parsed token starts:
		private int token_start;
		
		// offset in buffer where the currently parsed token chunk starts:
		private int token_chunk_start;
		
		// offset in buffer one char behind the currently parsed token (chunk) ending character:
		private int token_end;
		
		// offset of the lookahead character (number of characters parsed):
		private int lookahead_index;
		
		// number of characters read into the buffer:
		private int chars_read;
		
		// parsed token start position (wrt beginning of the stream):
		protected Position token_start_pos;
		
		// parsed token end position (wrt beginning of the stream):
		protected Position token_end_pos;
		
		private bool yy_at_bol = false;
		
		public LexicalStates CurrentLexicalState { get { return current_lexical_state; } set { current_lexical_state = value; } } 
		private LexicalStates current_lexical_state;
		
		public Lexer(System.IO.TextReader reader)
		{
			Initialize(reader, LexicalStates.INITIAL);
		}
		
		public void Initialize(System.IO.TextReader reader, LexicalStates lexicalState, bool atBol)
		{
			this.expanding_token = false;
			this.token_start = 0;
			this.chars_read = 0;
			this.lookahead_index = 0;
			this.token_chunk_start = 0;
			this.token_end = 0;
			this.reader = reader;
			this.yy_at_bol = atBol;
			this.current_lexical_state = lexicalState;
		}
		
		public void Initialize(System.IO.TextReader reader, LexicalStates lexicalState)
		{
			Initialize(reader, lexicalState, false);
		}
		
		#region Accept
		
		#pragma warning disable 162
		
		
		Tokens Accept0(int state,out bool accepted)
		{
			accepted = true;
			
			switch(state)
			{
				case 2:
					// #line 75
					{ 
						return Tokens.T_INLINE_HTML; 
					}
					break;
					
				case 3:
					// #line 103
					{ 
						if (AllowAspTags)
						{
							BEGIN(LexicalStates.ST_IN_SCRIPTING);
							return Tokens.T_OPEN_TAG;
						} 
						else 
						{
							return Tokens.T_INLINE_HTML;
						}
					}
					break;
					
				case 4:
					// #line 79
					{
						if (AllowShortTags || TokenLength>2) 
						{ 
							BEGIN(LexicalStates.ST_IN_SCRIPTING);
							return Tokens.T_OPEN_TAG;
						} 
						else 
						{
							return Tokens.T_INLINE_HTML;
						}
					}
					break;
					
				case 5:
					// #line 91
					{
						if (GetTokenChar(1) == '%' && AllowAspTags || GetTokenChar(1) == '?' && AllowShortTags) 
						{
							BEGIN(LexicalStates.ST_IN_SCRIPTING);
							return Tokens.T_OPEN_TAG_WITH_ECHO;
						} 
						else 
						{
							return Tokens.T_INLINE_HTML;
						}
					}
					break;
					
				case 6:
					// #line 115
					{
						BEGIN(LexicalStates.ST_IN_SCRIPTING);
						return Tokens.T_OPEN_TAG;
					}
					break;
					
				case 7:
					// #line 270
					{ return (Tokens)GetTokenChar(0); }
					break;
					
				case 8:
					// #line 357
					{ return Tokens.ERROR; }
					break;
					
				case 9:
					// #line 271
					{ return Tokens.T_STRING; }
					break;
					
				case 10:
					// #line 273
					{ return Tokens.T_WHITESPACE; }
					break;
					
				case 11:
					// #line 329
					{ 
						BEGIN(LexicalStates.ST_DOUBLE_QUOTES); 
						return (GetTokenChar(0) != '"') ? Tokens.T_BINARY_DOUBLE : Tokens.T_DOUBLE_QUOTES; 
					}
					break;
					
				case 12:
					// #line 335
					{ 
						// Gets here only in the case of unterminated singly-quoted string. That leads usually to an error token,
						// however when the source code is parsed per-line (as in Visual Studio colorizer) it is important to remember
						// that we are in the singly-quoted string at the end of the line.
						BEGIN(LexicalStates.ST_SINGLE_QUOTES); 
						yymore(); 
						break; 
					}
					break;
					
				case 13:
					// #line 274
					{ return Tokens.ParseDecimalNumber; }
					break;
					
				case 14:
					// #line 284
					{ BEGIN(LexicalStates.ST_ONE_LINE_COMMENT); yymore(); break; }
					break;
					
				case 15:
					// #line 307
					{ yy_push_state(LexicalStates.ST_IN_SCRIPTING); return Tokens.T_LBRACE; }
					break;
					
				case 16:
					// #line 308
					{ if (!yy_pop_state()) return Tokens.ERROR; return Tokens.T_RBRACE; }
					break;
					
				case 17:
					// #line 345
					{ 
						BEGIN(LexicalStates.ST_BACKQUOTE); 
						return Tokens.T_BACKQUOTE; 
					}
					break;
					
				case 18:
					// #line 255
					{ return Tokens.T_MOD_EQUAL; }
					break;
					
				case 19:
					// #line 310
					{
						if (AllowAspTags) 
						{
							BEGIN(LexicalStates.INITIAL);
							return Tokens.T_CLOSE_TAG;
						} 
						else
						{
							yyless(1);
							return Tokens.T_PERCENT;
						}
					}
					break;
					
				case 20:
					// #line 263
					{ return Tokens.T_SL; }
					break;
					
				case 21:
					// #line 248
					{ return Tokens.T_IS_SMALLER_OR_EQUAL; }
					break;
					
				case 22:
					// #line 247
					{ return Tokens.T_IS_NOT_EQUAL; }
					break;
					
				case 23:
					// #line 222
					{ return Tokens.T_LGENERIC; }
					break;
					
				case 24:
					// #line 123
					{ 
						BEGIN(LexicalStates.INITIAL); 
						return Tokens.T_CLOSE_TAG; 
					}
					break;
					
				case 25:
					// #line 217
					{ return (InLinq) ? Tokens.T_LINQ_IN : Tokens.T_STRING; }
					break;
					
				case 26:
					// #line 137
					{ return Tokens.T_IF; }
					break;
					
				case 27:
					// #line 148
					{ return Tokens.T_AS; }
					break;
					
				case 28:
					// #line 246
					{ return Tokens.T_IS_EQUAL; }
					break;
					
				case 29:
					// #line 241
					{ return Tokens.T_DOUBLE_ARROW; }
					break;
					
				case 30:
					// #line 323
					{ return Tokens.DoubleQuotedString; }
					break;
					
				case 31:
					// #line 324
					{ return Tokens.SingleQuotedString; }
					break;
					
				case 32:
					// #line 249
					{ return Tokens.T_IS_GREATER_OR_EQUAL; }
					break;
					
				case 33:
					// #line 264
					{ return Tokens.T_SR; }
					break;
					
				case 34:
					// #line 253
					{ return Tokens.T_DIV_EQUAL; }
					break;
					
				case 35:
					// #line 285
					{ BEGIN(LexicalStates.ST_ONE_LINE_COMMENT); yymore(); break; }
					break;
					
				case 36:
					// #line 287
					{ BEGIN(LexicalStates.ST_COMMENT); yymore(); break; }
					break;
					
				case 37:
					// #line 143
					{ return Tokens.T_DO; }
					break;
					
				case 38:
					// #line 171
					{ return Tokens.T_LOGICAL_OR; }
					break;
					
				case 39:
					// #line 218
					{ return (InLinq) ? Tokens.T_LINQ_BY : Tokens.T_STRING; }
					break;
					
				case 40:
					// #line 276
					{ return Tokens.ParseDouble; }
					break;
					
				case 41:
					// #line 223
					{ return Tokens.T_RGENERIC; }
					break;
					
				case 42:
					// #line 265
					{ return Tokens.T_DOUBLE_COLON; }
					break;
					
				case 43:
					// #line 250
					{ return Tokens.T_PLUS_EQUAL; }
					break;
					
				case 44:
					// #line 242
					{ return Tokens.T_INC; }
					break;
					
				case 45:
					// #line 251
					{ return Tokens.T_MINUS_EQUAL; }
					break;
					
				case 46:
					// #line 267
					{ yy_push_state(LexicalStates.ST_LOOKING_FOR_PROPERTY); return Tokens.T_OBJECT_OPERATOR; }
					break;
					
				case 47:
					// #line 243
					{ return Tokens.T_DEC; }
					break;
					
				case 48:
					// #line 252
					{ return Tokens.T_MUL_EQUAL; }
					break;
					
				case 49:
					// #line 254
					{ return Tokens.T_CONCAT_EQUAL; }
					break;
					
				case 50:
					// #line 258
					{ return Tokens.T_AND_EQUAL; }
					break;
					
				case 51:
					// #line 262
					{ return Tokens.T_BOOLEAN_AND; }
					break;
					
				case 52:
					// #line 259
					{ return Tokens.T_OR_EQUAL; }
					break;
					
				case 53:
					// #line 261
					{ return Tokens.T_BOOLEAN_OR; }
					break;
					
				case 54:
					// #line 260
					{ return Tokens.T_XOR_EQUAL; }
					break;
					
				case 55:
					// #line 268
					{ return Tokens.T_VARIABLE; }
					break;
					
				case 56:
					// #line 256
					{ return Tokens.T_SL_EQUAL; }
					break;
					
				case 57:
					// #line 202
					{ return Tokens.T_INT_TYPE; }
					break;
					
				case 58:
					// #line 326
					{ return Tokens.ErrorInvalidIdentifier; }
					break;
					
				case 59:
					// #line 183
					{ return Tokens.T_TRY; }
					break;
					
				case 60:
					// #line 172
					{ return Tokens.T_LOGICAL_AND; }
					break;
					
				case 61:
					// #line 159
					{ return Tokens.T_NEW; }
					break;
					
				case 62:
					// #line 128
					{
					  return Tokens.ErrorNotSupported; 
					}
					break;
					
				case 63:
					// #line 244
					{ return Tokens.T_IS_IDENTICAL; }
					break;
					
				case 64:
					// #line 257
					{ return Tokens.T_SR_EQUAL; }
					break;
					
				case 65:
					// #line 133
					{ return Tokens.T_EXIT; }
					break;
					
				case 66:
					// #line 173
					{ return Tokens.T_LOGICAL_XOR; }
					break;
					
				case 67:
					// #line 144
					{ return Tokens.T_FOR; }
					break;
					
				case 68:
					// #line 160
					{ return Tokens.T_VAR; }
					break;
					
				case 69:
					// #line 277
					{ return Tokens.ParseDouble; }
					break;
					
				case 70:
					// #line 245
					{ return Tokens.T_IS_NOT_IDENTICAL; }
					break;
					
				case 71:
					// #line 275
					{ return Tokens.ParseHexadecimalNumber; }
					break;
					
				case 72:
					// #line 235
					{ return Tokens.T_SELF; }
					break;
					
				case 73:
					// #line 151
					{ return Tokens.T_CASE; }
					break;
					
				case 74:
					// #line 325
					{ return Tokens.SingleQuotedIdentifier; }
					break;
					
				case 75:
					// #line 237
					{ return Tokens.T_TRUE; }
					break;
					
				case 76:
					// #line 174
					{ return Tokens.T_LIST; }
					break;
					
				case 77:
					// #line 239
					{ return Tokens.T_NULL; }
					break;
					
				case 78:
					// #line 199
					{ return Tokens.T_GOTO; }
					break;
					
				case 79:
					// #line 155
					{ return Tokens.T_ECHO; }
					break;
					
				case 80:
					// #line 140
					{ return Tokens.T_ELSE; }
					break;
					
				case 81:
					// #line 132
					{ return Tokens.T_EXIT; }
					break;
					
				case 82:
					// #line 161
					{ return Tokens.T_EVAL; }
					break;
					
				case 83:
					// #line 286
					{ BEGIN(LexicalStates.ST_DOC_COMMENT); yymore(); break; }
					break;
					
				case 84:
					// #line 210
					{ return Tokens.T_LINQ_FROM; }
					break;
					
				case 85:
					// #line 201
					{ return Tokens.T_BOOL_TYPE; }
					break;
					
				case 86:
					// #line 350
					{
						bool is_binary = GetTokenChar(0) != '<';
						hereDocLabel = GetTokenSubstring(is_binary ? 4 : 3).Trim();
						BEGIN(LexicalStates.ST_HEREDOC);
						return is_binary ? Tokens.T_BINARY_HEREDOC : Tokens.T_START_HEREDOC;
					}
					break;
					
				case 87:
					// #line 272
					{ return Tokens.T_NAMESPACE_NAME; }
					break;
					
				case 88:
					// #line 157
					{ return Tokens.T_CLASS; }
					break;
					
				case 89:
					// #line 188
					{ return Tokens.T_CLONE; }
					break;
					
				case 90:
					// #line 184
					{ return Tokens.T_CATCH; }
					break;
					
				case 91:
					// #line 135
					{ return Tokens.T_CONST; }
					break;
					
				case 92:
					// #line 167
					{ return Tokens.T_ISSET; }
					break;
					
				case 93:
					// #line 203
					{ return Tokens.T_INT64_TYPE; }
					break;
					
				case 94:
					// #line 156
					{ return Tokens.T_PRINT; }
					break;
					
				case 95:
					// #line 185
					{ return Tokens.T_THROW; }
					break;
					
				case 96:
					// #line 175
					{ return Tokens.T_ARRAY; }
					break;
					
				case 97:
					// #line 216
					{ return (InLinq) ? Tokens.T_LINQ_GROUP : Tokens.T_STRING; }
					break;
					
				case 98:
					// #line 170
					{ return Tokens.T_UNSET; }
					break;
					
				case 99:
					// #line 139
					{ return Tokens.T_ENDIF; }
					break;
					
				case 100:
					// #line 168
					{ return Tokens.T_EMPTY; }
					break;
					
				case 101:
					// #line 190
					{ return Tokens.T_FINAL; }
					break;
					
				case 102:
					// #line 238
					{ return Tokens.T_FALSE; }
					break;
					
				case 103:
					// #line 141
					{ return Tokens.T_WHILE; }
					break;
					
				case 104:
					// #line 211
					{ return (InLinq) ? Tokens.T_LINQ_WHERE : Tokens.T_STRING; }
					break;
					
				case 105:
					// #line 153
					{ return Tokens.T_BREAK; }
					break;
					
				case 106:
					// #line 227
					{ return Tokens.T_SET; }
					break;
					
				case 107:
					// #line 226
					{ return Tokens.T_GET; }
					break;
					
				case 108:
					// #line 291
					{ return Tokens.T_INT32_CAST; }
					break;
					
				case 109:
					// #line 205
					{ return Tokens.T_STRING_TYPE; }
					break;
					
				case 110:
					// #line 169
					{ return Tokens.T_STATIC; }
					break;
					
				case 111:
					// #line 215
					{ return (InLinq) ? Tokens.T_LINQ_SELECT : Tokens.T_STRING; }
					break;
					
				case 112:
					// #line 149
					{ return Tokens.T_SWITCH; }
					break;
					
				case 113:
					// #line 136
					{ return Tokens.T_RETURN; }
					break;
					
				case 114:
					// #line 198
					{ return Tokens.T_IMPORT; }
					break;
					
				case 115:
					// #line 234
					{ return Tokens.T_PARENT; }
					break;
					
				case 116:
					// #line 193
					{ return Tokens.T_PUBLIC; }
					break;
					
				case 117:
					// #line 225
					{ return Tokens.T_ASSERT; }
					break;
					
				case 118:
					// #line 166
					{ return Tokens.T_GLOBAL; }
					break;
					
				case 119:
					// #line 138
					{ return Tokens.T_ELSEIF; }
					break;
					
				case 120:
					// #line 145
					{ return Tokens.T_ENDFOR; }
					break;
					
				case 121:
					// #line 204
					{ return Tokens.T_DOUBLE_TYPE; }
					break;
					
				case 122:
					// #line 207
					{ return Tokens.T_OBJECT_TYPE; }
					break;
					
				case 123:
					// #line 228
					{ return Tokens.T_CALL; }
					break;
					
				case 124:
					// #line 297
					{ return Tokens.T_DOUBLE_CAST; }
					break;
					
				case 125:
					// #line 289
					{ return Tokens.T_INT8_CAST; }
					break;
					
				case 126:
					// #line 295
					{ return Tokens.T_UINT32_CAST; }
					break;
					
				case 127:
					// #line 304
					{ return Tokens.T_BOOL_CAST; }
					break;
					
				case 128:
					// #line 164
					{ return Tokens.T_REQUIRE; }
					break;
					
				case 129:
					// #line 162
					{ return Tokens.T_INCLUDE; }
					break;
					
				case 130:
					// #line 191
					{ return Tokens.T_PRIVATE; }
					break;
					
				case 131:
					// #line 220
					{ return Tokens.T_PARTIAL; }
					break;
					
				case 132:
					// #line 158
					{ return Tokens.T_EXTENDS; }
					break;
					
				case 133:
					// #line 152
					{ return Tokens.T_DEFAULT; }
					break;
					
				case 134:
					// #line 146
					{ return Tokens.T_FOREACH; }
					break;
					
				case 135:
					// #line 212
					{ return (InLinq) ? Tokens.T_LINQ_ORDERBY : Tokens.T_STRING; }
					break;
					
				case 136:
					// #line 233
					{ return Tokens.T_SLEEP; }
					break;
					
				case 137:
					// #line 181
					{ return Tokens.T_DIR; }
					break;
					
				case 138:
					// #line 292
					{ return Tokens.T_INT64_CAST; }
					break;
					
				case 139:
					// #line 290
					{ return Tokens.T_INT16_CAST; }
					break;
					
				case 140:
					// #line 302
					{ return Tokens.T_ARRAY_CAST; }
					break;
					
				case 141:
					// #line 293
					{ return Tokens.T_UINT8_CAST; }
					break;
					
				case 142:
					// #line 305
					{ return Tokens.T_UNSET_CAST; }
					break;
					
				case 143:
					// #line 298
					{ return Tokens.T_FLOAT_CAST; }
					break;
					
				case 144:
					// #line 154
					{ return Tokens.T_CONTINUE; }
					break;
					
				case 145:
					// #line 206
					{ return Tokens.T_RESOURCE_TYPE; }
					break;
					
				case 146:
					// #line 189
					{ return Tokens.T_ABSTRACT; }
					break;
					
				case 147:
					// #line 142
					{ return Tokens.T_ENDWHILE; }
					break;
					
				case 148:
					// #line 134
					{ return Tokens.T_FUNCTION; }
					break;
					
				case 149:
					// #line 179
					{ return Tokens.T_LINE; }
					break;
					
				case 150:
					// #line 180
					{ return Tokens.T_FILE; }
					break;
					
				case 151:
					// #line 232
					{ return Tokens.T_WAKEUP; }
					break;
					
				case 152:
					// #line 299
					{ return Tokens.T_STRING_CAST; }
					break;
					
				case 153:
					// #line 296
					{ return Tokens.T_UINT64_CAST; }
					break;
					
				case 154:
					// #line 294
					{ return Tokens.T_UINT16_CAST; }
					break;
					
				case 155:
					// #line 303
					{ return Tokens.T_OBJECT_CAST; }
					break;
					
				case 156:
					// #line 300
					{ return Tokens.T_BINARY_CAST; }
					break;
					
				case 157:
					// #line 208
					{ return Tokens.T_TYPEOF; }
					break;
					
				case 158:
					// #line 186
					{ return Tokens.T_INTERFACE; }
					break;
					
				case 159:
					// #line 192
					{ return Tokens.T_PROTECTED; }
					break;
					
				case 160:
					// #line 214
					{ return (InLinq) ? Tokens.T_LINQ_ASCENDING : Tokens.T_STRING; }
					break;
					
				case 161:
					// #line 197
					{ return Tokens.T_NAMESPACE; }
					break;
					
				case 162:
					// #line 150
					{ return Tokens.T_ENDSWITCH; }
					break;
					
				case 163:
					// #line 176
					{ return Tokens.T_CLASS_C; }
					break;
					
				case 164:
					// #line 301
					{ return Tokens.T_UNICODE_CAST; }
					break;
					
				case 165:
					// #line 194
					{ return Tokens.T_INSTANCEOF; }
					break;
					
				case 166:
					// #line 187
					{ return Tokens.T_IMPLEMENTS; }
					break;
					
				case 167:
					// #line 147
					{ return Tokens.T_ENDFOREACH; }
					break;
					
				case 168:
					// #line 213
					{ return (InLinq) ? Tokens.T_LINQ_DESCENDING : Tokens.T_STRING; }
					break;
					
				case 169:
					// #line 229
					{ return Tokens.T_TOSTRING; }
					break;
					
				case 170:
					// #line 236
					{ return Tokens.T_AUTOLOAD; }
					break;
					
				case 171:
					// #line 231
					{ return Tokens.T_DESTRUCT; }
					break;
					
				case 172:
					// #line 178
					{ return Tokens.T_METHOD_C; }
					break;
					
				case 173:
					// #line 230
					{ return Tokens.T_CONSTRUCT; }
					break;
					
				case 174:
					// #line 165
					{ return Tokens.T_REQUIRE_ONCE; }
					break;
					
				case 175:
					// #line 163
					{ return Tokens.T_INCLUDE_ONCE; }
					break;
					
				case 176:
					// #line 177
					{ return Tokens.T_FUNC_C; }
					break;
					
				case 177:
					// #line 196
					{ return Tokens.T_NAMESPACE_C; }
					break;
					
				case 178:
					// #line 280
					{ BEGIN(LexicalStates.ST_ONE_LINE_COMMENT); return Tokens.T_PRAGMA_FILE; }
					break;
					
				case 179:
					// #line 279
					{ BEGIN(LexicalStates.ST_ONE_LINE_COMMENT); return Tokens.T_PRAGMA_LINE; }
					break;
					
				case 180:
					// #line 281
					{ BEGIN(LexicalStates.ST_ONE_LINE_COMMENT); return Tokens.T_PRAGMA_DEFAULT_LINE; }
					break;
					
				case 181:
					// #line 282
					{ BEGIN(LexicalStates.ST_ONE_LINE_COMMENT); return Tokens.T_PRAGMA_DEFAULT_FILE; }
					break;
					
				case 182:
					// #line 483
					{ return Tokens.T_ENCAPSED_AND_WHITESPACE; }
					break;
					
				case 183:
					// #line 484
					{ return Tokens.T_CHARACTER; }
					break;
					
				case 184:
					// #line 466
					{ inString = true; return Tokens.T_STRING; }
					break;
					
				case 185:
					// #line 476
					{ BEGIN(LexicalStates.ST_IN_SCRIPTING); return Tokens.T_DOUBLE_QUOTES; }
					break;
					
				case 186:
					// #line 465
					{ return Tokens.T_NUM_STRING; }
					break;
					
				case 187:
					// #line 482
					{ inString = true; return (Tokens)GetTokenChar(0); }
					break;
					
				case 188:
					// #line 475
					{ return Tokens.T_ENCAPSED_AND_WHITESPACE; }
					break;
					
				case 189:
					// #line 474
					{ return Tokens.T_BAD_CHARACTER; }
					break;
					
				case 190:
					// #line 472
					{ return Tokens.EscapedCharacter; }
					break;
					
				case 191:
					// #line 470
					{ inString = true; return (inUnicodeString) ? Tokens.UnicodeCharName : Tokens.T_STRING; }
					break;
					
				case 192:
					// #line 469
					{ inString = true; return (inUnicodeString) ? Tokens.UnicodeCharCode : Tokens.T_STRING; }
					break;
					
				case 193:
					// #line 471
					{ return Tokens.EscapedCharacter; }
					break;
					
				case 194:
					// #line 467
					{ return Tokens.OctalCharCode; }
					break;
					
				case 195:
					// #line 473
					{ inString = true; return Tokens.T_STRING; }
					break;
					
				case 196:
					// #line 480
					{ yy_push_state(LexicalStates.ST_LOOKING_FOR_PROPERTY); inString = true; return Tokens.T_OBJECT_OPERATOR; }
					break;
					
				case 197:
					// #line 479
					{ yyless(1); return Tokens.T_CHARACTER; }
					break;
					
				case 198:
					// #line 477
					{ inString = true; return Tokens.T_VARIABLE; }
					break;
					
				case 199:
					// #line 478
					{ yy_push_state(LexicalStates.ST_LOOKING_FOR_VARNAME); return Tokens.T_DOLLAR_OPEN_CURLY_BRACES; }
					break;
					
				case 200:
					// #line 481
					{ yy_push_state(LexicalStates.ST_IN_SCRIPTING); yyless(1); return Tokens.T_CURLY_OPEN; }
					break;
					
				case 201:
					// #line 468
					{ return Tokens.HexCharCode; }
					break;
					
				case 202:
					// #line 425
					{ yymore(); break; }
					break;
					
				case 203:
					// #line 426
					{ BEGIN(LexicalStates.ST_IN_SCRIPTING); return Tokens.SingleQuotedString; }
					break;
					
				case 204:
					// #line 506
					{ return Tokens.T_ENCAPSED_AND_WHITESPACE; }
					break;
					
				case 205:
					// #line 507
					{ return Tokens.T_CHARACTER; }
					break;
					
				case 206:
					// #line 489
					{ inString = true; return Tokens.T_STRING; }
					break;
					
				case 207:
					// #line 498
					{ return Tokens.T_ENCAPSED_AND_WHITESPACE; }
					break;
					
				case 208:
					// #line 488
					{ return Tokens.T_NUM_STRING; }
					break;
					
				case 209:
					// #line 504
					{ inString = true; return (Tokens)GetTokenChar(0); }
					break;
					
				case 210:
					// #line 499
					{ BEGIN(LexicalStates.ST_IN_SCRIPTING); return Tokens.T_BACKQUOTE; }
					break;
					
				case 211:
					// #line 497
					{ return Tokens.T_BAD_CHARACTER; }
					break;
					
				case 212:
					// #line 495
					{ return Tokens.EscapedCharacter; }
					break;
					
				case 213:
					// #line 493
					{ inString = true; return (inUnicodeString) ? Tokens.UnicodeCharName : Tokens.T_STRING; }
					break;
					
				case 214:
					// #line 492
					{ inString = true; return (inUnicodeString) ? Tokens.UnicodeCharCode : Tokens.T_STRING; }
					break;
					
				case 215:
					// #line 490
					{ return Tokens.OctalCharCode; }
					break;
					
				case 216:
					// #line 496
					{ inString = true; return Tokens.T_STRING; }
					break;
					
				case 217:
					// #line 494
					{ return Tokens.EscapedCharacter; }
					break;
					
				case 218:
					// #line 503
					{ yy_push_state(LexicalStates.ST_LOOKING_FOR_PROPERTY); inString = true; return Tokens.T_OBJECT_OPERATOR; }
					break;
					
				case 219:
					// #line 502
					{ yyless(1); return Tokens.T_CHARACTER; }
					break;
					
				case 220:
					// #line 500
					{ inString = true; return Tokens.T_VARIABLE; }
					break;
					
				case 221:
					// #line 501
					{ yy_push_state(LexicalStates.ST_LOOKING_FOR_VARNAME); return Tokens.T_DOLLAR_OPEN_CURLY_BRACES; }
					break;
					
				case 222:
					// #line 505
					{ yy_push_state(LexicalStates.ST_IN_SCRIPTING); yyless(1); return Tokens.T_CURLY_OPEN; }
					break;
					
				case 223:
					// #line 491
					{ return Tokens.HexCharCode; }
					break;
					
				case 224:
					// #line 461
					{ return Tokens.T_ENCAPSED_AND_WHITESPACE; }
					break;
					
				case 225:
					// #line 462
					{ return Tokens.T_CHARACTER; }
					break;
					
				case 226:
					// #line 446
					{ inString = true; return Tokens.T_STRING; }
					break;
					
				case 227:
					// #line 454
					{ return Tokens.T_ENCAPSED_AND_WHITESPACE; }
					break;
					
				case 228:
					// #line 445
					{ return Tokens.T_NUM_STRING; }
					break;
					
				case 229:
					// #line 459
					{ inString = true; return (Tokens)GetTokenChar(0); }
					break;
					
				case 230:
					// #line 453
					{ return Tokens.T_BAD_CHARACTER; }
					break;
					
				case 231:
					// #line 451
					{ return Tokens.EscapedCharacter; }
					break;
					
				case 232:
					// #line 450
					{ inString = true; return (inUnicodeString) ? Tokens.UnicodeCharName : Tokens.T_STRING; }
					break;
					
				case 233:
					// #line 449
					{ inString = true; return (inUnicodeString) ? Tokens.UnicodeCharCode : Tokens.T_STRING; }
					break;
					
				case 234:
					// #line 447
					{ return Tokens.OctalCharCode; }
					break;
					
				case 235:
					// #line 452
					{ inString = true; return Tokens.T_STRING; }
					break;
					
				case 236:
					// #line 458
					{ yy_push_state(LexicalStates.ST_LOOKING_FOR_PROPERTY); inString = true; return Tokens.T_OBJECT_OPERATOR; }
					break;
					
				case 237:
					// #line 457
					{ yyless(1); return Tokens.T_CHARACTER; }
					break;
					
				case 238:
					// #line 455
					{ inString = true; return Tokens.T_VARIABLE; }
					break;
					
				case 239:
					// #line 456
					{ yy_push_state(LexicalStates.ST_LOOKING_FOR_VARNAME); return Tokens.T_DOLLAR_OPEN_CURLY_BRACES; }
					break;
					
				case 240:
					// #line 460
					{ yy_push_state(LexicalStates.ST_IN_SCRIPTING); yyless(1); return Tokens.T_CURLY_OPEN; }
					break;
					
				case 241:
					// #line 448
					{ return Tokens.HexCharCode; }
					break;
					
				case 242:
					// #line 430
					{
						if (IsCurrentHeredocEnd(0))
						{
						  yyless(hereDocLabel.Length);
						  hereDocLabel = null;
						  BEGIN(LexicalStates.ST_IN_SCRIPTING);
							return Tokens.T_END_HEREDOC;
						}
						else 
						{
							inString = true;
							return Tokens.T_STRING;
						}
					}
					break;
					
				case 243:
					// #line 370
					{
						yyless(0);
						if (!yy_pop_state()) return Tokens.ERROR;
						break;
					}
					break;
					
				case 244:
					// #line 363
					{
						if (!yy_pop_state()) return Tokens.ERROR;
						inString = (CurrentLexicalState != LexicalStates.ST_IN_SCRIPTING); 
						isCode = true;
						return Tokens.T_STRING;
					}
					break;
					
				case 245:
					// #line 384
					{
						yyless(0);
						if (!yy_pop_state()) return Tokens.ERROR;
						yy_push_state(LexicalStates.ST_IN_SCRIPTING);
						break;
					}
					break;
					
				case 246:
					// #line 378
					{
						if (!yy_pop_state()) return Tokens.ERROR;
						yy_push_state(LexicalStates.ST_IN_SCRIPTING);
						return Tokens.T_STRING_VARNAME;
					}
					break;
					
				case 247:
					// #line 419
					{ yymore(); break; }
					break;
					
				case 248:
					// #line 421
					{ yymore(); break; }
					break;
					
				case 249:
					// #line 420
					{ BEGIN(LexicalStates.ST_IN_SCRIPTING); return Tokens.T_DOC_COMMENT; }
					break;
					
				case 250:
					// #line 413
					{ yymore(); break; }
					break;
					
				case 251:
					// #line 415
					{ yymore(); break; }
					break;
					
				case 252:
					// #line 414
					{ BEGIN(LexicalStates.ST_IN_SCRIPTING); return Tokens.T_COMMENT; }
					break;
					
				case 253:
					// #line 393
					{ yymore(); break; }
					break;
					
				case 254:
					// #line 394
					{ yymore(); break; }
					break;
					
				case 255:
					// #line 395
					{ BEGIN(LexicalStates.ST_IN_SCRIPTING); return Tokens.T_LINE_COMMENT; }
					break;
					
				case 256:
					// #line 397
					{ 
					  if (AllowAspTags || GetTokenChar(TokenLength - 2) != '%') 
					  { 
							yyless(0);
							BEGIN(LexicalStates.ST_IN_SCRIPTING);
							return Tokens.T_LINE_COMMENT;
						} 
						else 
						{
							yymore();
							break;
						}
					}
					break;
					
				case 259: goto case 2;
				case 260: goto case 4;
				case 261: goto case 6;
				case 262: goto case 7;
				case 263: goto case 9;
				case 264: goto case 13;
				case 265: goto case 19;
				case 266: goto case 22;
				case 267: goto case 24;
				case 268: goto case 86;
				case 269: goto case 179;
				case 270: goto case 182;
				case 271: goto case 183;
				case 272: goto case 186;
				case 273: goto case 187;
				case 274: goto case 189;
				case 275: goto case 191;
				case 276: goto case 192;
				case 277: goto case 194;
				case 278: goto case 201;
				case 279: goto case 204;
				case 280: goto case 205;
				case 281: goto case 208;
				case 282: goto case 209;
				case 283: goto case 211;
				case 284: goto case 213;
				case 285: goto case 214;
				case 286: goto case 215;
				case 287: goto case 223;
				case 288: goto case 224;
				case 289: goto case 225;
				case 290: goto case 227;
				case 291: goto case 228;
				case 292: goto case 229;
				case 293: goto case 230;
				case 294: goto case 232;
				case 295: goto case 233;
				case 296: goto case 234;
				case 297: goto case 241;
				case 298: goto case 242;
				case 299: goto case 253;
				case 300: goto case 255;
				case 302: goto case 2;
				case 303: goto case 7;
				case 304: goto case 9;
				case 305: goto case 19;
				case 306: goto case 24;
				case 307: goto case 186;
				case 308: goto case 187;
				case 309: goto case 208;
				case 310: goto case 209;
				case 311: goto case 228;
				case 312: goto case 229;
				case 314: goto case 7;
				case 315: goto case 9;
				case 317: goto case 7;
				case 318: goto case 9;
				case 320: goto case 7;
				case 321: goto case 9;
				case 323: goto case 7;
				case 324: goto case 9;
				case 326: goto case 7;
				case 327: goto case 9;
				case 329: goto case 7;
				case 330: goto case 9;
				case 332: goto case 7;
				case 333: goto case 9;
				case 335: goto case 7;
				case 336: goto case 9;
				case 338: goto case 7;
				case 339: goto case 9;
				case 341: goto case 7;
				case 342: goto case 9;
				case 344: goto case 7;
				case 345: goto case 9;
				case 347: goto case 7;
				case 348: goto case 9;
				case 350: goto case 7;
				case 351: goto case 9;
				case 353: goto case 7;
				case 354: goto case 9;
				case 356: goto case 7;
				case 357: goto case 9;
				case 359: goto case 9;
				case 361: goto case 9;
				case 363: goto case 9;
				case 365: goto case 9;
				case 367: goto case 9;
				case 369: goto case 9;
				case 371: goto case 9;
				case 373: goto case 9;
				case 375: goto case 9;
				case 377: goto case 9;
				case 379: goto case 9;
				case 381: goto case 9;
				case 383: goto case 9;
				case 385: goto case 9;
				case 387: goto case 9;
				case 389: goto case 9;
				case 391: goto case 9;
				case 393: goto case 9;
				case 395: goto case 9;
				case 397: goto case 9;
				case 399: goto case 9;
				case 401: goto case 9;
				case 403: goto case 9;
				case 405: goto case 9;
				case 407: goto case 9;
				case 409: goto case 9;
				case 411: goto case 9;
				case 413: goto case 9;
				case 415: goto case 9;
				case 417: goto case 9;
				case 419: goto case 9;
				case 421: goto case 9;
				case 423: goto case 9;
				case 425: goto case 9;
				case 427: goto case 9;
				case 429: goto case 9;
				case 431: goto case 9;
				case 433: goto case 9;
				case 435: goto case 9;
				case 437: goto case 9;
				case 439: goto case 9;
				case 441: goto case 9;
				case 443: goto case 9;
				case 445: goto case 9;
				case 447: goto case 9;
				case 449: goto case 9;
				case 451: goto case 9;
				case 453: goto case 9;
				case 455: goto case 9;
				case 457: goto case 9;
				case 459: goto case 9;
				case 461: goto case 9;
				case 463: goto case 9;
				case 465: goto case 9;
				case 467: goto case 9;
				case 469: goto case 9;
				case 471: goto case 9;
				case 473: goto case 9;
				case 475: goto case 9;
				case 477: goto case 9;
				case 479: goto case 9;
				case 481: goto case 9;
				case 483: goto case 9;
				case 485: goto case 9;
				case 487: goto case 9;
				case 489: goto case 9;
				case 491: goto case 9;
				case 493: goto case 9;
				case 495: goto case 9;
				case 497: goto case 9;
				case 499: goto case 9;
				case 501: goto case 9;
				case 503: goto case 9;
				case 505: goto case 9;
				case 507: goto case 9;
				case 509: goto case 9;
				case 511: goto case 9;
				case 513: goto case 9;
				case 515: goto case 9;
				case 517: goto case 9;
				case 519: goto case 9;
				case 586: goto case 9;
				case 587: goto case 192;
				case 588: goto case 194;
				case 589: goto case 214;
				case 590: goto case 215;
				case 591: goto case 233;
				case 592: goto case 234;
				case 614: goto case 9;
				case 616: goto case 9;
				case 617: goto case 9;
				case 618: goto case 9;
				case 619: goto case 9;
				case 620: goto case 9;
				case 621: goto case 9;
				case 622: goto case 9;
				case 623: goto case 9;
				case 624: goto case 9;
				case 625: goto case 9;
				case 626: goto case 9;
				case 627: goto case 9;
				case 628: goto case 9;
				case 629: goto case 9;
				case 630: goto case 9;
				case 631: goto case 9;
				case 632: goto case 9;
				case 633: goto case 9;
				case 634: goto case 9;
				case 635: goto case 9;
				case 636: goto case 9;
				case 637: goto case 9;
				case 638: goto case 9;
				case 639: goto case 9;
				case 640: goto case 9;
				case 641: goto case 9;
				case 642: goto case 9;
				case 643: goto case 9;
				case 644: goto case 9;
				case 645: goto case 9;
				case 646: goto case 9;
				case 647: goto case 9;
				case 648: goto case 9;
				case 649: goto case 9;
				case 650: goto case 9;
				case 651: goto case 9;
				case 652: goto case 9;
				case 653: goto case 9;
				case 654: goto case 9;
				case 655: goto case 9;
				case 656: goto case 9;
				case 657: goto case 9;
				case 658: goto case 9;
				case 659: goto case 9;
				case 660: goto case 9;
				case 661: goto case 9;
				case 662: goto case 9;
				case 663: goto case 9;
				case 664: goto case 9;
				case 665: goto case 9;
				case 666: goto case 9;
				case 667: goto case 9;
				case 668: goto case 9;
				case 669: goto case 9;
				case 670: goto case 9;
				case 671: goto case 9;
				case 672: goto case 9;
				case 673: goto case 9;
				case 674: goto case 9;
				case 675: goto case 9;
				case 676: goto case 9;
				case 677: goto case 9;
				case 678: goto case 9;
				case 679: goto case 9;
				case 680: goto case 9;
				case 681: goto case 9;
				case 682: goto case 9;
				case 683: goto case 9;
				case 684: goto case 9;
				case 685: goto case 9;
				case 686: goto case 9;
				case 687: goto case 9;
				case 688: goto case 9;
				case 689: goto case 9;
				case 690: goto case 9;
				case 691: goto case 9;
				case 692: goto case 9;
				case 693: goto case 9;
				case 694: goto case 9;
				case 695: goto case 9;
				case 696: goto case 9;
				case 697: goto case 9;
				case 698: goto case 9;
				case 699: goto case 9;
				case 700: goto case 9;
				case 701: goto case 9;
				case 702: goto case 9;
				case 703: goto case 9;
				case 704: goto case 9;
				case 705: goto case 9;
				case 706: goto case 9;
				case 707: goto case 9;
				case 708: goto case 9;
				case 709: goto case 9;
				case 710: goto case 9;
				case 711: goto case 9;
				case 712: goto case 9;
				case 713: goto case 9;
				case 714: goto case 9;
				case 715: goto case 9;
				case 716: goto case 9;
				case 717: goto case 9;
				case 718: goto case 9;
				case 719: goto case 9;
				case 720: goto case 9;
				case 721: goto case 9;
				case 722: goto case 9;
				case 723: goto case 9;
				case 724: goto case 9;
				case 725: goto case 9;
				case 726: goto case 9;
				case 727: goto case 9;
				case 728: goto case 9;
				case 729: goto case 9;
				case 730: goto case 9;
				case 731: goto case 9;
				case 732: goto case 9;
				case 733: goto case 9;
				case 734: goto case 9;
				case 735: goto case 9;
				case 736: goto case 9;
				case 737: goto case 9;
				case 738: goto case 9;
				case 739: goto case 9;
				case 740: goto case 9;
				case 741: goto case 9;
				case 742: goto case 9;
				case 743: goto case 9;
				case 744: goto case 9;
				case 745: goto case 9;
				case 746: goto case 9;
				case 747: goto case 9;
				case 748: goto case 9;
				case 749: goto case 9;
				case 750: goto case 9;
				case 751: goto case 9;
				case 752: goto case 9;
				case 753: goto case 9;
				case 754: goto case 9;
				case 755: goto case 9;
				case 756: goto case 9;
				case 757: goto case 9;
				case 758: goto case 9;
				case 759: goto case 9;
				case 760: goto case 9;
				case 761: goto case 9;
				case 762: goto case 9;
				case 763: goto case 9;
				case 764: goto case 9;
				case 765: goto case 9;
				case 766: goto case 9;
				case 767: goto case 9;
				case 768: goto case 9;
				case 769: goto case 9;
				case 770: goto case 9;
				case 771: goto case 9;
				case 772: goto case 9;
				case 773: goto case 9;
				case 774: goto case 9;
				case 775: goto case 9;
				case 776: goto case 9;
				case 777: goto case 9;
				case 778: goto case 192;
				case 779: goto case 214;
				case 780: goto case 233;
				case 783: goto case 9;
				case 784: goto case 9;
				case 785: goto case 9;
				case 786: goto case 9;
				case 787: goto case 9;
				case 788: goto case 9;
				case 789: goto case 9;
				case 790: goto case 9;
				case 791: goto case 9;
				case 792: goto case 9;
				case 793: goto case 9;
				case 794: goto case 9;
				case 795: goto case 9;
				case 796: goto case 9;
				case 797: goto case 9;
				case 798: goto case 9;
				case 799: goto case 9;
				case 800: goto case 9;
				case 801: goto case 9;
				case 802: goto case 9;
				case 803: goto case 9;
				case 804: goto case 9;
				case 805: goto case 9;
				case 806: goto case 9;
				case 807: goto case 9;
				case 808: goto case 9;
				case 809: goto case 9;
				case 810: goto case 9;
				case 811: goto case 9;
				case 812: goto case 9;
				case 813: goto case 9;
				case 814: goto case 9;
				case 815: goto case 9;
				case 816: goto case 9;
				case 817: goto case 9;
				case 818: goto case 9;
				case 819: goto case 9;
				case 820: goto case 9;
				case 821: goto case 9;
				case 822: goto case 9;
				case 823: goto case 9;
				case 824: goto case 9;
				case 825: goto case 9;
				case 826: goto case 9;
				case 827: goto case 9;
				case 828: goto case 9;
				case 829: goto case 9;
				case 830: goto case 9;
				case 831: goto case 9;
				case 832: goto case 9;
				case 833: goto case 9;
				case 834: goto case 9;
				case 835: goto case 9;
				case 836: goto case 9;
				case 837: goto case 9;
				case 838: goto case 9;
				case 839: goto case 9;
				case 840: goto case 9;
				case 841: goto case 9;
				case 842: goto case 9;
				case 843: goto case 9;
				case 844: goto case 9;
				case 845: goto case 9;
				case 846: goto case 9;
				case 847: goto case 9;
				case 848: goto case 9;
				case 849: goto case 9;
				case 850: goto case 9;
				case 851: goto case 9;
				case 852: goto case 9;
				case 853: goto case 9;
				case 854: goto case 9;
				case 855: goto case 9;
				case 856: goto case 9;
				case 857: goto case 9;
				case 858: goto case 9;
				case 859: goto case 9;
				case 860: goto case 9;
				case 861: goto case 9;
				case 862: goto case 9;
				case 863: goto case 9;
				case 864: goto case 9;
				case 865: goto case 9;
				case 866: goto case 9;
				case 867: goto case 9;
				case 868: goto case 9;
				case 869: goto case 9;
				case 870: goto case 9;
				case 871: goto case 9;
				case 872: goto case 9;
				case 873: goto case 9;
				case 874: goto case 9;
				case 875: goto case 9;
				case 876: goto case 192;
				case 877: goto case 214;
				case 878: goto case 233;
				case 880: goto case 9;
				case 881: goto case 9;
				case 882: goto case 9;
				case 883: goto case 9;
				case 884: goto case 9;
				case 885: goto case 9;
				case 886: goto case 192;
				case 887: goto case 214;
				case 888: goto case 233;
				case 889: goto case 192;
				case 890: goto case 214;
				case 891: goto case 233;
			}
			accepted = false;
			return yyreturn;
		}
		
		#pragma warning restore 162
		
		
		#endregion
		private void BEGIN(LexicalStates state)
		{
			current_lexical_state = state;
		}
		
		private char Advance()
		{
			if (lookahead_index >= chars_read)
			{
				if (token_start > 0)
				{
					// shift buffer left:
					int length = chars_read - token_start;
					System.Buffer.BlockCopy(buffer, token_start << 1, buffer, 0, length << 1);
					token_end -= token_start;
					token_chunk_start -= token_start;
					token_start = 0;
					chars_read = lookahead_index = length;
					
					// populate the remaining bytes:
					int count = reader.Read(buffer, chars_read, buffer.Length - chars_read);
					if (count <= 0) return EOF;
					
					chars_read += count;
				}
				
				while (lookahead_index >= chars_read)
				{
					if (lookahead_index >= buffer.Length)
						buffer = ResizeBuffer(buffer);
					
					int count = reader.Read(buffer, chars_read, buffer.Length - chars_read);
					if (count <= 0) return EOF;
					chars_read += count;
				}
			}
			
			return Map(buffer[lookahead_index++]);
		}
		
		private char[] ResizeBuffer(char[] buf)
		{
			char[] result = new char[buf.Length << 1];
			System.Buffer.BlockCopy(buf, 0, result, 0, buf.Length << 1);
			return result;
		}
		
		private void AdvanceEndPosition(int from, int to)
		{
			int last_eoln = from - token_end_pos.Column;
			
			for (int i = from; i < to; i++)
			{
				if (buffer[i] == '\n')
				{
					token_end_pos.Line++;
					last_eoln = i;
				}
			}
			token_end_pos.Char += to - from;
			token_end_pos.Column = to - last_eoln;
		}
		
		private void TrimTokenEnd()
		{
			if (token_end > token_chunk_start && buffer[token_end - 1] == '\n')
				token_end--;
			if (token_end > token_chunk_start && buffer[token_end - 1] == '\r')
				token_end--;
			}
		
		private void MarkTokenChunkStart()
		{
			token_chunk_start = lookahead_index;
		}
		
		private void MarkTokenEnd()
		{
			token_end = lookahead_index;
		}
		
		private void MoveToTokenEnd()
		{
			lookahead_index = token_end;
			yy_at_bol = (token_end > token_chunk_start) && (buffer[token_end - 1] == '\r' || buffer[token_end - 1] == '\n');
		}
		
		public int TokenLength
		{
			get { return token_end - token_start; }
		}
		
		public int TokenChunkLength
		{
			get { return token_end - token_chunk_start; }
		}
		
		private void yymore()
		{
			if (!expanding_token)
			{
				token_start = token_chunk_start;
				expanding_token = true;
			}
		}
		
		private void yyless(int count)
		{
			lookahead_index = token_end = token_chunk_start + count;
		}
		
		private Stack<LexicalStates> stateStack = new Stack<LexicalStates>(20);
		
		private void yy_push_state(LexicalStates state)
		{
			stateStack.Push(current_lexical_state);
			current_lexical_state = state;
		}
		
		private bool yy_pop_state()
		{
			if (stateStack.Count == 0) return false;
			current_lexical_state = stateStack.Pop();
			return true;
		}
		
		private LexicalStates yy_top_state()
		{
			return stateStack.Peek();
		}
		
		#region Tables
		
		private static AcceptConditions[] acceptCondition = new AcceptConditions[]
		{
			AcceptConditions.NotAccept, // 0
			AcceptConditions.Accept, // 1
			AcceptConditions.Accept, // 2
			AcceptConditions.Accept, // 3
			AcceptConditions.Accept, // 4
			AcceptConditions.Accept, // 5
			AcceptConditions.Accept, // 6
			AcceptConditions.Accept, // 7
			AcceptConditions.Accept, // 8
			AcceptConditions.Accept, // 9
			AcceptConditions.Accept, // 10
			AcceptConditions.Accept, // 11
			AcceptConditions.Accept, // 12
			AcceptConditions.Accept, // 13
			AcceptConditions.Accept, // 14
			AcceptConditions.Accept, // 15
			AcceptConditions.Accept, // 16
			AcceptConditions.Accept, // 17
			AcceptConditions.Accept, // 18
			AcceptConditions.Accept, // 19
			AcceptConditions.Accept, // 20
			AcceptConditions.Accept, // 21
			AcceptConditions.Accept, // 22
			AcceptConditions.Accept, // 23
			AcceptConditions.Accept, // 24
			AcceptConditions.Accept, // 25
			AcceptConditions.Accept, // 26
			AcceptConditions.Accept, // 27
			AcceptConditions.Accept, // 28
			AcceptConditions.Accept, // 29
			AcceptConditions.Accept, // 30
			AcceptConditions.Accept, // 31
			AcceptConditions.Accept, // 32
			AcceptConditions.Accept, // 33
			AcceptConditions.Accept, // 34
			AcceptConditions.Accept, // 35
			AcceptConditions.Accept, // 36
			AcceptConditions.Accept, // 37
			AcceptConditions.Accept, // 38
			AcceptConditions.Accept, // 39
			AcceptConditions.Accept, // 40
			AcceptConditions.Accept, // 41
			AcceptConditions.Accept, // 42
			AcceptConditions.Accept, // 43
			AcceptConditions.Accept, // 44
			AcceptConditions.Accept, // 45
			AcceptConditions.Accept, // 46
			AcceptConditions.Accept, // 47
			AcceptConditions.Accept, // 48
			AcceptConditions.Accept, // 49
			AcceptConditions.Accept, // 50
			AcceptConditions.Accept, // 51
			AcceptConditions.Accept, // 52
			AcceptConditions.Accept, // 53
			AcceptConditions.Accept, // 54
			AcceptConditions.Accept, // 55
			AcceptConditions.Accept, // 56
			AcceptConditions.Accept, // 57
			AcceptConditions.Accept, // 58
			AcceptConditions.Accept, // 59
			AcceptConditions.Accept, // 60
			AcceptConditions.Accept, // 61
			AcceptConditions.Accept, // 62
			AcceptConditions.Accept, // 63
			AcceptConditions.Accept, // 64
			AcceptConditions.Accept, // 65
			AcceptConditions.Accept, // 66
			AcceptConditions.Accept, // 67
			AcceptConditions.Accept, // 68
			AcceptConditions.Accept, // 69
			AcceptConditions.Accept, // 70
			AcceptConditions.Accept, // 71
			AcceptConditions.Accept, // 72
			AcceptConditions.Accept, // 73
			AcceptConditions.Accept, // 74
			AcceptConditions.Accept, // 75
			AcceptConditions.Accept, // 76
			AcceptConditions.Accept, // 77
			AcceptConditions.Accept, // 78
			AcceptConditions.Accept, // 79
			AcceptConditions.Accept, // 80
			AcceptConditions.Accept, // 81
			AcceptConditions.Accept, // 82
			AcceptConditions.Accept, // 83
			AcceptConditions.Accept, // 84
			AcceptConditions.Accept, // 85
			AcceptConditions.Accept, // 86
			AcceptConditions.Accept, // 87
			AcceptConditions.Accept, // 88
			AcceptConditions.Accept, // 89
			AcceptConditions.Accept, // 90
			AcceptConditions.Accept, // 91
			AcceptConditions.Accept, // 92
			AcceptConditions.Accept, // 93
			AcceptConditions.Accept, // 94
			AcceptConditions.Accept, // 95
			AcceptConditions.Accept, // 96
			AcceptConditions.Accept, // 97
			AcceptConditions.Accept, // 98
			AcceptConditions.Accept, // 99
			AcceptConditions.Accept, // 100
			AcceptConditions.Accept, // 101
			AcceptConditions.Accept, // 102
			AcceptConditions.Accept, // 103
			AcceptConditions.Accept, // 104
			AcceptConditions.Accept, // 105
			AcceptConditions.Accept, // 106
			AcceptConditions.Accept, // 107
			AcceptConditions.Accept, // 108
			AcceptConditions.Accept, // 109
			AcceptConditions.Accept, // 110
			AcceptConditions.Accept, // 111
			AcceptConditions.Accept, // 112
			AcceptConditions.Accept, // 113
			AcceptConditions.Accept, // 114
			AcceptConditions.Accept, // 115
			AcceptConditions.Accept, // 116
			AcceptConditions.Accept, // 117
			AcceptConditions.Accept, // 118
			AcceptConditions.Accept, // 119
			AcceptConditions.Accept, // 120
			AcceptConditions.Accept, // 121
			AcceptConditions.Accept, // 122
			AcceptConditions.Accept, // 123
			AcceptConditions.Accept, // 124
			AcceptConditions.Accept, // 125
			AcceptConditions.Accept, // 126
			AcceptConditions.Accept, // 127
			AcceptConditions.Accept, // 128
			AcceptConditions.Accept, // 129
			AcceptConditions.Accept, // 130
			AcceptConditions.Accept, // 131
			AcceptConditions.Accept, // 132
			AcceptConditions.Accept, // 133
			AcceptConditions.Accept, // 134
			AcceptConditions.Accept, // 135
			AcceptConditions.Accept, // 136
			AcceptConditions.Accept, // 137
			AcceptConditions.Accept, // 138
			AcceptConditions.Accept, // 139
			AcceptConditions.Accept, // 140
			AcceptConditions.Accept, // 141
			AcceptConditions.Accept, // 142
			AcceptConditions.Accept, // 143
			AcceptConditions.Accept, // 144
			AcceptConditions.Accept, // 145
			AcceptConditions.Accept, // 146
			AcceptConditions.Accept, // 147
			AcceptConditions.Accept, // 148
			AcceptConditions.Accept, // 149
			AcceptConditions.Accept, // 150
			AcceptConditions.Accept, // 151
			AcceptConditions.Accept, // 152
			AcceptConditions.Accept, // 153
			AcceptConditions.Accept, // 154
			AcceptConditions.Accept, // 155
			AcceptConditions.Accept, // 156
			AcceptConditions.Accept, // 157
			AcceptConditions.Accept, // 158
			AcceptConditions.Accept, // 159
			AcceptConditions.Accept, // 160
			AcceptConditions.Accept, // 161
			AcceptConditions.Accept, // 162
			AcceptConditions.Accept, // 163
			AcceptConditions.Accept, // 164
			AcceptConditions.Accept, // 165
			AcceptConditions.Accept, // 166
			AcceptConditions.Accept, // 167
			AcceptConditions.Accept, // 168
			AcceptConditions.Accept, // 169
			AcceptConditions.Accept, // 170
			AcceptConditions.Accept, // 171
			AcceptConditions.Accept, // 172
			AcceptConditions.Accept, // 173
			AcceptConditions.Accept, // 174
			AcceptConditions.Accept, // 175
			AcceptConditions.Accept, // 176
			AcceptConditions.Accept, // 177
			AcceptConditions.Accept, // 178
			AcceptConditions.Accept, // 179
			AcceptConditions.Accept, // 180
			AcceptConditions.Accept, // 181
			AcceptConditions.Accept, // 182
			AcceptConditions.Accept, // 183
			AcceptConditions.Accept, // 184
			AcceptConditions.Accept, // 185
			AcceptConditions.Accept, // 186
			AcceptConditions.Accept, // 187
			AcceptConditions.Accept, // 188
			AcceptConditions.Accept, // 189
			AcceptConditions.Accept, // 190
			AcceptConditions.Accept, // 191
			AcceptConditions.Accept, // 192
			AcceptConditions.Accept, // 193
			AcceptConditions.Accept, // 194
			AcceptConditions.Accept, // 195
			AcceptConditions.Accept, // 196
			AcceptConditions.Accept, // 197
			AcceptConditions.Accept, // 198
			AcceptConditions.Accept, // 199
			AcceptConditions.Accept, // 200
			AcceptConditions.Accept, // 201
			AcceptConditions.Accept, // 202
			AcceptConditions.Accept, // 203
			AcceptConditions.Accept, // 204
			AcceptConditions.Accept, // 205
			AcceptConditions.Accept, // 206
			AcceptConditions.Accept, // 207
			AcceptConditions.Accept, // 208
			AcceptConditions.Accept, // 209
			AcceptConditions.Accept, // 210
			AcceptConditions.Accept, // 211
			AcceptConditions.Accept, // 212
			AcceptConditions.Accept, // 213
			AcceptConditions.Accept, // 214
			AcceptConditions.Accept, // 215
			AcceptConditions.Accept, // 216
			AcceptConditions.Accept, // 217
			AcceptConditions.Accept, // 218
			AcceptConditions.Accept, // 219
			AcceptConditions.Accept, // 220
			AcceptConditions.Accept, // 221
			AcceptConditions.Accept, // 222
			AcceptConditions.Accept, // 223
			AcceptConditions.Accept, // 224
			AcceptConditions.Accept, // 225
			AcceptConditions.Accept, // 226
			AcceptConditions.Accept, // 227
			AcceptConditions.Accept, // 228
			AcceptConditions.Accept, // 229
			AcceptConditions.Accept, // 230
			AcceptConditions.Accept, // 231
			AcceptConditions.Accept, // 232
			AcceptConditions.Accept, // 233
			AcceptConditions.Accept, // 234
			AcceptConditions.Accept, // 235
			AcceptConditions.Accept, // 236
			AcceptConditions.Accept, // 237
			AcceptConditions.Accept, // 238
			AcceptConditions.Accept, // 239
			AcceptConditions.Accept, // 240
			AcceptConditions.Accept, // 241
			AcceptConditions.AcceptOnStart, // 242
			AcceptConditions.Accept, // 243
			AcceptConditions.Accept, // 244
			AcceptConditions.Accept, // 245
			AcceptConditions.Accept, // 246
			AcceptConditions.Accept, // 247
			AcceptConditions.Accept, // 248
			AcceptConditions.Accept, // 249
			AcceptConditions.Accept, // 250
			AcceptConditions.Accept, // 251
			AcceptConditions.Accept, // 252
			AcceptConditions.Accept, // 253
			AcceptConditions.Accept, // 254
			AcceptConditions.Accept, // 255
			AcceptConditions.Accept, // 256
			AcceptConditions.NotAccept, // 257
			AcceptConditions.Accept, // 258
			AcceptConditions.Accept, // 259
			AcceptConditions.Accept, // 260
			AcceptConditions.Accept, // 261
			AcceptConditions.Accept, // 262
			AcceptConditions.Accept, // 263
			AcceptConditions.Accept, // 264
			AcceptConditions.Accept, // 265
			AcceptConditions.Accept, // 266
			AcceptConditions.Accept, // 267
			AcceptConditions.Accept, // 268
			AcceptConditions.Accept, // 269
			AcceptConditions.Accept, // 270
			AcceptConditions.Accept, // 271
			AcceptConditions.Accept, // 272
			AcceptConditions.Accept, // 273
			AcceptConditions.Accept, // 274
			AcceptConditions.Accept, // 275
			AcceptConditions.Accept, // 276
			AcceptConditions.Accept, // 277
			AcceptConditions.Accept, // 278
			AcceptConditions.Accept, // 279
			AcceptConditions.Accept, // 280
			AcceptConditions.Accept, // 281
			AcceptConditions.Accept, // 282
			AcceptConditions.Accept, // 283
			AcceptConditions.Accept, // 284
			AcceptConditions.Accept, // 285
			AcceptConditions.Accept, // 286
			AcceptConditions.Accept, // 287
			AcceptConditions.Accept, // 288
			AcceptConditions.Accept, // 289
			AcceptConditions.Accept, // 290
			AcceptConditions.Accept, // 291
			AcceptConditions.Accept, // 292
			AcceptConditions.Accept, // 293
			AcceptConditions.Accept, // 294
			AcceptConditions.Accept, // 295
			AcceptConditions.Accept, // 296
			AcceptConditions.Accept, // 297
			AcceptConditions.AcceptOnStart, // 298
			AcceptConditions.Accept, // 299
			AcceptConditions.Accept, // 300
			AcceptConditions.NotAccept, // 301
			AcceptConditions.Accept, // 302
			AcceptConditions.Accept, // 303
			AcceptConditions.Accept, // 304
			AcceptConditions.Accept, // 305
			AcceptConditions.Accept, // 306
			AcceptConditions.Accept, // 307
			AcceptConditions.Accept, // 308
			AcceptConditions.Accept, // 309
			AcceptConditions.Accept, // 310
			AcceptConditions.Accept, // 311
			AcceptConditions.Accept, // 312
			AcceptConditions.NotAccept, // 313
			AcceptConditions.Accept, // 314
			AcceptConditions.Accept, // 315
			AcceptConditions.NotAccept, // 316
			AcceptConditions.Accept, // 317
			AcceptConditions.Accept, // 318
			AcceptConditions.NotAccept, // 319
			AcceptConditions.Accept, // 320
			AcceptConditions.Accept, // 321
			AcceptConditions.NotAccept, // 322
			AcceptConditions.Accept, // 323
			AcceptConditions.Accept, // 324
			AcceptConditions.NotAccept, // 325
			AcceptConditions.Accept, // 326
			AcceptConditions.Accept, // 327
			AcceptConditions.NotAccept, // 328
			AcceptConditions.Accept, // 329
			AcceptConditions.Accept, // 330
			AcceptConditions.NotAccept, // 331
			AcceptConditions.Accept, // 332
			AcceptConditions.Accept, // 333
			AcceptConditions.NotAccept, // 334
			AcceptConditions.Accept, // 335
			AcceptConditions.Accept, // 336
			AcceptConditions.NotAccept, // 337
			AcceptConditions.Accept, // 338
			AcceptConditions.Accept, // 339
			AcceptConditions.NotAccept, // 340
			AcceptConditions.Accept, // 341
			AcceptConditions.Accept, // 342
			AcceptConditions.NotAccept, // 343
			AcceptConditions.Accept, // 344
			AcceptConditions.Accept, // 345
			AcceptConditions.NotAccept, // 346
			AcceptConditions.Accept, // 347
			AcceptConditions.Accept, // 348
			AcceptConditions.NotAccept, // 349
			AcceptConditions.Accept, // 350
			AcceptConditions.Accept, // 351
			AcceptConditions.NotAccept, // 352
			AcceptConditions.Accept, // 353
			AcceptConditions.Accept, // 354
			AcceptConditions.NotAccept, // 355
			AcceptConditions.Accept, // 356
			AcceptConditions.Accept, // 357
			AcceptConditions.NotAccept, // 358
			AcceptConditions.Accept, // 359
			AcceptConditions.NotAccept, // 360
			AcceptConditions.Accept, // 361
			AcceptConditions.NotAccept, // 362
			AcceptConditions.Accept, // 363
			AcceptConditions.NotAccept, // 364
			AcceptConditions.Accept, // 365
			AcceptConditions.NotAccept, // 366
			AcceptConditions.Accept, // 367
			AcceptConditions.NotAccept, // 368
			AcceptConditions.Accept, // 369
			AcceptConditions.NotAccept, // 370
			AcceptConditions.Accept, // 371
			AcceptConditions.NotAccept, // 372
			AcceptConditions.Accept, // 373
			AcceptConditions.NotAccept, // 374
			AcceptConditions.Accept, // 375
			AcceptConditions.NotAccept, // 376
			AcceptConditions.Accept, // 377
			AcceptConditions.NotAccept, // 378
			AcceptConditions.Accept, // 379
			AcceptConditions.NotAccept, // 380
			AcceptConditions.Accept, // 381
			AcceptConditions.NotAccept, // 382
			AcceptConditions.Accept, // 383
			AcceptConditions.NotAccept, // 384
			AcceptConditions.Accept, // 385
			AcceptConditions.NotAccept, // 386
			AcceptConditions.Accept, // 387
			AcceptConditions.NotAccept, // 388
			AcceptConditions.Accept, // 389
			AcceptConditions.NotAccept, // 390
			AcceptConditions.Accept, // 391
			AcceptConditions.NotAccept, // 392
			AcceptConditions.Accept, // 393
			AcceptConditions.NotAccept, // 394
			AcceptConditions.Accept, // 395
			AcceptConditions.NotAccept, // 396
			AcceptConditions.Accept, // 397
			AcceptConditions.NotAccept, // 398
			AcceptConditions.Accept, // 399
			AcceptConditions.NotAccept, // 400
			AcceptConditions.Accept, // 401
			AcceptConditions.NotAccept, // 402
			AcceptConditions.Accept, // 403
			AcceptConditions.NotAccept, // 404
			AcceptConditions.Accept, // 405
			AcceptConditions.NotAccept, // 406
			AcceptConditions.Accept, // 407
			AcceptConditions.NotAccept, // 408
			AcceptConditions.Accept, // 409
			AcceptConditions.NotAccept, // 410
			AcceptConditions.Accept, // 411
			AcceptConditions.NotAccept, // 412
			AcceptConditions.Accept, // 413
			AcceptConditions.NotAccept, // 414
			AcceptConditions.Accept, // 415
			AcceptConditions.NotAccept, // 416
			AcceptConditions.Accept, // 417
			AcceptConditions.NotAccept, // 418
			AcceptConditions.Accept, // 419
			AcceptConditions.NotAccept, // 420
			AcceptConditions.Accept, // 421
			AcceptConditions.NotAccept, // 422
			AcceptConditions.Accept, // 423
			AcceptConditions.NotAccept, // 424
			AcceptConditions.Accept, // 425
			AcceptConditions.NotAccept, // 426
			AcceptConditions.Accept, // 427
			AcceptConditions.NotAccept, // 428
			AcceptConditions.Accept, // 429
			AcceptConditions.NotAccept, // 430
			AcceptConditions.Accept, // 431
			AcceptConditions.NotAccept, // 432
			AcceptConditions.Accept, // 433
			AcceptConditions.NotAccept, // 434
			AcceptConditions.Accept, // 435
			AcceptConditions.NotAccept, // 436
			AcceptConditions.Accept, // 437
			AcceptConditions.NotAccept, // 438
			AcceptConditions.Accept, // 439
			AcceptConditions.NotAccept, // 440
			AcceptConditions.Accept, // 441
			AcceptConditions.NotAccept, // 442
			AcceptConditions.Accept, // 443
			AcceptConditions.NotAccept, // 444
			AcceptConditions.Accept, // 445
			AcceptConditions.NotAccept, // 446
			AcceptConditions.Accept, // 447
			AcceptConditions.NotAccept, // 448
			AcceptConditions.Accept, // 449
			AcceptConditions.NotAccept, // 450
			AcceptConditions.Accept, // 451
			AcceptConditions.NotAccept, // 452
			AcceptConditions.Accept, // 453
			AcceptConditions.NotAccept, // 454
			AcceptConditions.Accept, // 455
			AcceptConditions.NotAccept, // 456
			AcceptConditions.Accept, // 457
			AcceptConditions.NotAccept, // 458
			AcceptConditions.Accept, // 459
			AcceptConditions.NotAccept, // 460
			AcceptConditions.Accept, // 461
			AcceptConditions.NotAccept, // 462
			AcceptConditions.Accept, // 463
			AcceptConditions.NotAccept, // 464
			AcceptConditions.Accept, // 465
			AcceptConditions.NotAccept, // 466
			AcceptConditions.Accept, // 467
			AcceptConditions.NotAccept, // 468
			AcceptConditions.Accept, // 469
			AcceptConditions.NotAccept, // 470
			AcceptConditions.Accept, // 471
			AcceptConditions.NotAccept, // 472
			AcceptConditions.Accept, // 473
			AcceptConditions.NotAccept, // 474
			AcceptConditions.Accept, // 475
			AcceptConditions.NotAccept, // 476
			AcceptConditions.Accept, // 477
			AcceptConditions.NotAccept, // 478
			AcceptConditions.Accept, // 479
			AcceptConditions.NotAccept, // 480
			AcceptConditions.Accept, // 481
			AcceptConditions.NotAccept, // 482
			AcceptConditions.Accept, // 483
			AcceptConditions.NotAccept, // 484
			AcceptConditions.Accept, // 485
			AcceptConditions.NotAccept, // 486
			AcceptConditions.Accept, // 487
			AcceptConditions.NotAccept, // 488
			AcceptConditions.Accept, // 489
			AcceptConditions.NotAccept, // 490
			AcceptConditions.Accept, // 491
			AcceptConditions.NotAccept, // 492
			AcceptConditions.Accept, // 493
			AcceptConditions.NotAccept, // 494
			AcceptConditions.Accept, // 495
			AcceptConditions.NotAccept, // 496
			AcceptConditions.Accept, // 497
			AcceptConditions.NotAccept, // 498
			AcceptConditions.Accept, // 499
			AcceptConditions.NotAccept, // 500
			AcceptConditions.Accept, // 501
			AcceptConditions.NotAccept, // 502
			AcceptConditions.Accept, // 503
			AcceptConditions.NotAccept, // 504
			AcceptConditions.Accept, // 505
			AcceptConditions.NotAccept, // 506
			AcceptConditions.Accept, // 507
			AcceptConditions.NotAccept, // 508
			AcceptConditions.Accept, // 509
			AcceptConditions.NotAccept, // 510
			AcceptConditions.Accept, // 511
			AcceptConditions.NotAccept, // 512
			AcceptConditions.Accept, // 513
			AcceptConditions.NotAccept, // 514
			AcceptConditions.Accept, // 515
			AcceptConditions.NotAccept, // 516
			AcceptConditions.Accept, // 517
			AcceptConditions.NotAccept, // 518
			AcceptConditions.Accept, // 519
			AcceptConditions.NotAccept, // 520
			AcceptConditions.NotAccept, // 521
			AcceptConditions.NotAccept, // 522
			AcceptConditions.NotAccept, // 523
			AcceptConditions.NotAccept, // 524
			AcceptConditions.NotAccept, // 525
			AcceptConditions.NotAccept, // 526
			AcceptConditions.NotAccept, // 527
			AcceptConditions.NotAccept, // 528
			AcceptConditions.NotAccept, // 529
			AcceptConditions.NotAccept, // 530
			AcceptConditions.NotAccept, // 531
			AcceptConditions.NotAccept, // 532
			AcceptConditions.NotAccept, // 533
			AcceptConditions.NotAccept, // 534
			AcceptConditions.NotAccept, // 535
			AcceptConditions.NotAccept, // 536
			AcceptConditions.NotAccept, // 537
			AcceptConditions.NotAccept, // 538
			AcceptConditions.NotAccept, // 539
			AcceptConditions.NotAccept, // 540
			AcceptConditions.NotAccept, // 541
			AcceptConditions.NotAccept, // 542
			AcceptConditions.NotAccept, // 543
			AcceptConditions.NotAccept, // 544
			AcceptConditions.NotAccept, // 545
			AcceptConditions.NotAccept, // 546
			AcceptConditions.NotAccept, // 547
			AcceptConditions.NotAccept, // 548
			AcceptConditions.NotAccept, // 549
			AcceptConditions.NotAccept, // 550
			AcceptConditions.NotAccept, // 551
			AcceptConditions.NotAccept, // 552
			AcceptConditions.NotAccept, // 553
			AcceptConditions.NotAccept, // 554
			AcceptConditions.NotAccept, // 555
			AcceptConditions.NotAccept, // 556
			AcceptConditions.NotAccept, // 557
			AcceptConditions.NotAccept, // 558
			AcceptConditions.NotAccept, // 559
			AcceptConditions.NotAccept, // 560
			AcceptConditions.NotAccept, // 561
			AcceptConditions.NotAccept, // 562
			AcceptConditions.NotAccept, // 563
			AcceptConditions.NotAccept, // 564
			AcceptConditions.NotAccept, // 565
			AcceptConditions.NotAccept, // 566
			AcceptConditions.NotAccept, // 567
			AcceptConditions.NotAccept, // 568
			AcceptConditions.NotAccept, // 569
			AcceptConditions.NotAccept, // 570
			AcceptConditions.NotAccept, // 571
			AcceptConditions.NotAccept, // 572
			AcceptConditions.NotAccept, // 573
			AcceptConditions.NotAccept, // 574
			AcceptConditions.NotAccept, // 575
			AcceptConditions.NotAccept, // 576
			AcceptConditions.NotAccept, // 577
			AcceptConditions.NotAccept, // 578
			AcceptConditions.NotAccept, // 579
			AcceptConditions.NotAccept, // 580
			AcceptConditions.NotAccept, // 581
			AcceptConditions.NotAccept, // 582
			AcceptConditions.NotAccept, // 583
			AcceptConditions.NotAccept, // 584
			AcceptConditions.NotAccept, // 585
			AcceptConditions.Accept, // 586
			AcceptConditions.Accept, // 587
			AcceptConditions.Accept, // 588
			AcceptConditions.Accept, // 589
			AcceptConditions.Accept, // 590
			AcceptConditions.Accept, // 591
			AcceptConditions.Accept, // 592
			AcceptConditions.NotAccept, // 593
			AcceptConditions.NotAccept, // 594
			AcceptConditions.NotAccept, // 595
			AcceptConditions.NotAccept, // 596
			AcceptConditions.NotAccept, // 597
			AcceptConditions.NotAccept, // 598
			AcceptConditions.NotAccept, // 599
			AcceptConditions.NotAccept, // 600
			AcceptConditions.NotAccept, // 601
			AcceptConditions.NotAccept, // 602
			AcceptConditions.NotAccept, // 603
			AcceptConditions.NotAccept, // 604
			AcceptConditions.NotAccept, // 605
			AcceptConditions.NotAccept, // 606
			AcceptConditions.NotAccept, // 607
			AcceptConditions.NotAccept, // 608
			AcceptConditions.NotAccept, // 609
			AcceptConditions.NotAccept, // 610
			AcceptConditions.NotAccept, // 611
			AcceptConditions.NotAccept, // 612
			AcceptConditions.NotAccept, // 613
			AcceptConditions.Accept, // 614
			AcceptConditions.NotAccept, // 615
			AcceptConditions.Accept, // 616
			AcceptConditions.Accept, // 617
			AcceptConditions.Accept, // 618
			AcceptConditions.Accept, // 619
			AcceptConditions.Accept, // 620
			AcceptConditions.Accept, // 621
			AcceptConditions.Accept, // 622
			AcceptConditions.Accept, // 623
			AcceptConditions.Accept, // 624
			AcceptConditions.Accept, // 625
			AcceptConditions.Accept, // 626
			AcceptConditions.Accept, // 627
			AcceptConditions.Accept, // 628
			AcceptConditions.Accept, // 629
			AcceptConditions.Accept, // 630
			AcceptConditions.Accept, // 631
			AcceptConditions.Accept, // 632
			AcceptConditions.Accept, // 633
			AcceptConditions.Accept, // 634
			AcceptConditions.Accept, // 635
			AcceptConditions.Accept, // 636
			AcceptConditions.Accept, // 637
			AcceptConditions.Accept, // 638
			AcceptConditions.Accept, // 639
			AcceptConditions.Accept, // 640
			AcceptConditions.Accept, // 641
			AcceptConditions.Accept, // 642
			AcceptConditions.Accept, // 643
			AcceptConditions.Accept, // 644
			AcceptConditions.Accept, // 645
			AcceptConditions.Accept, // 646
			AcceptConditions.Accept, // 647
			AcceptConditions.Accept, // 648
			AcceptConditions.Accept, // 649
			AcceptConditions.Accept, // 650
			AcceptConditions.Accept, // 651
			AcceptConditions.Accept, // 652
			AcceptConditions.Accept, // 653
			AcceptConditions.Accept, // 654
			AcceptConditions.Accept, // 655
			AcceptConditions.Accept, // 656
			AcceptConditions.Accept, // 657
			AcceptConditions.Accept, // 658
			AcceptConditions.Accept, // 659
			AcceptConditions.Accept, // 660
			AcceptConditions.Accept, // 661
			AcceptConditions.Accept, // 662
			AcceptConditions.Accept, // 663
			AcceptConditions.Accept, // 664
			AcceptConditions.Accept, // 665
			AcceptConditions.Accept, // 666
			AcceptConditions.Accept, // 667
			AcceptConditions.Accept, // 668
			AcceptConditions.Accept, // 669
			AcceptConditions.Accept, // 670
			AcceptConditions.Accept, // 671
			AcceptConditions.Accept, // 672
			AcceptConditions.Accept, // 673
			AcceptConditions.Accept, // 674
			AcceptConditions.Accept, // 675
			AcceptConditions.Accept, // 676
			AcceptConditions.Accept, // 677
			AcceptConditions.Accept, // 678
			AcceptConditions.Accept, // 679
			AcceptConditions.Accept, // 680
			AcceptConditions.Accept, // 681
			AcceptConditions.Accept, // 682
			AcceptConditions.Accept, // 683
			AcceptConditions.Accept, // 684
			AcceptConditions.Accept, // 685
			AcceptConditions.Accept, // 686
			AcceptConditions.Accept, // 687
			AcceptConditions.Accept, // 688
			AcceptConditions.Accept, // 689
			AcceptConditions.Accept, // 690
			AcceptConditions.Accept, // 691
			AcceptConditions.Accept, // 692
			AcceptConditions.Accept, // 693
			AcceptConditions.Accept, // 694
			AcceptConditions.Accept, // 695
			AcceptConditions.Accept, // 696
			AcceptConditions.Accept, // 697
			AcceptConditions.Accept, // 698
			AcceptConditions.Accept, // 699
			AcceptConditions.Accept, // 700
			AcceptConditions.Accept, // 701
			AcceptConditions.Accept, // 702
			AcceptConditions.Accept, // 703
			AcceptConditions.Accept, // 704
			AcceptConditions.Accept, // 705
			AcceptConditions.Accept, // 706
			AcceptConditions.Accept, // 707
			AcceptConditions.Accept, // 708
			AcceptConditions.Accept, // 709
			AcceptConditions.Accept, // 710
			AcceptConditions.Accept, // 711
			AcceptConditions.Accept, // 712
			AcceptConditions.Accept, // 713
			AcceptConditions.Accept, // 714
			AcceptConditions.Accept, // 715
			AcceptConditions.Accept, // 716
			AcceptConditions.Accept, // 717
			AcceptConditions.Accept, // 718
			AcceptConditions.Accept, // 719
			AcceptConditions.Accept, // 720
			AcceptConditions.Accept, // 721
			AcceptConditions.Accept, // 722
			AcceptConditions.Accept, // 723
			AcceptConditions.Accept, // 724
			AcceptConditions.Accept, // 725
			AcceptConditions.Accept, // 726
			AcceptConditions.Accept, // 727
			AcceptConditions.Accept, // 728
			AcceptConditions.Accept, // 729
			AcceptConditions.Accept, // 730
			AcceptConditions.Accept, // 731
			AcceptConditions.Accept, // 732
			AcceptConditions.Accept, // 733
			AcceptConditions.Accept, // 734
			AcceptConditions.Accept, // 735
			AcceptConditions.Accept, // 736
			AcceptConditions.Accept, // 737
			AcceptConditions.Accept, // 738
			AcceptConditions.Accept, // 739
			AcceptConditions.Accept, // 740
			AcceptConditions.Accept, // 741
			AcceptConditions.Accept, // 742
			AcceptConditions.Accept, // 743
			AcceptConditions.Accept, // 744
			AcceptConditions.Accept, // 745
			AcceptConditions.Accept, // 746
			AcceptConditions.Accept, // 747
			AcceptConditions.Accept, // 748
			AcceptConditions.Accept, // 749
			AcceptConditions.Accept, // 750
			AcceptConditions.Accept, // 751
			AcceptConditions.Accept, // 752
			AcceptConditions.Accept, // 753
			AcceptConditions.Accept, // 754
			AcceptConditions.Accept, // 755
			AcceptConditions.Accept, // 756
			AcceptConditions.Accept, // 757
			AcceptConditions.Accept, // 758
			AcceptConditions.Accept, // 759
			AcceptConditions.Accept, // 760
			AcceptConditions.Accept, // 761
			AcceptConditions.Accept, // 762
			AcceptConditions.Accept, // 763
			AcceptConditions.Accept, // 764
			AcceptConditions.Accept, // 765
			AcceptConditions.Accept, // 766
			AcceptConditions.Accept, // 767
			AcceptConditions.Accept, // 768
			AcceptConditions.Accept, // 769
			AcceptConditions.Accept, // 770
			AcceptConditions.Accept, // 771
			AcceptConditions.Accept, // 772
			AcceptConditions.Accept, // 773
			AcceptConditions.Accept, // 774
			AcceptConditions.Accept, // 775
			AcceptConditions.Accept, // 776
			AcceptConditions.Accept, // 777
			AcceptConditions.Accept, // 778
			AcceptConditions.Accept, // 779
			AcceptConditions.Accept, // 780
			AcceptConditions.NotAccept, // 781
			AcceptConditions.NotAccept, // 782
			AcceptConditions.Accept, // 783
			AcceptConditions.Accept, // 784
			AcceptConditions.Accept, // 785
			AcceptConditions.Accept, // 786
			AcceptConditions.Accept, // 787
			AcceptConditions.Accept, // 788
			AcceptConditions.Accept, // 789
			AcceptConditions.Accept, // 790
			AcceptConditions.Accept, // 791
			AcceptConditions.Accept, // 792
			AcceptConditions.Accept, // 793
			AcceptConditions.Accept, // 794
			AcceptConditions.Accept, // 795
			AcceptConditions.Accept, // 796
			AcceptConditions.Accept, // 797
			AcceptConditions.Accept, // 798
			AcceptConditions.Accept, // 799
			AcceptConditions.Accept, // 800
			AcceptConditions.Accept, // 801
			AcceptConditions.Accept, // 802
			AcceptConditions.Accept, // 803
			AcceptConditions.Accept, // 804
			AcceptConditions.Accept, // 805
			AcceptConditions.Accept, // 806
			AcceptConditions.Accept, // 807
			AcceptConditions.Accept, // 808
			AcceptConditions.Accept, // 809
			AcceptConditions.Accept, // 810
			AcceptConditions.Accept, // 811
			AcceptConditions.Accept, // 812
			AcceptConditions.Accept, // 813
			AcceptConditions.Accept, // 814
			AcceptConditions.Accept, // 815
			AcceptConditions.Accept, // 816
			AcceptConditions.Accept, // 817
			AcceptConditions.Accept, // 818
			AcceptConditions.Accept, // 819
			AcceptConditions.Accept, // 820
			AcceptConditions.Accept, // 821
			AcceptConditions.Accept, // 822
			AcceptConditions.Accept, // 823
			AcceptConditions.Accept, // 824
			AcceptConditions.Accept, // 825
			AcceptConditions.Accept, // 826
			AcceptConditions.Accept, // 827
			AcceptConditions.Accept, // 828
			AcceptConditions.Accept, // 829
			AcceptConditions.Accept, // 830
			AcceptConditions.Accept, // 831
			AcceptConditions.Accept, // 832
			AcceptConditions.Accept, // 833
			AcceptConditions.Accept, // 834
			AcceptConditions.Accept, // 835
			AcceptConditions.Accept, // 836
			AcceptConditions.Accept, // 837
			AcceptConditions.Accept, // 838
			AcceptConditions.Accept, // 839
			AcceptConditions.Accept, // 840
			AcceptConditions.Accept, // 841
			AcceptConditions.Accept, // 842
			AcceptConditions.Accept, // 843
			AcceptConditions.Accept, // 844
			AcceptConditions.Accept, // 845
			AcceptConditions.Accept, // 846
			AcceptConditions.Accept, // 847
			AcceptConditions.Accept, // 848
			AcceptConditions.Accept, // 849
			AcceptConditions.Accept, // 850
			AcceptConditions.Accept, // 851
			AcceptConditions.Accept, // 852
			AcceptConditions.Accept, // 853
			AcceptConditions.Accept, // 854
			AcceptConditions.Accept, // 855
			AcceptConditions.Accept, // 856
			AcceptConditions.Accept, // 857
			AcceptConditions.Accept, // 858
			AcceptConditions.Accept, // 859
			AcceptConditions.Accept, // 860
			AcceptConditions.Accept, // 861
			AcceptConditions.Accept, // 862
			AcceptConditions.Accept, // 863
			AcceptConditions.Accept, // 864
			AcceptConditions.Accept, // 865
			AcceptConditions.Accept, // 866
			AcceptConditions.Accept, // 867
			AcceptConditions.Accept, // 868
			AcceptConditions.Accept, // 869
			AcceptConditions.Accept, // 870
			AcceptConditions.Accept, // 871
			AcceptConditions.Accept, // 872
			AcceptConditions.Accept, // 873
			AcceptConditions.Accept, // 874
			AcceptConditions.Accept, // 875
			AcceptConditions.Accept, // 876
			AcceptConditions.Accept, // 877
			AcceptConditions.Accept, // 878
			AcceptConditions.NotAccept, // 879
			AcceptConditions.Accept, // 880
			AcceptConditions.Accept, // 881
			AcceptConditions.Accept, // 882
			AcceptConditions.Accept, // 883
			AcceptConditions.Accept, // 884
			AcceptConditions.Accept, // 885
			AcceptConditions.Accept, // 886
			AcceptConditions.Accept, // 887
			AcceptConditions.Accept, // 888
			AcceptConditions.Accept, // 889
			AcceptConditions.Accept, // 890
			AcceptConditions.Accept, // 891
		};
		
		private static int[] colMap = new int[]
		{
			64, 64, 64, 64, 64, 64, 64, 64, 64, 23, 11, 64, 64, 24, 64, 64, 
			64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 
			69, 44, 20, 55, 50, 1, 47, 21, 56, 58, 45, 42, 53, 43, 46, 25, 
			54, 59, 61, 60, 39, 68, 38, 68, 57, 52, 41, 66, 2, 18, 22, 5, 
			53, 13, 31, 6, 26, 17, 28, 15, 19, 8, 40, 32, 12, 36, 14, 29, 
			9, 35, 7, 4, 10, 16, 33, 30, 27, 37, 51, 70, 3, 70, 49, 34, 
			65, 13, 31, 6, 26, 17, 28, 15, 19, 8, 40, 32, 12, 36, 14, 29, 
			9, 35, 7, 4, 10, 16, 33, 30, 27, 37, 51, 62, 48, 63, 53, 64, 
			67, 0
		};
		
		private static int[] rowMap = new int[]
		{
			0, 1, 2, 3, 4, 1, 1, 5, 1, 6, 7, 8, 9, 10, 11, 1, 
			1, 1, 1, 12, 13, 1, 1, 1, 14, 15, 16, 17, 18, 1, 1, 1, 
			1, 19, 1, 1, 20, 21, 22, 16, 23, 1, 1, 1, 1, 1, 1, 1, 
			1, 1, 1, 1, 1, 1, 1, 24, 1, 25, 1, 16, 16, 16, 16, 1, 
			1, 16, 16, 26, 16, 27, 1, 28, 16, 16, 1, 16, 16, 16, 16, 16, 
			29, 16, 16, 30, 16, 16, 1, 31, 16, 16, 16, 16, 16, 16, 16, 16, 
			16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 1, 16, 16, 16, 
			16, 16, 16, 16, 16, 16, 16, 16, 32, 16, 16, 16, 1, 1, 1, 1, 
			33, 34, 16, 16, 16, 16, 16, 16, 16, 16, 1, 1, 1, 1, 1, 1, 
			16, 16, 16, 16, 16, 16, 16, 16, 1, 1, 1, 1, 1, 16, 16, 16, 
			16, 16, 16, 16, 1, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 
			16, 16, 35, 36, 37, 38, 39, 40, 41, 1, 42, 43, 44, 1, 1, 45, 
			46, 1, 47, 1, 39, 1, 48, 1, 1, 49, 50, 1, 51, 52, 53, 54, 
			55, 56, 1, 1, 1, 57, 58, 59, 1, 1, 51, 1, 60, 1, 1, 61, 
			62, 63, 64, 65, 66, 67, 1, 1, 68, 69, 70, 1, 62, 1, 71, 1, 
			1, 72, 1, 1, 73, 1, 74, 75, 76, 1, 77, 78, 1, 79, 80, 1, 
			1, 81, 82, 83, 1, 84, 85, 86, 87, 1, 88, 1, 89, 90, 91, 1, 
			92, 93, 94, 1, 1, 1, 1, 95, 1, 96, 97, 98, 1, 1, 1, 1, 
			99, 1, 100, 101, 102, 103, 1, 1, 1, 1, 104, 1, 105, 106, 107, 108, 
			109, 110, 111, 112, 1, 113, 1, 114, 1, 115, 116, 117, 118, 119, 120, 121, 
			122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 
			138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 
			154, 1, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 
			169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 8, 183, 
			184, 185, 9, 186, 187, 188, 189, 190, 191, 192, 28, 193, 194, 195, 196, 197, 
			198, 199, 200, 201, 202, 203, 157, 204, 205, 206, 207, 208, 209, 210, 211, 212, 
			213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 30, 223, 224, 225, 27, 226, 
			227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 
			243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 
			259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 272, 273, 274, 
			275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 
			291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 
			307, 308, 309, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 
			323, 324, 325, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 
			339, 340, 341, 342, 343, 344, 345, 346, 347, 35, 348, 349, 350, 351, 352, 353, 
			354, 355, 356, 357, 358, 359, 112, 360, 361, 362, 363, 364, 113, 365, 366, 367, 
			114, 368, 369, 370, 371, 372, 373, 374, 375, 376, 377, 378, 379, 380, 381, 382, 
			383, 384, 385, 386, 387, 219, 388, 389, 390, 391, 392, 393, 394, 395, 396, 397, 
			398, 399, 400, 401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, 
			414, 415, 416, 417, 418, 419, 420, 421, 422, 423, 424, 425, 426, 427, 428, 429, 
			430, 431, 432, 433, 434, 435, 436, 437, 438, 439, 440, 441, 442, 443, 444, 445, 
			446, 447, 448, 449, 450, 451, 452, 453, 454, 455, 456, 457, 458, 459, 460, 461, 
			462, 463, 464, 465, 466, 467, 468, 469, 470, 471, 472, 473, 474, 475, 476, 477, 
			478, 479, 480, 481, 482, 483, 484, 485, 486, 487, 488, 489, 490, 491, 492, 493, 
			494, 495, 496, 497, 498, 499, 500, 501, 502, 503, 504, 505, 506, 507, 508, 509, 
			510, 511, 512, 513, 514, 515, 516, 517, 518, 519, 520, 521, 522, 523, 524, 525, 
			526, 527, 528, 529, 530, 531, 532, 533, 534, 535, 536, 537, 538, 539, 540, 541, 
			542, 543, 544, 545, 546, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556, 557, 
			558, 559, 560, 561, 562, 563, 564, 565, 566, 567, 568, 569, 570, 571, 572, 573, 
			574, 575, 576, 577, 578, 579, 580, 581, 582, 583, 584, 585, 586, 587, 588, 589, 
			590, 591, 592, 593, 594, 595, 596, 597, 598, 599, 600, 601, 602, 603, 604, 605, 
			606, 607, 608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 618, 619, 620, 621, 
			622, 623, 624, 625, 626, 627, 628, 629, 630, 631, 632, 633, 634, 635, 636, 637, 
			638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 648, 649, 650, 651, 652, 653, 
			654, 655, 542, 656, 657, 658, 659, 660, 16, 661, 662, 663, 664, 665, 666, 667, 
			668, 669, 670, 671, 672, 673, 674, 675, 676, 677, 678, 679
		};
		
		private static int[,] nextState = new int[,]
		{
			{ 1, 2, 259, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 2, 257, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, -1, 2, 2, 2 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 313, -1, -1, -1, -1, -1, -1, -1, -1, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 18, -1, -1, -1, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 875, -1, 872, 872, 872, 872, 872, 620, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 621, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10, -1 },
			{ -1, 382, 382, 384, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 30, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, -1, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, -1, 382, 382, 382 },
			{ -1, 386, 386, 388, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 31, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, -1, 386, 386, 386 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 392, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 13, 13, -1, -1, -1, -1, -1, -1, 40, -1, -1, -1, -1, -1, 13, -1, 13, -1, -1, 13, -1, 13, 13, 13, -1, -1, -1, -1, -1, -1, 13, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 396, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 265, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 305, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, 416, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 56, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 267, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 306, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 845, -1, 789, 872, 872, 872, 57, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 790, -1, 824, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 63, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 64, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 426, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 793, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 864, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 392, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, 40, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, 40, -1, -1, 40, -1, 40, 40, 40, -1, -1, -1, -1, -1, -1, 40, -1, -1 },
			{ -1, -1, -1, -1, 55, -1, 55, 55, 55, 55, 55, -1, 55, 55, 55, 55, 55, 55, -1, 55, -1, -1, -1, -1, -1, -1, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 55, 55, -1, 55, -1, -1, 55, -1, 55, 55, 55, -1, -1, -1, -1, -1, -1, 55, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 688, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 385, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 701, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 69, 69, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 69, -1, 69, -1, -1, 69, -1, 69, 69, 69, -1, -1, -1, -1, -1, -1, 69, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 71, -1, -1, -1, -1, -1, -1, 71, -1, -1, -1, 71, -1, -1, -1, -1, -1, -1, -1, -1, 71, -1, 71, -1, -1, 71, -1, -1, -1, -1, -1, -1, 71, 71, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 71, -1, 71, -1, -1, 71, -1, 71, 71, 71, -1, -1, -1, -1, -1, -1, 71, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 435, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 83, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 83, 83, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 83, -1 },
			{ -1, -1, -1, -1, 87, -1, 87, 87, 87, 87, 87, -1, 87, 87, 87, 87, 87, 87, -1, 87, -1, -1, -1, -1, -1, -1, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 87, 87, -1, 87, -1, -1, 87, -1, 87, 87, 87, -1, -1, -1, -1, -1, -1, 87, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 753, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 759, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 882, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, -1, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, 178, -1, 178, 178, 178 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 269, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, 179, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, -1, 179, -1, -1, 179, -1, 179, 179, 179, -1, -1, -1, -1, -1, -1, 179, 269, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 180, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 180, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1 },
			{ -1, 182, 182, -1, -1, 182, -1, -1, -1, -1, -1, 182, -1, -1, -1, -1, -1, -1, 182, -1, -1, 182, 182, 182, 182, 182, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 182, 182, 182, 182, 182, 182, 182, 182, 182, -1, -1, -1, 182, -1, 182, 182, -1, 182, -1, -1, -1, -1, -1, -1, -1, 182, -1, -1, 182, -1 },
			{ -1, 189, 189, 190, 189, 189, 191, 190, 189, 189, 190, 189, 189, 189, 190, 189, 192, 189, 189, 189, 193, 189, 189, 189, 189, 189, 189, 274, 189, 189, 189, 189, 189, 189, 189, 189, 189, 189, 194, 194, 189, 189, 189, 189, 189, 189, 189, 189, 189, 189, 190, 189, 189, 189, 194, 189, 189, 189, 189, 194, 194, 194, 195, 189, 189, 189, 189, -1, 194, 189, 189 },
			{ -1, -1, -1, -1, 184, -1, 184, 184, 184, 184, 184, -1, 184, 184, 184, 184, 184, 184, -1, 184, -1, -1, -1, -1, -1, -1, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 184, 184, -1, 184, -1, -1, 184, -1, 184, 184, 184, -1, -1, -1, -1, -1, -1, 184, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 186, 186, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 186, -1, 186, -1, -1, 186, -1, 186, 186, 186, -1, -1, -1, -1, -1, -1, 186, -1, -1 },
			{ -1, 197, 197, 197, 198, 197, 198, 198, 198, 198, 198, 197, 198, 198, 198, 198, 198, 198, 197, 198, 197, 197, 197, 197, 197, 197, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 197, 197, 198, 197, 197, 197, 197, 197, 197, 197, 197, 197, 197, 198, 197, 197, 197, 197, 197, 197, 197, 197, 197, 197, 199, 197, 197, 197, 197, -1, 197, 197, 197 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 188, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 567, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 889, -1, -1, -1, -1, -1, -1, 889, -1, -1, -1, 889, -1, -1, -1, -1, -1, -1, -1, -1, 889, -1, 889, -1, -1, 889, -1, -1, -1, -1, -1, -1, 889, 889, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 889, -1, 889, -1, -1, 889, -1, 889, 889, 889, -1, -1, -1, -1, -1, -1, 889, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 588, 588, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 588, -1, -1, -1, -1, 588, 588, 588, -1, -1, -1, -1, -1, -1, 588, -1, -1 },
			{ -1, -1, -1, -1, 198, -1, 198, 198, 198, 198, 198, -1, 198, 198, 198, 198, 198, 198, -1, 198, -1, -1, -1, -1, -1, -1, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, 198, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 198, 198, -1, 198, -1, -1, 198, -1, 198, 198, 198, -1, -1, -1, -1, -1, -1, 198, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 278, -1, -1, -1, -1, -1, -1, 278, -1, -1, -1, 278, -1, -1, -1, -1, -1, -1, -1, -1, 278, -1, 278, -1, -1, 278, -1, -1, -1, -1, -1, -1, 278, 278, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 278, -1, 278, -1, -1, 278, -1, 278, 278, 278, -1, -1, -1, -1, -1, -1, 278, -1, -1 },
			{ -1, 202, 202, 570, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, -1, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, -1, 202, 202, 202 },
			{ -1, 204, 204, -1, -1, 204, -1, -1, -1, -1, -1, 204, -1, -1, -1, -1, -1, -1, 204, -1, -1, 204, 204, 204, 204, 204, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 204, 204, 204, 204, 204, 204, 204, 204, 204, -1, -1, -1, 204, -1, 204, 204, -1, 204, -1, -1, -1, -1, -1, -1, -1, 204, -1, -1, 204, -1 },
			{ -1, 211, 211, 212, 211, 211, 213, 212, 211, 211, 212, 211, 211, 211, 212, 211, 214, 211, 211, 211, 211, 211, 211, 211, 211, 211, 211, 283, 211, 211, 211, 211, 211, 211, 211, 211, 211, 211, 215, 215, 211, 211, 211, 211, 211, 211, 211, 211, 211, 211, 212, 211, 211, 211, 215, 211, 211, 211, 211, 215, 215, 215, 216, 211, 211, 217, 211, -1, 215, 211, 211 },
			{ -1, -1, -1, -1, 206, -1, 206, 206, 206, 206, 206, -1, 206, 206, 206, 206, 206, 206, -1, 206, -1, -1, -1, -1, -1, -1, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 206, 206, -1, 206, -1, -1, 206, -1, 206, 206, 206, -1, -1, -1, -1, -1, -1, 206, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 207, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 208, 208, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 208, -1, 208, -1, -1, 208, -1, 208, 208, 208, -1, -1, -1, -1, -1, -1, 208, -1, -1 },
			{ -1, 219, 219, 219, 220, 219, 220, 220, 220, 220, 220, 219, 220, 220, 220, 220, 220, 220, 219, 220, 219, 219, 219, 219, 219, 219, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 219, 219, 220, 219, 219, 219, 219, 219, 219, 219, 219, 219, 219, 220, 219, 219, 219, 219, 219, 219, 219, 219, 219, 219, 221, 219, 219, 219, 219, -1, 219, 219, 219 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 573, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 890, -1, -1, -1, -1, -1, -1, 890, -1, -1, -1, 890, -1, -1, -1, -1, -1, -1, -1, -1, 890, -1, 890, -1, -1, 890, -1, -1, -1, -1, -1, -1, 890, 890, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 890, -1, 890, -1, -1, 890, -1, 890, 890, 890, -1, -1, -1, -1, -1, -1, 890, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 590, 590, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 590, -1, -1, -1, -1, 590, 590, 590, -1, -1, -1, -1, -1, -1, 590, -1, -1 },
			{ -1, -1, -1, -1, 220, -1, 220, 220, 220, 220, 220, -1, 220, 220, 220, 220, 220, 220, -1, 220, -1, -1, -1, -1, -1, -1, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, 220, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 220, 220, -1, 220, -1, -1, 220, -1, 220, 220, 220, -1, -1, -1, -1, -1, -1, 220, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 287, -1, -1, -1, -1, -1, -1, 287, -1, -1, -1, 287, -1, -1, -1, -1, -1, -1, -1, -1, 287, -1, 287, -1, -1, 287, -1, -1, -1, -1, -1, -1, 287, 287, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 287, -1, 287, -1, -1, 287, -1, 287, 287, 287, -1, -1, -1, -1, -1, -1, 287, -1, -1 },
			{ -1, 224, 224, -1, -1, 224, -1, -1, -1, -1, -1, 224, -1, -1, -1, -1, -1, -1, 224, -1, -1, 224, 224, 224, 224, 224, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 224, 224, 224, 224, 224, 224, 224, 224, 224, -1, -1, -1, 224, -1, 224, 224, -1, 224, -1, -1, -1, -1, -1, -1, -1, 224, -1, -1, 224, -1 },
			{ -1, 230, 230, 231, 230, 230, 232, 231, 230, 230, 231, 230, 230, 230, 231, 230, 233, 230, 230, 230, 230, 230, 230, 230, 230, 230, 230, 293, 230, 230, 230, 230, 230, 230, 230, 230, 230, 230, 234, 234, 230, 230, 230, 230, 230, 230, 230, 230, 230, 230, 231, 230, 230, 230, 234, 230, 230, 230, 230, 234, 234, 234, 235, 230, 230, 230, 230, -1, 234, 230, 230 },
			{ -1, -1, -1, -1, 226, -1, 226, 226, 226, 226, 226, -1, 226, 226, 226, 226, 226, 226, -1, 226, -1, -1, -1, -1, -1, -1, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 226, 226, -1, 226, -1, -1, 226, -1, 226, 226, 226, -1, -1, -1, -1, -1, -1, 226, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 227, 227, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 227, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 228, 228, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 228, -1, 228, -1, -1, 228, -1, 228, 228, 228, -1, -1, -1, -1, -1, -1, 228, -1, -1 },
			{ -1, 237, 237, 237, 238, 237, 238, 238, 238, 238, 238, 237, 238, 238, 238, 238, 238, 238, 237, 238, 237, 237, 237, 237, 237, 237, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 237, 237, 238, 237, 237, 237, 237, 237, 237, 237, 237, 237, 237, 238, 237, 237, 237, 237, 237, 237, 237, 237, 237, 237, 239, 237, 237, 237, 237, -1, 237, 237, 237 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 578, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 891, -1, -1, -1, -1, -1, -1, 891, -1, -1, -1, 891, -1, -1, -1, -1, -1, -1, -1, -1, 891, -1, 891, -1, -1, 891, -1, -1, -1, -1, -1, -1, 891, 891, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 891, -1, 891, -1, -1, 891, -1, 891, 891, 891, -1, -1, -1, -1, -1, -1, 891, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 592, 592, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 592, -1, -1, -1, -1, 592, 592, 592, -1, -1, -1, -1, -1, -1, 592, -1, -1 },
			{ -1, -1, -1, -1, 238, -1, 238, 238, 238, 238, 238, -1, 238, 238, 238, 238, 238, 238, -1, 238, -1, -1, -1, -1, -1, -1, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, 238, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 238, 238, -1, 238, -1, -1, 238, -1, 238, 238, 238, -1, -1, -1, -1, -1, -1, 238, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 297, -1, -1, -1, -1, -1, -1, 297, -1, -1, -1, 297, -1, -1, -1, -1, -1, -1, -1, -1, 297, -1, 297, -1, -1, 297, -1, -1, -1, -1, -1, -1, 297, 297, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 297, -1, 297, -1, -1, 297, -1, 297, 297, 297, -1, -1, -1, -1, -1, -1, 297, -1, -1 },
			{ -1, -1, -1, -1, 244, -1, 244, 244, 244, 244, 244, -1, 244, 244, 244, 244, 244, 244, -1, 244, -1, -1, -1, -1, -1, -1, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 244, 244, -1, 244, -1, -1, 244, -1, 244, 244, 244, -1, -1, -1, -1, -1, -1, 244, -1, -1 },
			{ -1, -1, -1, -1, 246, -1, 246, 246, 246, 246, 246, -1, 246, 246, 246, 246, 246, 246, -1, 246, -1, -1, -1, -1, -1, -1, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 246, 246, -1, 246, -1, -1, 246, -1, 246, 246, 246, -1, -1, -1, -1, -1, -1, 246, -1, -1 },
			{ -1, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, -1, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, -1, 247, 247, 247 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 249, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, -1, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, -1, 250, 250, 250 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 252, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 256, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, 254, 254, 254, -1, 254, 254, 254, 254, 254, -1, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, -1, 254, -1, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, -1, 254, 254, 254 },
			{ -1, -1, -1, 2, -1, -1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, -1, 2, 2, 2 },
			{ -1, -1, -1, -1, 577, -1, 577, 577, 577, 577, 577, -1, 577, 577, 577, 577, 577, 577, -1, 577, -1, -1, -1, -1, -1, -1, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, -1, -1, 577, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 577, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 3, -1, 2, 302, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, -1, 2, 2, 2 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, 20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 21, -1, -1, -1, 22, -1, -1, 376, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 626, -1, 872, 872, 872, 872, 872, -1, 872, 872, 25, 872, 872, 872, -1, 872, -1, 380, -1, -1, -1, -1, 872, 872, 26, 872, 872, 872, 872, 872, 872, 872, 627, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 392, -1, -1, -1, -1, -1, -1, -1, -1, -1, 394, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 13, 13, -1, -1, -1, -1, -1, -1, 40, -1, -1, -1, -1, -1, 13, -1, 13, -1, -1, 13, -1, 13, 13, 13, -1, -1, -1, -1, -1, -1, 13, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 70, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 86, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 269, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 269, -1 },
			{ -1, 182, 182, -1, -1, 182, -1, -1, -1, -1, -1, 182, -1, -1, -1, -1, -1, -1, 182, -1, -1, 182, 196, 182, 182, 182, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 182, 182, 182, 182, 182, 182, 182, 182, 182, -1, -1, -1, 182, -1, 182, 182, -1, 182, -1, -1, -1, -1, -1, -1, -1, 182, -1, -1, 182, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 566, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 186, 186, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 186, -1, 186, -1, -1, 186, -1, 186, 186, 186, -1, -1, -1, -1, -1, -1, 186, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 200, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 201, -1, -1, -1, -1, -1, -1, 201, -1, -1, -1, 201, -1, -1, -1, -1, -1, -1, -1, -1, 201, -1, 201, -1, -1, 201, -1, -1, -1, -1, -1, -1, 201, 201, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 201, -1, 201, -1, -1, 201, -1, 201, 201, 201, -1, -1, -1, -1, -1, -1, 201, -1, -1 },
			{ -1, 204, 204, -1, -1, 204, -1, -1, -1, -1, -1, 204, -1, -1, -1, -1, -1, -1, 204, -1, -1, 204, 218, 204, 204, 204, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 204, 204, 204, 204, 204, 204, 204, 204, 204, -1, -1, -1, 204, -1, 204, 204, -1, 204, -1, -1, -1, -1, -1, -1, -1, 204, -1, -1, 204, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 572, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 208, 208, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 208, -1, 208, -1, -1, 208, -1, 208, 208, 208, -1, -1, -1, -1, -1, -1, 208, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 222, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 223, -1, -1, -1, -1, -1, -1, 223, -1, -1, -1, 223, -1, -1, -1, -1, -1, -1, -1, -1, 223, -1, 223, -1, -1, 223, -1, -1, -1, -1, -1, -1, 223, 223, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 223, -1, 223, -1, -1, 223, -1, 223, 223, 223, -1, -1, -1, -1, -1, -1, 223, -1, -1 },
			{ -1, 224, 224, -1, -1, 224, -1, -1, -1, -1, -1, 224, -1, -1, -1, -1, -1, -1, 224, -1, -1, 224, 236, 224, 224, 224, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 224, 224, 224, 224, 224, 224, 224, 224, 224, -1, -1, -1, 224, -1, 224, 224, -1, 224, -1, -1, -1, -1, -1, -1, -1, 224, -1, -1, 224, -1 },
			{ -1, 224, 224, -1, -1, 224, -1, -1, -1, -1, -1, 224, -1, -1, -1, -1, -1, -1, 224, -1, 227, 290, 224, 224, 224, 224, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 224, 224, 224, 224, 224, 224, 224, 224, 224, -1, -1, -1, 224, -1, 224, 224, -1, 224, -1, -1, -1, -1, -1, -1, 227, 224, -1, -1, 224, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 576, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 228, 228, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 228, -1, 228, -1, -1, 228, -1, 228, 228, 228, -1, -1, -1, -1, -1, -1, 228, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 240, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 241, -1, -1, -1, -1, -1, -1, 241, -1, -1, -1, 241, -1, -1, -1, -1, -1, -1, -1, -1, 241, -1, 241, -1, -1, 241, -1, -1, -1, -1, -1, -1, 241, 241, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 241, -1, 241, -1, -1, 241, -1, 241, 241, 241, -1, -1, -1, -1, -1, -1, 241, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 242, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 255, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 316, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 301, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 27, -1, 872, 821, 872, 872, 872, -1, 872, 872, 327, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 785, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 265, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 267, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 307, -1, -1, -1, -1, -1, -1, 307, -1, -1, -1, 307, -1, -1, -1, -1, -1, -1, -1, -1, 307, -1, 307, -1, -1, 307, -1, -1, -1, -1, -1, -1, 307, 307, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 307, -1, 307, -1, -1, 307, -1, 307, 307, 307, -1, -1, -1, -1, -1, -1, 307, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 309, -1, -1, -1, -1, -1, -1, 309, -1, -1, -1, 309, -1, -1, -1, -1, -1, -1, -1, -1, 309, -1, 309, -1, -1, 309, -1, -1, -1, -1, -1, -1, 309, 309, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 309, -1, 309, -1, -1, 309, -1, 309, 309, 309, -1, -1, -1, -1, -1, -1, 309, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 311, -1, -1, -1, -1, -1, -1, 311, -1, -1, -1, 311, -1, -1, -1, -1, -1, -1, -1, -1, 311, -1, 311, -1, -1, 311, -1, -1, -1, -1, -1, -1, 311, 311, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 311, -1, 311, -1, -1, 311, -1, 311, 311, 311, -1, -1, -1, -1, -1, -1, 311, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 319, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 28, -1, -1, -1, 29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 336, 872, 872, -1, 872, 872, 872, 872, 872, 641, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 37, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 593, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 32, -1, -1, -1, 33, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 38, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 644, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 322, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 34, -1, -1, -1, -1, -1, -1, 35, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 36, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, 390, -1, 872, -1, 872, 646, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, 11, 12, -1, -1, -1, -1, 872, 872, 872, 647, 872, 872, 872, 872, 872, 872, 872, 39, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6, 261, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 41, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 42, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 354, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 59, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 328, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 43, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 44, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 60, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 331, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 331, 331, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 331, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 45, -1, -1, -1, 46, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 47, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 61, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 331, 334, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 331, 331, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 331, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 266, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 62, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 337, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 65, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 340, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 49, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, 40, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, 40, -1, -1, 40, -1, 40, 40, 40, -1, -1, -1, -1, -1, -1, 40, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 66, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 343, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 50, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 51, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 67, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 346, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 52, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 53, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 68, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 349, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 54, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 683, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 72, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 352, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 55, -1, 55, 55, 55, 55, 55, -1, 55, 55, 55, 55, 55, 55, -1, 55, -1, -1, -1, -1, -1, -1, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, -1, -1, 55, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 55, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 73, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 355, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 75, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 355, -1, -1, -1, -1, -1, -1, 358, -1, -1, -1, -1, 355, 355, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 355, -1 },
			{ -1, -1, -1, -1, 398, -1, -1, 400, 402, -1, -1, -1, -1, 598, -1, -1, 404, -1, -1, -1, -1, -1, -1, 406, -1, -1, 408, -1, 410, 412, -1, 414, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 406, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 76, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 360, -1, 358, -1, -1, -1, -1, -1, -1, -1, -1, 362, 879, -1, 358, 358, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 358, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 77, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 594, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 78, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 364, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 79, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 368, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 80, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 366, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 260, 366, 366, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 366, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 81, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 370, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 82, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 366, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 84, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 366, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 85, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ 1, 7, 262, 8, 9, 303, 777, 818, 263, 842, 586, 10, 855, 304, 614, 863, 616, 869, 314, 872, 11, 12, 317, 10, 10, 320, 315, 617, 618, 318, 873, 321, 872, 619, 874, 872, 872, 872, 13, 13, 872, 323, 326, 329, 332, 335, 338, 341, 344, 347, 350, 872, 13, 353, 264, 14, 356, 13, 353, 13, 13, 13, 15, 16, 8, 17, 353, 1, 13, 10, 353 },
			{ -1, -1, -1, -1, 88, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 418, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 89, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 596, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 90, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 420, 422, 424, 420, 420, 420, 420, 420, 420, 420, 422, 420, 420, 420, 420, 420, 420, 420, 420, 420, 58, 422, 420, 422, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 422, 420, 420, 420, 420, 420, 420, 420, 420, 420, 422, 420, -1, 420, 420, 420 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 91, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 92, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, 382, -1, 382, 382, 382 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 93, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 94, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, 386, -1, 386, 386, 386 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 95, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, 428, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 96, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 69, 69, -1, -1, 430, 430, -1, -1, -1, -1, -1, -1, -1, -1, 69, -1, 69, -1, -1, 69, -1, 69, 69, 69, -1, -1, -1, -1, -1, -1, 69, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 97, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 98, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 432, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 99, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 434, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 100, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 599, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 101, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 436, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 102, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 438, -1, -1, -1, -1, -1, 440, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 103, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 104, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 442, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 105, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 444, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 106, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 446, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 107, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 601, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 602, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 109, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 448, -1, 448, 448, 448, 448, 448, -1, 448, 448, 448, 448, 448, 448, -1, 448, -1, -1, -1, 416, -1, -1, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, -1, -1, 448, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 448, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 416, -1 },
			{ -1, -1, -1, -1, 872, -1, 110, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 782, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 111, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 420, 422, 424, 420, 420, 420, 420, 420, 420, 420, 422, 420, 420, 420, 420, 420, 420, 420, 420, 420, 74, 422, 420, 422, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 422, 420, 420, 420, 420, 420, 420, 420, 420, 420, 422, 420, -1, 420, 420, 420 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 112, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 422, 422, 452, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 58, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, -1, 422, 422, 422 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 113, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 420, 597, 420, 420, 420, 420, 420, 420, 420, 420, 597, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 597, 420, 454, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 420, 597, 420, 420, 420, 420, 420, 420, 420, 420, 420, 597, 420, -1, 420, 420, 420 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 114, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 115, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, 416, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 116, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 117, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 456, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 118, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 458, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 119, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 462, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 120, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 466, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 121, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 468, -1, -1, -1, 470, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 122, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 472, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 123, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 605, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 128, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 606, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 129, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 448, -1, 448, 448, 448, 448, 448, 86, 448, 448, 448, 448, 448, 448, -1, 448, -1, -1, -1, -1, 268, -1, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, 448, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 448, 448, -1, 448, -1, -1, 448, -1, 448, 448, 448, -1, -1, -1, -1, -1, -1, 448, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 130, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 87, -1, 87, 87, 87, 87, 87, -1, 87, 87, 87, 87, 87, 87, -1, 87, -1, -1, -1, -1, -1, -1, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, 87, -1, -1, 87, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 87, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 131, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 454, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, 597, -1, 597, 597, 597 },
			{ -1, -1, -1, -1, 132, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, 422, 422, 452, 422, 422, 422, 422, 422, 422, 422, 597, 422, 422, 422, 422, 422, 422, 422, 422, 422, 58, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, 422, -1, 422, 422, 422 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 133, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 474, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 134, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 476, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 135, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 478, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 136, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 480, -1, -1, -1, -1, -1, 482, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 484, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 486, 108, 488, 490, -1, -1, -1, -1, -1, -1, -1, -1, 482, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 137, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 492, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 144, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 494, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 145, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 496, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 146, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 498, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 147, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 500, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 148, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 510, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 149, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 607, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 150, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 478, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 124, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 478, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 151, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 512, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 157, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 482, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 108, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 482, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 158, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 514, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 159, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 486, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 125, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 486, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 160, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 516, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 161, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 482, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 162, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 518, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 163, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 520, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 521, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 522, 126, 523, 524, -1, -1, -1, -1, -1, -1, -1, -1, 520, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 165, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 525, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 166, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 526, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 167, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 527, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 168, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 529, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 169, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 530, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 170, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 609, -1, -1, -1, -1, -1, 531, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 127, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 531, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 171, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 610, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 172, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 532, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 173, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 534, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 174, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 514, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 138, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 514, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 175, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 516, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 139, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 516, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 176, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 518, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 140, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 518, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 177, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 520, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 126, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 520, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 535, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 522, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 141, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 522, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 536, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 520, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 525, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 142, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 525, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 537, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 478, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 528, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 143, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 528, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 538, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 539, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 531, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 127, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 531, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 542, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 542, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 533, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 152, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 533, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 482, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 535, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 153, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 535, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 536, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 154, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 536, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 543, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 538, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 155, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 538, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 539, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 156, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 539, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 531, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 541, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 24, 541, 541, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 541, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 544, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 542, -1, -1, 545, -1, 611, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 542, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 543, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 164, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 543, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 546, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 547, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 549, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 550, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 612, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 551, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 552, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 554, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 554, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 555, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 554, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, 179, -1, -1, -1, 556, -1, -1, -1, -1, -1, -1, -1, -1, 179, -1, 179, -1, -1, 179, -1, 179, 179, 179, -1, -1, -1, -1, -1, -1, 179, 554, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 557, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, 179, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, -1, 179, -1, -1, 179, -1, 179, 179, 179, -1, -1, -1, -1, -1, -1, 179, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 558, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 559, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 559, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 560, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 559, -1, -1, -1, -1, 613, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 559, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 561, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 563, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 564, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 180, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ 1, 182, 182, 183, 184, 182, 184, 184, 184, 184, 184, 182, 184, 184, 184, 184, 184, 184, 182, 184, 185, 182, 182, 182, 182, 182, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 184, 186, 186, 184, 182, 182, 270, 182, 182, 182, 182, 182, 182, 187, 184, 186, 182, 272, 182, 182, 186, 182, 186, 186, 186, 273, 308, 271, 188, 182, 1, 186, 182, 308 },
			{ -1, -1, -1, -1, 568, -1, 568, 568, 568, 568, 568, -1, 568, 568, 568, 568, 568, 568, -1, 568, -1, -1, -1, -1, -1, -1, 568, 568, 568, 568, 568, 568, 568, 568, -1, 568, 568, 568, 568, 568, 568, -1, -1, 568, -1, -1, -1, -1, -1, -1, -1, 568, 568, -1, 568, -1, -1, 568, -1, 568, 568, 568, -1, -1, -1, -1, -1, -1, 568, 568, -1 },
			{ -1, -1, -1, -1, 568, -1, 568, 568, 568, 568, 568, -1, 568, 568, 568, 568, 568, 568, -1, 568, -1, -1, -1, -1, -1, -1, 568, 568, 568, 568, 568, 568, 568, 568, -1, 568, 568, 568, 568, 568, 568, -1, -1, 568, -1, -1, -1, -1, -1, -1, -1, 568, 568, -1, 568, -1, -1, 568, -1, 568, 568, 568, -1, 275, -1, -1, -1, -1, 568, 568, -1 },
			{ 1, 202, 202, 570, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 203, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 1, 202, 202, 202 },
			{ -1, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, 202, -1, 202, 202, 202 },
			{ 1, 204, 204, 205, 206, 204, 206, 206, 206, 206, 206, 204, 206, 206, 206, 206, 206, 206, 204, 206, 207, 204, 204, 204, 204, 204, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 206, 208, 208, 206, 204, 204, 279, 204, 204, 204, 204, 204, 204, 209, 206, 208, 204, 281, 204, 204, 208, 204, 208, 208, 208, 282, 310, 280, 210, 204, 1, 208, 204, 310 },
			{ -1, -1, -1, -1, 574, -1, 574, 574, 574, 574, 574, -1, 574, 574, 574, 574, 574, 574, -1, 574, -1, -1, -1, -1, -1, -1, 574, 574, 574, 574, 574, 574, 574, 574, -1, 574, 574, 574, 574, 574, 574, -1, -1, 574, -1, -1, -1, -1, -1, -1, -1, 574, 574, -1, 574, -1, -1, 574, -1, 574, 574, 574, -1, -1, -1, -1, -1, -1, 574, 574, -1 },
			{ -1, -1, -1, -1, 574, -1, 574, 574, 574, 574, 574, -1, 574, 574, 574, 574, 574, 574, -1, 574, -1, -1, -1, -1, -1, -1, 574, 574, 574, 574, 574, 574, 574, 574, -1, 574, 574, 574, 574, 574, 574, -1, -1, 574, -1, -1, -1, -1, -1, -1, -1, 574, 574, -1, 574, -1, -1, 574, -1, 574, 574, 574, -1, 284, -1, -1, -1, -1, 574, 574, -1 },
			{ 1, 224, 224, 225, 226, 224, 226, 226, 226, 226, 226, 224, 226, 226, 226, 226, 226, 226, 224, 226, 227, 290, 224, 224, 224, 224, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 226, 228, 228, 226, 224, 224, 288, 224, 224, 224, 224, 224, 224, 229, 226, 228, 224, 291, 224, 224, 228, 224, 228, 228, 228, 292, 312, 289, 227, 224, 258, 228, 224, 312 },
			{ -1, -1, -1, -1, 577, -1, 577, 577, 577, 577, 577, 242, 577, 577, 577, 577, 577, 577, -1, 577, -1, -1, -1, -1, 298, -1, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, 577, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 577, 577, -1, 577, -1, -1, 577, -1, 577, 577, 577, -1, -1, -1, -1, 579, -1, 577, -1, -1 },
			{ -1, -1, -1, -1, 580, -1, 580, 580, 580, 580, 580, -1, 580, 580, 580, 580, 580, 580, -1, 580, -1, -1, -1, -1, -1, -1, 580, 580, 580, 580, 580, 580, 580, 580, -1, 580, 580, 580, 580, 580, 580, -1, -1, 580, -1, -1, -1, -1, -1, -1, -1, 580, 580, -1, 580, -1, -1, 580, -1, 580, 580, 580, -1, -1, -1, -1, -1, -1, 580, 580, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 242, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 298, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 580, -1, 580, 580, 580, 580, 580, -1, 580, 580, 580, 580, 580, 580, -1, 580, -1, -1, -1, -1, -1, -1, 580, 580, 580, 580, 580, 580, 580, 580, -1, 580, 580, 580, 580, 580, 580, -1, -1, 580, -1, -1, -1, -1, -1, -1, -1, 580, 580, -1, 580, -1, -1, 580, -1, 580, 580, 580, -1, 294, -1, -1, -1, -1, 580, 580, -1 },
			{ 1, 243, 243, 243, 244, 243, 244, 244, 244, 244, 244, 243, 244, 244, 244, 244, 244, 244, 243, 244, 243, 243, 243, 243, 243, 243, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 244, 243, 243, 244, 243, 243, 243, 243, 243, 243, 243, 243, 243, 243, 244, 243, 243, 243, 243, 243, 243, 243, 243, 243, 243, 243, 243, 243, 243, 243, 1, 243, 243, 243 },
			{ 1, 245, 245, 245, 246, 245, 246, 246, 246, 246, 246, 245, 246, 246, 246, 246, 246, 246, 245, 246, 245, 245, 245, 245, 245, 245, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 246, 245, 245, 246, 245, 245, 245, 245, 245, 245, 245, 245, 245, 245, 246, 245, 245, 245, 245, 245, 245, 245, 245, 245, 245, 245, 245, 245, 245, 245, 1, 245, 245, 245 },
			{ 1, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 248, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 1, 247, 247, 247 },
			{ 1, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 251, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 1, 250, 250, 250 },
			{ 1, 253, 254, 254, 254, 253, 254, 254, 254, 254, 254, 255, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 299, 254, 300, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 1, 254, 254, 254 },
			{ -1, -1, -1, -1, 872, -1, 872, 324, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 787, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 276, -1, -1, -1, -1, -1, -1, 276, -1, -1, -1, 276, -1, -1, -1, -1, -1, -1, -1, -1, 276, -1, 276, -1, -1, 276, -1, -1, -1, -1, -1, -1, 276, 276, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 276, -1, 276, -1, -1, 276, -1, 276, 276, 276, -1, -1, -1, -1, -1, -1, 276, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 277, 277, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 277, -1, -1, -1, -1, 277, 277, 277, -1, -1, -1, -1, -1, -1, 277, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 285, -1, -1, -1, -1, -1, -1, 285, -1, -1, -1, 285, -1, -1, -1, -1, -1, -1, -1, -1, 285, -1, 285, -1, -1, 285, -1, -1, -1, -1, -1, -1, 285, 285, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 285, -1, 285, -1, -1, 285, -1, 285, 285, 285, -1, -1, -1, -1, -1, -1, 285, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 286, 286, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 286, -1, -1, -1, -1, 286, 286, 286, -1, -1, -1, -1, -1, -1, 286, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 295, -1, -1, -1, -1, -1, -1, 295, -1, -1, -1, 295, -1, -1, -1, -1, -1, -1, -1, -1, 295, -1, 295, -1, -1, 295, -1, -1, -1, -1, -1, -1, 295, 295, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 295, -1, 295, -1, -1, 295, -1, 295, 295, 295, -1, -1, -1, -1, -1, -1, 295, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 296, 296, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 296, -1, -1, -1, -1, 296, 296, 296, -1, -1, -1, -1, -1, -1, 296, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 325, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 366, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 372, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 450, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 600, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 460, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 464, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 615, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 604, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 508, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 506, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 608, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 502, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 533, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 528, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 540, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 541, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 548, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 553, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 562, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 632, 872, 872, 633, 330, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 504, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 333, -1, 872, 872, 872, 872, 872, -1, 872, 872, 820, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 339, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 642, 784, 872, 872, -1, 872, 643, 872, 872, 819, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 342, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 345, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 348, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 791, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 825, 872, 872, 872, -1, 872, 650, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 651, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 351, -1, 872, 872, 872, 872, 652, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 653, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 822, -1, 872, 872, 872, 872, 654, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 792, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 655, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 656, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 657, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 857, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 658, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 823, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 357, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 844, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 359, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 661, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 361, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 363, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 365, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 664, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 367, 872, 856, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 369, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 666, -1, 843, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 667, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 371, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 669, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 870, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 670, 872, 872, -1, 872, 872, 872, 872, 872, 671, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 672, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 373, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 673, -1, 674, 872, 872, 872, 675, -1, 676, 826, 795, 677, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 678, 872, 679, 872, 827, 872, 872, 872, 872, 872, 680, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 682, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 375, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 377, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 379, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 381, -1, 872, 872, 872, 872, 880, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 686, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 383, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 797, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 689, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 387, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 858, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 828, -1, 872, 872, 872, 872, 872, 690, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 389, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 391, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 393, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 695, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 395, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 696, -1, 872, 872, 397, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 848, 872, 697, 872, 698, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 399, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 859, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 798, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 401, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 403, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 405, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 407, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 409, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 867, 872, 872, 872, 872, 411, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 803, 703, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 831, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 801, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 850, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 413, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 849, 872, 872, -1, 872, 872, 872, 872, 872, 883, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 705, 872, 872, -1, 872, 872, 872, 872, 861, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 832, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 415, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 417, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 419, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 421, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 707, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 423, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 709, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 710, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 425, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 427, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 429, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 431, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 714, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 805, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 433, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 715, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 437, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 807, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 439, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 835, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 720, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 441, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 443, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 727, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 810, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 838, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 854, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 445, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 447, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 732, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 449, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 809, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 451, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 852, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 837, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 453, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 736, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 333, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 455, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 457, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 459, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 461, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 836, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 738, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 739, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 812, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 853, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 463, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 743, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 465, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 467, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 746, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 469, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 752, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 471, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 754, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 473, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 815, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 475, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 477, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 840, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 479, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 758, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 481, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 760, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 483, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 761, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 485, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 487, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 489, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 491, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 718, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 762, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 763, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 493, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 766, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 767, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 768, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 769, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 495, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 497, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 499, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 501, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 503, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 505, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 771, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 507, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 509, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 772, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 511, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 774, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 513, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 515, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 776, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 517, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 519, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 622, 623, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 624, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 587, -1, -1, -1, -1, -1, -1, 587, -1, -1, -1, 587, -1, -1, -1, -1, -1, -1, -1, -1, 587, -1, 587, -1, -1, 587, -1, -1, -1, -1, -1, -1, 587, 587, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 587, -1, 587, -1, -1, 587, -1, 587, 587, 587, -1, -1, -1, -1, -1, -1, 587, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 589, -1, -1, -1, -1, -1, -1, 589, -1, -1, -1, 589, -1, -1, -1, -1, -1, -1, -1, -1, 589, -1, 589, -1, -1, 589, -1, -1, -1, -1, -1, -1, 589, 589, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 589, -1, 589, -1, -1, 589, -1, 589, 589, 589, -1, -1, -1, -1, -1, -1, 589, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 591, -1, -1, -1, -1, -1, -1, 591, -1, -1, -1, 591, -1, -1, -1, -1, -1, -1, -1, -1, 591, -1, 591, -1, -1, 591, -1, -1, -1, -1, -1, -1, 591, 591, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 591, -1, 591, -1, -1, 591, -1, 591, 591, 591, -1, -1, -1, -1, -1, -1, 591, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 595, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 603, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 681, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 668, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 865, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 665, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 659, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 662, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 687, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 692, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 684, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 796, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 699, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 700, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 704, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 708, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 804, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 719, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 833, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 716, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 724, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 726, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 723, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 862, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 811, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 734, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 735, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 884, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 748, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 740, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 750, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 814, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 757, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 765, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 764, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 770, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 773, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 625, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 794, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 663, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 660, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 846, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 691, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 693, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 685, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 802, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 706, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 713, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 712, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 721, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 808, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 729, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 733, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 741, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 737, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 755, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 751, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 742, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 816, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 885, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 775, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 628, 872, 872, 872, -1, 872, 629, 872, 872, 630, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 866, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 694, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 847, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 860, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 871, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 806, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 728, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 725, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 745, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 749, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 756, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 744, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 631, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 800, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 829, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 711, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 717, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 731, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 834, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 747, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 634, 872, 872, 872, -1, 788, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 635, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 830, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 799, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 722, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 730, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 636, 872, 872, 872, 872, -1, 637, 872, 638, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 639, 872, 872, 872, 872, 872, 640, 872, 872, 786, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 702, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 851, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 645, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 648, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 783, 872, 872, 872, -1, 872, 649, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 778, -1, -1, -1, -1, -1, -1, 778, -1, -1, -1, 778, -1, -1, -1, -1, -1, -1, -1, -1, 778, -1, 778, -1, -1, 778, -1, -1, -1, -1, -1, -1, 778, 778, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 778, -1, 778, -1, -1, 778, -1, 778, 778, 778, -1, -1, -1, -1, -1, -1, 778, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 779, -1, -1, -1, -1, -1, -1, 779, -1, -1, -1, 779, -1, -1, -1, -1, -1, -1, -1, -1, 779, -1, 779, -1, -1, 779, -1, -1, -1, -1, -1, -1, 779, 779, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 779, -1, 779, -1, -1, 779, -1, 779, 779, 779, -1, -1, -1, -1, -1, -1, 779, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 780, -1, -1, -1, -1, -1, -1, 780, -1, -1, -1, 780, -1, -1, -1, -1, -1, -1, -1, -1, 780, -1, 780, -1, -1, 780, -1, -1, -1, -1, -1, -1, 780, 780, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 780, -1, 780, -1, -1, 780, -1, 780, 780, 780, -1, -1, -1, -1, -1, -1, 780, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 781, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 868, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 813, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 817, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 881, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 839, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, 872, -1, 872, 872, 872, 872, 872, -1, 872, 872, 872, 872, 872, 872, -1, 872, -1, -1, -1, -1, -1, -1, 872, 872, 872, 841, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 872, 378, -1, -1, -1, -1, -1, -1, -1, -1, -1, 872, 872, -1, 872, -1, -1, 872, -1, 872, 872, 872, -1, -1, -1, -1, -1, -1, 872, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 876, -1, -1, -1, -1, -1, -1, 876, -1, -1, -1, 876, -1, -1, -1, -1, -1, -1, -1, -1, 876, -1, 876, -1, -1, 876, -1, -1, -1, -1, -1, -1, 876, 876, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 876, -1, 876, -1, -1, 876, -1, 876, 876, 876, -1, -1, -1, -1, -1, -1, 876, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 877, -1, -1, -1, -1, -1, -1, 877, -1, -1, -1, 877, -1, -1, -1, -1, -1, -1, -1, -1, 877, -1, 877, -1, -1, 877, -1, -1, -1, -1, -1, -1, 877, 877, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 877, -1, 877, -1, -1, 877, -1, 877, 877, 877, -1, -1, -1, -1, -1, -1, 877, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 878, -1, -1, -1, -1, -1, -1, 878, -1, -1, -1, 878, -1, -1, -1, -1, -1, -1, -1, -1, 878, -1, 878, -1, -1, 878, -1, -1, -1, -1, -1, -1, 878, 878, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 878, -1, 878, -1, -1, 878, -1, 878, 878, 878, -1, -1, -1, -1, -1, -1, 878, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 886, -1, -1, -1, -1, -1, -1, 886, -1, -1, -1, 886, -1, -1, -1, -1, -1, -1, -1, -1, 886, -1, 886, -1, -1, 886, -1, -1, -1, -1, -1, -1, 886, 886, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 886, -1, 886, -1, -1, 886, -1, 886, 886, 886, -1, -1, -1, -1, -1, -1, 886, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 887, -1, -1, -1, -1, -1, -1, 887, -1, -1, -1, 887, -1, -1, -1, -1, -1, -1, -1, -1, 887, -1, 887, -1, -1, 887, -1, -1, -1, -1, -1, -1, 887, 887, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 887, -1, 887, -1, -1, 887, -1, 887, 887, 887, -1, -1, -1, -1, -1, -1, 887, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, 888, -1, -1, -1, -1, -1, -1, 888, -1, -1, -1, 888, -1, -1, -1, -1, -1, -1, -1, -1, 888, -1, 888, -1, -1, 888, -1, -1, -1, -1, -1, -1, 888, 888, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 888, -1, 888, -1, -1, 888, -1, 888, 888, 888, -1, -1, -1, -1, -1, -1, 888, -1, -1 }
		};
		
		
		private static int[] yy_state_dtrans = new int[]
		{
			  0,
			  374,
			  565,
			  569,
			  571,
			  575,
			  581,
			  582,
			  583,
			  584,
			  585
		};
		
		#endregion
		
		public Tokens GetNextToken()
		{
			int current_state = yy_state_dtrans[(int)current_lexical_state];
			int last_accept_state = NoState;
			bool is_initial_state = true;
			
			MarkTokenChunkStart();
			token_start = token_chunk_start;
			expanding_token = false;
			AdvanceEndPosition((token_end > 0) ? token_end - 1 : 0, token_start);
			
			// capture token start position:
			token_start_pos.Char = token_end_pos.Char;
			token_start_pos.Line = token_end_pos.Line;
			token_start_pos.Column = token_end_pos.Column;
			
			if (acceptCondition[current_state] != AcceptConditions.NotAccept)
			{
				last_accept_state = current_state;
				MarkTokenEnd();
			}
			
			while (true)
			{
				char lookahead = (is_initial_state && yy_at_bol) ? BOL : Advance();
				int next_state = nextState[rowMap[current_state], colMap[lookahead]];
				
				if (lookahead == EOF && is_initial_state)
				{
					return Tokens.EOF;
				}
				if (next_state != -1)
				{
					current_state = next_state;
					is_initial_state = false;
					
					if (acceptCondition[current_state] != AcceptConditions.NotAccept)
					{
						last_accept_state = current_state;
						MarkTokenEnd();
					}
				}
				else
				{
					if (last_accept_state == NoState)
					{
						return Tokens.ERROR;
					}
					else
					{
						if ((acceptCondition[last_accept_state] & AcceptConditions.AcceptOnEnd) != 0)
							TrimTokenEnd();
						MoveToTokenEnd();
						
						if (last_accept_state < 0)
						{
							System.Diagnostics.Debug.Assert(last_accept_state >= 892);
						}
						else
						{
							bool accepted = false;
							yyreturn = Accept0(last_accept_state, out accepted);
							if (accepted)
							{
								AdvanceEndPosition(token_start, token_end - 1);
								return yyreturn;
							}
						}
						
						// token ignored:
						is_initial_state = true;
						current_state = yy_state_dtrans[(int)current_lexical_state];
						last_accept_state = NoState;
						MarkTokenChunkStart();
						if (acceptCondition[current_state] != AcceptConditions.NotAccept)
						{
							last_accept_state = current_state;
							MarkTokenEnd();
						}
					}
				}
			}
		} // end of GetNextToken
	}
}
