// Conformance runner (Go). Evaluates the shared corpus and writes results/go.json.
package main

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"

	switchboard "github.com/switchboard-io/switchboard-go"
)

func findRoot() string {
	dir, _ := os.Getwd()
	for {
		if _, err := os.Stat(filepath.Join(dir, "corpus.json")); err == nil {
			return dir
		}
		parent := filepath.Dir(dir)
		if parent == dir {
			panic("corpus.json not found")
		}
		dir = parent
	}
}

func main() {
	root := findRoot()
	raw, err := os.ReadFile(filepath.Join(root, "corpus.json"))
	if err != nil {
		panic(err)
	}
	var corpus map[string]interface{}
	if err := json.Unmarshal(raw, &corpus); err != nil {
		panic(err)
	}

	flags := corpus["flags"].([]interface{})
	snapshot, _ := json.Marshal(map[string]interface{}{"flags": flags})
	client := switchboard.FromJSON(string(snapshot))

	var cases []map[string]interface{}
	for _, c := range corpus["cases"].([]interface{}) {
		cm := c.(map[string]interface{})
		ctx := cm["context"].(map[string]interface{})
		r := client.EvaluateDetail(cm["flag"].(string), ctx)
		cases = append(cases, map[string]interface{}{
			"id":             cm["id"],
			"variationIndex": r.VariationIndex,
			"reason":         r.Reason,
			"value":          r.Value,
		})
	}

	var buckets []map[string]interface{}
	for _, b := range corpus["buckets"].([]interface{}) {
		bm := b.(map[string]interface{})
		fk := bm["flagKey"].(string)
		salt := bm["salt"].(string)
		ck := bm["contextKey"].(string)
		buckets = append(buckets, map[string]interface{}{
			"id":     fmt.Sprintf("%s.%s.%s", fk, salt, ck),
			"actual": switchboard.BucketOf(fk, salt, ck),
		})
	}

	resultsDir := filepath.Join(root, "results")
	_ = os.MkdirAll(resultsDir, 0o755)
	out, _ := json.MarshalIndent(map[string]interface{}{"lang": "go", "cases": cases, "buckets": buckets}, "", "  ")
	_ = os.WriteFile(filepath.Join(resultsDir, "go.json"), out, 0o644)
	fmt.Printf("go runner: %d cases, %d buckets\n", len(cases), len(buckets))
}
