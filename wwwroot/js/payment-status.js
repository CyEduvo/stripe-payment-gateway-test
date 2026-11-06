// Uses tab-local storage only to limit refreshes, never to decide payment status.
"use strict";

// Clears the cart only when the server-rendered result says Paid.
function updatePaymentPage() {
    const result = document.getElementById("payment-result");
    if (!result) return;
    const message = document.getElementById("payment-message");
    const key = "payment-reloads:" + result.dataset.sessionId;
    try {
        if (result.dataset.status === "Paid") {
            localStorage.removeItem("cart");
            sessionStorage.removeItem(key);
            return;
        }
        const saved = Number(sessionStorage.getItem(key) || "0");
        const count = Number.isInteger(saved) && saved >= 0 ? saved : 10;
        if (count >= 10) {
            message.textContent = "Automatic checks have stopped. Check Orders or refresh this page later.";
            return;
        }
        sessionStorage.setItem(key, String(count + 1));
        message.textContent = "Checking again in 3 seconds (" + (count + 1) + " of 10).";
        window.setTimeout(reloadPaymentPage, 3000);
    } catch {
        message.textContent = "Browser storage is unavailable. Check Orders or refresh manually.";
    }
}

// Asks the server again instead of inferring that a timer means payment succeeded.
function reloadPaymentPage() {
    window.location.reload();
}

updatePaymentPage();
