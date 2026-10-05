using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Switchboard.Evaluation;

var flag = FlagConfig.Parse("""
{ "key":"checkout-v2", "enabled":true, "variations":["off","on"], "offVariation":0,
  "targets":[{"values":["vip"],"variation":1}],
  "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US","CA","GB"]}],"variation":1}],
  "fallthrough":{"rollout":[{"variation":0,"weight":75000},{"variation":1,"weight":25000}]},
  "salt":"abc" }
""");

EvalContext Ctx(int i) => new EvalContext($"user-{i}").Set("country", JsonValue.Create(i % 2 == 0 ? "US" : "IN"));

// Warmup
for (int i = 0; i < 100_000; i++) Evaluator.Evaluate(flag, Ctx(i));

const int N = 2_000_000;
var sw = Stopwatch.StartNew();
long sink = 0;
for (int i = 0; i < N; i++) sink += Evaluator.Evaluate(flag, Ctx(i)).VariationIndex;
sw.Stop();

var perOp = sw.Elapsed.TotalNanoseconds / N;
var opsPerSec = N / sw.Elapsed.TotalSeconds;
Console.WriteLine($"Switchboard.Evaluation benchmark (.NET)");
Console.WriteLine($"  evaluations : {N:N0}");
Console.WriteLine($"  total time  : {sw.Elapsed.TotalMilliseconds:N1} ms");
Console.WriteLine($"  per eval    : {perOp:N1} ns");
Console.WriteLine($"  throughput  : {opsPerSec:N0} evals/sec");
Console.WriteLine($"  (sink={sink})");