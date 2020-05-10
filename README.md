# JSON Playground

Current results:

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.778 (1903/May2019Update/19H1)
Intel Core i5-6600K CPU 3.50GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT


|         Method |       Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------- |-----------:|---------:|---------:|-------:|------:|------:|----------:|
| NewtonsoftJson |   647.7 ns |  4.58 ns |  3.83 ns | 1.7872 |     - |     - |    5608 B |
| SystemTextJson | 4,543.8 ns | 26.06 ns | 24.38 ns | 0.2899 |     - |     - |     912 B |
```