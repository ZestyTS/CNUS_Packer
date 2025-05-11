# CNUS_Packer (.NET Standard Fork)

> A modern, dependency-free .NET Standard 2.0 library for packing Wii U content, rebuilt from the ground up to support cross-platform development, modular code, and async workflows.

## 🔄 Why This Fork?

This is a major architectural rewrite of the original [CNUS_Packer](https://github.com/Morilli/NUSPacker) project. The original port was a standalone `x86`-targeted `.exe` with static state, limited testability, and synchronous logic. This fork:

- Converts the application into a **.NET Standard 2.0 DLL**  
- Enables use in other tools like GUI frontends or CLI apps  
- Removes all static global state  
- Implements proper **dependency injection** with `ILogger`  
- Applies **async/await** to I/O-heavy operations for performance and stability  
- Refactors settings into injectable configuration classes  
- Adds logging and diagnostic visibility through structured logs  

---

## 🧱 Architectural Improvements

| Feature                     | Original Port          | This Fork                    |
|----------------------------|------------------------|------------------------------|
| Build Target               | `.exe (x86)`           | `.dll (.NET Standard 2.0)`   |
| Static Globals             | Yes                    | No                           |
| Async/Await                | No                     | Yes (I/O, encryption, hashing) |
| Dependency Injection       | None                   | `ILogger<T>`, `ILoggerFactory` |
| Settings Handling          | Static class           | `Settings.Default` config object |
| Cross-platform             | No (Windows-only EXE)  | Yes (any .NET runtime)       |

---

## 🛠 Usage

You can now reference this DLL and call it from your own application:

```csharp
var runner = new CnusPackagerRunner(logger);
await runner.RunAsync(new CnusPackagerOptions
{
    InputPath = @"C:\path\to\game",
    OutputPath = @"C:\path\to\output",
    EncryptionKey = "13371337133713371337133713371337",
    EncryptKeyWith = "00000000000000000000000000000000",
    SkipXmlParsing = false
});
