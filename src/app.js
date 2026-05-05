const API_BASE_URL = "https://example-be.local/api";
const LOGIN_ENDPOINT = "/auth/login";
const SUCCESS_PAGE = "./success.html";
const RECAPTCHA_SITE_KEY = "6LePJ9osAAAAAA5jZKoezO5ivFqyswiT3ehlcgi9";

const form = document.querySelector("#login-form");
const submitButton = document.querySelector("#submit-button");
const message = document.querySelector("#form-message");
const recaptchaContainer = document.querySelector("#recaptcha-container");
let recaptchaWidgetId = null;

function setMessage(text, type = "") {
  message.textContent = text;
  message.className = `form-message ${type}`.trim();
}

async function postJson(endpoint, payload) {
  const headers = {
    "Content-Type": "application/json",
  };

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    method: "POST",
    headers,
    body: JSON.stringify(payload),
  });

  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    const errorMessage = data.message || "Richiesta non riuscita.";
    throw new Error(errorMessage);
  }

  return data;
}

function renderRecaptcha() {
  if (!window.grecaptcha || !recaptchaContainer || recaptchaWidgetId !== null) {
    return;
  }

  recaptchaWidgetId = window.grecaptcha.render(recaptchaContainer, {
    sitekey: RECAPTCHA_SITE_KEY,
  });
}

function loadRecaptchaScript() {
  window.onRecaptchaReady = renderRecaptcha;

  const script = document.createElement("script");
  script.src =
    "https://www.google.com/recaptcha/api.js?onload=onRecaptchaReady&render=explicit";
  script.async = true;
  script.defer = true;
  document.head.appendChild(script);
}

async function getCaptchaToken() {
  if (!window.grecaptcha || recaptchaWidgetId === null) {
    throw new Error("reCAPTCHA non ancora pronto.");
  }

  const token = window.grecaptcha.getResponse(recaptchaWidgetId);

  if (!token) {
    throw new Error("Completa la verifica reCAPTCHA.");
  }

  return token;
}

function resetCaptcha() {
  if (window.grecaptcha && recaptchaWidgetId !== null) {
    window.grecaptcha.reset(recaptchaWidgetId);
  }
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  const formData = new FormData(form);
  submitButton.disabled = true;
  setMessage("Invio credenziali in corso...");

  try {
    const captchaToken = await getCaptchaToken();
    const credentials = {
      email: formData.get("email"),
      password: formData.get("password"),
      captchaToken,
    };

    const loginResult = await postJson(LOGIN_ENDPOINT, credentials);

    if (loginResult.ok !== true) {
      throw new Error(loginResult.message || "Login non autorizzato.");
    }

    window.location.href = SUCCESS_PAGE;
  } catch (error) {
    setMessage(error.message, "error");
    resetCaptcha();
  } finally {
    submitButton.disabled = false;
  }
});

loadRecaptchaScript();
