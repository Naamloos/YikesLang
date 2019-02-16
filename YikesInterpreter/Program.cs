using System;

namespace YikesInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            var code =
"^entry:welcome. @method:welcome[@print:\"this is printed from within welcome\". @test.]. @test[@print: \"this is printed from within test\". @welcome.]. ";


            var intr = new Interpreter();

            intr.CompileScript(new StringStream(code));

            intr.ExecuteScript();

            Console.ReadKey();
        }
    }
}
