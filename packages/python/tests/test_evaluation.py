import unittest

from switchboard_sdk import SwitchboardClient, bucket_of


class TestEvaluation(unittest.TestCase):
    def setUp(self):
        self.client = SwitchboardClient.from_json({"flags": [
            {"key": "disabled", "enabled": False, "variations": [False, True], "offVariation": 0,
             "fallthrough": {"variation": 1}, "salt": "s"},
            {"key": "target", "enabled": True, "variations": [False, True], "offVariation": 0,
             "targets": [{"values": ["vip"], "variation": 1}], "fallthrough": {"variation": 0}, "salt": "s"},
            {"key": "rule", "enabled": True, "variations": ["off", "on"], "offVariation": 0,
             "rules": [{"clauses": [{"attribute": "country", "op": "in", "values": ["US", "CA"]}], "variation": 1}],
             "fallthrough": {"variation": 0}, "salt": "s"},
            {"key": "rollout", "enabled": True, "variations": [False, True], "offVariation": 0,
             "fallthrough": {"rollout": [{"variation": 0, "weight": 50000}, {"variation": 1, "weight": 50000}]}, "salt": "s"},
        ]})

    def test_bucket_parity(self):
        self.assertEqual(bucket_of("f", "s", "user-A"), 0.250274217889144)
        self.assertEqual(bucket_of("checkout-v2", "abc", "user-1"), 0.5864417850195552)

    def test_disabled_off(self):
        r = self.client.evaluate_detail("disabled", {"key": "u"})
        self.assertEqual(r["reason"], "OFF")
        self.assertFalse(r["value"])

    def test_target(self):
        r = self.client.evaluate_detail("target", {"key": "vip"})
        self.assertEqual(r["reason"], "TARGET_MATCH")
        self.assertTrue(r["value"])

    def test_rule(self):
        r = self.client.evaluate_detail("rule", {"key": "u", "attributes": {"country": "US"}})
        self.assertEqual(r["reason"], "RULE_MATCH")
        self.assertEqual(r["value"], "on")

    def test_fallthrough(self):
        r = self.client.evaluate_detail("rule", {"key": "u", "attributes": {"country": "IN"}})
        self.assertEqual(r["reason"], "FALLTHROUGH")

    def test_rollout_deterministic(self):
        a = self.client.evaluate_detail("rollout", {"key": "user-A"})["variationIndex"]
        b = self.client.evaluate_detail("rollout", {"key": "user-A"})["variationIndex"]
        self.assertEqual(a, b)
        zero = one = 0
        for i in range(2000):
            idx = self.client.evaluate_detail("rollout", {"key": f"user-{i}"})["variationIndex"]
            zero += idx == 0
            one += idx == 1
        self.assertGreater(zero, 700)
        self.assertGreater(one, 700)


if __name__ == "__main__":
    unittest.main()
