# JSON Playground

Current results:

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.778 (1903/May2019Update/19H1)
Intel Core i5-6600K CPU 3.50GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT


|         Method |      Mean |     Error |    StdDev |    Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------- |----------:|----------:|----------:|----------:|-------:|------:|------:|----------:|
| NewtonsoftJson | 674.80 ns | 13.344 ns | 22.295 ns | 664.58 ns | 1.7872 |     - |     - |    5608 B |
| SystemTextJson |  53.77 ns |  0.443 ns |  0.393 ns |  53.61 ns | 0.0178 |     - |     - |      56 B |
```