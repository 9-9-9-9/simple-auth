### This project helps exporting permissions from assembly
by scanning classes and method within provided assemblies
then collecting information from SaModule and SaPermission attributes

```
var scanner = new PermissionScanner<SaModuleAttribute, SaPermissionAttribute>();
scanner
	.AddAssembly(typeof(Program).Assembly)
	.ScanToFile();
```

Sample result exported from Test/WebApiPlayground
```
weatherforecast.a	View
weatherforecast.b	View
best			View
best.a			View
best.b			View
best.b			Edit
```
