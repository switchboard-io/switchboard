# frozen_string_literal: true

# switchboard-sdk — placeholder release (0.0.1).
# Reserves the gem name on RubyGems while the full SDK is built.
# Docs: https://switchboard.co  •  Source: https://github.com/switchboard-io/switchboard
module Switchboard
  module Sdk
    VERSION = "0.0.1"
    NAME = "Switchboard"
    PLACEHOLDER = true

    # Returns a short description of the SDK.
    def self.about
      "switchboard-sdk (placeholder) — local-evaluation feature flags for Ruby. See https://switchboard.co"
    end
  end
end
