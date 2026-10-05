// @switchboard/openfeature-provider — OpenFeature-style provider backed by the
// Switchboard local-evaluation client. Exposes resolve methods returning a value
// plus the evaluation reason (OpenFeature ResolutionDetails shape).
export const metadata = { name: "Switchboard" };

export class SwitchboardProvider {
  /** @param {{ evaluateDetail: (key: string, ctx: object) => {value:any,reason:string} }} client */
  constructor(client) { this.client = client; }

  resolveBooleanEvaluation(key, def, ctx) {
    const r = this.client.evaluateDetail(key, ctx || { key: "anonymous" });
    return { value: typeof r.value === "boolean" ? r.value : def, reason: r.reason };
  }
  resolveStringEvaluation(key, def, ctx) {
    const r = this.client.evaluateDetail(key, ctx || { key: "anonymous" });
    return { value: typeof r.value === "string" ? r.value : def, reason: r.reason };
  }
  resolveNumberEvaluation(key, def, ctx) {
    const r = this.client.evaluateDetail(key, ctx || { key: "anonymous" });
    return { value: typeof r.value === "number" ? r.value : def, reason: r.reason };
  }
}

export default { metadata, SwitchboardProvider };