using ExpressParser;

//just some testing utilite
//TODO: simple CLI program to create and evaluate expressions
Expression e = new("(((a+b)*(a+b))+((a+b)*(a+b)))*(((a+b)*(a+b))+((a+b)*(a+b)))");
e.SetArgument("a", 1.1);
e.SetArgument("b", 1.1);
double ddd = 1;
for(int i = 0; i < 1000000; i++) //avg time on my PC: 3300ms
{ 
    double v = e.Evaluate();
}
e.Compile(); //avg time on my PC: <50ms
ddd = 1; 
for (int i = 0; i < 1000000; i++) //avg time on my PC: 400ms
{
    double v = e.EvaluateIL();
}
ddd = 1;