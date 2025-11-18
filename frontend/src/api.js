const API_BASE = "https://localhost:5001/api";

// Существующие методы...
export async function login(username, password) {
    const res = await fetch(`${API_BASE}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
        credentials: "include",
    });
    if (!res.ok) throw new Error("Login failed");
    return res.json();
}

export async function fetchCommands() {
    const res = await fetch(`${API_BASE}/commands`, { credentials: "include" });
    return res.json();
}

export async function createCommand(cmd) {
    const res = await fetch(`${API_BASE}/commands`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(cmd),
        credentials: "include",
    });
    return res.json();
}

export async function updateCommand(id, cmd) {
    const res = await fetch(`${API_BASE}/commands/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(cmd),
        credentials: "include",
    });
    return res.json();
}

export async function deleteCommand(id) {
    return fetch(`${API_BASE}/commands/${id}`, {
        method: "DELETE",
        credentials: "include",
    });
}

export async function botControl(action) {
    return fetch(`${API_BASE}/bot/${action}`, { method: "POST", credentials: "include" });
}

// Новые методы для статистики
export async function getStats() {
    const res = await fetch(`${API_BASE}/stats`, { credentials: "include" });
    return res.json();
}

export async function getLogs() {
    const res = await fetch(`${API_BASE}/logs`, { credentials: "include" });
    return res.json();
}

export async function getBotStatus() {
    const res = await fetch(`${API_BASE}/bot/status`, { credentials: "include" });
    return res.json();
}