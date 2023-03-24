![](https://github.com/JanCotar/JanC_dll/blob/main/JanC_icon.svg) ![](https://github.com/JanCotar/JanC_dll/blob/main/Caption.svg)

[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-orange)](https://www.nuget.org/packages/JanCLang/) [![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE.md)

## JanCLang

[JanCLang](#JanC-implementation) is an interpreter for [JanC programming language](#JanC-language-specification) with Slovene syntax. The language resembles C#, which is also the language the interpreter is developed in. The project is a library that targets *.NET Standard 2.0*.

It can be used as an interface integrated into an application to allow users of the application to write their own simple business logic using JanC language. It can also be used for teaching purposes. It is [easily](#JanC-language-specification) localized to any natural language as all that needs to be done is translate the [keywords](#JanC-language-specification) and exceptions messages.

Below is a simple example of **Hello, World!** program in JanC programming language. JanC source code is written in a .txt file.

JanCsourceCode.txt content:
```
Program
{
    Izpiši("Pozdravljen, svet!");
}
```
And a C# code to run the interpreter:
```cs
using System;
using System.IO;
using JanCLang;

// JanC source code that outputs "Pozdravljen, svet!" (Eng. "Hello, World!").
string sourceCode = File.ReadAllText("JanCsourceCode.txt");

try
{
    // Initialize JanC object.
    JanC program = new JanC();
    // Override default empty output method (Izpiši()).
    program.RegisterWrite(toWrite => { Console.WriteLine(toWrite) });
    // Run JanC source code.
    program.Run(sourceCode);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

JanCLang is also available as a [NuGet package](https://www.nuget.org/packages/JanCLang/).

**This project is licensed under the terms of the [MIT License](LICENSE.md).**

## JanC language specification

A strongly typed imperative programming language with basic functionalities.
JanC language is easy to learn and use. It was designed with a goal to provide an elementary programming experience in Slovene syntax. If translated to another language, JanC can offer the same experience anywhere in the world.

JanC offers:
 - Declaration and initialization of variables of `int`, `double` and `string` data types.
 - Nested control flow statements (`if`, `if else` and `while`).
 - Algebraic expressions (addition, subtraction, multiplication and division) that can resolve parentheses.
 - Definition of functions with `void`, `int`, `double` and `string` return data types.
 - [Registered C# methods](#Registered-methods) feature *
 - Single line comments that start with `#` *

>\* *Not a part of the language per se, but a part of this implementation of language.*

JanC **keywords** translations/descriptions:

<table>
    <tr>
        <th>KEYWORD</th><th>TRANSLATION/DESCRIPTION</th>
    </tr>
    <tr>
        <td>Program</td><td>Entry point</td>
    </tr>
    <tr>
        <td>funkcija</td><td>function</td>
    </tr>
    <tr>
        <td>vrni</td><td>return</td>
    </tr>
    <tr>
        <td>prazno</td><td>void</td>
    </tr>
    <tr>
        <td>niz</td><td>string</td>
    </tr>
    <tr>
        <td>celo</td><td>int</td>
    </tr>
    <tr>
        <td>decimalno</td><td>double</td>
    </tr>
    <tr>
        <td>Izpiši</td><td>Implemented output method</td>
    </tr>
    <tr>
        <td>Vnos</td><td>Implemented input method</td>
    </tr>
    <tr>
        <td>če</td><td>if</td>
    </tr>
    <tr>
        <td>sicer</td><td>else</td>
    </tr>
    <tr>
        <td>dokler</td><td>while</td>
    </tr>
</table>

### EBNF
```ebnf
program
= { function } , "Program" , block ;

function
= "funkcija" , ("celo" | "decimalno" | "niz") , identifier , "(" , ( | (parameter , { "," , parameter }) ) , ")" , function-block
| "funkcija" , "prazno" , identifier , "(" , ( | (parameter , { "," , parameter }) ) , ")" , block ;

block
= "{" , { statement } , "}" ;

function-block
= "{" , { statement } , "vrni" , (literal | identifier) , "}" ;

parameter
= ("celo" | "decimalno" | "niz") , identifier ;

statement
= ("celo" | "decimalno" | "niz") , identifier , ["=" , ["+"|"-"] , expression] ";"
| identifier , "=" , ["+"|"-"] , expression , ";"
| "Izpiši" , "(" , (literal | identifier) , { "+" , (literal | identifier) } , ")" , ";"
| "Vnos" , "(" , identifier , ")" , ";"
| "če" , "(" , condition , ")" , block , ["sicer" , block]
| "dokler" , "(" , condition , ")" , block ;

(*
  If assigning a value to a string (niz) only first rule and a + sign can be used.
  This is a semantic limitation so it is not explicitly defined in EBNF.
*) expression
= (literal | identifier | function-call) , [("+"|"-"|"*"|"/") , expression]
| "(" , expression , ")" , [("+"|"-"|"*"|"/") , expression] ;

condition
= (literal | identifier) , ("=="|"!="|"<"|"<="|">"|">=") , (literal | identifier) ;

literal
= number | string ;

(*
  Identifier can only start with a letter or an underscore (_). Maximum of 199 characters can follow.
  Identifier cannot be the same as a key-word.
*) identifier
= ((letter | "_") , 199 * { (digit | letter | "_") }) - key-word ;

(*
  Function call and registered method call are syntactically equal so this definition is used for both of them.
*) function-call
= identifier , "(" , ( | ((literal | identifier) , { "," , (literal | identifier) }) ) , ")" ;

number
= "0" | natural-number | decimal-number ;

natural-number
= digit-excluding-zero , { digit } ;

(*
  Although JanC uses Slovene syntax that defines comma (,) as a decimal separator,
  the latter is a dot (.) due to easier implementation.
*) decimal-number
= natural-number , "." , digit , { digit } 
| "0" , "." , { digit } , natural-number ;

digit
= "0" | digit-excluding-zero ;

digit-excluding-zero
= "1"|"2"|"3"|"4"|"5"|"6"|"7"|"8"|"9" ;

string
= '"' , { character - '"' } , '"' ;

letter
= "A"|"B"|"C"|"Č"|"D"|"E"|"F"|"G"|"H"|"I"|"J"|"K"|"L"|"M"|"N"|"O"|"P"|"R"|"S"|"Š"|"T"|"U"|"V"|"Z"|"Ž"|"Q"|"W"|"X"|"Y"
 |"a"|"b"|"c"|"č"|"d"|"e"|"f"|"g"|"h"|"i"|"j"|"k"|"l"|"m"|"n"|"o"|"p"|"r"|"s"|"š"|"t"|"u"|"v"|"z"|"ž"|"q"|"w"|"x"|"y" ;

key-word
= "Program"|"funkcija"|"vrni"|"prazno"|"niz"|"celo"|"decimalno"|"Izpiši"|"Vnos"|"če"|"sicer"|"dokler" ;

character
= ? any visible and invisible character ? ;
```
 
## JanC implementation

This project is a JanC implementation in the form of an interpreter called JanCLang. It is developed in C# and targets *.NET Standard 2.0*.

Stages of interpreting:
 1. Lexical analysis
    - Outputs queue of tokens.
 2. Syntax analysis
    - Outputs abstract syntax tree.
 3. Semantic analysis
    - Checks abstract syntax tree for semantic errors.
 4. JanC program execution
    - Runs abstract syntax tree representation of JanC program.

JanCLang offers:
 - Executing a JanC source code.
 - Recognising lexical, syntax and semantic errors in JanC source code and throwing corresponding exceptions.
 - Static type checking and variable scope checking.
 - Registration of arbitrary C# [methods](#Registered-methods) into the JanC environment.
 - Overriding implemented output and input methods.
 
### Registered methods

**Registered methods** are a way to **enhance** JanC capabilities. You can define any C# methods with all of C# functionalities as long as it's parameters and return value are of type `int`, `double` and/or `string`. Pass the method as a delegate to register it into the JanC environment.

Registered methods can also be used as APIs between C# application and JanC program. You can send values from C# application to JanC program and vice versa.

Example shows two simple registered methods. `Pridobi()` gets a value from C# application and `Pošlji(int value)` sends a value to the application.

JanC source code:
```
Program
{
    celo a = Pridobi() + 1;
    Pošlji(a);
}
```
C# code:
```cs
string sourceCode = File.ReadAllText("aboveJanCCode.txt");

try
{
    int valueToGet = 5;
    int valueToSet = 0;

    Func<int> getValue = () =>
    {
        return valueToGet;
    };

    Action<int> setValue = (value) =>
    {
        valueToSet = value;
    };

    JanC program = new JanC();
    program.RegisterMethod("Pridobi", getValue);
    program.RegisterMethod("Pošlji", setValue);
    program.Run(sourceCode);

    Console.WriteLine("Value is set to: {0}", valueToSet);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```
Program outputs the following:
```
Value is set to: 6
```

## Instructions

Once JanCLang is accessible within your project ...

1. Store JanC source code in a `string`.
2. Initialize a `JanC` object.
   - Override implemented output method (optional).
   - Override implemented input method (optional).
   - Register arbitrary method (optional).
3. Call method `JanC.Run(string sourceCode)` to run the interpreter.

Here is an example of a JanC source code.
It defines two functions with `double` and `void` return respectively.
It uses a registered method `Zaokroži(d, decimals)`.
```
funkcija decimalno NaravnaPotenca(decimalno osnova, celo potenca)
{
    decimalno rezultat = 1;
    
    če(potenca > 0)
    {
        celo i = 0;
        dokler(i < potenca)
        {
            rezultat = rezultat * osnova;
            i = i + 1;
        }
    }
    
    vrni rezultat;
}

funkcija prazno NarišiČrto()
{
    Izpiši("---------------------------------");
}

Program
{
    NarišiČrto();
    
    decimalno d = NaravnaPotenca(2.3, 4);
    Izpiši("d pred zaokroževanjem = " + d);
    
    d = Zaokroži(d, 4);
    Izpiši("d po zaokroževanju = " + d);
    
    NarišiČrto();
}
```
Following is a C# code that defines and registers `Zaokroži(d, decimals)` method (`round` anonymous function), overrides output `Izpiši()` method and runs the above JanC program.
```cs
string sourceCode = File.ReadAllText("aboveJanCCode.txt");

try
{
    Func<double, int, double> round = (d, decimals) =>
    {
        return Math.Round(d, decimals);
    };
    
    JanC program = new JanC();
    program.RegisterWrite(toWrite => { Console.WriteLine(toWrite) });
    program.RegisterMethod("Zaokroži", round);
    program.Run(sourceCode);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```
Program outputs the following:
```
---------------------------------
d pred zaokroževanjem = 27.98409999999999
d po zaokroževanju = 27.9841
---------------------------------
```

## Inspiration

[JanC language](#JanC-language-specification) and this [interpreter](#JanC-implementation) were developed for a diploma thesis by [Jan Čotar](https://github.com/JanCotar) under the mentorship of [Jan Robas](https://github.com/janrobas).
Both language and it's interpreter were developed simultaneously. The whole of the interpreter (lexical analyzer, syntax analyzer, semantic analyzer and interpreter) were written by hand.

JanC syntax was inspired by C#.

JanC logo ![](https://github.com/JanCotar/JanC_dll/blob/main/JanC_icon_small2.svg) was designed and created by [Jan Čotar](https://github.com/JanCotar).
