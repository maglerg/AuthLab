import { useState } from "react";
import AuthForm from "./components/AuthForm";
import { logout } from "./api/auth";
import "./App.css";

function getSafeReturnUrl() {
    const returnUrl = new URLSearchParams(
        window.location.search
    ).get("returnUrl");

    if (!returnUrl) {
        return null;
    }

    try {
        const url = new URL(returnUrl);
        const apiUrl = new URL(import.meta.env.VITE_API_URL);

        // Разрешаем возвращение только к endpoint авторизации
        // нашего собственного провайдера.
        if (
            url.origin !== apiUrl.origin ||
            url.pathname !== "/connect/authorize"
        ) {
            return null;
        }

        return url.toString();
    } catch {
        return null;
    }
}

export default function App() {
    const [userEmail, setUserEmail] = useState("");
    const [error, setError] = useState("");
    const [isLoggingOut, setIsLoggingOut] = useState(false);

    const returnUrl = getSafeReturnUrl();
    console.log("Current URL:", window.location.href);
    console.log(
        "Raw returnUrl:",
        new URLSearchParams(window.location.search).get("returnUrl")
    );
    console.log("Safe returnUrl:", returnUrl);
    function handleLogin(email) {
        setError("");

        console.log("handleLogin called");
        console.log("returnUrl inside handleLogin:", returnUrl);

        if (returnUrl) {
            console.log("Redirecting to:", returnUrl);
            window.location.assign(returnUrl);
            return;
        }

        console.log("No returnUrl, showing dashboard");
        setUserEmail(email);
    }

    async function handleLogout() {
        setError("");
        setIsLoggingOut(true);

        try {
            await logout();
            setUserEmail("");
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Не удалось выйти из аккаунта."
            );
        } finally {
            setIsLoggingOut(false);
        }
    }

    if (!userEmail) {
        return (
            <main className="app">
                <div className="background-glow" />

                <AuthForm onLogin={handleLogin} />
            </main>
        );
    }

    return (
        <main className="app">
            <div className="background-glow" />

            <section className="dashboard-card">
                <div className="brand-icon">A</div>

                <p className="eyebrow">
                    AUTHENTICATION PROVIDER
                </p>

                <h1>Добро пожаловать!</h1>

                <p className="dashboard-description">
                    Ты успешно вошла в AuthLab Provider.
                </p>

                <div className="user-info">
                    <span className="status-dot" />

                    <div>
                        <span className="info-label">
                            Текущий аккаунт
                        </span>

                        <strong>{userEmail}</strong>
                    </div>
                </div>

                <div className="info-panel">
                    <h2>Что сейчас работает?</h2>

                    <ul>
                        <li>
                            Регистрация через ASP.NET Core Identity
                        </li>

                        <li>
                            Проверка email и пароля
                        </li>

                        <li>
                            Cookie авторизации провайдера
                        </li>

                        <li>
                            OAuth 2.0 Authorization Code + PKCE
                        </li>
                    </ul>
                </div>

                {error && (
                    <div
                        className="alert alert-error"
                        role="alert"
                    >
                        {error}
                    </div>
                )}

                <button
                    className="submit-button"
                    type="button"
                    onClick={handleLogout}
                    disabled={isLoggingOut}
                >
                    {isLoggingOut
                        ? "Выходим..."
                        : "Выйти из аккаунта"}
                </button>
            </section>
        </main>
    );
}