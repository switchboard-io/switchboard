# frozen_string_literal: true

require "digest"
require "json"

# switchboard-sdk — local-evaluation feature-flag engine for Ruby.
# Implements docs/SPEC.md identically to the other language SDKs.
module Switchboard
  module Sdk
    VERSION = "0.0.1"
    MAX = 0xFFFFFFFFFFFFF # 2**52 - 1

    module_function

    # Deterministic rollout bucket in [0, 1) — identical across all SDKs (SPEC §4).
    def bucket_of(flag_key, salt, context_key)
      input = "#{flag_key}.#{salt}.#{context_key}"
      hex = Digest::SHA1.hexdigest(input)
      n = hex[0, 13].to_i(16)
      n.to_f / MAX
    end

    def as_number(v)
      return nil if v == true || v == false
      return v.to_f if v.is_a?(Numeric)
      nil
    end

    def as_string(v)
      return "true" if v == true
      return "false" if v == false
      return v if v.is_a?(String)
      if v.is_a?(Numeric)
        return v.to_i.to_s if v == v.to_i
        return v.to_s
      end
      nil
    end

    def scalar_equals(a, b)
      na = as_number(a)
      nb = as_number(b)
      return na == nb if na && nb
      return a == b if [true, false].include?(a) && [true, false].include?(b)
      as_string(a) == as_string(b)
    end

    def match_op(op, attr, values)
      return false if attr.nil?

      case op
      when "in"
        values.any? { |v| scalar_equals(attr, v) }
      when "contains", "startsWith", "endsWith"
        s = as_string(attr)
        return false if s.nil?
        values.any? do |v|
          t = as_string(v)
          next false if t.nil?
          case op
          when "contains" then s.include?(t)
          when "startsWith" then s.start_with?(t)
          when "endsWith" then s.end_with?(t)
          end
        end
      when "greaterThan", "lessThan"
        a = as_number(attr)
        b = values.empty? ? nil : as_number(values[0])
        return false if a.nil? || b.nil?
        op == "greaterThan" ? a > b : a < b
      when "regexMatch"
        s = as_string(attr)
        p = values.empty? ? nil : as_string(values[0])
        return false if s.nil? || p.nil?
        !Regexp.new(p).match(s).nil?
      else
        false
      end
    end

    def match_clause(clause, ctx)
      attribute = clause["attribute"] || ""
      attr = if attribute == "key"
               ctx["key"]
             else
               (ctx["attributes"] || {})[attribute]
             end
      r = match_op(clause["op"] || "", attr, clause["values"] || [])
      clause["negate"] ? !r : r
    end

    def resolve_index(flag, variation, rollout, ctx)
      return variation unless variation.nil?

      if rollout && !rollout.empty?
        bucket = bucket_of(flag["key"], flag["salt"] || "", ctx["key"])
        cumulative = 0
        rollout.each do |wv|
          cumulative += wv["weight"]
          return wv["variation"] if bucket * 100_000 < cumulative
        end
        return rollout.last["variation"]
      end
      nil
    end

    def result(flag, index, reason)
      variations = flag["variations"] || []
      return { "value" => nil, "variationIndex" => -1, "reason" => "ERROR" } if index.nil? || index.negative? || index >= variations.length

      { "value" => variations[index], "variationIndex" => index, "reason" => reason }
    end

    # Evaluate a flag for a context. `resolver` is a lambda returning prerequisite flags by key.
    def evaluate(flag, ctx, resolver = nil)
      off = flag["offVariation"] || 0

      return result(flag, off, "OFF") unless flag["enabled"]

      (flag["prerequisites"] || []).each do |p|
        pre = resolver&.call(p["key"])
        return result(flag, off, "PREREQUISITE_FAILED") if pre.nil? || evaluate(pre, ctx, resolver)["variationIndex"] != p["variation"]
      end

      (flag["targets"] || []).each do |t|
        return result(flag, t["variation"], "TARGET_MATCH") if (t["values"] || []).include?(ctx["key"])
      end

      (flag["rules"] || []).each do |rule|
        if (rule["clauses"] || []).all? { |c| match_clause(c, ctx) }
          return result(flag, resolve_index(flag, rule["variation"], rule["rollout"], ctx), "RULE_MATCH")
        end
      end

      ft = flag["fallthrough"] || {}
      result(flag, resolve_index(flag, ft["variation"], ft["rollout"], ctx), "FALLTHROUGH")
    rescue StandardError
      { "value" => nil, "variationIndex" => -1, "reason" => "ERROR" }
    end

    # A client over a snapshot: { "flags" => [ ...FlagConfig ] }.
    class Client
      def initialize(flags)
        @flags = {}
        flags.each { |f| @flags[f["key"]] = f }
      end

      def self.from_json(data)
        parsed = data.is_a?(String) ? JSON.parse(data) : data
        new(parsed["flags"] || [])
      end

      def evaluate_detail(key, ctx)
        flag = @flags[key]
        return { "value" => nil, "variationIndex" => -1, "reason" => "ERROR" } if flag.nil?

        Sdk.evaluate(flag, ctx, ->(k) { @flags[k] })
      end
    end

    def about
      "switchboard-sdk — local-evaluation feature flags for Ruby. See https://switchboard.co"
    end
  end
end
