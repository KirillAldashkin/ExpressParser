# Express~~ion~~Parser
## About
Simple mathematical expression parser capable of generating IL code to speed up calculations.
```CSharp
using ExpressParser;

Expression expr = new("(1+1/a)^pow");
expr.SetArgument("a", 2);
expr.SetArgument("pow", 4);
double val1 = expr.Evaluate();
expr.Compile();
double val2 = expr.EvaluateIL();
```
## Projects
* **ConsoleExample** - simple CLI program to create and evaluate expressions
* **ExpressParser** - the library itself
* **ExpressCodeGen** - source generator for **ExpressParser**
## Licensing
`Express Analyzer` is distributed under the [MIT License](https://github.com/KirillAldashkin/ExpressParser/blob/main/LICENSE.md).