import { ApiClient } from "./api/api-client";

export interface Deps {
  apiClient: ApiClient;
}

function mustParseConfiguration() {
  return {
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "",
  };
}

function makeDeps(): Deps {
  const settings = mustParseConfiguration();

  return {
    apiClient: new ApiClient(settings.apiBaseUrl),
  };
}

export const deps: Deps = makeDeps();
