# Login Flow — FE → BE → Google

```mermaid
sequenceDiagram
    actor User
    participant FE as Frontend<br/>(browser)
    participant Google as Google<br/>(reCAPTCHA API)
    participant BE as Backend<br/>(ASP.NET Core)

    %% ── Page load ──────────────────────────────────────────────
    Note over FE,Google: Page load
    FE->>Google: GET /recaptcha/api.js<br/>(async, onload=onRecaptchaReady)
    Google-->>FE: reCAPTCHA script
    FE->>FE: grecaptcha.render() → widget shown

    %% ── User interaction ────────────────────────────────────────
    Note over User,FE: User fills the form
    User->>FE: types email + password
    User->>FE: solves reCAPTCHA challenge

    %% ── Submit ──────────────────────────────────────────────────
    User->>FE: clicks "Accedi"

    alt reCAPTCHA widget not ready or token missing
        FE-->>User: Error — "Completa la verifica reCAPTCHA."
        Note over FE: no HTTP request sent
    else captcha token present
        FE->>BE: POST /api/auth/login<br/>{ email, password, captchaToken }

        %% ── BE: guard — empty token ─────────────────────────────
        alt captchaToken is null / whitespace
            BE-->>FE: 400 Bad Request<br/>{ ok: false, message: "Token reCAPTCHA mancante." }
            FE-->>User: Error message shown, captcha reset

        %% ── BE: verify token with Google ────────────────────────
        else token present → call Google siteverify
            BE->>Google: POST /recaptcha/api/siteverify<br/>{ secret, response: captchaToken }

            alt Google unreachable / HTTP error
                Google-->>BE: non-2xx response
                BE-->>FE: 422 Unprocessable Entity<br/>{ ok: false, message: "Verifica reCAPTCHA fallita." }
                FE-->>User: Error message shown, captcha reset

            else Google responds { success: false }
                Google-->>BE: 200 { "success": false }
                BE-->>FE: 422 Unprocessable Entity<br/>{ ok: false, message: "Verifica reCAPTCHA fallita." }
                FE-->>User: Error message shown, captcha reset

            else Google responds { success: true }
                Google-->>BE: 200 { "success": true }

                %% ── BE: validate credentials ────────────────────
                alt wrong email or password
                    BE-->>FE: 401 Unauthorized<br/>{ ok: false, message: "Credenziali non valide." }
                    FE-->>User: Error message shown, captcha reset

                else credentials match config (Auth:Email / Auth:Password)
                    BE-->>FE: 200 OK<br/>{ ok: true }
                    FE->>FE: window.location.href = success.html
                    FE-->>User: Redirected to success page
                end
            end
        end
    end
```

## Scenario summary

| # | Scenario | Triggered by | HTTP status | Outcome |
|---|----------|-------------|-------------|---------|
| 1 | reCAPTCHA widget not ready or unchecked | FE guard | — (no request) | Error shown, captcha reset |
| 2 | Missing token reaches BE | Empty `captchaToken` field | 400 Bad Request | Error shown, captcha reset |
| 3 | Google API unreachable | Network error / non-2xx from Google | 422 Unprocessable Entity | Error shown, captcha reset |
| 4 | Google rejects token | `{ "success": false }` from siteverify | 422 Unprocessable Entity | Error shown, captcha reset |
| 5 | Wrong credentials | Email or password mismatch vs config | 401 Unauthorized | Error shown, captcha reset |
| 6 | Happy path | All checks pass | 200 OK | Redirect to `success.html` |
