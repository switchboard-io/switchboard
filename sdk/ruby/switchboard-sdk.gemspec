Gem::Specification.new do |spec|
  spec.name          = "switchboard-sdk"
  spec.version       = "0.0.1"
  spec.authors       = ["Switchboard"]
  spec.summary       = "Local-evaluation feature-flag SDK for Ruby (placeholder release)."
  spec.description   = "Real-time, offline-capable, OpenFeature-compatible feature flags for Ruby. " \
                       "Placeholder release reserving the gem name while the full SDK is built."
  spec.homepage      = "https://switchboard.co"
  spec.license       = "Apache-2.0"
  spec.required_ruby_version = ">= 3.0"

  spec.metadata = {
    "source_code_uri" => "https://github.com/switchboard-io/switchboard",
    "bug_tracker_uri" => "https://github.com/switchboard-io/switchboard/issues"
  }

  spec.files         = ["lib/switchboard/sdk.rb", "README.md"]
  spec.require_paths = ["lib"]
end
