# Backend — validazioneCaptcha

Backend ASP.NET Core 10 con Minimal API per il form di login Kirey.  
Espone un singolo endpoint REST che verifica il token reCAPTCHA con Google e autentica l'utente.

## Struttura del progetto

```
BE/validazioneCaptcha/
├── validazioneCaptcha.csproj
├── Program.cs                        # Entry point + definizione Minimal API
├── appsettings.json                  # Configurazione (secret key reCAPTCHA)
├── appsettings.Development.json      # Override per sviluppo locale
├── Models/
│   ├── LoginRequest.cs               # Payload JSON in ingresso dal frontend
│   └── LoginResponse.cs              # Payload JSON in uscita verso il frontend
└── Services/
    └── ReCaptchaService.cs           # Verifica server-to-server con Google
```

## Avvio

```powershell
cd BE/validazioneCaptcha
dotnet run
```

Il server si avvia su `http://localhost:5067`.

## Endpoint

### POST /api/auth/login

**Payload richiesta:**

```json
{
	"email": "nome@azienda.it",
	"password": "password",
	"captchaToken": "<token-dal-widget-recaptcha>"
}
```

**Risposta successo:**

```json
{ "ok": true }
```

**Risposta errore:**

```json
{ "ok": false, "message": "Credenziali non valide." }
```

## Flusso di verifica

1. Il backend riceve il payload dal frontend
2. Controlla che `captchaToken` non sia null o vuoto
3. Invia `captchaToken` + `secretKey` a Google (`/recaptcha/api/siteverify`)
4. Se Google risponde `success: false` → risponde `{ "ok": false }`
5. Solo se il captcha è valido → verifica le credenziali utente
6. Se le credenziali sono corrette → risponde `{ "ok": true }`

## Configurazione reCAPTCHA

La **secret key** è configurata in `appsettings.json`:

```json
{
	"ReCaptcha": {
		"SecretKey": "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe"
	}
}
```

Questa è la chiave di test Google per sviluppo locale: accetta sempre qualsiasi token senza validazione reale.  
In produzione non committare mai la chiave reale. Usa una variabile d'ambiente:

```powershell
$env:ReCaptcha__SecretKey = "LA-TUA-SECRET-KEY-REALE"
```

Il doppio underscore `__` è il separatore gerarchico .NET per le variabili d'ambiente.

## Collegamento al frontend

Aggiorna `src/app.js` nel frontend con la porta corretta:

```js
const API_BASE_URL = "http://localhost:5067/api";
```

## Test

### Login riuscito

```powershell
Invoke-RestMethod -Uri "http://localhost:5067/api/auth/login" -Method POST -ContentType "application/json" -Body '{"email":"admin@kirey.com","password":"password123","captchaToken":"test"}'
```

Risposta attesa:

```
ok    message
--    -------
True
```

### Login fallito (credenziali errate)

```powershell
Invoke-RestMethod -Uri "http://localhost:5067/api/auth/login" -Method POST -ContentType "application/json" -Body '{"email":"sbagliata@kirey.com","password":"wrong","captchaToken":"test"}'
```

Risposta attesa:

```
ok    message
--    -------
False Credenziali non valide.
```

## Credenziali di test

Per sviluppo l'endpoint accetta solo:

| Campo    | Valore            |
| -------- | ----------------- |
| Email    | `admin@kirey.com` |
| Password | `password123`     |

> Da sostituire con vera autenticazione (database, hash password, JWT) in prod (TO-BE?).
