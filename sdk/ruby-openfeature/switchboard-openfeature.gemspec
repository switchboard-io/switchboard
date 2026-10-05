Gem::Specification.new do |spec|
  spec.name          = "switchboard-openfeature"
  spec.version       = "0.0.1"
  spec.authors       = ["Switchboard"]
  spec.summary       = "OpenFeature provider for Switchboard feature management (Ruby, placeholder release)."
  spec.description   = "Plug Switchboard into any OpenFeature-based Ruby application. " \
                       "Placeholder release reserving the gem name while the full provider is built."
  spec.homepage      = "https://switchboard.co"
  spec.license       = "Apache-2.0"
  spec.required_ruby_version = ">= 3.0"

  spec.metadata = {
    "source_code_uri" => "https://github.com/switchboard-io/switchboard",
    "bug_tracker_uri" => "https://github.com/switchboard-io/switchboard/issues"
  }

  spec.files         = ["lib/switchboard/openfeature.rb", "README.md"]
  spec.require_paths = ["lib"]
end
