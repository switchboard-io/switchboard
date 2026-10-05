//! switchboard-sdk — placeholder release (0.0.1).
//!
//! Reserves the `switchboard-sdk` crate name on crates.io while the full
//! local-evaluation, streaming, OpenFeature-compatible client is built.
//!
//! Docs: <https://switchboard.co> — Source: <https://github.com/switchboard-io/switchboard>

/// Product name.
pub const NAME: &str = "Switchboard";

/// Marks this as a name-reservation build.
pub const IS_PLACEHOLDER: bool = true;

/// Returns a short description of the SDK.
pub fn about() -> &'static str {
    "switchboard-sdk (placeholder) — local-evaluation feature flags for Rust. See https://switchboard.co"
}
