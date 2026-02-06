import { z } from "zod";
import { ApiClient } from "./api/api-client";

const settingsSchema = z.object({
  apiBaseUrl: z.coerce.string().default(""),
});

type Settings = z.infer<typeof settingsSchema>;

export interface Deps {
  apiClient: ApiClient;
}

function mustParseConfiguration(): Settings {
  return settingsSchema.parse({
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL,
  });
}

function makeDeps(): Deps {
  const settings = mustParseConfiguration();

  return {
    apiClient: new ApiClient(settings.apiBaseUrl),
  };
}

export const deps: Deps = makeDeps();
