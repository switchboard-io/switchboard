"""switchboard-openfeature-provider — OpenFeature provider backed by the
Switchboard local-evaluation client.

Exposes resolve methods returning a value plus the evaluation reason
(OpenFeature ResolutionDetails shape).
"""
from __future__ import annotations

from typing import Any, Dict, Optional

__version__ = "0.0.1"

METADATA_NAME = "Switchboard"


class SwitchboardProvider:
    """Wraps a ``switchboard_sdk.SwitchboardClient`` for OpenFeature."""

    def __init__(self, client: Any):
        self._client = client

    def get_metadata(self) -> Dict[str, str]:
        return {"name": METADATA_NAME}

    def resolve_boolean_details(self, key: str, default: bool, ctx: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        r = self._client.evaluate_detail(key, ctx or {"key": "anonymous"})
        v = r["value"]
        return {"value": v if isinstance(v, bool) else default, "reason": r["reason"]}

    def resolve_string_details(self, key: str, default: str, ctx: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        r = self._client.evaluate_detail(key, ctx or {"key": "anonymous"})
        v = r["value"]
        return {"value": v if isinstance(v, str) else default, "reason": r["reason"]}

    def resolve_number_details(self, key: str, default: float, ctx: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        r = self._client.evaluate_detail(key, ctx or {"key": "anonymous"})
        v = r["value"]
        ok = isinstance(v, (int, float)) and not isinstance(v, bool)
        return {"value": float(v) if ok else default, "reason": r["reason"]}


def about() -> str:
    return "switchboard-openfeature-provider — OpenFeature provider for Python. See https://switchboard.co"
