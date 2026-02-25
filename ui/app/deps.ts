import { ApiClient } from "@cashregister/api-client";
import { mustParseConfiguration } from "@cashregister/settings";

export interface Deps {
  apiClient: ApiClient;
}

function makeDeps(): Deps {
  const settings = mustParseConfiguration();

  return {
    apiClient: new ApiClient(settings.apiBaseUrl),
  };
}

export const deps: Deps = makeDeps();
