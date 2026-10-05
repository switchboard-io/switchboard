//! switchboard-openfeature — placeholder release (0.0.1).
//!
//! Reserves the `switchboard-openfeature` crate name on crates.io while the full
//! OpenFeature provider is built.
//!
//! Docs: <https://switchboard.co> — Source: <https://github.com/switchboard-io/switchboard>

/// Provider metadata name reported to OpenFeature.
pub const NAME: &str = "Switchboard";

/// Marks this as a name-reservation build.
pub const IS_PLACEHOLDER: bool = true;

/// Returns a short description of the provider.
pub fn about() -> &'static str {
    "switchboard-openfeature (placeholder) — OpenFeature provider for Rust. See https://switchboard.co"
}
