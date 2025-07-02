#!/usr/bin/env python3
import http.server
import socketserver
import os
import sys

# Change to the build directory
os.chdir('build')

PORT = 3000

class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory='.', **kwargs)

print(f"CARBOX PWA serving on port {PORT}")
print(f"Access your app at: http://localhost:{PORT}")

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nServer stopped.")
        sys.exit(0)