package io.github.switchboard.sdk;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.List;
import java.util.Map;

/**
 * The local-evaluation feature-flag engine for Java. Implements docs/SPEC.md
 * identically to the other language SDKs. Operates on parsed JSON structures
 * (Map/List/String/Double/Boolean), as produced by {@link MiniJson}.
 */
public final class Switchboard {

    private static final double MAX = 0xFFFFFFFFFFFFFL; // 2^52 - 1

    private Switchboard() {}

    /** Deterministic rollout bucket in [0, 1) — identical across all SDKs (SPEC §4). */
    public static double bucketOf(String flagKey, String salt, String contextKey) {
        String input = flagKey + "." + salt + "." + contextKey;
        try {
            MessageDigest md = MessageDigest.getInstance("SHA-1");
            byte[] digest = md.digest(input.getBytes(StandardCharsets.UTF_8));
            StringBuilder hex = new StringBuilder(digest.length * 2);
            for (byte b : digest) hex.append(String.format("%02x", b));
            long n = Long.parseLong(hex.substring(0, 13), 16);
            return n / MAX;
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }

    /** Result of an evaluation. */
    public static final class Result {
        public final Object value;
        public final int variationIndex;
        public final String reason;
        Result(Object value, int variationIndex, String reason) {
            this.value = value; this.variationIndex = variationIndex; this.reason = reason;
        }
    }

    /** Resolves a prerequisite flag by key, or null. */
    public interface Resolver { Map<String, Object> resolve(String key); }

    private static Result err() { return new Result(null, -1, "ERROR"); }

    @SuppressWarnings("unchecked")
    private static Map<String, Object> asMap(Object o) { return o instanceof Map ? (Map<String, Object>) o : null; }

    @SuppressWarnings("unchecked")
    private static List<Object> asList(Object o) { return o instanceof List ? (List<Object>) o : null; }

    private static Double asNumber(Object v) {
        if (v instanceof Double) return (Double) v;
        if (v instanceof Integer) return ((Integer) v).doubleValue();
        if (v instanceof Long) return ((Long) v).doubleValue();
        return null;
    }

    private static String asString(Object v) {
        if (v instanceof String) return (String) v;
        if (v instanceof Boolean) return ((Boolean) v) ? "true" : "false";
        if (v instanceof Double) {
            double d = (Double) v;
            if (d == Math.floor(d) && !Double.isInfinite(d)) return Long.toString((long) d);
            return Double.toString(d);
        }
        return null;
    }

    private static boolean scalarEquals(Object a, Object b) {
        Double na = asNumber(a), nb = asNumber(b);
        if (na != null && nb != null) return na.doubleValue() == nb.doubleValue();
        if (a instanceof Boolean && b instanceof Boolean) return a.equals(b);
        String sa = asString(a), sb = asString(b);
        return sa != null && sa.equals(sb);
    }

    private static int intOf(Object v, int def) {
        Double d = asNumber(v);
        return d != null ? (int) d.doubleValue() : def;
    }

    private static boolean matchOp(String op, Object attr, List<Object> values) {
        if (attr == null) return false;
        switch (op) {
            case "in":
                for (Object v : values) if (scalarEquals(attr, v)) return true;
                return false;
            case "contains":
            case "startsWith":
            case "endsWith": {
                String s = asString(attr);
                if (s == null) return false;
                for (Object v : values) {
                    String t = asString(v);
                    if (t == null) continue;
                    if (op.equals("contains") && s.contains(t)) return true;
                    if (op.equals("startsWith") && s.startsWith(t)) return true;
                    if (op.equals("endsWith") && s.endsWith(t)) return true;
                }
                return false;
            }
            case "greaterThan":
            case "lessThan": {
                Double a = asNumber(attr);
                Double b = values.isEmpty() ? null : asNumber(values.get(0));
                if (a == null || b == null) return false;
                return op.equals("greaterThan") ? a > b : a < b;
            }
            case "regexMatch": {
                String s = asString(attr);
                String p = values.isEmpty() ? null : asString(values.get(0));
                if (s == null || p == null) return false;
                return java.util.regex.Pattern.compile(p).matcher(s).find();
            }
            default:
                return false;
        }
    }

    private static boolean matchClause(Map<String, Object> clause, Map<String, Object> ctx) {
        String attribute = (String) clause.getOrDefault("attribute", "");
        Object attr;
        if ("key".equals(attribute)) {
            attr = ctx.get("key");
        } else {
            Map<String, Object> attrs = asMap(ctx.get("attributes"));
            attr = attrs == null ? null : attrs.get(attribute);
        }
        List<Object> values = asList(clause.get("values"));
        if (values == null) values = java.util.Collections.emptyList();
        boolean r = matchOp((String) clause.getOrDefault("op", ""), attr, values);
        Object neg = clause.get("negate");
        return Boolean.TRUE.equals(neg) ? !r : r;
    }

    private static Integer resolveIndex(Map<String, Object> flag, Object variation, List<Object> rollout, Map<String, Object> ctx) {
        if (variation != null) return intOf(variation, -1);
        if (rollout != null && !rollout.isEmpty()) {
            String flagKey = (String) flag.getOrDefault("key", "");
            String salt = (String) flag.getOrDefault("salt", "");
            String ctxKey = (String) ctx.getOrDefault("key", "");
            double bucket = bucketOf(flagKey, salt, ctxKey);
            double cumulative = 0;
            for (Object item : rollout) {
                Map<String, Object> wv = asMap(item);
                Double w = asNumber(wv.get("weight"));
                cumulative += (w == null ? 0 : w);
                if (bucket * 100000 < cumulative) return intOf(wv.get("variation"), -1);
            }
            Map<String, Object> last = asMap(rollout.get(rollout.size() - 1));
            return intOf(last.get("variation"), -1);
        }
        return null;
    }

    private static Result result(Map<String, Object> flag, Integer index, String reason) {
        if (index == null) return err();
        List<Object> variations = asList(flag.get("variations"));
        if (variations == null || index < 0 || index >= variations.size()) return err();
        return new Result(variations.get(index), index, reason);
    }

    /** Evaluate a flag for a context. {@code resolver} returns prerequisite flags by key. */
    public static Result evaluate(Map<String, Object> flag, Map<String, Object> ctx, Resolver resolver) {
        try {
            int off = intOf(flag.get("offVariation"), 0);

            if (!Boolean.TRUE.equals(flag.get("enabled")))
                return result(flag, off, "OFF");

            List<Object> prereqs = asList(flag.get("prerequisites"));
            if (prereqs != null) {
                for (Object p : prereqs) {
                    Map<String, Object> pm = asMap(p);
                    String key = (String) pm.get("key");
                    Map<String, Object> pre = resolver != null ? resolver.resolve(key) : null;
                    int want = intOf(pm.get("variation"), -1);
                    if (pre == null || evaluate(pre, ctx, resolver).variationIndex != want)
                        return result(flag, off, "PREREQUISITE_FAILED");
                }
            }

            String ctxKey = (String) ctx.getOrDefault("key", "");
            List<Object> targets = asList(flag.get("targets"));
            if (targets != null) {
                for (Object t : targets) {
                    Map<String, Object> tm = asMap(t);
                    List<Object> vals = asList(tm.get("values"));
                    if (vals != null && vals.contains(ctxKey))
                        return result(flag, intOf(tm.get("variation"), -1), "TARGET_MATCH");
                }
            }

            List<Object> rules = asList(flag.get("rules"));
            if (rules != null) {
                for (Object r : rules) {
                    Map<String, Object> rule = asMap(r);
                    List<Object> clauses = asList(rule.get("clauses"));
                    boolean all = true;
                    if (clauses != null) {
                        for (Object c : clauses) {
                            if (!matchClause(asMap(c), ctx)) { all = false; break; }
                        }
                    }
                    if (all) {
                        Integer idx = resolveIndex(flag, rule.get("variation"), asList(rule.get("rollout")), ctx);
                        return result(flag, idx, "RULE_MATCH");
                    }
                }
            }

            Map<String, Object> ft = asMap(flag.get("fallthrough"));
            if (ft == null) ft = java.util.Collections.emptyMap();
            Integer idx = resolveIndex(flag, ft.get("variation"), asList(ft.get("rollout")), ctx);
            return result(flag, idx, "FALLTHROUGH");
        } catch (Exception e) {
            return err();
        }
    }

    /** A client over a snapshot: {@code { "flags": [ ...FlagConfig ] }}. */
    public static final class Client {
        private final Map<String, Map<String, Object>> flags = new java.util.HashMap<>();

        public Client(List<Object> flagList) {
            for (Object f : flagList) {
                Map<String, Object> fm = asMap(f);
                flags.put((String) fm.get("key"), fm);
            }
        }

        @SuppressWarnings("unchecked")
        public static Client fromJson(String json) {
            Map<String, Object> root = (Map<String, Object>) MiniJson.parse(json);
            List<Object> list = asList(root.get("flags"));
            return new Client(list == null ? java.util.Collections.emptyList() : list);
        }

        public Result evaluateDetail(String key, Map<String, Object> ctx) {
            Map<String, Object> flag = flags.get(key);
            if (flag == null) return err();
            return evaluate(flag, ctx, flags::get);
        }
    }

    public static String about() {
        return "switchboard-sdk — local-evaluation feature flags for Java. See https://switchboard.co";
    }
}
