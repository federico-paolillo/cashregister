import { z } from "zod";

const settingsSchema = z.object({
  apiBaseUrl: z.coerce.string().default(""),
});

export type Settings = z.infer<typeof settingsSchema>;

export function mustParseConfiguration(): Settings {
  return settingsSchema.parse({
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL,
  });
}
