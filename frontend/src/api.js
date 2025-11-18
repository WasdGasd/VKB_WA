const API_BASE = "https://localhost:5001/api";

// Аутентификация
export async function login(username, password) {
    const res = await fetch(`${API_BASE}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
        credentials: "include",
    });

    if (res.status === 401) {
        throw new Error("Invalid username or password");
    }

    if (!res.ok) {
        throw new Error("Login failed");
    }

    try {
        return await res.json();
    } catch {
        return { success: true };
    }
}

// Команды
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

// Управление ботом
export async function botControl(action) {
    return fetch(`${API_BASE}/bot/${action}`, {
        method: "POST",
        credentials: "include"
    });
}

export async function startBot() {
    return botControl('start');
}

export async function stopBot() {
    return botControl('stop');
}

export async function restartBot() {
    return botControl('restart');
}

// Статистика и логи
export async function getStats() {
    const res = await fetch(`${API_BASE}/bot/stats`, {
        credentials: "include"
    });
    if (!res.ok) throw new Error('Failed to fetch stats');
    return res.json();
}

export async function getBotStatus() {
    const res = await fetch(`${API_BASE}/bot/status`, {
        credentials: "include"
    });
    if (!res.ok) throw new Error('Failed to fetch bot status');
    return res.json();
}

export async function getLogs() {
    const res = await fetch(`${API_BASE}/logs`, {
        credentials: "include"
    });
    if (!res.ok) throw new Error('Failed to fetch logs');
    return res.json();
}

// Получение статистики команд
export async function getCommandStats() {
    const res = await fetch(`${API_BASE}/stats/commands`, {
        credentials: "include"
    });
    if (!res.ok) throw new Error('Failed to fetch command stats');
    return res.json();
}

// Получение информации о пользователях
export async function getUserStats() {
    const res = await fetch(`${API_BASE}/stats/users`, {
        credentials: "include"
    });
    if (!res.ok) throw new Error('Failed to fetch user stats');
    return res.json();
}

// Получение информации о системе
export async function getSystemStats() {
    const res = await fetch(`${API_BASE}/stats/system`, {
        credentials: "include"
    });
    if (!res.ok) throw new Error('Failed to fetch system stats');
    return res.json();
}