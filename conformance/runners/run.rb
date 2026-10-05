#!/usr/bin/env ruby
# frozen_string_literal: true

# Conformance runner (Ruby). Evaluates the shared corpus and writes results/ruby.json.
require "json"

here = File.expand_path(__dir__)
root = File.expand_path("..", here) # conformance/
$LOAD_PATH.unshift(File.expand_path("../packages/ruby/lib", root))
require "switchboard/sdk"

corpus = JSON.parse(File.read(File.join(root, "corpus.json")))
client = Switchboard::Sdk::Client.from_json({ "flags" => corpus["flags"] })

cases = corpus["cases"].map do |c|
  r = client.evaluate_detail(c["flag"], c["context"])
  { "id" => c["id"], "variationIndex" => r["variationIndex"], "reason" => r["reason"], "value" => r["value"] }
end

buckets = corpus["buckets"].map do |b|
  { "id" => "#{b['flagKey']}.#{b['salt']}.#{b['contextKey']}",
    "actual" => Switchboard::Sdk.bucket_of(b["flagKey"], b["salt"], b["contextKey"]) }
end

results_dir = File.join(root, "results")
Dir.mkdir(results_dir) unless Dir.exist?(results_dir)
File.write(File.join(results_dir, "ruby.json"),
           JSON.pretty_generate({ "lang" => "ruby", "cases" => cases, "buckets" => buckets }))
puts "ruby runner: #{cases.length} cases, #{buckets.length} buckets"
