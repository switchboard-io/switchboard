// Benchmark: evaluations/sec for switchboard-go. Run: go run ./cmd/bench
package main

import (
	"encoding/json"
	"fmt"
	"time"

	switchboard "github.com/switchboard-io/switchboard-go"
)

func main() {
	client := switchboard.FromJSON(`{ "flags": [{
      "key":"checkout-v2", "enabled":true, "variations":["off","on"], "offVariation":0,
      "targets":[{"values":["vip"],"variation":1}],
      "rules":[{"clauses":[{"attribute":"country","op":"in","values":["US","CA","GB"]}],"variation":1}],
      "fallthrough":{"rollout":[{"variation":0,"weight":75000},{"variation":1,"weight":25000}]},
      "salt":"abc" }] }`)

	ctx := func(i int) map[string]interface{} {
		country := "IN"
		if i%2 == 0 {
			country = "US"
		}
		var m map[string]interface{}
		_ = json.Unmarshal([]byte(fmt.Sprintf(`{"key":"user-%d","attributes":{"country":"%s"}}`, i, country)), &m)
		return m
	}

	for i := 0; i < 100000; i++ {
		client.EvaluateDetail("checkout-v2", ctx(i))
	}

	const n = 2000000
	start := time.Now()
	sink := 0
	for i := 0; i < n; i++ {
		sink += client.EvaluateDetail("checkout-v2", ctx(i)).VariationIndex
	}
	elapsed := time.Since(start)

	fmt.Println("Switchboard SDK benchmark (Go)")
	fmt.Printf("  evaluations : %d\n", n)
	fmt.Printf("  total time  : %.1f ms\n", float64(elapsed.Nanoseconds())/1e6)
	fmt.Printf("  per eval    : %.1f ns\n", float64(elapsed.Nanoseconds())/float64(n))
	fmt.Printf("  throughput  : %.0f evals/sec\n", float64(n)/elapsed.Seconds())
	fmt.Printf("  (sink=%d)\n", sink)
}
