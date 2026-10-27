import { useEffect, useRef, useState } from "react";
import {
    cleanCallbackUrl,
    exchangeCodeForToken,
    getCallbackParameters,
    startAuthorization,
} from "./api/oauth";
import "./App.css";

export default function App() {
    const [token, setToken] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const callbackStarted = useRef(false);

    const isCallback =
        window.location.pathname === "/callback";

    useEffect(() => {
        if (!isCallback || callbackStarted.current) {
            return;
        }

        callbackStarted.current = true;

        const {
            code,
            state,
            error: oauthError,
        } = getCallbackParameters();

        // Удаляем code и state из адресной строки.
        cleanCallbackUrl();

        if (oauthError) {
            // Оборачиваем в микротаску, чтобы ESLint не ругался на синхронный setState
            Promise.resolve().then(() => {
                setError(`Провайдер вернул ошибку: ${oauthError}`);
            });
            return;
        }

        if (!code || !state) {
            Promise.resolve().then(() => {
                setError(
                    "В callback отсутствует authorization code или state."
                );
            });
            return;
        }

        let cancelled = false;

        async function completeLogin() {
            setLoading(true);
            setError("");

            try {
                const result = await exchangeCodeForToken(
                    code,
                    state
                );

                if (cancelled) {
                    return;
                }

                const accessToken =
                    result.access_token ?? result.accessToken;

                if (
                    typeof accessToken !== "string" ||
                    accessToken.length === 0
                ) {
                    throw new Error(
                        "Провайдер не вернул access token."
                    );
                }

                setToken(accessToken);
            } catch (err) {
                if (!cancelled) {
                    setError(
                        err instanceof Error
                            ? err.message
                            : "Не удалось завершить авторизацию."
                    );
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        completeLogin();

        return () => {
            cancelled = true;
        };
    }, [isCallback]);

    async function handleLogin() {
        setError("");
        setLoading(true);

        try {
            await startAuthorization();

            // Обычно страница уже перенаправлена на провайдера.
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Не удалось начать авторизацию."
            );

            setLoading(false);
        }
    }

    function handleLogout() {
        // Очищаем токен только в состоянии React.
        setToken("");
        setError("");
        setLoading(false);
    }

    async function handleCopyToken() {
        try {
            await navigator.clipboard.writeText(token);
        } catch {
            setError(
                "Не удалось скопировать токен. Проверь разрешения браузера."
            );
        }
    }

    return (
        <main className="page">
            <section className="client-card">
                <div className="logo">A</div>

                <p className="eyebrow">
                    OAUTH 2.0 DEMO
                </p>

                <h1>AuthLab Client</h1>

                <p className="description">
                    Войди через отдельный провайдер авторизации
                    и получи access token.
                </p>

                {!isCallback && !token && (
                    <button
                        className="primary-button"
                        type="button"
                        onClick={handleLogin}
                        disabled={loading}
                    >
                        {loading
                            ? "Перенаправление..."
                            : "Войти через AuthLab"}
                    </button>
                )}

                {loading && isCallback && (
                    <p className="status" role="status">
                        Обмениваем код на access token…
                    </p>
                )}

                {error && (
                    <div
                        className="error"
                        role="alert"
                    >
                        {error}
                    </div>
                )}

                {token && (
                    <div className="token-section">
                        <div className="success">
                            <span className="success-dot" />
                            Авторизация завершена
                        </div>

                        <label htmlFor="accessToken">
                            Access token (JWT)
                        </label>

                        <textarea
                            id="accessToken"
                            value={token}
                            readOnly
                            rows={7}
                        />

                        <button
                            className="secondary-button"
                            type="button"
                            onClick={handleCopyToken}
                        >
                            Копировать токен
                        </button>

                        <button
                            className="primary-button"
                            type="button"
                            onClick={handleLogout}
                        >
                            Очистить токен
                        </button>

                        <p className="hint">
                            Очистка удаляет токен только из интерфейса.
                            Это не отзывает токен на сервере.
                        </p>
                    </div>
                )}

                <div className="flow-info">
                    <h2>Как это работает</h2>

                    <ol>
                        <li>
                            Клиент создаёт PKCE verifier и challenge.
                        </li>

                        <li>
                            Провайдер проверяет сессию пользователя.
                        </li>

                        <li>
                            Клиент получает одноразовый authorization code.
                        </li>

                        <li>
                            Код обменивается на JWT с проверкой PKCE.
                        </li>
                    </ol>
                </div>

                <p className="footer">
                    AuthLab · Учебный проект
                </p>
            </section>
        </main>
    );
}