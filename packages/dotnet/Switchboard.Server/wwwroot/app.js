// Switchboard Admin SPA — talks to the control-plane REST API and subscribes to the
// Server-Sent-Events stream for live updates.
const $ = (id) => document.getElementById(id);
let flags = [];
let selected = null;

async function api(method, path, body) {
  const opts = { method, headers: {} };
  if (body !== undefined) { opts.body = typeof body === "string" ? body : JSON.stringify(body); }
  const r = await fetch("/api" + path, opts);
  const text = await r.text();
  let data = null;
  try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  if (!r.ok) throw new Error((data && data.error) || r.statusText);
  return data;
}

function toast(msg, isErr = false) {
  const t = $("toast");
  t.textContent = msg;
  t.className = "toast show" + (isErr ? " err" : "");
  setTimeout(() => (t.className = "toast"), 2200);
}

function flash() { const d = $("dot"); d.classList.add("flash"); setTimeout(() => d.classList.remove("flash"), 400); }

async function loadFlags() {
  const snap = await api("GET", "/flags");
  flags = snap.flags || [];
  $("ver").textContent = "v" + snap.version;
  renderList();
}

function renderList() {
  const q = $("search").value.trim().toLowerCase();
  const list = $("list");
  const shown = flags.filter((f) => f.key.toLowerCase().includes(q));
  if (!shown.length) { list.innerHTML = '<p class="empty" style="margin-top:20px;">No flags</p>'; return; }
  list.innerHTML = shown.map((f) => `
    <div class="flag-item ${selected === f.key ? "active" : ""}" data-key="${f.key}">
      <div>
        <div class="key">${f.key}</div>
        <div class="meta">${(f.variations || []).length} variations · ${(f.rules || []).length} rules</div>
      </div>
      <label class="switch" title="toggle enabled">
        <input type="checkbox" ${f.enabled ? "checked" : ""} data-toggle="${f.key}" />
        <span class="slider"></span>
      </label>
    </div>`).join("");

  list.querySelectorAll(".flag-item").forEach((el) => {
    el.addEventListener("click", (e) => {
      if (e.target.closest(".switch")) return;
      select(el.dataset.key);
    });
  });
  list.querySelectorAll("[data-toggle]").forEach((cb) => {
    cb.addEventListener("click", async (e) => {
      e.stopPropagation();
      const key = cb.dataset.toggle;
      const flag = flags.find((f) => f.key === key);
      flag.enabled = cb.checked;
      try { await api("PUT", "/flags/" + encodeURIComponent(key), flag); toast(`${key} ${cb.checked ? "enabled" : "disabled"}`); }
      catch (err) { toast(err.message, true); cb.checked = !cb.checked; }
    });
  });
}

function select(key) {
  selected = key;
  renderList();
  const flag = flags.find((f) => f.key === key);
  $("editor").value = JSON.stringify(flag, null, 2);
  $("detail").style.display = "block";
  $("detailTitle").textContent = key;
  $("evalResult").textContent = "—";
  renderChips(flag);
}

function renderChips(flag) {
  const chips = [];
  chips.push(`<span class="pill ${flag.enabled ? "on" : "off"}">${flag.enabled ? "ENABLED" : "disabled"}</span>`);
  chips.push(`<span class="chip">variations <b>${(flag.variations || []).length}</b></span>`);
  chips.push(`<span class="chip">targets <b>${(flag.targets || []).length}</b></span>`);
  chips.push(`<span class="chip">rules <b>${(flag.rules || []).length}</b></span>`);
  const ft = flag.fallthrough || {};
  chips.push(`<span class="chip">fallthrough <b>${ft.rollout ? "rollout" : "variation " + ft.variation}</b></span>`);
  $("chips").innerHTML = chips.join("");
}

async function saveFlag() {
  let flag;
  try { flag = JSON.parse($("editor").value); }
  catch (e) { toast("Invalid JSON: " + e.message, true); return; }
  try {
    await api("PUT", "/flags/" + encodeURIComponent(flag.key), flag);
    toast("Saved " + flag.key);
    selected = flag.key;
  } catch (e) { toast(e.message, true); }
}

async function deleteFlag() {
  if (!selected) return;
  if (!confirm(`Delete flag "${selected}"?`)) return;
  try { await api("DELETE", "/flags/" + encodeURIComponent(selected)); toast("Deleted " + selected); selected = null; $("detail").style.display = "none"; }
  catch (e) { toast(e.message, true); }
}

async function evaluate() {
  if (!selected) return;
  let ctx;
  try { ctx = $("ctx").value; JSON.parse(ctx); }
  catch (e) { toast("Invalid context JSON", true); return; }
  try {
    const r = await api("POST", "/eval/" + encodeURIComponent(selected), ctx);
    const cls = r.reason === "ERROR" ? "err" : "on";
    $("evalResult").innerHTML =
      `<span class="${cls}">value          = ${JSON.stringify(r.value)}\n` +
      `variationIndex = ${r.variationIndex}\n` +
      `reason         = ${r.reason}</span>`;
  } catch (e) { $("evalResult").innerHTML = `<span class="err">${e.message}</span>`; }
}

function newFlag() {
  const key = prompt("New flag key:");
  if (!key) return;
  const flag = { key, enabled: false, variations: [false, true], offVariation: 0, fallthrough: { variation: 0 }, salt: Math.random().toString(36).slice(2, 8) };
  flags.push(flag);
  select(key);
  saveFlag();
}

// --- live updates via SSE ---
function connectStream() {
  const es = new EventSource("/api/stream");
  es.addEventListener("change", () => { flash(); loadFlags(); });
  es.onerror = () => { $("dot").style.background = "var(--danger)"; };
  es.addEventListener("hello", () => { $("dot").style.background = "var(--accent2)"; });
}

window.addEventListener("DOMContentLoaded", () => {
  $("search").addEventListener("input", renderList);
  $("newBtn").addEventListener("click", newFlag);
  $("saveBtn").addEventListener("click", saveFlag);
  $("deleteBtn").addEventListener("click", deleteFlag);
  $("evalBtn").addEventListener("click", evaluate);
  loadFlags().catch((e) => toast(e.message, true));
  connectStream();
});
