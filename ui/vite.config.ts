import tailwindcss from "@tailwindcss/vite";
import { reactRouter } from "@react-router/dev/vite";
import { defineConfig } from "vitest/config";

declare const process: { env: Record<string, string | undefined> };

export default defineConfig({
  plugins: [tailwindcss(), !process.env.VITEST && reactRouter()],
  test: {
    environment: "jsdom",
    include: ["app/**/*.test.{ts,tsx}"],
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5122',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    }
  }
});
