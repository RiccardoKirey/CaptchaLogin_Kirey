# PRJ_Kirey

Frontend minimale per una schermata di login Kirey, pronto per inviare le
credenziali a un backend con verifica reCAPTCHA.

## Avvio

Apri `index.html` nel browser. Non sono richieste dipendenze, build o server di
sviluppo.

Per testare reCAPTCHA e chiamate API e' consigliato servire la cartella da un
host locale invece di aprire la pagina con protocollo `file://`.

Dal terminale interno di Visual Studio Code:

```bash
npm run dev
```

Poi apri:

```text
http://localhost:5500
```

## Struttura

- `index.html`: pagina iniziale con form di login
- `success.html`: pagina mostrata quando il backend conferma il login
- `styles.css`: layout responsive, logo e palette arancione/viola
- `src/app.js`: logica di submit e chiamate API
- `asset/Logo kirey.jpeg`: logo mostrato sopra al form
- `asset/color.jpeg`: riferimento colore usato per il viola secondario
- `asset/sfondo kirey.jpeg`: immagine originale dello sfondo
- `asset/sfondo-login.jpeg`: ritaglio usato nel pannello laterale del login

## Configurazione API

Gli endpoint provvisori sono configurati in `src/app.js`:

```js
const API_BASE_URL = "https://example-be.local/api";
const LOGIN_ENDPOINT = "/auth/login";
```

Per collegare il backend reale sara' sufficiente aggiornare questi valori.

## Flusso Login

Al submit del form il frontend:

1. legge `email` e `password`
2. recupera un eventuale `captchaToken`
3. invia il payload a `POST /auth/login`
4. se il backend risponde `ok: true`, apre `success.html`
5. se il backend risponde `ok: false`, mostra il messaggio di errore nel form

Payload previsto per il login:

```json
{
  "email": "nome@azienda.it",
  "password": "password",
  "captchaToken": null
}
```

Risposta login attesa:

```json
{
  "ok": true
}
```

Il frontend considera valido solo `ok: true`. Se il backend risponde con
`ok: false`, oppure non restituisce `ok: true`, il login viene trattato come non
autorizzato.

## reCAPTCHA

Il progetto usa reCAPTCHA v2 checkbox.

Nel frontend la site key e' configurata in `src/app.js`:

```js
const RECAPTCHA_SITE_KEY = "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI";
```

Questa e' una chiave di test per sviluppo. In produzione dovra' essere sostituita
con la site key reale registrata per il dominio del progetto.

La funzione `getCaptchaToken()` recupera il token generato dal widget e lo invia
nel payload di login come `captchaToken`.

Il backend dovra':

1. ricevere `captchaToken` nel payload di login
2. validarlo server-to-server con Google
3. bloccare il login se reCAPTCHA non e' valido
4. verificare le credenziali solo dopo una verifica CAPTCHA positiva

Per la verifica server-to-server servira' una secret key Google, da salvare solo
lato backend, per esempio in una variabile ambiente come `RECAPTCHA_SECRET_KEY`.

## Note Backend

Per ora gli endpoint sono generici e pensati per essere raffinati in seguito.

- `POST /auth/login`: autenticazione utente con verifica reCAPTCHA
