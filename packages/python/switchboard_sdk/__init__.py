"""switchboard-sdk — local-evaluation feature-flag engine.

Implements docs/SPEC.md identically to the other language SDKs.
"""
from __future__ import annotations

import hashlib
import json
import re
from typing import Any, Callable, Dict, List, Optional

__version__ = "0.0.1"

_MAX = 0xFFFFFFFFFFFFF  # 2**52 - 1


def bucket_of(flag_key: str, salt: str, context_key: str) -> float:
    """Deterministic rollout bucket in [0, 1) — identical across all SDKs (SPEC §4)."""
    data = f"{flag_key}.{salt}.{context_key}".encode("utf-8")
    hex_digest = hashlib.sha1(data).hexdigest()
    n = int(hex_digest[:13], 16)  # 52 bits -> exact double
    return n / _MAX


def _as_number(v: Any) -> Optional[float]:
    if isinstance(v, bool):
        return None
    if isinstance(v, (int, float)):
        return float(v)
    return None


def _as_string(v: Any) -> Optional[str]:
    if isinstance(v, bool):
        return "true" if v else "false"
    if isinstance(v, str):
        return v
    if isinstance(v, (int, float)):
        if isinstance(v, float) and v.is_integer():
            return str(int(v))
        return str(v)
    return None


def _scalar_equals(a: Any, b: Any) -> bool:
    na, nb = _as_number(a), _as_number(b)
    if na is not None and nb is not None:
        return na == nb
    if isinstance(a, bool) and isinstance(b, bool):
        return a == b
    return _as_string(a) == _as_string(b)


def _match_op(op: str, attr: Any, values: List[Any]) -> bool:
    if attr is None:
        return False
    if op == "in":
        return any(_scalar_equals(attr, v) for v in values)
    if op == "contains":
        s = _as_string(attr)
        return s is not None and any((t := _as_string(v)) is not None and t in s for v in values)
    if op == "startsWith":
        s = _as_string(attr)
        return s is not None and any((t := _as_string(v)) is not None and s.startswith(t) for v in values)
    if op == "endsWith":
        s = _as_string(attr)
        return s is not None and any((t := _as_string(v)) is not None and s.endswith(t) for v in values)
    if op == "greaterThan":
        a, b = _as_number(attr), (_as_number(values[0]) if values else None)
        return a is not None and b is not None and a > b
    if op == "lessThan":
        a, b = _as_number(attr), (_as_number(values[0]) if values else None)
        return a is not None and b is not None and a < b
    if op == "regexMatch":
        s = _as_string(attr)
        p = _as_string(values[0]) if values else None
        return s is not None and p is not None and re.search(p, s) is not None
    return False


def _match_clause(clause: Dict[str, Any], ctx: Dict[str, Any]) -> bool:
    if clause.get("attribute") == "key":
        attr = ctx.get("key")
    else:
        attr = (ctx.get("attributes") or {}).get(clause.get("attribute"))
    r = _match_op(clause.get("op", ""), attr, clause.get("values", []))
    return (not r) if clause.get("negate") else r


def _resolve_index(flag: Dict[str, Any], variation: Optional[int], rollout: Optional[List[Dict[str, Any]]], ctx: Dict[str, Any]) -> int:
    if variation is not None:
        return variation
    if rollout:
        bucket = bucket_of(flag["key"], flag.get("salt", ""), ctx["key"])
        cumulative = 0
        for wv in rollout:
            cumulative += wv["weight"]
            if bucket * 100000 < cumulative:
                return wv["variation"]
        return rollout[-1]["variation"]
    raise ValueError("rule has neither variation nor rollout")


def _result(flag: Dict[str, Any], index: int, reason: str) -> Dict[str, Any]:
    variations = flag.get("variations", [])
    if index < 0 or index >= len(variations):
        return {"value": None, "variationIndex": -1, "reason": "ERROR"}
    return {"value": variations[index], "variationIndex": index, "reason": reason}


def evaluate(flag: Dict[str, Any], ctx: Dict[str, Any], resolver: Optional[Callable[[str], Optional[Dict[str, Any]]]] = None) -> Dict[str, Any]:
    """Evaluate a flag for a context. ``resolver(key)`` returns prerequisite flags."""
    try:
        if not flag.get("enabled"):
            return _result(flag, flag.get("offVariation", 0), "OFF")
        for p in flag.get("prerequisites", []):
            pre = resolver(p["key"]) if resolver else None
            if pre is None or evaluate(pre, ctx, resolver)["variationIndex"] != p["variation"]:
                return _result(flag, flag.get("offVariation", 0), "PREREQUISITE_FAILED")
        for t in flag.get("targets", []):
            if ctx["key"] in t.get("values", []):
                return _result(flag, t["variation"], "TARGET_MATCH")
        for rule in flag.get("rules", []):
            if all(_match_clause(c, ctx) for c in rule.get("clauses", [])):
                return _result(flag, _resolve_index(flag, rule.get("variation"), rule.get("rollout"), ctx), "RULE_MATCH")
        ft = flag.get("fallthrough", {})
        return _result(flag, _resolve_index(flag, ft.get("variation"), ft.get("rollout"), ctx), "FALLTHROUGH")
    except Exception:
        return {"value": None, "variationIndex": -1, "reason": "ERROR"}


class SwitchboardClient:
    """A client over a snapshot: ``{"flags": [ ...FlagConfig ]}``."""

    def __init__(self, flags: List[Dict[str, Any]]):
        self._flags = {f["key"]: f for f in flags}

    @classmethod
    def from_json(cls, data: Any) -> "SwitchboardClient":
        d = json.loads(data) if isinstance(data, str) else data
        return cls(d.get("flags", []))

    def evaluate_detail(self, key: str, ctx: Dict[str, Any]) -> Dict[str, Any]:
        flag = self._flags.get(key)
        if flag is None:
            return {"value": None, "variationIndex": -1, "reason": "ERROR"}
        return evaluate(flag, ctx, lambda k: self._flags.get(k))

    def get_bool(self, key: str, ctx: Dict[str, Any], default: bool = False) -> bool:
        v = self.evaluate_detail(key, ctx)["value"]
        return v if isinstance(v, bool) else default

    def get_string(self, key: str, ctx: Dict[str, Any], default: str = "") -> str:
        v = self.evaluate_detail(key, ctx)["value"]
        return v if isinstance(v, str) else default

    def get_number(self, key: str, ctx: Dict[str, Any], default: float = 0) -> float:
        v = self.evaluate_detail(key, ctx)["value"]
        return float(v) if isinstance(v, (int, float)) and not isinstance(v, bool) else default


def about() -> str:
    return "switchboard-sdk — local-evaluation feature flags for Python. See https://switchboard.co"
