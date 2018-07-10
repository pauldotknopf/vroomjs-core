# VroomJs [![NuGet](https://img.shields.io/nuget/v/VroomJs.svg?maxAge=2592000)](https://www.nuget.org/packages/VroomJs/)

This is a version of VroomJs that can run on on the new .NET Core runtime. 

## Examples

Execute some Javascript:

```c#
using (var engine = new JsEngine())
{
    using (var context = engine.CreateContext())
    {
        var x = (double)context.Execute("3.14159+2.71828");
        Console.WriteLine(x);  // prints 5.85987
    }
}
```

Create and return a Javascript object, then call a method on it:

```c#
using (JsEngine js = new JsEngine(4, 32))
{
    using (JsContext context = js.CreateContext())
    {
        // Create a global variable on the JS side.
        context.Execute("var x = {'answer':42, 'tellme':function (x) { return x+' '+this.answer; }}");
        // Get it and use "dynamic" to tell the compiler to use runtime binding.
        dynamic x = context.GetVariable("x");
        // Call the method and print the result. This will print:
        // "What is the answer to ...? 42"
        Console.WriteLine(x.tellme("What is the answer to ...?"));
    }
}
```

Access properties and call methods on CLR objects from Javascript:

```c#
class Test
{
    public int Value { get; set; }
    public void PrintValue(string msg)
    {
        Console.WriteLine(msg+" "+Value);
    }
}

using (JsEngine js = new JsEngine(4, 32))
{
    using (JsContext context = js.CreateContext())
    {
        context.SetVariable("m", new Test());
        // Sets the property from Javascript.
        context.Execute("m.Value = 42");
        // Call a method on the CLR object from Javascript. This prints:
        // "And the answer is (again!): 42"
        context.Execute("m.PrintValue('And the answer is (again!):')");
    }
}
```
## Platforms

### Windows

There are embedded .dlls (x64/x86) in the project that can be loaded dynamically.

```c#
VroomJs.AssemblyLoader.EnsureLoaded(); // windows only
```

Call this method on start of your application.

### Mac/Linux

The native libraries used in this project must be manually built for Mac/Linux. We are using a forked version of VroomJs used by [ReactJS.NET](http://reactjs.net/). The instructions to generate a native assembly are the same (found [here](http://reactjs.net/guides/mono.html)).

```bash
# Get a supported version of V8
cd /usr/local/src/
git clone https://github.com/v8/v8.git v8-3.17
cd v8-3.17
git checkout tags/3.17.16.2

# Build V8
make dependencies
make native werror=no library=shared soname_version=3.17.16.2 -j4
cp out/native/lib.target/libv8.so.3.17.16.2 /usr/local/lib/

# Get VroomJs's version of libvroomjs
cd /usr/local/src/
git clone https://github.com/pauldotknopf/vroomjs-core.git
cd vroomjs-core
cd native/libVroomJs/

# Build libvroomjs
g++ jscontext.cpp jsengine.cpp managedref.cpp bridge.cpp jsscript.cpp -o libVroomJsNative.so -shared -L /usr/local/src/v8-3.17/out/native/lib.target/ -I /usr/local/src/v8-3.17/include/ -fPIC -Wl,--no-as-needed -l:/usr/local/lib/libv8.so.3.17.16.2
cp libVroomJsNative.so /usr/local/lib/
ldconfig
```
