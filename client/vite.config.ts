import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],

  server: {
    port: 5173,
    strictPort: true,

    proxy: {
      "/api": {
        target: "http://localhost:5244",
        changeOrigin: true,
        secure: false,
      },
      // SignalR hubs (negotiate is HTTP POST; upgrade is WebSocket).
      "/hubs": {
        target: "http://localhost:5244",
        changeOrigin: true,
        secure: false,
        ws: true,
      },
      "/uploads": {
        target: "http://localhost:5244",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});