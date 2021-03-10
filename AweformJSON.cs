using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Aweform {

	//
	// AweformJSON
	// This is a tiny JSON parser used to parse a JSON string into a simple
	// in memory representation. This "library" is optimally used by just adding 
	// this single file to your .Net project
	// NOTE: that this parser has only basic syntax error handling
	// (c) 2021 - Aweform - https://aweform.com/
	////////////////////////////////////////////////////////////////////////////////////

		public static class AweformJSON {

			public class InvalidSyntaxException : Exception {

				public InvalidSyntaxException(String message) : base(message) {
				}
			}

			private class ParseContext {

				public Char[] Chars;
				public Int32 Index;
				public Int32 PeekIndex;
				public StringBuilder ParseStringStringBuilder;
			}

			public enum Token {

				EndOrUnknown,
				ObjectStart,
				ObjectEnd,
				ArrayStart,
				ArrayEnd,
				Colon,
				Comma,
				String,
				Number,
				True,
				False,
				Null
			}

			public enum ElementType {

				String,
				Number,
				Object,
				Array,
				Boolean,
				Null
			}

			public class Element {

				public ElementType Type { get; set; }
				public String Name { get; set; }
				public String Value { get; set; } // String, Number, true, false, null
				public List<Element> Elements { get; set; } // Attributes if "Object", Items if "Array"

				public Element(ElementType type, String value = null) {

					Type = type;
					Elements = null;
					Value = value;
				}

				public void SetAttribute(String name, String value) {

					GetOrCreateAttributeOfType(name, (value == null)? ElementType.Null : ElementType.String).Value = value;
				}

				public void SetAttribute(String name, Int32 value) {

					GetOrCreateAttributeOfType(name, ElementType.Number).Value = value.ToString(CultureInfo.InvariantCulture);
				}

				public void SetAttribute(String name, Int64 value) {

					GetOrCreateAttributeOfType(name, ElementType.Number).Value = value.ToString(CultureInfo.InvariantCulture);
				}

				public void SetAttribute(String name, Decimal value) {

					GetOrCreateAttributeOfType(name, ElementType.Number).Value = value.ToString(CultureInfo.InvariantCulture);
				}

				public void SetAttribute(String name, Double value) {

					GetOrCreateAttributeOfType(name, ElementType.Number).Value = value.ToString(CultureInfo.InvariantCulture);
				}

				public void SetAttribute(String name, Boolean value) {

					GetOrCreateAttributeOfType(name, ElementType.Boolean).Value = value.ToString().ToLower();
				}

				private Element GetOrCreateAttributeOfType(String name, ElementType type) {

					if (Type != ElementType.Object) { throw new Exception("You cannot set an attribute on a none Object type Element"); }
	
					Element attribute = GetAttribute(name);

					if (attribute == null) { 
					
						attribute = new Element(type); 
						attribute.Name = name;

						Elements.Add(attribute); 
					
					} else {

						attribute.Type = type;
					}

					return attribute;
				}

				public String GetAttributeAsString(String name, String defaultValue = "") {

					Element attribute = GetAttribute(name);

					if (attribute == null || attribute.Value == null) { 
				
						return defaultValue;
					}

					return attribute.Value;
				}

				public Int32 GetAttributeAsInt32(String name, Int32 defaultValue = 0) {

					Element attribute = GetAttribute(name);

					if (attribute == null || attribute.Value == null) {

						return defaultValue;
					}

					Int32 int32;

					if (Int32.TryParse(attribute.Value, out int32)) {

						return int32;

					} else {

						return defaultValue;
					}
				}
			
				public Int64 GetAttributeAsInt64(String name, Int64 defaultValue = 0) {

					Element attribute = GetAttribute(name);

					if (attribute == null || attribute.Value == null) {

						return defaultValue;
					}

					Int64 int64;

					if (Int64.TryParse(attribute.Value, out int64)) {

						return int64;

					} else {

						return defaultValue;
					}
				}

				public Decimal GetAttributeAsDecimal(String name, Decimal defaultValue = 0) {

					Element attribute = GetAttribute(name);

					if (attribute == null || attribute.Value == null) {

						return defaultValue;
					}

					Decimal d;

					if (Decimal.TryParse(attribute.Value, out d)) {

						return d;

					} else {

						return defaultValue;
					}
				}

				public Double GetAttributeAsDouble(String name, Double defaultValue = 0) {

					Element attribute = GetAttribute(name);

					if (attribute == null || attribute.Value == null) {

						return defaultValue;
					}

					Double d;

					if (Double.TryParse(attribute.Value, out d)) {

						return d;

					} else {

						return defaultValue;
					}
				}

				public Boolean GetAttributeAsBoolean(String name, Boolean defaultValue = false) {

					Element attribute = GetAttribute(name);

					if (attribute == null || attribute.Value == null) {

						return defaultValue;
					}

					Boolean b;

					if (Boolean.TryParse(attribute.Value, out b)) {

						return b;

					} else {

						return defaultValue;
					}
				}

				public Element GetAttribute(String name) {

					if (Type != ElementType.Object) { throw new Exception("You cannot get an attribute on a none Object type Element"); }

					foreach (Element value in Elements) {

						if (value.Name == name) {

							return value;
						}
					}

					return null;
				}

				public List<Element> GetAttributes() {

					if (Type != ElementType.Object) {

						return null;
					}

					return Elements;
				}

				public List<Element> GetItems() {

					if (Type != ElementType.Array) {

						return null;
					}

					return Elements;
				}

				public String ToJSON() {

					if (Type == ElementType.Null) {

						return "null";

					} else if (Type == ElementType.Boolean) {

						return Value;

					} else if (Type == ElementType.Number) {

						return Value;

					} else if (Type == ElementType.String) {

						return "\"" + EncodeStringValue(Value) + "\"";
					}

					StringBuilder sb = new StringBuilder();

					if (Type == ElementType.Object) {

						sb.Append("{");

						Boolean isFirst = true;

						foreach (Element attribute in Elements) {

							if (isFirst) { isFirst = false; } else { sb.Append(", "); }

							sb.Append("\"" + EncodeStringValue(attribute.Name) + "\": ");
							sb.Append(attribute.ToJSON());
						}

						sb.Append("}");

					} else if (Type == ElementType.Array) {

						sb.Append("[");

						Boolean isFirst = true;

						foreach (Element value in Elements) {

							if (isFirst) { isFirst = false; } else { sb.Append(", "); }
							sb.Append(value.ToJSON());
						}

						sb.Append("]");
					}

					return sb.ToString();
				}
			}
		
			public static Element Parse(String json) {
						
				if (json == null) { throw new InvalidSyntaxException("Cannot parse a null string"); }

				ParseContext parseContext = new ParseContext();
				parseContext.Chars = json.ToCharArray();
				parseContext.ParseStringStringBuilder = new StringBuilder();
				
				return ParseElement(parseContext);
			}
		
			private static Element ParseElement(ParseContext parseContext) {

				Token nextToken = PeekToken(parseContext);

				if (nextToken == Token.ObjectStart) {

					return ParseObjectElement(parseContext);

				} else if (nextToken == Token.ArrayStart) {

					return ParseArrayElement(parseContext);

				} else if (nextToken == Token.String) {

					parseContext.Index = parseContext.PeekIndex - 1;
					return new Element(ElementType.String, ParseString(parseContext));

				} else if (nextToken == Token.Number) {

					return ParseNumberElement(parseContext);

				} else if (nextToken == Token.True) {

					parseContext.Index = parseContext.PeekIndex;
					return new Element(ElementType.Boolean, "true");

				} else if (nextToken == Token.False) {

					parseContext.Index = parseContext.PeekIndex;
					return new Element(ElementType.Boolean, "false");

				} else if (nextToken == Token.Null) {

					parseContext.Index = parseContext.PeekIndex;
					return new Element(ElementType.Null, "null");

				} else {

					throw new InvalidSyntaxException("Unexpected token (" + nextToken + ") at char " + parseContext.Index);
				}
			}

			private static Element ParseObjectElement(ParseContext parseContext) {

				parseContext.Index = parseContext.PeekIndex;

				Element objectElement = new Element(ElementType.Object);
				objectElement.Elements = new List<Element>();

				while (true) {

					Token nextToken = PeekToken(parseContext);

					if (nextToken == Token.String) {

						String attributeName = ParseString(parseContext);

						if (GetNextToken(parseContext) != Token.Colon) {

							throw new InvalidSyntaxException("Unexpected token at char " + parseContext.Index + " expected a Colon before the attribute value");
						}

						Element attributeElement = ParseElement(parseContext);
						attributeElement.Name = attributeName;

						objectElement.Elements.Add(attributeElement);

					} else if (nextToken == Token.Comma) {

						if (objectElement.Elements.Count == 0) {

							throw new InvalidSyntaxException("Unexpected Comma at the start of an Object at char " + parseContext.Index);
						}

						parseContext.Index = parseContext.PeekIndex;
						
					} else if (nextToken == Token.ObjectEnd) {

						parseContext.Index = parseContext.PeekIndex;
						return objectElement;

					} else {

						throw new InvalidSyntaxException("Unexpected token (" + nextToken + ") at char " + parseContext.Index);
					}
				}
			}

			private static Element ParseArrayElement(ParseContext parseContext) {

				parseContext.Index = parseContext.PeekIndex;
			
				Element arrayElement = new Element(ElementType.Array);
				arrayElement.Elements = new List<Element>();

				while (true) {

					Token nextToken = PeekToken(parseContext);

					if (nextToken == Token.EndOrUnknown) {
				
						throw new InvalidSyntaxException("Unexpected token (" + nextToken + ") at char " + parseContext.Index);

					} else if (nextToken == Token.Comma) {

						if (arrayElement.Elements.Count == 0) {

							throw new InvalidSyntaxException("Unexpected Comma at the start of an Array at char " + parseContext.Index);
						}

						parseContext.Index = parseContext.PeekIndex;

					} else if (nextToken == Token.ArrayEnd) {

						parseContext.Index = parseContext.PeekIndex;
						break;

					} else {

						arrayElement.Elements.Add(ParseElement(parseContext));
					}
				}

				return arrayElement;
			}

			private static String ParseString(ParseContext parseContext) {

				parseContext.Index = parseContext.PeekIndex; // move to "

				StringBuilder sb = parseContext.ParseStringStringBuilder;
				sb.Clear();

				while (true) {

					if (parseContext.Index == parseContext.Chars.Length) {

						break;
					}

					Char c = parseContext.Chars[parseContext.Index++];

					if (c == '"') {

						return sb.ToString();

					} else if (c == '\\') {

						if (parseContext.Index == parseContext.Chars.Length) {
					
							break;
						}

						c = parseContext.Chars[parseContext.Index++];

						if (c == '"') {

							sb.Append('"');

						} else if (c == '\\') {
						
							sb.Append('\\');

						} else if (c == '/') {
						
							sb.Append('/');

						} else if (c == 'b') {
						
							sb.Append('\b');

						} else if (c == 'f') {
						
							sb.Append('\f');

						} else if (c == 'n') {
						
							sb.Append('\n');

						} else if (c == 'r') {
						
							sb.Append('\r');

						} else if (c == 't') {
						
							sb.Append('\t');

						} else if (c == 'u') {
						
							Int32 remainingLength = parseContext.Chars.Length - parseContext.Index;
						
							if (remainingLength >= 4) {
						
								Int32 codePoint;
								String potential = new String(parseContext.Chars, parseContext.Index, 4);

								if (!Int32.TryParse(potential, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)) {

									throw new InvalidSyntaxException("\"" + potential + "\" is not a valid unicode character code");
								}
							
								sb.Append(Char.ConvertFromUtf32((Int32)codePoint));
							
								parseContext.Index += 4;

							} else {

								break;
							}

						} else {

							throw new InvalidSyntaxException("Invalid escape sequence at char " + parseContext.Index);
						}

					} else {

						sb.Append(c);
					}
				}

				throw new InvalidSyntaxException("Found an incomplete string at char " + parseContext.Index);
			}

			private static Element ParseNumberElement(ParseContext parseContext) {

				parseContext.Index = parseContext.PeekIndex - 1;

				Int32 lastNumberCharacterIndex;
				String numberCharacters = "0123456789-.eE";

				for (lastNumberCharacterIndex = parseContext.Index; lastNumberCharacterIndex < parseContext.Chars.Length; ++lastNumberCharacterIndex) {
			
					if (!numberCharacters.Contains(parseContext.Chars[lastNumberCharacterIndex], StringComparison.Ordinal)) {
			
						break;
					}
				}

				lastNumberCharacterIndex -= 1;

				Int32 charLength = (lastNumberCharacterIndex - parseContext.Index) + 1;

				Double number;
				String potential = new String(parseContext.Chars, parseContext.Index, charLength);

				if (!Double.TryParse(potential, NumberStyles.Any, CultureInfo.InvariantCulture, out number)) {

					throw new InvalidSyntaxException("\"" + potential + "\" is not a valid number at char " + parseContext.Index);
				}

				parseContext.Index = lastNumberCharacterIndex + 1;

				Element numberElement = new Element(ElementType.Number);
				numberElement.Value = number.ToString(CultureInfo.InvariantCulture);

				return numberElement;
			}

			private static Token PeekToken(ParseContext parseContext) {
			
				Int32 peekFromIndex = parseContext.Index;
				Token peekToken = GetNextToken(parseContext);
				parseContext.PeekIndex = parseContext.Index; // we store the PeekIndex to make things faster in some cases
				parseContext.Index = peekFromIndex;

				return peekToken;
			}

			private static Token GetNextToken(ParseContext parseContext) {
			
				SkipWhitespace(parseContext);

				if (parseContext.Index == parseContext.Chars.Length) {
			
					return Token.EndOrUnknown;
				}

				Char c = parseContext.Chars[parseContext.Index++];
			
				if (c == '{') {
					
					return Token.ObjectStart;
				
				} else if (c == '}') {

					return Token.ObjectEnd;

				} else if (c == '[') {

					return Token.ArrayStart;

				} else if (c == ']') {

					return Token.ArrayEnd;

				} else if (c == ':') {

					return Token.Colon;

				} else if (c == ',') {

					return Token.Comma;

				} else if (c == '"') {

					return Token.String;

				} else if ("-0123456789".Contains(c, StringComparison.Ordinal)) {

					return Token.Number;
				}

				parseContext.Index--;

				if (ForwardMatch("false", parseContext)) {

					return Token.False;

				} else if (ForwardMatch("true", parseContext)) {

					return Token.True;

				} else if (ForwardMatch("null", parseContext)) {

					return Token.Null;

				} else {

					return Token.EndOrUnknown;
				}
			}

			private static Boolean ForwardMatch(String what, ParseContext parseContext) {

				Int32 remainingLength = parseContext.Chars.Length - parseContext.Index;

				if (remainingLength >= what.Length) {

					for (Int32 i = 0; i < what.Length; ++i) {

						if (parseContext.Chars[parseContext.Index + i] != what[i]) {

							return false;
						}
					}

					parseContext.Index += what.Length;
					return true;
				}

				return false;
			}

			private static void SkipWhitespace(ParseContext parseContext) {
			
				while (parseContext.Index < parseContext.Chars.Length) {

					Char c = parseContext.Chars[parseContext.Index];

					if (c != ' ' && c != '\t' && c != '\n' && c != '\r') {

						break;
					}

					parseContext.Index++;
				}
			}

			private static String EncodeStringValue(String s) {

				if (s == null || s == "") { return ""; }

				Char[] chars = s.ToCharArray();
				StringBuilder sb = new StringBuilder();

				foreach (Char c in chars) {

					if (c == '"') {

						sb.Append("\\\"");

					} else if (c == '\\') {

						sb.Append("\\\\");

					} else if (c == '\b') {

						sb.Append("\\b");

					} else if (c == '\f') {

						sb.Append("\\f");

					} else if (c == '\t') {

						sb.Append("\\t");

					} else if (c == '\n') {

						sb.Append("\\n");

					} else if (c == '\r') {

						sb.Append("\\r");

					} else if (c >= 32 || c <= 126) {

						sb.Append(c);

					} else {

						sb.Append("\\u" + Convert.ToString(c, 16).PadLeft(4, '0'));
					}
				}

				return sb.ToString();
			}
		}
}
