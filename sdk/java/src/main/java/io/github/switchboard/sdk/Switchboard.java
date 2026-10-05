package io.github.switchboard.sdk;

/**
 * Placeholder entry point for the Switchboard Java SDK.
 *
 * <p>This 0.0.1 release reserves the Maven coordinates
 * {@code io.github.switchboard-io:sdk} while the full local-evaluation,
 * streaming, OpenFeature-compatible client is built.
 *
 * <p>Docs: https://switchboard.co — Source: https://github.com/switchboard-io/switchboard
 */
public final class Switchboard {

    /** Product name. */
    public static final String NAME = "Switchboard";

    /** Marks this as a name-reservation build. */
    public static final boolean IS_PLACEHOLDER = true;

    private Switchboard() {
    }

    /** Returns a short description of the SDK. */
    public static String about() {
        return "switchboard sdk (placeholder) — local-evaluation feature flags for Java. See https://switchboard.co";
    }
}
