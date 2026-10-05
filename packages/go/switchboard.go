// Package switchboard is the Go SDK for Switchboard feature management.
//
// Placeholder release (v0.0.1) reserving the module path while the full
// local-evaluation, streaming, OpenFeature-compatible client is built.
//
// Docs: https://switchboard.co  •  Source: https://github.com/switchboard-io/switchboard
package switchboard

// Name is the product name.
const Name = "Switchboard"

// IsPlaceholder marks this as a name-reservation build.
const IsPlaceholder = true

// About returns a short description of the SDK.
func About() string {
	return "switchboard-go (placeholder) — local-evaluation feature flags for Go. See https://switchboard.co"
}
