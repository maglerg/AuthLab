import { useState } from "react";
import { login, register } from "../api/auth";

export default function AuthForm({ onLogin }) {
    const [mode, setMode] = useState("login");

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [displayName, setDisplayName] = useState("");

    const [error, setError] = useState("");
    const [message, setMessage] = useState("");
    const [loading, setLoading] = useState(false);

    const isRegister = mode === "register";

    async function handleSubmit(event) {
        event.preventDefault();

        console.log("1. Form submitted");

        setError("");
        setMessage("");
        setLoading(true);

        try {
            if (isRegister) {
                console.log("Registration started");

                await register(email, password, displayName);

                setMessage("Аккаунт создан. Теперь войди в него.");
                setMode("login");
                setPassword("");
            } else {
                console.log("2. Login started", email);

                await login(email, password);

                console.log("3. Login successful");

                setMessage("Вход выполнен успешно.");
                onLogin(email);
            }
        } catch (err) {
            console.error("Login error:", err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    function changeMode(nextMode) {
        setMode(nextMode);
        setError("");
        setMessage("");
    }

    return (
        <section className="auth-card">
            <div className="auth-heading">
                <div className="brand-icon">A</div>
                <h1>AuthLab</h1>
                <p>
                    {isRegister
                        ? "Создай аккаунт провайдера"
                        : "Войди в свой аккаунт"}
                </p>
            </div>

            <div className="auth-tabs">
                <button
                    type="button"
                    className={!isRegister ? "active" : ""}
                    onClick={() => changeMode("login")}
                >
                    Вход
                </button>

                <button
                    type="button"
                    className={isRegister ? "active" : ""}
                    onClick={() => changeMode("register")}
                >
                    Регистрация
                </button>
            </div>

            <form onSubmit={handleSubmit} className="auth-form">
                {isRegister && (
                    <div className="form-field">
                        <label htmlFor="displayName">Имя</label>
                        <input
                            id="displayName"
                            type="text"
                            value={displayName}
                            onChange={(event) =>
                                setDisplayName(event.target.value)
                            }
                            placeholder="Как к тебе обращаться"
                            autoComplete="name"
                            required
                        />
                    </div>
                )}

                <div className="form-field">
                    <label htmlFor="email">Email</label>
                    <input
                        id="email"
                        type="email"
                        value={email}
                        onChange={(event) => setEmail(event.target.value)}
                        placeholder="you@example.com"
                        autoComplete="email"
                        required
                    />
                </div>

                <div className="form-field">
                    <label htmlFor="password">Пароль</label>
                    <input
                        id="password"
                        type="password"
                        value={password}
                        onChange={(event) => setPassword(event.target.value)}
                        placeholder="Введи пароль"
                        autoComplete={
                            isRegister ? "new-password" : "current-password"
                        }
                        minLength={isRegister ? 10 : undefined}
                        required
                    />
                </div>

                {error && (
                    <div className="alert alert-error" role="alert">
                        {error}
                    </div>
                )}

                {message && (
                    <div className="alert alert-success" role="status">
                        {message}
                    </div>
                )}

                <button
                    className="submit-button"
                    type="submit"
                    disabled={loading}
                >
                    {loading
                        ? "Подожди..."
                        : isRegister
                            ? "Создать аккаунт"
                            : "Войти"}
                </button>
            </form>

            <p className="auth-footer">
                AuthLab Provider · Учебный проект авторизации
            </p>
        </section>
    );
}