### This is an extension for SimpleAuth.Client.AspNetCore
#### which helps exporting permissions from assembly
by scanning classes and method within provided assemblies

then collecting information from SaModule and SaPermission attributes

```
// Init scanner
var scanner = new PermissionScanner<SaModuleAttribute, SaPermissionAttribute>();

// Several ways to select assemblies to be exported
scanner.AddAssembly(typeof(Program).Assembly); // add specific assembly
scanner.AddAssembly<Program>(); // add assembly which contains target class
scanner.AddAssembly(typeof(Program)); // add assembly which contains target type

// Do scan
scanner.Scan(); // returns collection of PermissionInfo models (contains Module, SubModules and Verb)
scanner.ScanToFile(optional file name); // direct printing to a file
```

Sample result exported from Test/WebApiPlayground

> weatherforecast.a	View
>
> weatherforecast.b	View
>
> best			    View
>
> best.a			View
>
> best.b			View
>
> best.b			Edit

