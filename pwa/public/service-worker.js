
// Filename - public/worker.js
 
let STATIC_CACHE_NAME = "gfg-pwa";
let DYNAMIC_CACHE_NAME = "dynamic-gfg-pwa";
 
// Add Routes and pages using React Browser Router
let urlsToCache = ["/", "/search", "/aboutus", "/profile"];
 
// Install a service worker
self.addEventListener("install", (event) => {
    // Perform install steps
    console.log('service worker installed')
});
 

// Cache and return requests
self.addEventListener("fetch", (event) => {
    const url = new URL(event.request.url);
  
    // Ignore unsupported protocols
    if (url.protocol !== "http:" && url.protocol !== "https:") {
      console.warn("Ignoring unsupported protocol:", url.protocol);
      return;
    }
  
    // Skip API requests
    if (url.pathname.startsWith("/api/")) {
      console.log("Skipping API request:", url.pathname);
      return;
    }
  
    event.respondWith(
      caches.match(event.request).then((cacheRes) => {
        return (
          cacheRes ||
          fetch(event.request)
            .then((fetchRes) => {
              return caches.open(DYNAMIC_CACHE_NAME).then((cache) => {
                cache.put(event.request, fetchRes.clone());
                // Limit the cache size
                cache.keys().then((keys) => {
                  if (keys.length > 50) {
                    cache.delete(keys[0]);
                  }
                });
                return fetchRes;
              });
            })
            .catch(() => {
              return new Response("You are offline.", {
                status: 503,
                statusText: "Service Unavailable",
              });
            })
        );
      })
    );
  });
  
// // Cache and return requests
// self.addEventListener("fetch", (event) => {
//     event.respondWith(
//         caches.match(event.request).then((cacheRes) => {
//             // If the file is not present in STATIC_CACHE,
//             // it will be searched in DYNAMIC_CACHE
//             return (
//                 cacheRes ||
//                 fetch(event.request).then((fetchRes) => {
//                     return caches
//                         .open(DYNAMIC_CACHE_NAME)
//                         .then((cache) => {
//                             cache.put(
//                                 event.request.url,
//                                 fetchRes.clone()
//                             );
//                             return fetchRes;
//                         });
//                 })
//             );
//         })
//     );
// });
 
// Update a service worker
self.addEventListener("activate", (event) => {
    let cacheWhitelist = ["gfg-pwa"];
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (
                        cacheWhitelist.indexOf(
                            cacheName
                        ) === -1
                    ) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

self.addEventListener('message', (event) => {
    console.log('Message received:', event.data);

    if (event.ports && event.ports[0]) {
      event.ports[0].postMessage({ success: true });
    }
  });
  
