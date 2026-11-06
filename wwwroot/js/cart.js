// Stores only product IDs and quantities; displayed prices never authorize payment.
"use strict";
let cart = [];
const message = document.getElementById("cart-message");
const checkoutButton = document.getElementById("checkout-button");

// Reads saved items and discards extra fields such as a browser-injected price.
function loadCart() {
    try {
        const saved = JSON.parse(localStorage.getItem("cart") || "[]");
        if (!Array.isArray(saved)) throw new Error("Invalid cart");
        cart = [];
        for (const item of saved) {
            if (!item || typeof item.productId !== "string" || !Number.isInteger(item.quantity)) {
                throw new Error("Invalid item");
            }
            cart.push({ productId: item.productId, quantity: item.quantity });
        }
    } catch {
        cart = [];
        message.textContent = "Your saved cart could not be read. Please add your items again.";
    }
}

// Saves only the two fields the server accepts, with a friendly storage error.
function saveCart() {
    try {
        const saved = [];
        for (const item of cart) saved.push({ productId: item.productId, quantity: item.quantity });
        localStorage.setItem("cart", JSON.stringify(saved));
    } catch {
        message.textContent = "Browser storage is unavailable. Your cart will not survive a refresh.";
    }
}

// Adds one item while keeping the classroom limit visible to the shopper.
function addToCart(productId) {
    message.textContent = "";
    let existing = null;
    for (const item of cart) if (item.productId === productId) existing = item;
    if (existing) {
        if (existing.quantity >= 10) {
            message.textContent = "You can add up to 10 of each item.";
            return;
        }
        existing.quantity += 1;
    } else {
        cart.push({ productId, quantity: 1 });
    }
    saveCart();
    renderCart();
}

// Removes a whole product line from the browser cart.
function removeFromCart(productId) {
    const remaining = [];
    for (const item of cart) if (item.productId !== productId) remaining.push(item);
    cart = remaining;
    saveCart();
    renderCart();
}

// Clears the browser cart without changing any server orders.
function clearCart() {
    cart = [];
    message.textContent = "";
    saveCart();
    renderCart();
}

// Looks up display information without constructing selectors from saved input.
function findCard(productId) {
    for (const card of document.querySelectorAll(".product-card")) {
        if (card.dataset.id === productId) return card;
    }
    return null;
}

// Builds safe text nodes so edited localStorage cannot inject page HTML.
function renderCart() {
    const container = document.getElementById("cart-items");
    container.replaceChildren();
    let total = 0;
    if (cart.length === 0) {
        const empty = document.createElement("p");
        empty.className = "cart-empty";
        empty.textContent = "Your next adventure is waiting. Add something you like.";
        container.append(empty);
    }
    for (const item of cart) {
        const card = findCard(item.productId);
        const row = document.createElement("div");
        row.className = "cart-row";
        const label = document.createElement("span");
        label.textContent = (card ? card.dataset.name : "Unknown product") + " × " + item.quantity;
        const remove = document.createElement("button");
        remove.type = "button";
        remove.className = "text-button";
        remove.textContent = "Remove";
        remove.dataset.removeId = item.productId;
        row.append(label, remove);
        container.append(row);
        if (card) total += Number(card.dataset.price) * item.quantity;
    }
    document.getElementById("cart-total").textContent = "RM" + (total / 100).toFixed(2);
    checkoutButton.disabled = cart.length === 0;
}

// Sends IDs and quantities; the server rebuilds all prices before contacting Stripe.
async function checkout() {
    checkoutButton.disabled = true;
    message.textContent = "Opening test checkout...";
    try {
        const response = await fetch("/checkout/create", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-CSRF-TOKEN": document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ items: cart })
        });
        if (!(response.headers.get("content-type") || "").includes("application/json")) {
            throw new Error("Checkout could not start. Refresh the page and try again.");
        }
        const result = await response.json();
        if (!response.ok) throw new Error(result.error || "Checkout could not start.");
        window.location.href = result.url;
    } catch (error) {
        message.textContent = error.message || "Could not connect. Please try again.";
        checkoutButton.disabled = cart.length === 0;
    }
}

// Routes button clicks through one readable event handler.
function handleClick(event) {
    const button = event.target.closest("button");
    if (!button) return;
    if (button.dataset.productId) addToCart(button.dataset.productId);
    if (button.dataset.removeId) removeFromCart(button.dataset.removeId);
    if (button.id === "clear-button") clearCart();
    if (button.id === "checkout-button") checkout();
}

document.addEventListener("click", handleClick);
loadCart();
renderCart();
