const http = require("http");
const fs = require("fs");
const path = require("path");

const port = 5500;
const root = __dirname;
const types = {
  ".css": "text/css; charset=utf-8",
  ".html": "text/html; charset=utf-8",
  ".jpeg": "image/jpeg",
  ".jpg": "image/jpeg",
  ".js": "text/javascript; charset=utf-8",
  ".png": "image/png",
};

function sendFile(response, filePath) {
  fs.readFile(filePath, (error, content) => {
    if (error) {
      response.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" });
      response.end("File non trovato");
      return;
    }

    const extension = path.extname(filePath);
    response.writeHead(200, {
      "Content-Type": types[extension] || "application/octet-stream",
    });
    response.end(content);
  });
}

const server = http.createServer((request, response) => {
  const requestUrl = new URL(request.url, `http://localhost:${port}`);
  const pathname = requestUrl.pathname === "/" ? "/index.html" : requestUrl.pathname;
  const filePath = path.join(root, decodeURIComponent(pathname));

  if (!filePath.startsWith(root)) {
    response.writeHead(403, { "Content-Type": "text/plain; charset=utf-8" });
    response.end("Accesso negato");
    return;
  }

  sendFile(response, filePath);
});

server.listen(port, "127.0.0.1", () => {
  console.log(`Server avviato su http://localhost:${port}`);
});
