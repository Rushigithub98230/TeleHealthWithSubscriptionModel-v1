// Minimal test client for Telehealth chat and video
// Assumes SignalR endpoints at /chatHub and /videoCallHub

let chatConnection = null;
let videoConnection = null;
let isVideoConnected = false;
let authToken = localStorage.getItem('authToken') || 'your-test-token-here';

const statusEl = document.getElementById('status');
const chatInput = document.getElementById('chat-input');
const sendChatBtn = document.getElementById('send-chat-btn');
const chatMessages = document.getElementById('chat-messages');
const startVideoBtn = document.getElementById('start-video-btn');
const endVideoBtn = document.getElementById('end-video-btn');
const videoContainer = document.getElementById('video-container');

function setStatus(text, connected) {
    statusEl.textContent = text;
    if (connected) {
        statusEl.classList.add('connected');
    } else {
        statusEl.classList.remove('connected');
    }
}

// --- Chat ---
async function connectChat() {
    chatConnection = new signalR.HubConnectionBuilder()
        .withUrl('/chatHub', {
            accessTokenFactory: () => authToken
        })
        .withAutomaticReconnect()
        .build();

    chatConnection.on('MessageReceived', (message) => {
        addChatMessage(message.senderName || 'User', message.content, 'received');
    });

    chatConnection.onreconnecting(() => setStatus('Chat reconnecting...', false));
    chatConnection.onreconnected(() => setStatus('Chat connected', true));
    chatConnection.onclose(() => setStatus('Chat disconnected', false));

    try {
        await chatConnection.start();
        setStatus('Chat connected', true);
        chatInput.disabled = false;
        sendChatBtn.disabled = false;
    } catch (err) {
        setStatus('Chat connection failed', false);
    }
}

function addChatMessage(sender, content, type) {
    const msg = document.createElement('div');
    msg.className = 'message ' + type;
    msg.innerHTML = `<strong>${sender}:</strong> ${content}`;
    chatMessages.appendChild(msg);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

sendChatBtn.addEventListener('click', sendChatMessage);
chatInput.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') sendChatMessage();
});

function sendChatMessage() {
    const text = chatInput.value.trim();
    if (!text || !chatConnection) return;
    chatConnection.invoke('SendMessage', { content: text })
        .then(() => {
            addChatMessage('Me', text, 'sent');
            chatInput.value = '';
        })
        .catch(() => setStatus('Failed to send chat', false));
}

// --- Video ---
async function connectVideo() {
    videoConnection = new signalR.HubConnectionBuilder()
        .withUrl('/videoCallHub', {
            accessTokenFactory: () => authToken
        })
        .withAutomaticReconnect()
        .build();

    videoConnection.on('CallInitiated', (call) => {
        isVideoConnected = true;
        showVideoConnected();
    });
    videoConnection.on('VideoCallEnded', () => {
        isVideoConnected = false;
        showVideoDisconnected();
    });
    videoConnection.onreconnecting(() => setStatus('Video reconnecting...', false));
    videoConnection.onreconnected(() => setStatus('Video connected', true));
    videoConnection.onclose(() => setStatus('Video disconnected', false));

    try {
        await videoConnection.start();
        setStatus('Video connected', true);
        startVideoBtn.disabled = false;
    } catch (err) {
        setStatus('Video connection failed', false);
    }
}

startVideoBtn.addEventListener('click', async function() {
    if (!videoConnection) return;
    try {
        await videoConnection.invoke('InitiateCall');
        showVideoConnected();
        startVideoBtn.disabled = true;
        endVideoBtn.disabled = false;
    } catch {
        setStatus('Failed to start video', false);
    }
});

endVideoBtn.addEventListener('click', async function() {
    if (!videoConnection) return;
    try {
        await videoConnection.invoke('EndCall');
        showVideoDisconnected();
        startVideoBtn.disabled = false;
        endVideoBtn.disabled = true;
    } catch {
        setStatus('Failed to end video', false);
    }
});

function showVideoConnected() {
    videoContainer.innerHTML = '<div class="video-placeholder" style="color: #28a745;">Video Connected</div>';
}
function showVideoDisconnected() {
    videoContainer.innerHTML = '<div class="video-placeholder">Video not connected</div>';
}

// --- Init ---
window.addEventListener('DOMContentLoaded', () => {
    connectChat();
    connectVideo();
});

// Replace with your Stripe test publishable key
const stripe = Stripe('pk_test_51RbfqBCI7YurXiFNHk4WcajFzdxGJCxD32qJbtcQCTSaVU5qbpHZZR2D4iujZeh3bcGZEEtCetI94SadTICFXjFG005IoPIAYC'); // TODO: Replace with your key
const elements = stripe.elements();
const card = elements.create('card');
card.mount('#card-element');

function log(msg) {
  const resultDiv = document.getElementById('result');
  resultDiv.textContent += '\n' + msg;
}

document.getElementById('payment-form').addEventListener('submit', async (e) => {
  e.preventDefault();
  log('Creating payment method...');
  const {paymentMethod, error} = await stripe.createPaymentMethod({
    type: 'card',
    card: card,
  });
  if (error) {
    log('Error: ' + error.message);
  } else {
    console.log('Payment Method ID: ' + paymentMethod.id);
    // Send this ID to your backend for further testing
    sendToBackend(paymentMethod.id);
  }
});

function sendToBackend(paymentMethodId) {
  console.log('Sending Payment Method ID to backend...');
  fetch('https://localhost:58676/api/stripe/test-payment', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ paymentMethodId })
  })
  .then(res => res.json())
  .then(data => {
    log('Backend response: ' + JSON.stringify(data));
  })
  .catch(err => {
    log('Backend error: ' + err);
  });
} 