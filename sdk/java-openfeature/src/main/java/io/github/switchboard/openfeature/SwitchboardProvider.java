package io.github.switchboard.openfeature;

/**
 * Placeholder OpenFeature provider for Switchboard (Java).
 *
 * <p>Reserves the Maven coordinates {@code io.github.switchboard-io:openfeature-provider}
 * while the full provider (implementing OpenFeature's FeatureProvider) is built.
 *
 * <p>Docs: https://switchboard.co — Source: https://github.com/switchboard-io/switchboard
 */
public final class SwitchboardProvider {

    /** Provider metadata name reported to OpenFeature. */
    public static final String NAME = "Switchboard";

    /** Marks this as a name-reservation build. */
    public static final boolean IS_PLACEHOLDER = true;

    private SwitchboardProvider() {
    }

    /** Returns a short description of the provider. */
    public static String about() {
        return "switchboard openfeature-provider (placeholder) — OpenFeature provider for Java. See https://switchboard.co";
    }
}
