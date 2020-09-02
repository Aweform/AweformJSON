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
	// NOTE that this parser has only basic error handling, it will NOT report
	// in a meaningful way on potential JSON issues
	// (c) 2020 - Aweform - https://aweform.com/
	////////////////////////////////////////////////////////////////////////////////////

		public static class AweformJSON {

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
				public List<Element> Values { get; set; } // Attributes if "Object", Items if "Array"

				public Element(ElementType type, String value = null) {

					Type = type;
					Values = null;
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

						Values.Add(attribute); 
					
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

					if (Type == ElementType.Object) {

						foreach (Element value in Values) {

							if (value.Name == name) {

								return value;
							}
						}
					}

					return null;
				}

				public List<Element> GetAttributes() {

					if (Type != ElementType.Object) {

						return null;
					}

					return Values;
				}

				public List<Element> GetItems() {

					if (Type != ElementType.Array) {

						return null;
					}

					return Values;
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

						foreach (Element attribute in Values) {

							if (isFirst) { isFirst = false; } else { sb.Append(", "); }

							sb.Append("\"" + attribute.Name + "\": ");
							sb.Append(attribute.ToJSON());
						}

						sb.Append("}");

					} else if (Type == ElementType.Array) {

						sb.Append("[");

						Boolean isFirst = true;

						foreach (Element value in Values) {

							if (isFirst) { isFirst = false; } else { sb.Append(", "); }
							sb.Append(value.ToJSON());
						}

						sb.Append("]");
					}

					return sb.ToString();
				}
			}
		
			public static Element Parse(String json) {
						
				if (json != null) {
			
					Int32 ix = 0;
					Boolean success = true;

					Element element = ParseElement(json.ToCharArray(), ref ix, ref success);

					if (success) {

						return element;

					} else {

						return null;
					}
			
				} else {
			
					return null;
				}
			}

			private static Element ParseObject(Char[] chars, ref Int32 ix, ref Boolean success) {
			
				Element element = new Element(ElementType.Object);
			
				GetNextToken(chars, ref ix); // skip {

				while (true) {

					Token nextToken = PeekNextToken(chars, ix);

					if (nextToken == Token.EndOrUnknown) {

						success = false;
						return null;

					} else if (nextToken == Token.Comma) {

						GetNextToken(chars, ref ix); // skip ,

					} else if (nextToken == Token.ObjectEnd) {

						GetNextToken(chars, ref ix); // skip }
						return element;

					} else if (nextToken == Token.String) {

						String name = ParseString(chars, ref ix, ref success);

						if (!success) {

							return null;
						}

						nextToken = GetNextToken(chars, ref ix);

						if (nextToken != Token.Colon) {

							success = false;
							return null;
						}

						Element value = ParseElement(chars, ref ix, ref success);
					
						if (!success) {
					
							return null;
						}

						value.Name = name;

						if (element.Values == null) {

							element.Values = new List<Element>();
						}

						element.Values.Add(value);

					} else {

						success = false;
						break;
					}
				}

				return element;
			}

			private static Element ParseArray(Char[] chars, ref Int32 ix, ref Boolean success) {

				Element element = new Element(ElementType.Array);
				element.Values = new List<Element>();

				GetNextToken(chars, ref ix); // skip [

				while (true) {

					Token token = PeekNextToken(chars, ix);

					if (token == Token.EndOrUnknown) {
				
						success = false;
						return null;

					} else if (token == Token.Comma) {

						GetNextToken(chars, ref ix); // skip ,

					} else if (token == Token.ArrayEnd) {
					
						GetNextToken(chars, ref ix); // skip ]
						break;

					} else {

						element.Values.Add(ParseElement(chars, ref ix, ref success));
				
						if (!success) {
				
							return null;
						}
					}
				}

				return element;
			}

			private static Element ParseElement(Char[] chars, ref Int32 ix, ref Boolean success) {

				Element element = null;

				Token nextToken = PeekNextToken(chars, ix);

				if (nextToken == Token.String) {

					element = new Element(ElementType.String, ParseString(chars, ref ix, ref success));

				} else if (nextToken == Token.Number) {

					element = ParseNumber(chars, ref ix, ref success);

				} else if (nextToken == Token.ObjectStart) {

					element = ParseObject(chars, ref ix, ref success);

				} else if (nextToken == Token.ArrayStart) {

					element = ParseArray(chars, ref ix, ref success);
					
				} else if (nextToken == Token.True || nextToken == Token.False) {

					GetNextToken(chars, ref ix);

					element = new Element(ElementType.Boolean, (nextToken == Token.True)? "true" : "false");

				} else if (nextToken == Token.Null) {

					GetNextToken(chars, ref ix);

					element = new Element(ElementType.Null, "null");

				} else {

					success = false;
				}

				return element;
			}

			private static String ParseString(Char[] chars, ref Int32 ix, ref Boolean success) {
			
				StringBuilder sb = new StringBuilder();

				SkipWhitespace(chars, ref ix);

				ix++; // skip "

				Boolean complete = false;

				while (!complete) {

					if (ix == chars.Length) {

						break;
					}

					Char c = chars[ix++];

					if (c == '"') {

						complete = true;
						break;

					} else if (c == '\\') {

						if (ix == chars.Length) {
					
							break;
						}

						c = chars[ix++];

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
						
							Int32 remainingLength = chars.Length - ix;
						
							if (remainingLength >= 4) {
						
								Int32 codePoint;

								if (!Int32.TryParse(new String(chars, ix, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)) {

									success = false;
									return "";
								}
							
								sb.Append(Char.ConvertFromUtf32((Int32)codePoint));
							
								ix += 4;

							} else {

								break;
							}
						}

					} else {

						sb.Append(c);
					}
				}

				if (!complete) {
				
					success = false;
					return null;
				}

				return sb.ToString();
			}

			private static Element ParseNumber(Char[] chars, ref Int32 ix, ref Boolean success) {

				SkipWhitespace(chars, ref ix);

				Int32 lastNumberCharacterIndex;
				String numberCharacters = "0123456789-.eE";

				for (lastNumberCharacterIndex = ix; lastNumberCharacterIndex < chars.Length; ++lastNumberCharacterIndex) {
			
					if (!numberCharacters.Contains(chars[lastNumberCharacterIndex], StringComparison.Ordinal)) {
			
						break;
					}
				}

				lastNumberCharacterIndex -= 1;

				Int32 charLength = (lastNumberCharacterIndex - ix) + 1;

				Double number;
				success = Double.TryParse(new String(chars, ix, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

				ix = lastNumberCharacterIndex + 1;

				Element element = new Element(ElementType.Number);
				element.Value = number.ToString(CultureInfo.InvariantCulture);

				return element;
			}

			private static Token PeekNextToken(Char[] chars, Int32 ix) {
			
				Int32 peekFromIndex = ix;
				return GetNextToken(chars, ref peekFromIndex);
			}

			private static Token GetNextToken(Char[] chars, ref Int32 ix) {
			
				SkipWhitespace(chars, ref ix);

				if (ix == chars.Length) {
			
					return Token.EndOrUnknown;
				}

				Char c = chars[ix++];
			
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

				ix--;

				if (ForwardMatch("false", chars, ref ix)) {

					return Token.False;

				} else if (ForwardMatch("true", chars, ref ix)) {

					return Token.True;

				} else if (ForwardMatch("null", chars, ref ix)) {

					return Token.Null;

				} else {

					return Token.EndOrUnknown;
				}
			}

			private static Boolean ForwardMatch(String what, Char[] chars, ref Int32 ix) {

				Int32 remainingLength = chars.Length - ix;

				if (remainingLength >= what.Length) {

					for (Int32 i = 0; i < what.Length; ++i) {

						if (chars[ix + i] != what[i]) {

							return false;
						}
					}

					ix += what.Length;
					return true;
				}

				return false;
			}

			private static void SkipWhitespace(Char[] chars, ref Int32 ix) {
			
				while (ix < chars.Length) {

					Char c = chars[ix];

					if (c != ' ' && c != '\t' && c != '\n' && c != '\r') {

						break;
					}

					ix++;
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

					} else if (c == '\n') {

						sb.Append("\\n");

					} else if (c == '\r') {

						sb.Append("\\r");

					} else if (c < 20) {

						sb.Append("\\u" + ((Int32)c).ToString().PadLeft(4, '0'));

					} else {

						sb.Append(c);
					}
				}

				return sb.ToString();
			}
		}
}