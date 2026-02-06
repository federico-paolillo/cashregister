import tailwindcss from "@tailwindcss/vite";
import { reactRouter } from "@react-router/dev/vite";
import { defineConfig } from "vitest/config";

export default defineConfig({
  plugins: [tailwindcss(), reactRouter()],
  test: {
    environment: "jsdom",
    include: ["app/**/*.test.{ts,tsx}"],
  },
});
