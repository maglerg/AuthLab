const API_URL = import.meta.env.VITE_API_URL;

async function sendRequest(endpoint, body) {
    const response = await fetch(`${API_URL}${endpoint}`, {
        method: "POST",
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(body),
    });

    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
        const message = data.error
            || data.errors?.join(", ")
            || "Произошла ошибка при выполнении запроса.";

        throw new Error(message);
    }

    return data;
}

export function register(email, password, displayName) {
    return sendRequest("/account/register", {
        email,
        password,
        displayName,
    });
}

export function login(email, password) {
    return sendRequest("/account/login", {
        email,
        password,
    });
}

export async function logout() {
    const response = await fetch(`${API_URL}/account/logout`, {
        method: "POST",
        credentials: "include",
    });

    if (!response.ok) {
        throw new Error("Не удалось выйти из аккаунта.");
    }

    return response.json();
}