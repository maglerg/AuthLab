const PROVIDER_URL = import.meta.env.VITE_PROVIDER_URL;
const PROVIDER_UI_URL = import.meta.env.VITE_PROVIDER_UI_URL;

const CLIENT_ID = "authlab-react";
const REDIRECT_URI = `${window.location.origin}/callback`;

function base64UrlEncode(bytes) {
    let binary = "";

    for (const byte of bytes) {
        binary += String.fromCharCode(byte);
    }

    return btoa(binary)
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, "");
}

async function createCodeChallenge(verifier) {
    const bytes = new TextEncoder().encode(verifier);
    const digest = await crypto.subtle.digest("SHA-256", bytes);

    return base64UrlEncode(new Uint8Array(digest));
}

export async function startAuthorization() {
    console.log("START AUTHORIZATION:", window.location.origin);
    // Генерируем случайный PKCE verifier.
    const randomBytes = crypto.getRandomValues(new Uint8Array(32));
    const verifier = base64UrlEncode(randomBytes);

    // Генерируем state для защиты от подмены ответа.
    const stateBytes = crypto.getRandomValues(new Uint8Array(32));
    const state = base64UrlEncode(stateBytes);

    const challenge = await createCodeChallenge(verifier);

    // Сохраняем значения до возвращения из провайдера.
    sessionStorage.setItem("pkce_verifier", verifier);
    sessionStorage.setItem("oauth_state", state);

    console.log("OAuth storage check:", {
        origin: window.location.origin,
        stateSaved: !!sessionStorage.getItem("oauth_state"),
        verifierSaved: !!sessionStorage.getItem("pkce_verifier"),
    });

    const authorizeUrl = new URL(
        "/connect/authorize",
        PROVIDER_URL
    );

    authorizeUrl.search = new URLSearchParams({
        response_type: "code",
        client_id: CLIENT_ID,
        redirect_uri: REDIRECT_URI,
        state,
        code_challenge: challenge,
        code_challenge_method: "S256",
    }).toString();

    // Открываем интерфейс провайдера.
    const providerLoginUrl = new URL("/", PROVIDER_UI_URL);
    providerLoginUrl.searchParams.set(
        "returnUrl",
        authorizeUrl.toString()
    );
    console.log("Provider UI URL:", PROVIDER_UI_URL);
    console.log("Login URL:", providerLoginUrl.toString());

    window.location.assign(providerLoginUrl.toString());
}

export async function exchangeCodeForToken(code, returnedState) {
    const expectedState = sessionStorage.getItem("oauth_state");
    const verifier = sessionStorage.getItem("pkce_verifier");

    console.log("OAuth callback storage check:", {
        origin: window.location.origin,
        stateSaved: !!expectedState,
        verifierSaved: !!verifier,
    });

    if (!expectedState || returnedState !== expectedState) {
        throw new Error("Проверка OAuth state не пройдена.");
    }

    if (!verifier) {
        throw new Error("PKCE verifier не найден. Начни вход заново.");
    }

    const body = new URLSearchParams({
        grant_type: "authorization_code",
        code,
        client_id: CLIENT_ID,
        redirect_uri: REDIRECT_URI,
        code_verifier: verifier,
    });

    const response = await fetch(`${PROVIDER_URL}/connect/token`, {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
        },
        body,
    });

    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
        throw new Error(
            data.error || "Не удалось обменять код на токен."
        );
    }

    // Код и verifier больше не нужны.
    sessionStorage.removeItem("oauth_state");
    sessionStorage.removeItem("pkce_verifier");

    return data;
}

export function getCallbackParameters() {
    const url = new URL(window.location.href);

    return {
        code: url.searchParams.get("code"),
        state: url.searchParams.get("state"),
        error: url.searchParams.get("error"),
    };
}

export function cleanCallbackUrl() {
    window.history.replaceState({}, document.title, "/callback");
}