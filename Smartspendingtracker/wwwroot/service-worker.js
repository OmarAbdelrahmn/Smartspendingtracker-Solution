// Install
self.addEventListener('install', event => {
    self.skipWaiting();
});

// Activate
self.addEventListener('activate', event => {
    event.waitUntil(self.clients.claim());
});

// Fetch - no caching at all
self.addEventListener('fetch', event => {
    event.respondWith(fetch(event.request));
});