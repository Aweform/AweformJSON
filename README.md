# AweformJSON
This tiny JSON parser was created for use in [Aweform](https://aweform.com). It will decode a JSON file into a series of AweformJSON.Element objects that can be used to conveniently access the data, make some modifications, and then if desired convert it back into a JSON string.

```c#
AweformJSON.Element element = AweformJSON.Parse("{ \"number\": 123, \"string\": \"Hello World\", \"booleanTrue\": true, \"booleanFalse\": false, \"array\": [ \"string\", 123, true, false, null ], \"nullVariable\": null, \"object\": { \"subArray\": [ { \"with\": \"objects\" } ] } }");

Console.WriteLine(element.ToJSON());
Console.WriteLine(element.GetAttributeAsInt32("number"));
Console.WriteLine(element.GetAttributeAsString("string"));
Console.WriteLine(element.GetAttributeAsBoolean("booleanTrue"));
Console.WriteLine(element.GetAttribute("array").ToJSON());

element.SetAttribute("somethingNew", "forUsToSee");

Console.WriteLine(element.ToJSON());

element.SetAttribute("somethingNew", 123);

Console.WriteLine(element.ToJSON());
```
