// Package openfeature is the OpenFeature provider for Switchboard (Go).
//
// Placeholder release reserving the import path
// github.com/switchboard-io/switchboard-go/openfeature while the full provider is built.
//
// Docs: https://switchboard.co  •  Source: https://github.com/switchboard-io/switchboard
package openfeature

// Name is the provider metadata name reported to OpenFeature.
const Name = "Switchboard"

// IsPlaceholder marks this as a name-reservation build.
const IsPlaceholder = true

// About returns a short description of the provider.
func About() string {
	return "switchboard-go/openfeature (placeholder) — OpenFeature provider for Go. See https://switchboard.co"
}
