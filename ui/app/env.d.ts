/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// Invoker Commands API — supported in Chrome 135+, Edge 135+, Safari TP
// https://developer.mozilla.org/en-US/docs/Web/API/Invoker_Commands_API

declare namespace React {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  interface ButtonHTMLAttributes<T> {
    command?: string;
    commandfor?: string;
  }
}
