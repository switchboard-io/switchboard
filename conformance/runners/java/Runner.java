import io.github.switchboard.sdk.Switchboard;
import io.github.switchboard.sdk.MiniJson;

import java.io.File;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/** Conformance runner (Java). Evaluates the shared corpus and writes results/java.json. */
public class Runner {

    @SuppressWarnings("unchecked")
    public static void main(String[] args) throws Exception {
        Path root = findRoot();
        String raw = Files.readString(root.resolve("corpus.json"));
        Map<String, Object> corpus = (Map<String, Object>) MiniJson.parse(raw);

        List<Object> flags = (List<Object>) corpus.get("flags");
        // Rebuild a snapshot JSON for the client (re-serialize via a simple writer).
        Switchboard.Client client = new Switchboard.Client(flags);

        StringBuilder sb = new StringBuilder();
        sb.append("{\n  \"lang\": \"java\",\n  \"cases\": [\n");
        List<Object> cases = (List<Object>) corpus.get("cases");
        List<String> caseLines = new ArrayList<>();
        for (Object c : cases) {
            Map<String, Object> cm = (Map<String, Object>) c;
            Map<String, Object> ctx = (Map<String, Object>) cm.get("context");
            Switchboard.Result r = client.evaluateDetail((String) cm.get("flag"), ctx);
            caseLines.add(String.format("    { \"id\": %s, \"variationIndex\": %d, \"reason\": %s, \"value\": %s }",
                    jsonStr((String) cm.get("id")), r.variationIndex, jsonStr(r.reason), jsonVal(r.value)));
        }
        sb.append(String.join(",\n", caseLines));
        sb.append("\n  ],\n  \"buckets\": [\n");

        List<Object> buckets = (List<Object>) corpus.get("buckets");
        List<String> bucketLines = new ArrayList<>();
        for (Object b : buckets) {
            Map<String, Object> bm = (Map<String, Object>) b;
            String fk = (String) bm.get("flagKey"), salt = (String) bm.get("salt"), ck = (String) bm.get("contextKey");
            double actual = Switchboard.bucketOf(fk, salt, ck);
            bucketLines.add(String.format("    { \"id\": %s, \"actual\": %s }",
                    jsonStr(fk + "." + salt + "." + ck), Double.toString(actual)));
        }
        sb.append(String.join(",\n", bucketLines));
        sb.append("\n  ]\n}\n");

        Path resultsDir = root.resolve("results");
        Files.createDirectories(resultsDir);
        Files.write(resultsDir.resolve("java.json"), sb.toString().getBytes(StandardCharsets.UTF_8));
        System.out.printf("java runner: %d cases, %d buckets%n", cases.size(), buckets.size());
    }

    static Path findRoot() {
        File dir = new File(System.getProperty("user.dir"));
        while (dir != null) {
            if (new File(dir, "corpus.json").exists()) return dir.toPath();
            dir = dir.getParentFile();
        }
        throw new RuntimeException("corpus.json not found");
    }

    static String jsonStr(String s) {
        return "\"" + s.replace("\\", "\\\\").replace("\"", "\\\"") + "\"";
    }

    static String jsonVal(Object v) {
        if (v == null) return "null";
        if (v instanceof String) return jsonStr((String) v);
        if (v instanceof Boolean) return v.toString();
        if (v instanceof Double) {
            double d = (Double) v;
            if (d == Math.floor(d) && !Double.isInfinite(d)) return Long.toString((long) d);
            return Double.toString(d);
        }
        return jsonStr(v.toString());
    }
}
