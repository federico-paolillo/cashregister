import { ApiClient } from "@cashregister/api-client";
import { mustParseConfiguration } from "@cashregister/settings";

export interface Deps {
  apiClient: ApiClient;
  lowQuantityWarningThreshold: number;
}

function makeDeps(): Deps {
  const settings = mustParseConfiguration();

  return {
    apiClient: new ApiClient(settings.apiBaseUrl),
    lowQuantityWarningThreshold: settings.lowQuantityWarningThreshold,
  };
}

export const deps: Deps = makeDeps();
